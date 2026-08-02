using Aero.Core;

namespace Aero.Services;

/// <summary>
/// Defines an interface for IJwtTokenBuilder.
/// </summary>
public interface IJwtTokenBuilder
{
        /// <summary>
    /// AddSecurityKey method.
    /// </summary>
JwtTokenBuilder AddSecurityKey();
        /// <summary>
    /// AddSecurityKey method.
    /// </summary>
JwtTokenBuilder AddSecurityKey(string secret);
        /// <summary>
    /// AddSecurityKey method.
    /// </summary>
JwtTokenBuilder AddSecurityKey(SecurityKey securityKey);
        /// <summary>
    /// AddSubject method.
    /// </summary>
JwtTokenBuilder AddSubject(string subject);
        /// <summary>
    /// AddIssuer method.
    /// </summary>
JwtTokenBuilder AddIssuer(string issuer);
        /// <summary>
    /// AddAudience method.
    /// </summary>
JwtTokenBuilder AddAudience(string audience);
        /// <summary>
    /// AddClaim method.
    /// </summary>
JwtTokenBuilder AddClaim(string type, string value);
        /// <summary>
    /// AddClaims method.
    /// </summary>
JwtTokenBuilder AddClaims(Dictionary<string, string> claims);
        /// <summary>
    /// AddExpiry method.
    /// </summary>
JwtTokenBuilder AddExpiry(int expiryInMinutes);
        /// <summary>
    /// AddExpiry method.
    /// </summary>
JwtTokenBuilder AddExpiry(TimeSpan expiry);
        /// <summary>
    /// Build method.
    /// </summary>
JwtToken Build();
}

/// <summary>
/// Represents a class for JwtToken.
/// </summary>
public sealed class JwtToken(JwtSecurityToken token)
{
        /// <summary>
    /// Gets or sets the Valid To.
    /// </summary>
public DateTime ValidTo => token.ValidTo;
        /// <summary>
    /// Gets or sets the Value.
    /// </summary>
public string Value => new JwtSecurityTokenHandler().WriteToken(token);
}


/// <summary>
/// Represents a class for JwtTokenBuilder.
/// </summary>
public sealed class JwtTokenBuilder(IOptions<AppSettings> settings) : IJwtTokenBuilder
{
    private SecurityKey securityKey = default;
    private string subject = "";
    private string issuer = string.IsNullOrEmpty(settings.Value.ValidIssuers[0]) switch
    {
        true => throw new ArgumentNullException(nameof(AppSettings.ValidIssuers)),
        false => settings.Value.ValidIssuers[0]
    };
    private string audience = "";
    private Dictionary<string, string> claims = [];
    private TimeSpan expiry = TimeSpan.FromMinutes(15);

        /// <summary>
    /// AddSecurityKey method.
    /// </summary>
public JwtTokenBuilder AddSecurityKey() => AddSecurityKey(settings.Value.Secret);

        /// <summary>
    /// AddSecurityKey method.
    /// </summary>
public JwtTokenBuilder AddSecurityKey(string secret)
    {
        ArgumentException.ThrowIfNullOrEmpty(secret);
        if(secret.Length < 32)
            throw new ArgumentOutOfRangeException(nameof(secret), "Secret must be at least 32 characters long.");
        securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));

        return this;
    }

        /// <summary>
    /// AddSecurityKey method.
    /// </summary>
public JwtTokenBuilder AddSecurityKey(SecurityKey securityKey)
    {
        this.securityKey = securityKey;
        return this;
    }

        /// <summary>
    /// AddSubject method.
    /// </summary>
public JwtTokenBuilder AddSubject(string subject)
    {
        ArgumentException.ThrowIfNullOrEmpty(subject);
        this.subject = subject;
        return this;
    }

        /// <summary>
    /// AddIssuer method.
    /// </summary>
public JwtTokenBuilder AddIssuer(string issuer)
    {
        ArgumentException.ThrowIfNullOrEmpty(issuer);
        this.issuer = issuer;
        return this;
    }

        /// <summary>
    /// AddAudience method.
    /// </summary>
public JwtTokenBuilder AddAudience(string audience)
    {
        ArgumentException.ThrowIfNullOrEmpty(audience);
        this.audience = audience;
        return this;
    }

        /// <summary>
    /// AddClaim method.
    /// </summary>
public JwtTokenBuilder AddClaim(string type, string value)
    {
        ArgumentException.ThrowIfNullOrEmpty(type);
        ArgumentException.ThrowIfNullOrEmpty(value);
        claims.Add(type, value);
        return this;
    }

        /// <summary>
    /// AddClaims method.
    /// </summary>
public JwtTokenBuilder AddClaims(Dictionary<string, string> claims)
    {
        ArgumentNullException.ThrowIfNull(claims);
        this.claims = this.claims.Union(claims).ToDictionary();
        return this;
    }

        /// <summary>
    /// AddExpiry method.
    /// </summary>
public JwtTokenBuilder AddExpiry(TimeSpan expiry) => AddExpiry((int)expiry.TotalMinutes);

        /// <summary>
    /// AddExpiry method.
    /// </summary>
public JwtTokenBuilder AddExpiry(int expiry)
    {
        ArgumentOutOfRangeException
            .ThrowIfLessThan(expiry, 1, nameof(expiry));
        this.expiry = TimeSpan.FromMinutes(expiry);
        return this;
    }

        /// <summary>
    /// Build method.
    /// </summary>
public JwtToken Build()
    {
        EnsureArguments();

        var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, subject),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            }
            .Union(this.claims.Select(item => new Claim(item.Key, item.Value)));

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiry.TotalMinutes),
            signingCredentials: new SigningCredentials(
                securityKey, SecurityAlgorithms.HmacSha256));

        var jwt = new JwtToken(token);
        return jwt;
    }

    private void EnsureArguments()
    {
        ArgumentNullException.ThrowIfNull(securityKey);
        ArgumentException.ThrowIfNullOrEmpty(subject);
        ArgumentException.ThrowIfNullOrEmpty(issuer);
        ArgumentException.ThrowIfNullOrEmpty(audience);
    }
}