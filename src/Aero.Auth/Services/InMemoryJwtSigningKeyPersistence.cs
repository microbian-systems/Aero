namespace Aero.Auth.Services;

/// <summary>
/// In-memory implementation of JWT signing key persistence.
/// Useful for testing and development. Keys are not persisted across application restarts.
/// </summary>
public class InMemoryJwtSigningKeyPersistence : IJwtSigningKeyPersistence
{
    private readonly Dictionary<string, JwtSigningKey> _keys = new();
    private string? _currentKeyId;

        /// <summary>
    /// GetCurrentSigningKeyAsync method.
    /// </summary>
public Task<JwtSigningKey?> GetCurrentSigningKeyAsync(CancellationToken cancellationToken = default)
    {
        if (_currentKeyId is null || !_keys.TryGetValue(_currentKeyId, out var key))
        {
            return Task.FromResult((JwtSigningKey?)null);
        }

        return Task.FromResult((JwtSigningKey?)key);
    }

        /// <summary>
    /// GetValidSigningKeysAsync method.
    /// </summary>
public Task<IEnumerable<JwtSigningKey>> GetValidSigningKeysAsync(CancellationToken cancellationToken = default)
    {
        var validKeys = _keys.Values
            .Where(k => k.RevokedAt == null)
            .AsEnumerable();
        return Task.FromResult(validKeys);
    }

        /// <summary>
    /// GetKeyByIdAsync method.
    /// </summary>
public Task<JwtSigningKey?> GetKeyByIdAsync(string keyId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(keyId, nameof(keyId));
        
        _keys.TryGetValue(keyId, out var key);
        return Task.FromResult(key);
    }

        /// <summary>
    /// AddKeyAsync method.
    /// </summary>
public Task<bool> AddKeyAsync(JwtSigningKey key, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(key, nameof(key));
        ArgumentException.ThrowIfNullOrEmpty(key.KeyId, nameof(key.KeyId));

        _keys[key.KeyId] = key;
        if (key.IsCurrentSigningKey)
        {
            _currentKeyId = key.KeyId;
        }

        return Task.FromResult(true);
    }

        /// <summary>
    /// UpdateKeyAsync method.
    /// </summary>
public Task<bool> UpdateKeyAsync(JwtSigningKey key, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(key, nameof(key));
        ArgumentException.ThrowIfNullOrEmpty(key.KeyId, nameof(key.KeyId));

        if (!_keys.ContainsKey(key.KeyId))
        {
            return Task.FromResult(false);
        }

        _keys[key.KeyId] = key;
        if (key.IsCurrentSigningKey)
        {
            _currentKeyId = key.KeyId;
        }

        return Task.FromResult(true);
    }

        /// <summary>
    /// RevokeKeyAsync method.
    /// </summary>
public Task<bool> RevokeKeyAsync(string keyId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(keyId, nameof(keyId));

        if (!_keys.TryGetValue(keyId, out var key))
        {
            return Task.FromResult(false);
        }

        key.RevokedAt = DateTimeOffset.UtcNow;
        _keys[keyId] = key;

        return Task.FromResult(true);
    }

        /// <summary>
    /// DeactivateCurrentKeyAsync method.
    /// </summary>
public Task<bool> DeactivateCurrentKeyAsync(CancellationToken cancellationToken = default)
    {
        if (_currentKeyId is null || !_keys.TryGetValue(_currentKeyId, out var key))
        {
            return Task.FromResult(false);
        }

        key.IsCurrentSigningKey = false;
        _keys[_currentKeyId] = key;
        _currentKeyId = null;

        return Task.FromResult(true);
    }

        /// <summary>
    /// SaveChangesAsync method.
    /// </summary>
public Task<bool> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // In-memory implementation doesn't need to save
        return Task.FromResult(true);
    }
}
