using TUnit.Core;
using Aero.Social.Twitter.Client.Clients;
using Aero.Social.Twitter.Client.Configuration;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;

namespace Aero.Social.Twitter.Clients;

public class TwitterClientTests
{
    [Test]
    public async Task TwitterClient_ShouldImplementITwitterClient()
    {
        // Arrange
        var httpClient = new HttpClient();
        var options = Options.Create(new TwitterClientOptions
        {
            BearerToken = "test_bearer_token"
        });

        // Act
        var client = new TwitterClient(httpClient, options);

        // Assert
        await Assert.That(client).IsAssignableTo<ITwitterClient>();
    }

    [Test]
    public void TwitterClient_Constructor_ShouldThrowOnNullHttpClient()
    {
        // Arrange
        var options = Options.Create(new TwitterClientOptions());

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new TwitterClient(null!, options));
    }

    [Test]
    public void TwitterClient_Constructor_ShouldThrowOnNullOptions()
    {
        // Arrange
        var httpClient = new HttpClient();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new TwitterClient(httpClient, null!));
    }
}

public class ITwitterClientTests
{
    [Test]
    public async Task ITwitterClient_ShouldHaveGetTweetAsyncMethod()
    {
        // This test verifies the interface contract
        var type = typeof(ITwitterClient);
        var method = type.GetMethod("GetTweetAsync");

        Assert.NotNull(method);
        await Assert.That(method.ReturnType).IsEqualTo(typeof(Task<>).MakeGenericType(typeof(Aero.Social.Twitter.Client.Models.Tweet)));
}
}