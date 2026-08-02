using Microsoft.WindowsAzure.Storage.Blob;

namespace Aero.Core.Extensions;

/// <summary>
/// Represents a class for BlobStorageClientHelpers.
/// </summary>
public static class BlobStorageClientHelpers
{
        /// <summary>
    /// GetLastDirectoryReference method.
    /// </summary>
public static CloudBlobDirectory GetLastDirectoryReference(this CloudBlobContainer container, List<string> folders)
    {
        var dirs = new List<CloudBlobDirectory>();
        foreach (var folder in folders)
        {
            var directory = container.GetDirectoryReference(folder);
            dirs.Add(directory);
        }

        return dirs.Count > 0 ? dirs.Last() : null;
    }
}