namespace Aero.Services;

/// <summary>
/// Defines an interface for ITokenValidationService.
/// </summary>
public interface ITokenValidationService
{
        /// <summary>
    /// ValidateToken method.
    /// </summary>
WebResponse<bool> ValidateToken(string token);
        /// <summary>
    /// GenerateToken method.
    /// </summary>
string GenerateToken<T>(T user, IEnumerable<Claim> roles) where T : AeroUser;
        /// <summary>
    /// GenerateRefreshToken method.
    /// </summary>
string GenerateRefreshToken();
        /// <summary>
    /// GetPrincipalFromExpiredToken method.
    /// </summary>
ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
        /// <summary>
    /// GetSecurityAndPrinciple method.
    /// </summary>
(ClaimsPrincipal principle, SecurityToken validated) GetSecurityAndPrinciple(string token);
        /// <summary>
    /// GetRefreshToken method.
    /// </summary>
string GetRefreshToken(string id);
}