using TUnit.Core;
using Aero.Social.Twitter.Client.Authentication;
using Aero.Social.Twitter.Client.Configuration;
using System.Threading.Tasks;

namespace Aero.Social.Twitter.Authentication;

/// <summary>
/// Represents a class for OAuth1AuthenticationProviderTests.
/// </summary>
public class OAuth1AuthenticationProviderTests
{
        /// <summary>
    /// Constructor_ShouldThrowOnNullOptions method.
    /// </summary>
[Test]
    public void Constructor_ShouldThrowOnNullOptions()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new OAuth1AuthenticationProvider(null!));
    }

        /// <summary>
    /// Constructor_ShouldThrowOnMissingConsumerKey method.
    /// </summary>
[Test]
    public void Constructor_ShouldThrowOnMissingConsumerKey()
    {
        // Arrange
        var options = new TwitterClientOptions
        {
            ConsumerSecret = "secret",
            AccessToken = "token",
            AccessTokenSecret = "token_secret"
        };

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => new OAuth1AuthenticationProvider(options));
    }

        /// <summary>
    /// Constructor_ShouldThrowOnMissingConsumerSecret method.
    /// </summary>
[Test]
    public void Constructor_ShouldThrowOnMissingConsumerSecret()
    {
        // Arrange
        var options = new TwitterClientOptions
        {
            ConsumerKey = "key",
            AccessToken = "token",
            AccessTokenSecret = "token_secret"
        };

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => new OAuth1AuthenticationProvider(options));
    }

        /// <summary>
    /// Constructor_ShouldThrowOnMissingAccessToken method.
    /// </summary>
[Test]
    public void Constructor_ShouldThrowOnMissingAccessToken()
    {
        // Arrange
        var options = new TwitterClientOptions
        {
            ConsumerKey = "key",
            ConsumerSecret = "secret",
            AccessTokenSecret = "token_secret"
        };

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => new OAuth1AuthenticationProvider(options));
    }

        /// <summary>
    /// Constructor_ShouldThrowOnMissingAccessTokenSecret method.
    /// </summary>
[Test]
    public void Constructor_ShouldThrowOnMissingAccessTokenSecret()
    {
        // Arrange
        var options = new TwitterClientOptions
        {
            ConsumerKey = "key",
            ConsumerSecret = "secret",
            AccessToken = "token"
        };

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => new OAuth1AuthenticationProvider(options));
    }

        /// <summary>
    /// AuthenticateRequestAsync_ShouldAddAuthorizationHeader method.
    /// </summary>
[Test]
    public async Task AuthenticateRequestAsync_ShouldAddAuthorizationHeader()
    {
        // Arrange
        var options = new TwitterClientOptions
        {
            ConsumerKey = "test_consumer_key",
            ConsumerSecret = "test_consumer_secret",
            AccessToken = "test_access_token",
            AccessTokenSecret = "test_access_token_secret"
        };
        var provider = new OAuth1AuthenticationProvider(options);
        var request = new HttpRequestMessage(HttpMethod.Get, "https://api.twitter.com/2/tweets/123");

        // Act
        await provider.AuthenticateRequestAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(request.Headers.Authorization);
        await Assert.That(request.Headers.Authorization.Scheme).IsEqualTo("OAuth");
        Assert.NotNull(request.Headers.Authorization.Parameter);
        await Assert.That(request.Headers.Authorization.Parameter).Contains("oauth_consumer_key=\"test_consumer_key\"");
        await Assert.That(request.Headers.Authorization.Parameter).Contains("oauth_token=\"test_access_token\"");
        await Assert.That(request.Headers.Authorization.Parameter).Contains("oauth_signature_method=\"HMAC-SHA1\"");
        await Assert.That(request.Headers.Authorization.Parameter).Contains("oauth_version=\"1.0\"");
    }

        /// <summary>
    /// AuthenticateRequestAsync_ShouldThrowOnNullRequest method.
    /// </summary>
[Test]
    public async Task AuthenticateRequestAsync_ShouldThrowOnNullRequest()
    {
        // Arrange
        var options = new TwitterClientOptions
        {
            ConsumerKey = "key",
            ConsumerSecret = "secret",
            AccessToken = "token",
            AccessTokenSecret = "token_secret"
        };
        var provider = new OAuth1AuthenticationProvider(options);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => provider.AuthenticateRequestAsync(null!, CancellationToken.None));
}
}