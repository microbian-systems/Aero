namespace Aero.Services.Images;

public interface IStaticPhotosClient
{
    string GetPhotoUrl(string category = "blurred", string size = "640x360", int? index = null);
}

public class StaticPhotosClient(HttpClient httpClient) : IStaticPhotosClient
{
    private readonly Random _random = new();

    public string GetPhotoUrl(string category = "blurred", string size = "640x360", int? index = null)
    {
        // the number at the end is any number from 1 to 100000
        var id = index ?? _random.Next(1, 100_001);
        id = Math.Clamp(id, 1, 100_000);
        
        var baseUri = httpClient.BaseAddress ?? new Uri("https://static.photos/");
        return new Uri(baseUri, $"{category}/{size}/{id}").ToString();
    }
}
