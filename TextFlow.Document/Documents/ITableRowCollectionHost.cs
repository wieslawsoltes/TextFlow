namespace TextFlow.Document.Documents;

internal interface ITableRowCollectionHost
{
    FlowDocument? Document { get; }

    void NotifyRowsChanged();
}
