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
        var extraction = new PageExtraction(this, limits, cancellationToken);
        if (pdf.Length > limits.MaxInputBytes)
        {
            extraction.Issues.Add(new("InputLimit", "PDF exceeds input byte limit."));
            return extraction.Result();
        }
        PdfInspection inspected;
        try { inspected = inspector.Inspect(pdf, limits, cancellationToken); }
        catch (DocumentAdapterException e)
        {
            cancellationToken.ThrowIfCancellationRequested();
            extraction.Issues.Add(new(e.Code, e.Message)); return extraction.Result();
        }
        extraction.Issues.AddRange(inspected.Issues);
        foreach (var page in inspected.Pages)
            await extraction.AppendPageAsync(pdf, page);
        extraction.Issues.Add(new("LayoutCoverage", "Text and OCR do not guarantee table, image, form or visual interpretation."));
        return extraction.Result();
    }

    // Mutable budgets belong to this invocation; the injected strategies remain host-owned.
    private sealed class PageExtraction(PdfOcrDocumentProcessor owner, ExtractionLimits limits,
        CancellationToken cancellationToken)
    {
        private readonly List<DocumentBlock> blocks = [];
        private readonly List<OcrPageEvidence> evidence = [];
        private int selected;
        private int retained;
        public List<DocumentIssue> Issues { get; } = [];
        public PdfOcrExtraction Result() => new(new("pdfpig+local-ocr:1", blocks.ToArray(), Issues.ToArray()), evidence.ToArray());

        public async Task AppendPageAsync(byte[] pdf, PdfPageInfo page)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var location = new DocumentLocation("page", Page: page.Number);
            string? text = await SelectTextAsync(pdf, page, location);
            if (text is null) return;
            if (string.IsNullOrWhiteSpace(text))
            {
                Issues.Add(new("NoPageText", "No usable text retained for this page.", location));
                return;
            }
            if (text.Length > limits.MaxCharacters - retained)
            {
                Issues.Add(new("TextLimit", "Combined native/OCR text limit reached; page text was not retained.", location));
                return;
            }
            retained += text.Length;
            blocks.Add(new(text, location));
        }

        private async Task<string?> SelectTextAsync(byte[] pdf, PdfPageInfo page, DocumentLocation location)
        {
            if (!owner.policy.ShouldRecognize(page))
            {
                if (page.HasImages)
                    Issues.Add(new("MixedPageCoverage", "Native text retained; image regions may contain additional text and were not OCR'd.", location));
                return page.Text;
            }
            if (++selected > owner.options.MaxOcrPages)
            {
                Issues.Add(new("OcrPageLimit", "OCR page budget reached; selected page was not recognized.", location));
                return null;
            }
            try { return await RecognizePageAsync(pdf, page, location); }
            catch (DocumentAdapterException error)
            {
                cancellationToken.ThrowIfCancellationRequested();
                Issues.Add(new(error.Code, error.Message, location));
                return null;
            }
        }

        private async Task<string> RecognizePageAsync(byte[] pdf, PdfPageInfo page, DocumentLocation location)
        {
            var (width, height) = RasterSize(page, owner.options);
            var image = await owner.renderer.RenderAsync(pdf, page.Number, width, height, owner.options.MaxImageBytes, cancellationToken);
            if (image.Width < 1 || image.Height < 1 || (long)image.Width * image.Height > owner.options.MaxPixels
                || image.Png.Length > owner.options.MaxImageBytes)
                throw new DocumentAdapterException("RasterLimit", "Renderer output exceeds configured budget.");
            var recognized = await owner.engine.RecognizeAsync(image, cancellationToken);
            evidence.Add(new(page.Number, image.Width, image.Height, recognized));
            Issues.AddRange(recognized.Issues.Select(x => x with { Location = location }));
            Issues.Add(new("OcrDerived", "OCR-derived text requires source review, particularly amounts and deadlines.", location));
            if (recognized.Words.Any(x => x.Confidence < owner.options.MinimumWordConfidence))
                Issues.Add(new("LowOcrConfidence", "Some recognized words have low engine confidence.", location));
            return recognized.Text;
        }

        private static (int Width, int Height) RasterSize(PdfPageInfo page, PdfOcrOptions options)
        {
            double width = Math.Ceiling(page.WidthPoints * options.Dpi / 72d);
            double height = Math.Ceiling(page.HeightPoints * options.Dpi / 72d);
            if (!double.IsFinite(width) || !double.IsFinite(height) || width < 1 || height < 1
                || width > int.MaxValue || height > int.MaxValue || width * height > options.MaxPixels)
                throw new DocumentAdapterException("RasterLimit", "Page rasterization exceeds configured dimensions/pixel budget.");
            return ((int)width, (int)height);
        }
    }
}
