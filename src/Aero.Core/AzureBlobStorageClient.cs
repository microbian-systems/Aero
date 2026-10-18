using System.Globalization;
using Aero.Core.Extensions;
using Aero.Core.Helpers;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace Aero.Core;

/// <summary>Uploads blobs using the historical Aero storage contract.</summary>
/// <remarks>
/// This legacy media API enables public blob access and overwrites existing
/// blobs. It is not suitable for private, create-only attachment originals.
/// </remarks>
public class AzureBlobStorageClient(ILogger<AzureBlobStorageClient> log) : IBlobStorageClient
{
    /// <summary>Uploads using the application-configured connection and container.</summary>
    public void Post(MemoryStream ms, string filename, bool compress = true) =>
        PostAsync(ms, filename, Config.GetStorageConnectionString(),
            Config.GetSetting("FeedContainer"), compress).GetAwaiter().GetResult();

    /// <summary>Uploads from the current stream position, leaving the stream open.</summary>
    /// <remarks>
    /// For compatibility, compress only sets the gzip content-encoding label;
    /// it does not transform the supplied bytes. Nested paths retain the legacy
    /// last-directory-only interpretation.
    /// </remarks>
    public async Task PostAsync(MemoryStream ms, string filename, string connString,
        string path, bool compress = true, string contentType = "text/xml", bool forceLowerCase = true)
    {
        if (forceLowerCase)
            filename = filename.ToLower(CultureInfo.InvariantCulture);
        if (string.IsNullOrEmpty(filename))
            throw new ArgumentException($"{nameof(filename)} argument was not specified. the blob filename must have a value");

        if (string.IsNullOrEmpty(path))
        {
            log.LogInformation("Blob storage container name was null. Defaulting to feeds");
            path = "feeds";
        }

        // Preserve the existing current-culture path normalization.
        var storagePath = ParseContainerPath(path.ToLower());
        var service = new BlobServiceClient(connString);
        var container = service.GetBlobContainerClient(storagePath.Container);
        await container.CreateIfNotExistsAsync().ConfigureAwait(false);
        // Must also run for existing containers; creation alone is not equivalent.
        await container.SetAccessPolicyAsync(PublicAccessType.Blob).ConfigureAwait(false);
        var blob = container.GetLastDirectoryBlobClient(storagePath.FoldersList, filename);
        await blob.UploadAsync(ms, new BlobUploadOptions
        {
            HttpHeaders = new BlobHttpHeaders
            {
                ContentType = contentType,
                ContentEncoding = compress ? "gzip" : null
            }
        }).ConfigureAwait(false);
        // Never log the connection string, SAS query, or signed request headers.
        log.LogInformation("Uploaded blob {Filename} in container {Container}", filename, storagePath.Container);
    }

    /// <summary>Parses the historical container/folder path format.</summary>
    protected BlobStoragePath ParseContainerPath(string path)
    {
        var paths = path.Replace("//", "/").StripTrailingBackSlash().Split('/');
        var storagePath = new BlobStoragePath { Container = paths.FirstOrDefault()! };
        storagePath.FoldersList.AddRange(paths.Skip(1));
        return storagePath;
    }
}
