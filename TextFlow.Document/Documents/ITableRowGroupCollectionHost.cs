namespace TextFlow.Document.Documents;

internal interface ITableRowGroupCollectionHost
{
    FlowDocument? Document { get; }

    void NotifyRowGroupsChanged();
}
