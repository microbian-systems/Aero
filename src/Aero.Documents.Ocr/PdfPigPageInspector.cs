using UglyToad.PdfPig;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;

namespace Aero.Documents.Ocr;

/// <summary>Native PDF inspection; cannot prove visual completeness from embedded text.</summary>
public sealed class PdfPigPageInspector : IPdfPageInspector
{
    /// <inheritdoc />
    public PdfInspection Inspect(byte[] pdf, ExtractionLimits limits, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!pdf.AsSpan().StartsWith("%PDF-"u8))
            throw new DocumentAdapterException("FormatMismatch", "PDF signature is missing.");
        try
        {
            using var document = PdfDocument.Open(pdf);
            var pages = new List<PdfPageInfo>();
            var issues = new List<DocumentIssue>();
            int retained = 0;
            for (int n = 1; n <= Math.Min(document.NumberOfPages, limits.MaxUnits); n++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var page = document.GetPage(n);
                var text = ContentOrderTextExtractor.GetText(page);
                if (text.Length > limits.MaxCharacters - retained)
                {
                    issues.Add(new("TextLimit", "Native text retention limit reached; remaining pages were not inspected.", new("page", Page: n)));
                    break;
                }
                retained += text.Length;
                pages.Add(new(n, text, page.GetImages().Any(), page.Width, page.Height));
            }
            if (document.NumberOfPages > limits.MaxUnits)
                issues.Add(new("UnitLimit", "PDF page inspection limit reached."));
            return new(pages, issues);
        }
        catch (Exception e) when (e is not OperationCanceledException and not OutOfMemoryException
            and not StackOverflowException and not DocumentAdapterException)
        {
            cancellationToken.ThrowIfCancellationRequested();
            throw new DocumentAdapterException("InvalidDocument", "PDF is malformed, encrypted or unsupported.");
        }
    }
}
/// <summary>Conservative policy: preserve usable native text and OCR only likely image-only scans.</summary>
public sealed class ImageOnlyOcrPolicy : IOcrPagePolicy
{
    /// <inheritdoc />
    public bool ShouldRecognize(PdfPageInfo page) => page.HasImages && !page.Text.Any(char.IsLetterOrDigit);
}
