using Avalonia.Media;

namespace TextFlow.Document.Documents;

/// <summary>
/// Bold inline span.
/// </summary>
public class Bold : Span
{
    public Bold()
    {
        FontWeight = global::Avalonia.Media.FontWeight.Bold;
    }

    public Bold(params Inline[] inlines)
        : this()
    {
        AddInlines(inlines);
    }

    public Bold(string text)
        : this()
    {
        Inlines.Add(new Run(text));
    }
}
