using TUnit.Core;
using System.Text.Json;
using Aero.Social.Twitter.Client.Models;
using System.Threading.Tasks;

namespace Aero.Social.Twitter.Models;

/// <summary>
/// Represents a class for TweetTests.
/// </summary>
public class TweetTests
{
        /// <summary>
    /// Tweet_ShouldHaveRequiredProperties method.
    /// </summary>
[Test]
    public async Task Tweet_ShouldHaveRequiredProperties()
    {
        // Arrange & Act
        var tweet = new Tweet
        {
            Id = "1234567890",
            Text = "Hello, Twitter!",
            CreatedAt = DateTimeOffset.UtcNow,
            AuthorId = "9876543210"
        };

        // Assert
        Assert.NotNull(tweet.Id);
        Assert.NotNull(tweet.Text);
        await Assert.That(tweet.CreatedAt).IsNotEqualTo(default(DateTimeOffset));
    }

        /// <summary>
    /// Tweet_Serialization_ShouldIncludeAllProperties method.
    /// </summary>
[Test]
    public async Task Tweet_Serialization_ShouldIncludeAllProperties()
    {
        // Arrange
        var createdAt = DateTimeOffset.Parse("2024-01-15T10:30:00+00:00");
        var tweet = new Tweet
        {
            Id = "1234567890",
            Text = "Hello, Twitter!",
            CreatedAt = createdAt,
            AuthorId = "9876543210",
            PublicMetrics = new PublicMetrics
            {
                RetweetCount = 10,
                ReplyCount = 5,
                LikeCount = 50,
                QuoteCount = 2
            }
        };

        // Act
        var json = JsonSerializer.Serialize(tweet);

        // Assert
        await Assert.That(json).Contains("1234567890");
        await Assert.That(json).Contains("Hello, Twitter!");
        await Assert.That(json).Contains("9876543210");
    }

        /// <summary>
    /// Tweet_Deserialization_ShouldParseAllProperties method.
    /// </summary>
[Test]
    public async Task Tweet_Deserialization_ShouldParseAllProperties()
    {
        // Arrange
        var json = @"{
                ""id"": ""1234567890"",
                ""text"": ""Hello, Twitter!"",
                ""created_at"": ""2024-01-15T10:30:00.000Z"",
                ""author_id"": ""9876543210"",
                ""public_metrics"": {
                    ""retweet_count"": 10,
                    ""reply_count"": 5,
                    ""like_count"": 50,
                    ""quote_count"": 2
                }
            }";

        // Act
        var tweet = JsonSerializer.Deserialize<Tweet>(json);

        // Assert
        Assert.NotNull(tweet);
        await Assert.That(tweet.Id).IsEqualTo("1234567890");
        await Assert.That(tweet.Text).IsEqualTo("Hello, Twitter!");
        await Assert.That(tweet.AuthorId).IsEqualTo("9876543210");
        Assert.NotNull(tweet.PublicMetrics);
        await Assert.That(tweet.PublicMetrics.RetweetCount).IsEqualTo(10);
        await Assert.That(tweet.PublicMetrics.ReplyCount).IsEqualTo(5);
        await Assert.That(tweet.PublicMetrics.LikeCount).IsEqualTo(50);
        await Assert.That(tweet.PublicMetrics.QuoteCount).IsEqualTo(2);
    }

        /// <summary>
    /// Tweet_Deserialization_ShouldHandleNullableFields method.
    /// </summary>
[Test]
    public async Task Tweet_Deserialization_ShouldHandleNullableFields()
    {
        // Arrange
        var json = @"{
                ""id"": ""1234567890"",
                ""text"": ""Just a simple tweet"",
                ""created_at"": ""2024-01-15T10:30:00.000Z""
            }";

        // Act
        var tweet = JsonSerializer.Deserialize<Tweet>(json);

        // Assert
        Assert.NotNull(tweet);
        await Assert.That(tweet.Id).IsEqualTo("1234567890");
        Assert.Null(tweet.AuthorId);
        Assert.Null(tweet.PublicMetrics);
}
}