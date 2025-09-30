namespace TextFlow.Document.Documents;

internal interface ITableColumnCollectionHost
{
    FlowDocument? Document { get; }

    void NotifyColumnsChanged();
}
