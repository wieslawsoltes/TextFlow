using Avalonia.Media;
using TextFlow.Editor.Controls;
using TextFlow.Editor.Documents;

namespace TextFlow.Editor.Tests;

public class TextFlowEditorTests
{
    [Fact]
    public void Document_InsertText_AppendsContent()
    {
        var document = new RichTextDocument();

        document.InsertText(0, "Hello", RichTextStyle.Default);

        Assert.Equal("Hello", document.Text);
        Assert.Equal(5, document.Length);
    }

    [Fact]
    public void Document_InsertText_HandlesNewLines()
    {
        var document = new RichTextDocument();

        document.InsertText(0, "Hello\r\nWorld", RichTextStyle.Default);

        Assert.Equal("Hello\nWorld", document.Text);
        Assert.Equal(11, document.Length);
    }

    [Fact]
    public void Document_DeleteRange_RemovesRequestedSegment()
    {
        var document = new RichTextDocument();
        document.InsertText(0, "Avalonia", RichTextStyle.Default);

        document.DeleteRange(1, 3); // remove "val"

        Assert.Equal("Aonia", document.Text);
        Assert.Equal(5, document.Length);
    }

    [Fact]
    public void Document_ApplyStyle_UpdatesSpanProperties()
    {
        var document = new RichTextDocument();
        document.InsertText(0, "RichText", RichTextStyle.Default);

        document.ApplyStyle(4, 4, style => style.WithFontWeight(FontWeight.Bold));

        var unaffectedStyle = document.GetStyleAtOffset(1);
        var styled = document.GetStyleAtOffset(5);

        Assert.Equal(FontWeight.Normal, unaffectedStyle.FontWeight);
        Assert.Equal(FontWeight.Bold, styled.FontWeight);
    }

    [Fact]
    public void RichTextBox_AppendText_UpdatesDocumentAndCaret()
    {
        var richTextBox = new RichTextBox();

        richTextBox.AppendText("Sample");

        Assert.Equal("Sample", richTextBox.Document.Text);
        Assert.Equal(6, richTextBox.CaretOffset);
        Assert.Equal(0, richTextBox.SelectionLength);
    }

    [Fact]
    public void RichTextBox_SelectAll_SelectsEntireDocument()
    {
        var richTextBox = new RichTextBox();
        richTextBox.AppendText("Presentation");

        richTextBox.SelectAll();

        Assert.Equal(0, richTextBox.SelectionStart);
        Assert.Equal(richTextBox.Document.Length, richTextBox.SelectionEnd);
        Assert.Equal(richTextBox.Document.Length, richTextBox.CaretOffset);
    }

    [Fact]
    public void Document_CreateSnapshot_OnlyIndentedParagraphGetsMargin()
    {
        var document = new RichTextDocument();
        document.SetText("First\nSecond");

        var secondLineStart = document.Text.IndexOf('\n') + 1;
        document.InsertText(secondLineStart, "    ", RichTextStyle.Default);

        var snapshot = document.CreateFlowSnapshot(TextAlignment.Left);

        Assert.Equal(2, snapshot.Paragraphs.Count);
        Assert.Equal(0, snapshot.Paragraphs[0].Paragraph.Margin.Left);
        Assert.True(snapshot.Paragraphs[1].Paragraph.Margin.Left > 0);
    }

    [Fact]
    public void Document_CreateSnapshot_PreservesTrailingEmptyParagraph()
    {
        var document = new RichTextDocument();
        document.SetText("Hello\n");

        var snapshot = document.CreateFlowSnapshot(TextAlignment.Left);

        Assert.Equal(2, snapshot.Paragraphs.Count);

        var trailing = snapshot.Paragraphs[1];
        Assert.Equal(document.Length, trailing.DocumentStart);
        Assert.Equal(0, trailing.DocumentLength);

        var caretParagraph = snapshot.FindParagraphByDocumentOffset(document.Length);
        Assert.NotNull(caretParagraph);
        Assert.Equal(trailing.DocumentStart, caretParagraph!.DocumentStart);
        Assert.Equal(snapshot.FlowLength, snapshot.ToFlowOffset(document.Length));
    }

    [Fact]
    public void Document_CreateSnapshot_HandlesSingleNewLineDocument()
    {
        var document = new RichTextDocument();
        document.SetText("\n");

        var snapshot = document.CreateFlowSnapshot(TextAlignment.Left);

        Assert.Equal(2, snapshot.Paragraphs.Count);
        Assert.Equal(0, snapshot.Paragraphs[0].DocumentLength);
        Assert.Equal(document.Length, snapshot.Paragraphs[1].DocumentStart);
    }

    [Fact]
    public void RichTextBox_IncreaseIndentation_IndentsSelectedLinesOnly()
    {
        var richTextBox = new RichTextBox();
        richTextBox.Text = "Alpha\nBravo\nCharlie";

        var secondLineStart = richTextBox.Text.IndexOf('\n') + 1;
        richTextBox.Select(secondLineStart, 0);

        richTextBox.IncreaseIndentation();

        var expected = "Alpha\n    Bravo\nCharlie";
        Assert.Equal(expected, richTextBox.Document.Text);

        var snapshot = richTextBox.Document.CreateFlowSnapshot(TextAlignment.Left);
        Assert.Equal(3, snapshot.Paragraphs.Count);
        Assert.Equal(0, snapshot.Paragraphs[0].Paragraph.Margin.Left);
        Assert.True(snapshot.Paragraphs[1].Paragraph.Margin.Left > 0);
        Assert.Equal(0, snapshot.Paragraphs[2].Paragraph.Margin.Left);
    }

    [Fact]
    public void Document_SetParagraphAlignment_IsIdempotent()
    {
        var document = new RichTextDocument();
        document.SetText("One\nTwo");

        var secondLineStart = document.Text.IndexOf('\n') + 1;
        var secondLineLength = document.Text.Length - secondLineStart;

        document.SetParagraphAlignment(new[] { (secondLineStart, secondLineLength) }, TextAlignment.Center);
        var snapshotBefore = document.CreateFlowSnapshot(TextAlignment.Left);

        Assert.Equal(TextAlignment.Left, snapshotBefore.Paragraphs[0].Paragraph.TextAlignment ?? TextAlignment.Left);
        Assert.Equal(TextAlignment.Center, snapshotBefore.Paragraphs[1].Paragraph.TextAlignment);

        document.SetParagraphAlignment(new[] { (secondLineStart, secondLineLength) }, TextAlignment.Center);
        var snapshotAfter = document.CreateFlowSnapshot(TextAlignment.Left);

        Assert.Equal(TextAlignment.Center, snapshotAfter.Paragraphs[1].Paragraph.TextAlignment);
    }

    [Fact]
    public void Document_ClearParagraphAlignment_RevertsToDefault()
    {
        var document = new RichTextDocument();
        document.SetText("Left\nRight");

        var secondLineStart = document.Text.IndexOf('\n') + 1;
        var secondLineLength = document.Text.Length - secondLineStart;

        document.SetParagraphAlignment(new[] { (secondLineStart, secondLineLength) }, TextAlignment.Center);
        document.ClearParagraphAlignment(new[] { (secondLineStart, secondLineLength) });

        var snapshot = document.CreateFlowSnapshot(TextAlignment.Left);

        Assert.Equal(TextAlignment.Left, snapshot.Paragraphs[1].Paragraph.TextAlignment ?? TextAlignment.Left);
    }
}
