using Avalonia.Media;

namespace TextFlow.Document.Documents;

/// <summary>
/// Italic inline span.
/// </summary>
public class Italic : Span
{
    public Italic()
    {
        FontStyle = global::Avalonia.Media.FontStyle.Italic;
    }

    public Italic(params Inline[] inlines)
        : this()
    {
        AddInlines(inlines);
    }

    public Italic(string text)
        : this()
    {
        Inlines.Add(new Run(text));
    }
}
