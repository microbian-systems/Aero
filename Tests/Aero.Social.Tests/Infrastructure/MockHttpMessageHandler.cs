using System.Net;
using System.Text;
using System.Text.Json;

namespace Aero.Social.Tests.Infrastructure;

/// <summary>
/// Represents a class for MockHttpMessageHandler.
/// </summary>
public class MockHttpMessageHandler : HttpMessageHandler
{
    private readonly List<MockedRequest> _mockedRequests = new();
    private readonly List<HttpRequestMessage> _receivedRequests = new();

        /// <summary>
    /// Gets or sets the Received Requests.
    /// </summary>
public IReadOnlyList<HttpRequestMessage> ReceivedRequests => _receivedRequests;

        /// <summary>
    /// When method.
    /// </summary>
public MockHttpMessageHandler When(Func<HttpRequestMessage, bool> predicate)
    {
        _mockedRequests.Add(new MockedRequest { Predicate = predicate });
        return this;
    }

        /// <summary>
    /// WhenGet method.
    /// </summary>
public MockHttpMessageHandler WhenGet(string urlPattern)
    {
        return When(req => req.Method == HttpMethod.Get && MatchUrl(req, urlPattern));
    }

        /// <summary>
    /// WhenPost method.
    /// </summary>
public MockHttpMessageHandler WhenPost(string urlPattern)
    {
        return When(req => req.Method == HttpMethod.Post && MatchUrl(req, urlPattern));
    }

        /// <summary>
    /// RespondWith method.
    /// </summary>
public MockedRequest RespondWith(string content, HttpStatusCode statusCode = HttpStatusCode.OK, string contentType = "application/json")
    {
        var mock = _mockedRequests.Last();
        mock.ResponseContent = content;
        mock.StatusCode = statusCode;
        mock.ContentType = contentType;
        return mock;
    }

        /// <summary>
    /// RespondWith method.
    /// </summary>
public MockedRequest RespondWith(Func<HttpRequestMessage, HttpResponseMessage> responseFactory)
    {
        var mock = _mockedRequests.Last();
        mock.ResponseFactory = responseFactory;
        return mock;
    }

        /// <summary>
    /// RespondWithJson method.
    /// </summary>
public MockedRequest RespondWithJson<T>(T data, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        var json = JsonSerializer.Serialize(data, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        return RespondWith(json, statusCode);
    }

        /// <summary>
    /// RespondWithStatusCode method.
    /// </summary>
public MockedRequest RespondWithStatusCode(HttpStatusCode statusCode)
    {
        var mock = _mockedRequests.Last();
        mock.StatusCode = statusCode;
        return mock;
    }

    private static bool MatchUrl(HttpRequestMessage request, string pattern)
    {
        if (request.RequestUri == null) return false;
        var url = request.RequestUri.ToString();
        if (pattern.Contains('*'))
        {
            var regex = "^" + System.Text.RegularExpressions.Regex.Escape(pattern).Replace("\\*", ".*") + "$";
            return System.Text.RegularExpressions.Regex.IsMatch(url, regex, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        }
        return url.Contains(pattern, StringComparison.OrdinalIgnoreCase);
    }

        /// <summary>
    /// SendAsync method.
    /// </summary>
protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        _receivedRequests.Add(request);

        foreach (var mock in _mockedRequests)
        {
            if (mock.Predicate(request))
            {
                await Task.Delay(mock.DelayMs, cancellationToken);

                if (mock.ThrowException != null)
                {
                    throw mock.ThrowException;
                }

                if (mock.ResponseFactory != null)
                {
                    return mock.ResponseFactory(request);
                }

                var response = new HttpResponseMessage(mock.StatusCode)
                {
                    Content = mock.ResponseContent != null
                        ? new StringContent(mock.ResponseContent, Encoding.UTF8, mock.ContentType)
                        : null
                };

                return response;
            }
        }

        return new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent($"{{\"error\": \"No mock configured for {request.Method} {request.RequestUri}\"}}")
        };
    }

        /// <summary>
    /// Reset method.
    /// </summary>
public void Reset()
    {
        _mockedRequests.Clear();
        _receivedRequests.Clear();
    }
}

/// <summary>
/// Represents a class for MockedRequest.
/// </summary>
public class MockedRequest
{
        /// <summary>
    /// Gets or sets the Predicate.
    /// </summary>
public Func<HttpRequestMessage, bool> Predicate { get; set; } = _ => false;
        /// <summary>
    /// Gets or sets the Response Content.
    /// </summary>
public string? ResponseContent { get; set; }
        /// <summary>
    /// Gets or sets the Status Code.
    /// </summary>
public HttpStatusCode StatusCode { get; set; } = HttpStatusCode.OK;
        /// <summary>
    /// Gets or sets the Content Type.
    /// </summary>
public string ContentType { get; set; } = "application/json";
        /// <summary>
    /// Gets or sets the Delay Ms.
    /// </summary>
public int DelayMs { get; set; }
        /// <summary>
    /// Gets or sets the Throw Exception.
    /// </summary>
public Exception? ThrowException { get; set; }
        /// <summary>
    /// Gets or sets the Response Factory.
    /// </summary>
public Func<HttpRequestMessage, HttpResponseMessage>? ResponseFactory { get; set; }

        /// <summary>
    /// WithDelay method.
    /// </summary>
public MockedRequest WithDelay(int milliseconds)
    {
        DelayMs = milliseconds;
        return this;
    }

        /// <summary>
    /// Throw method.
    /// </summary>
public MockedRequest Throw(Exception exception)
    {
        ThrowException = exception;
        return this;
    }
}
