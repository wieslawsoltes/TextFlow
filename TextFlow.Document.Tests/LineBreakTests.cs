using TextFlow.Document.Documents;
using Xunit;

namespace TextFlow.Document.Tests;

public class LineBreakTests
{
    [Fact]
    public void LineBreak_ProducesNewLineInPlainText()
    {
        var paragraph = new Paragraph();
        paragraph.Inlines.Add(new Run("Alpha"));
        paragraph.Inlines.Add(new LineBreak());
        paragraph.Inlines.Add(new Run("Beta"));

        var text = paragraph.GetPlainText();

        Assert.Equal("Alpha\nBeta", text);
    }
}
