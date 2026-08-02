using Aero.Core.Entities;

namespace Aero.Services.Models;

/// <summary>
/// Represents a record for UserViewModel.
/// </summary>
public record UserViewModel : UserViewModel<long>;

/// <summary>
/// Represents a record for UserViewModel.
/// </summary>
public record UserViewModel<TKey> : IEntity<TKey>
    where TKey : IEquatable<TKey> , IComparable<TKey>
{
    // [JsonPropertyName("id")]
    // public TKey Id { get; set; }

        /// <summary>
    /// Gets or sets the First Name.
    /// </summary>
[JsonPropertyName("firstname")]
    public string FirstName { get; set; }

        /// <summary>
    /// Gets or sets the Last Name.
    /// </summary>
[JsonPropertyName("lastname")]
    public string LastName { get; set; }

        /// <summary>
    /// Gets or sets the Username.
    /// </summary>
[JsonPropertyName("username")]
    public string Username { get; set; }

        /// <summary>
    /// Gets or sets the Email.
    /// </summary>
[JsonPropertyName("email")]
    public string Email { get; set; }

    // [JsonPropertyName("password")]
    // public string Password { get; set; } = null;

        /// <summary>
    /// Gets or sets the Token.
    /// </summary>
[JsonPropertyName("token")]
    public string Token { get; set; }

        /// <summary>
    /// Gets or sets the Refresh Token.
    /// </summary>
[JsonPropertyName("refresh_token")]
    public string RefreshToken { get; set; }

        /// <summary>
    /// Gets or sets the Roles.
    /// </summary>
[JsonPropertyName("roles")]
    public List<string> Roles { get; } = [];

        /// <summary>
    /// Gets or sets the Claims.
    /// </summary>
[JsonPropertyName("claims")]
    public List<Claim> Claims { get; } = [];

        /// <summary>
    /// Gets or sets the Id.
    /// </summary>
public TKey Id { get; set; }
        /// <summary>
    /// Gets or sets the Created On.
    /// </summary>
public DateTimeOffset CreatedOn { get; set; }
        /// <summary>
    /// Gets or sets the Modified On.
    /// </summary>
public DateTimeOffset? ModifiedOn { get; set; } = DateTime.UtcNow;
        /// <summary>
    /// Gets or sets the Created By.
    /// </summary>
public string CreatedBy { get; set; }
        /// <summary>
    /// Gets or sets the Modified By.
    /// </summary>
public string ModifiedBy { get; set; }
}