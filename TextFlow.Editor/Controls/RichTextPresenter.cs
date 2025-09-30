using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.TextFormatting;
using Avalonia.Threading;
using TextFlow.Document.Controls;
using TextFlow.Document.Documents;
using FlowDocumentDocument = TextFlow.Document.Documents.FlowDocument;
using TextFlow.Editor.Documents;

namespace TextFlow.Editor.Controls;

public class RichTextPresenter : FlowDocumentView
{
    public static new readonly StyledProperty<RichTextDocument> DocumentProperty =
        AvaloniaProperty.Register<RichTextPresenter, RichTextDocument>(
            nameof(Document),
            defaultValue: new RichTextDocument());

    public static readonly StyledProperty<int> CaretOffsetProperty =
        AvaloniaProperty.Register<RichTextPresenter, int>(nameof(CaretOffset));

    public static readonly StyledProperty<int> SelectionStartProperty =
        AvaloniaProperty.Register<RichTextPresenter, int>(nameof(SelectionStart));

    public static readonly StyledProperty<int> SelectionEndProperty =
        AvaloniaProperty.Register<RichTextPresenter, int>(nameof(SelectionEnd));

    public static readonly StyledProperty<bool> CaretVisibleProperty =
        AvaloniaProperty.Register<RichTextPresenter, bool>(nameof(CaretVisible));

    public static readonly StyledProperty<IBrush?> SelectionBrushProperty =
        AvaloniaProperty.Register<RichTextPresenter, IBrush?>(
            nameof(SelectionBrush),
            defaultValue: new SolidColorBrush(Color.FromArgb(0xFF, 0x42, 0x82, 0xF1)));

    public static readonly StyledProperty<IBrush?> CaretBrushProperty =
        AvaloniaProperty.Register<RichTextPresenter, IBrush?>(
            nameof(CaretBrush),
            defaultValue: Brushes.Black);

    public static readonly StyledProperty<double> CaretWidthProperty =
        AvaloniaProperty.Register<RichTextPresenter, double>(nameof(CaretWidth), 1.0);

    public static readonly StyledProperty<FontFamily> FontFamilyProperty =
        AvaloniaProperty.Register<RichTextPresenter, FontFamily>(nameof(FontFamily), FontFamily.Default);

    public static readonly StyledProperty<double> FontSizeProperty =
        AvaloniaProperty.Register<RichTextPresenter, double>(nameof(FontSize), 14);

    public static readonly StyledProperty<FontStyle> FontStyleProperty =
        AvaloniaProperty.Register<RichTextPresenter, FontStyle>(nameof(FontStyle), FontStyle.Normal);

    public static readonly StyledProperty<FontWeight> FontWeightProperty =
        AvaloniaProperty.Register<RichTextPresenter, FontWeight>(nameof(FontWeight), FontWeight.Normal);

    public static readonly StyledProperty<IBrush?> ForegroundProperty =
        AvaloniaProperty.Register<RichTextPresenter, IBrush?>(nameof(Foreground), Brushes.Black);

    public static readonly StyledProperty<TextWrapping> TextWrappingProperty =
        AvaloniaProperty.Register<RichTextPresenter, TextWrapping>(nameof(TextWrapping), TextWrapping.Wrap);

    public static readonly StyledProperty<TextAlignment> TextAlignmentProperty =
        AvaloniaProperty.Register<RichTextPresenter, TextAlignment>(nameof(TextAlignment), TextAlignment.Left);

    public static readonly StyledProperty<double> LineHeightProperty =
        AvaloniaProperty.Register<RichTextPresenter, double>(nameof(LineHeight), double.NaN);

    private FlowDocumentSnapshot? _snapshot;
    private RichTextDocument? _currentDocument;
    private bool _snapshotRebuildQueued;

    static RichTextPresenter()
    {
        ClipToBoundsProperty.OverrideDefaultValue<RichTextPresenter>(true);
        FocusableProperty.OverrideDefaultValue<RichTextPresenter>(false);
        CursorProperty.OverrideDefaultValue<RichTextPresenter>(new Cursor(StandardCursorType.Ibeam));
        AffectsMeasure<RichTextPresenter>(DocumentProperty, TextWrappingProperty, TextAlignmentProperty, LineHeightProperty,
            FontFamilyProperty, FontSizeProperty, FontStyleProperty, FontWeightProperty, ForegroundProperty);
        AffectsRender<RichTextPresenter>(CaretOffsetProperty, SelectionStartProperty, SelectionEndProperty, CaretVisibleProperty, SelectionBrushProperty, CaretBrushProperty, CaretWidthProperty);
    }

    public RichTextPresenter()
    {
        SetCurrentValue(DocumentProperty, new RichTextDocument());
        AttachToDocument(Document);
        QueueSnapshotRebuild();
    }

    public new RichTextDocument Document
    {
        get => GetValue(DocumentProperty);
        set => SetValue(DocumentProperty, value);
    }

    public int CaretOffset
    {
        get => GetValue(CaretOffsetProperty);
        set => SetValue(CaretOffsetProperty, value);
    }

    public int SelectionStart
    {
        get => GetValue(SelectionStartProperty);
        set => SetValue(SelectionStartProperty, value);
    }

    public int SelectionEnd
    {
        get => GetValue(SelectionEndProperty);
        set => SetValue(SelectionEndProperty, value);
    }

    public bool CaretVisible
    {
        get => GetValue(CaretVisibleProperty);
        set => SetValue(CaretVisibleProperty, value);
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

    public FontFamily FontFamily
    {
        get => GetValue(FontFamilyProperty);
        set => SetValue(FontFamilyProperty, value);
    }

    public double FontSize
    {
        get => GetValue(FontSizeProperty);
        set => SetValue(FontSizeProperty, value);
    }

    public FontStyle FontStyle
    {
        get => GetValue(FontStyleProperty);
        set => SetValue(FontStyleProperty, value);
    }

    public FontWeight FontWeight
    {
        get => GetValue(FontWeightProperty);
        set => SetValue(FontWeightProperty, value);
    }

    public IBrush? Foreground
    {
        get => GetValue(ForegroundProperty);
        set => SetValue(ForegroundProperty, value);
    }

    public TextWrapping TextWrapping
    {
        get => GetValue(TextWrappingProperty);
        set => SetValue(TextWrappingProperty, value);
    }

    public TextAlignment TextAlignment
    {
        get => GetValue(TextAlignmentProperty);
        set => SetValue(TextAlignmentProperty, value);
    }

    public double LineHeight
    {
        get => GetValue(LineHeightProperty);
        set => SetValue(LineHeightProperty, value);
    }

    private int SelectionLength => Math.Abs(SelectionEnd - SelectionStart);

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == DocumentProperty)
        {
            AttachToDocument(change.GetNewValue<RichTextDocument>());
            QueueSnapshotRebuild();
            return;
        }

        if (change.Property == FontFamilyProperty ||
            change.Property == FontSizeProperty ||
            change.Property == FontStyleProperty ||
            change.Property == FontWeightProperty ||
            change.Property == ForegroundProperty ||
            change.Property == TextAlignmentProperty ||
            change.Property == LineHeightProperty)
        {
            ApplyPresentationProperties();
            InvalidateMeasure();
            InvalidateVisual();
            return;
        }

        if (change.Property == TextWrappingProperty)
        {
            InvalidateMeasure();
            return;
        }

        if (change.Property == CaretOffsetProperty ||
            change.Property == SelectionStartProperty ||
            change.Property == SelectionEndProperty ||
            change.Property == CaretVisibleProperty ||
            change.Property == SelectionBrushProperty ||
            change.Property == CaretBrushProperty ||
            change.Property == CaretWidthProperty)
        {
            InvalidateVisual();
        }
    }

    protected override Size MeasureOverride(Size availableSize) => base.MeasureOverride(availableSize);

    protected override Size ArrangeOverride(Size finalSize) => base.ArrangeOverride(finalSize);

    public override void Render(DrawingContext context)
    {
        base.Render(context);
        DrawSelection(context);
        DrawCaret(context);
    }
    
    protected override void RenderDocumentContent(DrawingContext context, FlowDocumentDocument doc, Point origin)
    {
        base.RenderDocumentContent(context, doc, origin);
    }

    internal int GetOffsetFromPoint(Point point)
    {
        if (_snapshot is null)
        {
            return 0;
        }

        var blocks = GetVisualBlocks();
        if (blocks.Count == 0)
        {
            return 0;
        }

        foreach (var block in blocks)
        {
            if (block.Block is not Paragraph paragraph ||
                !_snapshot.TryGetParagraphSnapshot(paragraph, out var paragraphSnapshot) ||
                paragraphSnapshot is null)
            {
                continue;
            }

            if (point.Y < block.Top)
            {
                return paragraphSnapshot.DocumentStart;
            }

            if (point.Y < block.TextTop)
            {
                return paragraphSnapshot.DocumentStart;
            }

            if (point.Y <= block.TextBottom)
            {
                var localPoint = new Point(
                    point.X - block.TextOrigin.X,
                    Math.Clamp(point.Y - block.TextTop, 0, block.Layout.Height));

                var hit = block.Layout.HitTestPoint(localPoint);
                var flowPosition = hit.CharacterHit.FirstCharacterIndex;
                if (hit.IsTrailing)
                {
                    flowPosition += hit.CharacterHit.TrailingLength;
                }

                var localFlow = Math.Clamp(flowPosition, 0, paragraphSnapshot.FlowLength);
                var flowOffset = paragraphSnapshot.FlowStart + localFlow;
                return _snapshot.ToDocumentOffset(flowOffset);
            }

            if (point.Y <= block.Bottom)
            {
                if (paragraphSnapshot.HasTrailingNewline && paragraphSnapshot.Index + 1 < _snapshot.Paragraphs.Count)
                {
                    return _snapshot.Paragraphs[paragraphSnapshot.Index + 1].DocumentStart;
                }

                return paragraphSnapshot.DocumentEnd;
            }
        }

        var lastBlock = blocks[^1];
        if (lastBlock.Block is Paragraph lastParagraph &&
            _snapshot.TryGetParagraphSnapshot(lastParagraph, out var lastSnapshot) &&
            lastSnapshot is not null)
        {
            if (lastSnapshot.HasTrailingNewline && lastSnapshot.Index + 1 < _snapshot.Paragraphs.Count)
            {
                return _snapshot.Paragraphs[lastSnapshot.Index + 1].DocumentStart;
            }

            return lastSnapshot.DocumentEnd;
        }

        return _snapshot.DocumentLength;
    }

    internal Rect? GetCaretRectangle(int offset)
    {
        if (_snapshot is null)
        {
            return null;
        }

        offset = Math.Clamp(offset, 0, _snapshot.DocumentLength);
        var paragraphSnapshot = _snapshot.FindParagraphByDocumentOffset(offset);
        if (paragraphSnapshot is null)
        {
            return null;
        }

        foreach (var block in GetVisualBlocks())
        {
            if (!ReferenceEquals(block.Block, paragraphSnapshot.Paragraph))
            {
                continue;
            }

            var flowOffset = _snapshot.ToFlowOffset(offset);
            flowOffset = Math.Clamp(flowOffset, paragraphSnapshot.FlowStart, paragraphSnapshot.FlowEnd);
            var local = flowOffset - paragraphSnapshot.FlowStart;
            local = Math.Clamp(local, 0, paragraphSnapshot.FlowLength);

            var hit = block.Layout.HitTestTextPosition(local);
            var width = Math.Max(CaretWidth, 1d);
            var origin = block.TextOrigin + hit.Position;
            var height = Math.Max(hit.Height, block.Layout.Height);
            return new Rect(origin, new Size(width, height));
        }

        return null;
    }

    internal void ForceSnapshotRebuild() => QueueSnapshotRebuild();

    private void DrawSelection(DrawingContext context)
    {
        if (_snapshot is null || SelectionLength <= 0 || SelectionBrush is null)
        {
            return;
        }

        var (startFlow, endFlow) = GetOrderedFlowRange(SelectionStart, SelectionEnd);

        foreach (var block in GetVisualBlocks())
        {
            if (block.Block is not Paragraph paragraph ||
                !_snapshot.TryGetParagraphSnapshot(paragraph, out var snapshot) ||
                snapshot is null)
            {
                continue;
            }

            var blockStart = snapshot.FlowStart;
            var blockEnd = snapshot.FlowEnd;
            if (endFlow <= blockStart || startFlow >= blockEnd)
            {
                continue;
            }

            var localStart = Math.Max(0, startFlow - blockStart);
            var localEnd = Math.Min(blockEnd, endFlow) - blockStart;
            var length = Math.Max(0, localEnd - localStart);
            if (length <= 0)
            {
                continue;
            }

            foreach (var rect in block.Layout.HitTestTextRange(localStart, length))
            {
                if (rect.Width <= 0 && rect.Height <= 0)
                {
                    continue;
                }

                var translated = new Rect(rect.Position + block.TextOrigin, rect.Size);
                var brush = SelectionBrush?.ToImmutable();
                if (brush is not null)
                {
                    context.FillRectangle(brush, translated);
                }
            }
        }
    }

    private void DrawCaret(DrawingContext context)
    {
        if (!CaretVisible)
        {
            return;
        }

        var caretRect = GetCaretRectangle(CaretOffset);
        if (caretRect is null)
        {
            return;
        }

        var brush = CaretBrush ?? Brushes.Black;
        context.FillRectangle(brush, caretRect.Value);
    }

    private (int Start, int End) GetOrderedFlowRange(int startOffset, int endOffset)
    {
        if (_snapshot is null)
        {
            return (0, 0);
        }

        var start = _snapshot.ToFlowOffset(startOffset);
        var end = _snapshot.ToFlowOffset(endOffset);
        return start <= end ? (start, end) : (end, start);
    }

    private IReadOnlyList<FlowDocumentView.TextBlockVisualInfo> GetVisualBlocks()
    {
        return GetTextBlockVisuals();
    }

    private void OnDocumentChanged(object? sender, EventArgs e)
    {
        QueueSnapshotRebuild();
    }

    private void RebuildSnapshot()
    {
        if (_currentDocument is null)
        {
            _snapshot = null;
            base.Document = null;
            InvalidateMeasure();
            InvalidateVisual();
            return;
        }

        _snapshot = _currentDocument.CreateFlowSnapshot(TextAlignment);
        ApplyPresentationProperties();
        base.Document = _snapshot.FlowDocument;

        InvalidateMeasure();
        InvalidateVisual();
    }

    private void ApplyPresentationProperties()
    {
        if (_snapshot?.FlowDocument is not { } flowDocument)
        {
            return;
        }

        flowDocument.FontFamily = FontFamily;
        flowDocument.FontSize = FontSize;
        flowDocument.FontWeight = FontWeight;
        flowDocument.FontStyle = FontStyle;
        flowDocument.Foreground = Foreground;
        flowDocument.LineHeight = double.IsNaN(LineHeight) ? double.NaN : LineHeight;
        flowDocument.TextAlignment = TextAlignment;
    }

    private void QueueSnapshotRebuild()
    {
        if (_snapshotRebuildQueued)
        {
            return;
        }

        _snapshotRebuildQueued = true;

        void Rebuild()
        {
            _snapshotRebuildQueued = false;
            RebuildSnapshot();
        }

        if (Dispatcher.UIThread.CheckAccess())
        {
            Rebuild();
        }
        else
        {
            Dispatcher.UIThread.Post(Rebuild, DispatcherPriority.Render);
        }
    }

    private void AttachToDocument(RichTextDocument? document)
    {
        if (ReferenceEquals(_currentDocument, document))
        {
            return;
        }

        if (_currentDocument is { } oldDoc)
        {
            oldDoc.Changed -= OnDocumentChanged;
        }

        _currentDocument = document;

        if (_currentDocument is { } newDoc)
        {
            newDoc.Changed += OnDocumentChanged;
        }
    }
}
