using System;
using Avalonia;

namespace TextFlow.Document.Documents;

/// <summary>
/// Represents an anchored block that can be positioned relative to the content column using figure semantics.
/// </summary>
public class Figure : AnchoredBlock
{
    public static readonly StyledProperty<FigureLength> WidthProperty =
        AvaloniaProperty.Register<Figure, FigureLength>(nameof(Width), FigureLength.Auto);

    public static readonly StyledProperty<FigureLength> HeightProperty =
        AvaloniaProperty.Register<Figure, FigureLength>(nameof(Height), FigureLength.Auto);

    public static readonly StyledProperty<FigureHorizontalAnchor> HorizontalAnchorProperty =
        AvaloniaProperty.Register<Figure, FigureHorizontalAnchor>(nameof(HorizontalAnchor), FigureHorizontalAnchor.ContentLeft);

    public static readonly StyledProperty<FigureVerticalAnchor> VerticalAnchorProperty =
        AvaloniaProperty.Register<Figure, FigureVerticalAnchor>(nameof(VerticalAnchor), FigureVerticalAnchor.ParagraphTop);

    public static readonly StyledProperty<double> HorizontalOffsetProperty =
        AvaloniaProperty.Register<Figure, double>(nameof(HorizontalOffset));

    public static readonly StyledProperty<double> VerticalOffsetProperty =
        AvaloniaProperty.Register<Figure, double>(nameof(VerticalOffset));

    /// <summary>
    /// Gets or sets the desired figure width.
    /// </summary>
    public FigureLength Width
    {
        get => GetValue(WidthProperty);
        set => SetValue(WidthProperty, value);
    }

    /// <summary>
    /// Gets or sets the desired figure height.
    /// </summary>
    public FigureLength Height
    {
        get => GetValue(HeightProperty);
        set => SetValue(HeightProperty, value);
    }

    /// <summary>
    /// Gets or sets the horizontal anchor determining where the figure is placed horizontally.
    /// </summary>
    public FigureHorizontalAnchor HorizontalAnchor
    {
        get => GetValue(HorizontalAnchorProperty);
        set => SetValue(HorizontalAnchorProperty, value);
    }

    /// <summary>
    /// Gets or sets the vertical anchor determining where the figure is placed vertically.
    /// </summary>
    public FigureVerticalAnchor VerticalAnchor
    {
        get => GetValue(VerticalAnchorProperty);
        set => SetValue(VerticalAnchorProperty, value);
    }

    /// <summary>
    /// Gets or sets the horizontal offset applied after anchoring.
    /// </summary>
    public double HorizontalOffset
    {
        get => GetValue(HorizontalOffsetProperty);
        set => SetValue(HorizontalOffsetProperty, value);
    }

    /// <summary>
    /// Gets or sets the vertical offset applied after anchoring.
    /// </summary>
    public double VerticalOffset
    {
        get => GetValue(VerticalOffsetProperty);
        set => SetValue(VerticalOffsetProperty, value);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == WidthProperty ||
            change.Property == HeightProperty ||
            change.Property == HorizontalAnchorProperty ||
            change.Property == VerticalAnchorProperty ||
            change.Property == HorizontalOffsetProperty ||
            change.Property == VerticalOffsetProperty)
        {
            RaiseContentInvalidated();
        }
    }
}

/// <summary>
/// Represents a logical figure length value.
/// </summary>
public readonly struct FigureLength : IEquatable<FigureLength>
{
    public FigureLength(double value, FigureUnitType unit)
    {
        if (double.IsNaN(value) || double.IsInfinity(value))
        {
            throw new ArgumentException("Value must be a finite number.", nameof(value));
        }

        if (unit != FigureUnitType.Auto && value < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(value));
        }

        Value = unit == FigureUnitType.Auto ? 0 : value;
        Unit = unit;
    }

    /// <summary>
    /// Gets the length value interpreted according to <see cref="Unit"/>.
    /// </summary>
    public double Value { get; }

    /// <summary>
    /// Gets the measurement unit for the figure length.
    /// </summary>
    public FigureUnitType Unit { get; }

    public static FigureLength Auto => new FigureLength(0, FigureUnitType.Auto);

    public static FigureLength FromPixels(double value) => new FigureLength(value, FigureUnitType.Pixel);

    public bool Equals(FigureLength other) => Value.Equals(other.Value) && Unit == other.Unit;

    public override bool Equals(object? obj) => obj is FigureLength other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(Value, (int)Unit);

    public static bool operator ==(FigureLength left, FigureLength right) => left.Equals(right);

    public static bool operator !=(FigureLength left, FigureLength right) => !left.Equals(right);

    public override string ToString() => Unit == FigureUnitType.Auto ? "Auto" : $"{Value} {Unit}";
}

/// <summary>
/// Identifies the measurement unit applied to a <see cref="FigureLength"/> value.
/// </summary>
public enum FigureUnitType
{
    Auto,
    Pixel,
    Column,
    Content,
    Page
}

/// <summary>
/// Defines horizontal anchor positions for a figure.
/// </summary>
public enum FigureHorizontalAnchor
{
    PageLeft,
    PageCenter,
    PageRight,
    ContentLeft,
    ContentCenter,
    ContentRight,
    ColumnLeft,
    ColumnCenter,
    ColumnRight
}

/// <summary>
/// Defines vertical anchor positions for a figure.
/// </summary>
public enum FigureVerticalAnchor
{
    PageTop,
    PageCenter,
    PageBottom,
    ParagraphTop,
    ParagraphCenter,
    ParagraphBottom,
    ContentTop,
    ContentCenter,
    ContentBottom
}
