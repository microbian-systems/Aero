using System.ComponentModel.DataAnnotations;

namespace Aero.Web.Models;

/// <summary>
/// Represents a record for ApiClaimsModel.
/// </summary>
public record ApiClaimsModel
{
        /// <summary>
    /// Gets or sets the Id.
    /// </summary>
[Key]
    public int Id { get; set; }
        /// <summary>
    /// Gets or sets the Claim Key.
    /// </summary>
public string ClaimKey { get; set; }
        /// <summary>
    /// Gets or sets the Claim Value.
    /// </summary>
public string ClaimValue { get; set; }
    
        /// <summary>
    /// Gets or sets the Account Id.
    /// </summary>
public int AccountId { get; set; }
}