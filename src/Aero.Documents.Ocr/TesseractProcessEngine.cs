using FluentValidation;

namespace Aero.Documents.Ocr;

/// <summary>Trusted operator configuration, not tenant-supplied paths or CLI flags.</summary>
public sealed record TesseractOptions
{
    /// <summary>Absolute path or trusted deployment PATH executable name.</summary>
    public string Executable { get; init; } = "tesseract";
    /// <summary>Deadline per page.</summary>
    public TimeSpan Timeout { get; init; } = TimeSpan.FromSeconds(45);
    /// <summary>Bounded TSV output.</summary>
    public int MaxOutputCharacters { get; init; } = 4_000_000;
    /// <summary>Maximum recognized words per page.</summary>
    public int MaxWords { get; init; } = 20000;
    /// <summary>Maximum recognized text characters per page.</summary>
    public int MaxTextCharacters { get; init; } = 200000;
    /// <summary>Maximum PNG input bytes.</summary>
    public int MaxImageBytes { get; init; } = 20 * 1024 * 1024;
}
internal sealed class TesseractOptionsValidator : AbstractValidator<TesseractOptions>
{
    public TesseractOptionsValidator()
    {
        RuleFor(x => x.Executable).NotEmpty();
        RuleFor(x => x.Timeout).GreaterThan(TimeSpan.Zero).LessThanOrEqualTo(TimeSpan.FromMinutes(10));
        RuleFor(x => x.MaxOutputCharacters).InclusiveBetween(1, 16_000_000);
        RuleFor(x => x.MaxWords).InclusiveBetween(1, 100000);
        RuleFor(x => x.MaxTextCharacters).InclusiveBetween(1, 1000000);
        RuleFor(x => x.MaxImageBytes).InclusiveBetween(1, 100 * 1024 * 1024);
    }
}
/// <summary>English local CLI recognition; no shell, paid provider or model fallback.</summary>
public sealed class TesseractProcessEngine : IOcrEngine
{
    private readonly ILocalProcessRunner runner;
    private readonly TesseractOptions options;
    private readonly TesseractTsvParser parser;
    /// <summary>Constructs an adapter with an injected process boundary.</summary>
    public TesseractProcessEngine(ILocalProcessRunner runner, TesseractTsvParser parser, TesseractOptions? options = null)
    {
        this.runner = runner ?? throw new ArgumentNullException(nameof(runner));
        this.parser = parser ?? throw new ArgumentNullException(nameof(parser));
        this.options = options ?? new();
        new TesseractOptionsValidator().ValidateAndThrow(this.options);
    }
    /// <inheritdoc />
    public async Task<OcrResult> RecognizeAsync(RenderedPage image, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(image);
        cancellationToken.ThrowIfCancellationRequested();
        OcrResult Failure(string code, string message) => new("tesseract-cli-tsv:1", "", [], [new(code, message)]);
        if (!PngValidation.IsBounded(image, options.MaxImageBytes))
            return Failure("InvalidImage", "OCR requires a bounded PNG with matching image dimensions.");
        var workspace = Directory.CreateTempSubdirectory("aero-ocr-");
        OcrResult result;
        bool cleanupFailed = false;
        try
        {
            string input = Path.Combine(workspace.FullName, "page.png");
            await File.WriteAllBytesAsync(input, image.Png, cancellationToken);
            var process = await runner.RunAsync(new(options.Executable,
                [input, "stdout", "-l", "eng", "--oem", "1", "tsv"], options.Timeout, options.MaxOutputCharacters,
                new Dictionary<string, string> { ["OMP_THREAD_LIMIT"] = "1" }), cancellationToken);
            result = process.ExitCode != 0 ? Failure("OcrFailed", "Local OCR process failed; verify runtime/language data and source image.")
                : parser.Parse(process.Output, image.Width, image.Height, options.MaxWords, options.MaxTextCharacters);
        }
        catch (DocumentAdapterException error)
        {
            cancellationToken.ThrowIfCancellationRequested();
            result = Failure(error.Code, error.Message);
        }
        catch (IOException)
        {
            cancellationToken.ThrowIfCancellationRequested();
            result = Failure("OcrIoFailure", "Local OCR workspace could not be accessed.");
        }
        catch (UnauthorizedAccessException)
        {
            cancellationToken.ThrowIfCancellationRequested();
            result = Failure("OcrIoFailure", "Local OCR workspace could not be accessed.");
        }
        finally
        {
            // This exact directory was exclusively created by this invocation, never a caller path.
            try { workspace.Delete(recursive: true); }
            catch (IOException) { cleanupFailed = true; }
            catch (UnauthorizedAccessException) { cleanupFailed = true; }
        }
        if (cleanupFailed) result = result with { Issues = [.. result.Issues,
            new("WorkspaceCleanupFailed", "Local temporary image cleanup failed; the host must remove abandoned workspaces.")] };
        return result;
    }
}
