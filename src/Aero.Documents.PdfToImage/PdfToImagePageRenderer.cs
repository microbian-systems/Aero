using Aero.Documents.Ocr;
using PDFtoImage;

namespace Aero.Documents.PdfToImage;

/// <summary>MIT PDFtoImage adapter. Native PDFium rendering must be hosted in an isolated worker.</summary>
public sealed class PdfToImagePageRenderer : IPdfPageRenderer
{
    /// <inheritdoc />
    public Task<RenderedPage> RenderAsync(byte[] pdf, int page, int width, int height, int maxBytes, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(pdf);
        ArgumentOutOfRangeException.ThrowIfLessThan(page, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(width, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(height, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(maxBytes, 1);
        cancellationToken.ThrowIfCancellationRequested();
        if (!OperatingSystem.IsWindows() && !OperatingSystem.IsLinux() && !OperatingSystem.IsMacOS())
            throw new DocumentAdapterException("RuntimeUnavailable", "This renderer adapter supports desktop/server Windows, Linux and macOS only.");
        if ((long)width * height > 64_000_000)
            throw new DocumentAdapterException("RasterLimit", "Raster pixel limit exceeded.");
        try
        {
            // PDFtoImage's platform annotations use noncanonical platform names;
            // the explicit server/desktop OS guard above is the runtime boundary.
#pragma warning disable CA1416
            using var bitmap = Conversion.ToImage(pdf, page: page - 1,
                options: new RenderOptions(Width: width, Height: height, WithAnnotations: false, WithFormFill: false));
#pragma warning restore CA1416
            cancellationToken.ThrowIfCancellationRequested();
            if (bitmap.Width != width || bitmap.Height != height)
                throw new DocumentAdapterException("RasterDimensions", "Renderer returned unexpected dimensions.");
            using var data = bitmap.Encode(SkiaSharp.SKEncodedImageFormat.Png, 100);
            if (data is null || data.Size > maxBytes)
                throw new DocumentAdapterException("RasterLimit", "Encoded page exceeds PNG byte budget.");
            return Task.FromResult(new RenderedPage(data.ToArray(), bitmap.Width, bitmap.Height));
        }
        catch (Exception e) when (e is not OperationCanceledException and not OutOfMemoryException
            and not StackOverflowException and not DocumentAdapterException)
        {
            cancellationToken.ThrowIfCancellationRequested();
            throw new DocumentAdapterException("RenderFailed", "PDF rendering failed; verify native runtime dependencies and input.");
        }
    }
}
