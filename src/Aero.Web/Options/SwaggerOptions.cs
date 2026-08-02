namespace Aero.Web.Options;

/// <summary>
/// Represents a record for SwaggerOptions.
/// </summary>
public record SwaggerOptions
{
        /// <summary>
    /// Gets or sets the Version.
    /// </summary>
public string? Version { get; set; }
        /// <summary>
    /// Gets or sets the Title.
    /// </summary>
public string? Title { get; set; }
        /// <summary>
    /// Gets or sets the Description.
    /// </summary>
public string? Description { get; set; }
        /// <summary>
    /// Gets or sets the Terms Of Service Url.
    /// </summary>
public string? TermsOfServiceUrl { get; set; }
        /// <summary>
    /// Gets or sets the Contact Name.
    /// </summary>
public string? ContactName { get; set; }
        /// <summary>
    /// Gets or sets the Contact Email.
    /// </summary>
public string? ContactEmail { get; set; }
        /// <summary>
    /// Gets or sets the Contact Url.
    /// </summary>
public string? ContactUrl { get; set; }
        /// <summary>
    /// Gets or sets the License Name.
    /// </summary>
public string? LicenseName { get; set; }
        /// <summary>
    /// Gets or sets the License Url.
    /// </summary>
public string? LicenseUrl { get; set; }
}