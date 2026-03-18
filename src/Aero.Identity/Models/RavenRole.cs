using Microsoft.AspNetCore.Identity;

namespace Aero.Identity.Models;

/// <summary>
/// AeroDB document model for an identity role claim.
/// </summary>
public class AeroRoleClaim
{
    public string ClaimType { get; set; } = string.Empty;
    public string ClaimValue { get; set; } = string.Empty;
}

/// <summary>
/// AeroDB document model for an identity role.
/// </summary>
public class AeroRole : IdentityRole<string>
{
    /// <summary>
    /// Gets or sets the list of claims associated with the role.
    /// </summary>
    public List<AeroRoleClaim> Claims { get; set; } = [];
}

