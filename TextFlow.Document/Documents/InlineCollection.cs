using System;
using System.Collections.ObjectModel;

namespace TextFlow.Document.Documents;

/// <summary>
/// Collection that maintains ownership semantics for <see cref="Inline"/> instances.
/// </summary>
public sealed class InlineCollection : ObservableCollection<Inline>
{
    private IInlineCollectionHost? _host;
    private FlowDocument? _attachedDocument;

    internal InlineCollection()
    {
    }

    internal InlineCollection(IInlineCollectionHost host)
    {
        _host = host ?? throw new ArgumentNullException(nameof(host));
        _attachedDocument = host.Document;
    }

    internal void AttachHost(IInlineCollectionHost? host)
    {
        var newDocument = host?.Document;

        if (_host == host && ReferenceEquals(_attachedDocument, newDocument))
        {
            return;
        }

        _host = host;

        foreach (var inline in this)
        {
            DetachInline(inline);
            AttachInline(inline);
        }

        _attachedDocument = newDocument;
        _host?.NotifyInlineStructureChanged();
    }

    protected override void InsertItem(int index, Inline item)
    {
        ArgumentNullException.ThrowIfNull(item);
        EnsureInlineIsDetached(item);

        base.InsertItem(index, item);
        AttachInline(item);
        _host?.NotifyInlineStructureChanged();
    }

    protected override void SetItem(int index, Inline item)
    {
        ArgumentNullException.ThrowIfNull(item);
        EnsureInlineIsDetached(item);

        var previous = this[index];
        DetachInline(previous);

        base.SetItem(index, item);
        AttachInline(item);
        _host?.NotifyInlineStructureChanged();
    }

    protected override void RemoveItem(int index)
    {
        var existing = this[index];
        DetachInline(existing);
        base.RemoveItem(index);
        _host?.NotifyInlineStructureChanged();
    }

    protected override void ClearItems()
    {
        foreach (var inline in this)
        {
            DetachInline(inline);
        }

        base.ClearItems();
        _host?.NotifyInlineStructureChanged();
    }

    private void AttachInline(Inline inline)
    {
        inline.AttachToHost(_host);
        inline.ContentInvalidated += OnInlineInvalidated;
    }

    private void DetachInline(Inline inline)
    {
        inline.ContentInvalidated -= OnInlineInvalidated;
        inline.AttachToHost(null);
    }

    private void OnInlineInvalidated(object? sender, EventArgs e)
    {
        _host?.NotifyInlineStructureChanged();
    }

    private static void EnsureInlineIsDetached(Inline inline)
    {
        if (inline.Host is not null)
        {
            throw new InvalidOperationException("The inline already belongs to a host.");
        }
    }
}
