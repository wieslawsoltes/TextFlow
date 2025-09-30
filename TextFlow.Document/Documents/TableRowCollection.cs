using System;
using System.Collections.ObjectModel;

namespace TextFlow.Document.Documents;

public sealed class TableRowCollection : ObservableCollection<TableRow>
{
    private readonly ITableRowCollectionHost _owner;

    internal TableRowCollection(ITableRowCollectionHost owner)
    {
        _owner = owner ?? throw new ArgumentNullException(nameof(owner));
    }

    protected override void InsertItem(int index, TableRow item)
    {
        ArgumentNullException.ThrowIfNull(item);
        EnsureRowIsDetached(item);

        base.InsertItem(index, item);
        AttachRow(item);
        _owner.NotifyRowsChanged();
    }

    protected override void SetItem(int index, TableRow item)
    {
        ArgumentNullException.ThrowIfNull(item);
        EnsureRowIsDetached(item);

        var existing = this[index];
        DetachRow(existing);
        base.SetItem(index, item);
        AttachRow(item);
        _owner.NotifyRowsChanged();
    }

    protected override void RemoveItem(int index)
    {
        var existing = this[index];
        DetachRow(existing);
        base.RemoveItem(index);
        _owner.NotifyRowsChanged();
    }

    protected override void ClearItems()
    {
        foreach (var row in this)
        {
            DetachRow(row);
        }

        base.ClearItems();
        _owner.NotifyRowsChanged();
    }

    private void AttachRow(TableRow row)
    {
        if (_owner is TableRowGroup group)
        {
            row.AttachToRowGroup(group);
        }
        else
        {
            throw new InvalidOperationException("TableRowCollection owner must be a TableRowGroup.");
        }
    }

    private static void DetachRow(TableRow row)
    {
        row.AttachToRowGroup(null);
    }

    private static void EnsureRowIsDetached(TableRow row)
    {
        if (row.ParentRowGroup is not null)
        {
            throw new InvalidOperationException("The row already belongs to a TableRowGroup.");
        }
    }
}
