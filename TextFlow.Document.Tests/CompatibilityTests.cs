using System.Linq;
using TextFlow.Document.Documents;
using Xunit;

namespace TextFlow.Document.Tests
{
    public class CompatibilityTests
    {
        [Fact]
        public void BuildDocumentLikeWpfSample()
        {
            var doc = new TextFlow.Document.Documents.FlowDocument();

            // Title paragraph
            var title = new Paragraph();
            title.Inlines.Add(new Run("Avalonia FlowDocument Sample — Comprehensive Demo") { });
            doc.Blocks.Add(title);

            // Mixed inline paragraph
            var p1 = new Paragraph();
            p1.Inlines.Add(new Run("This document demonstrates many FlowDocument features. You can mix inline formatting such as "));
            p1.Inlines.Add(new Bold());
            // Bold needs a Span with Inlines in this implementation
            var boldSpan = new Span();
            boldSpan.Inlines.Add(new Run("bold"));
            p1.Inlines.Add(boldSpan);
            p1.Inlines.Add(new Run(", "));
            var italicSpan = new Span();
            italicSpan.Inlines.Add(new Run("italic"));
            p1.Inlines.Add(italicSpan);
            doc.Blocks.Add(p1);

            // List
            var list = new List();
            var li1 = new ListItem();
            var li1p = new Paragraph();
            li1p.Inlines.Add(new Run("First numbered item with bold text."));
            li1.Blocks.Add(li1p);
            list.ListItems.Add(li1);
            doc.Blocks.Add(list);

            // Table with two columns
            var table = new Table();
            table.Columns.Add(new TableColumn());
            table.Columns.Add(new TableColumn());
            var rg = new TableRowGroup();
            var headerRow = new TableRow();
            var cellA = new TableCell();
            cellA.Blocks.Add(new Paragraph("Column A"));
            var cellB = new TableCell();
            cellB.Blocks.Add(new Paragraph("Column B"));
            headerRow.Cells.Add(cellA);
            headerRow.Cells.Add(cellB);
            rg.Rows.Add(headerRow);
            table.RowGroups.Add(rg);
            doc.Blocks.Add(table);

            // Figure - create as a Figure block if available
            var fig = new Figure();
            fig.Width = TextFlow.Document.Documents.FigureLength.FromPixels(200);
            var figPara = new Paragraph();
            figPara.Inlines.Add(new Run("Figure: BlockUIContainer not present in Avalonia sample; using Paragraph inside Figure."));
            fig.Blocks.Add(figPara);
            // Figures in this Avalonia implementation are inline anchored blocks — add to a paragraph's inlines.
            var figPara2 = new Paragraph();
            figPara2.Inlines.Add(fig);
            doc.Blocks.Add(figPara2);

            // Smoke assertions: ensure structure contains expected blocks
            Assert.True(doc.Blocks.OfType<Paragraph>().Any());
            Assert.Contains(doc.Blocks, b => b is Table);
            Assert.Contains(doc.Blocks, b => b is List);
            Assert.Contains(doc.Blocks, b => b is Paragraph p && p.Inlines.Any(i => i is AnchoredBlock));
        }
    }
}
