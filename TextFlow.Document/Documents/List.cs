using System;
using System.Text;
using Avalonia;
using Avalonia.Metadata;

namespace TextFlow.Document.Documents;

/// <summary>
/// Represents a list of items.
/// </summary>
public class List : Block
{
    public static readonly StyledProperty<ListMarkerStyle> MarkerStyleProperty =
        AvaloniaProperty.Register<List, ListMarkerStyle>(nameof(MarkerStyle), ListMarkerStyle.Disc);

    public static readonly StyledProperty<int> StartIndexProperty =
        AvaloniaProperty.Register<List, int>(nameof(StartIndex), 1);

    public static readonly StyledProperty<double> MarkerOffsetProperty =
        AvaloniaProperty.Register<List, double>(nameof(MarkerOffset), 24);

    public List()
    {
        ListItems = new ListItemCollection(this);
    }

    [Content]
    public ListItemCollection ListItems { get; }

    public ListMarkerStyle MarkerStyle
    {
        get => GetValue(MarkerStyleProperty);
        set => SetValue(MarkerStyleProperty, value);
    }

    public int StartIndex
    {
        get => GetValue(StartIndexProperty);
        set => SetValue(StartIndexProperty, value);
    }

    public double MarkerOffset
    {
        get => GetValue(MarkerOffsetProperty);
        set => SetValue(MarkerOffsetProperty, value);
    }

    internal override void CollectPlainText(StringBuilder builder)
    {
        int index = Math.Max(1, StartIndex);
        foreach (var item in ListItems)
        {
            builder.Append(MarkerStyle == ListMarkerStyle.Decimal ? $"{index}. " : "• ");
            item.CollectPlainText(builder);
            builder.AppendLine();
            index++;
        }
    }

    internal void NotifyItemsChanged() => RaiseContentInvalidated();

    protected override void OnDocumentChanged(FlowDocument? document)
    {
        base.OnDocumentChanged(document);

        foreach (var item in ListItems)
        {
            item.AttachToList(document is null ? null : this);
        }
    }
}
