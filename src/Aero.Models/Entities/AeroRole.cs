using Aero.Core;
using Aero.Core.Data;
using Aero.Core.Entities;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace Aero.Models.Entities;

/// <summary>
/// Represents the concrete persisted ASP.NET Core Identity role for Aero.
/// </summary>
[Table("Roles", Schema = Schemas.Aero)]
public class AeroRole : AeroRole<long>
{
    /// <summary>
    /// Initializes a new instance of <see cref="AeroRole"/> with a Snowflake identity.
    /// </summary>
    public AeroRole() => Id = Snowflake.NewId();

    /// <summary>
    /// Initializes a new instance of <see cref="AeroRole"/> with the supplied role name.
    /// </summary>
    public AeroRole(string roleName)
        : this()
    {
        Name = roleName;
    }
}

/// <summary>
/// Represents an Aero ASP.NET Core Identity role with a custom primary-key type.
/// </summary>
/// <typeparam name="TKey">The primary-key type.</typeparam>
[Table("Roles", Schema = Schemas.Aero)]
public abstract class AeroRole<TKey> : IdentityRole<TKey>, IEntity<TKey>
    where TKey : IEquatable<TKey>, IComparable<TKey>
{
    protected AeroRole()
    {
    }

    protected AeroRole(string roleName)
        : base(roleName)
    {
    }

    public override bool Equals(object? obj) =>
        obj is AeroRole<TKey> other && EqualityComparer<TKey>.Default.Equals(Id, other.Id);

    public override int GetHashCode() => Id?.GetHashCode() ?? 0;

    public List<IdentityRoleClaim<TKey>> Claims { get; set; } = [];

    public List<TKey> Users { get; set; } = [];

    public DateTimeOffset CreatedOn { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? ModifiedOn { get; set; } = DateTimeOffset.UtcNow;

    public string? CreatedBy { get; set; }

    public string? ModifiedBy { get; set; }
}
