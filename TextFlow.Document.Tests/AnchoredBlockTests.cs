using System;
using Documents = TextFlow.Document.Documents;
using Xunit;

namespace TextFlow.Document.Tests;

public class AnchoredBlockTests
{
    [Fact]
    public void Figure_blocks_attach_to_document()
    {
        var document = new Documents.FlowDocument();
        var paragraph = new Documents.Paragraph();
        var figure = new Documents.Figure();
        var innerParagraph = new Documents.Paragraph("Inside figure");

        figure.Blocks.Add(innerParagraph);
        paragraph.Inlines.Add(figure);
        document.Blocks.Add(paragraph);

        Assert.Same(document, innerParagraph.Document);
    }

    [Fact]
    public void Mutating_figure_blocks_notifies_document()
    {
        var document = new Documents.FlowDocument();
        var paragraph = new Documents.Paragraph();
        var figure = new Documents.Figure();

        paragraph.Inlines.Add(figure);
        document.Blocks.Add(paragraph);

        var changeCount = RecordChanges(document, () =>
        {
            figure.Blocks.Add(new Documents.Paragraph("Hello"));
        });

        Assert.True(changeCount > 0, "Adding a block to a figure should invalidate the document.");
    }

    [Fact]
    public void Mutating_floater_blocks_notifies_document()
    {
        var document = new Documents.FlowDocument();
        var paragraph = new Documents.Paragraph();
        var floater = new Documents.Floater();

        paragraph.Inlines.Add(floater);
        document.Blocks.Add(paragraph);

        var changeCount = RecordChanges(document, () =>
        {
            floater.Blocks.Add(new Documents.Paragraph("Floating"));
        });

        Assert.True(changeCount > 0, "Adding a block to a floater should invalidate the document.");
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
