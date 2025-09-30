using System.Text;
using Avalonia.Controls.Documents;
using Avalonia.Metadata;

namespace TextFlow.Document.Documents;

/// <summary>
/// Represents an item within a <see cref="List"/>.
/// </summary>
public class ListItem : TextElement, IBlockCollectionHost
{
    internal ListItem()
    {
        Blocks = new BlockCollection(this);
    }

    public ListItem(Block block)
        : this()
    {
        ArgumentNullException.ThrowIfNull(block);
        Blocks.Add(block);
    }

    [Content]
    public BlockCollection Blocks { get; }

    internal List? ParentList { get; private set; }

    internal void CollectPlainText(StringBuilder builder)
    {
        foreach (var block in Blocks)
        {
            block.CollectPlainText(builder);
        }
    }

    internal void AttachToList(List? list)
    {
        if (ParentList == list)
        {
            AttachToDocument(list?.Document);
            return;
        }

        ParentList = list;
        AttachToDocument(list?.Document);
    }

    protected override void OnDocumentChanged(FlowDocument? document)
    {
        base.OnDocumentChanged(document);

        foreach (var block in Blocks)
        {
            block.AttachToDocument(document);
        }
    }

    FlowDocument? IBlockCollectionHost.Document => Document;

    void IBlockCollectionHost.NotifyBlocksChanged() => ParentList?.NotifyItemsChanged();
}
