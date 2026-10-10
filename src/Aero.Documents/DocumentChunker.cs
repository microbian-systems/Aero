using FluentValidation;

namespace Aero.Documents;

/// <summary>Model-independent character limits; the embedding adapter must enforce its token budget.</summary>
public sealed record ChunkingOptions
{
    /// <summary>Maximum characters including repeated context.</summary>
    public int MaxCharacters { get; init; } = 1500;
    /// <summary>Maximum context characters per chunk.</summary>
    public int MaxContextCharacters { get; init; } = 300;
}
internal sealed class ChunkingOptionsValidator : AbstractValidator<ChunkingOptions>
{
    public ChunkingOptionsValidator()
    {
        RuleFor(x => x.MaxCharacters).InclusiveBetween(128, 16384);
        RuleFor(x => x.MaxContextCharacters).GreaterThanOrEqualTo(0)
            .LessThanOrEqualTo(x => x.MaxCharacters / 2);
    }
}
/// <summary>Source content with a UTF-16 offset within its block and separately labelled context.</summary>
public sealed record DocumentChunk(int Ordinal, int BlockOrdinal, int CharacterOffset,
    string Content, string Context, DocumentLocation Location);

/// <summary>Bounded chunks preserve processor block boundaries and repeat bounded source context.</summary>
public sealed class DocumentChunker
{
    /// <summary>Chunks extracted text without AI calls, embeddings, dropping text or inventing headers.</summary>
    public IReadOnlyList<DocumentChunk> Chunk(ExtractionResult document, ChunkingOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(document);
        options ??= new();
        new ChunkingOptionsValidator().ValidateAndThrow(options);
        cancellationToken.ThrowIfCancellationRequested();
        var chunks = new List<DocumentChunk>();
        for (int blockIndex = 0; blockIndex < document.Blocks.Count; blockIndex++)
        {
            var block = document.Blocks[blockIndex];
            int contextLength = SafeLength(block.Context, 0, Math.Min(block.Context.Length, options.MaxContextCharacters));
            string context = block.Context[..contextLength];
            int budget = options.MaxCharacters - context.Length;
            for (int offset = 0; offset < block.Text.Length;)
            {
                cancellationToken.ThrowIfCancellationRequested();
                int length = SafeLength(block.Text, offset, Math.Min(budget, block.Text.Length - offset));
                if (offset + length < block.Text.Length)
                {
                    // Prefer word boundaries without trimming away source characters.
                    for (int i = length - 1; i >= length / 2; i--)
                        if (char.IsWhiteSpace(block.Text[offset + i])) { length = i + 1; break; }
                }
                chunks.Add(new(chunks.Count, blockIndex, offset, block.Text.Substring(offset, length), context, block.Location));
                offset += length;
            }
        }
        return chunks;
    }
    private static int SafeLength(string text, int start, int length) =>
        length > 0 && start + length < text.Length && char.IsHighSurrogate(text[start + length - 1])
        && char.IsLowSurrogate(text[start + length]) ? length - 1 : length;
}
