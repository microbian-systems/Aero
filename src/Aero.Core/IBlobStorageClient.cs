namespace Aero.Core;

/// <summary>
/// Defines an interface for IBlobStorageClient.
/// </summary>
public interface IBlobStorageClient
{
        /// <summary>
    /// Post method.
    /// </summary>
void Post(MemoryStream ms, string filename, bool compress = true);
        /// <summary>
    /// PostAsync method.
    /// </summary>
Task PostAsync(MemoryStream ms, string filename, string connString, string container, bool compress = true, string contenttype = "text/xml", bool forceLowerCase = true);
}