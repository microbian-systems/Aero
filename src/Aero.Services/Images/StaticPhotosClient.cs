namespace Aero.Services.Images;

/// <summary>
public interface IStaticPhotosClient
{
    /// <summary>
    /// Gets the URL of a static photo based on the specified category, size, and index. If the index is not provided, a random index will be generated. The base URL for the photos can be configured via the HttpClient's BaseAddress property.
    /// </summary>
    /// <param name="category"></param>
    /// <param name="size"></param>
    /// <param name="index"></param>
    /// <returns></returns>
    string GetPhotoUrl(string category = "blurred", string size = "640x360", int? index = null);
}


/// <summary>
/// A client for fetching static photos from a service. The photos are categorized and can be fetched by category, size, and index. The index is a number between 1 and 100000, and if not provided, a random index will be generated. The base URL for the photos can be configured via the HttpClient's BaseAddress property.
/// /// static.photos/blurred/640x360/110 (the number at the end is any number form 1 to 100000)
///    - ## Sample Image Categories
///        - nature
///        - office
///        - people
///        - technology
///        - minimal
///        - abstract
///        - aerial
///        - blurred
///        - bokeh
///        - gradient
///        - monochrome
///        - vintage
///        - white
///        - black
///        - blue
///        - red
///        - green
///        - yellow
///        - cityscape
///        - workspace
///        - food
///        - travel
///        - textures
///        - industry
///        - indoor
///        - outdoor
///        - studio
///        - finance
///        - medical
///        - season
///        - holiday
///        - event
///        - sport
///        - science
///        - legal
///        - estate
///        - restaurant
///        - retail
///        - wellness
///        - agriculture
///        - construction
///        - craft
///        - cosmetic
///        - automotive
///        - gaming
///        - education
///
///    - ## Sample Image Sizes
///        - 200x200
///        - 320x240
///        - 640x360
///        - 1024x576
///        - 1200x630
/// </summary>
/// <param name="httpClient"></param>
public class StaticPhotosClient(HttpClient httpClient) : IStaticPhotosClient
{
    private readonly Random _random = new();

        /// <summary>
    /// GetPhotoUrl method.
    /// </summary>
public string GetPhotoUrl(string category = "blurred", string size = "640x360", int? index = null)
    {
        // the number at the end is any number from 1 to 100000
        var id = index ?? _random.Next(1, 100_001);
        id = Math.Clamp(id, 1, 100_000);
        
        var baseUri = httpClient.BaseAddress ?? new Uri("https://static.photos/");
        return new Uri(baseUri, $"{category}/{size}/{id}").ToString();
    }
}
