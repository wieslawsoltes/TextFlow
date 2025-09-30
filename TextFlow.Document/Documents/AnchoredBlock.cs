using System.Text;
using Avalonia;
using Avalonia.Controls.Documents;
using Avalonia.Metadata;

namespace TextFlow.Document.Documents;

/// <summary>
/// Base class for anchored block elements that live within inline content.
/// </summary>
public abstract class AnchoredBlock : Inline, IBlockCollectionHost
{
    public static readonly StyledProperty<Thickness> MarginProperty =
        AvaloniaProperty.Register<AnchoredBlock, Thickness>(nameof(Margin), new Thickness(0));

    public static readonly StyledProperty<Thickness> PaddingProperty =
        AvaloniaProperty.Register<AnchoredBlock, Thickness>(nameof(Padding), new Thickness(12, 8, 12, 8));

    protected AnchoredBlock()
    {
        Blocks = new BlockCollection(this);
    }

    /// <summary>
    /// Gets the collection of blocks hosted by the anchored block.
    /// </summary>
    [Content]
    public BlockCollection Blocks { get; }

    /// <summary>
    /// Gets or sets the margin applied around the anchored block.
    /// </summary>
    public Thickness Margin
    {
        get => GetValue(MarginProperty);
        set => SetValue(MarginProperty, value);
    }

    /// <summary>
    /// Gets or sets the padding applied inside the anchored block container.
    /// </summary>
    public Thickness Padding
    {
        get => GetValue(PaddingProperty);
        set => SetValue(PaddingProperty, value);
    }

    internal override void AppendPlainText(StringBuilder builder)
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

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == MarginProperty ||
            change.Property == PaddingProperty)
        {
            RaiseContentInvalidated();
        }
    }

    FlowDocument? IBlockCollectionHost.Document => Document;

    void IBlockCollectionHost.NotifyBlocksChanged() => RaiseContentInvalidated();
}
