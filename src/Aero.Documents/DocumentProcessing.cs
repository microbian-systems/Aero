using System.IO.Compression;
using FluentValidation;

namespace Aero.Documents;

/// <summary>Shared validation/error boundary for file-specific processors.</summary>
public abstract class DocumentProcessor : IDocumentProcessor
{
    /// <inheritdoc />
    public abstract string Extension { get; }
    /// <inheritdoc />
    public ExtractionResult Process(byte[] content, ExtractionLimits limits, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(content);
        ArgumentNullException.ThrowIfNull(limits);
        new ExtractionLimitsValidator().ValidateAndThrow(limits);
        cancellationToken.ThrowIfCancellationRequested();
        if (content.Length > limits.MaxInputBytes)
            return Failure("InputLimit", "Input exceeds the configured byte limit.");
        try { return Extract(content, limits, cancellationToken); }
        catch (DocumentBoundaryException error)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Failure(error.Code, error.Message);
        }
        catch (Exception error) when (error is not OperationCanceledException
            && error is not OutOfMemoryException && error is not StackOverflowException)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Failure("InvalidDocument", "The document is malformed, encrypted or unsupported by this processor.");
        }
    }
    /// <summary>Extracts validated bytes; implementers must respect limits and cancellation.</summary>
    protected abstract ExtractionResult Extract(byte[] content, ExtractionLimits limits, CancellationToken cancellationToken);
    /// <summary>Constructs an extraction failure without exposing library exceptions.</summary>
    protected ExtractionResult Failure(string code, string message) =>
        new(Extension + ":1", [], [new(code, message)]);

    internal static void CheckOfficePackage(byte[] content, ExtractionLimits limits, string requiredPart,
        CancellationToken cancellationToken)
    {
        using var stream = new MemoryStream(content, false);
        using var zip = new ZipArchive(stream, ZipArchiveMode.Read);
        if (zip.Entries.Count > limits.MaxArchiveEntries)
            throw new DocumentBoundaryException("ArchiveLimit", "Office package entry limit exceeded.");
        long expanded = 0;
        foreach (var entry in zip.Entries)
        {
            cancellationToken.ThrowIfCancellationRequested();
            expanded = checked(expanded + entry.Length);
            if (expanded > limits.MaxExpandedBytes)
                throw new DocumentBoundaryException("ExpandedLimit", "Office package decompression limit exceeded.");
            if (entry.FullName.EndsWith("vbaProject.bin", StringComparison.OrdinalIgnoreCase))
                throw new DocumentBoundaryException("UnsupportedMacros", "Macro-bearing packages are not supported.");
        }
        if (zip.GetEntry(requiredPart) is null) throw new InvalidDataException();
    }
}

/// <summary>An explicit extraction-budget failure converted to a structured issue by DocumentProcessor.</summary>
public sealed class DocumentBoundaryException(string code, string message) : Exception(message)
{
    /// <summary>Stable machine-readable boundary failure code.</summary>
    public string Code { get; } = code;
}

/// <summary>Explicit extension routing; reads from the current stream position and leaves it open.</summary>
public sealed class DocumentProcessorRegistry
{
    private readonly IReadOnlyDictionary<string, IDocumentProcessor> processors;
    /// <summary>Constructs a registry without reflection or automatic plugin discovery.</summary>
    public DocumentProcessorRegistry(IEnumerable<IDocumentProcessor> processors)
    {
        ArgumentNullException.ThrowIfNull(processors);
        this.processors = processors.ToDictionary(x => x.Extension, StringComparer.OrdinalIgnoreCase);
    }
    /// <summary>The initial PDF/DOCX/XLSX/TXT set. Legacy binary Office is intentionally excluded.</summary>
    public static DocumentProcessorRegistry CreateDefault() => new([
        new PdfDocumentProcessor(), new WordDocumentProcessor(),
        new SpreadsheetDocumentProcessor(), new PlainTextDocumentProcessor()]);
    /// <summary>Extracts with a bounded buffer; no remote URLs, storage destinations or credentials accepted.</summary>
    public async Task<ExtractionResult> ExtractAsync(string fileName, Stream input,
        ExtractionLimits? limits = null, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);
        ArgumentNullException.ThrowIfNull(input);
        limits ??= new();
        new ExtractionLimitsValidator().ValidateAndThrow(limits);
        cancellationToken.ThrowIfCancellationRequested();
        var extension = Path.GetExtension(fileName);
        if (!processors.TryGetValue(extension, out var processor))
            return new("unsupported", [], [new("UnsupportedFormat", "No processor is registered for this file extension.")]);
        using var buffer = new MemoryStream();
        var scratch = new byte[8192];
        while (true)
        {
            int read = await input.ReadAsync(scratch, cancellationToken);
            if (read == 0) break;
            if (buffer.Length + read > limits.MaxInputBytes)
                return new(extension + ":1", [], [new("InputLimit", "Input exceeds the configured byte limit.")]);
            await buffer.WriteAsync(scratch.AsMemory(0, read), cancellationToken);
        }
        return processor.Process(buffer.ToArray(), limits, cancellationToken);
    }
}
