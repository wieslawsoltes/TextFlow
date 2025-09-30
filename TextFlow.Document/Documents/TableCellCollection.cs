using System;
using System.Collections.ObjectModel;

namespace TextFlow.Document.Documents;

public sealed class TableCellCollection : ObservableCollection<TableCell>
{
    private readonly ITableCellCollectionHost _owner;

    internal TableCellCollection(ITableCellCollectionHost owner)
    {
        _owner = owner ?? throw new ArgumentNullException(nameof(owner));
    }

    protected override void InsertItem(int index, TableCell item)
    {
        ArgumentNullException.ThrowIfNull(item);
        EnsureCellIsDetached(item);

        base.InsertItem(index, item);
        AttachCell(item);
        _owner.NotifyCellsChanged();
    }

    protected override void SetItem(int index, TableCell item)
    {
        ArgumentNullException.ThrowIfNull(item);
        EnsureCellIsDetached(item);

        var existing = this[index];
        DetachCell(existing);
        base.SetItem(index, item);
        AttachCell(item);
        _owner.NotifyCellsChanged();
    }

    protected override void RemoveItem(int index)
    {
        var existing = this[index];
        DetachCell(existing);
        base.RemoveItem(index);
        _owner.NotifyCellsChanged();
    }

    protected override void ClearItems()
    {
        foreach (var cell in this)
        {
            DetachCell(cell);
        }

        base.ClearItems();
        _owner.NotifyCellsChanged();
    }

    private void AttachCell(TableCell cell)
    {
        if (_owner is TableRow row)
        {
            cell.AttachToRow(row);
        }
        else
        {
            throw new InvalidOperationException("TableCellCollection owner must be a TableRow.");
        }
    }

    private static void DetachCell(TableCell cell)
    {
        cell.AttachToRow(null);
    }

    private static void EnsureCellIsDetached(TableCell cell)
    {
        if (cell.ParentRow is not null)
        {
            throw new InvalidOperationException("The cell already belongs to a TableRow.");
        }
    }
}
