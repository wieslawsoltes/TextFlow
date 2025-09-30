using Avalonia;
using Avalonia.Styling;

namespace TextFlow.Document.Documents;

/// <summary>
/// Represents metadata for a table column.
/// </summary>
public class TableColumn : StyledElement
{
    public static readonly StyledProperty<double> WidthProperty =
        AvaloniaProperty.Register<TableColumn, double>(nameof(Width), double.NaN);

    internal Table? ParentTable { get; private set; }

    public double Width
    {
        get => GetValue(WidthProperty);
        set => SetValue(WidthProperty, value);
    }

    internal void AttachToTable(Table? table)
    {
        ParentTable = table;
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == WidthProperty)
        {
            ParentTable?.NotifyStructureChanged();
        }
    }
}
