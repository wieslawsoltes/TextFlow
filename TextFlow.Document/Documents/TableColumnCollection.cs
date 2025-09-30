using System;
using System.Collections.ObjectModel;

namespace TextFlow.Document.Documents;

public sealed class TableColumnCollection : ObservableCollection<TableColumn>
{
    private readonly ITableColumnCollectionHost _owner;

    internal TableColumnCollection(ITableColumnCollectionHost owner)
    {
        _owner = owner ?? throw new ArgumentNullException(nameof(owner));
    }

    protected override void InsertItem(int index, TableColumn item)
    {
        ArgumentNullException.ThrowIfNull(item);
        EnsureColumnIsDetached(item);

        base.InsertItem(index, item);
        AttachColumn(item);
        _owner.NotifyColumnsChanged();
    }

    protected override void SetItem(int index, TableColumn item)
    {
        ArgumentNullException.ThrowIfNull(item);
        EnsureColumnIsDetached(item);

        var existing = this[index];
        DetachColumn(existing);
        base.SetItem(index, item);
        AttachColumn(item);
        _owner.NotifyColumnsChanged();
    }

    protected override void RemoveItem(int index)
    {
        var existing = this[index];
        DetachColumn(existing);
        base.RemoveItem(index);
        _owner.NotifyColumnsChanged();
    }

    protected override void ClearItems()
    {
        foreach (var column in this)
        {
            DetachColumn(column);
        }

        base.ClearItems();
        _owner.NotifyColumnsChanged();
    }

    private void AttachColumn(TableColumn column)
    {
        column.AttachToTable(_owner as Table);
    }

    private static void DetachColumn(TableColumn column)
    {
        column.AttachToTable(null);
    }

    private static void EnsureColumnIsDetached(TableColumn column)
    {
        if (column.ParentTable is not null)
        {
            throw new InvalidOperationException("The column already belongs to a Table.");
        }
    }
}
