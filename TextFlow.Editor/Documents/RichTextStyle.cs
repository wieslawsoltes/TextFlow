using Avalonia.Media;

namespace TextFlow.Editor.Documents;

/// <summary>
/// Represents the formatting attributes applied to a span of rich text.
/// </summary>
public readonly record struct RichTextStyle(
    FontFamily FontFamily,
    double FontSize,
    FontWeight FontWeight,
    FontStyle FontStyle,
    bool Underline,
    IBrush? Foreground,
    IBrush? Background)
{
    public static RichTextStyle Default { get; } = new(
        FontFamily.Default,
        14,
        FontWeight.Normal,
        FontStyle.Normal,
        false,
        Brushes.Black,
        null);

    public RichTextStyle WithFontFamily(FontFamily fontFamily) => this with { FontFamily = fontFamily };

    public RichTextStyle WithFontSize(double fontSize) => this with { FontSize = fontSize };

    public RichTextStyle WithFontWeight(FontWeight fontWeight) => this with { FontWeight = fontWeight };

    public RichTextStyle WithFontStyle(FontStyle fontStyle) => this with { FontStyle = fontStyle };

    public RichTextStyle WithUnderline(bool underline) => this with { Underline = underline };

    public RichTextStyle WithForeground(IBrush? foreground) => this with { Foreground = foreground };

    public RichTextStyle WithBackground(IBrush? background) => this with { Background = background };

    public static RichTextStyle Blend(RichTextStyle primary, RichTextStyle secondary)
    {
        return new RichTextStyle(
            secondary.FontFamily,
            secondary.FontSize,
            secondary.FontWeight,
            secondary.FontStyle,
            secondary.Underline,
            secondary.Foreground ?? primary.Foreground,
            secondary.Background ?? primary.Background);
    }
}
