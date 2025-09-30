using System.Text;

namespace TextFlow.Document.Documents;

/// <summary>
/// Represents an explicit line break within a <see cref="Paragraph"/>.
/// </summary>
public sealed class LineBreak : Inline
{
    internal override void AppendPlainText(StringBuilder builder)
    {
        builder.Append('\n');
    }
}
