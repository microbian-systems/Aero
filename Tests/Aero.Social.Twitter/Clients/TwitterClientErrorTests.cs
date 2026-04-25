using TUnit.Core;
using Aero.Social.Twitter.Client.Clients;
using Aero.Social.Twitter.Client.Configuration;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;

namespace Aero.Social.Twitter.Clients;

public class TwitterClientErrorTests
{
    [Test]
    public async Task Constructor_ShouldThrow_WhenNoCredentialsProvided()
    {
        // Arrange
        var httpClient = new HttpClient();
        var options = Options.Create(new TwitterClientOptions());

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => new TwitterClient(httpClient, options));
        await Assert.That(exception.Message).Contains("No authentication credentials configured");
    }

    [Test]
    public async Task GetTweetAsync_ShouldThrowArgumentException_WhenTweetIdIsNull()
    {
        // Arrange
        var httpClient = new HttpClient();
        var options = Options.Create(new TwitterClientOptions { BearerToken = "test" });
        var client = new TwitterClient(httpClient, options);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => client.GetTweetAsync(null!));
    }

    [Test]
    public async Task GetTweetAsync_ShouldThrowArgumentException_WhenTweetIdIsEmpty()
    {
        // Arrange
        var httpClient = new HttpClient();
        var options = Options.Create(new TwitterClientOptions { BearerToken = "test" });
        var client = new TwitterClient(httpClient, options);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => client.GetTweetAsync(""));
}
}