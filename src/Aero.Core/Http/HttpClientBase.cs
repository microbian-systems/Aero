using Aero.Core.Railway;
using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.Mime;

namespace Aero.Core.Http;

public abstract class HttpClientBase(HttpClient client, ILogger<HttpClientBase> log)
{
    protected readonly ILogger<HttpClientBase> log = log;
    protected readonly HttpClient client = client;
    protected readonly string jsonMediaType = MediaTypeNames.Application.Json;

    protected virtual Uri CreateUri(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new ArgumentException("Url cannot be null or empty", nameof(url));

        if (Uri.TryCreate(url, UriKind.Absolute, out var absoluteUri))
            return absoluteUri;

        if (client.BaseAddress is not null)
            return new Uri(client.BaseAddress, url);

        if (Uri.TryCreate(url, UriKind.Relative, out var relativeUri))
            return relativeUri;

        throw new UriFormatException($"Invalid URI: '{url}'");
    }

    protected static string FormatUriForLog(Uri uri)
        => uri.IsAbsoluteUri ? uri.AbsoluteUri : uri.ToString();

    public virtual async Task<Result<HttpResponseMessage, AeroError>> GetAsync(string url, CancellationToken ct = default)
        => await GetAsync(CreateUri(url), ct);

    public virtual async Task<Result<HttpResponseMessage, AeroError>> GetAsync(Uri uri, CancellationToken ct = default)
    {
        var response = await SendRequestAsync(() => client.GetAsync(uri, ct), uri, ct);

        return response;
    }

    public virtual async Task<Result<HttpResponseMessage, AeroError>> PostAsync<T>(Uri uri, T data, CancellationToken ct = default)
        where T : class
    {
        var response = await SendRequestAsync(() => client.PostAsJsonAsync(uri, data, ct), uri, ct);

        return response;
    }

    public virtual Task<Result<HttpResponseMessage, AeroError>> PostAsync<T>(string url, T data, CancellationToken ct = default)
        where T : class
        => PostAsync(CreateUri(url), data, ct);

    public virtual Task<Result<HttpResponseMessage, AeroError>> PutAsync<T>(Uri uri, T data, CancellationToken ct = default)
        => SendRequestAsync(() => client.PutAsJsonAsync(uri, data, ct), uri, ct);

    public virtual Task<Result<HttpResponseMessage, AeroError>> DeleteAsync(string url, CancellationToken ct = default)
        => DeleteAsync(CreateUri(url), ct);

    public virtual async Task<Result<HttpResponseMessage, AeroError>> DeleteAsync(Uri uri, CancellationToken ct = default)
    {
        var request = new HttpRequestMessage(HttpMethod.Delete, uri);
        var response = await SendRequestAsync(() => client.SendAsync(request, ct), uri, ct);

        return response;
    }

    public virtual Task<Result<HttpResponseMessage, AeroError>> PatchAsync<T>(string url, T data, CancellationToken ct = default)
        where T : class 
        => PatchAsync(CreateUri(url), data, ct);

    public virtual async Task<Result<HttpResponseMessage, AeroError>> PatchAsync<T>(Uri uri, T data, CancellationToken ct = default) where T : class
    {
        var response = await SendRequestAsync(() => client.PatchAsJsonAsync(uri, data, ct), uri, ct);

        return response;
    }

    public virtual async Task<Result<HttpResponseMessage, AeroError>> OptionAsync(string url, CancellationToken ct = default)
        => await OptionAsync(CreateUri(url), ct);

    public virtual async Task<Result<HttpResponseMessage, AeroError>> OptionAsync(Uri url, CancellationToken ct = default)
    {

        var request = new HttpRequestMessage(HttpMethod.Options, url);
        var response = await SendRequestAsync(() => client.SendAsync(request, ct), url, ct);

        return response;
    }



    /// <summary>
    /// Sends a GET request and returns the result as a type-safe <see cref="Result{T, AeroError}"/>.
    /// </summary>
    protected virtual async Task<Result<T, AeroError>> GetAsync<T>(string url, CancellationToken ct = default)
        where T : class => await SendRequestAsync<T>(CreateRequest(url, HttpMethod.Get), ct);

    /// <summary>
    /// Sends a POST request and returns the result as a type-safe <see cref="Result{TResponse, AeroError}"/>.
    /// </summary>
    protected virtual async Task<Result<TResponse, AeroError>> PostAsync<TRequest, TResponse>(string url, TRequest data, CancellationToken ct = default)
        where TRequest : class
        where TResponse : class => await SendRequestAsync<TResponse>(CreateRequest(url, HttpMethod.Post, data), ct);

    /// <summary>
    /// Sends a POST request with an object payload and returns the result as a type-safe <see cref="Result{T, AeroError}"/>.
    /// </summary>
    protected virtual async Task<Result<T, AeroError>> PostAsync<T>(string url, object data, CancellationToken ct = default)
        where T : class => await SendRequestAsync<T>(CreateRequest(url, HttpMethod.Post, data), ct);

    /// <summary>
    /// Sends a PUT request and returns the result as a type-safe <see cref="Result{TResponse, AeroError}"/>.
    /// </summary>
    protected virtual async Task<Result<TResponse, AeroError>> PutAsync<TRequest, TResponse>(string url, TRequest data, CancellationToken ct = default)
        where TRequest : class
        where TResponse : class => await SendRequestAsync<TResponse>(CreateRequest(url, HttpMethod.Put, data), ct);

        protected virtual async Task<Result<HttpResponseMessage, AeroError>> SendRequestAsync(HttpRequestMessage request, CancellationToken ct = default)
        => await SendRequestAsync(() => client.SendAsync(request, ct), request.RequestUri!, ct);

    /// <summary>
    /// Sends an HTTP request and returns the result as a type-safe <see cref="Result{T, AeroError}"/>.
    /// </summary>
    /// <typeparam name="T">The type to deserialize the response body into.</typeparam>
    /// <param name="request">The HTTP request message to send.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A result containing the deserialized object if successful; otherwise, an error.</returns>
    protected virtual async Task<Result<T, AeroError>> SendRequestAsync<T>(HttpRequestMessage request, CancellationToken ct = default)
        where T : class
    {
        var result = await SendRequestAsync(request, ct);
        return await result.BindAsync(async response =>
        {
            try
            {
                var data = await DeserializeAsync<T>(response, ct);
                return data is null
                    ? AeroError.HttpRequestError(response.StatusCode, "Deserialization returned null")
                    : new Result<T, AeroError>.Ok(data);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "there was an http request exception");
                return (Result<T, AeroError>)AeroError.HttpRequestError(response.StatusCode, $"Deserialization failed: {ex.Message}");
            }
        });
    }

    protected async Task<Result<HttpResponseMessage, AeroError>> SendRequestAsync(
        Func<Task<HttpResponseMessage>> request, 
        Uri uri, CancellationToken ct)
    {
        try
        {
            var response = await request();

            if (response.IsSuccessStatusCode)
                return response;

            var errorMsg = await response.Content.ReadAsStringAsync(ct);
            log.LogError("Failed request for {Uri}: {StatusCode}", uri, response.StatusCode);
            return AeroError.HttpRequestError(response.StatusCode, errorMsg
                ?? response.ReasonPhrase
                ?? "unknown httprequest error");
        }
        catch (Exception ex) when (ex is TaskCanceledException || ex is OperationCanceledException)
        {
            log.LogError(ex, "there was an http request exception");

            return ct.IsCancellationRequested
                ? AeroError.CancelledError("Canceled")
                : AeroError.TimeoutError("Timed out");
        }
        catch (HttpRequestException ex)
        {
            log.LogError(ex, "there was an http request exception");

            return AeroError.HttpRequestError(HttpStatusCode.ServiceUnavailable, ex.Message);
        }
        catch (Exception ex)
        {
            var url = FormatUriForLog(uri);
            log.LogError(ex, "Exception during GET {Uri}", url);
            return AeroError.CreateError($"Exception during GET {url}: {ex.Message}");
        }
    }


    protected virtual Result<byte[], AeroError> GetBytes(string url, CancellationToken ct = default)
        => GetBytes(CreateUri(url), ct);

    protected virtual Result<byte[], AeroError> GetBytes(Uri uri, CancellationToken ct = default)
        => GetBytesAsync(uri, ct).GetAwaiter().GetResult();

    protected virtual async Task<Result<byte[], AeroError>> GetBytesAsync(Uri uri, CancellationToken ct = default)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, uri);
        request.Headers.Accept.Clear();
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(MediaTypeNames.Application.Octet));

        var response = await SendRequestAsync(() => 
            client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct), uri, ct);

        return response switch
        {
            // If successful, await the byte array and return it (wrapped implicitly via your operator)
            Result<HttpResponseMessage, AeroError>.Ok(var resp) => await resp.Content.ReadAsByteArrayAsync(ct),

            // If failure, return the error (wrapped implicitly via your operator)
            Result<HttpResponseMessage, AeroError>.Failure(var err) => err,

            _ => throw new UnreachableException()
        };
    }

    protected virtual Result<Stream, AeroError> GetStream(string url, CancellationToken ct = default)
        => GetStream(CreateUri(url), ct);

    protected virtual Result<Stream, AeroError> GetStream(Uri uri, CancellationToken ct = default)
        => GetStreamAsync(uri, ct).GetAwaiter().GetResult();

    protected virtual async Task<Result<Stream, AeroError>> GetStreamAsync(Uri uri, CancellationToken ct = default)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, uri);
        request.Headers.Accept.Clear();
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(MediaTypeNames.Application.Octet));

        var response = await SendRequestAsync(() =>
            client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct), uri, ct);

        return response switch
        {
            // If successful, await the byte array and return it (wrapped implicitly via your operator)
            Result<HttpResponseMessage, AeroError>.Ok(var resp) => await resp.Content.ReadAsStreamAsync(ct),

            // If failure, return the error (wrapped implicitly via your operator)
            Result<HttpResponseMessage, AeroError>.Failure(var err) => err,

            _ => throw new UnreachableException()
        };
    }


    public virtual async Task<Result<byte[]?, AeroError>> DownloadBytesAsync(Uri uri, CancellationToken ct = default)
    {
        return await GetBytesAsync(uri, ct) switch
        {
            // If successful, await the byte array and return it (wrapped implicitly via your operator)
            Result<byte[], AeroError>.Ok(var resp) => resp,

            // If failure, return the error (wrapped implicitly via your operator)
            Result<byte[], AeroError>.Failure(var err) => err,

            _ => throw new UnreachableException()
        };
    }

    public virtual async Task<Result<Stream, AeroError>> DownloadStreamAsync(Uri uri, CancellationToken ct = default)
    {
        return await GetStreamAsync(uri, ct) switch
        {
            // If successful, await the byte array and return it (wrapped implicitly via your operator)
            Result<Stream, AeroError>.Ok(var resp) => resp,

            // If failure, return the error (wrapped implicitly via your operator)
            Result<Stream, AeroError>.Failure(var err) => err,

            _ => throw new UnreachableException()
        };
    }


    protected virtual HttpRequestMessage CreateRequest(string url, HttpMethod method)
        => CreateRequest<object>(url, method, null);

    protected virtual HttpRequestMessage CreateRequest(Uri uri, HttpMethod method, CancellationToken ct = default)
        => CreateRequest<object>(uri, method, null);

    protected virtual HttpRequestMessage CreateRequest<T>(string url, HttpMethod method, T? data, CancellationToken ct = default)
        where T : class => CreateRequest(CreateUri(url), method, data);

    protected virtual HttpRequestMessage CreateRequest<T>(Uri uri, HttpMethod method, T? data)
        where T : class
    {
        if (uri is null)
            throw new ArgumentNullException(nameof(uri), "Url cannot be null or empty");

        if (method is null)
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
