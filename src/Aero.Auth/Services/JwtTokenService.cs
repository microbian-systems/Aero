namespace Aero.Auth.Services;

/// <summary>
/// Production-grade JWT token service for generating and validating access tokens.
/// Uses the configured signing key store for key rotation support.
/// </summary>
public class JwtTokenService : IJwtTokenService
{
    private readonly IJwtSigningKeyStore _signingKeyStore;
    private readonly ILogger<JwtTokenService> _logger;
    private readonly IConfiguration _config;

    /// <summary>
    /// Gets the lifetime of the access token in seconds.
    /// </summary>
    public int AccessTokenLifetime { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="JwtTokenService"/> class.
    /// </summary>
    /// <param name="signingKeyStore">The signing key store.</param>
    /// <param name="logger">The logger instance.</param>
    /// <param name="config">The configuration instance.</param>
    public JwtTokenService(
        IJwtSigningKeyStore signingKeyStore,
        ILogger<JwtTokenService> logger,
        IConfiguration config)
    {
        _signingKeyStore = signingKeyStore;
        _logger = logger;
        _config = config;
        AccessTokenLifetime = _config.GetValue("Auth:AccessTokenLifetimeSeconds", 300); // 5 minutes
    }

    /// <summary>
    /// Generates a short-lived access token for a user.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="email">The email address of the user.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A signed JWT access token.</returns>
    public async Task<string> GenerateAccessTokenAsync(
        long userId,
        string email,
        CancellationToken cancellationToken = default)
    {
        var signingCredentials = await _signingKeyStore.GetSigningCredentialsAsync(cancellationToken);
        var keyId = await _signingKeyStore.GetCurrentKeyIdAsync(cancellationToken);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.Email, email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        var token = new JwtSecurityToken(
            issuer: _config["Auth:Jwt:Issuer"] ?? "Aero",
            audience: _config["Auth:Jwt:Audience"] ?? "AeroClients",
            claims: claims,
            expires: DateTime.UtcNow.AddSeconds(AccessTokenLifetime),
            signingCredentials: signingCredentials);

        // Add kid header for key rotation support
        token.Header["kid"] = keyId;

        var tokenHandler = new JwtSecurityTokenHandler();
        var jwt = tokenHandler.WriteToken(token);

        _logger.LogDebug("Generated access token for user {UserId}", userId);

        return jwt;
    }

    /// <summary>
    /// Validates an access token and returns the principal if valid.
    /// </summary>
    /// <param name="token">The JWT access token to validate.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A tuple indicating success and the resulting principal.</returns>
    public async Task<(bool IsValid, ClaimsPrincipal? Principal)> ValidateAccessTokenAsync(
        string token,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var validationKeys = await _signingKeyStore.GetValidationKeysAsync(cancellationToken);

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKeys = validationKeys,
                ValidateIssuer = true,
                ValidIssuer = _config["Auth:Jwt:Issuer"] ?? "Aero",
                ValidateAudience = true,
                ValidAudience = _config["Auth:Jwt:Audience"] ?? "AeroClients",
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);

            if (validatedToken is JwtSecurityToken jwtToken)
            {
                var userId = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value;
                _logger.LogDebug("Validated access token for user {UserId}", userId);
            }

            return (true, principal);
        }
        catch (SecurityTokenException ex)
        {
            _logger.LogWarning("Token validation failed: {Message}", ex.Message);
            return (false, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error validating token");
            return (false, null);
        }
    }
}
