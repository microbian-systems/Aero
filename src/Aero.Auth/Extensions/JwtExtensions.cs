using Aero.Core.Railway;

namespace Aero.Auth.Extensions;

/// <summary>
/// Extension methods for handling JWT tokens.
/// </summary>
public static class JwtExtensions
{
    /// <summary>
    /// The Unix Epoch (January 1, 1970).
    /// </summary>
    public static readonly DateTime UnixEpoch = new(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

    /// <summary>
    /// Generates a JWT token for the specified user.
    /// </summary>
    /// <typeparam name="TKey">The type of the user key.</typeparam>
    /// <param name="user">The user to generate the token for.</param>
    /// <param name="secret">The secret key used to sign the token.</param>
    /// <param name="issuer">The token issuer.</param>
    /// <param name="audience">The token audience.</param>
    /// <param name="expires">The token expiration time. Defaults to 15 minutes if null.</param>
    /// <param name="claims">Optional additional claims.</param>
    /// <returns>A signed JWT token string.</returns>
    public static string ToJwtToken<TKey>(this IdentityUser<TKey> user, string secret, string issuer, string audience,
        TimeSpan? expires = null, List<Claim>? claims = null) where TKey : IEquatable<TKey>
    {
        expires ??= TimeSpan.FromMinutes(15);
        claims ??=
        [
            new Claim(JwtRegisteredClaimNames.Sub, user.Email!),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        ];
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(issuer, audience, claims, expires: new DateTime(expires.Value.Ticks), signingCredentials: creds);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// Extracts the <see cref="ClaimsPrincipal"/> from an expired JWT token.
    /// </summary>
    /// <param name="expiredToken">The expired JWT token string.</param>
    /// <param name="secret">The secret key used to validate the token signature.</param>
    /// <param name="validateAudience">Whether to validate the audience.</param>
    /// <param name="validateIssuer">Whether to validate the issuer.</param>
    /// <returns>A <see cref="ClaimsPrincipal"/> if the token is valid (excluding expiration), otherwise null.</returns>
    public static ClaimsPrincipal? GetPrincipleFromExpiredToken(this string expiredToken, string secret,
        bool validateAudience = false, bool validateIssuer = false)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = validateAudience,
            ValidateIssuer = validateIssuer,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
            ValidateLifetime = false //ignore token's expiration date
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var principal = tokenHandler.ValidateToken(expiredToken, tokenValidationParameters, out var securityToken);
        var jwtSecurityToken = securityToken as JwtSecurityToken;
        if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256,
                StringComparison.InvariantCultureIgnoreCase))
            return null;

        return principal;
    }

    /// <summary>
    /// Converts a <see cref="DateTime"/> to Unix epoch seconds.
    /// </summary>
    /// <param name="src">The source date time.</param>
    /// <returns>The number of seconds since the Unix epoch.</returns>
    public static double ToUnixEpochExpiration(this DateTime src)
    {
        var unixEpoch = UnixEpoch;
        return Math.Round((src - unixEpoch).TotalSeconds);
    }

    /// <summary>
    /// Decodes the JWT payload from an HTTP request's Authorization header.
    /// </summary>
    /// <param name="request">The current HTTP request.</param>
    /// <param name="secret">The secret key used to validate the token.</param>
    /// <returns>An <see cref="Option{JwtPayload}"/> containing the payload if decoding was successful.</returns>
    public static Option<JwtPayload> DecodeJwtPayload(this HttpRequest request, string secret)
    {
        var token = request.Headers["Authorization"]
            .ToString()?
            .Replace("Bearer ", "") ?? string.Empty;
        return DecodeJwtPayload(token, secret);
    }

    /// <summary>
    /// Decodes the JWT payload from a token string.
    /// </summary>
    /// <param name="token">The JWT token string.</param>
    /// <param name="secret">The secret key used to validate the token.</param>
    /// <returns>An <see cref="Option{JwtPayload}"/> containing the payload if decoding was successful.</returns>
    public static Option<JwtPayload> DecodeJwtPayload(this string token, string secret)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));

        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = key,
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = false
        };

        try
        {
            tokenHandler.ValidateToken(token, validationParameters, out var securityToken);
            var jwtSecurityToken = securityToken as JwtSecurityToken;
            return jwtSecurityToken != null ? Prelude.Some(jwtSecurityToken.Payload) : Prelude.None;
        }
        catch (Exception)
        {
            return Prelude.None;
        }
    }
}
