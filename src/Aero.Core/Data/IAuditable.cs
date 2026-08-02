using System.Text.Json.Serialization;

namespace Aero.Core.Data;

/// <summary>
/// Contract for entities that maintain audit metadata — who created/modified the record and when.
/// </summary>
public interface IAuditable
{
    [JsonPropertyName("created_on")]
    DateTimeOffset CreatedOn { get; set; }

    [JsonPropertyName("modified_on")]
    DateTimeOffset? ModifiedOn { get; set; }

    [JsonPropertyName("created_by")]
    string? CreatedBy { get; set; }

    [JsonPropertyName("updated_by")]
    string? ModifiedBy { get; set; }
}
