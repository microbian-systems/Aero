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

/// <summary>
/// Represents a class for TwitterClientUserTests.
/// </summary>
public class TwitterClientUserTests
{
    private readonly HttpClient _httpClient;
    private readonly IOptions<TwitterClientOptions> _options;
    private readonly ILogger<TwitterClient> _logger;

        /// <summary>
    /// Initializes a new instance of the <see cref="TwitterClientUserTests"/> class.
    /// </summary>
public TwitterClientUserTests()
    {
        _httpClient = new HttpClient();
        _options = Options.Create(new TwitterClientOptions
        {
            BearerToken = "test_bearer_token"
        });
        _logger = Substitute.For<ILogger<TwitterClient>>();
    }

        /// <summary>
    /// GetUserByIdAsync_WithValidId_ReturnsUser method.
    /// </summary>
[Test]
    public async Task GetUserByIdAsync_WithValidId_ReturnsUser()
    {
        // Arrange
        var handler = new TestHttpMessageHandler(request =>
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(@"{
                        ""data"": {
                            ""id"": ""1234567890"",
                            ""name"": ""Test User"",
                            ""username"": ""testuser"",
                            ""created_at"": ""2020-01-01T00:00:00.000Z"",
                            ""verified"": true
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
        var user = await twitterClient.GetUserByIdAsync("1234567890");

        // Assert
        Assert.NotNull(user);
        await Assert.That(user.Id).IsEqualTo("1234567890");
        await Assert.That(user.Name).IsEqualTo("Test User");
        await Assert.That(user.Username).IsEqualTo("testuser");
        await Assert.That(user.Verified).IsTrue();
    }

        /// <summary>
    /// GetUserByIdAsync_WithFields_IncludesFieldsInRequest method.
    /// </summary>
[Test]
    public async Task GetUserByIdAsync_WithFields_IncludesFieldsInRequest()
    {
        // Arrange
        HttpRequestMessage? capturedRequest = null;
        var handler = new TestHttpMessageHandler(request =>
        {
            capturedRequest = request;
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(@"{
                        ""data"": {
                            ""id"": ""1234567890"",
                            ""name"": ""Test User"",
                            ""username"": ""testuser"",
                            ""description"": ""Test description"",
                            ""location"": ""Test Location""
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
        await twitterClient.GetUserByIdAsync("1234567890", UserFields.Description | UserFields.Location);

        // Assert
        Assert.NotNull(capturedRequest);
        await Assert.That(capturedRequest.RequestUri?.Query).Contains("user.fields");
        await Assert.That(capturedRequest.RequestUri?.Query).Contains("description");
        await Assert.That(capturedRequest.RequestUri?.Query).Contains("location");
    }

        /// <summary>
    /// GetUserByIdAsync_WithNullUserId_ThrowsArgumentException method.
    /// </summary>
[Test]
    public async Task GetUserByIdAsync_WithNullUserId_ThrowsArgumentException()
    {
        // Arrange
        var twitterClient = new TwitterClient(_httpClient, _options, _logger);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
            twitterClient.GetUserByIdAsync(null!));
        await Assert.That(exception.Message).Contains("User ID cannot be null or empty");
    }

        /// <summary>
    /// GetUserByIdAsync_WithEmptyUserId_ThrowsArgumentException method.
    /// </summary>
[Test]
    public async Task GetUserByIdAsync_WithEmptyUserId_ThrowsArgumentException()
    {
        // Arrange
        var twitterClient = new TwitterClient(_httpClient, _options, _logger);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
            twitterClient.GetUserByIdAsync(""));
        await Assert.That(exception.Message).Contains("User ID cannot be null or empty");
    }

        /// <summary>
    /// GetUserByIdAsync_WithNotFound_ThrowsTwitterApiException method.
    /// </summary>
[Test]
    public async Task GetUserByIdAsync_WithNotFound_ThrowsTwitterApiException()
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
            twitterClient.GetUserByIdAsync("nonexistent"));
        await Assert.That(exception.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
    }

        /// <summary>
    /// GetUserByUsernameAsync_WithValidUsername_ReturnsUser method.
    /// </summary>
[Test]
    public async Task GetUserByUsernameAsync_WithValidUsername_ReturnsUser()
    {
        // Arrange
        var handler = new TestHttpMessageHandler(request =>
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(@"{
                        ""data"": {
                            ""id"": ""1234567890"",
                            ""name"": ""Test User"",
                            ""username"": ""testuser"",
                            ""created_at"": ""2020-01-01T00:00:00.000Z""
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
        var user = await twitterClient.GetUserByUsernameAsync("testuser");

        // Assert
        Assert.NotNull(user);
        await Assert.That(user.Id).IsEqualTo("1234567890");
        await Assert.That(user.Username).IsEqualTo("testuser");
    }

        /// <summary>
    /// GetUserByUsernameAsync_WithAtPrefix_RemovesPrefix method.
    /// </summary>
[Test]
    public async Task GetUserByUsernameAsync_WithAtPrefix_RemovesPrefix()
    {
        // Arrange
        HttpRequestMessage? capturedRequest = null;
        var handler = new TestHttpMessageHandler(request =>
        {
            capturedRequest = request;
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(@"{
                        ""data"": {
                            ""id"": ""1234567890"",
                            ""name"": ""Test User"",
                            ""username"": ""testuser""
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
        await twitterClient.GetUserByUsernameAsync("@testuser");

        // Assert
        Assert.NotNull(capturedRequest);
        await Assert.That(capturedRequest.RequestUri?.AbsolutePath).Contains("/by/username/testuser");
        await Assert.That(capturedRequest.RequestUri?.AbsolutePath).DoesNotContain("@");
    }

        /// <summary>
    /// GetUserByUsernameAsync_WithFields_IncludesFieldsInRequest method.
    /// </summary>
[Test]
    public async Task GetUserByUsernameAsync_WithFields_IncludesFieldsInRequest()
    {
        // Arrange
        HttpRequestMessage? capturedRequest = null;
        var handler = new TestHttpMessageHandler(request =>
        {
            capturedRequest = request;
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(@"{
                        ""data"": {
                            ""id"": ""1234567890"",
                            ""name"": ""Test User"",
                            ""username"": ""testuser"",
                            ""public_metrics"": {
                                ""followers_count"": 100,
                                ""following_count"": 50
                            }
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
        await twitterClient.GetUserByUsernameAsync("testuser", UserFields.PublicMetrics);

        // Assert
        Assert.NotNull(capturedRequest);
        await Assert.That(capturedRequest.RequestUri?.Query).Contains("user.fields");
        await Assert.That(capturedRequest.RequestUri?.Query).Contains("public_metrics");
    }

        /// <summary>
    /// GetUserByUsernameAsync_WithNullUsername_ThrowsArgumentException method.
    /// </summary>
[Test]
    public async Task GetUserByUsernameAsync_WithNullUsername_ThrowsArgumentException()
    {
        // Arrange
        var twitterClient = new TwitterClient(_httpClient, _options, _logger);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
            twitterClient.GetUserByUsernameAsync(null!));
        await Assert.That(exception.Message).Contains("Username cannot be null or empty");
    }

    private class TestHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, Task<HttpResponseMessage>> _handler;

                /// <summary>
        /// Initializes a new instance of the <see cref="TestHttpMessageHandler"/> class.
        /// </summary>
public TestHttpMessageHandler(Func<HttpRequestMessage, Task<HttpResponseMessage>> handler)
        {
            _handler = handler;
        }

                /// <summary>
        /// SendAsync method.
        /// </summary>
protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return await _handler(request);
}
    }
}