namespace Aero.Secrets.Models;

/// <summary>
/// Represents a record for InfisicalSecretManagerOptions.
/// </summary>
public sealed record InfisicalSecretManagerOptions
{
        /// <summary>
    /// Gets or sets the Host Uri.
    /// </summary>
public Uri HostUri { get; init; } = new("http://localhost:8080");

        /// <summary>
    /// Gets or sets the Project Id.
    /// </summary>
public string ProjectId { get; init; } = string.Empty;

        /// <summary>
    /// Gets or sets the Environment Slug.
    /// </summary>
public string EnvironmentSlug { get; init; } = string.Empty;

        /// <summary>
    /// Gets or sets the Secret Path.
    /// </summary>
public string SecretPath { get; init; } = "/";

        /// <summary>
    /// Gets or sets the Machine Id.
    /// </summary>
public string MachineId { get; init; } = string.Empty;

        /// <summary>
    /// Gets or sets the Client Secret.
    /// </summary>
public string ClientSecret { get; init; } = string.Empty;
}
