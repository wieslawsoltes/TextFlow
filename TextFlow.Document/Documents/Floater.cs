using Avalonia;
using Avalonia.Layout;

namespace TextFlow.Document.Documents;

/// <summary>
/// Represents a floating anchored block that can align left, right, or center within a paragraph.
/// </summary>
public class Floater : AnchoredBlock
{
    public static readonly StyledProperty<HorizontalAlignment> HorizontalAlignmentProperty =
        AvaloniaProperty.Register<Floater, HorizontalAlignment>(nameof(HorizontalAlignment), HorizontalAlignment.Left);

    public static readonly StyledProperty<double> WidthProperty =
        AvaloniaProperty.Register<Floater, double>(nameof(Width), double.NaN);

    public static readonly StyledProperty<double> HeightProperty =
        AvaloniaProperty.Register<Floater, double>(nameof(Height), double.NaN);

    /// <summary>
    /// Gets or sets the horizontal alignment of the floater relative to the content column.
    /// </summary>
    public HorizontalAlignment HorizontalAlignment
    {
        get => GetValue(HorizontalAlignmentProperty);
        set => SetValue(HorizontalAlignmentProperty, value);
    }

    /// <summary>
    /// Gets or sets the desired width of the floater in device-independent pixels. Set to <see cref="double.NaN"/> to auto size.
    /// </summary>
    public double Width
    {
        get => GetValue(WidthProperty);
        set => SetValue(WidthProperty, value);
    }

    /// <summary>
    /// Gets or sets the desired height of the floater in device-independent pixels. Set to <see cref="double.NaN"/> to auto size.
    /// </summary>
    public double Height
    {
        get => GetValue(HeightProperty);
        set => SetValue(HeightProperty, value);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == HorizontalAlignmentProperty ||
            change.Property == WidthProperty ||
            change.Property == HeightProperty)
        {
            RaiseContentInvalidated();
        }
    }
}
