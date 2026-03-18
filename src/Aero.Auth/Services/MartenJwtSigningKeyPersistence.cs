using Aero.MartenDB;
using Marten;
using Marten.Patching;
using Microsoft.Extensions.Logging;

namespace Aero.Auth.Services;

/// <summary>
/// Marten implementation of JWT signing key persistence.
/// </summary>
public class MartenJwtSigningKeyPersistence : IJwtSigningKeyPersistence
{
    private readonly IAeroDbUnitOfWork _uow;
    private readonly ILogger<MartenJwtSigningKeyPersistence> _logger;
    private const string KeyCollectionName = "JwtSigningKeys";

    public MartenJwtSigningKeyPersistence(
        IAeroDbUnitOfWork uow,
        ILogger<MartenJwtSigningKeyPersistence> logger)
    {
        _uow = uow ?? throw new ArgumentNullException(nameof(uow));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<JwtSigningKey?> GetCurrentSigningKeyAsync(CancellationToken cancellationToken = default)
    {
        var session = GetSession();

        var key = await session
            .Query<JwtSigningKey>()
            .Where(k => k.IsCurrentSigningKey && k.RevokedAt == null)
            .FirstOrDefaultAsync(cancellationToken);

        _logger.LogDebug("Retrieved current signing key: {KeyId}", key?.KeyId ?? "not found");
        return key;
    }

    public async Task<IEnumerable<JwtSigningKey>> GetValidSigningKeysAsync(CancellationToken cancellationToken = default)
    {
        var session = GetSession();

        var keys = await session
            .Query<JwtSigningKey>()
            .Where(k => k.RevokedAt == null)
            .ToListAsync(cancellationToken);

        _logger.LogDebug("Retrieved {Count} valid signing keys", keys.Count);
        return keys;
    }

    public async Task<JwtSigningKey?> GetKeyByIdAsync(string keyId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(keyId))
        {
            throw new ArgumentException("Key ID cannot be null or empty", nameof(keyId));
        }

        var session = GetSession();

        var key = await session
            .Query<JwtSigningKey>()
            .Where(k => k.KeyId == keyId && k.RevokedAt == null)
            .FirstOrDefaultAsync(cancellationToken);

        _logger.LogDebug("Retrieved signing key by ID: {KeyId}", keyId);
        return key;
    }

    public async Task<bool> AddKeyAsync(JwtSigningKey key, CancellationToken cancellationToken = default)
    {
        if (key == null)
        {
            throw new ArgumentNullException(nameof(key));
        }

        var session = GetSession();

        try
        {
            key.Id ??= $"{KeyCollectionName}/{Guid.NewGuid()}";
            session.Store(key); // Store does not accept a CancellationToken
            _logger.LogInformation("Added new signing key: {KeyId}", key.KeyId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add signing key: {KeyId}", key.KeyId);
            return false;
        }
    }

    public async Task<bool> UpdateKeyAsync(JwtSigningKey key, CancellationToken cancellationToken = default)
    {
        if (key == null)
        {
            throw new ArgumentNullException(nameof(key));
        }

        try
        {
            var session = GetSession();

            session.Patch<JwtSigningKey>(key.Id)
                   .Set(x => x.IsCurrentSigningKey, key.IsCurrentSigningKey)
                   .Set(x => x.ModifiedOn, key.ModifiedOn);

            if (key.RevokedAt.HasValue)
            {
                session.Patch<JwtSigningKey>(key.Id)
                       .Set(x => x.RevokedAt, key.RevokedAt);
            }

            _logger.LogInformation("Updated signing key: {KeyId}", key.KeyId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update signing key: {KeyId}", key.KeyId);
            return false;
        }
    }

    public async Task<bool> DeactivateCurrentKeyAsync(CancellationToken cancellationToken = default)
    {
        var session = GetSession();

        try
        {
            var currentKey = await GetCurrentSigningKeyAsync(cancellationToken);

            if (currentKey == null)
            {
                _logger.LogWarning("No current signing key to deactivate");
                return true;
            }

            session.Patch<JwtSigningKey>(currentKey.Id)
                   .Set(x => x.IsCurrentSigningKey, false)
                   .Set(x => x.ModifiedOn, DateTimeOffset.UtcNow);

            _logger.LogInformation("Deactivated current signing key: {KeyId}", currentKey.KeyId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to deactivate current signing key");
            return false;
        }
    }

    public async Task<bool> RevokeKeyAsync(string keyId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(keyId))
        {
            throw new ArgumentException("Key ID cannot be null or empty", nameof(keyId));
        }

        var session = GetSession();

        try
        {
            var key = await GetKeyByIdAsync(keyId, cancellationToken);

            if (key == null)
            {
                _logger.LogWarning("Key not found for revocation: {KeyId}", keyId);
                return false;
            }

            session.Patch<JwtSigningKey>(key.Id)
                   .Set(x => x.RevokedAt, DateTimeOffset.UtcNow)
                   .Set(x => x.ModifiedOn, DateTimeOffset.UtcNow);

            _logger.LogInformation("Revoked signing key: {KeyId}", keyId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to revoke signing key: {KeyId}", keyId);
            return false;
        }
    }

    public async Task<bool> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _uow.SaveChangesAsync(cancellationToken);
            _logger.LogDebug("Saved changes to signing keys store");
            return result > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save changes to signing keys store");
            return false;
        }
    }

    /// <summary>
    /// Gets the current Marten document session.
    /// </summary>
    private IDocumentSession GetSession()
    {
        return _uow.Session;
    }
}
