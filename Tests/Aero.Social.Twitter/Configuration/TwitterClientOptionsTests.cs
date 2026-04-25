using TUnit.Core;
using Aero.Social.Twitter.Client.Configuration;
using System.Threading.Tasks;

namespace Aero.Social.Twitter.Configuration;

public class TwitterClientOptionsTests
{
    [Test]
    public async Task TwitterClientOptions_ShouldHaveDefaultTimeout()
    {
        // Arrange & Act
        var options = new TwitterClientOptions();

        // Assert
        await Assert.That(options.Timeout).IsEqualTo(TimeSpan.FromSeconds(30));
    }

    [Test]
    public async Task TwitterClientOptions_ShouldHaveDefaultMaxRetries()
    {
        // Arrange & Act
        var options = new TwitterClientOptions();

        // Assert
        await Assert.That(options.MaxRetries).IsEqualTo(3);
    }

    [Test]
    public async Task TwitterClientOptions_ShouldAllowSettingCredentials()
    {
        // Arrange
        var options = new TwitterClientOptions
        {
            ConsumerKey = "test-consumer-key",
            ConsumerSecret = "test-consumer-secret",
            AccessToken = "test-access-token",
            AccessTokenSecret = "test-access-token-secret",
            BearerToken = "test-bearer-token"
        };

        // Assert
        await Assert.That(options.ConsumerKey).IsEqualTo("test-consumer-key");
        await Assert.That(options.ConsumerSecret).IsEqualTo("test-consumer-secret");
        await Assert.That(options.AccessToken).IsEqualTo("test-access-token");
        await Assert.That(options.AccessTokenSecret).IsEqualTo("test-access-token-secret");
        await Assert.That(options.BearerToken).IsEqualTo("test-bearer-token");
    }

    [Test]
    public async Task TwitterClientOptions_ShouldAllowCustomizingTimeout()
    {
        // Arrange
        var customTimeout = TimeSpan.FromSeconds(60);

        // Act
        var options = new TwitterClientOptions
        {
            Timeout = customTimeout
        };

        // Assert
        await Assert.That(options.Timeout).IsEqualTo(customTimeout);
    }

    [Test]
    public async Task TwitterClientOptions_ShouldAllowCustomizingMaxRetries()
    {
        // Arrange
        var customRetries = 5;

        // Act
        var options = new TwitterClientOptions
        {
            MaxRetries = customRetries
        };

        // Assert
        await Assert.That(options.MaxRetries).IsEqualTo(customRetries);
}
}