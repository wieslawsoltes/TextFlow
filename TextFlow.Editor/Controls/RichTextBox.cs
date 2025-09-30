using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using TextFlow.Editor.Documents;

namespace TextFlow.Editor.Controls;

public class RichTextBox : TemplatedControl
{

    public static readonly StyledProperty<RichTextDocument> DocumentProperty =
        AvaloniaProperty.Register<RichTextBox, RichTextDocument>(
            nameof(Document),
            defaultValue: new RichTextDocument());

    public static readonly StyledProperty<IBrush?> SelectionBrushProperty =
        AvaloniaProperty.Register<RichTextBox, IBrush?>(
            nameof(SelectionBrush),
            defaultValue: new SolidColorBrush(Color.FromArgb(0xFF, 0x42, 0x82, 0xF1)));

    public static readonly StyledProperty<IBrush?> CaretBrushProperty =
        AvaloniaProperty.Register<RichTextBox, IBrush?>(nameof(CaretBrush));

    public static readonly StyledProperty<double> CaretWidthProperty =
        AvaloniaProperty.Register<RichTextBox, double>(nameof(CaretWidth), 1.0);

    public static readonly StyledProperty<bool> IsReadOnlyProperty =
        AvaloniaProperty.Register<RichTextBox, bool>(nameof(IsReadOnly));

    public static readonly DirectProperty<RichTextBox, int> CaretOffsetProperty =
        AvaloniaProperty.RegisterDirect<RichTextBox, int>(
            nameof(CaretOffset),
            x => x._caretOffset,
            (x, v) => x.SetCaretOffset(v));

    public static readonly DirectProperty<RichTextBox, int> SelectionStartProperty =
        AvaloniaProperty.RegisterDirect<RichTextBox, int>(
            nameof(SelectionStart),
            x => x._selectionStart,
            (x, v) => x.SetSelectionStart(v));

    public static readonly DirectProperty<RichTextBox, int> SelectionEndProperty =
        AvaloniaProperty.RegisterDirect<RichTextBox, int>(
            nameof(SelectionEnd),
            x => x._selectionEnd,
            (x, v) => x.SetSelectionEnd(v));

    public static readonly DirectProperty<RichTextBox, string> TextProperty =
        AvaloniaProperty.RegisterDirect<RichTextBox, string>(
            nameof(Text),
            x => x.Document.Text,
            (x, v) => x.SetText(v ?? string.Empty));

    public static readonly StyledProperty<TextAlignment> TextAlignmentProperty =
        AvaloniaProperty.Register<RichTextBox, TextAlignment>(nameof(TextAlignment), TextAlignment.Left);

    private RichTextPresenter? _presenter;
    private readonly DispatcherTimer _caretTimer;
    private bool _caretVisible;
    private int _caretOffset;
    private int _selectionStart;
    private int _selectionEnd;
    private bool _isSelecting;
    private readonly Stack<RichTextDocument> _undoStack = new();
    private readonly Stack<RichTextDocument> _redoStack = new();
    private bool _suppressUndo;
    private RichTextStyle _currentStyle = RichTextStyle.Default;
    private bool _suspendSelectionNotifications;

    public event EventHandler? SelectionChanged;

    public event EventHandler? TextChanged;

    static RichTextBox()
    {
        FocusableProperty.OverrideDefaultValue<RichTextBox>(true);
        ClipToBoundsProperty.OverrideDefaultValue<RichTextBox>(true);
    }

    public RichTextBox()
    {
    _caretTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(530) };
    _caretTimer.Tick += (_, _) => ToggleCaretVisibility();
        SetCurrentValue(DocumentProperty, new RichTextDocument());
    }

    public RichTextDocument Document
    {
        get => GetValue(DocumentProperty);
        set => SetValue(DocumentProperty, value);
    }

    public IBrush? SelectionBrush
    {
        get => GetValue(SelectionBrushProperty);
        set => SetValue(SelectionBrushProperty, value);
    }

    public IBrush? CaretBrush
    {
        get => GetValue(CaretBrushProperty);
        set => SetValue(CaretBrushProperty, value);
    }

    public double CaretWidth
    {
        get => GetValue(CaretWidthProperty);
        set => SetValue(CaretWidthProperty, value);
    }

    public bool IsReadOnly
    {
        get => GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }

    public int CaretOffset => _caretOffset;

    public int SelectionStart => _selectionStart;

    public int SelectionEnd => _selectionEnd;

    public int SelectionLength => Math.Abs(_selectionEnd - _selectionStart);

    public string SelectedText => SelectionLength == 0 ? string.Empty : Document.GetTextRange(Math.Min(_selectionStart, _selectionEnd), SelectionLength);

    public string Text
    {
        get => Document.Text;
        set => SetText(value ?? string.Empty);
    }

    public TextAlignment TextAlignment
    {
        get => GetValue(TextAlignmentProperty);
        set => SetValue(TextAlignmentProperty, value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _presenter = e.NameScope.Find<RichTextPresenter>("PART_Presenter");

        if (_presenter is not null)
        {
            _presenter.Document = Document;
            _presenter.SelectionBrush = SelectionBrush;
            _presenter.CaretBrush = CaretBrush;
            _presenter.CaretWidth = CaretWidth;
            _presenter.SelectionStart = _selectionStart;
            _presenter.SelectionEnd = _selectionEnd;
            _presenter.CaretOffset = _caretOffset;
            _presenter.CaretVisible = _caretVisible;
            _presenter.TextAlignment = TextAlignment;
        }
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);

        if (_presenter is null)
        {
            return;
        }

        Focus();
        ResetCaretBlink();

        var point = e.GetPosition(_presenter);
        var offset = _presenter.GetOffsetFromPoint(point);

        if (e.KeyModifiers.HasFlag(KeyModifiers.Shift))
        {
            SetSelectionEnd(offset);
        }
        else
        {
            SetSelection(offset, offset);
        }

        SetCaretOffset(offset);
    _isSelecting = true;
    e.Pointer.Capture(_presenter);
        e.Handled = true;
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);

        if (!_isSelecting || _presenter is null)
        {
            return;
        }

        var position = e.GetPosition(_presenter);
        var offset = _presenter.GetOffsetFromPoint(position);
        SetSelectionEnd(offset);
        SetCaretOffset(offset);
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);

        if (_isSelecting)
        {
            e.Pointer.Capture(null);
        }

        _isSelecting = false;
    }

    protected override void OnLostFocus(RoutedEventArgs e)
    {
        base.OnLostFocus(e);
        _caretTimer.Stop();
        _caretVisible = false;
        UpdatePresenterCaret();
    }

    protected override void OnGotFocus(GotFocusEventArgs e)
    {
        base.OnGotFocus(e);
        ResetCaretBlink();
    }

    protected override void OnTextInput(TextInputEventArgs e)
    {
        base.OnTextInput(e);

        if (IsReadOnly || string.IsNullOrEmpty(e.Text))
        {
            return;
        }

        InsertText(e.Text);
        e.Handled = true;
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        if (HandleNavigationKeys(e))
        {
            e.Handled = true;
            return;
        }

        if (HandleEditingKeys(e))
        {
            e.Handled = true;
            return;
        }

        if (HandleFormattingKeys(e))
        {
            e.Handled = true;
            return;
        }
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == DocumentProperty)
        {
            if (change.GetOldValue<RichTextDocument>() is { } oldDoc)
            {
                oldDoc.Changed -= OnDocumentChanged;
            }

            if (change.GetNewValue<RichTextDocument>() is { } newDoc)
            {
                newDoc.Changed += OnDocumentChanged;
                if (_presenter is not null)
                {
                    _presenter.Document = newDoc;
                }
            }

            ClampSelection();
            UpdatePresenterSelection();
            UpdatePresenterCaret();
            RaiseTextChanged();
            return;
        }

        if (_presenter is null)
        {
            return;
        }

        if (change.Property == SelectionBrushProperty)
        {
            _presenter.SelectionBrush = change.GetNewValue<IBrush?>();
        }
        else if (change.Property == CaretBrushProperty)
        {
            _presenter.CaretBrush = change.GetNewValue<IBrush?>();
        }
        else if (change.Property == CaretWidthProperty)
        {
            _presenter.CaretWidth = change.GetNewValue<double>();
        }
        else if (change.Property == TextAlignmentProperty)
        {
            _presenter.TextAlignment = change.GetNewValue<TextAlignment>();
        }
    }

    public void SelectAll()
    {
        SetSelection(0, Document.Length);
        SetCaretOffset(Document.Length);
    }

    public void Select(int start, int length)
    {
        start = Math.Clamp(start, 0, Document.Length);
        length = Math.Clamp(length, 0, Document.Length - start);
        SetSelection(start, start + length);
        SetCaretOffset(start + length);
    }

    public void AppendText(string text)
    {
        InsertText(text, Document.Length, replaceSelection: false);
    }

    public async Task CopyAsync()
    {
        if (SelectionLength == 0)
        {
            return;
        }

        var text = SelectedText;
        if (string.IsNullOrEmpty(text))
        {
            return;
        }

        if (TopLevel.GetTopLevel(this) is { Clipboard: { } clipboard })
        {
            await clipboard.SetTextAsync(text);
        }
    }

    public async Task CutAsync()
    {
        if (IsReadOnly)
        {
            return;
        }

        if (SelectionLength == 0)
        {
            return;
        }

        await CopyAsync();
        DeleteSelection();
    }

    public async Task PasteAsync()
    {
        if (IsReadOnly)
        {
            return;
        }

        if (TopLevel.GetTopLevel(this) is { Clipboard: { } clipboard })
        {
            var text = await clipboard.GetTextAsync();
            if (!string.IsNullOrEmpty(text))
            {
                InsertText(text);
            }
        }
    }

    public void Undo()
    {
        if (_undoStack.Count == 0)
        {
            return;
        }

        _suppressUndo = true;
        var current = Document.Clone();
        _redoStack.Push(current);

        var snapshot = _undoStack.Pop();
        Document.RestoreFrom(snapshot);
        ClampSelection();
        ResetCaretBlink();
        RaiseTextChanged();
        _suppressUndo = false;
    }

    public void Redo()
    {
        if (_redoStack.Count == 0)
        {
            return;
        }

        _suppressUndo = true;
        _undoStack.Push(Document.Clone());
        var snapshot = _redoStack.Pop();
        Document.RestoreFrom(snapshot);
        ClampSelection();
        ResetCaretBlink();
        RaiseTextChanged();
        _suppressUndo = false;
    }

    public void SetTextAlignment(TextAlignment alignment)
    {
        SetValue(TextAlignmentProperty, alignment);

        if (IsReadOnly)
        {
            return;
        }

        var lines = GetSelectedLineRanges();
        if (lines.Count == 0)
        {
            RequestPresenterRefresh();
            return;
        }

        PushUndoSnapshot();

        if (alignment == TextAlignment.Left)
        {
            Document.ClearParagraphAlignment(lines);
        }
        else
        {
            Document.SetParagraphAlignment(lines, alignment);
        }

        _redoStack.Clear();
        RequestPresenterRefresh();
    }

    public void InsertTable(int rows, int columns)
    {
        if (IsReadOnly)
        {
            return;
        }

        rows = Math.Max(1, rows);
        columns = Math.Max(1, columns);

        PushUndoSnapshot();

        if (SelectionLength > 0)
        {
            var start = Math.Min(_selectionStart, _selectionEnd);
            Document.DeleteRange(start, SelectionLength);
            SetSelection(start, start);
            SetCaretOffset(start);
        }

        var insertionOffset = _caretOffset;
        var insertedLength = Document.InsertTable(insertionOffset, rows, columns);
        var newOffset = insertionOffset + insertedLength;
        SetSelection(newOffset, newOffset);
        SetCaretOffset(newOffset);
        ResetCaretBlink();
        _redoStack.Clear();
        UpdateCurrentStyleFromCaret();
        RequestPresenterRefresh();
    }

    public void IncreaseIndentation()
    {
        var lines = GetSelectedLineRanges();
        if (lines.Count == 0)
        {
            return;
        }

        PushUndoSnapshot();
        var indent = "    ";
        var originalLastEnd = lines[^1].Start + lines[^1].Length;

        for (var i = lines.Count - 1; i >= 0; i--)
        {
            var line = lines[i];
            var style = GetStyleForInsertion(line.Start);
            Document.InsertText(line.Start, indent, style);
        }

        var totalDelta = indent.Length * lines.Count;
        var newStart = lines[0].Start;
        var newEnd = originalLastEnd + totalDelta;
        SetSelection(newStart, Math.Max(newStart, newEnd));
        SetCaretOffset(Math.Max(newStart, newEnd));
        _redoStack.Clear();
            RequestPresenterRefresh();
    }

    public void DecreaseIndentation()
    {
        var lines = GetSelectedLineRanges();
        if (lines.Count == 0)
        {
            return;
        }

        PushUndoSnapshot();
        var text = Document.Text;
        var totalRemoved = 0;
        var originalLastEnd = lines[^1].Start + lines[^1].Length;

        for (var i = lines.Count - 1; i >= 0; i--)
        {
            var line = lines[i];
            var removal = CalculateIndentRemoval(text, line);
            if (removal <= 0)
            {
                continue;
            }

            Document.DeleteRange(line.Start, removal);
            totalRemoved += removal;
        }

        var newStart = lines[0].Start;
        var newEnd = Math.Max(newStart, originalLastEnd - totalRemoved);
        SetSelection(newStart, newEnd);
        SetCaretOffset(newEnd);
        _redoStack.Clear();
    RequestPresenterRefresh();
    }

    public void ToggleBulletList()
    {
        var lines = GetSelectedLineRanges();
        if (lines.Count == 0)
        {
            return;
        }

        var text = Document.Text;
        var allBulleted = true;
        foreach (var line in lines)
        {
            if (!LineStartsWithBullet(text, line))
            {
                allBulleted = false;
                break;
            }
        }

        PushUndoSnapshot();
        var totalDelta = 0;
        var originalLastEnd = lines[^1].Start + lines[^1].Length;

        if (allBulleted)
        {
            for (var i = lines.Count - 1; i >= 0; i--)
            {
                var line = lines[i];
                var removed = RemoveBulletPrefix(text, line);
                if (removed > 0)
                {
                    Document.DeleteRange(line.Start, removed);
                    totalDelta -= removed;
                }
            }
        }
        else
        {
            for (var i = lines.Count - 1; i >= 0; i--)
            {
                var line = lines[i];
                var style = GetStyleForInsertion(line.Start);
                Document.InsertText(line.Start, "• ", style);
                totalDelta += 2;
            }
        }

        var newStart = lines[0].Start;
        var newEnd = originalLastEnd + totalDelta;
        newEnd = Math.Max(newStart, newEnd);
        SetSelection(newStart, newEnd);
        SetCaretOffset(newEnd);
        _redoStack.Clear();
    }

    public void ToggleNumberedList()
    {
        var lines = GetSelectedLineRanges();
        if (lines.Count == 0)
        {
            return;
        }

        var text = Document.Text;
        var allNumbered = true;
        foreach (var line in lines)
        {
            if (!TryGetNumberPrefixLength(text, line, out _))
            {
                allNumbered = false;
                break;
            }
        }

        PushUndoSnapshot();
        var totalDelta = 0;
        var originalLastEnd = lines[^1].Start + lines[^1].Length;

        if (allNumbered)
        {
            for (var i = lines.Count - 1; i >= 0; i--)
            {
                var line = lines[i];
                if (TryGetNumberPrefixLength(text, line, out var length))
                {
                    Document.DeleteRange(line.Start, length);
                    totalDelta -= length;
                }
            }
        }
        else
        {
            for (var i = lines.Count - 1; i >= 0; i--)
            {
                var line = lines[i];

                if (LineStartsWithBullet(text, line))
                {
                    var removed = RemoveBulletPrefix(text, line);
                    if (removed > 0)
                    {
                        Document.DeleteRange(line.Start, removed);
                        totalDelta -= removed;
                    }
                }
                else if (TryGetNumberPrefixLength(text, line, out var existing))
                {
                    Document.DeleteRange(line.Start, existing);
                    totalDelta -= existing;
                }

                var number = i + 1;
                var prefix = $"{number}. ";
                var style = GetStyleForInsertion(line.Start);
                Document.InsertText(line.Start, prefix, style);
                totalDelta += prefix.Length;
            }
        }

        var newStart = lines[0].Start;
        var newEnd = originalLastEnd + totalDelta;
        newEnd = Math.Max(newStart, newEnd);
        SetSelection(newStart, newEnd);
        SetCaretOffset(newEnd);
        _redoStack.Clear();
    }

    public bool SelectionHasBulletList()
    {
        var lines = GetSelectedLineRanges();
        if (lines.Count == 0)
        {
            return false;
        }

        var text = Document.Text;
        foreach (var line in lines)
        {
            if (!LineStartsWithBullet(text, line))
            {
                return false;
            }
        }

        return true;
    }

    public bool SelectionHasNumberedList()
    {
        var lines = GetSelectedLineRanges();
        if (lines.Count == 0)
        {
            return false;
        }

        var text = Document.Text;
        foreach (var line in lines)
        {
            if (!TryGetNumberPrefixLength(text, line, out _))
            {
                return false;
            }
        }

        return true;
    }

    private void OnDocumentChanged(object? sender, EventArgs e)
    {
        if (_presenter is not null)
        {
            _presenter.Document = Document;
        }

        ClampSelection();
        UpdatePresenterSelection();
        if (!_suppressUndo)
        {
            RaiseTextChanged();
        }
    }

    private void SetCaretOffset(int value)
    {
        value = Math.Clamp(value, 0, Document.Length);
        if (SetAndRaise(CaretOffsetProperty, ref _caretOffset, value))
        {
            ResetCaretBlink();
            UpdatePresenterCaret();
            EnsureCaretVisible();
            UpdateCurrentStyleFromCaret();
        }
    }

    private void SetSelectionStart(int value)
    {
        value = Math.Clamp(value, 0, Document.Length);
        if (SetAndRaise(SelectionStartProperty, ref _selectionStart, value))
        {
            if (!_suspendSelectionNotifications)
            {
                SelectionChangedInternal();
            }
        }
    }

    private void SetSelectionEnd(int value)
    {
        value = Math.Clamp(value, 0, Document.Length);
        if (SetAndRaise(SelectionEndProperty, ref _selectionEnd, value))
        {
            if (!_suspendSelectionNotifications)
            {
                SelectionChangedInternal();
            }
        }
    }

    private void SetSelection(int start, int end)
    {
        if (start == _selectionStart && end == _selectionEnd)
        {
            return;
        }

        _suspendSelectionNotifications = true;
        SetSelectionStart(start);
        SetSelectionEnd(end);
        _suspendSelectionNotifications = false;
        SelectionChangedInternal();
    }

    private void InsertText(string text)
    {
        InsertText(text, _caretOffset, replaceSelection: true);
    }

    private void InsertText(string text, int offset, bool replaceSelection)
    {
        if (string.IsNullOrEmpty(text))
        {
            return;
        }

        if (IsReadOnly)
        {
            return;
        }

        PushUndoSnapshot();

        if (replaceSelection && SelectionLength > 0)
        {
            offset = Math.Min(_selectionStart, _selectionEnd);
            Document.DeleteRange(offset, SelectionLength);
            SetSelection(offset, offset);
            SetCaretOffset(offset);
        }

        Document.InsertText(offset, text, _currentStyle);
        var newOffset = offset + text.Replace("\r\n", "\n").Replace('\r', '\n').Length;
        SetCaretOffset(newOffset);
        SetSelection(newOffset, newOffset);
        _redoStack.Clear();
    }

    private void DeleteSelection()
    {
        if (SelectionLength == 0)
        {
            return;
        }

        if (IsReadOnly)
        {
            return;
        }

        PushUndoSnapshot();

        var start = Math.Min(_selectionStart, _selectionEnd);
        Document.DeleteRange(start, SelectionLength);
        SetCaretOffset(start);
        SetSelection(start, start);
        _redoStack.Clear();
    }

    private void DeleteBackward()
    {
        if (IsReadOnly)
        {
            return;
        }

        if (SelectionLength > 0)
        {
            DeleteSelection();
            return;
        }

        if (_caretOffset == 0)
        {
            return;
        }

        PushUndoSnapshot();
        Document.DeleteRange(_caretOffset - 1, 1);
        SetCaretOffset(_caretOffset - 1);
        SetSelection(_caretOffset, _caretOffset);
        _redoStack.Clear();
    }

    private void DeleteForward()
    {
        if (IsReadOnly)
        {
            return;
        }

        if (SelectionLength > 0)
        {
            DeleteSelection();
            return;
        }

        if (_caretOffset >= Document.Length)
        {
            return;
        }

        PushUndoSnapshot();
        Document.DeleteRange(_caretOffset, 1);
        SetSelection(_caretOffset, _caretOffset);
        _redoStack.Clear();
    }

    public void ToggleBold()
    {
        ApplyStyle(style => style.WithFontWeight(style.FontWeight == FontWeight.Bold ? FontWeight.Normal : FontWeight.Bold));
    }

    public void ToggleItalic()
    {
        ApplyStyle(style => style.WithFontStyle(style.FontStyle == FontStyle.Italic ? FontStyle.Normal : FontStyle.Italic));
    }

    public void ToggleUnderline()
    {
        ApplyStyle(style => style.WithUnderline(!style.Underline));
    }

    public void SetFontFamily(FontFamily fontFamily)
    {
        ApplyStyle(style => style.WithFontFamily(fontFamily));
    }

    public void SetFontSize(double fontSize)
    {
        if (double.IsNaN(fontSize) || double.IsInfinity(fontSize) || fontSize <= 0)
        {
            return;
        }

        ApplyStyle(style => style.WithFontSize(fontSize));
    }

    public void SetForeground(IBrush? brush)
    {
        ApplyStyle(style => style.WithForeground(brush));
    }

    public void SetBackground(IBrush? brush)
    {
        ApplyStyle(style => style.WithBackground(brush));
    }

    public RichTextStyle GetCurrentStyleSnapshot()
    {
        if (SelectionLength > 0 && Document.Length > 0)
        {
            var start = Math.Min(_selectionStart, _selectionEnd);
            return Document.GetStyleAtOffset(start);
        }

        return _currentStyle;
    }

    public TextAlignment GetParagraphAlignmentAtCaret()
    {
        if (Document.Length == 0)
        {
            return GetValue(TextAlignmentProperty);
        }

        var offset = SelectionLength > 0
            ? Math.Min(_selectionStart, _selectionEnd)
            : Math.Clamp(_caretOffset, 0, Math.Max(0, Document.Length - 1));

        var alignment = Document.GetParagraphAlignment(offset);
        return alignment ?? GetValue(TextAlignmentProperty);
    }

    public void ApplyStyle(Func<RichTextStyle, RichTextStyle> transform)
    {
        ArgumentNullException.ThrowIfNull(transform);

        if (SelectionLength > 0)
        {
            PushUndoSnapshot();
            var start = Math.Min(_selectionStart, _selectionEnd);
            Document.ApplyStyle(start, SelectionLength, transform);
            _redoStack.Clear();
        }
        else
        {
            _currentStyle = transform(_currentStyle);
        }

        UpdatePresenterSelection();
    }

    private bool HandleNavigationKeys(KeyEventArgs e)
    {
        var modifiers = e.KeyModifiers;
        var shift = modifiers.HasFlag(KeyModifiers.Shift);
        var command = HasCommandModifier(modifiers);

        switch (e.Key)
        {
            case Key.Left:
                MoveCaretHorizontal(-1, shift, command);
                return true;
            case Key.Right:
                MoveCaretHorizontal(1, shift, command);
                return true;
            case Key.Home:
                if (command)
                {
                    MoveCaretTo(0, shift);
                }
                else
                {
                    MoveCaretToLineStart(shift);
                }
                return true;
            case Key.End:
                if (command)
                {
                    MoveCaretTo(Document.Length, shift);
                }
                else
                {
                    MoveCaretToLineEnd(shift);
                }
                return true;
            case Key.Up:
                MoveCaretVertical(-1, shift);
                return true;
            case Key.Down:
                MoveCaretVertical(1, shift);
                return true;
            case Key.A when command:
                SelectAll();
                return true;
        }

        return false;
    }

    private bool HandleEditingKeys(KeyEventArgs e)
    {
        if (IsReadOnly)
        {
            return false;
        }

        var command = HasCommandModifier(e.KeyModifiers);

        switch (e.Key)
        {
            case Key.Back:
                DeleteBackward();
                return true;
            case Key.Delete:
                DeleteForward();
                return true;
            case Key.Enter:
                InsertText("\n");
                return true;
            case Key.Tab:
                InsertText("\t");
                return true;
            case Key.Z when command && !e.KeyModifiers.HasFlag(KeyModifiers.Shift):
                Undo();
                return true;
            case Key.Z when command && e.KeyModifiers.HasFlag(KeyModifiers.Shift):
            case Key.Y when command:
                Redo();
                return true;
            case Key.C when command:
                CopyAsync().FireAndForget();
                return true;
            case Key.X when command:
                CutAsync().FireAndForget();
                return true;
            case Key.V when command:
                PasteAsync().FireAndForget();
                return true;
        }

        return false;
    }

    private bool HandleFormattingKeys(KeyEventArgs e)
    {
        var command = HasCommandModifier(e.KeyModifiers);
        if (!command)
        {
            return false;
        }

        switch (e.Key)
        {
            case Key.B:
                ToggleBold();
                return true;
            case Key.I:
                ToggleItalic();
                return true;
            case Key.U:
                ToggleUnderline();
                return true;
        }

        return false;
    }

    private void MoveCaretHorizontal(int delta, bool extendSelection, bool wordWise)
    {
        var offset = _caretOffset;
        if (wordWise)
        {
            offset = delta < 0 ? FindPreviousWordOffset(offset) : FindNextWordOffset(offset);
        }
        else
        {
            offset = Math.Clamp(offset + delta, 0, Document.Length);
        }

        MoveCaretTo(offset, extendSelection);
    }

    private void MoveCaretVertical(int direction, bool extendSelection)
    {
        var text = Document.Text;
        var currentLineStart = text.LastIndexOf('\n', Math.Max(0, _caretOffset - 1)) + 1;
        var currentLineEnd = text.IndexOf('\n', _caretOffset);
        if (currentLineEnd == -1)
        {
            currentLineEnd = text.Length;
        }

        var column = _caretOffset - currentLineStart;

        if (direction < 0)
        {
            if (currentLineStart == 0)
            {
                MoveCaretTo(0, extendSelection);
                return;
            }

            var previousLineEnd = currentLineStart - 1;
            var previousLineStart = text.LastIndexOf('\n', Math.Max(0, previousLineEnd - 1)) + 1;
            var previousLineLength = previousLineEnd - previousLineStart;
            var target = previousLineStart + Math.Min(column, previousLineLength);
            MoveCaretTo(target, extendSelection);
        }
        else
        {
            if (currentLineEnd >= text.Length)
            {
                MoveCaretTo(text.Length, extendSelection);
                return;
            }

            var nextLineStart = currentLineEnd + 1;
            var nextLineEnd = text.IndexOf('\n', nextLineStart);
            if (nextLineEnd == -1)
            {
                nextLineEnd = text.Length;
            }

            var nextLineLength = nextLineEnd - nextLineStart;
            var target = nextLineStart + Math.Min(column, nextLineLength);
            MoveCaretTo(target, extendSelection);
        }
    }

    private void MoveCaretToLineStart(bool extendSelection)
    {
        var text = Document.Text;
        var start = text.LastIndexOf('\n', Math.Max(0, _caretOffset - 1)) + 1;
        MoveCaretTo(start, extendSelection);
    }

    private void MoveCaretToLineEnd(bool extendSelection)
    {
        var text = Document.Text;
        var end = text.IndexOf('\n', _caretOffset);
        if (end == -1)
        {
            end = text.Length;
        }
        MoveCaretTo(end, extendSelection);
    }

    private void MoveCaretTo(int offset, bool extendSelection)
    {
        offset = Math.Clamp(offset, 0, Document.Length);
        if (extendSelection)
        {
            SetSelectionEnd(offset);
        }
        else
        {
            SetSelection(offset, offset);
        }

        SetCaretOffset(offset);
    }

    private int FindPreviousWordOffset(int offset)
    {
        if (offset == 0)
        {
            return 0;
        }

        var text = Document.Text;
        offset--;
        while (offset > 0 && char.IsWhiteSpace(text[offset]))
        {
            offset--;
        }

        while (offset > 0 && !char.IsWhiteSpace(text[offset - 1]))
        {
            offset--;
        }

        return offset;
    }

    private int FindNextWordOffset(int offset)
    {
        var text = Document.Text;
        var length = text.Length;
        if (offset >= length)
        {
            return length;
        }

        while (offset < length && !char.IsWhiteSpace(text[offset]))
        {
            offset++;
        }

        while (offset < length && char.IsWhiteSpace(text[offset]))
        {
            offset++;
        }

        return offset;
    }

    private void EnsureCaretVisible()
    {
        if (_presenter is null)
        {
            return;
        }

        var rect = _presenter.GetCaretRectangle(_caretOffset);
        if (rect is null)
        {
            return;
        }

        _presenter.BringIntoView(rect.Value);
    }

    private void SetText(string text)
    {
        PushUndoSnapshot();
        Document.SetText(text, _currentStyle);
        SetSelection(0, 0);
        SetCaretOffset(0);
        _redoStack.Clear();
    }

    private void ClampSelection()
    {
        SetSelection(Math.Clamp(_selectionStart, 0, Document.Length), Math.Clamp(_selectionEnd, 0, Document.Length));
        SetCaretOffset(Math.Clamp(_caretOffset, 0, Document.Length));
    }

    private void UpdatePresenterSelection()
    {
        if (_presenter is null)
        {
            return;
        }

        _presenter.SelectionStart = _selectionStart;
        _presenter.SelectionEnd = _selectionEnd;
    }

    private void UpdatePresenterCaret()
    {
        if (_presenter is null)
        {
            return;
        }

        _presenter.CaretOffset = _caretOffset;
        _presenter.CaretVisible = _caretVisible;
        _presenter.CaretBrush = CaretBrush;
        _presenter.CaretWidth = CaretWidth;
    }

    private void ResetCaretBlink()
    {
        _caretVisible = true;
        UpdatePresenterCaret();
        _caretTimer.Stop();
        _caretTimer.Start();
    }

    private void ToggleCaretVisibility()
    {
        _caretVisible = !_caretVisible;
        UpdatePresenterCaret();
    }

    private void RaiseTextChanged()
    {
        TextChanged?.Invoke(this, EventArgs.Empty);
    }

    private void SelectionChangedInternal()
    {
        UpdatePresenterSelection();
        SelectionChanged?.Invoke(this, EventArgs.Empty);
    }

    private void PushUndoSnapshot()
    {
        if (_suppressUndo)
        {
            return;
        }

        _undoStack.Push(Document.Clone());
    }

    private List<(int Start, int Length)> GetSelectedLineRanges()
    {
        var ranges = new List<(int Start, int Length)>();
        var text = Document.Text;

        if (string.IsNullOrEmpty(text))
        {
            ranges.Add((0, 0));
            return ranges;
        }

        var (lineStarts, lineEnds) = GetLineBoundaries(text);

        var selectionStart = Math.Min(_selectionStart, _selectionEnd);
        var selectionEnd = Math.Max(_selectionStart, _selectionEnd);

        selectionStart = Math.Clamp(selectionStart, 0, text.Length);
        selectionEnd = Math.Clamp(selectionEnd, 0, text.Length);

        var startLine = GetLineIndex(lineStarts, selectionStart);
        var target = selectionEnd > selectionStart ? Math.Max(selectionEnd - 1, selectionStart) : selectionStart;
        var endLine = GetLineIndex(lineStarts, target);

        for (var line = startLine; line <= endLine && line < lineStarts.Count; line++)
        {
            var lineStart = lineStarts[line];
            var lineEnd = lineEnds[line];
            ranges.Add((lineStart, Math.Max(0, lineEnd - lineStart)));
        }

        return ranges;
    }

    private static (List<int> Starts, List<int> Ends) GetLineBoundaries(string text)
    {
        var starts = new List<int>();
        var ends = new List<int>();

        var lineStart = 0;
        for (var i = 0; i < text.Length; i++)
        {
            if (text[i] == '\n')
            {
                starts.Add(lineStart);
                ends.Add(i);
                lineStart = i + 1;
            }
        }

        starts.Add(lineStart);
        ends.Add(text.Length);

        return (starts, ends);
    }

    private static int GetLineIndex(List<int> lineStarts, int offset)
    {
        var index = lineStarts.BinarySearch(offset);
        if (index >= 0)
        {
            return index;
        }

        index = ~index - 1;
        return Math.Clamp(index, 0, Math.Max(0, lineStarts.Count - 1));
    }

    private static int CalculateIndentRemoval(string text, (int Start, int Length) line)
    {
        if (line.Length <= 0 || line.Start >= text.Length)
        {
            return 0;
        }

        if (text[line.Start] == '\t')
        {
            return 1;
        }

        var max = Math.Min(line.Length, 4);
        var removal = 0;

        for (var i = 0; i < max && line.Start + i < text.Length; i++)
        {
            if (text[line.Start + i] == ' ')
            {
                removal++;
            }
            else
            {
                break;
            }
        }

        return removal;
    }

    private static bool LineStartsWithBullet(string text, (int Start, int Length) line)
    {
        if (line.Length <= 0 || line.Start >= text.Length)
        {
            return false;
        }

        if (text[line.Start] != '•')
        {
            return false;
        }

        if (line.Length == 1)
        {
            return true;
        }

        return line.Start + 1 < text.Length && text[line.Start + 1] == ' ';
    }

    private static int RemoveBulletPrefix(string text, (int Start, int Length) line)
    {
        if (!LineStartsWithBullet(text, line))
        {
            return 0;
        }

        if (line.Start + 1 < text.Length && text[line.Start + 1] == ' ')
        {
            return 2;
        }

        return 1;
    }

    private static bool TryGetNumberPrefixLength(string text, (int Start, int Length) line, out int length)
    {
        length = 0;

        if (line.Length <= 0 || line.Start >= text.Length)
        {
            return false;
        }

        var index = line.Start;
        var max = Math.Min(line.Start + line.Length, text.Length);

        while (index < max && char.IsDigit(text[index]))
        {
            index++;
        }

        if (index == line.Start)
        {
            return false;
        }

        if (index >= max || text[index] != '.')
        {
            return false;
        }

        if (index + 1 >= text.Length || text[index + 1] != ' ')
        {
            return false;
        }

        length = (index - line.Start) + 2;
        return true;
    }

    private RichTextStyle GetStyleForInsertion(int offset)
    {
        if (Document.Length == 0)
        {
            return _currentStyle;
        }

        var clamped = Math.Clamp(offset, 0, Math.Max(0, Document.Length - 1));
        return Document.GetStyleAtOffset(clamped);
    }

    private void RequestPresenterRefresh()
    {
        _presenter?.ForceSnapshotRebuild();
    }

    private void UpdateCurrentStyleFromCaret()
    {
        if (Document.Length == 0)
        {
            _currentStyle = RichTextStyle.Default;
            return;
        }

        var offset = Math.Clamp(_caretOffset - 1, 0, Math.Max(0, Document.Length - 1));
        _currentStyle = Document.GetStyleAtOffset(offset);
    }

    private static bool HasCommandModifier(KeyModifiers modifiers)
    {
        return RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
            ? modifiers.HasFlag(KeyModifiers.Meta)
            : modifiers.HasFlag(KeyModifiers.Control);
    }
}

internal static class TaskExtensions
{
    public static void FireAndForget(this Task task)
    {
        if (task is null)
        {
            return;
        }

        task.ContinueWith(t => Debug.WriteLine(t.Exception), TaskContinuationOptions.OnlyOnFaulted);
    }
}
