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
            if (element is W.Paragraph paragraph)
            {
                if (++units > limits.MaxUnits) { result.Issue("UnitLimit", "Word block limit reached."); break; }
                var text = string.Concat(paragraph.Descendants<W.Text>().Select(x => x.Text));
                if (!result.Add(text, new($"body:paragraph:{elementNumber}"), heading)) break;
                var style = paragraph.ParagraphProperties?.ParagraphStyleId?.Val?.Value;
                if (style?.StartsWith("Heading", StringComparison.OrdinalIgnoreCase) == true)
                    heading = text[..Math.Min(text.Length, 1024)];
            }
            else if (element is W.Table table)
            {
                int rowNumber = 0;
                string priorRow = "";
                foreach (var row in table.Elements<W.TableRow>())
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    rowNumber++;
                    if (++units > limits.MaxUnits) { result.Issue("UnitLimit", "Word block limit reached."); break; }
                    var cells = row.Elements<W.TableCell>().Select((cell, index) =>
                        $"Column {index + 1}: " + string.Join(" / ", cell.Descendants<W.Paragraph>()
                            .Select(p => string.Concat(p.Descendants<W.Text>().Select(t => t.Text)))));
                    var text = string.Join(" | ", cells);
                    if (!result.Add(text, new($"body:table:{elementNumber}:row:{rowNumber}"),
                        heading + (priorRow.Length == 0 ? "" : "\nPrevious row (not inferred headers): " + priorRow))) break;
                    priorRow = text[..Math.Min(text.Length, 1024)];
                }
                if (units > limits.MaxUnits || result.Exhausted) break;
            }
            else if (element is not W.SectionProperties)
                result.Issue("UnsupportedBodyElement", "An unsupported body element was not extracted.");
        }
        result.Issue("BodyCoverage", "Main body only; headers, footnotes, drawings, tracked changes and merged/nested table semantics require further review.");
        return result.Result();
    }
}
