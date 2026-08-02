using System.ComponentModel.DataAnnotations;
using Aero.Core.Entities;

namespace Aero.Models.Entities;

/// <summary>
/// Represents a class for CityModel.
/// </summary>
public class CityModel : Entity<int>
{
        /// <summary>
    /// Gets or sets the State Id.
    /// </summary>
[JsonPropertyName("state_id")]
    public long StateId { get; set; }
        /// <summary>
    /// Gets or sets the FIPS.
    /// </summary>
[JsonPropertyName("fips")]
    [MaxLength(128)]
    public string FIPS { get; set; }    
        /// <summary>
    /// Gets or sets the ISO.
    /// </summary>
[JsonPropertyName("iso")]
    [MaxLength(128)]
    public string ISO { get; set; }
        /// <summary>
    /// Gets or sets the Name.
    /// </summary>
[JsonPropertyName("name")]
    [MaxLength(128)]
    public string Name { get; set; }
}