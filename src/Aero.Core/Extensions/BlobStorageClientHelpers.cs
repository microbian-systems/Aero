using Azure.Storage.Blobs;

namespace Aero.Core.Extensions;

/// <summary>Resolves blob names for the historical Aero media storage API.</summary>
public static class BlobStorageClientHelpers
{
    /// <summary>
    /// Gets a modern blob client, preserving the old last-directory-only behavior.
    /// </summary>
    /// <remarks>
    /// Replaces GetLastDirectoryReference, whose signature exposed removed
    /// WindowsAzure.Storage types. External callers must migrate and recompile.
    /// </remarks>
    public static BlobClient GetLastDirectoryBlobClient(this BlobContainerClient container,
        IReadOnlyList<string> folders, string filename)
    {
        var folder = folders.Count == 0 ? "" : folders[^1];
        var blobName = string.IsNullOrEmpty(folder) ? filename : $"{folder.TrimEnd('/')}/{filename}";
        return container.GetBlobClient(blobName);
    }
}
