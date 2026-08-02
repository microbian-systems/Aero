namespace Aero.Core;

/// <summary>
/// Represents a class for BlobStoragePath.
/// </summary>
public class BlobStoragePath
{
        /// <summary>
    /// Gets or sets the Container.
    /// </summary>
public string Container { get; set; } = "";
        /// <summary>
    /// Gets or sets the Folders List.
    /// </summary>
public List<string> FoldersList { get; protected set; } = new();
}