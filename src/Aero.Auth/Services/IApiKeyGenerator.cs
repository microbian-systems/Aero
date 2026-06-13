using System.Security.Cryptography;

namespace Aero.Auth.Services;

public enum ApiKeyEnvironment
{
    Test,
    Live
}

public sealed record GeneratedApiKey(
    string KeyId,
    string RawApiKey,
    string SecretHash);

public interface IApiKeyGenerator
{
    GeneratedApiKey Generate(ApiKeyEnvironment environment);
}

public sealed class HashedApiKeyGenerator : IApiKeyGenerator
{
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
