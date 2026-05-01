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

public class TwitterClientTimelineTests
{
    private readonly IOptions<TwitterClientOptions> _options;
    private readonly ILogger<TwitterClient> _logger;

    public TwitterClientTimelineTests()
    {
        _options = Options.Create(new TwitterClientOptions
        {
            BearerToken = "test_bearer_token"
        });
        _logger = Substitute.For<ILogger<TwitterClient>>();
    }

    //#region GetUserTweetsAsync Tests

    [Test]
    public async Task GetUserTweetsAsync_WithValidUserId_ReturnsTweets()
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
                                ""text"": ""My first tweet"",
                                ""created_at"": ""2020-01-01T00:00:00.000Z"",
                                ""author_id"": ""9876543210""
                            },
                            {
                                ""id"": ""1234567891"",
                                ""text"": ""My second tweet"",
                                ""created_at"": ""2020-01-02T00:00:00.000Z"",
                                ""author_id"": ""9876543210""
                            }
                        ],
                        ""meta"": {
                            ""result_count"": 2,
                            ""next_token"": ""next_page_token"",
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
        var result = await twitterClient.GetUserTweetsAsync("9876543210");

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Data);
        await Assert.That(result.Data.Count).IsEqualTo(2);
        await Assert.That(result.Data[0].Id).IsEqualTo("1234567890");
        await Assert.That(result.Data[1].Id).IsEqualTo("1234567891");
        Assert.NotNull(result.Meta);
        await Assert.That(result.Meta.NextToken).IsEqualTo("next_page_token");
    }

    [Test]
    public async Task GetUserTweetsAsync_WithOptions_IncludesAllParameters()
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

        var options = new TimelineOptions
        {
            MaxResults = 50,
            SinceId = "1000000000",
            UntilId = "9999999999",
            StartTime = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero),
            EndTime = new DateTimeOffset(2024, 12, 31, 23, 59, 59, TimeSpan.Zero),
            PaginationToken = "b26v89c19zqg8o3f",
            Exclude = "retweets,replies",
            TweetFields = TweetFields.PublicMetrics | TweetFields.CreatedAt,
            Expansions = ExpansionOptions.AuthorId
        };

        // Act
        await twitterClient.GetUserTweetsAsync("9876543210", options);

        // Assert
        Assert.NotNull(capturedRequest);
        var query = capturedRequest.RequestUri?.Query;
        await Assert.That(query).Contains("max_results=50");
        await Assert.That(query).Contains("since_id=1000000000");
        await Assert.That(query).Contains("until_id=9999999999");
        await Assert.That(query).Contains("start_time=");
        await Assert.That(query).Contains("end_time=");
        await Assert.That(query).Contains("pagination_token=b26v89c19zqg8o3f");
        await Assert.That(query).Contains("exclude=retweets%2Creplies");
        await Assert.That(query).Contains("tweet.fields=");
        await Assert.That(query).Contains("expansions=author_id");
    }

    [Test]
    public async Task GetUserTweetsAsync_WithNullUserId_ThrowsArgumentException()
    {
        // Arrange
        var httpClient = new HttpClient();
        var twitterClient = new TwitterClient(httpClient, _options, _logger);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            twitterClient.GetUserTweetsAsync(null!));
        await Assert.That(exception.Message).Contains("User ID cannot be null or empty");
    }

    [Test]
    public async Task GetUserTweetsAsync_WithInvalidMaxResults_ThrowsArgumentException()
    {
        // Arrange
        var httpClient = new HttpClient();
        var twitterClient = new TwitterClient(httpClient, _options, _logger);

        var options = new TimelineOptions
        {
            MaxResults = 3 // Invalid: must be >= 5
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            twitterClient.GetUserTweetsAsync("9876543210", options));
        await Assert.That(exception.Message).Contains("MaxResults must be between 5 and 100");
    }

    [Test]
    public async Task GetUserTweetsAsync_WithNotFound_ThrowsTwitterApiException()
    {
        // Arrange
        var handler = new TestHttpMessageHandler(request =>
        {
            var response = new HttpResponseMessage(HttpStatusCode.NotFound)
            {
                Content = new StringContent(@"{
                        ""errors"": [{
                            ""message"": ""User not found"",
                            ""code"": 50
                        }]
                    }")
            };
            return Task.FromResult(response);
        });

        var client = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://api.twitter.com")
        };

        var twitterClient = new TwitterClient(client, _options, _logger);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<TwitterApiException>(() =>
            twitterClient.GetUserTweetsAsync("nonexistent"));
        await Assert.That(exception.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
    }

    //#endregion

    //#region GetUserMentionsAsync Tests

    [Test]
    public async Task GetUserMentionsAsync_WithValidUserId_ReturnsMentions()
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
                                ""text"": ""@testuser Hello!"",
                                ""created_at"": ""2020-01-01T00:00:00.000Z"",
                                ""author_id"": ""1111111111""
                            },
                            {
                                ""id"": ""1234567891"",
                                ""text"": ""@testuser How are you?"",
                                ""created_at"": ""2020-01-02T00:00:00.000Z"",
                                ""author_id"": ""2222222222""
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
        var result = await twitterClient.GetUserMentionsAsync("9876543210");

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Data);
        await Assert.That(result.Data.Count).IsEqualTo(2);
        await Assert.That(result.Data[0].Text).Contains("@testuser");
        await Assert.That(result.Data[1].Text).Contains("@testuser");
    }

    [Test]
    public async Task GetUserMentionsAsync_WithOptions_IncludesParameters()
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

        var options = new TimelineOptions
        {
            MaxResults = 25,
            PaginationToken = "test_token",
            TweetFields = TweetFields.PublicMetrics
        };

        // Act
        await twitterClient.GetUserMentionsAsync("9876543210", options);

        // Assert
        Assert.NotNull(capturedRequest);
        var query = capturedRequest.RequestUri?.Query;
        await Assert.That(query).Contains("max_results=25");
        await Assert.That(query).Contains("pagination_token=test_token");
        await Assert.That(query).Contains("tweet.fields=public_metrics");
    }

    [Test]
    public async Task GetUserMentionsAsync_WithNullUserId_ThrowsArgumentException()
    {
        // Arrange
        var httpClient = new HttpClient();
        var twitterClient = new TwitterClient(httpClient, _options, _logger);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            twitterClient.GetUserMentionsAsync(null!));
        await Assert.That(exception.Message).Contains("User ID cannot be null or empty");
    }

    [Test]
    public async Task GetUserMentionsAsync_WithEmptyResult_ReturnsEmptyList()
    {
        // Arrange
        var handler = new TestHttpMessageHandler(request =>
        {
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
        var result = await twitterClient.GetUserMentionsAsync("9876543210");

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Data);
        await Assert.That(result.Data).IsEmpty();
        await Assert.That(result.Meta?.ResultCount).IsEqualTo(0);
    }

    //#endregion

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