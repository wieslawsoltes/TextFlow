using Avalonia.Media;

namespace TextFlow.Document.Documents;

/// <summary>
/// Underlined inline span.
/// </summary>
public class Underline : Span
{
    public Underline()
    {
        TextDecorations = new TextDecorationCollection(global::Avalonia.Media.TextDecorations.Underline);
    }

    public Underline(params Inline[] inlines)
        : this()
    {
        AddInlines(inlines);
    }

    public Underline(string text)
        : this()
    {
        Inlines.Add(new Run(text));
    }
}
