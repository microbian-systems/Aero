namespace Aero.MartenDB;

/// <summary>
/// Configuration settings for AeroDB persistence
/// </summary>
public class AeroDbSettings
{
    public const string SectionName = "AeroDb";

    // todo - configure postgres to make use of an embedded server
    // https://github.com/mysticmind/mysticmind-postgresembed
    /// <summary>
    /// Whether to use embedded AeroDB mode.
    /// Note: For actual embedded mode, AeroDB.Embedded NuGet package is required.
    /// When false, uses standard server connection.
    /// </summary>
    public bool UseEmbedded { get; set; } = false;

    /// <summary>
    /// Path for embedded database (used when UseEmbedded is true).
    /// Ignored if UseEmbedded is false.
    /// </summary>
    public string? EmbeddedPath { get; set; } = ".";
    
    /// <summary>
    /// AeroDB server URL(s) - can be comma-separated for multiple nodes
    /// </summary>
    public string? Hosts { get; set; } = "localhost:8080";
    
    /// <summary>
    /// Database name
    /// </summary>
    public string DatabaseName { get; set; } = "AeroDb";
}
