using System.Text;
using Avalonia;
using Avalonia.Controls.Documents;
using Avalonia.Media;

namespace TextFlow.Document.Documents;

/// <summary>
/// Base class for block-level elements in the flow document.
/// </summary>
public abstract class Block : TextElement
{
    public static readonly StyledProperty<Thickness> MarginProperty =
        AvaloniaProperty.Register<Block, Thickness>(nameof(Margin), new Thickness(0, 0, 0, 12));

    public static readonly StyledProperty<TextAlignment?> TextAlignmentProperty =
        AvaloniaProperty.Register<Block, TextAlignment?>(nameof(TextAlignment));

    public static readonly StyledProperty<double?> LineHeightProperty =
        AvaloniaProperty.Register<Block, double?>(nameof(LineHeight));

    public Thickness Margin
    {
        get => GetValue(MarginProperty);
        set => SetValue(MarginProperty, value);
    }

    public TextAlignment? TextAlignment
    {
        get => GetValue(TextAlignmentProperty);
        set => SetValue(TextAlignmentProperty, value);
    }

    public double? LineHeight
    {
        get => GetValue(LineHeightProperty);
        set => SetValue(LineHeightProperty, value);
    }

    internal virtual void CollectPlainText(StringBuilder builder)
    {
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == MarginProperty ||
            change.Property == TextAlignmentProperty ||
            change.Property == LineHeightProperty)
        {
            RaiseContentInvalidated();
        }
    }
}
