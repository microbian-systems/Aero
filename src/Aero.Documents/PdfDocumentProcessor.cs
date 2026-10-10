using UglyToad.PdfPig;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;

namespace Aero.Documents;

/// <summary>PdfPig page-anchored text extraction; no OCR or table reconstruction.</summary>
public sealed class PdfDocumentProcessor : DocumentProcessor
{
    /// <inheritdoc />
    public override string Extension => ".pdf";
    /// <inheritdoc />
    protected override ExtractionResult Extract(byte[] content, ExtractionLimits limits, CancellationToken cancellationToken)
    {
        if (!content.AsSpan().StartsWith("%PDF-"u8)) return Failure("FormatMismatch", "PDF signature is missing.");
        using var pdf = PdfDocument.Open(content);
        var result = new ExtractionBuffer("pdfpig:1", limits);
        for (int pageNumber = 1; pageNumber <= Math.Min(pdf.NumberOfPages, limits.MaxUnits); pageNumber++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var page = pdf.GetPage(pageNumber);
            var location = new DocumentLocation("page", Page: pageNumber);
            var text = ContentOrderTextExtractor.GetText(page);
            if (string.IsNullOrWhiteSpace(text))
                result.Issue("NoPageText", "No text on this page; OCR or manual review may be needed.", location);
            else if (!result.Add(text, location)) break;
        }
        if (pdf.NumberOfPages > limits.MaxUnits) result.Issue("UnitLimit", "PDF page limit reached.");
        result.Issue("LayoutCoverage", "Text extraction does not interpret images, forms or reconstruct table semantics.");
        return result.Result();
    }
}
