namespace Aero.Auth.Jwt;

/// <summary>
/// Represents a record for JwtOptions.
/// </summary>
public record JwtOptions
{
        /// <summary>
    /// Gets or sets the Issuer.
    /// </summary>
public string? Issuer { get; set; }
        /// <summary>
    /// Gets or sets the Subject.
    /// </summary>
public string? Subject { get; set; }
        /// <summary>
    /// Gets or sets the Audience.
    /// </summary>
public string? Audience { get; set; }
        /// <summary>
    /// Gets or sets the Key.
    /// </summary>
public string? Key { get; set; }
        /// <summary>
    /// Gets or sets the Expiry In Minutes.
    /// </summary>
public int ExpiryInMinutes { get; set; }
        /// <summary>
    /// Gets or sets the Refresh Expiry In Minutes.
    /// </summary>
public int RefreshExpiryInMinutes { get; set; }
        /// <summary>
    /// Gets or sets the Encryption Key.
    /// </summary>
public string? EncryptionKey { get; set; }
}

