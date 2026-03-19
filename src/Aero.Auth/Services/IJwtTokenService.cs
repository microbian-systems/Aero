namespace Aero.Auth.Services;

/// <summary>
/// Core JWT token generation and validation service
/// </summary>
public interface IJwtTokenService
{
    /// <summary>
    /// Generates a short-lived access token for a user.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="email">The email address of the user.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the JWT access token string.</returns>
    Task<string> GenerateAccessTokenAsync(
        ulong userId,
        string email,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates an access token and returns claims if valid.
    /// </summary>
    /// <param name="token">The JWT access token to validate.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a tuple indicating if the token is valid and the principal if valid.</returns>
    Task<(bool IsValid, System.Security.Claims.ClaimsPrincipal? Principal)> ValidateAccessTokenAsync(
        string token,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the lifetime of access tokens (in seconds)
    /// </summary>
    int AccessTokenLifetime { get; }
}
