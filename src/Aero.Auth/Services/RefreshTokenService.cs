using Aero.Auth.Persistence;
using Aero.Core;
using AeroDB.Sable;
using System.Security.Cryptography;

namespace Aero.Auth.Services;

/// <summary>
/// Issues, validates, rotates, and revokes hashed refresh tokens.
/// </summary>
/// <remarks>
/// Rotation uses a version-conditional predecessor update and an insert-only
/// successor in one Sable save. Callers must reconcile a propagated commit
/// outcome exception rather than retrying a potentially committed rotation.
/// </remarks>
public class RefreshTokenService(
    IDocumentSession session,
    ILogger<RefreshTokenService> logger,
    IConfiguration config)
    : IRefreshTokenService
{
    private readonly RefreshTokenPersistence persistence = new(session);
    private readonly int refreshTokenLifetimeDays = config.GetValue("Auth:RefreshTokenLifetimeDays", 30);

    /// <inheritdoc />
    public async Task<string> GenerateRefreshTokenAsync(
        long userId,
        string clientType,
        string? ipAddress = null,
        string? userAgent = null,
        CancellationToken cancellationToken = default)
    {
        var token = GenerateRandomToken(64);
        var now = DateTimeOffset.UtcNow;
        var refreshToken = CreateToken(userId, HashToken(token), clientType, ipAddress, userAgent, now);

        persistence.Insert(refreshToken);
        await persistence.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Generated refresh token for user {UserId} from {IpAddress} ({ClientType})",
            userId,
            ipAddress ?? "unknown",
            clientType);

        return token;
    }

    /// <inheritdoc />
    public async Task<long?> ValidateRefreshTokenAsync(
        string token,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(token))
        {
            logger.LogWarning("Empty refresh token provided");
            return null;
        }

        var refreshToken = await persistence.FindByHashAsync(HashToken(token), cancellationToken);
        if (refreshToken is null
            || refreshToken.Version < 1
            || !refreshToken.IsActiveAt(DateTimeOffset.UtcNow))
        {
            logger.LogWarning("Refresh token not found or is not active");
            return null;
        }

        return refreshToken.UserId;
    }

    /// <inheritdoc />
    public async Task<string> RotateRefreshTokenAsync(
        string oldToken,
        string clientType,
        string? ipAddress = null,
        string? userAgent = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(oldToken);

        var oldTokenRecord = await persistence.FindByHashAsync(HashToken(oldToken), cancellationToken);
        var now = DateTimeOffset.UtcNow;
        if (oldTokenRecord is null || !oldTokenRecord.IsActiveAt(now) || oldTokenRecord.Version < 1)
        {
            throw new InvalidOperationException("Invalid refresh token.");
        }

        var newToken = GenerateRandomToken(64);
        var successor = CreateToken(
            oldTokenRecord.UserId,
            HashToken(newToken),
            clientType,
            ipAddress,
            userAgent,
            now);

        oldTokenRecord.ReplacedByTokenId = successor.Id;
        persistence.UpdateExpectedVersion(oldTokenRecord, oldTokenRecord.Version);
        persistence.Insert(successor);
        await persistence.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Rotated refresh token for user {UserId}", oldTokenRecord.UserId);
        return newToken;
    }

    /// <inheritdoc />
    public async Task RevokeRefreshTokenAsync(
        string token,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(token);

        var refreshToken = await persistence.FindByHashAsync(HashToken(token), cancellationToken);
        if (refreshToken is null)
        {
            return;
        }

        EnsureVersioned(refreshToken);
        refreshToken.RevokedAt = DateTimeOffset.UtcNow;
        persistence.UpdateExpectedVersion(refreshToken, refreshToken.Version);
        await persistence.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Revoked refresh token for user {UserId}", refreshToken.UserId);
    }

    /// <inheritdoc />
    public async Task RevokeAllUserTokensAsync(
        long userId,
        CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var activeTokens = await persistence.FindActiveByUserAsync(userId, now, cancellationToken);
        foreach (var token in activeTokens)
        {
            EnsureVersioned(token);
            token.RevokedAt = now;
            persistence.UpdateExpectedVersion(token, token.Version);
        }

        if (activeTokens.Count == 0)
        {
            return;
        }

        await persistence.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Revoked all refresh tokens for user {UserId} ({Count} tokens)", userId, activeTokens.Count);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<(long Id, string ClientType, DateTimeOffset CreatedAt, string? IpAddress)>> GetActiveTokensAsync(
        long userId,
        CancellationToken cancellationToken = default)
    {
        var tokens = await persistence.FindActiveByUserAsync(userId, DateTimeOffset.UtcNow, cancellationToken);
        return tokens.Select(token => (token.Id, token.ClientType, token.CreatedOn, token.IssuedFromIpAddress));
    }

    private RefreshTokenDocument CreateToken(
        long userId,
        string tokenHash,
        string clientType,
        string? ipAddress,
        string? userAgent,
        DateTimeOffset now) => new()
        {
            Id = Snowflake.NewId(),
            UserId = userId,
            TokenHash = tokenHash,
            CreatedOn = now,
            ExpiresAt = now.AddDays(refreshTokenLifetimeDays),
            ClientType = clientType,
            IssuedFromIpAddress = ipAddress,
            UserAgent = userAgent,
            Version = 1
        };

    private static void EnsureVersioned(RefreshTokenDocument token)
    {
        if (token.Version < 1)
        {
            throw new InvalidOperationException("Refresh token does not have supported concurrency metadata.");
        }
    }

    private static string HashToken(string token)
    {
        using var sha256 = SHA256.Create();
        return Convert.ToBase64String(sha256.ComputeHash(Encoding.UTF8.GetBytes(token)));
    }

    private static string GenerateRandomToken(uint length)
    {
        var randomBytes = new byte[length];
        RandomNumberGenerator.Fill(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }
}
