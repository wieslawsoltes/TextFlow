using System;
using Avalonia;
using Avalonia.Media;
using Avalonia.Metadata;
using Avalonia.Styling;

namespace TextFlow.Document.Documents;

/// <summary>
/// Represents a table block within a flow document.
/// </summary>
public class Table : Block, ITableRowGroupCollectionHost, ITableColumnCollectionHost
{
    public static readonly StyledProperty<double> CellSpacingProperty =
        AvaloniaProperty.Register<Table, double>(nameof(CellSpacing), 6);

    public static readonly StyledProperty<IBrush?> GridLinesBrushProperty =
        AvaloniaProperty.Register<Table, IBrush?>(nameof(GridLinesBrush), Brushes.LightGray);

    public static readonly StyledProperty<double> GridLinesThicknessProperty =
        AvaloniaProperty.Register<Table, double>(nameof(GridLinesThickness), 1);

    public Table()
    {
        Columns = new TableColumnCollection(this);
        RowGroups = new TableRowGroupCollection(this);
    }

    public TableColumnCollection Columns { get; }

    [Content]
    public TableRowGroupCollection RowGroups { get; }

    public double CellSpacing
    {
        get => GetValue(CellSpacingProperty);
        set => SetValue(CellSpacingProperty, value);
    }

    public IBrush? GridLinesBrush
    {
        get => GetValue(GridLinesBrushProperty);
        set => SetValue(GridLinesBrushProperty, value);
    }

    public double GridLinesThickness
    {
        get => GetValue(GridLinesThicknessProperty);
        set => SetValue(GridLinesThicknessProperty, value);
    }

    internal void NotifyStructureChanged() => RaiseContentInvalidated();

    FlowDocument? ITableRowGroupCollectionHost.Document => Document;

    FlowDocument? ITableColumnCollectionHost.Document => Document;

    void ITableRowGroupCollectionHost.NotifyRowGroupsChanged() => NotifyStructureChanged();

    void ITableColumnCollectionHost.NotifyColumnsChanged() => NotifyStructureChanged();

    protected override void OnDocumentChanged(FlowDocument? document)
    {
        base.OnDocumentChanged(document);

        foreach (var column in Columns)
        {
            column.AttachToTable(document is null ? null : this);
        }

        foreach (var group in RowGroups)
        {
            group.AttachToTable(document is null ? null : this);
        }
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == CellSpacingProperty ||
            change.Property == GridLinesBrushProperty ||
            change.Property == GridLinesThicknessProperty)
        {
            RaiseContentInvalidated();
        }
    }
}
