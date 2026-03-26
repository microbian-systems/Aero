namespace Aero.Social;

public class RefreshTokenException(
    string identifier,
    string? responseBody = null,
    object? requestBody = null,
    string? message = null)
    : Exception(message ?? "Token refresh required")
{
    public string Identifier { get; } = identifier;
    public string? ResponseBody { get; } = responseBody;
    public object? RequestBody { get; } = requestBody;
}

public class BadBodyException(
    string identifier,
    string? responseBody = null,
    object? requestBody = null,
    string? message = null)
    : Exception(message ?? "Bad request body")
{
    public string Identifier { get; } = identifier;
    public string? ResponseBody { get; } = responseBody;
    public object? RequestBody { get; } = requestBody;
}

public class NotEnoughScopesException(string message = "Not enough OAuth scopes granted") : Exception(message);

public class RateLimitException(TimeSpan? retryAfter = null, string? message = null)
    : Exception(message ?? "Rate limit exceeded")
{
    public TimeSpan? RetryAfter { get; } = retryAfter;
}
