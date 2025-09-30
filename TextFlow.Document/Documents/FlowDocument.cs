using System;
using Avalonia;
using Avalonia.Media;
using Avalonia.Metadata;
using Avalonia.Styling;

namespace TextFlow.Document.Documents;

/// <summary>
/// Lightweight Avalonia implementation of WPF's <c>FlowDocument</c> surface.
/// </summary>
public class FlowDocument : StyledElement, IBlockCollectionHost
{
    public static readonly StyledProperty<FontFamily> FontFamilyProperty =
        AvaloniaProperty.Register<FlowDocument, FontFamily>(nameof(FontFamily), FontFamily.Default);

    public static readonly StyledProperty<double> FontSizeProperty =
        AvaloniaProperty.Register<FlowDocument, double>(nameof(FontSize), 14);

    public static readonly StyledProperty<FontStyle> FontStyleProperty =
        AvaloniaProperty.Register<FlowDocument, FontStyle>(nameof(FontStyle), FontStyle.Normal);

    public static readonly StyledProperty<FontWeight> FontWeightProperty =
        AvaloniaProperty.Register<FlowDocument, FontWeight>(nameof(FontWeight), FontWeight.Normal);

    public static readonly StyledProperty<FontStretch> FontStretchProperty =
        AvaloniaProperty.Register<FlowDocument, FontStretch>(nameof(FontStretch), FontStretch.Normal);

    public static readonly StyledProperty<IBrush?> ForegroundProperty =
        AvaloniaProperty.Register<FlowDocument, IBrush?>(nameof(Foreground), Brushes.Black);

    public static readonly StyledProperty<IBrush?> BackgroundProperty =
        AvaloniaProperty.Register<FlowDocument, IBrush?>(nameof(Background));

    public static readonly StyledProperty<TextAlignment> TextAlignmentProperty =
        AvaloniaProperty.Register<FlowDocument, TextAlignment>(nameof(TextAlignment), TextAlignment.Left);

    public static readonly StyledProperty<FlowDirection> FlowDirectionProperty =
        AvaloniaProperty.Register<FlowDocument, FlowDirection>(nameof(FlowDirection), FlowDirection.LeftToRight);

    public static readonly StyledProperty<double> LineHeightProperty =
        AvaloniaProperty.Register<FlowDocument, double>(nameof(LineHeight), double.NaN);

    public static readonly StyledProperty<Thickness> PagePaddingProperty =
        AvaloniaProperty.Register<FlowDocument, Thickness>(nameof(PagePadding), new Thickness(24));

    public static readonly StyledProperty<TextDecorationCollection?> TextDecorationsProperty =
        AvaloniaProperty.Register<FlowDocument, TextDecorationCollection?>(nameof(TextDecorations));

    private readonly BlockCollection _blocks;

    public FlowDocument()
    {
    _blocks = new BlockCollection(this);
    }

    [Content]
    public BlockCollection Blocks => _blocks;

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

    public FontStretch FontStretch
    {
        get => GetValue(FontStretchProperty);
        set => SetValue(FontStretchProperty, value);
    }

    public IBrush? Foreground
    {
        get => GetValue(ForegroundProperty);
        set => SetValue(ForegroundProperty, value);
    }

    public IBrush? Background
    {
        get => GetValue(BackgroundProperty);
        set => SetValue(BackgroundProperty, value);
    }

    public TextAlignment TextAlignment
    {
        get => GetValue(TextAlignmentProperty);
        set => SetValue(TextAlignmentProperty, value);
    }

    public FlowDirection FlowDirection
    {
        get => GetValue(FlowDirectionProperty);
        set => SetValue(FlowDirectionProperty, value);
    }

    public double LineHeight
    {
        get => GetValue(LineHeightProperty);
        set => SetValue(LineHeightProperty, value);
    }

    public Thickness PagePadding
    {
        get => GetValue(PagePaddingProperty);
        set => SetValue(PagePaddingProperty, value);
    }

    public TextDecorationCollection? TextDecorations
    {
        get => GetValue(TextDecorationsProperty);
        set => SetValue(TextDecorationsProperty, value);
    }

    internal event EventHandler? Changed;

    internal void NotifyChanged()
    {
        Changed?.Invoke(this, EventArgs.Empty);
    }

    FlowDocument? IBlockCollectionHost.Document => this;

    void IBlockCollectionHost.NotifyBlocksChanged() => NotifyChanged();

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == FontFamilyProperty ||
            change.Property == FontSizeProperty ||
            change.Property == FontStyleProperty ||
            change.Property == FontWeightProperty ||
            change.Property == FontStretchProperty ||
            change.Property == ForegroundProperty ||
            change.Property == BackgroundProperty ||
            change.Property == TextAlignmentProperty ||
            change.Property == FlowDirectionProperty ||
            change.Property == LineHeightProperty ||
            change.Property == PagePaddingProperty ||
            change.Property == TextDecorationsProperty)
        {
            NotifyChanged();
        }
    }
}
