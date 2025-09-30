using System.Collections.Generic;
using FlowDocumentElement = TextFlow.Document.Documents.FlowDocument;
using FlowParagraph = TextFlow.Document.Documents.Paragraph;

namespace TextFlow.Editor.Documents;

internal sealed class FlowDocumentSnapshot
{
    private readonly Dictionary<FlowParagraph, ParagraphSnapshot> _paragraphLookup;

    public FlowDocumentSnapshot(FlowDocumentElement flowDocument, List<ParagraphSnapshot> paragraphs, int documentLength)
    {
        FlowDocument = flowDocument;
        Paragraphs = paragraphs;
        DocumentLength = documentLength;
        FlowLength = paragraphs.Count == 0 ? 0 : paragraphs[^1].FlowEnd;
        _paragraphLookup = new Dictionary<FlowParagraph, ParagraphSnapshot>(paragraphs.Count);
        foreach (var snapshot in paragraphs)
        {
            if (snapshot.Paragraph is not null)
            {
                _paragraphLookup[snapshot.Paragraph] = snapshot;
            }
        }
    }

    public FlowDocumentElement FlowDocument { get; }

    public IReadOnlyList<ParagraphSnapshot> Paragraphs { get; }

    public int DocumentLength { get; }

    public int FlowLength { get; }

    public bool TryGetParagraphSnapshot(FlowParagraph paragraph, out ParagraphSnapshot? snapshot) =>
        _paragraphLookup.TryGetValue(paragraph, out snapshot);

    public ParagraphSnapshot? FindParagraphByDocumentOffset(int documentOffset)
    {
        if (Paragraphs.Count == 0)
        {
            return null;
        }

        documentOffset = documentOffset < 0 ? 0 : documentOffset;
        documentOffset = documentOffset > DocumentLength ? DocumentLength : documentOffset;

        ParagraphSnapshot? last = null;

        for (var i = 0; i < Paragraphs.Count; i++)
        {
            var paragraph = Paragraphs[i];
            if (documentOffset < paragraph.DocumentStart)
            {
                return paragraph;
            }

            if (documentOffset <= paragraph.DocumentEnd)
            {
                return paragraph;
            }

            if (paragraph.HasTrailingNewline && documentOffset == paragraph.DocumentEnd + 1)
            {
                if (i + 1 < Paragraphs.Count)
                {
                    return Paragraphs[i + 1];
                }

                return paragraph;
            }

            last = paragraph;
        }

        return last;
    }

    public int ToDocumentOffset(int flowOffset)
    {
        if (Paragraphs.Count == 0)
        {
            return 0;
        }

        flowOffset = flowOffset < 0 ? 0 : flowOffset;
        flowOffset = flowOffset > FlowLength ? FlowLength : flowOffset;

        for (var i = 0; i < Paragraphs.Count; i++)
        {
            var paragraph = Paragraphs[i];

            if (flowOffset < paragraph.FlowStart)
            {
                return paragraph.DocumentStart;
            }

            if (flowOffset <= paragraph.FlowEnd)
            {
                if (flowOffset == paragraph.FlowEnd && paragraph.FlowLength == 0 && i + 1 < Paragraphs.Count)
                {
                    return Paragraphs[i + 1].DocumentStart;
                }

                var local = flowOffset - paragraph.FlowStart;
                var documentOffset = paragraph.DocumentStart + paragraph.LeadingTrim + local;
                if (documentOffset < paragraph.DocumentStart)
                {
                    return paragraph.DocumentStart;
                }

                if (documentOffset > paragraph.DocumentEnd)
                {
                    return paragraph.DocumentEnd;
                }

                return (int)documentOffset;
            }
        }

        return DocumentLength;
    }

    public int ToFlowOffset(int documentOffset)
    {
        if (Paragraphs.Count == 0)
        {
            return 0;
        }

        documentOffset = documentOffset < 0 ? 0 : documentOffset;
        documentOffset = documentOffset > DocumentLength ? DocumentLength : documentOffset;

        for (var i = 0; i < Paragraphs.Count; i++)
        {
            var paragraph = Paragraphs[i];
            if (documentOffset < paragraph.DocumentStart)
            {
                return paragraph.FlowStart;
            }

            var paragraphEnd = paragraph.DocumentEnd;

            if (documentOffset < paragraphEnd)
            {
                var local = documentOffset - paragraph.DocumentStart;
                if (local <= paragraph.LeadingTrim)
                {
                    return paragraph.FlowStart;
                }

                var flowLocal = local - paragraph.LeadingTrim;
                var flowOffset = paragraph.FlowStart + flowLocal;
                if (flowOffset > paragraph.FlowEnd)
                {
                    return paragraph.FlowEnd;
                }

                return (int)flowOffset;
            }

            if (documentOffset == paragraphEnd)
            {
                return paragraph.FlowEnd;
            }

            if (paragraph.HasTrailingNewline && documentOffset == paragraphEnd + 1)
            {
                if (i + 1 < Paragraphs.Count)
                {
                    return Paragraphs[i + 1].FlowStart;
                }

                return paragraph.FlowEnd;
            }
        }

        return FlowLength;
    }
}

internal sealed class ParagraphSnapshot
{
    public ParagraphSnapshot(
        FlowParagraph paragraph,
        int documentStart,
        int documentLength,
        bool hasTrailingNewline,
        int flowStart,
        int flowLength,
        int leadingTrim,
        int index)
    {
        Paragraph = paragraph;
        DocumentStart = documentStart;
        DocumentLength = documentLength;
        HasTrailingNewline = hasTrailingNewline;
        FlowStart = flowStart;
        FlowLength = flowLength;
        LeadingTrim = leadingTrim;
        Index = index;
    }

    public FlowParagraph Paragraph { get; }

    public int DocumentStart { get; }

    public int DocumentLength { get; }

    public bool HasTrailingNewline { get; }

    public int FlowStart { get; }

    public int FlowLength { get; }

    public int LeadingTrim { get; }

    public int DocumentEnd => DocumentStart + DocumentLength;

    public int FlowEnd => FlowStart + FlowLength;

    public int Index { get; }
}
