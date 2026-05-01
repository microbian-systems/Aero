using TUnit.Core;
using System.Text.Json;
using Aero.Social.Twitter.Client.Models;
using System.Threading.Tasks;

namespace Aero.Social.Twitter.Models;

public class UserPublicMetricsTests
{
    [Test]
    public async Task UserPublicMetrics_DefaultValues_AreZero()
    {
        // Arrange & Act
        var metrics = new UserPublicMetrics();

        // Assert
        await Assert.That(metrics.FollowersCount).IsEqualTo(0);
        await Assert.That(metrics.FollowingCount).IsEqualTo(0);
        await Assert.That(metrics.TweetCount).IsEqualTo(0);
        await Assert.That(metrics.ListedCount).IsEqualTo(0);
    }

    [Test]
    public async Task UserPublicMetrics_Serialization_ReturnsCorrectJson()
    {
        // Arrange
        var metrics = new UserPublicMetrics
        {
            FollowersCount = 1000,
            FollowingCount = 500,
            TweetCount = 250,
            ListedCount = 50
        };

        // Act
        var json = JsonSerializer.Serialize(metrics);

        // Assert
        await Assert.That(json).Contains("\"followers_count\":1000");
        await Assert.That(json).Contains("\"following_count\":500");
        await Assert.That(json).Contains("\"tweet_count\":250");
        await Assert.That(json).Contains("\"listed_count\":50");
    }

    [Test]
    public async Task UserPublicMetrics_Deserialization_PopulatesCorrectly()
    {
        // Arrange
        var json = @"{
                ""followers_count"": 1000,
                ""following_count"": 500,
                ""tweet_count"": 250,
                ""listed_count"": 50
            }";

        // Act
        var metrics = JsonSerializer.Deserialize<UserPublicMetrics>(json);

        // Assert
        Assert.NotNull(metrics);
        await Assert.That(metrics.FollowersCount).IsEqualTo(1000);
        await Assert.That(metrics.FollowingCount).IsEqualTo(500);
        await Assert.That(metrics.TweetCount).IsEqualTo(250);
        await Assert.That(metrics.ListedCount).IsEqualTo(50);
    }

    [Test]
    public async Task UserPublicMetrics_Deserialization_WithPartialData_PopulatesCorrectly()
    {
        // Arrange
        var json = @"{
                ""followers_count"": 100,
                ""tweet_count"": 25
            }";

        // Act
        var metrics = JsonSerializer.Deserialize<UserPublicMetrics>(json);

        // Assert
        Assert.NotNull(metrics);
        await Assert.That(metrics.FollowersCount).IsEqualTo(100);
        await Assert.That(metrics.FollowingCount).IsEqualTo(0);
        await Assert.That(metrics.TweetCount).IsEqualTo(25);
        await Assert.That(metrics.ListedCount).IsEqualTo(0);
    }

    [Test]
    public async Task UserPublicMetrics_Deserialization_WithZeroValues_HandlesCorrectly()
    {
        // Arrange
        var json = @"{
                ""followers_count"": 0,
                ""following_count"": 0,
                ""tweet_count"": 0,
                ""listed_count"": 0
            }";

        // Act
        var metrics = JsonSerializer.Deserialize<UserPublicMetrics>(json);

        // Assert
        Assert.NotNull(metrics);
        await Assert.That(metrics.FollowersCount).IsEqualTo(0);
        await Assert.That(metrics.FollowingCount).IsEqualTo(0);
        await Assert.That(metrics.TweetCount).IsEqualTo(0);
        await Assert.That(metrics.ListedCount).IsEqualTo(0);
}
}