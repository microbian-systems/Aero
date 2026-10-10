using FluentValidation;

namespace Aero.Documents;

/// <summary>Host-configured resource limits. These do not replace process isolation.</summary>
public sealed record ExtractionLimits
{
    /// <summary>Maximum input bytes.</summary>
    public int MaxInputBytes { get; init; } = 20 * 1024 * 1024;
    /// <summary>Maximum total advertised uncompressed Office package bytes.</summary>
    public long MaxExpandedBytes { get; init; } = 64L * 1024 * 1024;
    /// <summary>Maximum Office package entries.</summary>
    public int MaxArchiveEntries { get; init; } = 2048;
    /// <summary>Maximum pages, rows or paragraphs visited.</summary>
    public int MaxUnits { get; init; } = 10000;
    /// <summary>Maximum cells visited across a workbook.</summary>
    public int MaxCells { get; init; } = 100000;
    /// <summary>Maximum characters retained across all blocks and contexts.</summary>
    public int MaxCharacters { get; init; } = 1000000;
}

/// <summary>Shared configuration validator for extraction adapters.</summary>
public sealed class ExtractionLimitsValidator : AbstractValidator<ExtractionLimits>
{
    /// <summary>Defines bounded host-configured extraction budgets.</summary>
    public ExtractionLimitsValidator()
    {
        RuleFor(x => x.MaxInputBytes).InclusiveBetween(1, 100 * 1024 * 1024);
        RuleFor(x => x.MaxExpandedBytes).InclusiveBetween(1, 256L * 1024 * 1024);
        RuleFor(x => x.MaxArchiveEntries).InclusiveBetween(1, 10000);
        RuleFor(x => x.MaxUnits).InclusiveBetween(1, 100000);
        RuleFor(x => x.MaxCells).InclusiveBetween(1, 1000000);
        RuleFor(x => x.MaxCharacters).InclusiveBetween(1, 10000000);
    }
}

/// <summary>A stable location within the original document.</summary>
public sealed record DocumentLocation(string Part, int? Page = null, string? Sheet = null,
    uint? Row = null, string? CellRange = null);
/// <summary>Text and bounded preceding context; context is not an inferred header.</summary>
public sealed record DocumentBlock(string Text, DocumentLocation Location, string Context = "");
/// <summary>A structured coverage or processing issue. Messages contain no exception payloads.</summary>
public sealed record DocumentIssue(string Code, string Message, DocumentLocation? Location = null);
/// <summary>Extraction output only; not a claim of complete visual or semantic interpretation.</summary>
public sealed record ExtractionResult(string Processor, IReadOnlyList<DocumentBlock> Blocks,
    IReadOnlyList<DocumentIssue> Issues)
{
    /// <summary>Whether the extractor reported no coverage gaps or failures.</summary>
    public bool IsComplete => Issues.Count == 0;
}
/// <summary>One explicit processor for one file extension; content must also match its format.</summary>
public interface IDocumentProcessor
{
    /// <summary>Supported extension, including its leading dot.</summary>
    string Extension { get; }
    /// <summary>Extracts bounded content. Cancellation propagates; no network or AI calls occur.</summary>
    ExtractionResult Process(byte[] content, ExtractionLimits limits, CancellationToken cancellationToken = default);
}

internal sealed class ExtractionBuffer(string processor, ExtractionLimits limits)
{
    private readonly List<DocumentBlock> blocks = [];
    private readonly List<DocumentIssue> issues = [];
    private int retainedCharacters;
    public bool Exhausted { get; private set; }
    public void Issue(string code, string message, DocumentLocation? location = null)
    {
        if (!issues.Any(x => x.Code == code && x.Location == location))
            issues.Add(new(code, message, location));
    }
    public bool Add(string text, DocumentLocation location, string context = "")
    {
        if (Exhausted) return false;
        if (string.IsNullOrWhiteSpace(text)) return true;
        int available = limits.MaxCharacters - retainedCharacters;
        if (text.Length + context.Length > available)
        {
            // Do not keep half a row/paragraph or discard its context silently.
            Issue("TextLimit", "Text retention limit reached; remaining content was not retained.", location);
            Exhausted = true;
            return false;
        }
        blocks.Add(new(text, location, context));
        retainedCharacters += text.Length + context.Length;
        return true;
    }
    public ExtractionResult Result()
    {
        if (blocks.Count == 0 && issues.Count == 0)
            Issue("NoText", "No extractable text was found.");
        return new(processor, blocks.ToArray(), issues.ToArray());
    }
}
