namespace Aero.Core.Http;

/// <summary>
/// Represents a class for InMemoryTokenProvider.
/// </summary>
public sealed class InMemoryTokenProvider : ITokenProvider
{
    private string? _accessToken;

        /// <summary>
    /// GetAccessTokenAsync method.
    /// </summary>
public ValueTask<string?> GetAccessTokenAsync(CancellationToken cancellationToken)
    {
        return ValueTask.FromResult(_accessToken);
    }

        /// <summary>
    /// SetToken method.
    /// </summary>
public void SetToken(string? token)
    {
        _accessToken = token;
    }
}
