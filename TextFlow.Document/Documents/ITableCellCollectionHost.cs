namespace TextFlow.Document.Documents;

internal interface ITableCellCollectionHost
{
    FlowDocument? Document { get; }

    void NotifyCellsChanged();
}
