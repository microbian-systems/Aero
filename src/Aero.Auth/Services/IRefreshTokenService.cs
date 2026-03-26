namespace Aero.Auth.Services;

/// <summary>
/// Abstraction for refresh token management.
/// Handles creation, validation, rotation, and revocation of refresh tokens.
/// </summary>
public interface IRefreshTokenService
{
    /// <summary>
    /// Generates a new refresh token for a user
    /// </summary>
    Task<string> GenerateRefreshTokenAsync(
        long userId,
        string clientType,
        string? ipAddress = null,
        string? userAgent = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates a refresh token and returns the user ID if valid
    /// </summary>
    Task<long?> ValidateRefreshTokenAsync(string token,
        CancellationToken cancellationToken);

    /// <summary>
    /// Rotates a refresh token (invalidates old, returns new)
    /// </summary>
    Task<string> RotateRefreshTokenAsync(
        string oldToken,
        string clientType,
        string? ipAddress = null,
        string? userAgent = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Revokes a refresh token (marks as invalid)
    /// </summary>
    Task RevokeRefreshTokenAsync(
        string token,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Revokes all refresh tokens for a user (logout everywhere)
    /// </summary>
    Task RevokeAllUserTokensAsync(
        long userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all active refresh tokens for a user
    /// </summary>
    Task<IEnumerable<(long Id, string ClientType, DateTimeOffset CreatedAt, string? IpAddress)>> GetActiveTokensAsync(
        long userId,
        CancellationToken cancellationToken);
}
