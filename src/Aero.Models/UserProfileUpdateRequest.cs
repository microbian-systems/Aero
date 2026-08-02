namespace Aero.Models;

/// <summary>
/// Represents a class for UserProfileUpdateRequest.
/// </summary>
public class UserProfileUpdateRequest
{
        /// <summary>
    /// Gets or sets the Id.
    /// </summary>
[JsonPropertyName("id")]
    public string? Id {get; set;}
        
        /// <summary>
    /// Gets or sets the Name.
    /// </summary>
[JsonPropertyName("name")]
    public string Name { get; set; }
        
        /// <summary>
    /// Gets or sets the Website.
    /// </summary>
[JsonPropertyName("website")]
    public string Website { get; set; }

        /// <summary>
    /// Gets or sets the Social Media.
    /// </summary>
[JsonPropertyName("social_media")] 
    public Dictionary<string, string> SocialMedia { get; set; } = new();

//        [JsonPropertyName("firstname")] 
//        public string Firstname { get; set; }
//        
//        [JsonPropertyName("lastname")]
//        public string Lastname { get; set; }
        
        /// <summary>
    /// Gets or sets the Tagline.
    /// </summary>
[JsonPropertyName("tagline")]
    public string Tagline { get; set; }
        
        /// <summary>
    /// Gets or sets the Location.
    /// </summary>
[JsonPropertyName("location")]
    public string Location { get; set; }
        
        /// <summary>
    /// Gets or sets the Bio.
    /// </summary>
[JsonPropertyName("bio")]
    public string Bio { get; set; }
}