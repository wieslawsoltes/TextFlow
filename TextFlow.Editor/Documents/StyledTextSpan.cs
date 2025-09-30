namespace TextFlow.Editor.Documents;

internal sealed class StyledTextSpan
{
    public StyledTextSpan(string text, RichTextStyle style)
    {
        Text = text;
        Style = style;
    }

    public string Text { get; set; }

    public RichTextStyle Style { get; set; }

    public int Length => Text.Length;

    public StyledTextSpan Clone() => new(Text, Style);
}
