using System.Net.Http.Json;
using System.Text.Json;
using Aero.Core.Http;
using Microsoft.Extensions.Logging;

namespace Aero.Social.Forem;

// todo - stitch this in: https://dev.to/tiaeastwood/how-to-use-the-forem-api-to-display-your-devto-blog-posts-on-your-website-easy-3dl3

public class ForemArticleService(HttpClient client, ILogger<ForemArticleService> log) : HttpClientBase(client, log)
{
    private readonly HttpClient _httpClient = client;

    public async Task<ArticleCreateResponse?> CreateArticleAsync(ArticleCreateRequest request, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync("/api/articles", request, cancellationToken);

        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<ArticleCreateResponse>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }
}