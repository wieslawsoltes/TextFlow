using System.Text;

namespace TextFlow.Document.Documents;

/// <summary>
/// Logical inline element hosted inside a <see cref="Paragraph"/>.
/// </summary>
public abstract class Inline : TextElement
{
    private IInlineCollectionHost? _host;

    internal IInlineCollectionHost? Host => _host;

    internal void AttachToHost(IInlineCollectionHost? host)
    {
        if (_host == host)
        {
            return;
        }

        _host = host;
        AttachToDocument(host?.Document);
    }

    internal virtual void AppendPlainText(StringBuilder builder)
    {
    }
}
