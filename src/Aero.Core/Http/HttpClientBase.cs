using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Aero.Core.Http;

public abstract class HttpClientBase(HttpClient httpClient, ILogger<HttpClientBase> log)
{
    protected readonly ILogger<HttpClientBase> log = log;
    protected readonly HttpClient httpClient = httpClient;
    protected readonly string jsonMediaType = "application/json";

    public virtual async Task<HttpResponseMessage> GetAsync(string url, CancellationToken ct = default) 
        => await GetAsync(new Uri(url), ct);

    public virtual async Task<HttpResponseMessage> GetAsync(Uri uri, CancellationToken ct = default)
    {
        var response = await httpClient.GetAsync(uri, ct);
        if(!response.IsSuccessStatusCode)
        {
            var ex = new HttpRequestException($"Failed http GET request for {response.RequestMessage.RequestUri}: {response.StatusCode} : {response.ReasonPhrase}");
            log.LogError(ex, ex.Message);
        }
        return response;
    }

    public virtual async Task<HttpResponseMessage> PostAsync<T>(string url, T data, CancellationToken ct = default)
        where T : class
    {
        var request = new HttpRequestMessage(HttpMethod.Post, url);
        var serialized = JsonSerializer.Serialize(data);
        request.Content = new StringContent(serialized, Encoding.UTF8, jsonMediaType);
        var response = await httpClient.SendAsync(request, ct);

        if (!response.IsSuccessStatusCode)
        {
            var ex = new HttpRequestException
                ($"Failed http POST request for {response.RequestMessage.RequestUri}: {response.StatusCode} : {response.ReasonPhrase}");
            log.LogError(ex, ex.Message);
        }

        return response;
    }

    public virtual Task<HttpResponseMessage> PostAsync<T>(Uri url, T data, CancellationToken ct = default) 
        where T : class
    {
        return PostAsync(url.ToString(), data);
    }

    public virtual Task<HttpResponseMessage> PutAsync<T>(string url, T data, CancellationToken ct = default) 
        where T : class => PutAsync(new Uri(url), data, ct);

    public virtual async Task<HttpResponseMessage> PutAsync<T>(Uri uri, T data, CancellationToken ct = default) 
        where T : class
    {
        var serialized = JsonSerializer.Serialize(data);    
        var content = new StringContent(serialized, Encoding.UTF8, jsonMediaType);
        var response = await httpClient.PutAsync(uri, content, ct);

        if (!response.IsSuccessStatusCode)
        {
            var ex = new HttpRequestException(
                $"Failed http PUT request for {response.RequestMessage.RequestUri}: {response.StatusCode} : {response.ReasonPhrase}");
            log.LogError(ex, ex.Message);
        }

        return response;
    }

    public virtual async Task<HttpResponseMessage> DeleteAsync(string url, CancellationToken ct = default)
    {
        var request = new HttpRequestMessage(HttpMethod.Delete, url);
        var response = await httpClient.SendAsync(request, ct);

        if (!response.IsSuccessStatusCode)
        {
            var ex = new HttpRequestException($"Failed http DELETE request for {response.RequestMessage.RequestUri}: {response.StatusCode} : {response.ReasonPhrase}");
            log.LogError(ex, ex.Message);
        }

        return response;
    }

    public virtual async Task<HttpResponseMessage> PatchAsync<T>(string url, T data, CancellationToken ct = default)
        where T : class => await PatchAsync(new Uri(url), data, ct);
    
    public virtual async Task<HttpResponseMessage> PatchAsync<T>(Uri url, T data, CancellationToken ct = default) where T : class
    {
        var serialized = JsonSerializer.Serialize(data);
        var content = new StringContent(serialized, Encoding.UTF8, jsonMediaType);
        var request = new HttpRequestMessage(new HttpMethod("PATCH"), url)
        {
            Content = content
        };
        var response = await httpClient.SendAsync(request, ct);

        if (!response.IsSuccessStatusCode)
        {
            var ex = new HttpRequestException($"Failed http PATCH request for {response.RequestMessage.RequestUri}: {response.StatusCode} : {response.ReasonPhrase}");
            log.LogError(ex, ex.Message);
        }

        return response;
    }
    
    public virtual async Task<HttpResponseMessage> OptionAsync(string url, CancellationToken ct = default)
        => await OptionAsync(new Uri(url));
    
    public virtual async Task<HttpResponseMessage> OptionAsync(Uri url, CancellationToken ct = default)
    {
        var request = new HttpRequestMessage(HttpMethod.Options, url);
        var response = await httpClient.SendAsync(request, ct);

        if (!response.IsSuccessStatusCode)
        {
            var ex = new HttpRequestException($"Failed http OPTION request for {response.RequestMessage.RequestUri}: {response.StatusCode} : {response.ReasonPhrase}");
            log.LogError(ex, ex.Message);
        }
        
        return response;
    }
    
    public virtual async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct = default)
    {
        var response = await httpClient.SendAsync(request, ct);

        if (!response.IsSuccessStatusCode)
        {
            var ex = new HttpRequestException($"Failed http [{response.RequestMessage.Method}] for {response.RequestMessage.RequestUri}: {response.StatusCode} : {response.ReasonPhrase}");
            log.LogError(ex, ex.Message);
        }

        return response;
    }
    
    public virtual async Task<(T result, HttpResponseMessage response)> SendAsync<T>(HttpRequestMessage request, CancellationToken ct = default) where T : class
    {
        var response = await httpClient.SendAsync(request, ct);
        if (!response.IsSuccessStatusCode)
        {
            var ex = new HttpRequestException($"Failed http [{response.RequestMessage.Method}] for {response.RequestMessage.RequestUri}: {response.StatusCode} : {response.ReasonPhrase}");
            log.LogError(ex, ex.Message);
        }
        
        var stream = await response.Content.ReadAsStreamAsync(ct);
        var result = await DeserializeAsync<T>(stream);

        return (result, response);
    }


    protected virtual async Task<HttpResponseMessage> GetBinaryAsync(string url, CancellationToken ct = default)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Accept.Clear();
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/octet-stream"));
        
        return await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
    }


    public virtual async Task<byte[]?> DownloadBytesAsync(string url, CancellationToken ct = default)
    {
        var response = await GetBinaryAsync(url, ct);
        return await response.Content.ReadAsByteArrayAsync();
    }

    public virtual async Task<Stream?> DownloadStreamAsync(string url, CancellationToken ct = default)
    {
        var response = await GetBinaryAsync(url, ct);
        return await response.Content.ReadAsStreamAsync();
    }


    protected virtual HttpRequestMessage CreateRequest(string url, HttpMethod method) 
        => CreateRequest<object>(url, method, null);
    
    protected virtual HttpRequestMessage CreateRequest(Uri uri, HttpMethod method, CancellationToken ct = default) 
        => CreateRequest<object>(uri, method, null);

    protected virtual HttpRequestMessage CreateRequest<T>(string url, HttpMethod method, T? data, CancellationToken ct = default) 
        where T : class => CreateRequest(new Uri(url), method, data);
    
    protected virtual HttpRequestMessage CreateRequest<T>(Uri uri, HttpMethod method, T? data) 
        where T : class
    {
        if (string.IsNullOrEmpty(uri.AbsoluteUri))
            throw new ArgumentNullException(nameof(uri), "Url cannot be null or empty");
        
        if(method is null)
            throw new ArgumentNullException(nameof(method), "HttpMethod cannot be null");
        
        var request = new HttpRequestMessage(method, uri);

        if (data is null) return request;
        
        var json = JsonSerializer.Serialize(data);
        var content = new StringContent(json, Encoding.UTF8, jsonMediaType);
        request.Content = content;

        return request;
    }

    /// <summary>
    /// Determines whether the specified HTTP response indicates a legacy server failure based on its status code.
    /// </summary>
    /// <remarks>Legacy server failures are identified by specific HTTP status codes that may indicate
    /// misconfigured or outdated server behavior. This method is intended to help distinguish between standard client
    /// errors and those likely caused by legacy server issues.</remarks>
    /// <param name="response">The HTTP response message to evaluate for legacy server failure conditions. Cannot be null.</param>
    /// <returns>true if the response status code matches a known legacy server failure condition; otherwise, false.</returns>
    protected static (bool status, HttpStatusCode code) IsLegacyServerFailure(HttpResponseMessage response)
    {
        var code = response.StatusCode;
        return response.StatusCode switch
        {
            HttpStatusCode.BadRequest => (true, code),          // 400 - also could be a legitimate bad request
            HttpStatusCode.LengthRequired => (true, code),      // 411
            HttpStatusCode.NotImplemented => (true, code),      // 501
            HttpStatusCode.UnsupportedMediaType => (true, code),// 415 (some servers misreport)
            _ => (false, code)
        };
    }

    protected virtual JsonSerializerOptions GetDefaultSerializerOptions() => 
        new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            AllowTrailingCommas = true,
            PropertyNameCaseInsensitive = true,
            ReferenceHandler = ReferenceHandler.IgnoreCycles
        };
    
    protected virtual Task<T> DeserializeAsync<T>(string json, CancellationToken ct = default) where T : class
        => DeserializeAsync<T>(json, GetDefaultSerializerOptions(), ct);
    
    protected virtual async Task<T> DeserializeAsync<T>(string json, JsonSerializerOptions opts, CancellationToken ct = default) 
        where T : class
    {
        if (string.IsNullOrEmpty(json))
        {
            log.LogWarning("parameter {JsonName} was null or empty. Unable to convert to type {Name}", nameof(json), typeof(T).Name);
            return default!;
        }
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
        return await DeserializeAsync<T>(stream, opts, ct);
    }

    protected virtual async Task<T> DeserializeAsync<T>(HttpResponseMessage response, CancellationToken ct = default)
        where T : class => await DeserializeAsync<T>(response, GetDefaultSerializerOptions(), ct);
    
    protected virtual async Task<T> DeserializeAsync<T>(HttpResponseMessage response, JsonSerializerOptions opts, CancellationToken ct = default) 
        where T : class
    {
        var str = await response.Content.ReadAsStringAsync();
        return await DeserializeAsync<T>(str, opts, ct);
    }

    protected virtual async Task<T> DeserializeAsync<T>(Stream stream, CancellationToken ct = default)
        where T : class => await DeserializeAsync<T>(stream, GetDefaultSerializerOptions(), ct);
    
    protected virtual async Task<T> DeserializeAsync<T>(Stream stream, JsonSerializerOptions opts, CancellationToken ct = default) 
        where T : class
    {
        var result = await JsonSerializer.DeserializeAsync<T>(stream, opts, ct);
        return result!;
    }
}