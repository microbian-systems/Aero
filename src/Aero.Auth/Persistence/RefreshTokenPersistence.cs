using Aero.Core.Entities;
using AeroDB.Sable;

namespace Aero.Auth.Persistence;

/// <summary>
/// Configures and accesses the server-side refresh-token document. The public
/// model remains browser-safe; only this document participates in Sable's
/// generated metadata and optimistic concurrency protocol.
/// </summary>
public sealed class RefreshTokenPersistence
{
    private readonly IDocumentSession session;

    internal RefreshTokenPersistence(IDocumentSession session)
    {
        this.session = session ?? throw new ArgumentNullException(nameof(session));
        EnsureGeneratedMetadata();
    }

    /// <summary>
    /// Configures the versioned server-side refresh-token table and its lookup indexes.
    /// </summary>
    public static void Configure(AeroDB.Sable.StoreOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        EnsureGeneratedMetadata();

        var mapping = options.Schema.For<RefreshTokenDocument>()
            .TableName("refresh_token")
            .SetSchemaMode(SchemaMode.Flexible);
        mapping.UseOptimisticConcurrency = true;
        mapping.UniqueIndex(token => token.TokenHash);
        mapping.Index(token => token.UserId);
    }

    internal Task<RefreshTokenDocument?> FindByHashAsync(string tokenHash, CancellationToken cancellationToken)
        => session.Query<RefreshTokenDocument>()
            .FirstOrDefaultAsync(token => token.TokenHash == tokenHash, cancellationToken);

    internal async Task<IReadOnlyList<RefreshTokenDocument>> FindActiveByUserAsync(
        long userId,
        DateTimeOffset now,
        CancellationToken cancellationToken)
        => await session.Query<RefreshTokenDocument>()
            .Where(token => token.UserId == userId
                && token.Version >= 1
                && token.RevokedAt == null
                && token.ReplacedByTokenId == null
                && token.ExpiresAt > now)
            .ToListAsync(cancellationToken);

    internal void Insert(RefreshTokenDocument token) => session.Insert(token);

    internal void UpdateExpectedVersion(RefreshTokenDocument token, long expectedVersion)
        => session.UpdateExpectedVersion(token, expectedVersion);

    internal Task<int> SaveChangesAsync(CancellationToken cancellationToken)
        => session.SaveChangesAsync(cancellationToken);

    private static void EnsureGeneratedMetadata()
    {
        var metadata = AeroDB.Sable.Metadata.MetadataRegistry.TryGet<RefreshTokenDocument>();
        if (metadata is null
            || !metadata.HasVersion
            || !string.Equals(metadata.VersionFieldName, nameof(RefreshTokenDocument.Version), StringComparison.Ordinal)
            || metadata.GetVersionAccessor is null
            || metadata.SetVersionAccessor is null
            || AeroDB.Sable.Metadata.MetadataRegistry.GetShimType(typeof(RefreshTokenDocument)) is null)
        {
            throw new InvalidOperationException(
                "Refresh-token persistence requires the generated Sable metadata and materializer. " +
                "Build this server assembly with AeroDB.SourceGenerators enabled.");
        }
    }
}

/// <summary>
/// Persisted refresh-token record. This is intentionally server-only so the
/// generated Sable materializer and concurrency metadata never become a
/// browser-shared model dependency.
/// </summary>
internal sealed class RefreshTokenDocument : Entity, ISableDocument<long>, IVersioned
{
    public long UserId { get; set; }

    public string TokenHash { get; set; } = string.Empty;

    public DateTimeOffset ExpiresAt { get; set; }

    public DateTimeOffset? RevokedAt { get; set; }

    public long? ReplacedByTokenId { get; set; }

    public string ClientType { get; set; } = "web";

    public string? IssuedFromIpAddress { get; set; }

    public string? UserAgent { get; set; }

    public long Version { get; set; }

    internal bool IsActiveAt(DateTimeOffset now)
        => RevokedAt == null && ReplacedByTokenId == null && ExpiresAt > now;
}
