using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Aero.Core.Railway;
using Microsoft.Extensions.Logging;

namespace Aero.Core.Http;

public abstract class HttpClientBaseV2(
    HttpClient httpClient,
    ILogger logger,
    Polly.ResiliencePipeline<HttpResponseMessage>? resiliencePipeline = null)
{
    protected readonly ILogger Logger = logger;
    protected readonly HttpClient HttpClient = httpClient;
    protected readonly string JsonMediaType = "application/json";
    protected readonly Polly.ResiliencePipeline<HttpResponseMessage>? ResiliencePipeline = resiliencePipeline;

    protected virtual async Task<HttpResponseMessage> GetAsync(string url)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        return await SendWithResilienceAsync(request);
    }

    protected virtual async Task<Result<string, T>> GetResultAsync<T>(string url, CancellationToken ct = default)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        return await SendResultAsync<T>(request, ct);
    }

    protected virtual async Task<HttpResponseMessage> PostAsync<T>(string url, T data) where T : class
    {
        var request = CreateRequest(url, HttpMethod.Post, data);
        return await SendWithResilienceAsync(request);
    }

    protected virtual async Task<Result<string, TResponse>> PostResultAsync<TRequest, TResponse>(string url, TRequest data, CancellationToken ct = default) 
        where TRequest : class
    {
        var request = CreateRequest(url, HttpMethod.Post, data);
        return await SendResultAsync<TResponse>(request, ct);
    }

    protected virtual Task<HttpResponseMessage> PostAsync<T>(Uri url, T data) where T : class
        => PostAsync(url.ToString(), data);

    protected virtual async Task<HttpResponseMessage> PutAsync<T>(string url, T data) where T : class
    {
        var request = CreateRequest(url, HttpMethod.Put, data);
        return await SendWithResilienceAsync(request);
    }

    protected virtual async Task<Result<string, TResponse>> PutResultAsync<TRequest, TResponse>(string url, TRequest data, CancellationToken ct = default) 
        where TRequest : class
    {
        var request = CreateRequest(url, HttpMethod.Put, data);
        return await SendResultAsync<TResponse>(request, ct);
    }

    protected virtual async Task<HttpResponseMessage> DeleteAsync(string url)
    {
        var request = new HttpRequestMessage(HttpMethod.Delete, url);
        return await SendWithResilienceAsync(request);
    }

    protected virtual async Task<Result<string, bool>> DeleteResultAsync(string url, CancellationToken ct = default)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Delete, url);
            var response = await SendWithResilienceAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var error = $"DELETE {url} failed with status {response.StatusCode}";
                Logger.LogWarning(error);
                return error;
            }

            return true;
        }
        catch (Exception ex)
        {
            var error = $"Exception during DELETE {url}";
            Logger.LogError(ex, error);
            return $"{error}: {ex.Message}";
        }
    }

    protected virtual async Task<HttpResponseMessage> PatchAsync<T>(string url, T data) where T : class
        => await PatchAsync(new Uri(url, UriKind.RelativeOrAbsolute), data);

    protected virtual async Task<HttpResponseMessage> PatchAsync<T>(Uri url, T data) where T : class
    {
        var request = CreateRequest(url, HttpMethod.Patch, data);
        return await SendWithResilienceAsync(request);
    }

    protected virtual async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
        => await SendWithResilienceAsync(request);

    protected virtual async Task<(T? Result, HttpResponseMessage Response)> SendAsync<T>(HttpRequestMessage request) where T : class
    {
        var response = await SendWithResilienceAsync(request);
        var result = await DeserializeAsync<T>(response);
        return (result, response);
    }

    protected virtual async Task<Result<string, T>> SendResultAsync<T>(HttpRequestMessage request, CancellationToken ct = default)
    {
        var url = request.RequestUri?.ToString() ?? "unknown";
        Logger.LogInformation("Sending {Method} request to {Url}...", request.Method, url);
        try
        {
            var response = await SendWithResilienceAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var error = $"{request.Method} {url} failed with status {response.StatusCode}";
                Logger.LogWarning(error);
                return error;
            }

            var result = await DeserializeAsync<T>(response);
            return result!;
        }
        catch (Exception ex)
        {
            var error = $"Exception during {request.Method} {url}";
            Logger.LogError(ex, error);
            return $"{error}: {ex.Message}";
        }
    }

    protected virtual HttpRequestMessage CreateRequest(string url, HttpMethod method)
        => CreateRequest<object>(url, method, null);

    protected virtual HttpRequestMessage CreateRequest(Uri uri, HttpMethod method)
        => CreateRequest<object>(uri, method, null);

    protected virtual HttpRequestMessage CreateRequest<T>(string url, HttpMethod method, T? data) where T : class
        => CreateRequest(new Uri(url, UriKind.RelativeOrAbsolute), method, data);

    protected virtual HttpRequestMessage CreateRequest<T>(Uri uri, HttpMethod method, T? data) where T : class
    {
        if (uri == null || (uri.IsAbsoluteUri && string.IsNullOrEmpty(uri.AbsoluteUri)) || (!uri.IsAbsoluteUri && string.IsNullOrEmpty(uri.OriginalString)))
             throw new ArgumentNullException(nameof(uri), "Url cannot be null or empty");

        if (method is null)
            throw new ArgumentNullException(nameof(method), "HttpMethod cannot be null");

        var request = new HttpRequestMessage(method, uri);

        if (data is not null)
        {
            var json = JsonSerializer.Serialize(data);
            request.Content = new StringContent(json, Encoding.UTF8, JsonMediaType);
        }

        return request;
    }

    protected virtual HttpRequestMessage CreateFormRequest(string url, HttpMethod method, FormUrlEncodedContent content)
    {
        var request = new HttpRequestMessage(method, url)
        {
            Content = content
        };
        return request;
    }

    protected virtual HttpRequestMessage CreateMultipartRequest(string url, HttpMethod method, MultipartFormDataContent content)
    {
        var request = new HttpRequestMessage(method, url)
        {
            Content = content
        };
        return request;
    }

    private async Task<HttpResponseMessage> SendWithResilienceAsync(HttpRequestMessage request)
    {
        if (ResiliencePipeline is not null)
        {
            return await ResiliencePipeline.ExecuteAsync(async ct =>
            {
                var clonedRequest = await CloneRequestAsync(request);
                return await HttpClient.SendAsync(clonedRequest, ct);
            });
        }

        return await HttpClient.SendAsync(request);
    }

    private static async Task<HttpRequestMessage> CloneRequestAsync(HttpRequestMessage request)
    {
        var cloned = new HttpRequestMessage(request.Method, request.RequestUri);

        foreach (var header in request.Headers)
        {
            cloned.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        if (request.Content is not null)
        {
            var content = await request.Content.ReadAsByteArrayAsync();
            cloned.Content = new ByteArrayContent(content);

            foreach (var header in request.Content.Headers)
            {
                cloned.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
        }

        return cloned;
    }

    protected virtual JsonSerializerOptions GetDefaultSerializerOptions() =>
        new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            AllowTrailingCommas = true,
            PropertyNameCaseInsensitive = true,
            ReferenceHandler = ReferenceHandler.IgnoreCycles
        };

    protected virtual Task<T?> DeserializeAsync<T>(string json)
        => DeserializeAsync<T>(json, GetDefaultSerializerOptions());

    protected virtual async Task<T?> DeserializeAsync<T>(string json, JsonSerializerOptions opts)
    {
        if (string.IsNullOrEmpty(json))
        {
            Logger.LogWarning("JSON was null or empty. Unable to convert to type {Type}", typeof(T).Name);
            return default;
        }

        var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
        return await JsonSerializer.DeserializeAsync<T>(stream, opts);
    }

    protected virtual async Task<T?> DeserializeAsync<T>(HttpResponseMessage response)
        => await DeserializeAsync<T>(response, GetDefaultSerializerOptions());

    protected virtual async Task<T?> DeserializeAsync<T>(HttpResponseMessage response, JsonSerializerOptions opts)
    {
        var str = await response.Content.ReadAsStringAsync();
        return await DeserializeAsync<T>(str, opts);
    }

    protected virtual async Task<T?> DeserializeAsync<T>(Stream stream)
        => await DeserializeAsync<T>(stream, GetDefaultSerializerOptions());

    protected virtual async Task<T?> DeserializeAsync<T>(Stream stream, JsonSerializerOptions opts)
    {
        return await JsonSerializer.DeserializeAsync<T>(stream, opts);
    }

    protected virtual async Task<byte[]?> DownloadBytesAsync(string url)
    {
        var response = await GetAsync(url);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsByteArrayAsync();
    }

    protected virtual async Task<Stream?> DownloadStreamAsync(string url)
    {
        var response = await GetAsync(url);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStreamAsync();
    }
}