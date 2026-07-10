namespace Aero.Models;

/// <summary>
/// Defines an interface for IAuthRequestModel.
/// </summary>
public interface IAuthRequestModel
{
        /// <summary>
    /// Gets or sets the Id.
    /// </summary>
[JsonPropertyName("id")]
    string Id { get; init; }
}

/// <summary>
/// Defines an interface for IApiKeyAuthRequestModel.
/// </summary>
public interface IApiKeyAuthRequestModel : IAuthRequestModel
{
        /// <summary>
    /// Gets or sets the Api Key.
    /// </summary>
[JsonPropertyName("api_key")]
    string ApiKey
    {
        get => Id;
        init => Id = value;
    }
}

/// <summary>
/// Defines an interface for IBasicAuthRequestModel.
/// </summary>
public interface IBasicAuthRequestModel : IAuthRequestModel
{
        /// <summary>
    /// Gets or sets the Username.
    /// </summary>
[JsonPropertyName("username")]
    public string Username
    {
        get => Id;
        init => Id = value;
    }
    
        /// <summary>
    /// Gets or sets the Password.
    /// </summary>
[JsonPropertyName("password")]
    public string Password { get; init; }
}