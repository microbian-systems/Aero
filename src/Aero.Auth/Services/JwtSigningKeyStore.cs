using System.Security.Cryptography;

namespace Aero.Auth.Services;

/// <summary>
/// Production-grade JWT signing key store with rotation support.
/// Keys are persisted via abstracted persistence layer for flexibility (AeroDB, EF Core, etc).
/// Can be rotated without downtime, with in-memory caching for performance.
/// </summary>
public class JwtSigningKeyStore(
    IJwtSigningKeyPersistence persistence,
    ILogger<JwtSigningKeyStore> logger,
    IMemoryCache cache)
    : IJwtSigningKeyStore
{
    private readonly IJwtSigningKeyPersistence _persistence = persistence ?? throw new ArgumentNullException(nameof(persistence));
    private readonly ILogger<JwtSigningKeyStore> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IMemoryCache _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    private const string CurrentKeyIdCacheKey = "jwt:current_key_id";
    private const string AllKeysCacheKey = "jwt:all_keys";
    private const int CacheDurationMinutes = 5;

    /// <summary>
    /// Gets the current signing key used to sign new tokens.
    /// If no key is found, it automatically initializes a new one.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The security key.</returns>
    public async Task<SecurityKey> GetCurrentSigningKeyAsync(CancellationToken cancellationToken = default)
    {
        var key = await _persistence.GetCurrentSigningKeyAsync(cancellationToken);

        if (key == null)
        {
            _logger.LogInformation("No current signing key found. Initializing new signing key.");
            await RotateSigningKeyAsync(cancellationToken);
            key = await _persistence.GetCurrentSigningKeyAsync(cancellationToken);

            if (key == null)
            {
                throw new InvalidOperationException(
                    "No current signing key found and failed to initialize one. Check persistence layer.");
            }
        }

        return new SymmetricSecurityKey(Convert.FromBase64String(key.KeyMaterial));
    }

    /// <summary>
    /// Gets the key identifier (kid) for the current signing key.
    /// If no key is found, it automatically initializes a new one.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The key identifier.</returns>
    public async Task<string> GetCurrentKeyIdAsync(CancellationToken cancellationToken = default)
    {
        // Try cache first
        if (_cache.TryGetValue(CurrentKeyIdCacheKey, out string? cachedKeyId))
        {
            return cachedKeyId!;
        }

        // Query persistence layer
        var currentKey = await _persistence.GetCurrentSigningKeyAsync(cancellationToken);

        if (currentKey == null)
        {
            _logger.LogInformation("No current signing key found. Initializing new signing key.");
            await RotateSigningKeyAsync(cancellationToken);
            currentKey = await _persistence.GetCurrentSigningKeyAsync(cancellationToken);

            if (currentKey == null)
            {
                throw new InvalidOperationException("No current signing key found and failed to initialize one.");
            }
        }

        // Cache result
        _cache.Set(CurrentKeyIdCacheKey, currentKey.KeyId, TimeSpan.FromMinutes(CacheDurationMinutes));

        return currentKey.KeyId;
    }

    /// <summary>
    /// Gets all valid (non-revoked) keys used for validating tokens.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of security keys.</returns>
    public async Task<IEnumerable<SecurityKey>> GetValidationKeysAsync(CancellationToken cancellationToken = default)
    {
        // Try cache first
        if (_cache.TryGetValue(AllKeysCacheKey, out IEnumerable<SecurityKey>? cachedKeys))
        {
            return cachedKeys!;
        }

        // Query persistence layer
        var keys = await _persistence.GetValidSigningKeysAsync(cancellationToken);

        var securityKeys = keys
            .Select(k => (SecurityKey)new SymmetricSecurityKey(Convert.FromBase64String(k.KeyMaterial)))
            .ToList();

        // Cache result
        _cache.Set(AllKeysCacheKey, securityKeys, TimeSpan.FromMinutes(CacheDurationMinutes));

        return securityKeys;
    }

    /// <summary>
    /// Gets the signing credentials (key + algorithm) for the current key.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The signing credentials.</returns>
    public async Task<SigningCredentials> GetSigningCredentialsAsync(CancellationToken cancellationToken = default)
    {
        var key = await GetCurrentSigningKeyAsync(cancellationToken);
        return new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
    }

    /// <summary>
    /// Rotates the signing key - marks current as old and creates new signing key.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The new key identifier.</returns>
    public async Task<string> RotateSigningKeyAsync(CancellationToken cancellationToken = default)
    {
        // Deactivate current signing key
        var deactivated = await _persistence.DeactivateCurrentKeyAsync(cancellationToken);
        if (!deactivated)
        {
            _logger.LogWarning("Failed to deactivate current signing key during rotation");
        }

        // Create new signing key
        var newKey = new JwtSigningKey
        {
            Id = Guid.NewGuid().ToString(),
            KeyId = Guid.NewGuid().ToString("N").Substring(0, 16),
            KeyMaterial = Convert.ToBase64String(GenerateRandomKey(32)),
            CreatedOn = DateTimeOffset.UtcNow,
            IsCurrentSigningKey = true,
            Algorithm = SecurityAlgorithms.HmacSha256
        };

        // Add new key to persistence
        var added = await _persistence.AddKeyAsync(newKey, cancellationToken);
        if (!added)
        {
            throw new InvalidOperationException("Failed to add new signing key during rotation");
        }

        // Save changes
        var saved = await _persistence.SaveChangesAsync(cancellationToken);
        if (!saved)
        {
            _logger.LogWarning("Failed to persist signing key rotation to store");
        }

        // Invalidate cache
        _cache.Remove(CurrentKeyIdCacheKey);
        _cache.Remove(AllKeysCacheKey);

        _logger.LogInformation("Successfully rotated signing key. New KeyId: {KeyId}", newKey.KeyId);

        return newKey.KeyId;
    }

    /// <summary>
    /// Revokes a specific key by ID.
    /// </summary>
    /// <param name="keyId">The key identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public async Task RevokeKeyAsync(string keyId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(keyId, nameof(keyId));

        var revoked = await _persistence.RevokeKeyAsync(keyId, cancellationToken);
        if (!revoked)
        {
            throw new InvalidOperationException($"Failed to revoke signing key: {keyId}");
        }

        var saved = await _persistence.SaveChangesAsync(cancellationToken);
        if (!saved)
        {
            _logger.LogWarning("Failed to persist key revocation to store");
        }

        // Invalidate cache
        _cache.Remove(AllKeysCacheKey);
        _logger.LogInformation("Revoked signing key: {KeyId}", keyId);
    }

    /// <summary>
    /// Gets a key by its ID (for validation based on JWT kid header).
    /// </summary>
    /// <param name="keyId">The key identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The security key, or null if not found.</returns>
    public async Task<SecurityKey?> GetKeyByIdAsync(string keyId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(keyId, nameof(keyId));

        var key = await _persistence.GetKeyByIdAsync(keyId, cancellationToken);

        if (key == null)
        {
            return null;
        }

        return new SymmetricSecurityKey(Convert.FromBase64String(key.KeyMaterial));
    }

    private static byte[] GenerateRandomKey(int length)
    {
        return RandomNumberGenerator.GetBytes(length);
    }
}
