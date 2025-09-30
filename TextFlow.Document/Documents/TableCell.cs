using System;
using Avalonia;
using Avalonia.Controls.Documents;
using Avalonia.Media;
using Avalonia.Metadata;

namespace TextFlow.Document.Documents;

/// <summary>
/// Represents a cell within a table row.
/// </summary>
public class TableCell : TextElement, IBlockCollectionHost
{
    public static readonly StyledProperty<Thickness> PaddingProperty =
        AvaloniaProperty.Register<TableCell, Thickness>(nameof(Padding), new Thickness(8, 4, 8, 4));

    public static readonly StyledProperty<IBrush?> BorderBrushProperty =
        AvaloniaProperty.Register<TableCell, IBrush?>(nameof(BorderBrush));

    public static readonly StyledProperty<Thickness> BorderThicknessProperty =
        AvaloniaProperty.Register<TableCell, Thickness>(nameof(BorderThickness), new Thickness(0));

    public TableCell()
    {
        Blocks = new BlockCollection(this);
    }

    public TableCell(Block block)
        : this()
    {
        ArgumentNullException.ThrowIfNull(block);
        Blocks.Add(block);
    }

    [Content]
    public BlockCollection Blocks { get; }

    public Thickness Padding
    {
        get => GetValue(PaddingProperty);
        set => SetValue(PaddingProperty, value);
    }

    public IBrush? BorderBrush
    {
        get => GetValue(BorderBrushProperty);
        set => SetValue(BorderBrushProperty, value);
    }

    public Thickness BorderThickness
    {
        get => GetValue(BorderThicknessProperty);
        set => SetValue(BorderThicknessProperty, value);
    }

    internal TableRow? ParentRow { get; private set; }

    internal void AttachToRow(TableRow? row)
    {
        if (ParentRow == row)
        {
            AttachToDocument(row?.Document);
            return;
        }

        ParentRow = row;
        AttachToDocument(row?.Document);
    }

    FlowDocument? IBlockCollectionHost.Document => Document;

    void IBlockCollectionHost.NotifyBlocksChanged()
    {
        RaiseContentInvalidated();
        ParentRow?.NotifyCellsChanged();
    }

    protected override void OnDocumentChanged(FlowDocument? document)
    {
        base.OnDocumentChanged(document);

        foreach (var block in Blocks)
        {
            block.AttachToDocument(document);
        }
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == PaddingProperty ||
            change.Property == BorderBrushProperty ||
            change.Property == BorderThicknessProperty)
        {
            RaiseContentInvalidated();
            ParentRow?.NotifyCellsChanged();
        }
    }
}
