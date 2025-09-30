namespace TextFlow.Document.Documents;

internal interface IBlockCollectionHost
{
    FlowDocument? Document { get; }
    void NotifyBlocksChanged();
}
