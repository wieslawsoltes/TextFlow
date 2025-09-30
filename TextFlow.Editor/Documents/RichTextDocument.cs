using System.Linq;
using System.Text;
using Avalonia;
using Avalonia.Media;
using FlowDocumentElement = TextFlow.Document.Documents.FlowDocument;
using FlowParagraph = TextFlow.Document.Documents.Paragraph;
using FlowRun = TextFlow.Document.Documents.Run;
using FlowTable = TextFlow.Document.Documents.Table;
using FlowTableCell = TextFlow.Document.Documents.TableCell;
using FlowTableColumn = TextFlow.Document.Documents.TableColumn;
using FlowTableRow = TextFlow.Document.Documents.TableRow;
using FlowTableRowGroup = TextFlow.Document.Documents.TableRowGroup;

namespace TextFlow.Editor.Documents;

/// <summary>
/// Represents a mutable rich text document made of styled spans.
/// </summary>
public sealed class RichTextDocument
{
    private readonly List<StyledTextSpan> _spans = new();
    private readonly SortedDictionary<int, ParagraphProperties> _paragraphProperties = new();
    private readonly List<TableRange> _tableRanges = new();

    public event EventHandler? Changed;

    internal IReadOnlyList<StyledTextSpan> Spans => _spans;

    internal IReadOnlyList<TableRange> TableRanges => _tableRanges;

    public int Length { get; private set; }

    public string Text { get; private set; } = string.Empty;

    public bool IsEmpty => Length == 0;

    public RichTextDocument()
    {
        UpdateCachedValues();
    }

    public RichTextDocument Clone()
    {
        var clone = new RichTextDocument();
        clone._spans.Clear();
        clone._spans.AddRange(_spans.Select(s => s.Clone()));
        clone._paragraphProperties.Clear();
        foreach (var pair in _paragraphProperties)
        {
            clone._paragraphProperties[pair.Key] = pair.Value.Clone();
        }

        clone._tableRanges.Clear();
        foreach (var table in _tableRanges)
        {
            clone._tableRanges.Add(table.Clone());
        }

        clone.UpdateCachedValues();
        return clone;
    }

    public void Clear()
    {
        if (_spans.Count == 0)
        {
            return;
        }

        _spans.Clear();
        _paragraphProperties.Clear();
        _tableRanges.Clear();
        UpdateCachedValues();
        OnChanged();
    }

    public void SetText(string text, RichTextStyle? style = null)
    {
        _spans.Clear();
        _paragraphProperties.Clear();
        _tableRanges.Clear();
        text = NormalizeNewLines(text);

        if (!string.IsNullOrEmpty(text))
        {
            _spans.Add(new StyledTextSpan(text, style ?? RichTextStyle.Default));
        }

        UpdateCachedValues();
        OnChanged();
    }

    public void InsertText(int offset, string text, RichTextStyle style, bool suppressChanged = false)
    {
        ArgumentNullException.ThrowIfNull(text);
        ValidateOffset(offset, allowEqual: true);

        text = NormalizeNewLines(text);
        if (text.Length == 0)
        {
            return;
        }

        if (_spans.Count == 0)
        {
            _spans.Add(new StyledTextSpan(text, style));
            UpdateCachedValues();
            OnChanged();
            return;
        }

        var index = SplitAt(offset);
        _spans.Insert(index, new StyledTextSpan(text, style));
        MergeAdjacent();
        AdjustParagraphMetadataOnInsert(offset, text.Length);
        AdjustTableRangesOnInsert(offset, text.Length);
        UpdateCachedValues();

        if (!suppressChanged)
        {
            OnChanged();
        }
    }

    public void DeleteRange(int start, int length)
    {
        if (length <= 0)
        {
            return;
        }

        if (Length == 0)
        {
            return;
        }

        ValidateOffset(start);
        ValidateOffset(start + length, allowEqual: true);

        if (length == 0 || _spans.Count == 0)
        {
            return;
        }

        SplitAt(start + length);
        SplitAt(start);

        var spansWithOffsets = GetSpanRanges().ToList();
        for (int i = spansWithOffsets.Count - 1; i >= 0; i--)
        {
            var spanRange = spansWithOffsets[i];
            if (spanRange.Start >= start + length)
            {
                continue;
            }

            if (spanRange.End <= start)
            {
                break;
            }

            if (spanRange.Start >= start && spanRange.End <= start + length)
            {
                _spans.RemoveAt(spanRange.Index);
            }
        }

        MergeAdjacent();
        AdjustParagraphMetadataOnDelete(start, length);
        AdjustTableRangesOnDelete(start, length);
        UpdateCachedValues();
        OnChanged();
    }

    public void ReplaceRange(int start, int length, string text, RichTextStyle style)
    {
        DeleteRange(start, length);
        InsertText(start, text, style);
    }

    public void ApplyStyle(int start, int length, Func<RichTextStyle, RichTextStyle> updater)
    {
        ArgumentNullException.ThrowIfNull(updater);

        if (length <= 0)
        {
            return;
        }

        if (Length == 0)
        {
            return;
        }

        ValidateOffset(start);
        ValidateOffset(start + length, allowEqual: true);

        SplitAt(start + length);
        SplitAt(start);

        foreach (var spanRange in GetSpanRanges())
        {
            if (spanRange.Start >= start + length)
            {
                break;
            }

            if (spanRange.End <= start)
            {
                continue;
            }

            _spans[spanRange.Index].Style = updater(_spans[spanRange.Index].Style);
        }

        MergeAdjacent();
        UpdateCachedValues();
        OnChanged();
    }

    public void SetParagraphAlignment(IEnumerable<(int Start, int Length)> paragraphs, TextAlignment alignment)
    {
        ArgumentNullException.ThrowIfNull(paragraphs);

        var changed = false;

        foreach (var (start, length) in paragraphs)
        {
            if (Length == 0)
            {
                continue;
            }

            var maxStart = Math.Max(0, Length - 1);
            var normalizedStart = Math.Clamp(start, 0, maxStart);
            var normalizedLength = Math.Clamp(length, 0, Math.Max(0, Length - normalizedStart));

            if (normalizedLength == 0)
            {
                // Preserve metadata for empty paragraphs by spanning a single character when possible.
                normalizedLength = Math.Min(1, Length - normalizedStart);
            }

            var existing = FindParagraphEntry(normalizedStart);

            if (existing is { } match)
            {
                var info = match.Info;
                var originalStart = info.Start;
                var originalLength = info.Length;
                var originalAlignment = info.Alignment;

                info.Start = normalizedStart;
                info.Length = normalizedLength;
                info.Alignment = alignment;

                var keyChanged = match.Key != info.Start;
                if (keyChanged)
                {
                    _paragraphProperties.Remove(match.Key);
                    _paragraphProperties[info.Start] = info;
                }

                if (keyChanged || originalLength != info.Length || originalAlignment != info.Alignment)
                {
                    changed = true;
                }
            }
            else
            {
                var info = new ParagraphProperties
                {
                    Start = normalizedStart,
                    Length = normalizedLength,
                    Alignment = alignment
                };

                _paragraphProperties[info.Start] = info;
                changed = true;
            }
        }

        if (changed)
        {
            OnChanged();
        }
    }

    public void ClearParagraphAlignment(IEnumerable<(int Start, int Length)> paragraphs)
    {
        ArgumentNullException.ThrowIfNull(paragraphs);

        var changed = false;

        foreach (var (start, _) in paragraphs)
        {
            if (_paragraphProperties.Remove(start))
            {
                changed = true;
                continue;
            }

            var match = FindParagraphEntry(start);
            if (match is { } entry && _paragraphProperties.Remove(entry.Key))
            {
                changed = true;
            }
        }

        if (changed)
        {
            OnChanged();
        }
    }

    internal TextAlignment? GetParagraphAlignment(int start)
    {
        if (_paragraphProperties.TryGetValue(start, out var direct) && direct.Alignment.HasValue)
        {
            return direct.Alignment;
        }

        foreach (var info in _paragraphProperties.Values)
        {
            if (info.Alignment.HasValue)
            {
                if (info.Length == 0 && start == info.Start)
                {
                    return info.Alignment;
                }

                if (start >= info.Start && start < info.End)
                {
                    return info.Alignment;
                }
            }
        }

        return null;
    }

    public int InsertTable(int offset, int rows, int columns)
    {
        if (rows <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(rows));
        }

        if (columns <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(columns));
        }

        offset = Math.Clamp(offset, 0, Length);

        var insertionStyle = Length == 0
            ? RichTextStyle.Default
            : GetStyleAtOffset(Math.Clamp(offset == Length ? Math.Max(0, offset - 1) : offset, 0, Math.Max(0, Length - 1)));

        var needsLeadingBreak = Length > 0 && offset > 0 && Text[offset - 1] != '\n';
        var needsTrailingBreak = Length == 0
            ? true
            : offset < Length
                ? Text[offset] != '\n'
                : true;

        var tableContent = BuildTableContent(rows, columns);
        var builder = new StringBuilder();

        if (needsLeadingBreak)
        {
            builder.Append('\n');
        }

        var contentStartOffset = builder.Length;
        builder.Append(tableContent);
        var contentLength = tableContent.Length;

        if (needsTrailingBreak)
        {
            builder.Append('\n');
        }

        if (builder.Length == 0)
        {
            return 0;
        }

        InsertText(offset, builder.ToString(), insertionStyle, suppressChanged: true);

        var tableRange = new TableRange
        {
            Start = offset + contentStartOffset,
            Length = contentLength,
            Columns = columns,
            HeaderRowCount = Math.Min(1, rows)
        };

        InsertTableRange(tableRange);
        OnChanged();
        return builder.Length;
    }

    public RichTextStyle GetStyleAtOffset(int offset)
    {
        if (Length == 0)
        {
            return RichTextStyle.Default;
        }

        ValidateOffset(offset, allowEqual: true);

        if (offset == Length)
        {
            return _spans[^1].Style;
        }

        var (index, _) = FindSpan(offset);
        return _spans[index].Style;
    }

    public string GetTextRange(int start, int length)
    {
        if (length == 0)
        {
            return string.Empty;
        }

        if (Length == 0)
        {
            return string.Empty;
        }

        ValidateOffset(start);
        ValidateOffset(start + length, allowEqual: true);

        var sb = new StringBuilder(length);
        var spansWithOffsets = GetSpanRanges();
        foreach (var spanRange in spansWithOffsets)
        {
            if (spanRange.Start >= start + length)
            {
                break;
            }

            if (spanRange.End <= start)
            {
                continue;
            }

            var span = _spans[spanRange.Index];
            int segmentStart = Math.Max(start, spanRange.Start);
            int segmentEnd = Math.Min(start + length, spanRange.End);
            int localStart = segmentStart - spanRange.Start;
            int localLength = segmentEnd - segmentStart;
            if (localLength > 0)
            {
                sb.Append(span.Text.AsSpan(localStart, localLength));
            }
        }

        return sb.ToString();
    }

    internal void RestoreFrom(RichTextDocument other)
    {
        _spans.Clear();
        _spans.AddRange(other._spans.Select(s => s.Clone()));
        _paragraphProperties.Clear();
        foreach (var pair in other._paragraphProperties)
        {
            _paragraphProperties[pair.Key] = pair.Value.Clone();
        }

        _tableRanges.Clear();
        foreach (var table in other._tableRanges)
        {
            _tableRanges.Add(table.Clone());
        }
        UpdateCachedValues();
        OnChanged();
    }

    internal FlowDocumentSnapshot CreateFlowSnapshot(TextAlignment alignment)
    {
        var builder = new FlowSnapshotBuilder(this, alignment);
        return builder.Build();
    }

    private sealed class FlowSnapshotBuilder
    {
        private const double IndentStep = 24;
        private const int SpacesPerTab = 4;
        private const double SpaceIndentWidth = IndentStep / (double)SpacesPerTab;

        private readonly RichTextDocument _owner;
        private readonly FlowDocumentElement _flowDocument;
        private readonly List<ParagraphSnapshot> _paragraphs = new();
        private int _currentFlowStart;

        public FlowSnapshotBuilder(RichTextDocument owner, TextAlignment alignment)
        {
            _owner = owner;
            _flowDocument = new FlowDocumentElement
            {
                FontFamily = RichTextStyle.Default.FontFamily,
                FontSize = RichTextStyle.Default.FontSize,
                FontWeight = RichTextStyle.Default.FontWeight,
                FontStyle = RichTextStyle.Default.FontStyle,
                Foreground = RichTextStyle.Default.Foreground,
                Background = RichTextStyle.Default.Background,
                TextAlignment = alignment
            };
        }

        public FlowDocumentSnapshot Build()
        {
            if (_owner.Length == 0)
            {
                BuildEmptyParagraph();
                return new FlowDocumentSnapshot(_flowDocument, _paragraphs, _owner.Length);
            }

            var slices = ComputeBlockSlices();
            if (slices.Count == 0)
            {
                BuildEmptyParagraph();
                return new FlowDocumentSnapshot(_flowDocument, _paragraphs, _owner.Length);
            }

            foreach (var slice in slices)
            {
                if (slice.IsTable && slice.Table is not null)
                {
                    BuildTableBlock(slice.Table);
                }
                else
                {
                    BuildParagraphBlock(slice.Start, slice.Length);
                }
            }

            return new FlowDocumentSnapshot(_flowDocument, _paragraphs, _owner.Length);
        }

        private void BuildParagraphBlock(int start, int length)
        {
            var hasTrailingNewline = length > 0 && start + length - 1 < _owner.Text.Length && _owner.Text[start + length - 1] == '\n';
            var contentLength = hasTrailingNewline ? Math.Max(0, length - 1) : length;

            var segments = GetSegmentsInRange(start, contentLength);
            if (segments.Count == 0)
            {
                segments.Add(new Segment(string.Empty, RichTextStyle.Default));
            }

            var alignmentOverride = _owner.GetParagraphAlignment(start);
            var paragraph = CreateParagraph(
                documentStart: start,
                documentLength: contentLength,
                hasTrailingNewline,
                segments,
                allowIndentTrim: true,
                alignmentOverride);

            _flowDocument.Blocks.Add(paragraph);
        }

        private void BuildTableBlock(TableRange table)
        {
            var rows = ExtractTableRows(table);
            if (rows.Count == 0)
            {
                rows.Add(new List<CellRange> { new CellRange(table.Start, 0) });
            }

            var columnCount = Math.Max(table.Columns, rows.Max(row => row.Count));
            if (columnCount <= 0)
            {
                columnCount = 1;
            }

            var flowTable = new FlowTable
            {
                CellSpacing = table.CellSpacing,
                GridLinesBrush = table.GridLinesBrush,
                GridLinesThickness = table.GridLinesThickness
            };

            for (var column = 0; column < columnCount; column++)
            {
                flowTable.Columns.Add(new FlowTableColumn());
            }

            FlowTableRowGroup? headerGroup = null;
            var headerRows = Math.Min(table.HeaderRowCount, rows.Count);
            if (headerRows > 0)
            {
                headerGroup = new FlowTableRowGroup();
            }

            var bodyGroup = new FlowTableRowGroup();

            for (var rowIndex = 0; rowIndex < rows.Count; rowIndex++)
            {
                var rowCells = rows[rowIndex];
                var flowRow = new FlowTableRow();

                for (var column = 0; column < columnCount; column++)
                {
                    CellRange cellRange;
                    if (column < rowCells.Count)
                    {
                        cellRange = rowCells[column];
                    }
                    else
                    {
                        var fallbackStart = rowCells.Count > 0 ? rowCells[^1].End : table.Start;
                        cellRange = new CellRange(fallbackStart, 0);
                    }

                    var segments = GetSegmentsInRange(cellRange.Start, cellRange.Length);
                    if (segments.Count == 0)
                    {
                        segments.Add(new Segment(string.Empty, RichTextStyle.Default));
                    }

                    var alignmentOverride = _owner.GetParagraphAlignment(cellRange.Start);
                    var paragraph = CreateParagraph(
                        cellRange.Start,
                        cellRange.Length,
                        hasTrailingNewline: false,
                        segments,
                        allowIndentTrim: false,
                        alignmentOverride);

                    paragraph.Margin = new Thickness(0);

                    var cell = new FlowTableCell(paragraph)
                    {
                        Padding = table.CellPadding,
                        BorderBrush = table.GridLinesBrush,
                        BorderThickness = new Thickness(table.GridLinesThickness)
                    };

                    flowRow.Cells.Add(cell);
                }

                if (headerGroup is not null && rowIndex < headerRows)
                {
                    headerGroup.Rows.Add(flowRow);
                }
                else
                {
                    bodyGroup.Rows.Add(flowRow);
                }
            }

            if (headerGroup is not null && headerGroup.Rows.Count > 0)
            {
                flowTable.RowGroups.Add(headerGroup);
            }

            if (bodyGroup.Rows.Count > 0)
            {
                flowTable.RowGroups.Add(bodyGroup);
            }

            _flowDocument.Blocks.Add(flowTable);
        }

        private void BuildEmptyParagraph()
        {
            var paragraph = CreateParagraph(
                documentStart: 0,
                documentLength: 0,
                hasTrailingNewline: false,
                segments: new List<Segment> { new Segment(string.Empty, RichTextStyle.Default) },
                allowIndentTrim: false,
                alignmentOverride: null);

            _flowDocument.Blocks.Add(paragraph);
        }

        private FlowParagraph CreateParagraph(
            int documentStart,
            int documentLength,
            bool hasTrailingNewline,
            List<Segment> segments,
            bool allowIndentTrim,
            TextAlignment? alignmentOverride)
        {
            int leadingTrimChars = 0;
            double indentWidth = 0;
            var processed = segments;

            if (allowIndentTrim)
            {
                (leadingTrimChars, indentWidth) = CalculateIndentation(segments);
                processed = TrimLeadingCharacters(segments, leadingTrimChars);
            }

            var paragraph = new FlowParagraph();

            var flowLength = 0;
            foreach (var segment in processed)
            {
                if (segment.Text.Length == 0)
                {
                    continue;
                }

                paragraph.Inlines.Add(CreateRun(segment.Text, segment.Style));
                flowLength += segment.Text.Length;
            }

            if (paragraph.Inlines.Count == 0)
            {
                paragraph.Inlines.Add(new FlowRun(string.Empty));
            }

            if (allowIndentTrim && indentWidth > 0)
            {
                var margin = paragraph.Margin;
                paragraph.Margin = new Thickness(margin.Left + indentWidth, margin.Top, margin.Right, margin.Bottom);
            }

            if (alignmentOverride.HasValue)
            {
                paragraph.TextAlignment = alignmentOverride.Value;
            }

            var snapshot = new ParagraphSnapshot(
                paragraph,
                documentStart,
                documentLength,
                hasTrailingNewline,
                _currentFlowStart,
                flowLength,
                allowIndentTrim ? leadingTrimChars : 0,
                _paragraphs.Count);

            _paragraphs.Add(snapshot);
            _currentFlowStart += flowLength;

            return paragraph;
        }

        private List<BlockSlice> ComputeBlockSlices()
        {
            var slices = new List<BlockSlice>();
            var length = _owner.Length;
            if (length == 0)
            {
                return slices;
            }

            var tables = new Queue<TableRange>(_owner._tableRanges.Where(t => t.Length > 0));
            var nextTable = tables.Count > 0 ? tables.Peek() : null;
            var cursor = 0;

            while (cursor < length)
            {
                if (nextTable is not null && cursor == nextTable.Start)
                {
                    slices.Add(new BlockSlice(nextTable));
                    cursor = nextTable.End;
                    tables.Dequeue();
                    nextTable = tables.Count > 0 ? tables.Peek() : null;
                    continue;
                }

                var nextTableStart = nextTable?.Start ?? length;
                var newlineIndex = _owner.Text.IndexOf('\n', cursor);
                int blockEnd;

                if (newlineIndex >= 0 && newlineIndex < nextTableStart)
                {
                    blockEnd = newlineIndex + 1;
                }
                else
                {
                    blockEnd = Math.Min(nextTableStart, length);
                    if (blockEnd == cursor)
                    {
                        blockEnd = Math.Min(length, cursor + 1);
                    }
                }

                slices.Add(new BlockSlice(cursor, Math.Max(0, blockEnd - cursor)));
                cursor = blockEnd;
            }

            if (length > 0 && cursor == length && _owner.Text[length - 1] == '\n')
            {
                slices.Add(new BlockSlice(length, 0));
            }

            return slices;
        }

        private List<List<CellRange>> ExtractTableRows(TableRange table)
        {
            var rows = new List<List<CellRange>>();
            var currentRow = new List<CellRange>();
            var text = _owner.Text;
            var cellStart = table.Start;
            var tableEnd = table.End;

            for (var cursor = table.Start; cursor < tableEnd; cursor++)
            {
                var ch = text[cursor];
                if (ch == '\t' || ch == '\n')
                {
                    currentRow.Add(new CellRange(cellStart, cursor - cellStart));
                    if (ch == '\n')
                    {
                        rows.Add(currentRow);
                        currentRow = new List<CellRange>();
                    }

                    cellStart = cursor + 1;
                }
            }

            if (cellStart <= tableEnd)
            {
                currentRow.Add(new CellRange(cellStart, tableEnd - cellStart));
            }

            if (currentRow.Count > 0)
            {
                rows.Add(currentRow);
            }

            return rows;
        }

        private List<Segment> GetSegmentsInRange(int start, int length)
        {
            var segments = new List<Segment>();
            if (length <= 0)
            {
                return segments;
            }

            var end = start + length;
            foreach (var spanRange in _owner.GetSpanRanges())
            {
                if (spanRange.End <= start)
                {
                    continue;
                }

                if (spanRange.Start >= end)
                {
                    break;
                }

                var overlapStart = Math.Max(start, spanRange.Start);
                var overlapEnd = Math.Min(end, spanRange.End);
                var overlapLength = overlapEnd - overlapStart;
                if (overlapLength <= 0)
                {
                    continue;
                }

                var span = _owner._spans[spanRange.Index];
                var localStart = overlapStart - spanRange.Start;
                var text = span.Text.Substring(localStart, overlapLength);
                segments.Add(new Segment(text, span.Style));
            }

            return segments;
        }

        private static (int LeadingTrimChars, double IndentWidth) CalculateIndentation(List<Segment> segments)
        {
            if (segments.Count == 0)
            {
                return (0, 0);
            }

            var leadingTrimChars = 0;
            var column = 0;

            foreach (var segment in segments)
            {
                foreach (var ch in segment.Text)
                {
                    if (ch == ' ')
                    {
                        leadingTrimChars++;
                        column += 1;
                        continue;
                    }

                    if (ch == '\t')
                    {
                        leadingTrimChars++;
                        column = ((column / SpacesPerTab) + 1) * SpacesPerTab;
                        continue;
                    }

                    return (leadingTrimChars, column * SpaceIndentWidth);
                }
            }

            return (leadingTrimChars, column * SpaceIndentWidth);
        }

        private static List<Segment> TrimLeadingCharacters(List<Segment> segments, int charactersToTrim)
        {
            if (charactersToTrim <= 0)
            {
                return new List<Segment>(segments);
            }

            var result = new List<Segment>(segments.Count);
            var remaining = charactersToTrim;

            foreach (var segment in segments)
            {
                if (remaining <= 0)
                {
                    result.Add(segment);
                    continue;
                }

                if (segment.Text.Length <= remaining)
                {
                    remaining -= segment.Text.Length;
                    continue;
                }

                var trimmedText = segment.Text.Substring(remaining);
                result.Add(new Segment(trimmedText, segment.Style));
                remaining = 0;
            }

            if (result.Count == 0)
            {
                result.Add(new Segment(string.Empty, RichTextStyle.Default));
            }

            return result;
        }

        private static FlowRun CreateRun(string text, RichTextStyle style)
        {
            var run = new FlowRun(text)
            {
                FontFamily = style.FontFamily,
                FontSize = style.FontSize,
                FontWeight = style.FontWeight,
                FontStyle = style.FontStyle,
                Foreground = style.Foreground,
                Background = style.Background
            };

            if (style.Underline)
            {
                run.TextDecorations = TextDecorations.Underline;
            }

            return run;
        }

        private readonly record struct Segment(string Text, RichTextStyle Style);

        private readonly struct BlockSlice
        {
            public BlockSlice(int start, int length)
            {
                Start = start;
                Length = length;
                Table = null;
            }

            public BlockSlice(TableRange table)
            {
                Start = table.Start;
                Length = table.Length;
                Table = table;
            }

            public int Start { get; }
            public int Length { get; }
            public TableRange? Table { get; }
            public bool IsTable => Table is not null;
        }

        private readonly struct CellRange
        {
            public CellRange(int start, int length)
            {
                Start = start;
                Length = Math.Max(0, length);
            }

            public int Start { get; }
            public int Length { get; }
            public int End => Start + Length;
        }
    }

    private int SplitAt(int offset)
    {
        if (offset <= 0)
        {
            return 0;
        }

        if (offset >= Length)
        {
            return _spans.Count;
        }

        var (index, innerOffset) = FindSpan(offset);
        if (innerOffset == 0)
        {
            return index;
        }

        var span = _spans[index];
        if (innerOffset == span.Length)
        {
            return index + 1;
        }

        var left = span.Text[..innerOffset];
        var right = span.Text[innerOffset..];

        span.Text = left;
        _spans.Insert(index + 1, new StyledTextSpan(right, span.Style));
        return index + 1;
    }

    private (int Index, int Offset) FindSpan(int offset)
    {
        ValidateOffset(offset, allowEqual: true);

        if (_spans.Count == 0)
        {
            return (0, 0);
        }

        int accumulated = 0;
        for (int i = 0; i < _spans.Count; i++)
        {
            var span = _spans[i];
            int next = accumulated + span.Length;
            if (offset < next)
            {
                return (i, offset - accumulated);
            }

            accumulated = next;
        }

        return (_spans.Count - 1, _spans[^1].Length);
    }

    private IEnumerable<(int Index, int Start, int End)> GetSpanRanges()
    {
        int start = 0;
        for (int i = 0; i < _spans.Count; i++)
        {
            int end = start + _spans[i].Length;
            yield return (i, start, end);
            start = end;
        }
    }

    private void MergeAdjacent()
    {
        for (int i = _spans.Count - 1; i >= 0; i--)
        {
            if (_spans[i].Length == 0)
            {
                _spans.RemoveAt(i);
            }
        }

        if (_spans.Count == 0)
        {
            return;
        }

        for (int i = _spans.Count - 2; i >= 0; i--)
        {
            var current = _spans[i];
            var next = _spans[i + 1];
            if (current.Style == next.Style)
            {
                current.Text += next.Text;
                _spans.RemoveAt(i + 1);
            }
        }
    }

    private void ValidateOffset(int offset, bool allowEqual = false)
    {
        if (allowEqual)
        {
            if (offset < 0 || offset > Length)
            {
                throw new ArgumentOutOfRangeException(nameof(offset));
            }
        }
        else
        {
            if (offset < 0 || offset >= Length)
            {
                throw new ArgumentOutOfRangeException(nameof(offset));
            }
        }
    }

    private void AdjustParagraphMetadataOnInsert(int offset, int delta)
    {
        if (delta == 0 || _paragraphProperties.Count == 0)
        {
            return;
        }

        var updates = new List<(int OldKey, ParagraphProperties Properties)>();

        foreach (var pair in _paragraphProperties)
        {
            var info = pair.Value;

            if (offset <= info.Start)
            {
                info.Start += delta;
                updates.Add((pair.Key, info));
            }
            else if (offset < info.End)
            {
                info.Length += delta;
                updates.Add((pair.Key, info));
            }
        }

        foreach (var (oldKey, info) in updates)
        {
            _paragraphProperties.Remove(oldKey);
            _paragraphProperties[info.Start] = info;
        }
    }

    private void AdjustParagraphMetadataOnDelete(int start, int length)
    {
        if (length == 0 || _paragraphProperties.Count == 0)
        {
            return;
        }

        var deleteEnd = start + length;
        var removals = new HashSet<int>();
        var updates = new List<(int OldKey, ParagraphProperties Properties)>();

        foreach (var pair in _paragraphProperties)
        {
            if (removals.Contains(pair.Key))
            {
                continue;
            }

            var info = pair.Value;
            var paragraphEnd = info.End;

            if (deleteEnd <= info.Start)
            {
                info.Start -= length;
                updates.Add((pair.Key, info));
            }
            else if (start >= paragraphEnd)
            {
                continue;
            }
            else
            {
                if (start < info.Start)
                {
                    var shift = Math.Min(length, info.Start - start);
                    info.Start -= shift;
                }

                var overlapStart = Math.Max(start, info.Start);
                var overlapEnd = Math.Min(deleteEnd, paragraphEnd);
                var removed = overlapEnd - overlapStart;
                info.Length -= removed;

                if (info.Length <= 0)
                {
                    removals.Add(pair.Key);
                    continue;
                }

                updates.Add((pair.Key, info));
            }
        }

        foreach (var key in removals)
        {
            _paragraphProperties.Remove(key);
        }

        foreach (var (oldKey, info) in updates)
        {
            _paragraphProperties.Remove(oldKey);
            _paragraphProperties[info.Start] = info;
        }
    }

    private static string BuildTableContent(int rows, int columns)
    {
        var builder = new StringBuilder();

        for (var row = 0; row < rows; row++)
        {
            for (var column = 0; column < columns; column++)
            {
                if (row == 0)
                {
                    builder.Append($"Header {column + 1}");
                }
                else
                {
                    builder.Append($"Row {row}, Column {column + 1}");
                }

                if (column < columns - 1)
                {
                    builder.Append('\t');
                }
            }

            if (row < rows - 1)
            {
                builder.Append('\n');
            }
        }

        return builder.ToString();
    }

    private void InsertTableRange(TableRange range)
    {
        if (range.Length <= 0)
        {
            return;
        }

        for (var i = _tableRanges.Count - 1; i >= 0; i--)
        {
            var existing = _tableRanges[i];
            if (range.Start < existing.End && range.End > existing.Start)
            {
                _tableRanges.RemoveAt(i);
            }
        }

        var insertIndex = _tableRanges.FindIndex(t => range.Start < t.Start);
        if (insertIndex < 0)
        {
            _tableRanges.Add(range);
        }
        else
        {
            _tableRanges.Insert(insertIndex, range);
        }
    }

    private void AdjustTableRangesOnInsert(int offset, int delta)
    {
        if (delta == 0 || _tableRanges.Count == 0)
        {
            return;
        }

        foreach (var table in _tableRanges)
        {
            if (offset <= table.Start)
            {
                table.Start += delta;
            }
            else if (offset < table.End)
            {
                table.Length += delta;
            }
        }
    }

    private void AdjustTableRangesOnDelete(int start, int length)
    {
        if (length == 0 || _tableRanges.Count == 0)
        {
            return;
        }

        var deleteEnd = start + length;

        for (var i = _tableRanges.Count - 1; i >= 0; i--)
        {
            var table = _tableRanges[i];

            if (deleteEnd <= table.Start)
            {
                table.Start -= length;
                continue;
            }

            if (start >= table.End)
            {
                continue;
            }

            if (start < table.Start)
            {
                var shift = Math.Min(length, table.Start - start);
                table.Start -= shift;
            }

            var originalEnd = table.End;
            var overlapStart = Math.Max(start, table.Start);
            var overlapEnd = Math.Min(deleteEnd, originalEnd);
            var removedInside = overlapEnd - overlapStart;
            if (removedInside > 0)
            {
                table.Length -= removedInside;
                if (table.Length < 0)
                {
                    table.Length = 0;
                }
            }

            if (table.Length <= 0)
            {
                _tableRanges.RemoveAt(i);
            }
        }
    }

    private (int Key, ParagraphProperties Info)? FindParagraphEntry(int start)
    {
        if (_paragraphProperties.TryGetValue(start, out var direct))
        {
            return (start, direct);
        }

        foreach (var pair in _paragraphProperties)
        {
            var info = pair.Value;
            if (start >= info.Start && start < info.End)
            {
                return (pair.Key, info);
            }
        }

        return null;
    }

    private static string NormalizeNewLines(string text)
    {
        return text.Replace("\r\n", "\n").Replace('\r', '\n');
    }

    private void UpdateCachedValues()
    {
        Length = _spans.Sum(s => s.Length);
        Text = _spans.Count == 0
            ? string.Empty
            : string.Concat(_spans.Select(s => s.Text));
    }

    private void OnChanged()
    {
        Changed?.Invoke(this, EventArgs.Empty);
    }

    internal sealed class ParagraphProperties
    {
        public int Start { get; set; }

        public int Length { get; set; }

        public TextAlignment? Alignment { get; set; }

        public int End => Start + Length;

        public ParagraphProperties Clone()
        {
            return new ParagraphProperties
            {
                Start = Start,
                Length = Length,
                Alignment = Alignment
            };
        }
    }

    internal sealed class TableRange
    {
        public int Start { get; set; }

        public int Length { get; set; }

        public int Columns { get; set; }

        public int HeaderRowCount { get; set; }

        public double CellSpacing { get; set; } = 6;

        public IBrush? GridLinesBrush { get; set; } = Brushes.LightGray;

        public double GridLinesThickness { get; set; } = 1;

        public Thickness CellPadding { get; set; } = new Thickness(12, 8, 12, 8);

        public int End => Start + Length;

        public TableRange Clone()
        {
            return new TableRange
            {
                Start = Start,
                Length = Length,
                Columns = Columns,
                HeaderRowCount = HeaderRowCount,
                CellSpacing = CellSpacing,
                GridLinesBrush = GridLinesBrush,
                GridLinesThickness = GridLinesThickness,
                CellPadding = CellPadding
            };
        }
    }
}
