using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Media;
using TextFlow.Editor.Controls;
using TextFlow.Editor.Documents;

namespace TextFlow.Editor.Sample;

public partial class MainWindow : Window
{
    private RichTextBox? _editor;
    private ToggleButton? _boldButton;
    private ToggleButton? _italicButton;
    private ToggleButton? _underlineButton;
    private ToggleButton? _alignLeftButton;
    private ToggleButton? _alignCenterButton;
    private ToggleButton? _alignJustifyButton;
    private ToggleButton? _bulletListButton;
    private ToggleButton? _numberListButton;
    private ComboBox? _fontFamilyCombo;
    private ComboBox? _fontSizeCombo;
    private ComboBox? _fontColorCombo;
    private bool _isUpdatingToolbar;

    private readonly List<FontOption> _fontOptions = new();
    private readonly ObservableCollection<double> _fontSizes = new() { 10, 12, 14, 16, 18, 20, 24, 28, 32, 36, 48 };
    private readonly List<BrushOption> _colorOptions = new();

    public MainWindow()
    {
        InitializeComponent();
        InitializeToolbar();
        ConfigureSampleDocument();
    }

    private void InitializeToolbar()
    {
        _editor = this.FindControl<RichTextBox>("Editor") ?? throw new InvalidOperationException("Editor control not found.");
        _boldButton = this.FindControl<ToggleButton>("BoldButton");
        _italicButton = this.FindControl<ToggleButton>("ItalicButton");
        _underlineButton = this.FindControl<ToggleButton>("UnderlineButton");
    _alignLeftButton = this.FindControl<ToggleButton>("AlignLeftButton");
    _alignCenterButton = this.FindControl<ToggleButton>("AlignCenterButton");
    _alignJustifyButton = this.FindControl<ToggleButton>("AlignJustifyButton");
    _bulletListButton = this.FindControl<ToggleButton>("BulletListButton");
    _numberListButton = this.FindControl<ToggleButton>("NumberListButton");
        _fontFamilyCombo = this.FindControl<ComboBox>("FontFamilyCombo");
        _fontSizeCombo = this.FindControl<ComboBox>("FontSizeCombo");
        _fontColorCombo = this.FindControl<ComboBox>("FontColorCombo");

        _fontOptions.AddRange(new[]
        {
            new FontOption("Default", FontFamily.Default),
            new FontOption("Inter", new FontFamily("avares://Avalonia.Fonts.Inter#Inter")),
            new FontOption("Times New Roman", new FontFamily("Times New Roman")),
            new FontOption("Georgia", new FontFamily("Georgia")),
            new FontOption("Courier New", new FontFamily("Courier New"))
        });

        _colorOptions.AddRange(new[]
        {
            new BrushOption("Black", Brushes.Black),
            new BrushOption("Gray", Brushes.DimGray),
            new BrushOption("Blue", Brushes.DodgerBlue),
            new BrushOption("Green", Brushes.ForestGreen),
            new BrushOption("Orange", Brushes.OrangeRed),
            new BrushOption("Purple", Brushes.MediumPurple)
        });

        _isUpdatingToolbar = true;
        try
        {
            if (_fontFamilyCombo is not null)
            {
                _fontFamilyCombo.ItemsSource = _fontOptions;
                _fontFamilyCombo.SelectedItem = _fontOptions.FirstOrDefault();
            }

            if (_fontSizeCombo is not null)
            {
                _fontSizeCombo.ItemsSource = _fontSizes;
                _fontSizeCombo.SelectedItem = 14d;
            }

            if (_fontColorCombo is not null)
            {
                _fontColorCombo.ItemsSource = _colorOptions;
                _fontColorCombo.SelectedItem = _colorOptions.FirstOrDefault();
            }
        }
        finally
        {
            _isUpdatingToolbar = false;
        }

        _editor.SelectionChanged += (_, _) => SyncFormattingControls();
        _editor.TextChanged += (_, _) => SyncFormattingControls();
    }

    private void ConfigureSampleDocument()
    {
        if (_editor is null)
        {
            return;
        }

        var document = new RichTextDocument();
        var content = "TextFlow Editor Sample\n\n" +
                      "This sample demonstrates the custom RichTextBox control built on Avalonia.\n" +
                      "Use the toolbar to apply bold, italic, underline, font changes, and colors just like a word processor.\n" +
                      "Common shortcuts like Ctrl+Z / Ctrl+Y, Ctrl+C / Ctrl+V are also available.\n\n" +
                      "Highlights:\n" +
                      "• Styled spans with bold, italic, underline, and colors.\n" +
                      "• Document API for programmatic manipulation.\n" +
                      "• Alignment, indentation, and list controls via the toolbar.\n" +
                      "• Clipboard support for copy, cut, and paste.\n" +
                      "• Undo/redo stacks and caret navigation.\n";

        document.SetText(content);

        var title = "TextFlow Editor Sample";
        document.ApplyStyle(0, title.Length, style =>
            style.WithFontSize(28)
                 .WithFontWeight(FontWeight.SemiBold));

        Highlight("This sample demonstrates the custom RichTextBox control built on Avalonia.",
            style => style.WithFontStyle(FontStyle.Italic));

        Highlight("Highlights:", style =>
            style.WithFontWeight(FontWeight.SemiBold)
                 .WithUnderline(true));

        Highlight("Styled spans with bold, italic, underline, and colors.",
            style => style.WithForeground(Brushes.DodgerBlue));

        Highlight("Document API for programmatic manipulation.",
            style => style.WithForeground(Brushes.ForestGreen));

        Highlight("Alignment, indentation, and list controls via the toolbar.",
            style => style.WithForeground(Brushes.MediumPurple));

        Highlight("Clipboard support for copy, cut, and paste.",
            style => style.WithForeground(Brushes.OrangeRed));

        _editor.Document = document;
        _editor.Select(document.Length, 0);
        SyncFormattingControls();

        void Highlight(string phrase, Func<RichTextStyle, RichTextStyle> update)
        {
            var index = document.Text.IndexOf(phrase, StringComparison.Ordinal);
            if (index >= 0)
            {
                document.ApplyStyle(index, phrase.Length, update);
            }
        }
    }

    private void BoldButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (_editor is null)
        {
            return;
        }

        _editor.ToggleBold();
        SyncFormattingControls();
    }

    private void ItalicButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (_editor is null)
        {
            return;
        }

        _editor.ToggleItalic();
        SyncFormattingControls();
    }

    private void UnderlineButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (_editor is null)
        {
            return;
        }

        _editor.ToggleUnderline();
        SyncFormattingControls();
    }

    private void AlignLeftButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (_editor is null)
        {
            return;
        }

        _editor.SetTextAlignment(TextAlignment.Left);
        SyncFormattingControls();
    }

    private void AlignCenterButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (_editor is null)
        {
            return;
        }

        _editor.SetTextAlignment(TextAlignment.Center);
        SyncFormattingControls();
    }

    private void AlignJustifyButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (_editor is null)
        {
            return;
        }

        _editor.SetTextAlignment(TextAlignment.Justify);
        SyncFormattingControls();
    }

    private void DecreaseIndentButton_OnClick(object? sender, RoutedEventArgs e)
    {
        _editor?.DecreaseIndentation();
        SyncFormattingControls();
    }

    private void IncreaseIndentButton_OnClick(object? sender, RoutedEventArgs e)
    {
        _editor?.IncreaseIndentation();
        SyncFormattingControls();
    }

    private void BulletListButton_OnClick(object? sender, RoutedEventArgs e)
    {
        _editor?.ToggleBulletList();
        SyncFormattingControls();
    }

    private void NumberListButton_OnClick(object? sender, RoutedEventArgs e)
    {
        _editor?.ToggleNumberedList();
        SyncFormattingControls();
    }

    private void InsertTableButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (_editor is null)
        {
            return;
        }

        _editor.InsertTable(3, 3);
        SyncFormattingControls();
    }

    private void FontFamilyCombo_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (_isUpdatingToolbar || _editor is null)
        {
            return;
        }

        if (sender is ComboBox combo && combo.SelectedItem is FontOption option)
        {
            _editor.SetFontFamily(option.Family);
            SyncFormattingControls();
        }
    }

    private void FontSizeCombo_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (_isUpdatingToolbar || _editor is null)
        {
            return;
        }

        if (sender is ComboBox combo && combo.SelectedItem is double size)
        {
            _editor.SetFontSize(size);
            SyncFormattingControls();
        }
    }

    private void FontColorCombo_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (_isUpdatingToolbar || _editor is null)
        {
            return;
        }

        if (sender is ComboBox combo && combo.SelectedItem is BrushOption option)
        {
            _editor.SetForeground(option.Brush);
            SyncFormattingControls();
        }
    }

    private void UndoButton_OnClick(object? sender, RoutedEventArgs e)
    {
        _editor?.Undo();
        SyncFormattingControls();
    }

    private void RedoButton_OnClick(object? sender, RoutedEventArgs e)
    {
        _editor?.Redo();
        SyncFormattingControls();
    }

    private async void CutButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (_editor is null)
        {
            return;
        }

        await _editor.CutAsync();
        SyncFormattingControls();
    }

    private async void CopyButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (_editor is null)
        {
            return;
        }

        await _editor.CopyAsync();
    }

    private async void PasteButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (_editor is null)
        {
            return;
        }

        await _editor.PasteAsync();
        SyncFormattingControls();
    }

    private void SyncFormattingControls()
    {
        if (_editor is null)
        {
            return;
        }

        var style = _editor.GetCurrentStyleSnapshot();
    var paragraphAlignment = _editor.GetParagraphAlignmentAtCaret();
        _isUpdatingToolbar = true;

        try
        {
            if (_boldButton is not null)
            {
                _boldButton.IsChecked = IsBold(style.FontWeight);
            }

            if (_italicButton is not null)
            {
                _italicButton.IsChecked = style.FontStyle == FontStyle.Italic;
            }

            if (_underlineButton is not null)
            {
                _underlineButton.IsChecked = style.Underline;
            }

            if (_alignLeftButton is not null)
            {
                _alignLeftButton.IsChecked = paragraphAlignment == TextAlignment.Left;
            }

            if (_alignCenterButton is not null)
            {
                _alignCenterButton.IsChecked = paragraphAlignment == TextAlignment.Center;
            }

            if (_alignJustifyButton is not null)
            {
                _alignJustifyButton.IsChecked = paragraphAlignment == TextAlignment.Justify;
            }

            if (_fontFamilyCombo is not null)
            {
                var match = _fontOptions.FirstOrDefault(option => FontFamiliesEqual(option.Family, style.FontFamily));
                _fontFamilyCombo.SelectedItem = match ?? _fontOptions.FirstOrDefault();
            }

            if (_fontSizeCombo is not null)
            {
                EnsureFontSizePresent(style.FontSize);
                var index = FindFontSizeIndex(style.FontSize);
                if (index >= 0)
                {
                    _fontSizeCombo.SelectedItem = _fontSizes[index];
                }
            }

            if (_fontColorCombo is not null)
            {
                BrushOption? match = null;
                if (style.Foreground is ISolidColorBrush solid)
                {
                    match = _colorOptions.FirstOrDefault(option =>
                        option.Brush is ISolidColorBrush brush && brush.Color == solid.Color);
                }

                _fontColorCombo.SelectedItem = match ?? _colorOptions.FirstOrDefault();
            }

            if (_bulletListButton is not null)
            {
                _bulletListButton.IsChecked = _editor.SelectionHasBulletList();
            }

            if (_numberListButton is not null)
            {
                _numberListButton.IsChecked = _editor.SelectionHasNumberedList();
            }
        }
        finally
        {
            _isUpdatingToolbar = false;
        }
    }

    private void EnsureFontSizePresent(double fontSize)
    {
        if (FindFontSizeIndex(fontSize) >= 0)
        {
            return;
        }

        for (var i = 0; i < _fontSizes.Count; i++)
        {
            if (fontSize < _fontSizes[i])
            {
                _fontSizes.Insert(i, fontSize);
                return;
            }
        }

        _fontSizes.Add(fontSize);
    }

    private int FindFontSizeIndex(double fontSize)
    {
        for (var i = 0; i < _fontSizes.Count; i++)
        {
            if (Math.Abs(_fontSizes[i] - fontSize) < 0.1)
            {
                return i;
            }
        }

        return -1;
    }

    private static bool FontFamiliesEqual(FontFamily left, FontFamily right)
    {
        return left.Equals(right) ||
               string.Equals(left.ToString(), right.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsBold(FontWeight weight)
    {
        return weight.ToString().Contains("Bold", StringComparison.OrdinalIgnoreCase);
    }

    private sealed record FontOption(string Name, FontFamily Family)
    {
        public override string ToString() => Name;
    }

    private sealed record BrushOption(string Name, IBrush Brush)
    {
        public override string ToString() => Name;
    }
}
