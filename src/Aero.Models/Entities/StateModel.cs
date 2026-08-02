using Aero.Core.Entities;

namespace Aero.Models.Entities;

/// <summary>
/// Represents a class for StateModel.
/// </summary>
public class StateModel : Entity
{
        /// <summary>
    /// Gets or sets the Country Id.
    /// </summary>
[JsonPropertyName("country_id")]
    public long CountryId { get; set; }
        /// <summary>
    /// Gets or sets the Name.
    /// </summary>
[JsonPropertyName("name")]
    public string Name { get; set; }
        /// <summary>
    /// Gets or sets the Abbr.
    /// </summary>
[JsonPropertyName("abbr")]
    public string Abbr { get; set; }
}
