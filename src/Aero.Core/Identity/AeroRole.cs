using Aero.Core.Data;
using Aero.Core.Entities;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
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

[Table("Roles", Schema = Schemas.Aero)]
public class AeroRole : AeroRole<long>
{
    public new long Id { get; set; } = Snowflake.NewId();

    public AeroRole() => Snowflake.NewId();

    public AeroRole(string roleName)
        : this()
    {
        this.Name = roleName;
    }
}


[Table("Roles", Schema = Schemas.Aero)]
public abstract class AeroRole<TKey> : IdentityRole<TKey>, IEntity<TKey>

    where TKey : IEquatable<TKey>, IComparable<TKey> 
{
    protected AeroRole() { }
    protected AeroRole(string roleName) : base(roleName) { }
    public List<IdentityRoleClaim<TKey>> Claims { get; set; } = [];
    public List<TKey> Users { get; set; } = [];
    public DateTimeOffset CreatedOn { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? ModifiedOn { get; set; } = DateTimeOffset.UtcNow;
    public string? CreatedBy { get; set; }
    public string? ModifiedBy { get; set; }
}
