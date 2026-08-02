namespace Aero.Auth.Jwt;

/// <summary>
/// Represents a record for JwtResponseModel.
/// </summary>
public record JwtResponseModel
{
        /// <summary>
    /// Gets or sets the Access Token.
    /// </summary>
public string AccessToken { get; set; }
        /// <summary>
    /// Gets or sets the Refresh Token.
    /// </summary>
public string RefreshToken { get; set; }
        /// <summary>
    /// Gets or sets the Expiry.
    /// </summary>
public DateTimeOffset Expiry { get; set; }
        /// <summary>
    /// Gets or sets the Refresh Expiry.
    /// </summary>
public DateTimeOffset RefreshExpiry { get; set; }
}

/// <summary>
/// Defines an interface for IJwtFactory.
/// </summary>
public interface IJwtFactory
{
        /// <summary>
    /// GenerateAccessToken method.
    /// </summary>
JwtResponseModel GenerateAccessToken(List<Claim> claims);
        /// <summary>
    /// GenerateRefreshToken method.
    /// </summary>
string GenerateRefreshToken();
        /// <summary>
    /// GetPrincipalFromToken method.
    /// </summary>
ClaimsPrincipal? GetPrincipalFromToken(string? token);
        /// <summary>
    /// IsValidToken method.
    /// </summary>
bool IsValidToken(string token);
}