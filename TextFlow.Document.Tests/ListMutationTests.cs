using System;
using Documents = TextFlow.Document.Documents;

namespace TextFlow.Document.Tests;

public class ListMutationTests
{
    [Fact]
    public void Adding_list_item_notifies_document()
    {
        var (document, list) = CreateDocumentWithList();
        list.ListItems.Add(CreateItem("Seed"));

        var changeCount = RecordChanges(document, () =>
        {
            list.ListItems.Add(CreateItem("Second"));
        });

        Assert.True(changeCount > 0, "Adding a list item should invalidate the document.");
    }

    [Fact]
    public void Removing_list_item_notifies_document()
    {
        var (document, list) = CreateDocumentWithList();
        var item = CreateItem("A");
        list.ListItems.Add(item);

        var changeCount = RecordChanges(document, () =>
        {
            list.ListItems.Remove(item);
        });

        Assert.True(changeCount > 0, "Removing a list item should invalidate the document.");
    }

    [Fact]
    public void Replacing_list_item_notifies_document()
    {
        var (document, list) = CreateDocumentWithList();
        list.ListItems.Add(CreateItem("First"));
        list.ListItems.Add(CreateItem("Second"));

        var changeCount = RecordChanges(document, () =>
        {
            list.ListItems[0] = CreateItem("Replacement");
        });

        Assert.True(changeCount > 0, "Replacing a list item should invalidate the document.");
    }

    [Fact]
    public void Clearing_list_items_notifies_document()
    {
        var (document, list) = CreateDocumentWithList();
        list.ListItems.Add(CreateItem("First"));
        list.ListItems.Add(CreateItem("Second"));

        var changeCount = RecordChanges(document, () =>
        {
            list.ListItems.Clear();
        });

        Assert.True(changeCount > 0, "Clearing list items should invalidate the document.");
    }

    [Fact]
    public void Mutating_list_item_blocks_notifies_document()
    {
        var (document, list) = CreateDocumentWithList();
        var item = CreateItem("First");
        list.ListItems.Add(item);

        var addChangeCount = RecordChanges(document, () =>
        {
            item.Blocks.Add(new Documents.Paragraph("Extra"));
        });

        Assert.True(addChangeCount > 0, "Adding a block to a list item should invalidate the document.");

        var removeChangeCount = RecordChanges(document, () =>
        {
            item.Blocks.RemoveAt(0);
        });

        Assert.True(removeChangeCount > 0, "Removing a block from a list item should invalidate the document.");
    }

    private static (Documents.FlowDocument Document, Documents.List List) CreateDocumentWithList()
    {
        var document = new Documents.FlowDocument();
        var list = new Documents.List();
        document.Blocks.Add(list);
        return (document, list);
    }

    private static Documents.ListItem CreateItem(string text)
    {
        return new Documents.ListItem(new Documents.Paragraph(text));
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
