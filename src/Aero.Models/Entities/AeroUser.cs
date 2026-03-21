using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Aero.Core.Entities;
using Aero.Core.Identity;
using Microsoft.AspNetCore.Identity;

namespace Aero.Models.Entities;

/// <summary>
/// Represents a concrete Aero user with a snowflake primary key.
/// </summary>
public class AeroUser : AeroUser<long>, IAeroUser;


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
    public DateTimeOffset? Birthday { get; set; }
    public string FirstName { get; set; }
    public string MiddleName { get; set; }
    public string LastName { get; set; }
    public string CreatedBy { get; set; }
    // todo - remove data attribute -> ModelBuilding (EF)
    public string ProfilePictureDataUrl { get; set; }
    public bool IsDeleted { get; set; } // todo - make IsDeleted a computed column from DeletedOn == null
    public DateTimeOffset? DeletedOn { get; set; }
    public bool IsActive { get; set; }
    public string RefreshToken { get; set; }
    public DateTimeOffset? RefreshTokenExpiryTime { get; set; }
    string? UserName { get; set; }
    string? NormalizedUserName { get; set; }
    string? Email { get; set; }
    string? NormalizedEmail { get; set; }
    bool EmailConfirmed { get; set; }
    string? PasswordHash { get; set; }
    string? SecurityStamp { get; set; }
    string? ConcurrencyStamp { get; set; }
    string? PhoneNumber { get; set; }
    bool PhoneNumberConfirmed { get; set; }
    bool TwoFactorEnabled { get; set; }
    DateTimeOffset? LockoutEnd { get; set; }
    public DateTimeOffset? LastLoginAt { get; set; }
    bool LockoutEnabled { get; set; }
    int AccessFailedCount { get; set; }
    public IList<IdentityUserClaim<long>> Claims { get; set; }
    public IList<IdentityLogin> Logins { get; set; }
    public IList<IdentityToken> Tokens { get; set; }
    public ISet<AeroRole> Roles { get; set; }
}

/// <summary>
/// Base class for an Aero user entity, extending ASP.NET Core Identity.
/// </summary>
/// <typeparam name="TKey">The type of the primary key.</typeparam>
public abstract class AeroUser<TKey> 
    : IdentityUser<TKey>, IEntity<TKey>, IAeroUser<TKey> 
    where TKey : IEquatable<TKey>
{
    protected AeroUser()
    {
        SecurityStamp = Guid.NewGuid().ToString("N");
    }

    [PersonalData] public DateTimeOffset? Birthday { get; set; }
    public string FirstName { get; set; }
    public string MiddleName { get; set; }
    public string LastName { get; set; }
    public string CreatedBy { get; set; }
    [Column(TypeName = "text")] // todo - remove data attribute -> ModelBuilding (EF)
    public string ProfilePictureDataUrl { get; set; }
    public DateTimeOffset CreatedOn { get; set; }
    public string ModifiedBy { get; set; }
    public DateTimeOffset? ModifiedOn { get; set; }
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedOn { get; set; }
    public bool IsActive { get; set; }
    public string RefreshToken { get; set; }
    public DateTimeOffset? RefreshTokenExpiryTime { get; set; }
    public DateTimeOffset? LastLoginAt { get; set; }
    public byte[] UserHandle { get; set; } 
    public TKey UserProfileId { get; set; }
    public bool AgreedToTos { get; set; } 
    public virtual IList<IdentityUserClaim<long>> Claims { get; set; } = [];
    public virtual IList<IdentityLogin> Logins { get; set; } = [];
    public virtual IList<IdentityToken> Tokens { get; set; } = [];
    public virtual ISet<AeroRole> Roles { get; set; } = new HashSet<AeroRole>();
    public virtual IList<string> TwoFactorRecoveryCodes { get; set; } = [];
    public virtual string? TwoFactorAuthenticatorKey { get; set; }
}


