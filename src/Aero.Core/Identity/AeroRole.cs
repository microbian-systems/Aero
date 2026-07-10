using Aero.Core.Data;
using Aero.Core.Entities;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace Aero.Core.Identity;

// public class UserRole : IdentityUserRole<string>
// {
//     /// <summary>
//     /// Reference to the Role document ID
//     /// </summary>
//     public string RoleId { get; set; } = string.Empty;
//
//     /// <summary>
//     /// Navigation property to the role (not serialized, loaded via Include)
//     /// </summary>
//     [JsonIgnore]
//     public CmsRole? Role { get; set; }
//
//     /// <summary>
//     /// Reference to the User document ID
//     /// </summary>
//     public string UserId { get; set; } = string.Empty;
//
//     /// <summary>
//     /// Navigation property to the user (not serialized, loaded via Include)
//     /// </summary>
//     [JsonIgnore]
//     public AeroUser? User { get; set; }
// }

/// <summary>
/// Represents a class for AeroRole.
/// </summary>
[Table("Roles", Schema = Schemas.Aero)]
public class AeroRole : AeroRole<long>
{
        /// <summary>
    /// Gets or sets the Id.
    /// </summary>
public new long Id { get; set; } = Snowflake.NewId();

        /// <summary>
    /// Initializes a new instance of the <see cref="AeroRole"/> class.
    /// </summary>
public AeroRole() => Snowflake.NewId();

        /// <summary>
    /// Initializes a new instance of the <see cref="AeroRole"/> class.
    /// </summary>
public AeroRole(string roleName)
        : this()
    {
        this.Name = roleName;
    }
}


/// <summary>
/// Represents a class for AeroRole.
/// </summary>
[Table("Roles", Schema = Schemas.Aero)]
public abstract class AeroRole<TKey> : IdentityRole<TKey>, IEntity<TKey>

    where TKey : IEquatable<TKey>, IComparable<TKey> 
{
        /// <summary>
    /// Initializes a new instance of the <see cref="AeroRole"/> class.
    /// </summary>
protected AeroRole() { }
        /// <summary>
    /// Initializes a new instance of the <see cref="AeroRole"/> class.
    /// </summary>
protected AeroRole(string roleName) : base(roleName) { }

        /// <summary>
    /// Equals method.
    /// </summary>
public override bool Equals(object? obj) =>
        obj is AeroRole<TKey> other && EqualityComparer<TKey>.Default.Equals(Id, other.Id);

        /// <summary>
    /// GetHashCode method.
    /// </summary>
public override int GetHashCode() => Id?.GetHashCode() ?? 0;
        /// <summary>
    /// Gets or sets the Claims.
    /// </summary>
public List<IdentityRoleClaim<TKey>> Claims { get; set; } = [];
        /// <summary>
    /// Gets or sets the Users.
    /// </summary>
public List<TKey> Users { get; set; } = [];
        /// <summary>
    /// Gets or sets the Created On.
    /// </summary>
public DateTimeOffset CreatedOn { get; set; } = DateTimeOffset.UtcNow;
        /// <summary>
    /// Gets or sets the Modified On.
    /// </summary>
public DateTimeOffset? ModifiedOn { get; set; } = DateTimeOffset.UtcNow;
        /// <summary>
    /// Gets or sets the Created By.
    /// </summary>
public string? CreatedBy { get; set; }
        /// <summary>
    /// Gets or sets the Modified By.
    /// </summary>
public string? ModifiedBy { get; set; }
}
