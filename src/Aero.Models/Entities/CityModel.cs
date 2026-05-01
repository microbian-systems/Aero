using System.ComponentModel.DataAnnotations;
using Aero.Core.Entities;

namespace Aero.Models.Entities;

public class CityModel : Entity<int>
{
    [JsonPropertyName("state_id")]
    public long StateId { get; set; }
    [JsonPropertyName("fips")]
    [MaxLength(128)]
    public string FIPS { get; set; }    
    [JsonPropertyName("iso")]
    [MaxLength(128)]
    public string ISO { get; set; }
    [JsonPropertyName("name")]
    [MaxLength(128)]
    public string Name { get; set; }
}