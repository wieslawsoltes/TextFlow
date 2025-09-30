using System;
using System.Collections.ObjectModel;

namespace TextFlow.Document.Documents;

public sealed class TableRowGroupCollection : ObservableCollection<TableRowGroup>
{
    private readonly ITableRowGroupCollectionHost _owner;

    internal TableRowGroupCollection(ITableRowGroupCollectionHost owner)
    {
        _owner = owner ?? throw new ArgumentNullException(nameof(owner));
    }

    protected override void InsertItem(int index, TableRowGroup item)
    {
        ArgumentNullException.ThrowIfNull(item);
        EnsureRowGroupIsDetached(item);

        base.InsertItem(index, item);
        AttachRowGroup(item);
        _owner.NotifyRowGroupsChanged();
    }

    protected override void SetItem(int index, TableRowGroup item)
    {
        ArgumentNullException.ThrowIfNull(item);
        EnsureRowGroupIsDetached(item);

        var existing = this[index];
        DetachRowGroup(existing);
        base.SetItem(index, item);
        AttachRowGroup(item);
        _owner.NotifyRowGroupsChanged();
    }

    protected override void RemoveItem(int index)
    {
        var existing = this[index];
        DetachRowGroup(existing);
        base.RemoveItem(index);
        _owner.NotifyRowGroupsChanged();
    }

    protected override void ClearItems()
    {
        foreach (var group in this)
        {
            DetachRowGroup(group);
        }

        base.ClearItems();
        _owner.NotifyRowGroupsChanged();
    }

    private void AttachRowGroup(TableRowGroup group)
    {
        group.AttachToTable((_owner as Table) ?? throw new InvalidOperationException("Table owner expected."));
    }

    private static void DetachRowGroup(TableRowGroup group)
    {
        group.AttachToTable(null);
    }

    private static void EnsureRowGroupIsDetached(TableRowGroup group)
    {
        if (group.ParentTable is not null)
        {
            throw new InvalidOperationException("The row group already belongs to a Table.");
        }
    }
}
