using System.Security.Cryptography;

namespace Aero.Auth.Jwt;

/// <summary>
/// Represents a class for JwtFactoryBase.
/// </summary>
public abstract class JwtFactoryBase(ILogger<JwtFactoryBase> log) : IJwtFactory
{
        /// <summary>
    /// log.
    /// </summary>
protected readonly ILogger<JwtFactoryBase> log = log;

        /// <summary>
    /// GenerateAccessToken method.
    /// </summary>
public abstract JwtResponseModel GenerateAccessToken(List<Claim> claims);
        /// <summary>
    /// GetPrincipalFromToken method.
    /// </summary>
public abstract ClaimsPrincipal? GetPrincipalFromToken(string? token);
        /// <summary>
    /// IsValidToken method.
    /// </summary>
public abstract bool IsValidToken(string token);

        /// <summary>
    /// GenerateRefreshToken method.
    /// </summary>
public string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        
        var token = Convert.ToBase64String(randomNumber);
        return token;
    }
}