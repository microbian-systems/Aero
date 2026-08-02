namespace Aero.Social.Abstractions;

/// <summary>
/// Represents a class for RefreshTokenException.
/// </summary>
public class RefreshTokenException(
    string identifier,
    string? responseBody = null,
    object? requestBody = null,
    string? message = null)
    : Exception(message ?? "Token refresh required")
{
        /// <summary>
    /// Gets or sets the Identifier.
    /// </summary>
public string Identifier { get; } = identifier;
        /// <summary>
    /// Gets or sets the Response Body.
    /// </summary>
public string? ResponseBody { get; } = responseBody;
        /// <summary>
    /// Gets or sets the Request Body.
    /// </summary>
public object? RequestBody { get; } = requestBody;
}

/// <summary>
/// Represents a class for BadBodyException.
/// </summary>
public class BadBodyException(
    string identifier,
    string? responseBody = null,
    object? requestBody = null,
    string? message = null)
    : Exception(message ?? "Bad request body")
{
        /// <summary>
    /// Gets or sets the Identifier.
    /// </summary>
public string Identifier { get; } = identifier;
        /// <summary>
    /// Gets or sets the Response Body.
    /// </summary>
public string? ResponseBody { get; } = responseBody;
        /// <summary>
    /// Gets or sets the Request Body.
    /// </summary>
public object? RequestBody { get; } = requestBody;
}

/// <summary>
/// Represents a class for NotEnoughScopesException.
/// </summary>
public class NotEnoughScopesException(string message = "Not enough OAuth scopes granted") : Exception(message);

/// <summary>
/// Represents a class for RateLimitException.
/// </summary>
public class RateLimitException(TimeSpan? retryAfter = null, string? message = null)
    : Exception(message ?? "Rate limit exceeded")
{
        /// <summary>
    /// Gets or sets the Retry After.
    /// </summary>
public TimeSpan? RetryAfter { get; } = retryAfter;
}
