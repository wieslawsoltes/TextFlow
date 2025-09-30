using System.Text;
using Avalonia.Metadata;
#if AVALONIA_PUBLIC_INLINE_HOST
using Avalonia;
using Avalonia.Collections;
#endif

namespace TextFlow.Document.Documents;

/// <summary>
/// Groups a sequence of inline elements so that formatting can be applied collectively.
/// </summary>
public class Span : Inline, IInlineCollectionHost
{
#if AVALONIA_PUBLIC_INLINE_HOST
    private readonly AvaloniaList<Visual> _visualChildren = new();
#endif
    public Span()
    {
        Inlines = new InlineCollection(this);
    }

    public Span(params Inline[] inlines)
        : this()
    {
        AddInlines(inlines);
    }

    public Span(string text)
        : this()
    {
        Inlines.Add(new Run(text));
    }

    [Content]
    public InlineCollection Inlines { get; }

    internal override void AppendPlainText(StringBuilder builder)
    {
        foreach (var inline in Inlines)
        {
            inline.AppendPlainText(builder);
        }
    }

    protected override void OnDocumentChanged(FlowDocument? document)
    {
        base.OnDocumentChanged(document);
        Inlines.AttachHost(document is null ? null : this);
    }

    Paragraph? IInlineCollectionHost.Paragraph => null;

    Span? IInlineCollectionHost.Span => this;

    FlowDocument? IInlineCollectionHost.Document => Document;

    void IInlineCollectionHost.NotifyInlineStructureChanged() => RaiseContentInvalidated();

#if AVALONIA_PUBLIC_INLINE_HOST
    void IInlineCollectionHost.Invalidate() => RaiseContentInvalidated();

    IAvaloniaList<Visual> IInlineCollectionHost.VisualChildren => _visualChildren;
#endif

    protected void AddInlines(params Inline[] inlines)
    {
        if (inlines is null)
        {
            return;
        }

        foreach (var inline in inlines)
        {
            if (inline is null)
            {
                continue;
            }

            Inlines.Add(inline);
        }
    }
}
