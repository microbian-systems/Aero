using FluentValidation;

namespace Aero.Documents.Ocr;

/// <summary>Native PDF text and page geometry; presence of an image is not proof of scanned text.</summary>
public sealed record PdfPageInfo(int Number, string Text, bool HasImages, double WidthPoints, double HeightPoints);
/// <summary>Bounded inspection results, including omitted-page or text-limit warnings.</summary>
public sealed record PdfInspection(IReadOnlyList<PdfPageInfo> Pages, IReadOnlyList<DocumentIssue> Issues);
/// <summary>Reads native text and image presence without invoking OCR.</summary>
public interface IPdfPageInspector
{
    /// <summary>One-based page numbers; cancellation propagates.</summary>
    PdfInspection Inspect(byte[] pdf, ExtractionLimits limits, CancellationToken cancellationToken);
}
/// <summary>Independent page-selection strategy.</summary>
public interface IOcrPagePolicy
{
    /// <summary>Only image-bearing pages without usable native text are selected by the initial policy.</summary>
    bool ShouldRecognize(PdfPageInfo page);
}
/// <summary>PNG output and its actual image geometry.</summary>
public sealed record RenderedPage(byte[] Png, int Width, int Height);
/// <summary>Swappable rasterization adapter; implementations must respect dimensions and byte limits.</summary>
public interface IPdfPageRenderer
{
    /// <summary>Renders a one-based page; no storage or network side effects.</summary>
    Task<RenderedPage> RenderAsync(byte[] pdf, int page, int width, int height, int maxBytes, CancellationToken cancellationToken);
}
/// <summary>A word and top-left-origin pixel coordinates; confidence is an engine score, not a probability.</summary>
public sealed record OcrWord(string Text, int Left, int Top, int Width, int Height, double Confidence,
    int Block, int Paragraph, int Line);
/// <summary>Engine text/evidence and explicit processing issues.</summary>
public sealed record OcrResult(string Engine, string Text, IReadOnlyList<OcrWord> Words, IReadOnlyList<DocumentIssue> Issues);
/// <summary>Swappable local recognition adapter.</summary>
public interface IOcrEngine
{
    /// <summary>Recognizes a bounded PNG. Cancellation propagates; operational failures are issues.</summary>
    Task<OcrResult> RecognizeAsync(RenderedPage image, CancellationToken cancellationToken);
}
/// <summary>OCR evidence retained separately from text chunks for source review.</summary>
public sealed record OcrPageEvidence(int Page, int ImageWidth, int ImageHeight, OcrResult Recognition);
/// <summary>Source-anchored document text plus retained OCR evidence.</summary>
public sealed record PdfOcrExtraction(ExtractionResult Document, IReadOnlyList<OcrPageEvidence> OcrPages);
/// <summary>Host-supplied local OCR budgets. Native parsing still requires worker isolation.</summary>
public sealed record PdfOcrOptions
{
    /// <summary>Maximum selected pages recognized per document.</summary>
    public int MaxOcrPages { get; init; } = 25;
    /// <summary>Target rasterization resolution.</summary>
    public int Dpi { get; init; } = 300;
    /// <summary>Maximum pixels per page; oversized pages are rejected, not silently downscaled.</summary>
    public long MaxPixels { get; init; } = 16_000_000;
    /// <summary>Maximum PNG bytes returned by the renderer.</summary>
    public int MaxImageBytes { get; init; } = 20 * 1024 * 1024;
    /// <summary>Threshold for flagging uncertain words; not an accuracy guarantee.</summary>
    public double MinimumWordConfidence { get; init; } = 70;
}
internal sealed class PdfOcrOptionsValidator : AbstractValidator<PdfOcrOptions>
{
    public PdfOcrOptionsValidator()
    {
        RuleFor(x => x.MaxOcrPages).InclusiveBetween(1, 1000);
        RuleFor(x => x.Dpi).InclusiveBetween(72, 600);
        RuleFor(x => x.MaxPixels).InclusiveBetween(1, 64_000_000);
        RuleFor(x => x.MaxImageBytes).InclusiveBetween(1, 100 * 1024 * 1024);
        RuleFor(x => x.MinimumWordConfidence).InclusiveBetween(0, 100);
    }
}
/// <summary>Only expected adapter failures are converted into sanitized issues.</summary>
public sealed class DocumentAdapterException(string code, string message) : Exception(message)
{
    /// <summary>Stable machine-readable failure code.</summary>
    public string Code { get; } = code;
}
