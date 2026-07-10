using System.Security.Cryptography;

namespace Aero.Auth.Services;

/// <summary>
/// Defines an enumeration for ApiKeyEnvironment.
/// </summary>
public enum ApiKeyEnvironment
{
    Test,
    Live
}

/// <summary>
/// Represents a record for GeneratedApiKey.
/// </summary>
public sealed record GeneratedApiKey(
    string KeyId,
    string RawApiKey,
    string SecretHash);

/// <summary>
/// Defines an interface for IApiKeyGenerator.
/// </summary>
public interface IApiKeyGenerator
{
        /// <summary>
    /// Generate method.
    /// </summary>
GeneratedApiKey Generate(ApiKeyEnvironment environment);
}

/// <summary>
/// Represents a class for HashedApiKeyGenerator.
/// </summary>
public sealed class HashedApiKeyGenerator : IApiKeyGenerator
{
        /// <summary>
    /// Generate method.
    /// </summary>
public GeneratedApiKey Generate(ApiKeyEnvironment environment)
    {
        var prefix = environment == ApiKeyEnvironment.Live
            ? "sk_live"
            : "sk_test";

        var keyId = Aero.Core.Snowflake.NewId().ToString();

        var secretBytes = RandomNumberGenerator.GetBytes(32);
        var secret = Convert.ToHexString(secretBytes).ToLowerInvariant();

        var rawApiKey = $"{prefix}_{keyId}_{secret}";

        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawApiKey));
        var secretHash = Convert.ToHexString(hashBytes).ToLowerInvariant();

        return new GeneratedApiKey(
            keyId,
            rawApiKey,
            secretHash);
    }
}
