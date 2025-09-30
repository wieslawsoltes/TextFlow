using System;
using System.Collections.ObjectModel;

namespace TextFlow.Document.Documents;

public sealed class BlockCollection : ObservableCollection<Block>
{
    private readonly IBlockCollectionHost _owner;

    internal BlockCollection(IBlockCollectionHost owner)
    {
        _owner = owner ?? throw new ArgumentNullException(nameof(owner));
    }

    protected override void InsertItem(int index, Block item)
    {
        ArgumentNullException.ThrowIfNull(item);
        EnsureBlockIsDetached(item);

        base.InsertItem(index, item);
        AttachBlock(item);
        _owner.NotifyBlocksChanged();
    }

    protected override void SetItem(int index, Block item)
    {
        ArgumentNullException.ThrowIfNull(item);
        EnsureBlockIsDetached(item);

        var old = this[index];
        DetachBlock(old);
        base.SetItem(index, item);
        AttachBlock(item);
        _owner.NotifyBlocksChanged();
    }

    protected override void RemoveItem(int index)
    {
        var existing = this[index];
        DetachBlock(existing);
        base.RemoveItem(index);
        _owner.NotifyBlocksChanged();
    }

    protected override void ClearItems()
    {
        foreach (var block in this)
        {
            DetachBlock(block);
        }

        base.ClearItems();
        _owner.NotifyBlocksChanged();
    }

    private void AttachBlock(Block block)
    {
        block.AttachToDocument(_owner.Document);
        block.ContentInvalidated += OnBlockContentInvalidated;
    }

    private void DetachBlock(Block block)
    {
        block.ContentInvalidated -= OnBlockContentInvalidated;
        block.AttachToDocument(null);
    }

    private void OnBlockContentInvalidated(object? sender, EventArgs e)
    {
        _owner.NotifyBlocksChanged();
    }

    private static void EnsureBlockIsDetached(Block block)
    {
        if (block.Document is not null)
        {
            throw new InvalidOperationException("The block already belongs to a FlowDocument.");
        }
    }
}
