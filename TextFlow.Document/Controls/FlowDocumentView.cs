using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.TextFormatting;
using Avalonia.Metadata;
using Avalonia.Threading;
using Avalonia.Utilities;
using Documents = TextFlow.Document.Documents;


namespace TextFlow.Document.Controls;

/// <summary>
/// Simple renderer for <see cref="Documents.FlowDocument"/> instances.
/// </summary>
public class FlowDocumentView : Control
{
    private const double MarkerPadding = 6;
    private const double MinimumColumnWidth = 48;

    public readonly struct BlockLayoutEntry
    {
        public BlockLayoutEntry(Documents.TextElement element, TextLayout layout, Thickness margin, MarkerInfo[]? markers)
            : this(element, layout, null, null, margin, markers, CalculateIndent(markers))
        {
        }

        private BlockLayoutEntry(
            Documents.TextElement element,
            TextLayout? layout,
            TableLayout? tableLayout,
            AnchoredBlockLayout? anchoredLayout,
            Thickness margin,
            MarkerInfo[]? markers,
            double contentIndent)
        {
            Element = element ?? throw new ArgumentNullException(nameof(element));
            Layout = layout;
            TableLayout = tableLayout;
            AnchoredLayout = anchoredLayout;
            Margin = margin;
            Markers = markers;
            ContentIndent = contentIndent;
        }

    public Documents.TextElement Element { get; }
    public Documents.Block? Block => Element as Documents.Block;
    public Documents.AnchoredBlock? AnchoredBlock => Element as Documents.AnchoredBlock;
        public TextLayout? Layout { get; }
        public TableLayout? TableLayout { get; }
        public AnchoredBlockLayout? AnchoredLayout { get; }
        public Thickness Margin { get; }
        public MarkerInfo[]? Markers { get; }
        public double ContentIndent { get; }

        public double ContentWidth => AnchoredLayout?.Width ?? TableLayout?.Width ?? Layout?.Width ?? 0;

        public double ContentHeight => AnchoredLayout?.Height ?? TableLayout?.Height ?? Layout?.Height ?? 0;

        public BlockLayoutEntry WithMargin(Thickness margin) => new(Element, Layout, TableLayout, AnchoredLayout, margin, Markers, ContentIndent);

        public BlockLayoutEntry WithMarkers(MarkerInfo[]? markers)
        {
            return new BlockLayoutEntry(Element, Layout, TableLayout, AnchoredLayout, Margin, markers, CalculateIndent(markers));
        }

        public BlockLayoutEntry AddMargin(double left, double top, double right, double bottom)
        {
            var updated = new Thickness(
                Margin.Left + left,
                Margin.Top + top,
                Margin.Right + right,
                Margin.Bottom + bottom);

            return new BlockLayoutEntry(Element, Layout, TableLayout, AnchoredLayout, updated, Markers, ContentIndent);
        }

        public static BlockLayoutEntry ForTable(Block block, Thickness margin, TableLayout tableLayout) =>
            new(block, null, tableLayout, null, margin, null, 0);

        public static BlockLayoutEntry ForAnchoredBlock(AnchoredBlock anchored, Thickness margin, AnchoredBlockLayout layout, MarkerInfo[]? markers) =>
            new(anchored, null, null, layout, margin, markers, CalculateIndent(markers));

        private static double CalculateIndent(MarkerInfo[]? markers)
        {
            if (markers is null || markers.Length == 0)
            {
                return 0;
            }

            double indent = 0;
            foreach (var marker in markers)
            {
                indent += marker.Width;
            }

            return indent;
        }
    }

    public readonly struct MarkerInfo
    {
        public MarkerInfo(TextLayout layout, double width, bool isVisible)
        {
            Layout = layout;
            Width = width;
            IsVisible = isVisible;
        }

        public TextLayout Layout { get; }
        public double Width { get; }
        public bool IsVisible { get; }

        public MarkerInfo AsHidden() => new(Layout, Width, false);
    }

    public sealed class TableLayout
    {
        public TableLayout(
            Table table,
            double width,
            double height,
            double cellSpacing,
            IReadOnlyList<double> columnWidths,
            IReadOnlyList<TableRowLayout> rows,
            IBrush? gridLinesBrush,
            double gridLinesThickness)
        {
            Table = table;
            Width = width;
            Height = height;
            CellSpacing = cellSpacing;
            ColumnWidths = columnWidths;
            Rows = rows;
            GridLinesBrush = gridLinesBrush;
            GridLinesThickness = gridLinesThickness;
        }

        public Table Table { get; }
        public double Width { get; }
        public double Height { get; }
        public double CellSpacing { get; }
        public IReadOnlyList<double> ColumnWidths { get; }
        public IReadOnlyList<TableRowLayout> Rows { get; }
        public IBrush? GridLinesBrush { get; }
        public double GridLinesThickness { get; }
    }

    public sealed class TableRowLayout
    {
        public TableRowLayout(double height, IReadOnlyList<TableCellLayout> cells)
        {
            Height = height;
            Cells = cells;
        }

        public double Height { get; }
        public IReadOnlyList<TableCellLayout> Cells { get; }
    }

    public sealed class TableCellLayout
    {
        public TableCellLayout(
            TableCell? cell,
            IReadOnlyList<BlockLayoutEntry> content,
            Size contentSize,
            double columnWidth,
            Thickness padding,
            Thickness borderThickness,
            IBrush? borderBrush)
        {
            Cell = cell;
            Content = content;
            ContentSize = contentSize;
            ColumnWidth = columnWidth;
            Padding = padding;
            BorderThickness = borderThickness;
            BorderBrush = borderBrush;
        }

        public TableCell? Cell { get; }
        public IReadOnlyList<BlockLayoutEntry> Content { get; }
        public Size ContentSize { get; }
        public double ColumnWidth { get; }
        public Thickness Padding { get; }
        public Thickness BorderThickness { get; }
        public IBrush? BorderBrush { get; }
        public double DesiredHeight => ContentSize.Height + Padding.Top + Padding.Bottom;
    }

    public sealed class AnchoredBlockLayout
    {
        public AnchoredBlockLayout(
            AnchoredBlock block,
            IReadOnlyList<BlockLayoutEntry> content,
            Size contentExtent,
            Thickness padding,
            IBrush? background,
            double width,
            double height,
            double availableWidth,
            AnchoredHorizontalAlignment horizontalAlignment,
            double horizontalOffset,
            double verticalOffset)
        {
            Block = block;
            Content = content;
            ContentExtent = contentExtent;
            Padding = padding;
            Background = background;
            Width = width;
            Height = height;
            AvailableWidth = availableWidth;
            HorizontalAlignment = horizontalAlignment;
            HorizontalOffset = horizontalOffset;
            VerticalOffset = verticalOffset;
        }

        public AnchoredBlock Block { get; }
        public IReadOnlyList<BlockLayoutEntry> Content { get; }
        public Size ContentExtent { get; }
        public Thickness Padding { get; }
        public IBrush? Background { get; }
        public double Width { get; }
        public double Height { get; }
        public double AvailableWidth { get; }
        public AnchoredHorizontalAlignment HorizontalAlignment { get; }
        public double HorizontalOffset { get; }
        public double VerticalOffset { get; }
    }

    public enum AnchoredHorizontalAlignment
    {
        Left,
        Center,
        Right
    }

    public static readonly StyledProperty<Documents.FlowDocument?> DocumentProperty =
        AvaloniaProperty.Register<FlowDocumentView, Documents.FlowDocument?>(nameof(Document));

    private Documents.FlowDocument? _currentDocument;
    private IReadOnlyList<BlockLayoutEntry> _layouts = Array.Empty<BlockLayoutEntry>();
    private Size _lastConstraint;
    private bool _layoutsDirty = true;
    private Size _lastArrangeSize;

    static FlowDocumentView()
    {
        DocumentProperty.Changed.AddClassHandler<FlowDocumentView>((view, change) =>
        {
            var oldValue = change.GetOldValue<Documents.FlowDocument?>();
            var newValue = change.GetNewValue<Documents.FlowDocument?>();
            view.OnDocumentChanged(oldValue, newValue);
        });
    }

    [Content]
    public Documents.FlowDocument? Document
    {
        get => GetValue(DocumentProperty);
        set => SetValue(DocumentProperty, value);
    }

    private void OnDocumentChanged(Documents.FlowDocument? oldValue, Documents.FlowDocument? newValue)
    {
        if (oldValue != null)
        {
            oldValue.Changed -= OnDocumentInvalidated;
        }

        if (newValue != null)
        {
            newValue.Changed += OnDocumentInvalidated;
        }

        _currentDocument = newValue;
        _layoutsDirty = true;
        InvalidateMeasure();
        InvalidateVisual();
    }

    private void OnDocumentInvalidated(object? sender, EventArgs e)
    {
        _layoutsDirty = true;
        Dispatcher.UIThread.Post(() =>
        {
            InvalidateMeasure();
            InvalidateVisual();
        }, DispatcherPriority.Render);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        EnsureLayouts(availableSize);

        var doc = _currentDocument;
        if (doc is null || _layouts.Count == 0)
        {
            return base.MeasureOverride(availableSize);
        }

        var padding = doc.PagePadding;
        double width = padding.Left + padding.Right;
        double height = padding.Top + padding.Bottom;

        foreach (var entry in _layouts)
        {
            var entryWidth = entry.Margin.Left + entry.ContentIndent + entry.ContentWidth + entry.Margin.Right;
            width = Math.Max(width, padding.Left + padding.Right + entryWidth);
            height += entry.Margin.Top + entry.ContentHeight + entry.Margin.Bottom;
        }

        return new Size(width, height);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        _lastArrangeSize = finalSize;

        if (finalSize.Width > 0 && !double.IsNaN(finalSize.Width))
        {
            EnsureLayouts(finalSize);
        }

        return base.ArrangeOverride(finalSize);
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        var doc = _currentDocument;
        if (doc is null)
        {
            return;
        }

        DrawDocumentBackground(context, doc);

        var padding = doc.PagePadding;
        RenderDocumentContent(context, doc, new Point(padding.Left, padding.Top));
    }

    protected virtual void DrawDocumentBackground(DrawingContext context, Documents.FlowDocument doc)
    {
        var background = doc.Background;
        if (background is not null)
        {
            context.FillRectangle(background, new Rect(Bounds.Size));
        }
    }

    protected virtual void RenderDocumentContent(DrawingContext context, Documents.FlowDocument doc, Point origin)
    {
        RenderBlockSequence(context, origin, _layouts);
    }

    private void EnsureLayouts(Size availableSize)
    {
        if (!_layoutsDirty && _lastConstraint == availableSize)
        {
            return;
        }

        _lastConstraint = availableSize;
        _layoutsDirty = false;

        var doc = _currentDocument = Document;
        if (doc is null)
        {
            _layouts = Array.Empty<BlockLayoutEntry>();
            return;
        }

        var padding = doc.PagePadding;
        var effectiveWidth = availableSize.Width;
        if (!double.IsFinite(effectiveWidth) || effectiveWidth <= 0)
        {
            var boundsWidth = Bounds.Width;
            if (double.IsFinite(boundsWidth) && boundsWidth > 0)
            {
                effectiveWidth = boundsWidth;
            }
        }

        if ((!double.IsFinite(effectiveWidth) || effectiveWidth <= 0) && double.IsFinite(_lastArrangeSize.Width) && _lastArrangeSize.Width > 0)
        {
            effectiveWidth = _lastArrangeSize.Width;
        }

        var widthConstraint = double.IsFinite(effectiveWidth) && effectiveWidth > 0
            ? Math.Max(0, effectiveWidth - padding.Left - padding.Right)
            : double.PositiveInfinity;

        var documentFormatting = CreateDocumentFormatting(doc);
        var layouts = new List<BlockLayoutEntry>();

        foreach (var block in doc.Blocks)
        {
            List<MarkerInfo>? pendingMarkers = null;
            AppendBlockLayouts(doc, block, widthConstraint, documentFormatting, layouts, 0, 0, ref pendingMarkers);
        }

        _layouts = layouts;
    }

    private void AppendBlockLayouts(
        Documents.FlowDocument doc,
        Block block,
        double constraint,
        InlineFormatting documentFormatting,
        List<BlockLayoutEntry> layouts,
        double inheritedLeftIndent,
        double inheritedRightIndent,
        ref List<MarkerInfo>? pendingMarkers)
    {
        switch (block)
        {
            case Paragraph paragraph:
            {
                MarkerInfo[]? markers = null;
                if (pendingMarkers is { Count: > 0 })
                {
                    markers = pendingMarkers.ToArray();
                }

                var paragraphEntries = BuildParagraphLayouts(doc, paragraph, constraint, inheritedLeftIndent, inheritedRightIndent, markers);
                if (paragraphEntries.Count > 0)
                {
                    layouts.AddRange(paragraphEntries);
                }
                pendingMarkers = null;
                break;
            }
            case Section section:
                AppendSectionLayouts(doc, section, constraint, documentFormatting, layouts, inheritedLeftIndent, inheritedRightIndent, ref pendingMarkers);
                break;
            case List list:
                AppendListLayouts(doc, list, constraint, documentFormatting, layouts, inheritedLeftIndent, inheritedRightIndent, ref pendingMarkers);
                break;
            case Table table:
            {
                var entry = BuildTableLayout(doc, table, constraint, documentFormatting, inheritedLeftIndent, inheritedRightIndent);
                layouts.Add(entry);
                pendingMarkers = null;
                break;
            }
            default:
                // Fallback: attempt to render plain text representation if provided.
                var buffer = new StringBuilder();
                block.CollectPlainText(buffer);
                if (buffer.Length > 0)
                {
                    var paragraph = new Paragraph(buffer.ToString())
                    {
                        Margin = block.Margin,
                        TextAlignment = block.TextAlignment,
                        LineHeight = block.LineHeight
                    };

                    AppendBlockLayouts(doc, paragraph, constraint, documentFormatting, layouts, inheritedLeftIndent, inheritedRightIndent, ref pendingMarkers);
                }
                break;
        }
    }

    private void AppendSectionLayouts(
        Documents.FlowDocument doc,
        Section section,
        double constraint,
        InlineFormatting documentFormatting,
        List<BlockLayoutEntry> layouts,
        double inheritedLeftIndent,
        double inheritedRightIndent,
        ref List<MarkerInfo>? pendingMarkers)
    {
        var startIndex = layouts.Count;
        var childLeftIndent = inheritedLeftIndent + section.Margin.Left;
        var childRightIndent = inheritedRightIndent + section.Margin.Right;

        var markerTemplate = pendingMarkers is { Count: > 0 } existingMarkers
            ? new List<MarkerInfo>(existingMarkers)
            : null;
        var hiddenTemplate = markerTemplate is not null ? CreateHiddenMarkers(markerTemplate) : null;
        var needsVisibleMarkers = markerTemplate is not null;
        var localMarkers = pendingMarkers;

        foreach (var child in section.Blocks)
        {
            List<MarkerInfo>? markersForChild = null;
            if (needsVisibleMarkers && markerTemplate is not null)
            {
                markersForChild = new List<MarkerInfo>(markerTemplate);
            }
            else if (!needsVisibleMarkers && hiddenTemplate is { Count: > 0 })
            {
                markersForChild = new List<MarkerInfo>(hiddenTemplate);
            }

            AppendBlockLayouts(doc, child, constraint, documentFormatting, layouts, childLeftIndent, childRightIndent, ref markersForChild);

            localMarkers = markersForChild;
            needsVisibleMarkers = markersForChild is not null;
        }

        pendingMarkers = localMarkers;

        if (layouts.Count == startIndex)
        {
            return;
        }

        var first = layouts[startIndex].AddMargin(0, section.Margin.Top, 0, 0);
        layouts[startIndex] = first;

        var lastIndex = layouts.Count - 1;
        var last = layouts[lastIndex].AddMargin(0, 0, 0, section.Margin.Bottom);
        layouts[lastIndex] = last;
    }

    private void AppendListLayouts(
        Documents.FlowDocument doc,
        List list,
        double constraint,
        InlineFormatting documentFormatting,
        List<BlockLayoutEntry> layouts,
        double inheritedLeftIndent,
        double inheritedRightIndent,
        ref List<MarkerInfo>? pendingMarkers)
    {
        var startIndex = layouts.Count;
        var childLeftIndent = inheritedLeftIndent + list.Margin.Left;
        var childRightIndent = inheritedRightIndent + list.Margin.Right;

        var outerMarkers = pendingMarkers;
        var index = Math.Max(1, list.StartIndex);

        foreach (var item in list.ListItems)
        {
            var visibleMarkers = CombineMarkersForListItem(doc, list, documentFormatting, index, outerMarkers);
            var hiddenMarkersTemplate = CreateHiddenMarkers(visibleMarkers);
            var needsVisibleMarkers = true;

            foreach (var itemBlock in item.Blocks)
            {
                List<MarkerInfo>? markersForBlock = null;
                if (needsVisibleMarkers)
                {
                    markersForBlock = new List<MarkerInfo>(visibleMarkers);
                }
                else if (hiddenMarkersTemplate is { Count: > 0 })
                {
                    markersForBlock = new List<MarkerInfo>(hiddenMarkersTemplate);
                }

                AppendBlockLayouts(doc, itemBlock, constraint, documentFormatting, layouts, childLeftIndent, childRightIndent, ref markersForBlock);

                if (outerMarkers is not null)
                {
                    outerMarkers = markersForBlock;
                }

                needsVisibleMarkers = markersForBlock is not null;
            }

            if (item.Blocks.Count == 0 && visibleMarkers.Count > 0)
            {
                outerMarkers = new List<MarkerInfo>(visibleMarkers);
            }

            index++;
        }

        pendingMarkers = outerMarkers;

        if (layouts.Count == startIndex)
        {
            return;
        }

        var first = layouts[startIndex].AddMargin(0, list.Margin.Top, 0, 0);
        layouts[startIndex] = first;

        var lastIndex = layouts.Count - 1;
        var last = layouts[lastIndex].AddMargin(0, 0, 0, list.Margin.Bottom);
        layouts[lastIndex] = last;
    }

    private BlockLayoutEntry BuildTableLayout(
        Documents.FlowDocument doc,
        Table table,
        double constraint,
        InlineFormatting documentFormatting,
        double inheritedLeftIndent,
        double inheritedRightIndent)
    {
        var margin = table.Margin;
        margin = new Thickness(
            margin.Left + inheritedLeftIndent,
            margin.Top,
            margin.Right + inheritedRightIndent,
            margin.Bottom);

        var cellSpacing = Math.Max(0, table.CellSpacing);
        var availableWidth = double.IsPositiveInfinity(constraint)
            ? double.PositiveInfinity
            : Math.Max(0, constraint - margin.Left - margin.Right);

        var columnCount = GetMaxTableColumnCount(table);
        if (columnCount == 0)
        {
            var emptyLayout = new TableLayout(
                table,
                cellSpacing * 2,
                cellSpacing * 2,
                cellSpacing,
                Array.Empty<double>(),
                Array.Empty<TableRowLayout>(),
                table.GridLinesBrush,
                table.GridLinesThickness);
            return BlockLayoutEntry.ForTable(table, margin, emptyLayout);
        }

        var columnWidths = new double[columnCount];
        var columnIsFixed = new bool[columnCount];
        var desiredAutoWidths = new double[columnCount];

        var fixedCount = 0;
        for (var i = 0; i < columnCount && i < table.Columns.Count; i++)
        {
            var width = table.Columns[i].Width;
            if (!double.IsNaN(width) && width > 0)
            {
                columnWidths[i] = Math.Max(MinimumColumnWidth, width);
                columnIsFixed[i] = true;
                fixedCount++;
            }
        }

        var tableFormatting = DeriveFormatting(table, documentFormatting);

        foreach (var group in table.RowGroups)
        {
            foreach (var row in group.Rows)
            {
                for (var columnIndex = 0; columnIndex < columnCount; columnIndex++)
                {
                    if (columnIsFixed[columnIndex])
                    {
                        continue;
                    }

                    TableCell? cell = columnIndex < row.Cells.Count ? row.Cells[columnIndex] : null;
                    if (cell is null)
                    {
                        continue;
                    }

                    var desiredWidth = MeasureCellDesiredWidth(doc, cell, tableFormatting);
                    desiredAutoWidths[columnIndex] = Math.Max(desiredAutoWidths[columnIndex], Math.Max(MinimumColumnWidth, desiredWidth));
                }
            }
        }

        for (var i = 0; i < columnCount; i++)
        {
            if (!columnIsFixed[i])
            {
                columnWidths[i] = desiredAutoWidths[i] > 0
                    ? desiredAutoWidths[i]
                    : MinimumColumnWidth;
            }
        }

        var spacingWidth = cellSpacing * (columnCount + 1);
        var widthForColumns = double.IsPositiveInfinity(availableWidth)
            ? double.PositiveInfinity
            : Math.Max(0, availableWidth - spacingWidth);

        var totalFixed = 0d;
        for (var i = 0; i < columnCount; i++)
        {
            if (columnIsFixed[i])
            {
                totalFixed += columnWidths[i];
            }
        }

        if (!double.IsPositiveInfinity(widthForColumns) && totalFixed > widthForColumns && totalFixed > 0)
        {
            var scale = widthForColumns / totalFixed;
            for (var i = 0; i < columnCount; i++)
            {
                if (columnIsFixed[i])
                {
                    columnWidths[i] = Math.Max(MinimumColumnWidth, columnWidths[i] * scale);
                }
            }

            totalFixed = Math.Min(totalFixed, widthForColumns);
        }

        var autoCount = columnCount - fixedCount;
        if (autoCount > 0)
        {
            var autoWidthSum = 0d;
            for (var i = 0; i < columnCount; i++)
            {
                if (!columnIsFixed[i])
                {
                    autoWidthSum += columnWidths[i];
                }
            }

            if (double.IsPositiveInfinity(widthForColumns))
            {
                if (autoWidthSum <= 0)
                {
                    var defaultWidth = MinimumColumnWidth * 2;
                    for (var i = 0; i < columnCount; i++)
                    {
                        if (!columnIsFixed[i])
                        {
                            columnWidths[i] = defaultWidth;
                        }
                    }
                }
            }
            else
            {
                var remaining = Math.Max(0, widthForColumns - totalFixed);
                if (autoWidthSum <= 0)
                {
                    var widthPerColumn = autoCount > 0 ? remaining / autoCount : 0;
                    widthPerColumn = Math.Max(MinimumColumnWidth, widthPerColumn);
                    for (var i = 0; i < columnCount; i++)
                    {
                        if (!columnIsFixed[i])
                        {
                            columnWidths[i] = widthPerColumn;
                        }
                    }
                }
                else
                {
                    var scale = remaining > 0 ? remaining / autoWidthSum : 0;
                    for (var i = 0; i < columnCount; i++)
                    {
                        if (!columnIsFixed[i])
                        {
                            var width = columnWidths[i] * scale;
                            columnWidths[i] = Math.Max(MinimumColumnWidth, width);
                        }
                    }
                }
            }
        }

        var rows = new List<TableRowLayout>();
        var totalHeight = cellSpacing;

        foreach (var group in table.RowGroups)
        {
            var groupMargin = group.Margin;
            totalHeight += groupMargin.Top;

            foreach (var row in group.Rows)
            {
                var cellLayouts = new List<TableCellLayout>(columnCount);
                var rowHeight = 0d;

                for (var columnIndex = 0; columnIndex < columnCount; columnIndex++)
                {
                    var columnWidth = columnWidths[columnIndex];
                    TableCell? cell = columnIndex < row.Cells.Count ? row.Cells[columnIndex] : null;

                    IReadOnlyList<BlockLayoutEntry> cellEntries;
                    Size contentSize;
                    Thickness padding;

                    Thickness borderThickness;
                    IBrush? borderBrush;

                    if (cell is null)
                    {
                        cellEntries = Array.Empty<BlockLayoutEntry>();
                        contentSize = new Size(0, 0);
                        padding = new Thickness(0);
                        borderThickness = new Thickness(0);
                        borderBrush = null;
                    }
                    else
                    {
                        var constraintForCell = double.IsPositiveInfinity(columnWidth)
                            ? double.PositiveInfinity
                            : Math.Max(0, columnWidth - cell.Padding.Left - cell.Padding.Right);

                        cellEntries = BuildCellLayouts(doc, cell, constraintForCell, tableFormatting);
                        contentSize = CalculateExtent(cellEntries);
                        padding = cell.Padding;
                        borderThickness = cell.BorderThickness;
                        borderBrush = cell.BorderBrush;
                    }

                    var cellLayout = new TableCellLayout(cell, cellEntries, contentSize, columnWidth, padding, borderThickness, borderBrush);
                    cellLayouts.Add(cellLayout);
                    rowHeight = Math.Max(rowHeight, cellLayout.DesiredHeight);
                }

                var rowLayout = new TableRowLayout(rowHeight, cellLayouts.ToArray());
                rows.Add(rowLayout);
                totalHeight += rowHeight + cellSpacing;
            }

            totalHeight += groupMargin.Bottom;
        }

        if (rows.Count == 0)
        {
            totalHeight = cellSpacing * 2;
        }

        var totalWidth = spacingWidth;
        for (var i = 0; i < columnCount; i++)
        {
            totalWidth += columnWidths[i];
        }

        var tableLayout = new TableLayout(
            table,
            totalWidth,
            totalHeight,
            cellSpacing,
            Array.AsReadOnly(columnWidths),
            rows.ToArray(),
            table.GridLinesBrush,
            table.GridLinesThickness);
    return BlockLayoutEntry.ForTable(table, margin, tableLayout);
    }

    private List<MarkerInfo> CombineMarkersForListItem(
        Documents.FlowDocument doc,
        List list,
        InlineFormatting documentFormatting,
        int index,
        List<MarkerInfo>? outerMarkers)
    {
        var marker = CreateListMarkerInfo(doc, list, index, documentFormatting);

        if (outerMarkers is null || outerMarkers.Count == 0)
        {
            return new List<MarkerInfo> { marker };
        }

        var combined = new List<MarkerInfo>(outerMarkers.Count + 1);
        combined.AddRange(outerMarkers);
        combined.Add(marker);
        return combined;
    }

    private static List<MarkerInfo>? CreateHiddenMarkers(IReadOnlyList<MarkerInfo> markers)
    {
        if (markers.Count == 0)
        {
            return null;
        }

        var hidden = new List<MarkerInfo>(markers.Count);
        foreach (var marker in markers)
        {
            hidden.Add(marker.AsHidden());
        }

        return hidden;
    }

    private static int GetMaxTableColumnCount(Table table)
    {
        var max = table.Columns.Count;

        foreach (var group in table.RowGroups)
        {
            foreach (var row in group.Rows)
            {
                if (row.Cells.Count > max)
                {
                    max = row.Cells.Count;
                }
            }
        }

        return max;
    }

    private double MeasureCellDesiredWidth(Documents.FlowDocument doc, TableCell cell, InlineFormatting tableFormatting)
    {
        var entries = BuildCellLayouts(doc, cell, double.PositiveInfinity, tableFormatting);
        var extent = CalculateExtent(entries);
        return extent.Width + cell.Padding.Left + cell.Padding.Right;
    }

    private IReadOnlyList<BlockLayoutEntry> BuildCellLayouts(
        Documents.FlowDocument doc,
        TableCell cell,
        double constraint,
        InlineFormatting tableFormatting)
    {
        var effectiveConstraint = constraint > 0 ? constraint : 0;
        var entries = new List<BlockLayoutEntry>();
        var cellFormatting = DeriveFormatting(cell, tableFormatting);

        foreach (var block in cell.Blocks)
        {
            List<MarkerInfo>? markers = null;
            AppendBlockLayouts(doc, block, effectiveConstraint, cellFormatting, entries, 0, 0, ref markers);
        }

        return entries.Count == 0 ? Array.Empty<BlockLayoutEntry>() : entries.ToArray();
    }

    private static Size CalculateExtent(IReadOnlyList<BlockLayoutEntry> entries)
    {
        double width = 0;
        double height = 0;

        foreach (var entry in entries)
        {
            var entryWidth = entry.Margin.Left + entry.ContentIndent + entry.ContentWidth + entry.Margin.Right;
            width = Math.Max(width, entryWidth);
            height += entry.Margin.Top + entry.ContentHeight + entry.Margin.Bottom;
        }

        return new Size(width, height);
    }

    private MarkerInfo CreateListMarkerInfo(
        Documents.FlowDocument doc,
        List list,
        int index,
        InlineFormatting documentFormatting)
    {
        var markerFormatting = DeriveFormatting(list, documentFormatting);
        var text = list.MarkerStyle switch
        {
            ListMarkerStyle.Decimal => string.Format(CultureInfo.CurrentCulture, "{0}.", index),
            _ => "•"
        };

        var markerTypeface = new Typeface(markerFormatting.FontFamily, markerFormatting.FontStyle, markerFormatting.FontWeight, markerFormatting.FontStretch);
        var foreground = markerFormatting.Foreground ?? doc.Foreground ?? Brushes.Black;
        var decorations = markerFormatting.TextDecorations ?? new TextDecorationCollection();

        var layout = new TextLayout(
            text,
            markerTypeface,
            fontSize: markerFormatting.FontSize,
            foreground: foreground,
            textAlignment: TextAlignment.Left,
            textWrapping: TextWrapping.NoWrap,
            textTrimming: TextTrimming.None,
            textDecorations: decorations,
            flowDirection: doc.FlowDirection,
            maxWidth: double.PositiveInfinity,
            maxHeight: double.PositiveInfinity,
            lineHeight: double.NaN,
            letterSpacing: 0,
            maxLines: 1);

        var reservedWidth = Math.Max(list.MarkerOffset, layout.Width + MarkerPadding);
        return new MarkerInfo(layout, reservedWidth, true);
    }

    private IReadOnlyList<BlockLayoutEntry> BuildParagraphLayouts(
        Documents.FlowDocument doc,
        Paragraph paragraph,
        double constraint,
        double inheritedLeftIndent,
        double inheritedRightIndent,
        MarkerInfo[]? markers)
    {
        var paragraphMargin = paragraph.Margin;
        paragraphMargin = new Thickness(
            paragraphMargin.Left + inheritedLeftIndent,
            paragraphMargin.Top,
            paragraphMargin.Right + inheritedRightIndent,
            paragraphMargin.Bottom);

        var baseFormatting = CreateParagraphFormatting(doc, paragraph);

        var entries = new List<BlockLayoutEntry>();
        var buffer = new StringBuilder();
        var spans = new List<ValueSpan<TextRunProperties>>();
        var hasEmittedText = false;

        var visibleMarkers = markers;
        var hiddenMarkers = CreateHiddenMarkerArray(markers);

        double markerIndent = 0;
        if (markers is { Length: > 0 })
        {
            foreach (var marker in markers)
            {
                markerIndent += marker.Width;
            }
        }

        var textConstraint = double.IsPositiveInfinity(constraint)
            ? double.PositiveInfinity
            : Math.Max(0, constraint - paragraphMargin.Left - paragraphMargin.Right - markerIndent);

        var baseMargin = new Thickness(paragraphMargin.Left, 0, paragraphMargin.Right, 0);

        foreach (var inline in paragraph.Inlines)
        {
            if (inline is AnchoredBlock anchored)
            {
                FlushTextSegment();

                var anchoredMarkers = hasEmittedText ? hiddenMarkers : hiddenMarkers;
                var anchoredEntry = BuildAnchoredBlockEntry(doc, anchored, constraint, baseMargin, baseFormatting, anchoredMarkers);
                entries.Add(anchoredEntry);
                continue;
            }

            AppendInline(inline, baseFormatting, buffer, spans);
        }

        FlushTextSegment();

        if (entries.Count == 0)
        {
            var paragraphDecorations = CombineDecorations(doc.TextDecorations, paragraph.TextDecorations);
            var resolvedLineHeight = paragraph.LineHeight ?? (double.IsNaN(doc.LineHeight) ? double.NaN : doc.LineHeight);
            var textAlignment = paragraph.TextAlignment ?? doc.TextAlignment;

            var defaultTypeface = new Typeface(baseFormatting.FontFamily, baseFormatting.FontStyle, baseFormatting.FontWeight, baseFormatting.FontStretch);
            var defaultForeground = baseFormatting.Foreground ?? doc.Foreground ?? Brushes.Black;

            var placeholderLayout = new TextLayout(
                string.Empty,
                defaultTypeface,
                fontSize: baseFormatting.FontSize,
                foreground: defaultForeground,
                textAlignment: textAlignment,
                textWrapping: TextWrapping.Wrap,
                textTrimming: TextTrimming.None,
                textDecorations: paragraphDecorations ?? new TextDecorationCollection(),
                flowDirection: doc.FlowDirection,
                maxWidth: textConstraint,
                maxHeight: double.PositiveInfinity,
                lineHeight: resolvedLineHeight,
                letterSpacing: 0,
                maxLines: int.MaxValue);

            var entryMarkers = visibleMarkers;
            var entry = new BlockLayoutEntry(paragraph, placeholderLayout, baseMargin, entryMarkers);
            entries.Add(entry);
            hasEmittedText = true;
        }

        entries[0] = entries[0].AddMargin(0, paragraphMargin.Top, 0, 0);
        entries[^1] = entries[^1].AddMargin(0, 0, 0, paragraphMargin.Bottom);

        return entries;

        void FlushTextSegment()
        {
            if (buffer.Length == 0)
            {
                return;
            }

            var text = buffer.ToString();
            var paragraphDecorations = CombineDecorations(doc.TextDecorations, paragraph.TextDecorations);
            var resolvedLineHeight = paragraph.LineHeight ?? (double.IsNaN(doc.LineHeight) ? double.NaN : doc.LineHeight);
            var textAlignment = paragraph.TextAlignment ?? doc.TextAlignment;

            var defaultTypeface = new Typeface(baseFormatting.FontFamily, baseFormatting.FontStyle, baseFormatting.FontWeight, baseFormatting.FontStretch);
            var defaultForeground = baseFormatting.Foreground ?? doc.Foreground ?? Brushes.Black;

            var overrides = spans.Count > 0 ? spans.ToArray() : Array.Empty<ValueSpan<TextRunProperties>>();

            var layout = new TextLayout(
                text,
                defaultTypeface,
                fontSize: baseFormatting.FontSize,
                foreground: defaultForeground,
                textAlignment: textAlignment,
                textWrapping: TextWrapping.Wrap,
                textTrimming: TextTrimming.None,
                textDecorations: paragraphDecorations ?? new TextDecorationCollection(),
                flowDirection: doc.FlowDirection,
                maxWidth: textConstraint,
                maxHeight: double.PositiveInfinity,
                lineHeight: resolvedLineHeight,
                letterSpacing: 0,
                maxLines: int.MaxValue,
                textStyleOverrides: overrides);

            var entryMarkers = !hasEmittedText ? visibleMarkers : hiddenMarkers;
            var entry = new BlockLayoutEntry(paragraph, layout, baseMargin, entryMarkers);
            entries.Add(entry);

            hasEmittedText = true;
            buffer.Clear();
            spans.Clear();
        }
    }

    private BlockLayoutEntry BuildAnchoredBlockEntry(
        Documents.FlowDocument doc,
        AnchoredBlock anchored,
        double constraint,
        Thickness paragraphMargin,
        InlineFormatting paragraphFormatting,
        MarkerInfo[]? markers)
    {
        var anchoredMargin = anchored.Margin;
        var combinedMargin = new Thickness(
            paragraphMargin.Left + anchoredMargin.Left,
            anchoredMargin.Top,
            paragraphMargin.Right + anchoredMargin.Right,
            anchoredMargin.Bottom);

        var availableWidth = double.IsPositiveInfinity(constraint)
            ? double.PositiveInfinity
            : Math.Max(0, constraint - combinedMargin.Left - combinedMargin.Right);

        if (!double.IsPositiveInfinity(availableWidth) && markers is { Length: > 0 })
        {
            double indent = 0;
            foreach (var marker in markers)
            {
                indent += marker.Width;
            }

            availableWidth = Math.Max(0, availableWidth - indent);
        }

        var anchoredFormatting = DeriveFormatting(anchored, paragraphFormatting);
        var layout = BuildAnchoredBlockLayout(doc, anchored, availableWidth, anchoredFormatting);

        return BlockLayoutEntry.ForAnchoredBlock(anchored, combinedMargin, layout, markers);
    }

    private AnchoredBlockLayout BuildAnchoredBlockLayout(
        Documents.FlowDocument doc,
        AnchoredBlock anchored,
        double availableWidth,
        InlineFormatting formatting)
    {
        var padding = anchored.Padding;
        var contentAvailableWidth = double.IsPositiveInfinity(availableWidth)
            ? double.PositiveInfinity
            : Math.Max(0, availableWidth - padding.Left - padding.Right);

        var resolvedWidth = ResolveAnchoredWidth(anchored, availableWidth);

        double childConstraint = contentAvailableWidth;
        if (!double.IsNaN(resolvedWidth))
        {
            var innerTarget = Math.Max(0, resolvedWidth - padding.Left - padding.Right);
            if (!double.IsPositiveInfinity(innerTarget))
            {
                childConstraint = Math.Min(contentAvailableWidth, innerTarget);
            }
        }

        var contentEntries = new List<BlockLayoutEntry>();
        foreach (var block in anchored.Blocks)
        {
            List<MarkerInfo>? markers = null;
            AppendBlockLayouts(doc, block, childConstraint, formatting, contentEntries, 0, 0, ref markers);
        }

        var contentExtent = contentEntries.Count == 0 ? new Size(0, 0) : CalculateExtent(contentEntries);

        var contentWidth = contentExtent.Width;
        if (!double.IsNaN(childConstraint) && !double.IsPositiveInfinity(childConstraint))
        {
            contentWidth = Math.Min(contentWidth, childConstraint);
        }

        var finalContentWidth = contentWidth;
        if (!double.IsNaN(resolvedWidth))
        {
            var innerWidth = Math.Max(0, resolvedWidth - padding.Left - padding.Right);
            finalContentWidth = Math.Max(contentWidth, innerWidth);
        }

        var totalWidth = finalContentWidth + padding.Left + padding.Right;
        if (!double.IsNaN(resolvedWidth))
        {
            totalWidth = Math.Max(totalWidth, resolvedWidth);
        }

        var contentHeight = contentExtent.Height;
        var totalHeight = contentHeight + padding.Top + padding.Bottom;

        var resolvedHeight = ResolveAnchoredHeight(anchored, double.PositiveInfinity);
        if (!double.IsNaN(resolvedHeight))
        {
            totalHeight = Math.Max(totalHeight, resolvedHeight);
        }

        var alignment = ResolveAnchoredHorizontalAlignment(anchored);
        var horizontalOffset = ResolveAnchoredHorizontalOffset(anchored);
        var verticalOffset = ResolveAnchoredVerticalOffset(anchored);

        return new AnchoredBlockLayout(
            anchored,
            contentEntries.Count == 0 ? Array.Empty<BlockLayoutEntry>() : contentEntries.ToArray(),
            contentExtent,
            padding,
            anchored.Background,
            totalWidth,
            totalHeight,
            availableWidth,
            alignment,
            horizontalOffset,
            verticalOffset);
    }

    private double ResolveAnchoredWidth(AnchoredBlock anchored, double availableWidth)
    {
        return anchored switch
        {
            Figure figure => ResolveFigureLength(figure.Width, availableWidth),
            Floater floater => double.IsNaN(floater.Width) ? double.NaN : floater.Width,
            _ => double.NaN
        };
    }

    private double ResolveAnchoredHeight(AnchoredBlock anchored, double availableHeight)
    {
        return anchored switch
        {
            Figure figure => ResolveFigureLength(figure.Height, availableHeight),
            Floater floater => double.IsNaN(floater.Height) ? double.NaN : floater.Height,
            _ => double.NaN
        };
    }

    private AnchoredHorizontalAlignment ResolveAnchoredHorizontalAlignment(AnchoredBlock anchored)
    {
        return anchored switch
        {
            Figure figure => figure.HorizontalAnchor switch
            {
                FigureHorizontalAnchor.PageCenter or FigureHorizontalAnchor.ContentCenter or FigureHorizontalAnchor.ColumnCenter => AnchoredHorizontalAlignment.Center,
                FigureHorizontalAnchor.PageRight or FigureHorizontalAnchor.ContentRight or FigureHorizontalAnchor.ColumnRight => AnchoredHorizontalAlignment.Right,
                _ => AnchoredHorizontalAlignment.Left
            },
            Floater floater => floater.HorizontalAlignment switch
            {
                HorizontalAlignment.Center => AnchoredHorizontalAlignment.Center,
                HorizontalAlignment.Right => AnchoredHorizontalAlignment.Right,
                _ => AnchoredHorizontalAlignment.Left
            },
            _ => AnchoredHorizontalAlignment.Left
        };
    }

    private double ResolveAnchoredHorizontalOffset(AnchoredBlock anchored)
    {
        return anchored switch
        {
            Figure figure => figure.HorizontalOffset,
            _ => 0
        };
    }

    private double ResolveAnchoredVerticalOffset(AnchoredBlock anchored)
    {
        return anchored switch
        {
            Figure figure => figure.VerticalOffset,
            _ => 0
        };
    }

    private double ResolveFigureLength(FigureLength length, double availableSize)
    {
        return length.Unit switch
        {
            FigureUnitType.Auto => double.NaN,
            FigureUnitType.Pixel => length.Value,
            FigureUnitType.Content or FigureUnitType.Page or FigureUnitType.Column => double.IsPositiveInfinity(availableSize) ? double.NaN : Math.Max(0, availableSize),
            _ => double.NaN
        };
    }

    private static InlineFormatting CreateDocumentFormatting(Documents.FlowDocument doc)
    {
        return new InlineFormatting(
            doc.FontFamily,
            doc.FontSize,
            doc.FontStyle,
            doc.FontWeight,
            doc.FontStretch,
            doc.Foreground,
            doc.Background,
            doc.TextDecorations);
    }

    private static InlineFormatting CreateParagraphFormatting(Documents.FlowDocument doc, Paragraph paragraph)
    {
        return DeriveFormatting(paragraph, CreateDocumentFormatting(doc));
    }

    private static InlineFormatting DeriveFormatting(Documents.TextElement element, InlineFormatting parent)
    {
        return new InlineFormatting(
            element.FontFamily ?? parent.FontFamily,
            element.FontSize ?? parent.FontSize,
            element.FontStyle ?? parent.FontStyle,
            element.FontWeight ?? parent.FontWeight,
            element.FontStretch ?? parent.FontStretch,
            element.Foreground ?? parent.Foreground,
            element.Background ?? parent.Background,
            CombineDecorations(parent.TextDecorations, element.TextDecorations));
    }

    private static TextDecorationCollection? CombineDecorations(TextDecorationCollection? parent, TextDecorationCollection? current)
    {
        if (current is null || current.Count == 0)
        {
            return parent;
        }

        if (parent is null || parent.Count == 0)
        {
            return new TextDecorationCollection(current);
        }

        var combined = new TextDecorationCollection(parent);
        foreach (var decoration in current)
        {
            combined.Add(decoration);
        }

        return combined;
    }

    private static MarkerInfo[]? CreateHiddenMarkerArray(MarkerInfo[]? markers)
    {
        if (markers is null || markers.Length == 0)
        {
            return null;
        }

        var hidden = new MarkerInfo[markers.Length];
        for (var i = 0; i < markers.Length; i++)
        {
            hidden[i] = markers[i].AsHidden();
        }

        return hidden;
    }

    private static void AppendInlineRuns(IEnumerable<Documents.Inline> inlines, InlineFormatting formatting, StringBuilder buffer, List<ValueSpan<TextRunProperties>> spans)
    {
        foreach (var inline in inlines)
        {
            AppendInline(inline, formatting, buffer, spans);
        }
    }

    private static void AppendInline(Documents.Inline inline, InlineFormatting parentFormatting, StringBuilder buffer, List<ValueSpan<TextRunProperties>> spans)
    {
        var formatting = DeriveFormatting(inline, parentFormatting);

        switch (inline)
        {
            case Documents.Run run:
                AppendRunText(run.Text ?? string.Empty, formatting, buffer, spans);
                break;
            case Documents.Span span:
                AppendInlineRuns(span.Inlines, formatting, buffer, spans);
                break;
            case Documents.LineBreak:
                AppendRunText("\n", formatting, buffer, spans);
                break;
            default:
                var fallback = new StringBuilder();
                inline.AppendPlainText(fallback);
                if (fallback.Length > 0)
                {
                    AppendRunText(fallback.ToString(), formatting, buffer, spans);
                }
                break;
        }
    }

    private static void AppendRunText(string text, InlineFormatting formatting, StringBuilder buffer, List<ValueSpan<TextRunProperties>> spans)
    {
        if (string.IsNullOrEmpty(text))
        {
            return;
        }

        var start = buffer.Length;
        buffer.Append(text);

        var decorations = formatting.TextDecorations is { Count: > 0 }
            ? new TextDecorationCollection(formatting.TextDecorations)
            : formatting.TextDecorations;

        var typeface = new Typeface(formatting.FontFamily, formatting.FontStyle, formatting.FontWeight, formatting.FontStretch);
        var runProperties = new GenericTextRunProperties(
            typeface,
            formatting.FontSize,
            decorations,
            formatting.Foreground,
            formatting.Background);

        spans.Add(new ValueSpan<TextRunProperties>(start, text.Length, runProperties));
    }

    protected void RenderBlockSequence(DrawingContext context, Point origin, IReadOnlyList<BlockLayoutEntry> entries)
    {
        var baseX = origin.X;
        var y = origin.Y;

        foreach (var entry in entries)
        {
            y += entry.Margin.Top;

            if (entry.TableLayout is { } tableLayout)
            {
                RenderTable(context, new Point(baseX + entry.Margin.Left, y), tableLayout);
                y += tableLayout.Height + entry.Margin.Bottom;
                continue;
            }

            if (entry.AnchoredLayout is { } anchoredLayout)
            {
                var anchoredX = baseX + entry.Margin.Left + entry.ContentIndent;

                if (!double.IsPositiveInfinity(anchoredLayout.AvailableWidth) && anchoredLayout.AvailableWidth > anchoredLayout.Width)
                {
                    var remaining = anchoredLayout.AvailableWidth - anchoredLayout.Width;
                    switch (anchoredLayout.HorizontalAlignment)
                    {
                        case AnchoredHorizontalAlignment.Center:
                            anchoredX += remaining / 2;
                            break;
                        case AnchoredHorizontalAlignment.Right:
                            anchoredX += remaining;
                            break;
                    }
                }

                anchoredX += anchoredLayout.HorizontalOffset;

                var blockY = y + anchoredLayout.VerticalOffset;
                var rect = new Rect(new Point(anchoredX, blockY), new Size(anchoredLayout.Width, anchoredLayout.Height));

                if (anchoredLayout.Background is not null)
                {
                    context.FillRectangle(anchoredLayout.Background, rect);
                }

                var contentOrigin = new Point(rect.X + anchoredLayout.Padding.Left, rect.Y + anchoredLayout.Padding.Top);
                RenderBlockSequence(context, contentOrigin, anchoredLayout.Content);

                y = rect.Y + anchoredLayout.Height + entry.Margin.Bottom;
                continue;
            }

            var layout = entry.Layout;
            if (layout is null)
            {
                y += entry.Margin.Bottom;
                continue;
            }

            var contentX = baseX + entry.Margin.Left;

            if (entry.Markers is { Length: > 0 } markers)
            {
                foreach (var marker in markers)
                {
                    var areaWidth = marker.Width;
                    var markerX = contentX;
                    var available = areaWidth - marker.Layout.Width;
                    if (!double.IsNaN(available) && available > MarkerPadding)
                    {
                        markerX += available - MarkerPadding;
                    }

                    if (marker.IsVisible)
                    {
                        var markerY = y + Math.Max(0, (layout.Height - marker.Layout.Height) / 2);
                        marker.Layout.Draw(context, new Point(markerX, markerY));
                    }

                    contentX += areaWidth;
                }
            }

            layout.Draw(context, new Point(contentX, y));
            y += layout.Height + entry.Margin.Bottom;
        }
    }

    internal IReadOnlyList<TextBlockVisualInfo> GetTextBlockVisuals()
    {
        var constraint = Bounds.Size;
        if (constraint.Width <= 0 || double.IsNaN(constraint.Width))
        {
            constraint = new Size(double.PositiveInfinity, constraint.Height);
        }
        if (constraint.Height <= 0 || double.IsNaN(constraint.Height))
        {
            constraint = new Size(constraint.Width, double.PositiveInfinity);
        }
        EnsureLayouts(constraint);

        if (_currentDocument is null || _layouts.Count == 0)
        {
            return Array.Empty<TextBlockVisualInfo>();
        }

        var padding = _currentDocument.PagePadding;
        var baseX = padding.Left;
        var y = padding.Top;
        var visuals = new List<TextBlockVisualInfo>(_layouts.Count);

        foreach (var entry in _layouts)
        {
            var blockTop = y + entry.Margin.Top;

            if (entry.TableLayout is { } table)
            {
                y = blockTop + table.Height + entry.Margin.Bottom;
                continue;
            }

            if (entry.AnchoredLayout is { } anchored)
            {
                var anchoredTop = blockTop + anchored.VerticalOffset;
                y = anchoredTop + anchored.Height + entry.Margin.Bottom;
                continue;
            }

            var layout = entry.Layout;
            if (layout is null)
            {
                y = blockTop + entry.Margin.Bottom;
                continue;
            }

            var contentX = baseX + entry.Margin.Left;
            var markerIndent = 0d;
            if (entry.Markers is { Length: > 0 } markers)
            {
                foreach (var marker in markers)
                {
                    markerIndent += marker.Width;
                }
            }

            var textOrigin = new Point(contentX + markerIndent, blockTop);
            var textBounds = new Rect(textOrigin, new Size(layout.Width, layout.Height));
            var textBottom = blockTop + layout.Height;
            var bottom = textBottom + entry.Margin.Bottom;

            visuals.Add(new TextBlockVisualInfo(
                entry.Block!,
                layout,
                textOrigin,
                textBounds,
                y,
                blockTop,
                textBottom,
                bottom,
                contentX,
                contentX + markerIndent + layout.Width,
                markerIndent,
                entry.Margin));

            y = bottom;
        }

        return visuals;
    }

    private void RenderTable(DrawingContext context, Point origin, TableLayout layout)
    {
        var tableRect = new Rect(origin, new Size(layout.Width, layout.Height));
        var tableBackground = layout.Table.Background;
        if (tableBackground is not null)
        {
            context.FillRectangle(tableBackground, tableRect);
        }

        var y = origin.Y + layout.CellSpacing;
        var gridLinesBrush = layout.GridLinesBrush;
        var uniformGridThickness = Math.Max(0, layout.GridLinesThickness);

        foreach (var row in layout.Rows)
        {
            var x = origin.X + layout.CellSpacing;

            foreach (var cell in row.Cells)
            {
                var cellRect = new Rect(new Point(x, y), new Size(cell.ColumnWidth, row.Height));

                var cellBackground = cell.Cell?.Background;
                if (cellBackground is not null)
                {
                    context.FillRectangle(cellBackground, cellRect);
                }

                var padding = cell.Padding;
                var contentOrigin = new Point(cellRect.X + padding.Left, cellRect.Y + padding.Top);
                RenderBlockSequence(context, contentOrigin, cell.Content);

                var borderBrush = cell.BorderBrush ?? gridLinesBrush;
                var borderThickness = NormalizeBorderThickness(cell.BorderThickness, uniformGridThickness);

                if (borderBrush is not null && HasBorder(borderThickness))
                {
                    DrawCellBorder(context, cellRect, borderThickness, borderBrush);
                }

                x += cell.ColumnWidth + layout.CellSpacing;
            }

            y += row.Height + layout.CellSpacing;
        }
    }

    private static Thickness NormalizeBorderThickness(Thickness thickness, double uniformFallback)
    {
        if (HasBorder(thickness))
        {
            return thickness;
        }

        return uniformFallback > 0 ? new Thickness(uniformFallback) : new Thickness(0);
    }

    private static bool HasBorder(Thickness thickness)
    {
        return thickness.Left > 0 || thickness.Top > 0 || thickness.Right > 0 || thickness.Bottom > 0;
    }

    private static void DrawCellBorder(DrawingContext context, Rect rect, Thickness thickness, IBrush brush)
    {
        if (thickness.Left > 0)
        {
            var width = Math.Min(thickness.Left, rect.Width);
            if (width > 0)
            {
                context.FillRectangle(brush, new Rect(rect.X, rect.Y, width, rect.Height));
            }
        }

        if (thickness.Top > 0)
        {
            var height = Math.Min(thickness.Top, rect.Height);
            if (height > 0)
            {
                context.FillRectangle(brush, new Rect(rect.X, rect.Y, rect.Width, height));
            }
        }

        if (thickness.Right > 0)
        {
            var width = Math.Min(thickness.Right, rect.Width);
            if (width > 0)
            {
                context.FillRectangle(brush, new Rect(rect.X + rect.Width - width, rect.Y, width, rect.Height));
            }
        }

        if (thickness.Bottom > 0)
        {
            var height = Math.Min(thickness.Bottom, rect.Height);
            if (height > 0)
            {
                context.FillRectangle(brush, new Rect(rect.X, rect.Y + rect.Height - height, rect.Width, height));
            }
        }
    }

    internal readonly struct TextBlockVisualInfo
    {
        public TextBlockVisualInfo(
            Block block,
            TextLayout layout,
            Point textOrigin,
            Rect textBounds,
            double top,
            double textTop,
            double textBottom,
            double bottom,
            double contentLeft,
            double contentRight,
            double markerIndent,
            Thickness margin)
        {
            Block = block;
            Layout = layout;
            TextOrigin = textOrigin;
            TextBounds = textBounds;
            Top = top;
            TextTop = textTop;
            TextBottom = textBottom;
            Bottom = bottom;
            ContentLeft = contentLeft;
            ContentRight = contentRight;
            MarkerIndent = markerIndent;
            Margin = margin;
        }

        public Block Block { get; }
        public TextLayout Layout { get; }
        public Point TextOrigin { get; }
        public Rect TextBounds { get; }
        public double Top { get; }
        public double TextTop { get; }
        public double TextBottom { get; }
        public double Bottom { get; }
        public double ContentLeft { get; }
        public double ContentRight { get; }
        public double MarkerIndent { get; }
        public Thickness Margin { get; }
    }

    private readonly record struct InlineFormatting(
        FontFamily FontFamily,
        double FontSize,
        FontStyle FontStyle,
        FontWeight FontWeight,
        FontStretch FontStretch,
        IBrush? Foreground,
        IBrush? Background,
        TextDecorationCollection? TextDecorations);
}
