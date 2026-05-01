namespace Aero.Core.Http;

public sealed class InMemoryTokenProvider : ITokenProvider
{
    private string? _accessToken;

    public ValueTask<string?> GetAccessTokenAsync(CancellationToken cancellationToken)
    {
        return ValueTask.FromResult(_accessToken);
    }

    public void SetToken(string? token)
    {
        _accessToken = token;
    }
}
