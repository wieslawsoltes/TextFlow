using System;
using Avalonia;
using Avalonia.Media;

namespace TextFlow.Document.Documents;

/// <summary>
/// Shared base for flow document elements that augments Avalonia's <see cref="Avalonia.Controls.Documents.TextElement"/>
/// with document tracking and invalidation semantics required by <see cref="FlowDocumentView"/>.
/// </summary>
public abstract class TextElement : global::Avalonia.Controls.Documents.TextElement
{
    internal event EventHandler? ContentInvalidated;

    internal FlowDocument? Document { get; private set; }

    public new FontFamily? FontFamily
    {
        get => IsSet(global::Avalonia.Controls.Documents.TextElement.FontFamilyProperty)
            ? base.FontFamily
            : null;
        set
        {
            if (value is not null)
            {
                base.FontFamily = value;
            }
            else
            {
                ClearValue(global::Avalonia.Controls.Documents.TextElement.FontFamilyProperty);
            }
        }
    }

    public new double? FontSize
    {
        get => IsSet(global::Avalonia.Controls.Documents.TextElement.FontSizeProperty)
            ? base.FontSize
            : null;
        set
        {
            if (value.HasValue)
            {
                base.FontSize = value.Value;
            }
            else
            {
                ClearValue(global::Avalonia.Controls.Documents.TextElement.FontSizeProperty);
            }
        }
    }

    public new FontStyle? FontStyle
    {
        get => IsSet(global::Avalonia.Controls.Documents.TextElement.FontStyleProperty)
            ? base.FontStyle
            : null;
        set
        {
            if (value.HasValue)
            {
                base.FontStyle = value.Value;
            }
            else
            {
                ClearValue(global::Avalonia.Controls.Documents.TextElement.FontStyleProperty);
            }
        }
    }

    public new FontWeight? FontWeight
    {
        get => IsSet(global::Avalonia.Controls.Documents.TextElement.FontWeightProperty)
            ? base.FontWeight
            : null;
        set
        {
            if (value.HasValue)
            {
                base.FontWeight = value.Value;
            }
            else
            {
                ClearValue(global::Avalonia.Controls.Documents.TextElement.FontWeightProperty);
            }
        }
    }

    public new FontStretch? FontStretch
    {
        get => IsSet(global::Avalonia.Controls.Documents.TextElement.FontStretchProperty)
            ? base.FontStretch
            : null;
        set
        {
            if (value.HasValue)
            {
                base.FontStretch = value.Value;
            }
            else
            {
                ClearValue(global::Avalonia.Controls.Documents.TextElement.FontStretchProperty);
            }
        }
    }

    public new IBrush? Foreground
    {
        get => IsSet(global::Avalonia.Controls.Documents.TextElement.ForegroundProperty)
            ? base.Foreground
            : null;
        set
        {
            if (value is not null)
            {
                base.Foreground = value;
            }
            else
            {
                ClearValue(global::Avalonia.Controls.Documents.TextElement.ForegroundProperty);
            }
        }
    }

    public new IBrush? Background
    {
        get => IsSet(global::Avalonia.Controls.Documents.TextElement.BackgroundProperty)
            ? base.Background
            : null;
        set
        {
            if (value is not null)
            {
                base.Background = value;
            }
            else
            {
                ClearValue(global::Avalonia.Controls.Documents.TextElement.BackgroundProperty);
            }
        }
    }

    /// <summary>
    /// Mirrors WPF's API surface and allows markup authors to assign text decorations on block elements.
    /// </summary>
    public TextDecorationCollection? TextDecorations
    {
    get => this.GetValue(global::Avalonia.Controls.Documents.Inline.TextDecorationsProperty);
    set => this.SetValue(global::Avalonia.Controls.Documents.Inline.TextDecorationsProperty, value);
    }

    internal void AttachToDocument(FlowDocument? document)
    {
        if (Document == document)
        {
            return;
        }

        Document = document;
        OnDocumentChanged(document);
        RaiseContentInvalidated();
    }

    protected virtual void OnDocumentChanged(FlowDocument? document)
    {
    }

    protected void RaiseContentInvalidated()
    {
        ContentInvalidated?.Invoke(this, EventArgs.Empty);
        Document?.NotifyChanged();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == FontFamilyProperty ||
            change.Property == FontSizeProperty ||
            change.Property == FontStyleProperty ||
            change.Property == FontWeightProperty ||
            change.Property == FontStretchProperty ||
            change.Property == ForegroundProperty ||
            change.Property == BackgroundProperty ||
            change.Property == global::Avalonia.Controls.Documents.Inline.TextDecorationsProperty)
        {
            RaiseContentInvalidated();
        }
    }
}
