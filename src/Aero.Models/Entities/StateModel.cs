using Aero.Core.Entities;

namespace Aero.Models.Entities;

public class StateModel : Entity
{
    [JsonPropertyName("country_id")]
    public long CountryId { get; set; }
    [JsonPropertyName("name")]
    public string Name { get; set; }
    [JsonPropertyName("abbr")]
    public string Abbr { get; set; }
}
