using Avalonia;
using Avalonia.Controls.Documents;
using Avalonia.Metadata;

namespace TextFlow.Document.Documents;

/// <summary>
/// Represents a group of table rows.
/// </summary>
public class TableRowGroup : TextElement, ITableRowCollectionHost
{
    public static readonly StyledProperty<Thickness> MarginProperty =
        AvaloniaProperty.Register<TableRowGroup, Thickness>(nameof(Margin));

    public TableRowGroup()
    {
        Rows = new TableRowCollection(this);
    }

    public TableRowGroup(params TableRow[] rows)
        : this()
    {
        foreach (var row in rows)
        {
            Rows.Add(row);
        }
    }

    [Content]
    public TableRowCollection Rows { get; }

    public Thickness Margin
    {
        get => GetValue(MarginProperty);
        set => SetValue(MarginProperty, value);
    }

    internal Table? ParentTable { get; private set; }

    internal void AttachToTable(Table? table)
    {
        if (ParentTable == table)
        {
            AttachToDocument(table?.Document);
            return;
        }

        ParentTable = table;
        AttachToDocument(table?.Document);
    }

    FlowDocument? ITableRowCollectionHost.Document => Document;

    void ITableRowCollectionHost.NotifyRowsChanged() => ParentTable?.NotifyStructureChanged();

    protected override void OnDocumentChanged(FlowDocument? document)
    {
        base.OnDocumentChanged(document);

        foreach (var row in Rows)
        {
            row.AttachToRowGroup(document is null ? null : this);
        }
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == MarginProperty)
        {
            ParentTable?.NotifyStructureChanged();
        }
    }
}
