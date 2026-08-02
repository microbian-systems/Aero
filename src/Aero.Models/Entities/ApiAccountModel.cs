using System.ComponentModel.DataAnnotations;
using Aero.Core.Entities;

namespace Aero.Models.Entities;

/// <summary>
/// Represents a class for ApiAccountModel.
/// </summary>
public class ApiAccountModel : Entity
{
        /// <summary>
    /// Gets or sets the Api Key.
    /// </summary>
[MaxLength(128)]
    public string? ApiKey { get; set; }
        /// <summary>
    /// Gets or sets the Email.
    /// </summary>
[MaxLength(256)]
    public string Email { get; set; }
        /// <summary>
    /// Gets or sets the Enabled.
    /// </summary>
public bool Enabled { get; set; }
        /// <summary>
    /// Gets or sets the Refresh Token.
    /// </summary>
[MaxLength(1024)]
    public string RefreshToken { get; set; }
        /// <summary>
    /// Gets or sets the Refresh Token Expiry.
    /// </summary>
public DateTimeOffset RefreshTokenExpiry { get; set; }
        /// <summary>
    /// Gets or sets the Claims.
    /// </summary>
public virtual List<ApiClaimsModel> Claims { get; set; } = [];
}

// public record ApiAccountModel : IEntity<int>
// {
//     [Key]
//     public int Id { get; set; }
//     public DateTimeOffset CreatedOn { get; set; }
//     public DateTimeOffset? ModifiedOn { get; set; }
//     public string CreatedBy { get; set; }
//     public string ModifiedBy { get; set; }
//     public string? ApiKey { get; set; }
//     public string Email { get; set; }
//     public bool Enabled { get; set; }
//     public string RefreshToken { get; set; }
//     public DateTimeOffset RefreshTokenExpiry { get; set; }
//     public DateTimeOffset CreateDate { get; set; }
//     public DateTimeOffset ModifiedDate { get; set; }
//     public virtual List<ApiClaimsModel> Claims { get; set; } = new();
// }