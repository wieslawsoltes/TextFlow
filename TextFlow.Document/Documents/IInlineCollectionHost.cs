using Avalonia.Controls.Documents;

#if AVALONIA_PUBLIC_INLINE_HOST
using Avalonia;
using Avalonia.Collections;
#endif

namespace TextFlow.Document.Documents;

#if AVALONIA_PUBLIC_INLINE_HOST
internal interface IInlineCollectionHost : IInlineHost
{
    Paragraph? Paragraph { get; }
    Span? Span { get; }
    FlowDocument? Document { get; }
    void NotifyInlineStructureChanged();

    void Invalidate();
    IAvaloniaList<Visual> VisualChildren { get; }
}
#else
internal interface IInlineCollectionHost
{
    Paragraph? Paragraph { get; }
    Span? Span { get; }
    FlowDocument? Document { get; }
    void NotifyInlineStructureChanged();
}
#endif
