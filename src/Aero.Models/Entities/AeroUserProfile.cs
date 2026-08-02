using System.ComponentModel.DataAnnotations;
using Aero.Core;
using Aero.Core.Entities;

namespace Aero.Models.Entities;

// todo - determine what format to store the profile
// todo - later denormalize if join performance costs too much (cache first, then denormalize)
// todo - add foreign key to the Users (AspNetUsers) table
// https://www.npgsql.org/efcore/mapping/json.html?tabs=data-annotations%2Cpoco
/// <summary>
/// Represents a class for AeroUserProfile.
/// </summary>
public class AeroUserProfile : Entity
{
        /// <summary>
    /// Initializes a new instance of the <see cref="AeroUserProfile"/> class.
    /// </summary>
public AeroUserProfile()
    {
        
    }
    /// <summary>
    /// Foreign key to the Aero Identity table
    /// </summary>
    [JsonPropertyName("user_id")]
    public long Userid { get; set; } // todo - make this generic so the type can vary for pkey

        /// <summary>
    /// Gets or sets the Username.
    /// </summary>
[MinLength(4)] // todo - remove data annotations and use FluentValidation
    [MaxLength(256)]
    [JsonPropertyName("username")]
    public string Username { get; set; }

        /// <summary>
    /// Gets or sets the Website.
    /// </summary>
[Url]
    [MinLength(4)]
    [MaxLength(1024)]
    [JsonPropertyName("website")]
    public string? Website { get; set; }

        /// <summary>
    /// Gets or sets the Social Media.
    /// </summary>
[JsonPropertyName("social_media")]
    public Dictionary<SocialMediaType, string> SocialMedia { get; } = [];

        /// <summary>
    /// Gets or sets the Headline.
    /// </summary>
[MaxLength(128)]
    [JsonPropertyName("headline")]
    public string Headline { get; set; }

        /// <summary>
    /// Gets or sets the Location.
    /// </summary>
[MaxLength(128)]
    [JsonPropertyName("location")]
    public string Location { get; set; }

        /// <summary>
    /// Gets or sets the Bio.
    /// </summary>
[MaxLength(1024)]
    [JsonPropertyName("bio")]
    public string? Bio { get; set; }

    /// <summary>
    /// Can store as base64 encoded image or path to url
    /// </summary>
    [Url]
    [MaxLength(1024)]
    [JsonPropertyName("image_url")]
    public string? ImageUrl { get; set; }
        /// <summary>
    /// Gets or sets the Address.
    /// </summary>
public AddressModel? Address { get; set; }
}