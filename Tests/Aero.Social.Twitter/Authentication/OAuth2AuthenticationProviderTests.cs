using TUnit.Core;
using Aero.Social.Twitter.Client.Authentication;
using Aero.Social.Twitter.Client.Configuration;
using System.Threading.Tasks;

namespace Aero.Social.Twitter.Authentication;

/// <summary>
/// Represents a class for OAuth2AuthenticationProviderTests.
/// </summary>
public class OAuth2AuthenticationProviderTests
{
        /// <summary>
    /// Constructor_ShouldThrowOnNullOptions method.
    /// </summary>
[Test]
    public void Constructor_ShouldThrowOnNullOptions()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new OAuth2AuthenticationProvider(null!));
    }

        /// <summary>
    /// Constructor_ShouldThrowOnMissingBearerToken method.
    /// </summary>
[Test]
    public void Constructor_ShouldThrowOnMissingBearerToken()
    {
        // Arrange
        var options = new TwitterClientOptions();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => new OAuth2AuthenticationProvider(options));
    }

        /// <summary>
    /// AuthenticateRequestAsync_ShouldAddBearerToken method.
    /// </summary>
[Test]
    public async Task AuthenticateRequestAsync_ShouldAddBearerToken()
    {
        // Arrange
        var options = new TwitterClientOptions
        {
            BearerToken = "test_bearer_token_123"
        };
        var provider = new OAuth2AuthenticationProvider(options);
        var request = new HttpRequestMessage(HttpMethod.Get, "https://api.twitter.com/2/tweets/123");

        // Act
        await provider.AuthenticateRequestAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(request.Headers.Authorization);
        await Assert.That(request.Headers.Authorization.Scheme).IsEqualTo("Bearer");
        await Assert.That(request.Headers.Authorization.Parameter).IsEqualTo("test_bearer_token_123");
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
            BearerToken = "token"
        };
        var provider = new OAuth2AuthenticationProvider(options);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => provider.AuthenticateRequestAsync(null!, CancellationToken.None));
}
}