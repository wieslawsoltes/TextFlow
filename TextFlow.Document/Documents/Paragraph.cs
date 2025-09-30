using System;
using System.Text;
using Avalonia.Controls.Documents;
using Avalonia.Metadata;
#if AVALONIA_PUBLIC_INLINE_HOST
using Avalonia;
using Avalonia.Collections;
#endif

namespace TextFlow.Document.Documents;

/// <summary>
/// Represents a block of inline content.
/// </summary>
public class Paragraph : Block, IInlineCollectionHost, IAddChild<string>
{
#if AVALONIA_PUBLIC_INLINE_HOST
    private readonly AvaloniaList<Visual> _visualChildren = new();
#endif
    public Paragraph()
    {
        Inlines = new InlineCollection(this);
    }

    public Paragraph(string text)
        : this()
    {
        ArgumentNullException.ThrowIfNull(text);
        Inlines.Add(new Run(text));
    }

    public InlineCollection Inlines { get; }

    internal override void CollectPlainText(StringBuilder builder)
    {
        foreach (var inline in Inlines)
        {
            inline.AppendPlainText(builder);
        }
    }

    internal string GetPlainText()
    {
        var builder = new StringBuilder();
        CollectPlainText(builder);
        return builder.ToString();
    }

    internal void NotifyInlineStructureChanged()
    {
        RaiseContentInvalidated();
    }

    protected override void OnDocumentChanged(FlowDocument? document)
    {
        base.OnDocumentChanged(document);

        Inlines.AttachHost(document is null ? null : this);
    }

    Paragraph? IInlineCollectionHost.Paragraph => this;

    Span? IInlineCollectionHost.Span => null;

    FlowDocument? IInlineCollectionHost.Document => Document;

    void IInlineCollectionHost.NotifyInlineStructureChanged() => NotifyInlineStructureChanged();
#if AVALONIA_PUBLIC_INLINE_HOST
    void IInlineCollectionHost.Invalidate() => NotifyInlineStructureChanged();

    IAvaloniaList<Visual> IInlineCollectionHost.VisualChildren => _visualChildren;
#endif
    public void AddChild(string child)
    {
        Inlines.Add(new Run(child));
    }
}
