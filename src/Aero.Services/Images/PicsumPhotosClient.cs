using System.Net.Http.Json;

namespace Aero.Services.Images;

/// <summary>
/// Represents a record for PicsumImage.
/// </summary>
public record PicsumImage(string Id, string Author, int Width, int Height, string Url, string DownloadUrl);

/// <summary>
/// Defines an interface for IPicsumPhotosClient.
/// </summary>
public interface IPicsumPhotosClient
{
        /// <summary>
    /// GetPhotoUrl method.
    /// </summary>
string GetPhotoUrl(int width, int height, string? id = null, string? seed = null, bool grayscale = false, int? blur = null);
        /// <summary>
    /// GetSquarePhotoUrl method.
    /// </summary>
string GetSquarePhotoUrl(int size, string? id = null, string? seed = null, bool grayscale = false, int? blur = null);
        /// <summary>
    /// ListImagesAsync method.
    /// </summary>
Task<IEnumerable<PicsumImage>> ListImagesAsync(int page = 1, int limit = 30);
        /// <summary>
    /// GetImageInfoAsync method.
    /// </summary>
Task<PicsumImage?> GetImageInfoAsync(string idOrSeed, bool isSeed = false);
}

/// <summary>
/// Represents a class for PicsumPhotosClient.
/// </summary>
public class PicsumPhotosClient(HttpClient httpClient) : IPicsumPhotosClient
{
        /// <summary>
    /// GetPhotoUrl method.
    /// </summary>
public string GetPhotoUrl(int width, int height, string? id = null, string? seed = null, bool grayscale = false, int? blur = null)
    {
        var path = "";
        if (seed != null) path = $"seed/{seed}/";
        else if (id != null) path = $"id/{id}/";

        var url = $"{path}{width}/{height}";
        return BuildUrl(url, grayscale, blur);
    }

        /// <summary>
    /// GetSquarePhotoUrl method.
    /// </summary>
public string GetSquarePhotoUrl(int size, string? id = null, string? seed = null, bool grayscale = false, int? blur = null)
    {
        var path = "";
        if (seed != null) path = $"seed/{seed}/";
        else if (id != null) path = $"id/{id}/";

        var url = $"{path}{size}";
        return BuildUrl(url, grayscale, blur);
    }

        /// <summary>
    /// ListImagesAsync method.
    /// </summary>
public async Task<IEnumerable<PicsumImage>> ListImagesAsync(int page = 1, int limit = 30)
    {
        return await httpClient.GetFromJsonAsync<IEnumerable<PicsumImage>>($"v2/list?page={page}&limit={limit}") ?? Enumerable.Empty<PicsumImage>();
    }

        /// <summary>
    /// GetImageInfoAsync method.
    /// </summary>
public async Task<PicsumImage?> GetImageInfoAsync(string idOrSeed, bool isSeed = false)
    {
        var path = isSeed ? "seed" : "id";
        return await httpClient.GetFromJsonAsync<PicsumImage>($"{path}/{idOrSeed}/info");
    }

    private string BuildUrl(string basePath, bool grayscale, int? blur)
    {
        var query = new List<string>();
        if (grayscale) query.Add("grayscale");
        if (blur.HasValue) query.Add($"blur={Math.Clamp(blur.Value, 1, 10)}");

        var queryString = query.Count > 0 ? "?" + string.Join("&", query) : "";
        var baseUri = httpClient.BaseAddress ?? new Uri("https://picsum.photos/");
        return new Uri(baseUri, $"{basePath}{queryString}").ToString();
    }
}
