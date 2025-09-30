using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Avalonia;
using Avalonia.Layout;
using Avalonia.Media;
using DocumentFlow = TextFlow.Document.Documents.FlowDocument;
using FlowParagraph = TextFlow.Document.Documents.Paragraph;
using FlowRun = TextFlow.Document.Documents.Run;
using FlowBold = TextFlow.Document.Documents.Bold;
using FlowItalic = TextFlow.Document.Documents.Italic;
using FlowSpan = TextFlow.Document.Documents.Span;
using FlowUnderline = TextFlow.Document.Documents.Underline;
using FlowSection = TextFlow.Document.Documents.Section;
using FlowList = TextFlow.Document.Documents.List;
using FlowListItem = TextFlow.Document.Documents.ListItem;
using FlowListMarkerStyle = TextFlow.Document.Documents.ListMarkerStyle;
using FlowTable = TextFlow.Document.Documents.Table;
using FlowTableCell = TextFlow.Document.Documents.TableCell;
using FlowTableColumn = TextFlow.Document.Documents.TableColumn;
using FlowTableRow = TextFlow.Document.Documents.TableRow;
using FlowTableRowGroup = TextFlow.Document.Documents.TableRowGroup;
using FlowFloater = TextFlow.Document.Documents.Floater;
using FlowFigure = TextFlow.Document.Documents.Figure;
using FlowFigureLength = TextFlow.Document.Documents.FigureLength;
using FlowFigureHorizontalAnchor = TextFlow.Document.Documents.FigureHorizontalAnchor;

namespace TextFlow.Document.Sample.ViewModels;

public sealed class MainWindowViewModel : INotifyPropertyChanged
{
    private readonly DelegateCommand _runMutationCommand;
    private IReadOnlyList<SampleDocument> _samples;
    private SampleDocument? _selectedSample;
    private DocumentFlow? _document;

    public MainWindowViewModel()
    {
        _samples = CreateSamples();
        _runMutationCommand = new DelegateCommand(OnRunMutation, () => SelectedSample?.MutateAction is not null);
        SelectedSample = _samples.Count > 0 ? _samples[0] : null;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public IReadOnlyList<SampleDocument> Samples
    {
        get => _samples;
        private set => SetProperty(ref _samples, value);
    }

    public SampleDocument? SelectedSample
    {
        get => _selectedSample;
        set
        {
            if (SetProperty(ref _selectedSample, value))
            {
                UpdateDocument();
                _runMutationCommand.RaiseCanExecuteChanged();
                OnPropertyChanged(nameof(CanMutate));
                OnPropertyChanged(nameof(Description));
            }
        }
    }

    public DocumentFlow? Document
    {
        get => _document;
        private set => SetProperty(ref _document, value);
    }

    public bool CanMutate => SelectedSample?.MutateAction is not null;

    public string Description => SelectedSample?.Description ?? string.Empty;

    public ICommand RunMutationCommand => _runMutationCommand;

    private void UpdateDocument()
    {
        Document = SelectedSample?.Factory();
    }

    private void OnRunMutation()
    {
        if (SelectedSample?.MutateAction is { } mutate && Document is { } doc)
        {
            mutate(doc);
        }
    }

    private IReadOnlyList<SampleDocument> CreateSamples()
    {
        return new[]
        {
            new SampleDocument(
                "Docs: Simple paragraph and list",
                "Replicates the first FlowDocument overview sample with a bold inline segment followed by a bulleted list.",
                CreateDocsSimpleFlowDocument,
                null),
            new SampleDocument(
                "Docs: Section with shared background",
                "Demonstrates the article's Section example where a parent block applies background to its child paragraphs.",
                CreateDocsSectionExample,
                null),
            new SampleDocument(
                "Docs: Block UI container (placeholder)",
                "Shows the intended layout from the BlockUIContainer example and documents the missing element support.",
                CreateDocsBlockUiContainerPlaceholder,
                null),
            new SampleDocument(
                "Docs: Span formatting",
                "Mirrors the Span sample with nested bold text and notes where InlineUIContainer support will light up.",
                CreateDocsSpanExample,
                null),
            new SampleDocument(
                "Docs: Inline UI container (placeholder)",
                "Captures the inline button example text while pointing to the commented code awaiting InlineUIContainer support.",
                CreateDocsInlineUiContainerPlaceholder,
                null),
            new SampleDocument(
                "Docs: Anchored blocks (figure & floater)",
                "Demonstrates anchored blocks hosting nested content inline via the new Figure and Floater elements.",
                CreateDocsAnchoredBlocksSample,
                null),
            new SampleDocument(
                "Docs: Table quick start",
                "Builds a compact reference table with headers, zebra striping, and inline styling cues.",
                CreateDocsTableQuickStart,
                null),
            new SampleDocument(
                "Docs: Line break formatting (placeholder)",
                "Explains the LineBreak example and displays the text in stacked paragraphs until the inline element exists.",
                CreateDocsLineBreakPlaceholder,
                null),
            new SampleDocument(
                "Docs: Typography variations (placeholder)",
                "Documents the typography sample and preserves the target code so it can be enabled once typography APIs land.",
                CreateDocsTypographyPlaceholder,
                null),
            new SampleDocument(
                "Welcome overview",
                "Introductory content that mirrors the original FlowDocument sample and now showcases inline spans plus the new Section/List block support.",
                CreateIntroDocument,
                null),
            new SampleDocument(
                "Activity log",
                "Start with a document header and append timestamped entries using the action button.",
                CreateLogDocument,
                AppendLogEntry),
            new SampleDocument(
                "Parity matrix",
                "Explore a feature table that tracks FlowDocument parity work, including custom column widths, spacing, and zebra rows.",
                CreateParityMatrixDocument,
                null),
            new SampleDocument(
                "Weekend edition layout",
                "Mimics a newspaper front page with columns, anchored figures, and callout tables to showcase richer layouts.",
                CreateWeekendEditionDocument,
                null)
        };
    }

    private static DocumentFlow CreateDocsSimpleFlowDocument()
    {
        var document = CreateDocsScaffold();

        var paragraph = new FlowParagraph
        {
            Margin = new Thickness(0, 0, 0, 18)
        };
        paragraph.Inlines.Add(new FlowBold(new FlowRun("Some bold text in the paragraph.")));
        paragraph.Inlines.Add(new FlowRun(" Some text that is not bold."));
        document.Blocks.Add(paragraph);

        var list = new FlowList
        {
            Margin = new Thickness(12, 0, 0, 0)
        };
        list.ListItems.Add(new FlowListItem(new FlowParagraph("ListItem 1")));
        list.ListItems.Add(new FlowListItem(new FlowParagraph("ListItem 2")));
        list.ListItems.Add(new FlowListItem(new FlowParagraph("ListItem 3")));
        document.Blocks.Add(list);

        return document;
    }

    private static DocumentFlow CreateDocsSectionExample()
    {
        var document = CreateDocsScaffold();

        var section = new FlowSection
        {
            Background = new SolidColorBrush(Color.Parse("#FFFFCDD2")),
            Margin = new Thickness(0, 0, 0, 12)
        };

        section.Blocks.Add(new FlowParagraph("Paragraph 1") { Margin = new Thickness(12, 6, 12, 6) });
        section.Blocks.Add(new FlowParagraph("Paragraph 2") { Margin = new Thickness(12, 6, 12, 6) });
        section.Blocks.Add(new FlowParagraph("Paragraph 3") { Margin = new Thickness(12, 6, 12, 6) });
        document.Blocks.Add(section);

        var note = new FlowParagraph
        {
            Margin = new Thickness(0, 4, 0, 0)
        };
        note.Inlines.Add(new FlowItalic("In WPF this section shows a red background shared across its child paragraphs."));
        document.Blocks.Add(note);

        return document;
    }

    private static DocumentFlow CreateDocsAnchoredBlocksSample()
    {
        var document = CreateDocsScaffold();

        var intro = new FlowParagraph
        {
            Margin = new Thickness(0, 0, 0, 12)
        };
        intro.Inlines.Add(new FlowBold(new FlowRun("Anchored blocks have arrived:")));
        intro.Inlines.Add(new FlowRun(" Figures and floaters now host full block content inside a paragraph."));
        document.Blocks.Add(intro);

        var paragraph = new FlowParagraph
        {
            Margin = new Thickness(0, 0, 0, 12)
        };
        paragraph.Inlines.Add(new FlowRun("This paragraph inlines a figure, adds explanatory text, and then concludes with a floater."));

        var figure = new FlowFigure
        {
            Width = FlowFigureLength.FromPixels(260),
            Margin = new Thickness(0, 10, 24, 12),
            Padding = new Thickness(16, 12, 16, 12),
            Background = new SolidColorBrush(Color.Parse("#FFE8F5E9")),
            HorizontalAnchor = FlowFigureHorizontalAnchor.ContentLeft
        };
        figure.Blocks.Add(new FlowParagraph("Figures act like mini sections. They can stack multiple blocks, inherit typography, and apply their own styling.")
        {
            Margin = new Thickness(0)
        });
        figure.Blocks.Add(new FlowParagraph("Try editing the code to add lists or tables inside the figure—the renderer recalculates instantly.")
        {
            Margin = new Thickness(0, 6, 0, 0)
        });
        paragraph.Inlines.Add(figure);

        paragraph.Inlines.Add(new FlowRun(" After the figure, regular text continues, sharing the same paragraph metrics."));

        var floater = new FlowFloater
        {
            Width = 220,
            Margin = new Thickness(24, 4, 0, 12),
            Padding = new Thickness(16, 10, 16, 10),
            Background = new SolidColorBrush(Color.Parse("#FFFFF3E0")),
            HorizontalAlignment = HorizontalAlignment.Right
        };
        floater.Blocks.Add(new FlowParagraph("Floaters align to the requested edge. In this milestone they render inline while groundwork for true text wrapping evolves.")
        {
            Margin = new Thickness(0)
        });
        paragraph.Inlines.Add(floater);

        paragraph.Inlines.Add(new FlowRun(" The closing sentence shows that anchored blocks participate in the document flow just like any other inline content."));
        document.Blocks.Add(paragraph);

        var note = new FlowParagraph
        {
            Margin = new Thickness(0, 0, 0, 0),
            FontSize = 13,
            Foreground = new SolidColorBrush(Color.Parse("#FF5D4037"))
        };
        note.Inlines.Add(new FlowItalic("Tip: Modify the Blocks inside the figure or floater to see live re-rendering in the view."));
        document.Blocks.Add(note);

        return document;
    }

    private static DocumentFlow CreateDocsTableQuickStart()
    {
        var document = CreateDocsScaffold();

        var title = new FlowParagraph("FlowDocument table quick start")
        {
            Margin = new Thickness(0, 0, 0, 12),
            FontWeight = FontWeight.SemiBold,
            FontSize = 20
        };
        document.Blocks.Add(title);

        var intro = new FlowParagraph
        {
            Margin = new Thickness(0, 0, 0, 12)
        };
        intro.Inlines.Add(new FlowRun("Tables support headers, body groups, column sizing, and cell-level styling. The grid below mirrors the quick start snippet from the docs."));
        document.Blocks.Add(intro);

        var table = new FlowTable
        {
            CellSpacing = 6,
            Margin = new Thickness(0, 0, 0, 16),
            Background = new SolidColorBrush(Color.Parse("#FFF8F9FF")),
            GridLinesBrush = new SolidColorBrush(Color.Parse("#FFB0BEC5")),
            GridLinesThickness = 1
        };

        table.Columns.Add(new FlowTableColumn { Width = 170 });
        table.Columns.Add(new FlowTableColumn());

        var headerGroup = new FlowTableRowGroup();
        headerGroup.Rows.Add(new FlowTableRow(
            CreateHeaderCell("Feature"),
            CreateHeaderCell("Details")));
        table.RowGroups.Add(headerGroup);

        var bodyGroup = new FlowTableRowGroup();
        bodyGroup.Rows.Add(new FlowTableRow(
            CreateBodyCell("Figure"),
            CreateBodyCell("Hosts block content inline with configurable width, background, and anchoring.")));
        bodyGroup.Rows.Add(new FlowTableRow(
            CreateBodyCell("Floater", zebra: true),
            CreateBodyCell("Aligns anchored content to a chosen edge. Future work will layer in text wrapping.", zebra: true)));
        bodyGroup.Rows.Add(new FlowTableRow(
            CreateBodyCell("Table", highlight: true),
            CreateBodyCell("Supports headers, zebra stripes, and arbitrary block content in each cell.", highlight: true)));
        table.RowGroups.Add(bodyGroup);

        document.Blocks.Add(table);

        var outro = new FlowParagraph
        {
            Margin = new Thickness(0, 0, 0, 0)
        };
        outro.Inlines.Add(new FlowRun("Cells are live: adjust padding, add lists, or nest figures directly inside cells to see composite layouts."));
        document.Blocks.Add(outro);

        return document;

        static FlowTableCell CreateHeaderCell(string text)
        {
            var paragraph = new FlowParagraph(text)
            {
                Margin = new Thickness(0),
                FontWeight = FontWeight.SemiBold,
                TextAlignment = TextAlignment.Left
            };

            return new FlowTableCell(paragraph)
            {
                Padding = new Thickness(14, 10, 14, 10),
                Background = new SolidColorBrush(Color.Parse("#FFE0E7FF")),
                BorderBrush = new SolidColorBrush(Color.Parse("#FF90A4AE")),
                BorderThickness = new Thickness(1)
            };
        }

        static FlowTableCell CreateBodyCell(string text, bool zebra = false, bool highlight = false)
        {
            var paragraph = new FlowParagraph
            {
                Margin = new Thickness(0)
            };
            paragraph.Inlines.Add(new FlowRun(text));

            var background = highlight
                ? new SolidColorBrush(Color.Parse("#FFFFF5E6"))
                : zebra
                    ? new SolidColorBrush(Color.Parse("#FFF0F4FF"))
                    : null;

            return new FlowTableCell(paragraph)
            {
                Padding = new Thickness(12, 8, 12, 8),
                Background = background,
                BorderBrush = new SolidColorBrush(Color.Parse("#FFCFD8DC")),
                BorderThickness = new Thickness(1)
            };
        }
    }

    private static DocumentFlow CreateDocsBlockUiContainerPlaceholder()
    {
        var document = CreateDocsScaffold();

        var intro = new FlowParagraph
        {
            Margin = new Thickness(0, 0, 0, 12)
        };
        intro.Inlines.Add(new FlowBold(new FlowRun("BlockUIContainer pending:")));
        intro.Inlines.Add(new FlowRun(" Avalonia's FlowDocument does not yet expose a BlockUIContainer analogue."));
        document.Blocks.Add(intro);

        var guidance = new FlowParagraph
        {
            Margin = new Thickness(0, 0, 0, 12)
        };
        guidance.Inlines.Add(new FlowRun("Once available, uncomment the sample code inside the view-model to embed Avalonia UI controls such as buttons, combo boxes, and text boxes inside flow content."));
        document.Blocks.Add(guidance);

        var markupHeader = new FlowParagraph
        {
            Margin = new Thickness(0, 0, 0, 4),
            FontWeight = FontWeight.SemiBold
        };
        markupHeader.Inlines.Add(new FlowRun("Commented port of the WPF example:"));
        document.Blocks.Add(markupHeader);

        var markupLines = new[]
        {
            "var uiSection = new Section { Background = Brushes.GhostWhite };",
            "uiSection.Blocks.Add(new Paragraph(\"A UIElement element may be embedded...\"));",
            "// BlockUIContainer support will let us host Avalonia controls:",
            "// uiSection.Blocks.Add(new BlockUIContainer { Child = new Button { Content = \"Click me!\" } });",
            "// ...additional controls omitted for brevity"
        };

        foreach (var line in markupLines)
        {
            document.Blocks.Add(new FlowParagraph(line)
            {
                FontFamily = FontFamily.Default,
                FontSize = 13,
                Margin = new Thickness(6, 0, 0, 0)
            });
        }

        return document;

        /*
        // Future implementation when BlockUIContainer arrives:
        var uiSection = new FlowSection { Background = Brushes.GhostWhite };
        uiSection.Blocks.Add(new FlowParagraph(
            "A UIElement element may be embedded directly in flow content by enclosing it in a BlockUIContainer element."));
        uiSection.Blocks.Add(new BlockUIContainer
        {
            Child = new Button { Content = "Click me!" }
        });
        document.Blocks.Add(uiSection);
        */
    }

    private static DocumentFlow CreateDocsSpanExample()
    {
        var document = CreateDocsScaffold();

        var paragraph = new FlowParagraph
        {
            Margin = new Thickness(0, 0, 0, 12)
        };
        paragraph.Inlines.Add(new FlowRun("Text before the Span. "));

        var highlightSpan = new FlowSpan
        {
            Background = new SolidColorBrush(Color.Parse("#FFFFEBEE")),
            Foreground = new SolidColorBrush(Color.Parse("#FF1A237E"))
        };
        highlightSpan.Inlines.Add(new FlowRun("Text within the Span is red and "));
        highlightSpan.Inlines.Add(new FlowBold(new FlowRun("this text is inside the Span-derived element Bold.")));
        highlightSpan.Inlines.Add(new FlowRun(" A Span can contain more than text, including inline UI once supported."));
        paragraph.Inlines.Add(highlightSpan);

        paragraph.Inlines.Add(new FlowRun(" Text after the Span."));
        document.Blocks.Add(paragraph);

        var note = new FlowParagraph
        {
            Margin = new Thickness(0, 0, 0, 0)
        };
        note.Inlines.Add(new FlowItalic("InlineUIContainer is not yet available; see the placeholder sample for the commented code."));
        document.Blocks.Add(note);

        return document;
    }

    private static DocumentFlow CreateDocsInlineUiContainerPlaceholder()
    {
        var document = CreateDocsScaffold();

        var explanation = new FlowParagraph
        {
            Margin = new Thickness(0, 0, 0, 12)
        };
        explanation.Inlines.Add(new FlowBold(new FlowRun("InlineUIContainer pending:")));
        explanation.Inlines.Add(new FlowRun(" This sample will host an Avalonia button inline with text once the container type exists."));
        document.Blocks.Add(explanation);

        var textPreview = new FlowParagraph
        {
            Margin = new Thickness(0, 0, 0, 12)
        };
        textPreview.Inlines.Add(new FlowRun("Text to precede the button... [button placeholder] ...Text to follow the button."));
        document.Blocks.Add(textPreview);

        var codeHeader = new FlowParagraph
        {
            Margin = new Thickness(0, 0, 0, 4),
            FontWeight = FontWeight.SemiBold
        };
        codeHeader.Inlines.Add(new FlowRun("Commented code awaiting InlineUIContainer support:"));
        document.Blocks.Add(codeHeader);

        var codeLines = new[]
        {
            "var button = new Button { Content = \"Button\" };",
            "var inlineContainer = new InlineUIContainer { Child = button, BaselineAlignment = BaselineAlignment.Bottom };",
            "paragraph.Inlines.Add(inlineContainer);"
        };

        foreach (var line in codeLines)
        {
            document.Blocks.Add(new FlowParagraph(line)
            {
                FontFamily = FontFamily.Default,
                FontSize = 13,
                Margin = new Thickness(6, 0, 0, 0)
            });
        }

        return document;

        /*
        // Future implementation when InlineUIContainer arrives:
        var paragraph = new FlowParagraph();
        paragraph.Inlines.Add(new FlowRun("Text to precede the button... "));
        paragraph.Inlines.Add(new InlineUIContainer
        {
            BaselineAlignment = BaselineAlignment.Bottom,
            Child = new Button { Content = "Button" }
        });
        paragraph.Inlines.Add(new FlowRun(" Text to follow the button..."));
        document.Blocks.Add(paragraph);
        */
    }

    private static DocumentFlow CreateDocsLineBreakPlaceholder()
    {
        var document = CreateDocsScaffold();

        var header = new FlowParagraph
        {
            Margin = new Thickness(0, 0, 0, 12)
        };
        header.Inlines.Add(new FlowBold(new FlowRun("LineBreak pending:")));
        header.Inlines.Add(new FlowRun(" The inline LineBreak element is not yet available, so each line is rendered as its own paragraph."));
        document.Blocks.Add(header);

        document.Blocks.Add(new FlowParagraph("Before the LineBreak in Paragraph."));
        document.Blocks.Add(new FlowParagraph("After the LineBreak in Paragraph."));
        document.Blocks.Add(new FlowParagraph("After two LineBreaks in Paragraph."));
        document.Blocks.Add(new FlowParagraph("<Paragraph><LineBreak/></Paragraph>"));
        document.Blocks.Add(new FlowParagraph("After a Paragraph with only a LineBreak in it."));

        return document;

        /*
        // Future implementation when LineBreak arrives:
        var paragraph = new FlowParagraph();
        paragraph.Inlines.Add(new FlowRun("Before the LineBreak in Paragraph."));
        paragraph.Inlines.Add(new LineBreak());
        paragraph.Inlines.Add(new FlowRun("After the LineBreak in Paragraph."));
        paragraph.Inlines.Add(new LineBreak());
        paragraph.Inlines.Add(new LineBreak());
        paragraph.Inlines.Add(new FlowRun("After two LineBreaks in Paragraph."));

                var summaryHeader = new FlowParagraph
                {
                    Margin = new Thickness(0, 12, 0, 6),
                    FontWeight = FontWeight.SemiBold
                };
                summaryHeader.Inlines.Add(new FlowRun("Anchored blocks and tables:"));
                document.Blocks.Add(summaryHeader);

                var summaryTable = new FlowTable
                {
                    CellSpacing = 6,
                    Margin = new Thickness(0, 0, 0, 12),
                    Background = new SolidColorBrush(Color.Parse("#FFF6FBFF"))
                };
                summaryTable.Columns.Add(new FlowTableColumn { Width = 180 });
                summaryTable.Columns.Add(new FlowTableColumn());

                var summaryGroup = new FlowTableRowGroup();
                summaryGroup.Rows.Add(new FlowTableRow(
                    CreateSummaryHeaderCell("Figure"),
                    CreateSummaryBodyCell("Hosts nested block content inline with configurable sizing.")));
                summaryGroup.Rows.Add(new FlowTableRow(
                    CreateSummaryHeaderCell("Floater", zebra: true),
                    CreateSummaryBodyCell("Aligns anchored content to the requested edge while sharing paragraph styling.", zebra: true)));
                summaryGroup.Rows.Add(new FlowTableRow(
                    CreateSummaryHeaderCell("Table", highlight: true),
                    CreateSummaryBodyCell("Supports headers, spacing, and zebra striping for parity matrices.", highlight: true)));
                summaryTable.RowGroups.Add(summaryGroup);
                document.Blocks.Add(summaryTable);

        document.Blocks.Add(paragraph);
        */
    }

    private static DocumentFlow CreateDocsTypographyPlaceholder()
    {
        var document = CreateDocsScaffold();

        var header = new FlowParagraph
        {
            Margin = new Thickness(0, 0, 0, 12)
        };
        header.Inlines.Add(new FlowBold(new FlowRun("Typography pending:")));
        header.Inlines.Add(new FlowRun(" TextElement.Typography is not surfaced yet; the sample highlights the intended settings."));
        document.Blocks.Add(header);

        var description = new FlowParagraph
        {
            Margin = new Thickness(0, 0, 0, 12)
        };
        description.Inlines.Add(new FlowRun("This document would normally render old-style numerals, stacked fractions, and inferior variants using an OpenType font."));
        document.Blocks.Add(description);

        var codeHeader = new FlowParagraph
        {
            Margin = new Thickness(0, 0, 0, 4),
            FontWeight = FontWeight.SemiBold
        };
        codeHeader.Inlines.Add(new FlowRun("Target code once typography APIs exist:"));
        document.Blocks.Add(codeHeader);

        var codeLines = new[]
        {
            "var paragraph = new Paragraph { FontFamily = new FontFamily(\"Palatino Linotype\"), FontSize = 18 };",
            "paragraph.Typography.NumeralStyle = FontNumeralStyle.OldStyle;",
            "paragraph.Typography.Fraction = FontFraction.Stacked;",
            "paragraph.Typography.Variants = FontVariants.Inferior;"
        };

        foreach (var line in codeLines)
        {
            document.Blocks.Add(new FlowParagraph(line)
            {
                FontFamily = FontFamily.Default,
                FontSize = 13,
                Margin = new Thickness(6, 0, 0, 0)
            });
        }

        return document;

        /*
        // Future implementation when typography properties are exposed:
        var paragraph = new FlowParagraph
        {
            TextAlignment = TextAlignment.Left,
            FontSize = 18,
            FontFamily = new FontFamily("Palatino Linotype")
        };
        paragraph.Typography.NumeralStyle = FontNumeralStyle.OldStyle;
        paragraph.Typography.Fraction = FontFraction.Stacked;
        paragraph.Typography.Variants = FontVariants.Inferior;
        paragraph.Inlines.Add(new FlowRun("This text has some altered typography characteristics..."));
        document.Blocks.Add(paragraph);
        */
    }

    private static DocumentFlow CreateDocsScaffold(double fontSize = 16)
    {
        return new DocumentFlow
        {
            FontFamily = FontFamily.Default,
            FontSize = fontSize,
            PagePadding = new Thickness(36, 32, 36, 32),
            Background = Brushes.White,
            Foreground = Brushes.Black
        };
    }

    private static DocumentFlow CreateIntroDocument()
    {
        var document = new DocumentFlow
        {
            FontFamily = FontFamily.Default,
            FontSize = 16,
            PagePadding = new Thickness(36, 32, 36, 32),
            Background = Brushes.White,
            Foreground = Brushes.Black
        };

        var title = new FlowParagraph("Avalonia FlowDocument")
        {
            FontSize = 28,
            FontWeight = FontWeight.Bold,
            Margin = new Thickness(0, 0, 0, 20)
        };
        document.Blocks.Add(title);

        var intro = new FlowParagraph
        {
            Margin = new Thickness(0, 0, 0, 12)
        };
        intro.Inlines.Add(new FlowRun("This sample showcases a lightweight "));
        intro.Inlines.Add(new FlowBold(new FlowUnderline("FlowDocument")));
        intro.Inlines.Add(new FlowRun(" surface for Avalonia. Inline spans let you mix "));
        intro.Inlines.Add(new FlowItalic("styling"));
        intro.Inlines.Add(new FlowRun(" within a single paragraph, including "));
        var highlight = new FlowSpan(new FlowBold("nested "), new FlowUnderline("decorations"))
        {
            Background = new SolidColorBrush(Color.Parse("#FFF3C4")),
            Foreground = Brushes.Black
        };
        intro.Inlines.Add(highlight);
        intro.Inlines.Add(new FlowRun(" and inherited typography."));
        document.Blocks.Add(intro);

        var highlights = new FlowSection
        {
            Margin = new Thickness(0, 4, 0, 12)
        };

        var highlightIntro = new FlowParagraph
        {
            Margin = new Thickness(0, 0, 0, 6)
        };
        highlightIntro.Inlines.Add(new FlowRun("Current highlights:"));
        highlights.Blocks.Add(highlightIntro);

        var featureList = new FlowList
        {
            MarkerStyle = FlowListMarkerStyle.Decimal,
            MarkerOffset = 28,
            Margin = new Thickness(20, 0, 0, 0)
        };

        var typography = new FlowParagraph();
        typography.Inlines.Add(new FlowBold("Typography inheritance"));
        typography.Inlines.Add(new FlowRun(" across documents, blocks, and spans."));
        featureList.ListItems.Add(new FlowListItem(typography));

        var inlineFormatting = new FlowParagraph();
        inlineFormatting.Inlines.Add(new FlowBold("Inline formatting"));
        inlineFormatting.Inlines.Add(new FlowRun(" with bold, italic, underline, and background highlights."));
        featureList.ListItems.Add(new FlowListItem(inlineFormatting));

        var blockExpansionSection = new FlowSection();

        var blockExpansionIntro = new FlowParagraph();
        blockExpansionIntro.Inlines.Add(new FlowBold("Block surface expansion"));
        blockExpansionIntro.Inlines.Add(new FlowRun(" introducing "));
        blockExpansionIntro.Inlines.Add(new FlowItalic("Section"));
        blockExpansionIntro.Inlines.Add(new FlowRun(" and "));
        blockExpansionIntro.Inlines.Add(new FlowItalic("List"));
        blockExpansionIntro.Inlines.Add(new FlowRun(" elements rendered by the viewer."));
        blockExpansionSection.Blocks.Add(blockExpansionIntro);

        var rendererDetails = new FlowList
        {
            MarkerStyle = FlowListMarkerStyle.Decimal,
            StartIndex = 3,
            MarkerOffset = 20,
            Margin = new Thickness(8, 6, 0, 0)
        };

        var sectionDetail = new FlowParagraph();
        sectionDetail.Inlines.Add(new FlowBold("Sections"));
        sectionDetail.Inlines.Add(new FlowRun(" wrap groups of blocks with shared margin."));
        rendererDetails.ListItems.Add(new FlowListItem(sectionDetail));

        var listDetail = new FlowParagraph();
        listDetail.Inlines.Add(new FlowBold("Lists"));
    listDetail.Inlines.Add(new FlowRun(" respect marker styles, offsets, and start indices (this nested sequence begins at 3)."));
        rendererDetails.ListItems.Add(new FlowListItem(listDetail));

        var nestingDetail = new FlowParagraph();
    nestingDetail.Inlines.Add(new FlowRun("Nested blocks align to marker indentation automatically."));
        rendererDetails.ListItems.Add(new FlowListItem(nestingDetail));

        blockExpansionSection.Blocks.Add(rendererDetails);
        featureList.ListItems.Add(new FlowListItem(blockExpansionSection));

        highlights.Blocks.Add(featureList);
        document.Blocks.Add(highlights);

        var summaryHeader = new FlowParagraph
        {
            Margin = new Thickness(0, 12, 0, 6),
            FontWeight = FontWeight.SemiBold
        };
        summaryHeader.Inlines.Add(new FlowRun("Anchored blocks and tables:"));
        document.Blocks.Add(summaryHeader);

        var summaryTable = new FlowTable
        {
            CellSpacing = 6,
            Margin = new Thickness(0, 0, 0, 12),
            Background = new SolidColorBrush(Color.Parse("#FFF6FBFF")),
            GridLinesBrush = new SolidColorBrush(Color.Parse("#FFB0BEC5")),
            GridLinesThickness = 1
        };
        summaryTable.Columns.Add(new FlowTableColumn { Width = 180 });
        summaryTable.Columns.Add(new FlowTableColumn());

        var summaryRows = new FlowTableRowGroup();
        summaryRows.Rows.Add(new FlowTableRow(
            CreateSummaryHeaderCell("Figure"),
            CreateSummaryBodyCell("Hosts nested block content inline with configurable sizing.")));
        summaryRows.Rows.Add(new FlowTableRow(
            CreateSummaryHeaderCell("Floater", zebra: true),
            CreateSummaryBodyCell("Aligns anchored content to the requested edge while sharing paragraph styling.", zebra: true)));
        summaryRows.Rows.Add(new FlowTableRow(
            CreateSummaryHeaderCell("Table", highlight: true),
            CreateSummaryBodyCell("Supports headers, spacing, and zebra striping for parity matrices.", highlight: true)));
        summaryTable.RowGroups.Add(summaryRows);
        document.Blocks.Add(summaryTable);

        var closing = new FlowParagraph
        {
            Margin = new Thickness(0, 8, 0, 0)
        };
        closing.Inlines.Add(new FlowRun("Try editing the document tree in code to see updates reflected immediately in the viewer. "));
    closing.Inlines.Add(new FlowRun("The new "));
        closing.Inlines.Add(new FlowBold("Section"));
        closing.Inlines.Add(new FlowRun(" and "));
        closing.Inlines.Add(new FlowBold("List"));
    closing.Inlines.Add(new FlowRun(" blocks plug right into the renderer, including nested lists with custom start indices and marker offsets. Use the "));
        var buttonHint = new FlowUnderline("Add sample entry")
        {
            FontWeight = FontWeight.SemiBold
        };
        closing.Inlines.Add(buttonHint);
        closing.Inlines.Add(new FlowRun(" action to append new paragraphs at runtime."));
        document.Blocks.Add(closing);

        static FlowTableCell CreateSummaryHeaderCell(string text, bool zebra = false, bool highlight = false)
        {
            var paragraph = new FlowParagraph(text)
            {
                Margin = new Thickness(0),
                FontWeight = FontWeight.SemiBold
            };

            var background = highlight
                ? new SolidColorBrush(Color.Parse("#FFFFF0E4"))
                : zebra
                    ? new SolidColorBrush(Color.Parse("#FFECEFFF"))
                    : new SolidColorBrush(Color.Parse("#FFE2ECFF"));

            return new FlowTableCell(paragraph)
            {
                Padding = new Thickness(12, 8, 12, 8),
                Background = background,
                BorderBrush = new SolidColorBrush(Color.Parse("#FF90A4AE")),
                BorderThickness = new Thickness(1)
            };
        }

        static FlowTableCell CreateSummaryBodyCell(string text, bool zebra = false, bool highlight = false)
        {
            var paragraph = new FlowParagraph
            {
                Margin = new Thickness(0)
            };
            paragraph.Inlines.Add(new FlowRun(text));

            var background = highlight
                ? new SolidColorBrush(Color.Parse("#FFFFF6E6"))
                : zebra
                    ? new SolidColorBrush(Color.Parse("#FFF3F7FF"))
                    : null;

            return new FlowTableCell(paragraph)
            {
                Padding = new Thickness(12, 8, 12, 8),
                Background = background,
                BorderBrush = new SolidColorBrush(Color.Parse("#FFCFD8DC")),
                BorderThickness = new Thickness(1)
            };
        }

        return document;
    }

    private static DocumentFlow CreateLogDocument()
    {
        var document = new DocumentFlow
        {
            FontFamily = FontFamily.Default,
            FontSize = 15,
            PagePadding = new Thickness(32, 28, 32, 28),
            Background = Brushes.White,
            Foreground = Brushes.Black
        };

        var header = new FlowParagraph("Build Activity Log")
        {
            FontSize = 24,
            FontWeight = FontWeight.SemiBold,
            Margin = new Thickness(0, 0, 0, 16)
        };
        document.Blocks.Add(header);

        var explanation = new FlowParagraph
        {
            Margin = new Thickness(0, 0, 0, 12)
        };
        explanation.Inlines.Add(new FlowRun("Use the action button to append timestamped entries below. Each click demonstrates how the document " +
                                             "structure stays live and re-renders on demand."));
        document.Blocks.Add(explanation);

        return document;
    }

    private static DocumentFlow CreateParityMatrixDocument()
    {
        var document = new DocumentFlow
        {
            FontFamily = FontFamily.Default,
            FontSize = 15,
            PagePadding = new Thickness(36, 30, 36, 30),
            Background = Brushes.White,
            Foreground = Brushes.Black
        };

        var title = new FlowParagraph("FlowDocument parity matrix")
        {
            FontSize = 26,
            FontWeight = FontWeight.SemiBold,
            Margin = new Thickness(0, 0, 0, 16)
        };
        document.Blocks.Add(title);

        var overview = new FlowParagraph
        {
            Margin = new Thickness(0, 0, 0, 12)
        };
        overview.Inlines.Add(new FlowRun("Tables join the renderer with column sizing, spacing, and cell-level backgrounds. The matrix below captures the current parity status across major features."));
        document.Blocks.Add(overview);

        var zebraRowBackground = new SolidColorBrush(Color.Parse("#FFF5F8FF"));
        var highlightRowBackground = new SolidColorBrush(Color.Parse("#FFFFF6EB"));

        var table = new FlowTable
        {
            CellSpacing = 8,
            Margin = new Thickness(0, 8, 0, 16),
            Background = new SolidColorBrush(Color.Parse("#FFF9FBFF")),
            GridLinesBrush = new SolidColorBrush(Color.Parse("#FFB0BEC5")),
            GridLinesThickness = 1
        };

        table.Columns.Add(new FlowTableColumn { Width = 220 });
        table.Columns.Add(new FlowTableColumn());
        table.Columns.Add(new FlowTableColumn { Width = 140 });

        var headerGroup = new FlowTableRowGroup();
        headerGroup.Rows.Add(new FlowTableRow(
            CreateHeaderCell("Area"),
            CreateHeaderCell("Highlights"),
            CreateHeaderCell("Status")));
        table.RowGroups.Add(headerGroup);

        var bodyGroup = new FlowTableRowGroup();
        bodyGroup.Rows.Add(CreateBodyRow(
            "Document model",
            "Paragraphs, inline spans, sections, lists, and tables mirror WPF FlowDocument semantics.",
            "✅ Complete",
            new SolidColorBrush(Color.Parse("#FF1B5E20")),
            null));
        bodyGroup.Rows.Add(CreateBodyRow(
            "Rendering surface",
            "FlowDocumentView now renders nested lists and multi-row tables with shared marker indentation.",
            "⚙️ In progress",
            new SolidColorBrush(Color.Parse("#FFB45309")),
            zebraRowBackground));
        bodyGroup.Rows.Add(CreateBodyRow(
            "Tooling & coverage",
            "Upcoming work focuses on mutation tests, serialization helpers, and printing support.",
            "🗓️ Planned",
            new SolidColorBrush(Color.Parse("#FF3F51B5")),
            highlightRowBackground));
        table.RowGroups.Add(bodyGroup);

        document.Blocks.Add(table);

        var outro = new FlowParagraph
        {
            Margin = new Thickness(0, 12, 0, 0)
        };
        outro.Inlines.Add(new FlowRun("Cells can host any block content, inherit styling, and sit alongside nested flow content. Try experimenting with additional rows or even embedding lists inside cells."));
        document.Blocks.Add(outro);

        return document;

        static FlowTableCell CreateHeaderCell(string text)
        {
            var paragraph = new FlowParagraph(text)
            {
                Margin = new Thickness(0),
                FontWeight = FontWeight.SemiBold,
                FontSize = 16
            };

            var cell = new FlowTableCell(paragraph)
            {
                Padding = new Thickness(14, 10, 14, 10),
                Background = new SolidColorBrush(Color.Parse("#FFE2ECFF")),
                BorderBrush = new SolidColorBrush(Color.Parse("#FF90A4AE")),
                BorderThickness = new Thickness(1)
            };

            return cell;
        }

        static FlowTableRow CreateBodyRow(string area, string description, string status, IBrush statusBrush, IBrush? rowBackground)
        {
            var areaParagraph = new FlowParagraph(area)
            {
                Margin = new Thickness(0),
                FontWeight = FontWeight.SemiBold
            };

            var descriptionParagraph = new FlowParagraph
            {
                Margin = new Thickness(0)
            };
            descriptionParagraph.Inlines.Add(new FlowRun(description));

            var statusParagraph = new FlowParagraph
            {
                Margin = new Thickness(0),
                TextAlignment = TextAlignment.Center,
                FontWeight = FontWeight.SemiBold,
                Foreground = statusBrush
            };
            statusParagraph.Inlines.Add(new FlowRun(status));

            var areaCell = new FlowTableCell(areaParagraph)
            {
                Padding = new Thickness(12, 8, 12, 8),
                Background = rowBackground,
                BorderBrush = new SolidColorBrush(Color.Parse("#FFCFD8DC")),
                BorderThickness = new Thickness(1)
            };

            var descriptionCell = new FlowTableCell(descriptionParagraph)
            {
                Padding = new Thickness(12, 8, 12, 8),
                Background = rowBackground,
                BorderBrush = new SolidColorBrush(Color.Parse("#FFCFD8DC")),
                BorderThickness = new Thickness(1)
            };

            var statusCell = new FlowTableCell(statusParagraph)
            {
                Padding = new Thickness(12, 8, 12, 8),
                Background = rowBackground,
                BorderBrush = new SolidColorBrush(Color.Parse("#FFCFD8DC")),
                BorderThickness = new Thickness(1)
            };

            return new FlowTableRow(areaCell, descriptionCell, statusCell);
        }
    }

    private static DocumentFlow CreateWeekendEditionDocument()
    {
        var document = new DocumentFlow
        {
            FontFamily = FontFamily.Default,
            FontSize = 15,
            PagePadding = new Thickness(48, 36, 48, 36),
            Background = Brushes.White,
            Foreground = Brushes.Black
        };

        var masthead = new FlowParagraph("The Avalonia Chronicle")
        {
            FontSize = 30,
            FontWeight = FontWeight.Bold,
            TextAlignment = TextAlignment.Center,
            Margin = new Thickness(0, 0, 0, 8)
        };
        document.Blocks.Add(masthead);

        var dateline = new FlowParagraph("Avalonia City • October 1, 2025")
        {
            TextAlignment = TextAlignment.Center,
            FontSize = 14,
            Foreground = new SolidColorBrush(Color.Parse("#FF546E7A")),
            Margin = new Thickness(0, 0, 0, 18)
        };
        document.Blocks.Add(dateline);

        var lead = new FlowParagraph
        {
            Margin = new Thickness(0, 0, 0, 16)
        };
        lead.Inlines.Add(new FlowBold(new FlowRun("Cross-platform documents headline the weekend edition.")));
        lead.Inlines.Add(new FlowRun(" Avalonia's FlowDocument renderer now layers anchored figures, tables, and sections for editorial layouts."));

        var pullQuote = new FlowFigure
        {
            Width = FlowFigureLength.FromPixels(220),
            Margin = new Thickness(0, 6, 20, 8),
            Padding = new Thickness(14, 12, 14, 12),
            Background = new SolidColorBrush(Color.Parse("#FFF1F8E9")),
            HorizontalAnchor = FlowFigureHorizontalAnchor.ContentLeft
        };
        pullQuote.Blocks.Add(new FlowParagraph("\"Design once, delight everywhere.\"")
        {
            Margin = new Thickness(0),
            FontSize = 18,
            FontStyle = FontStyle.Italic
        });
        pullQuote.Blocks.Add(new FlowParagraph("— Avalonia Design Team")
        {
            Margin = new Thickness(0, 6, 0, 0),
            FontSize = 13,
            Foreground = new SolidColorBrush(Color.Parse("#FF33691E"))
        });
        lead.Inlines.Add(pullQuote);
        lead.Inlines.Add(new FlowRun(" Explore the columns below to see lists, tables, and shaded sections working together like a weekend paper."));
        document.Blocks.Add(lead);

        var columns = new FlowTable
        {
            CellSpacing = 24,
            Margin = new Thickness(0, 0, 0, 16),
            GridLinesBrush = Brushes.Transparent,
            GridLinesThickness = 0
        };
        columns.Columns.Add(new FlowTableColumn());
        columns.Columns.Add(new FlowTableColumn());

        var columnGroup = new FlowTableRowGroup();
        columnGroup.Rows.Add(new FlowTableRow(
            CreateWeekendColumnCell(BuildLeadColumn),
            CreateWeekendColumnCell(BuildSidebarColumn)));
        columns.RowGroups.Add(columnGroup);
        document.Blocks.Add(columns);

        var footer = new FlowParagraph
        {
            Margin = new Thickness(0, 12, 0, 0),
            TextAlignment = TextAlignment.Center,
            FontSize = 13
        };
        footer.Inlines.Add(new FlowItalic("Tweak spacing, swap colors, or add new sections to tailor the edition to your audience."));
        document.Blocks.Add(footer);

        return document;

        static FlowTableCell CreateWeekendColumnCell(Action<FlowTableCell> populate)
        {
            var cell = new FlowTableCell
            {
                Padding = new Thickness(18, 14, 18, 14),
                Background = new SolidColorBrush(Color.Parse("#FFFDF5E6")),
                BorderBrush = new SolidColorBrush(Color.Parse("#FFCFD8DC")),
                BorderThickness = new Thickness(1)
            };

            populate(cell);
            return cell;
        }

        static void BuildLeadColumn(FlowTableCell cell)
        {
            var opener = new FlowParagraph
            {
                Margin = new Thickness(0, 0, 0, 10)
            };
            opener.Inlines.Add(new FlowBold("Morning brief: "));
            opener.Inlines.Add(new FlowRun("FlowDocumentView paints crisp grid lines and respects cell backgrounds across the entire table surface."));
            cell.Blocks.Add(opener);

            var highlights = new FlowList
            {
                Margin = new Thickness(8, 0, 0, 12),
                MarkerStyle = FlowListMarkerStyle.Decimal,
                MarkerOffset = 24
            };
            highlights.ListItems.Add(new FlowListItem(new FlowParagraph("Anchored figures now mirror paragraph padding and alignment semantics.")));
            highlights.ListItems.Add(new FlowListItem(new FlowParagraph("Table cells accept borders, zebra striping, and nested blocks.")));
            highlights.ListItems.Add(new FlowListItem(new FlowParagraph("Samples mutate live, so editors can iterate without restarting the app.")));
            cell.Blocks.Add(highlights);

            var recap = new FlowSection
            {
                Margin = new Thickness(0, 0, 0, 10),
                Background = new SolidColorBrush(Color.Parse("#FFE3F2FD"))
            };
            var recapHeader = new FlowParagraph("In this edition:")
            {
                Margin = new Thickness(12, 10, 12, 4),
                FontWeight = FontWeight.SemiBold
            };
            recap.Blocks.Add(recapHeader);

            var recapList = new FlowList
            {
                Margin = new Thickness(22, 0, 12, 12),
                MarkerStyle = FlowListMarkerStyle.Decimal,
                MarkerOffset = 20
            };
            recapList.ListItems.Add(new FlowListItem(new FlowParagraph("Rendering deep dive")));
            recapList.ListItems.Add(new FlowListItem(new FlowParagraph("Interactive samples round-up")));
            recapList.ListItems.Add(new FlowListItem(new FlowParagraph("Roadmap sneak peek")));
            recap.Blocks.Add(recapList);
            cell.Blocks.Add(recap);

            var closer = new FlowParagraph
            {
                Margin = new Thickness(0)
            };
            closer.Inlines.Add(new FlowRun("Editors can blend "));
            closer.Inlines.Add(new FlowBold("sections"));
            closer.Inlines.Add(new FlowRun(", "));
            closer.Inlines.Add(new FlowBold("lists"));
            closer.Inlines.Add(new FlowRun(", and "));
            closer.Inlines.Add(new FlowBold("tables"));
            closer.Inlines.Add(new FlowRun(" to compose a narrative that reads like print while staying entirely within FlowDocument."));
            cell.Blocks.Add(closer);
        }

        static void BuildSidebarColumn(FlowTableCell cell)
        {
            var title = new FlowParagraph("Weekend spotlight")
            {
                Margin = new Thickness(0, 0, 0, 6),
                FontWeight = FontWeight.SemiBold,
                FontSize = 18
            };
            cell.Blocks.Add(title);

            var blurb = new FlowParagraph
            {
                Margin = new Thickness(0, 0, 0, 10)
            };
            blurb.Inlines.Add(new FlowItalic("Photo essay: "));
            blurb.Inlines.Add(new FlowRun("Avalonia UI from desktop to handheld devices."));
            cell.Blocks.Add(blurb);

            var notes = new FlowSection
            {
                Margin = new Thickness(0, 0, 0, 12),
                Background = new SolidColorBrush(Color.Parse("#FFFFF3E0"))
            };
            var notesHeader = new FlowParagraph("Editor notes")
            {
                Margin = new Thickness(12, 10, 12, 4),
                FontWeight = FontWeight.SemiBold
            };
            notes.Blocks.Add(notesHeader);

            var notesBody = new FlowParagraph
            {
                Margin = new Thickness(14, 0, 14, 10)
            };
            notesBody.Inlines.Add(new FlowRun("• Use "));
            notesBody.Inlines.Add(new FlowBold("GridLinesBrush"));
            notesBody.Inlines.Add(new FlowRun(" to revive table borders."));
            notesBody.Inlines.Add(new FlowRun("\n• Combine "));
            notesBody.Inlines.Add(new FlowBold("TableCell.Background"));
            notesBody.Inlines.Add(new FlowRun(" with figures to build magazine-style callouts."));
            notes.Blocks.Add(notesBody);
            cell.Blocks.Add(notes);

            var schedule = new FlowTable
            {
                CellSpacing = 6,
                GridLinesBrush = new SolidColorBrush(Color.Parse("#FFB0BEC5")),
                GridLinesThickness = 1,
                Background = new SolidColorBrush(Color.Parse("#FFF8FBFF"))
            };
            schedule.Columns.Add(new FlowTableColumn { Width = 120 });
            schedule.Columns.Add(new FlowTableColumn());

            var scheduleHeader = new FlowTableRowGroup();
            scheduleHeader.Rows.Add(new FlowTableRow(
                CreateWeekendSidebarHeaderCell("Segment"),
                CreateWeekendSidebarHeaderCell("Time")));
            schedule.RowGroups.Add(scheduleHeader);

            var scheduleBody = new FlowTableRowGroup();
            scheduleBody.Rows.Add(new FlowTableRow(
                CreateWeekendSidebarBodyCell("Live coding"),
                CreateWeekendSidebarBodyCell("09:00")));
            scheduleBody.Rows.Add(new FlowTableRow(
                CreateWeekendSidebarBodyCell("Design review", zebra: true),
                CreateWeekendSidebarBodyCell("11:00", zebra: true)));
            scheduleBody.Rows.Add(new FlowTableRow(
                CreateWeekendSidebarBodyCell("Release update", highlight: true),
                CreateWeekendSidebarBodyCell("15:30", highlight: true)));
            schedule.RowGroups.Add(scheduleBody);

            cell.Blocks.Add(schedule);
        }

        static FlowTableCell CreateWeekendSidebarHeaderCell(string text)
        {
            var paragraph = new FlowParagraph(text)
            {
                Margin = new Thickness(0),
                FontWeight = FontWeight.SemiBold
            };

            return new FlowTableCell(paragraph)
            {
                Padding = new Thickness(10, 8, 10, 8),
                Background = new SolidColorBrush(Color.Parse("#FFE0E7FF")),
                BorderBrush = new SolidColorBrush(Color.Parse("#FF90A4AE")),
                BorderThickness = new Thickness(1)
            };
        }

        static FlowTableCell CreateWeekendSidebarBodyCell(string text, bool zebra = false, bool highlight = false)
        {
            var background = highlight
                ? new SolidColorBrush(Color.Parse("#FFFFF6E6"))
                : zebra
                    ? new SolidColorBrush(Color.Parse("#FFF1F4FB"))
                    : null;

            return new FlowTableCell(new FlowParagraph(text) { Margin = new Thickness(0) })
            {
                Padding = new Thickness(10, 6, 10, 6),
                Background = background,
                BorderBrush = new SolidColorBrush(Color.Parse("#FFCFD8DC")),
                BorderThickness = new Thickness(1)
            };
        }
    }

    private static void AppendLogEntry(DocumentFlow document)
    {
        var paragraph = new FlowParagraph
        {
            Margin = new Thickness(12, 0, 0, 8)
        };

        paragraph.Inlines.Add(new FlowRun("• "));
        var timestamp = new FlowSpan(new FlowRun(DateTimeOffset.Now.ToString("HH:mm:ss")))
        {
            FontWeight = FontWeight.SemiBold,
            Foreground = new SolidColorBrush(Color.Parse("#1B5E20"))
        };
        paragraph.Inlines.Add(timestamp);
        paragraph.Inlines.Add(new FlowRun(" — "));
        paragraph.Inlines.Add(new FlowItalic("Document updated."));
        document.Blocks.Add(paragraph);
    }

    private bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    public sealed record SampleDocument(
        string Name,
        string Description,
        Func<DocumentFlow> Factory,
        Action<DocumentFlow>? MutateAction)
    {
        public string Name { get; } = Name;
        public string Description { get; } = Description;
        public Func<DocumentFlow> Factory { get; } = Factory ?? throw new ArgumentNullException(nameof(Factory));
        public Action<DocumentFlow>? MutateAction { get; } = MutateAction;

        public override string ToString() => Name;
    }
}
