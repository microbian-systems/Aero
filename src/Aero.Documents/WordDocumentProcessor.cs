using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using W = DocumentFormat.OpenXml.Wordprocessing;

namespace Aero.Documents;

/// <summary>Read-only Open XML main-body extraction with paragraph and table-row locations.</summary>
public sealed class WordDocumentProcessor : DocumentProcessor
{
    /// <inheritdoc />
    public override string Extension => ".docx";
    /// <inheritdoc />
    protected override ExtractionResult Extract(byte[] content, ExtractionLimits limits, CancellationToken cancellationToken)
    {
        CheckOfficePackage(content, limits, "word/document.xml", cancellationToken);
        using var stream = new MemoryStream(content, false);
        using var doc = WordprocessingDocument.Open(stream, false,
            new OpenSettings { MaxCharactersInPart = limits.MaxExpandedBytes });
        var body = doc.MainDocumentPart?.Document?.Body ?? throw new InvalidDataException();
        var result = new ExtractionBuffer("openxml-docx:1", limits);
        int units = 0, elementNumber = 0;
        string heading = "";
        foreach (var element in body.Elements())
        {
            cancellationToken.ThrowIfCancellationRequested();
            elementNumber++;
            if (!ExtractElement(element, elementNumber, result, limits, cancellationToken, ref units, ref heading)) break;
        }
        result.Issue("BodyCoverage", "Main body only; headers, footnotes, drawings, tracked changes and merged/nested table semantics require further review.");
        return result.Result();
    }

    private static bool ExtractElement(OpenXmlElement element, int number, ExtractionBuffer result,
        ExtractionLimits limits, CancellationToken cancellationToken, ref int units, ref string heading)
    {
        switch (element)
        {
            case W.Paragraph paragraph:
                return ExtractParagraph(paragraph, number, result, limits, ref units, ref heading);
            case W.Table table:
                ExtractTable(table, number, heading, result, limits, cancellationToken, ref units);
                return units <= limits.MaxUnits && !result.Exhausted;
            case W.SectionProperties:
                return true;
            default:
                result.Issue("UnsupportedBodyElement", "An unsupported body element was not extracted.");
                return true;
        }
    }

    private static bool ExtractParagraph(W.Paragraph paragraph, int number, ExtractionBuffer result,
        ExtractionLimits limits, ref int units, ref string heading)
    {
        if (!TryVisitUnit(result, limits, ref units)) return false;
        string text = ParagraphText(paragraph);
        if (!result.Add(text, new($"body:paragraph:{number}"), heading)) return false;
        var style = paragraph.ParagraphProperties?.ParagraphStyleId?.Val?.Value;
        if (style?.StartsWith("Heading", StringComparison.OrdinalIgnoreCase) == true)
            heading = text[..Math.Min(text.Length, 1024)];
        return true;
    }

    private static void ExtractTable(W.Table table, int number, string heading, ExtractionBuffer result,
        ExtractionLimits limits, CancellationToken cancellationToken, ref int units)
    {
        int rowNumber = 0;
        string priorRow = "";
        foreach (var row in table.Elements<W.TableRow>())
        {
            cancellationToken.ThrowIfCancellationRequested();
            rowNumber++;
            if (!TryVisitUnit(result, limits, ref units)) break;
            var cells = row.Elements<W.TableCell>().Select((cell, index) =>
                $"Column {index + 1}: " + string.Join(" / ", cell.Descendants<W.Paragraph>().Select(ParagraphText)));
            var text = string.Join(" | ", cells);
            string context = heading;
            if (priorRow.Length != 0) context += "\nPrevious row (not inferred headers): " + priorRow;
            if (!result.Add(text, new($"body:table:{number}:row:{rowNumber}"), context)) break;
            priorRow = text[..Math.Min(text.Length, 1024)];
        }
    }

    private static bool TryVisitUnit(ExtractionBuffer result, ExtractionLimits limits, ref int units)
    {
        if (++units <= limits.MaxUnits) return true;
        result.Issue("UnitLimit", "Word block limit reached.");
        return false;
    }

    private static string ParagraphText(W.Paragraph paragraph) =>
        string.Concat(paragraph.Descendants<W.Text>().Select(x => x.Text));
}
