namespace Aero.Auth.Models;

/// <summary>
/// Response for web-based login (BFF cookie authentication)
/// </summary>
public class LoginWebResponse
{
    /// <summary>
    /// Gets or sets a value indicating whether the login was successful.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Gets or sets a descriptive message.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user ID if successful.
    /// </summary>
    public string? UserId { get; set; }

    /// <summary>
    /// Gets or sets the user's email if successful.
    /// </summary>
    public string? Email { get; set; }
}

/// <summary>
/// Response for app-based login (JWT + refresh token)
/// </summary>
public class LoginAppResponse
{
    /// <summary>
    /// Gets or sets a value indicating whether the login was successful.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Gets or sets a descriptive message.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the short-lived access token.
    /// </summary>
    public string? AccessToken { get; set; }

    /// <summary>
    /// Gets or sets the long-lived refresh token.
    /// </summary>
    public string? RefreshToken { get; set; }

    /// <summary>
    /// Gets or sets the access token lifetime in seconds.
    /// </summary>
    public int AccessTokenExpiresIn { get; set; } // in seconds

    /// <summary>
    /// Gets or sets the token type (usually "Bearer").
    /// </summary>
    public string TokenType { get; set; } = "Bearer";

    /// <summary>
    /// Gets or sets the user ID if successful.
    /// </summary>
    public string? UserId { get; set; }

    /// <summary>
    /// Gets or sets the user's email if successful.
    /// </summary>
    public string? Email { get; set; }
}

/// <summary>
/// Response for token refresh
/// </summary>
public class RefreshTokenResponse
{
    /// <summary>
    /// Gets or sets a value indicating whether the refresh was successful.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Gets or sets a descriptive message.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the new short-lived access token.
    /// </summary>
    public string? AccessToken { get; set; }

    /// <summary>
    /// Gets or sets the rotated long-lived refresh token.
    /// </summary>
    public string? RefreshToken { get; set; }

    /// <summary>
    /// Gets or sets the access token lifetime in seconds.
    /// </summary>
    public int AccessTokenExpiresIn { get; set; } // in seconds
}

/// <summary>
/// Response for logout
/// </summary>
public class LogoutResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// Response for external login challenge (initiates social/passkey flow)
/// </summary>
public class ExternalLoginChallengeResponse
{
    public string AuthorizationUrl { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
}
