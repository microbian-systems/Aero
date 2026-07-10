using TUnit.Core;
using Aero.Social.Twitter.Client.Clients;
using Aero.Social.Twitter.Client.Configuration;
using Aero.Social.Twitter.Client.Extensions;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace Aero.Social.Twitter.Extensions;

/// <summary>
/// Represents a class for ServiceCollectionExtensionsTests.
/// </summary>
public class ServiceCollectionExtensionsTests
{
        /// <summary>
    /// AddTwitterClient_ShouldRegisterServices method.
    /// </summary>
[Test]
    public async Task AddTwitterClient_ShouldRegisterServices()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddTwitterClient(options =>
        {
            options.BearerToken = "test-bearer-token";
        });

        // Assert
        var provider = services.BuildServiceProvider();
        var client = provider.GetService<ITwitterClient>();
        Assert.NotNull(client);
        await Assert.That(client).IsTypeOf<TwitterClient>();
    }

        /// <summary>
    /// AddTwitterClient_ShouldConfigureOptions method.
    /// </summary>
[Test]
    public async Task AddTwitterClient_ShouldConfigureOptions()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddTwitterClient(options =>
        {
            options.ConsumerKey = "test-key";
            options.ConsumerSecret = "test-secret";
            options.MaxRetries = 5;
        });

        // Assert
        var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<TwitterClientOptions>>().Value;
        await Assert.That(options.ConsumerKey).IsEqualTo("test-key");
        await Assert.That(options.ConsumerSecret).IsEqualTo("test-secret");
        await Assert.That(options.MaxRetries).IsEqualTo(5);
}
}