using System.Text;
using FluentValidation;

namespace Aero.Storage;

/// <summary>Validated relative logical key; never a URL, bucket or local filesystem path.</summary>
public sealed record ObjectKey
{
    /// <summary>Relative slash-separated key, without traversal segments.</summary>
    public string Value { get; }
    private ObjectKey(string value) => Value = value;
    /// <summary>Rejects absolute paths, URLs, controls, escapes and empty/traversal segments.</summary>
    public static ObjectKey Parse(string value)
    {
        new ObjectKeyValidator().ValidateAndThrow(value);
        return new(value);
    }
}
internal sealed class ObjectKeyValidator : AbstractValidator<string>
{
    public ObjectKeyValidator() => RuleFor(x => x).Cascade(CascadeMode.Stop).NotEmpty()
        .Must(x => Encoding.UTF8.GetByteCount(x) <= 768)
        .Must(x => x.All(c => char.IsLetterOrDigit(c) || c is '/' or '-' or '_' or '.'))
        .Must(x => x.Split('/').All(segment => segment.Length > 0 && segment is not "." and not ".."));
}
/// <summary>Known-length upload descriptor; source bytes remain caller-owned.</summary>
public sealed record ObjectUpload(ObjectKey Key, long Length, string ContentType);
/// <summary>Boundary validation shared by storage providers.</summary>
public sealed class ObjectUploadValidator : AbstractValidator<ObjectUpload>
{
    /// <summary>Validates a descriptor against the provider's maximum upload size.</summary>
    public ObjectUploadValidator(long maximumBytes)
    {
        RuleFor(x => x.Key).NotNull();
        RuleFor(x => x.Length).InclusiveBetween(0, maximumBytes);
        RuleFor(x => x.ContentType).Cascade(CascadeMode.Stop).NotEmpty().MaximumLength(255)
            .Must(x => System.Net.Http.Headers.MediaTypeHeaderValue.TryParse(x, out _))
            .Must(x => x.All(c => c >= 32 && c <= 126));
    }
}
/// <summary>Provider-neutral metadata. ETag is opaque, not a content hash.</summary>
public sealed record ObjectMetadata(ObjectKey Key, long Length, string? ContentType, string? ETag, DateTimeOffset? ModifiedUtc);
/// <summary>Creation receipt; an existing object is a conflict, never successful deduplication.</summary>
public sealed record ObjectCreated(ObjectKey Key, long Length, string? ETag);
/// <summary>Owned download lease. Dispose the lease or its Content stream to release the provider response.</summary>
public sealed class ObjectRead(ObjectMetadata metadata, Stream content) : IDisposable, IAsyncDisposable
{
    /// <summary>Metadata for this exact response.</summary>
    public ObjectMetadata Metadata { get; } = metadata ?? throw new ArgumentNullException(nameof(metadata));
    /// <summary>Read-only stream; consume to EOF to validate transfer length.</summary>
    public Stream Content { get; } = content ?? throw new ArgumentNullException(nameof(content));
    /// <inheritdoc />
    public void Dispose() => Content.Dispose();
    /// <inheritdoc />
    public ValueTask DisposeAsync() => Content.DisposeAsync();
}
/// <summary>Role-specific create-only boundary. No overwrite or delete operation.</summary>
public interface IObjectWriter
{
    /// <summary>Atomically creates an absent object, reading from the stream's current position.
    /// Does not close the caller's stream or upload bytes preceding its current position.
    /// Provider hashing/retries may seek within that range. Concurrent mutation is unsupported.
    /// Requires a readable seekable source with remaining length equal to the descriptor.</summary>
    Task<ObjectCreated> CreateAsync(ObjectUpload upload, Stream source, CancellationToken cancellationToken = default);
}
/// <summary>Role-specific streaming read boundary. Absence is null; denied/failed operations are exceptions.</summary>
public interface IObjectReader
{
    /// <summary>Opens an owned read lease, or null when the object is absent.</summary>
    Task<ObjectRead?> OpenReadAsync(ObjectKey key, CancellationToken cancellationToken = default);
    /// <summary>Retrieves metadata without downloading bytes, or null when absent.</summary>
    Task<ObjectMetadata?> HeadAsync(ObjectKey key, CancellationToken cancellationToken = default);
}
/// <summary>Explicit composition of the two storage roles.</summary>
public interface IObjectStore : IObjectReader, IObjectWriter { }
/// <summary>Sanitized operational classifications; caller cancellation remains OperationCanceledException.</summary>
public enum StorageError
{
    /// <summary>The object already exists; bytes were not overwritten.</summary>
    AlreadyExists,
    /// <summary>Conditional write race; retry only after application reconciliation.</summary>
    Conflict,
    /// <summary>Authentication or permission failure.</summary>
    AccessDenied,
    /// <summary>Invalid provider/bucket configuration.</summary>
    Configuration,
    /// <summary>Provider quota/rate limiting.</summary>
    Throttled,
    /// <summary>Transient provider or transport failure; operation outcome can be uncertain.</summary>
    Unavailable,
    /// <summary>Provider returned invalid metadata/status.</summary>
    InvalidResponse,
    /// <summary>Download exceeds the configured byte budget.</summary>
    TooLarge,
    /// <summary>Transferred length does not match declared length.</summary>
    LengthMismatch
}
/// <summary>Safe adapter error without raw provider diagnostics, keys, endpoints or credentials.</summary>
public sealed class ObjectStorageException(StorageError error) : Exception($"Object storage operation failed ({error}).")
{
    /// <summary>Machine-readable classification.</summary>
    public StorageError Error { get; } = error;
}
