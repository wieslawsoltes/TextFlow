using System;
using System.Text;
using Avalonia.Metadata;

namespace TextFlow.Document.Documents;

/// <summary>
/// Represents a group of blocks.
/// </summary>
public class Section : Block, IBlockCollectionHost
{
    public Section()
    {
        Blocks = new BlockCollection(this);
    }

    [Content]
    public BlockCollection Blocks { get; }

    internal override void CollectPlainText(StringBuilder builder)
    {
        foreach (var block in Blocks)
        {
            block.CollectPlainText(builder);
        }
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

    void IBlockCollectionHost.NotifyBlocksChanged() => RaiseContentInvalidated();
}
