using System.ComponentModel.DataAnnotations;
using Aero.Core.Entities;

namespace Aero.Models.Entities;

/// <summary>
/// Represents a class for ApiClaimsModel.
/// </summary>
public class ApiClaimsModel : Entity
{
        /// <summary>
    /// Gets or sets the Claim Key.
    /// </summary>
[MaxLength(128)]
    public required string ClaimKey { get; set; }
        /// <summary>
    /// Gets or sets the Claim Value.
    /// </summary>
[MaxLength(1024)]
    public required string ClaimValue { get; set; }
    
        /// <summary>
    /// Gets or sets the Account Id.
    /// </summary>
public long AccountId { get; set; }
}