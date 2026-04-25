using TUnit.Core;
using System.Net;
using Aero.Social.Twitter.Client.Clients;
using Aero.Social.Twitter.Client.Configuration;
using Aero.Social.Twitter.Client.Exceptions;
using Aero.Social.Twitter.Client.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using System.Threading.Tasks;

namespace Aero.Social.Twitter.Clients;

public class TwitterClientSearchTests
{
    private readonly IOptions<TwitterClientOptions> _options;
    private readonly ILogger<TwitterClient> _logger;

    public TwitterClientSearchTests()
    {
        _options = Options.Create(new TwitterClientOptions
        {
            BearerToken = "test_bearer_token"
        });
        _logger = Substitute.For<ILogger<TwitterClient>>();
    }

    [Test]
    public async Task SearchTweetsAsync_WithValidQuery_ReturnsTweets()
    {
        // Arrange
        var handler = new TestHttpMessageHandler(request =>
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(@"{
                        ""data"": [
                            {
                                ""id"": ""1234567890"",
                                ""text"": ""Test tweet about dotnet"",
                                ""created_at"": ""2020-01-01T00:00:00.000Z"",
                                ""author_id"": ""9876543210""
                            },
                            {
                                ""id"": ""1234567891"",
                                ""text"": ""Another tweet"",
                                ""created_at"": ""2020-01-02T00:00:00.000Z"",
                                ""author_id"": ""9876543211""
                            }
                        ],
                        ""meta"": {
                            ""result_count"": 2,
                            ""newest_id"": ""1234567891"",
                            ""oldest_id"": ""1234567890""
                        }
                    }")
            };
            return Task.FromResult(response);
        });

        var client = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://api.twitter.com")
        };

        var twitterClient = new TwitterClient(client, _options, _logger);

        // Act
        var result = await twitterClient.SearchTweetsAsync("dotnet");

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Data);
        await Assert.That(result.Data.Count).IsEqualTo(2);
        await Assert.That(result.Data[0].Id).IsEqualTo("1234567890");
        await Assert.That(result.Data[1].Id).IsEqualTo("1234567891");
        Assert.NotNull(result.Meta);
        await Assert.That(result.Meta.ResultCount).IsEqualTo(2);
    }

    [Test]
    public async Task SearchTweetsAsync_WithOptions_IncludesAllParameters()
    {
        // Arrange
        HttpRequestMessage? capturedRequest = null;
        var handler = new TestHttpMessageHandler(request =>
        {
            capturedRequest = request;
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(@"{
                        ""data"": [],
                        ""meta"": {
                            ""result_count"": 0
                        }
                    }")
            };
            return Task.FromResult(response);
        });

        var client = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://api.twitter.com")
        };

        var twitterClient = new TwitterClient(client, _options, _logger);

        var options = new SearchOptions
        {
            MaxResults = 50,
            SinceId = "1000000000",
            UntilId = "9999999999",
            StartTime = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero),
            EndTime = new DateTimeOffset(2024, 12, 31, 23, 59, 59, TimeSpan.Zero),
            TweetFields = TweetFields.PublicMetrics | TweetFields.CreatedAt,
            Expansions = ExpansionOptions.AuthorId
        };

        // Act
        await twitterClient.SearchTweetsAsync("test query", options);

        // Assert
        Assert.NotNull(capturedRequest);
        var query = capturedRequest.RequestUri?.Query;
        await Assert.That(query).Contains("query=test%20query");
        await Assert.That(query).Contains("max_results=50");
        await Assert.That(query).Contains("since_id=1000000000");
        await Assert.That(query).Contains("until_id=9999999999");
        await Assert.That(query).Contains("start_time=");
        await Assert.That(query).Contains("end_time=");
        await Assert.That(query).Contains("tweet.fields=");
        await Assert.That(query).Contains("expansions=author_id");
    }

    [Test]
    public async Task SearchTweetsAsync_WithNextToken_IncludesPaginationToken()
    {
        // Arrange
        HttpRequestMessage? capturedRequest = null;
        var handler = new TestHttpMessageHandler(request =>
        {
            capturedRequest = request;
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(@"{
                        ""data"": [],
                        ""meta"": {
                            ""result_count"": 0
                        }
                    }")
            };
            return Task.FromResult(response);
        });

        var client = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://api.twitter.com")
        };

        var twitterClient = new TwitterClient(client, _options, _logger);

        var options = new SearchOptions
        {
            NextToken = "b26v89c19zqg8o3f"
        };

        // Act
        await twitterClient.SearchTweetsAsync("test", options);

        // Assert
        Assert.NotNull(capturedRequest);
        await Assert.That(capturedRequest.RequestUri?.Query).Contains("next_token=b26v89c19zqg8o3f");
    }

    [Test]
    public async Task SearchTweetsAsync_WithNullQuery_ThrowsArgumentException()
    {
        // Arrange
        var httpClient = new HttpClient();
        var twitterClient = new TwitterClient(httpClient, _options, _logger);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            twitterClient.SearchTweetsAsync(null!));
        await Assert.That(exception.Message).Contains("Search query cannot be null or empty");
    }

    [Test]
    public async Task SearchTweetsAsync_WithEmptyQuery_ThrowsArgumentException()
    {
        // Arrange
        var httpClient = new HttpClient();
        var twitterClient = new TwitterClient(httpClient, _options, _logger);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            twitterClient.SearchTweetsAsync(""));
        await Assert.That(exception.Message).Contains("Search query cannot be null or empty");
    }

    [Test]
    public async Task SearchTweetsAsync_WithWhitespaceQuery_ThrowsArgumentException()
    {
        // Arrange
        var httpClient = new HttpClient();
        var twitterClient = new TwitterClient(httpClient, _options, _logger);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            twitterClient.SearchTweetsAsync("   "));
        await Assert.That(exception.Message).Contains("Search query cannot be null or empty");
    }

    [Test]
    public async Task SearchTweetsAsync_WithInvalidMaxResults_ThrowsArgumentException()
    {
        // Arrange
        var httpClient = new HttpClient();
        var twitterClient = new TwitterClient(httpClient, _options, _logger);

        var options = new SearchOptions
        {
            MaxResults = 5 // Invalid: must be >= 10
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            twitterClient.SearchTweetsAsync("test", options));
        await Assert.That(exception.Message).Contains("MaxResults must be between 10 and 100");
    }

    [Test]
    public async Task SearchTweetsAsync_WithInvalidTimeRange_ThrowsArgumentException()
    {
        // Arrange
        var httpClient = new HttpClient();
        var twitterClient = new TwitterClient(httpClient, _options, _logger);

        var options = new SearchOptions
        {
            StartTime = new DateTimeOffset(2024, 12, 31, 0, 0, 0, TimeSpan.Zero),
            EndTime = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero)
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            twitterClient.SearchTweetsAsync("test", options));
        await Assert.That(exception.Message).Contains("StartTime cannot be greater than EndTime");
    }

    [Test]
    public async Task SearchTweetsAsync_WithRateLimit_ThrowsTwitterRateLimitException()
    {
        // Arrange
        var handler = new TestHttpMessageHandler(request =>
        {
            var response = new HttpResponseMessage((HttpStatusCode)429)
            {
                Content = new StringContent(@"{
                        ""errors"": [{
                            ""message"": ""Rate limit exceeded"",
                            ""code"": 88
                        }]
                    }"),
                Headers = { { "Retry-After", "900" } }
            };
            return Task.FromResult(response);
        });

        var client = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://api.twitter.com")
        };

        var twitterClient = new TwitterClient(client, _options, _logger);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<TwitterRateLimitException>(() =>
            twitterClient.SearchTweetsAsync("test"));
        Assert.NotNull(exception.RetryAfter);
        await Assert.That(exception.RetryAfter).IsEqualTo(TimeSpan.FromSeconds(900));
    }

    [Test]
    public async Task SearchTweetsAsync_QueryEncoding_EncodesSpecialCharacters()
    {
        // Arrange
        HttpRequestMessage? capturedRequest = null;
        var handler = new TestHttpMessageHandler(request =>
        {
            capturedRequest = request;
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(@"{
                        ""data"": [],
                        ""meta"": {
                            ""result_count"": 0
                        }
                    }")
            };
            return Task.FromResult(response);
        });

        var client = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://api.twitter.com")
        };

        var twitterClient = new TwitterClient(client, _options, _logger);

        // Act
        await twitterClient.SearchTweetsAsync("test #hashtag @user");

        // Assert
        Assert.NotNull(capturedRequest);
        var query = capturedRequest.RequestUri?.Query;
        await Assert.That(query).Contains("query=test%20%23hashtag%20%40user");
    }

    private class TestHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, Task<HttpResponseMessage>> _handler;

        public TestHttpMessageHandler(Func<HttpRequestMessage, Task<HttpResponseMessage>> handler)
        {
            _handler = handler;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return await _handler(request);
}
    }
}