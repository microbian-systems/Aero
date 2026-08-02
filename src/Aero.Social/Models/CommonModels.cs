namespace Aero.Social.Models;

/// <summary>
/// Represents a class for ClientInformation.
/// </summary>
public class ClientInformation
{
        /// <summary>
    /// Gets or sets the Client Id.
    /// </summary>
public string ClientId { get; set; } = string.Empty;
        /// <summary>
    /// Gets or sets the Client Secret.
    /// </summary>
public string ClientSecret { get; set; } = string.Empty;
        /// <summary>
    /// Gets or sets the Instance Url.
    /// </summary>
public string InstanceUrl { get; set; } = string.Empty;
}

/// <summary>
/// Represents a class for FetchPageInformationResult.
/// </summary>
public class FetchPageInformationResult
{
        /// <summary>
    /// Gets or sets the Id.
    /// </summary>
public string Id { get; set; } = string.Empty;
        /// <summary>
    /// Gets or sets the Name.
    /// </summary>
public string Name { get; set; } = string.Empty;
        /// <summary>
    /// Gets or sets the Access Token.
    /// </summary>
public string AccessToken { get; set; } = string.Empty;
        /// <summary>
    /// Gets or sets the Picture.
    /// </summary>
public string Picture { get; set; } = string.Empty;
        /// <summary>
    /// Gets or sets the Username.
    /// </summary>
public string Username { get; set; } = string.Empty;
}

/// <summary>
/// Represents a class for MentionResult.
/// </summary>
public class MentionResult
{
        /// <summary>
    /// Gets or sets the Id.
    /// </summary>
public string Id { get; set; } = string.Empty;
        /// <summary>
    /// Gets or sets the Label.
    /// </summary>
public string Label { get; set; } = string.Empty;
        /// <summary>
    /// Gets or sets the Image.
    /// </summary>
public string Image { get; set; } = string.Empty;
        /// <summary>
    /// Gets or sets the Do Not Cache.
    /// </summary>
public bool DoNotCache { get; set; }
}

/// <summary>
/// Represents a class for NoMentionResult.
/// </summary>
public class NoMentionResult
{
        /// <summary>
    /// Gets or sets the None.
    /// </summary>
public bool None => true;
}
