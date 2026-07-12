using System.ComponentModel.DataAnnotations.Schema;
using Aero.Core.Entities;
using Aero.Core.Identity;
using Microsoft.AspNetCore.Identity;

namespace Aero.Models.Entities;

/// <summary>
/// Represents a concrete Aero user with a snowflake primary key.
/// </summary>
public class AeroUser : AeroUser<long>, IAeroUser
{
}


/// <summary>
/// Defines the core properties for an Aero user entity.
/// </summary>
public interface IAeroUser : IAeroUser<long>, ISnowflakeEntity;


/// <summary>
/// Generic interface for an Aero user entity with a custom primary key type.
/// </summary>
/// <typeparam name="TKey">The type of the primary key.</typeparam>
public interface IAeroUser<TKey> : IEntity<TKey> where TKey : IEquatable<TKey>
{
    /// <summary>
    /// Gets or sets the Birthday.
    /// </summary>
    public DateTimeOffset? Birthday { get; set; }
    /// <summary>
    /// Gets or sets the First Name.
    /// </summary>
    public string FirstName { get; set; }
    /// <summary>
    /// Gets or sets the Middle Name.
    /// </summary>
    public string MiddleName { get; set; }
    /// <summary>
    /// Gets or sets the Last Name.
    /// </summary>
    public string LastName { get; set; }
    /// <summary>
    /// Gets or sets the Created By.
    /// </summary>
    public string CreatedBy { get; set; }
    // todo - remove data attribute -> ModelBuilding (EF)
    /// <summary>
    /// Gets or sets the Profile Picture Data Url.
    /// </summary>
    public string ProfilePictureDataUrl { get; set; }
    /// <summary>
    /// Gets or sets the Is Deleted.
    /// </summary>
    public bool IsDeleted { get; set; } // todo - make IsDeleted a computed column from DeletedOn == null
    /// <summary>
    /// Gets or sets the Deleted On.
    /// </summary>
    public DateTimeOffset? DeletedOn { get; set; }
    /// <summary>
    /// Gets or sets the Is Active.
    /// </summary>
    public bool IsActive { get; set; }
    /// <summary>
    /// Gets or sets the Refresh Token.
    /// </summary>
    public string RefreshToken { get; set; }
    /// <summary>
    /// Gets or sets the Refresh Token Expiry Time.
    /// </summary>
    public DateTimeOffset? RefreshTokenExpiryTime { get; set; }
    /// <summary>
    /// Gets or sets the User Name.
    /// </summary>
    string? UserName { get; set; }
    /// <summary>
    /// Gets or sets the Normalized User Name.
    /// </summary>
    string? NormalizedUserName { get; set; }
    /// <summary>
    /// Gets or sets the Email.
    /// </summary>
    string? Email { get; set; }
    /// <summary>
    /// Gets or sets the Normalized Email.
    /// </summary>
    string? NormalizedEmail { get; set; }
    /// <summary>
    /// Gets or sets the Email Confirmed.
    /// </summary>
    bool EmailConfirmed { get; set; }
    /// <summary>
    /// Gets or sets the Password Hash.
    /// </summary>
    string? PasswordHash { get; set; }
    /// <summary>
    /// Gets or sets the Security Stamp.
    /// </summary>
    string? SecurityStamp { get; set; }
    /// <summary>
    /// Gets or sets the Concurrency Stamp.
    /// </summary>
    string? ConcurrencyStamp { get; set; }
    /// <summary>
    /// Gets or sets the Phone Number.
    /// </summary>
    string? PhoneNumber { get; set; }
    /// <summary>
    /// Gets or sets the Phone Number Confirmed.
    /// </summary>
    bool PhoneNumberConfirmed { get; set; }
    /// <summary>
    /// Gets or sets the Two Factor Enabled.
    /// </summary>
    bool TwoFactorEnabled { get; set; }
    /// <summary>
    /// Gets or sets the Lockout End.
    /// </summary>
    DateTimeOffset? LockoutEnd { get; set; }
    /// <summary>
    /// Gets or sets the Last Login At.
    /// </summary>
    public DateTimeOffset? LastLoginAt { get; set; }
    /// <summary>
    /// Gets or sets the Lockout Enabled.
    /// </summary>
    bool LockoutEnabled { get; set; }
    /// <summary>
    /// Gets or sets the Access Failed Count.
    /// </summary>
    int AccessFailedCount { get; set; }
    /// <summary>
    /// Gets or sets the Claims.
    /// </summary>
    public IList<IdentityUserClaim<long>> Claims { get; set; }
    /// <summary>
    /// Gets or sets the Logins.
    /// </summary>
    public IList<IdentityLogin> Logins { get; set; }
    /// <summary>
    /// Gets or sets the Tokens.
    /// </summary>
    public IList<IdentityToken> Tokens { get; set; }
    /// <summary>
    /// Gets or sets the Roles.
    /// </summary>
    public ICollection<AeroRole> Roles { get; set; }
}

/// <summary>
/// Base class for an Aero user entity, extending ASP.NET Core Identity.
/// </summary>
/// <typeparam name="TKey">The type of the primary key.</typeparam>
public abstract class AeroUser<TKey>
    : IdentityUser<TKey>, IEntity<TKey>, IAeroUser<TKey>
    where TKey : IEquatable<TKey>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AeroUser"/> class.
    /// </summary>
    protected AeroUser()
    {
        SecurityStamp = Guid.NewGuid().ToString("N");
    }

    /// <summary>
    /// Gets or sets the Birthday.
    /// </summary>
    [PersonalData] public DateTimeOffset? Birthday { get; set; }
    /// <summary>
    /// Gets or sets the First Name.
    /// </summary>
    public string FirstName { get; set; }
    /// <summary>
    /// Gets or sets the Middle Name.
    /// </summary>
    public string MiddleName { get; set; }
    /// <summary>
    /// Gets or sets the Last Name.
    /// </summary>
    public string LastName { get; set; }
    /// <summary>
    /// Gets or sets the Created By.
    /// </summary>
    public string CreatedBy { get; set; }
    /// <summary>
    /// Gets or sets the Profile Picture Data Url.
    /// </summary>
    [Column(TypeName = "text")] // todo - remove data attribute -> ModelBuilding (EF)
    public string ProfilePictureDataUrl { get; set; }
    /// <summary>
    /// Gets or sets the Created On.
    /// </summary>
    public DateTimeOffset CreatedOn { get; set; }
    /// <summary>
    /// Gets or sets the Modified By.
    /// </summary>
    public string ModifiedBy { get; set; }
    /// <summary>
    /// Gets or sets the Modified On.
    /// </summary>
    public DateTimeOffset? ModifiedOn { get; set; }
    /// <summary>
    /// Gets or sets the Is Deleted.
    /// </summary>
    public bool IsDeleted { get; set; }
    /// <summary>
    /// Gets or sets the Deleted On.
    /// </summary>
    public DateTimeOffset? DeletedOn { get; set; }
    /// <summary>
    /// Gets or sets the Is Active.
    /// </summary>
    public bool IsActive { get; set; }
    /// <summary>
    /// Gets or sets the Refresh Token.
    /// </summary>
    public string RefreshToken { get; set; }
    /// <summary>
    /// Gets or sets the Refresh Token Expiry Time.
    /// </summary>
    public DateTimeOffset? RefreshTokenExpiryTime { get; set; }
    /// <summary>
    /// Gets or sets the Last Login At.
    /// </summary>
    public DateTimeOffset? LastLoginAt { get; set; }
    /// <summary>
    /// Gets or sets the User Handle.
    /// </summary>
    public byte[] UserHandle { get; set; }
    /// <summary>
    /// Gets or sets the User Profile Id.
    /// </summary>
    public TKey UserProfileId { get; set; }
    /// <summary>
    /// Gets or sets the Agreed To Tos.
    /// </summary>
    public bool AgreedToTos { get; set; }
    /// <summary>
    /// Gets or sets the Claims.
    /// </summary>
    public virtual IList<IdentityUserClaim<long>> Claims { get; set; } = [];
    /// <summary>
    /// Gets or sets the Logins.
    /// </summary>
    public virtual IList<IdentityLogin> Logins { get; set; } = [];
    /// <summary>
    /// Gets or sets the Tokens.
    /// </summary>
    public virtual IList<IdentityToken> Tokens { get; set; } = [];
    /// <summary>
    /// Gets or sets the Roles.
    /// </summary>
    public virtual ICollection<AeroRole> Roles { get; set; } = new List<AeroRole>();
    /// <summary>
    /// Gets or sets the Two Factor Recovery Codes.
    /// </summary>
    public virtual IList<string> TwoFactorRecoveryCodes { get; set; } = [];
    /// <summary>
    /// Gets or sets the Two Factor Authenticator Key.
    /// </summary>
    public virtual string? TwoFactorAuthenticatorKey { get; set; }
}


