using System;
using System.Collections.ObjectModel;

namespace TextFlow.Document.Documents;

public sealed class ListItemCollection : ObservableCollection<ListItem>
{
    private readonly List _owner;

    internal ListItemCollection(List owner)
    {
        _owner = owner ?? throw new ArgumentNullException(nameof(owner));
    }

    protected override void InsertItem(int index, ListItem item)
    {
        ArgumentNullException.ThrowIfNull(item);
        EnsureItemIsDetached(item);

        base.InsertItem(index, item);
        AttachItem(item);
        _owner.NotifyItemsChanged();
    }

    protected override void SetItem(int index, ListItem item)
    {
        ArgumentNullException.ThrowIfNull(item);
        EnsureItemIsDetached(item);

        var existing = this[index];
        DetachItem(existing);
        base.SetItem(index, item);
        AttachItem(item);
        _owner.NotifyItemsChanged();
    }

    protected override void RemoveItem(int index)
    {
        var existing = this[index];
        DetachItem(existing);
        base.RemoveItem(index);
        _owner.NotifyItemsChanged();
    }

    protected override void ClearItems()
    {
        foreach (var item in this)
        {
            DetachItem(item);
        }

        base.ClearItems();
        _owner.NotifyItemsChanged();
    }

    private void AttachItem(ListItem item)
    {
        item.AttachToList(_owner);
        item.ContentInvalidated += OnItemContentInvalidated;
    }

    private void DetachItem(ListItem item)
    {
        item.ContentInvalidated -= OnItemContentInvalidated;
        item.AttachToList(null);
    }

    private void OnItemContentInvalidated(object? sender, EventArgs e)
    {
        _owner.NotifyItemsChanged();
    }

    private static void EnsureItemIsDetached(ListItem item)
    {
        if (item.ParentList is not null)
        {
            throw new InvalidOperationException("The list item already belongs to a List.");
        }
    }
}
