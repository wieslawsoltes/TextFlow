using Avalonia.Controls.Documents;
using Avalonia.Metadata;

namespace TextFlow.Document.Documents;

/// <summary>
/// Represents a single row within a table.
/// </summary>
public class TableRow : TextElement, ITableCellCollectionHost
{
    public TableRow()
    {
        Cells = new TableCellCollection(this);
    }

    public TableRow(params TableCell[] cells)
        : this()
    {
        foreach (var cell in cells)
        {
            Cells.Add(cell);
        }
    }

    [Content]
    public TableCellCollection Cells { get; }

    internal TableRowGroup? ParentRowGroup { get; private set; }

    internal Table? ParentTable => ParentRowGroup?.ParentTable;

    internal void AttachToRowGroup(TableRowGroup? rowGroup)
    {
        if (ParentRowGroup == rowGroup)
        {
            AttachToDocument(rowGroup?.Document);
            return;
        }

        ParentRowGroup = rowGroup;
        AttachToDocument(rowGroup?.Document);
    }

    FlowDocument? ITableCellCollectionHost.Document => Document;

    void ITableCellCollectionHost.NotifyCellsChanged() => ParentTable?.NotifyStructureChanged();

    internal void NotifyCellsChanged() => ParentTable?.NotifyStructureChanged();
    
    protected override void OnDocumentChanged(FlowDocument? document)
    {
        base.OnDocumentChanged(document);

        foreach (var cell in Cells)
        {
            cell.AttachToRow(document is null ? null : this);
        }
    }
}
