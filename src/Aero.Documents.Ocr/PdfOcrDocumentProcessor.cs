using FluentValidation;

namespace Aero.Documents.Ocr;

/// <summary>Opt-in asynchronous extraction facade. Text inspection, policy, rendering and OCR are strategies.</summary>
public sealed class PdfOcrDocumentProcessor
{
    private readonly IPdfPageInspector inspector;
    private readonly IOcrPagePolicy policy;
    private readonly IPdfPageRenderer renderer;
    private readonly IOcrEngine engine;
    private readonly PdfOcrOptions options;
    /// <summary>Explicit composition; no reflection, provider discovery or implicit paid fallback.</summary>
    public PdfOcrDocumentProcessor(IPdfPageInspector inspector, IOcrPagePolicy policy,
        IPdfPageRenderer renderer, IOcrEngine engine, PdfOcrOptions? options = null)
    {
        this.inspector = inspector ?? throw new ArgumentNullException(nameof(inspector));
        this.policy = policy ?? throw new ArgumentNullException(nameof(policy));
        this.renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));
        this.engine = engine ?? throw new ArgumentNullException(nameof(engine));
        this.options = options ?? new();
        new PdfOcrOptionsValidator().ValidateAndThrow(this.options);
    }
    /// <summary>Returns page text and OCR provenance; cancellation is never disguised as document failure.</summary>
    public async Task<PdfOcrExtraction> ExtractAsync(byte[] pdf, ExtractionLimits? limits = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(pdf);
        limits ??= new();
        new ExtractionLimitsValidator().ValidateAndThrow(limits);
        cancellationToken.ThrowIfCancellationRequested();
        var blocks = new List<DocumentBlock>();
        var issues = new List<DocumentIssue>();
        var evidence = new List<OcrPageEvidence>();
        PdfOcrExtraction Result() => new(new("pdfpig+local-ocr:1", blocks.ToArray(), issues.ToArray()), evidence.ToArray());
        if (pdf.Length > limits.MaxInputBytes)
        {
            issues.Add(new("InputLimit", "PDF exceeds input byte limit."));
            return Result();
        }
        PdfInspection inspected;
        try { inspected = inspector.Inspect(pdf, limits, cancellationToken); }
        catch (DocumentAdapterException e)
        {
            cancellationToken.ThrowIfCancellationRequested();
            issues.Add(new(e.Code, e.Message)); return Result();
        }
        issues.AddRange(inspected.Issues);
        int selected = 0, retained = 0;
        foreach (var page in inspected.Pages)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var location = new DocumentLocation("page", Page: page.Number);
            string text = page.Text;
            if (policy.ShouldRecognize(page))
            {
                if (++selected > options.MaxOcrPages)
                {
                    issues.Add(new("OcrPageLimit", "OCR page budget reached; selected page was not recognized.", location));
                    continue;
                }
                double w = Math.Ceiling(page.WidthPoints * options.Dpi / 72d);
                double h = Math.Ceiling(page.HeightPoints * options.Dpi / 72d);
                if (!double.IsFinite(w) || !double.IsFinite(h) || w < 1 || h < 1
                    || w > int.MaxValue || h > int.MaxValue || w * h > options.MaxPixels)
                {
                    issues.Add(new("RasterLimit", "Page rasterization exceeds configured dimensions/pixel budget.", location));
                    continue;
                }
                try
                {
                    var image = await renderer.RenderAsync(pdf, page.Number, (int)w, (int)h, options.MaxImageBytes, cancellationToken);
                    if (image.Width < 1 || image.Height < 1 || (long)image.Width * image.Height > options.MaxPixels
                        || image.Png.Length > options.MaxImageBytes)
                        throw new DocumentAdapterException("RasterLimit", "Renderer output exceeds configured budget.");
                    var recognized = await engine.RecognizeAsync(image, cancellationToken);
                    evidence.Add(new(page.Number, image.Width, image.Height, recognized));
                    issues.AddRange(recognized.Issues.Select(x => x with { Location = location }));
                    text = recognized.Text;
                    issues.Add(new("OcrDerived", "OCR-derived text requires source review, particularly amounts and deadlines.", location));
                    if (recognized.Words.Any(x => x.Confidence < options.MinimumWordConfidence))
                        issues.Add(new("LowOcrConfidence", "Some recognized words have low engine confidence.", location));
                }
                catch (DocumentAdapterException e)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    issues.Add(new(e.Code, e.Message, location)); continue;
                }
            }
            else if (page.HasImages)
                issues.Add(new("MixedPageCoverage", "Native text retained; image regions may contain additional text and were not OCR'd.", location));
            if (string.IsNullOrWhiteSpace(text))
            {
                issues.Add(new("NoPageText", "No usable text retained for this page.", location));
                continue;
            }
            if (text.Length > limits.MaxCharacters - retained)
            {
                issues.Add(new("TextLimit", "Combined native/OCR text limit reached; page text was not retained.", location));
                continue;
            }
            retained += text.Length;
            blocks.Add(new(text, location));
        }
        issues.Add(new("LayoutCoverage", "Text and OCR do not guarantee table, image, form or visual interpretation."));
        return Result();
    }
}
