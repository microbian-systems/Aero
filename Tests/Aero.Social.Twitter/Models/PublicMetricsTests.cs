using TUnit.Core;
using System.Text.Json;
using Aero.Social.Twitter.Client.Models;
using System.Threading.Tasks;

namespace Aero.Social.Twitter.Models;

public class PublicMetricsTests
{
    [Test]
    public async Task PublicMetrics_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var metrics = new PublicMetrics();

        // Assert
        await Assert.That(metrics.RetweetCount).IsEqualTo(0);
        await Assert.That(metrics.ReplyCount).IsEqualTo(0);
        await Assert.That(metrics.LikeCount).IsEqualTo(0);
        await Assert.That(metrics.QuoteCount).IsEqualTo(0);
    }

    [Test]
    public async Task PublicMetrics_Serialization_ShouldIncludeAllProperties()
    {
        // Arrange
        var metrics = new PublicMetrics
        {
            RetweetCount = 100,
            ReplyCount = 25,
            LikeCount = 500,
            QuoteCount = 10
        };

        // Act
        var json = JsonSerializer.Serialize(metrics);

        // Assert
        await Assert.That(json).Contains("100");
        await Assert.That(json).Contains("25");
        await Assert.That(json).Contains("500");
        await Assert.That(json).Contains("10");
    }

    [Test]
    public async Task PublicMetrics_Deserialization_ShouldParseAllProperties()
    {
        // Arrange
        var json = @"{
                ""retweet_count"": 100,
                ""reply_count"": 25,
                ""like_count"": 500,
                ""quote_count"": 10
            }";

        // Act
        var metrics = JsonSerializer.Deserialize<PublicMetrics>(json);

        // Assert
        Assert.NotNull(metrics);
        await Assert.That(metrics.RetweetCount).IsEqualTo(100);
        await Assert.That(metrics.ReplyCount).IsEqualTo(25);
        await Assert.That(metrics.LikeCount).IsEqualTo(500);
        await Assert.That(metrics.QuoteCount).IsEqualTo(10);
}
}