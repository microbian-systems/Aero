using System.Globalization;
using Aero.Core.Extensions;
using Aero.Core.Helpers;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Aero.Core;

/// <summary>
/// Represents a class for AzureBlobStorageClient.
/// </summary>
public class AzureBlobStorageClient(ILogger<AzureBlobStorageClient> log) : IBlobStorageClient
{

    // ?? JobLog.GetLog();

    // todo - convert MemorySTream to Stream
        /// <summary>
    /// Post method.
    /// </summary>
public void Post(MemoryStream ms, string filename, bool compress = true) =>
        PostAsync(ms, filename, Config.GetStorageConnectionString(),
            Config.GetSetting("FeedContainer"), compress).GetAwaiter().GetResult();

        /// <summary>
    /// PostAsync method.
    /// </summary>
public async Task PostAsync(MemoryStream ms, string filename, string connString, string path, bool compress = true, string contentType = "text/xml", bool forceLowerCase = true)
    {
        if (forceLowerCase)
            filename = filename.ToLower(CultureInfo.InvariantCulture); // filenames are case sensitive
        if (string.IsNullOrEmpty(filename))
            throw new ArgumentException($"{nameof(filename)} argument was not specified. the blob filename must have a value");

        if (string.IsNullOrEmpty(path))
        {
            log.LogInformation($"blob storage container name was null. defaulting to feeds");
            path = "feeds";
        }

        path = path.ToLower();
        var acct = CloudStorageAccount.Parse(connString);
        log.LogInformation($"getting blob storage for {acct.BlobStorageUri} and file {filename} - compressed = {compress}");
        var cbc = acct.CreateCloudBlobClient();
        //var blob = cbc.GetBlobReference(container + "/" + filename);
        var bsp = ParseContainerPath(path);

        var conref = cbc.GetContainerReference(bsp.Container);
        await conref.CreateIfNotExistsAsync();
        await conref.SetPermissionsAsync(new BlobContainerPermissions
        {
            PublicAccess = BlobContainerPublicAccessType.Blob
        });

        var dir = conref.GetLastDirectoryReference(bsp.FoldersList);
        var blob = dir?.GetBlockBlobReference(filename) ?? conref.GetBlockBlobReference(filename);
        if (compress)
            blob.Properties.ContentEncoding = "gzip";
        blob.Properties.ContentType = contentType;

        //blob.UploadFromStream(ms);
        //blob.UploadByteArray(ms.ToArray());
        if (!compress)
            await blob.UploadFromStreamAsync(ms);
        else
            await blob.UploadFromStreamAsync(ms); // todo - make this async compat
        //blob.UploadFromStream(ms.Compress());
        //blob.UploadByteArray(ms.Compress().ToArray());
        log.LogInformation($"successfully uploaded blob storage file {filename} at {acct.BlobStorageUri} @ {path}");
    }



        /// <summary>
    /// ParseContainerPath method.
    /// </summary>
protected BlobStoragePath ParseContainerPath(string path)
    {
        var paths = path.Replace("//", "/")
            .StripTrailingBackSlash()
            .Split('/').ToList();
        var bsp = new BlobStoragePath
        {
            Container = paths.FirstOrDefault()
        };
        bsp.FoldersList.AddRange(paths.Skip(1));
        return bsp;
    }
}