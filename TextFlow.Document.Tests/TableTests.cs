using System;
using Avalonia.Media;
using Documents = TextFlow.Document.Documents;
using Xunit;

namespace TextFlow.Document.Tests;

public class TableTests
{
    [Fact]
    public void Changing_cell_border_brush_notifies_document()
    {
        var document = new Documents.FlowDocument();
        var table = new Documents.Table();
        var group = new Documents.TableRowGroup();
        var row = new Documents.TableRow();
        var cell = new Documents.TableCell(new Documents.Paragraph("Hello"));

        row.Cells.Add(cell);
        group.Rows.Add(row);
        table.RowGroups.Add(group);
        document.Blocks.Add(table);

        var changeCount = RecordChanges(document, () =>
        {
            cell.BorderBrush = Brushes.Red;
        });

        Assert.True(changeCount > 0, "Updating TableCell.BorderBrush should invalidate the document.");
    }

    [Fact]
    public void Changing_table_grid_lines_notifies_document()
    {
        var document = new Documents.FlowDocument();
        var table = new Documents.Table();
        table.RowGroups.Add(new Documents.TableRowGroup());
        document.Blocks.Add(table);

        var changeCount = RecordChanges(document, () =>
        {
            table.GridLinesBrush = Brushes.Blue;
        });

        Assert.True(changeCount > 0, "Updating Table.GridLinesBrush should invalidate the document.");
    }

    private static int RecordChanges(Documents.FlowDocument document, Action action)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(action);

        var changeCount = 0;

        void Handler(object? sender, EventArgs e) => changeCount++;

        document.Changed += Handler;
        try
        {
            action();
        }
        finally
        {
            document.Changed -= Handler;
        }

        return changeCount;
    }
}
