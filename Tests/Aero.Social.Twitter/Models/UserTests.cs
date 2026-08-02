using TUnit.Core;
using System.Text.Json;
using Aero.Social.Twitter.Client.Models;
using System.Threading.Tasks;

namespace Aero.Social.Twitter.Models;

/// <summary>
/// Represents a class for UserTests.
/// </summary>
public class UserTests
{
        /// <summary>
    /// User_DefaultValues_AreSetCorrectly method.
    /// </summary>
[Test]
    public async Task User_DefaultValues_AreSetCorrectly()
    {
        // Arrange & Act
        var user = new User
        {
            Id = "1234567890" // Id is required
        };

        // Assert
        Assert.NotNull(user.Id);
        await Assert.That(user.CreatedAt).IsEqualTo(default(DateTimeOffset));
        await Assert.That(user.Verified).IsFalse();
    }

        /// <summary>
    /// User_Serialization_WithAllProperties_ReturnsCorrectJson method.
    /// </summary>
[Test]
    public async Task User_Serialization_WithAllProperties_ReturnsCorrectJson()
    {
        // Arrange
        var user = new User
        {
            Id = "1234567890",
            Name = "Test User",
            Username = "testuser",
            CreatedAt = new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero),
            Description = "This is a test user",
            Location = "Test Location",
            ProfileImageUrl = "https://example.com/image.jpg",
            Verified = true,
            Url = "https://example.com",
            VerifiedType = "blue",
            PublicMetrics = new UserPublicMetrics
            {
                FollowersCount = 100,
                FollowingCount = 50,
                TweetCount = 25,
                ListedCount = 10
            }
        };

        // Act
        var json = JsonSerializer.Serialize(user);

        // Assert
        await Assert.That(json).Contains("\"id\":\"1234567890\"");
        await Assert.That(json).Contains("\"name\":\"Test User\"");
        await Assert.That(json).Contains("\"username\":\"testuser\"");
        await Assert.That(json).Contains("\"created_at\":\"2020-01-01T00:00:00+00:00\"");
        await Assert.That(json).Contains("\"description\":\"This is a test user\"");
        await Assert.That(json).Contains("\"location\":\"Test Location\"");
        await Assert.That(json).Contains("\"profile_image_url\":\"https://example.com/image.jpg\"");
        await Assert.That(json).Contains("\"verified\":true");
        await Assert.That(json).Contains("\"url\":\"https://example.com\"");
        await Assert.That(json).Contains("\"verified_type\":\"blue\"");
        await Assert.That(json).Contains("\"public_metrics\"");
    }

        /// <summary>
    /// User_Serialization_WithMinimalProperties_ReturnsCorrectJson method.
    /// </summary>
[Test]
    public async Task User_Serialization_WithMinimalProperties_ReturnsCorrectJson()
    {
        // Arrange
        var user = new User
        {
            Id = "1234567890",
            Name = "Test User",
            Username = "testuser",
            CreatedAt = new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero)
        };

        // Act
        var json = JsonSerializer.Serialize(user);

        // Assert
        await Assert.That(json).Contains("\"id\":\"1234567890\"");
        await Assert.That(json).Contains("\"name\":\"Test User\"");
        await Assert.That(json).Contains("\"username\":\"testuser\"");
        await Assert.That(json).Contains("\"verified\":false");
    }

        /// <summary>
    /// User_Deserialization_WithAllProperties_PopulatesCorrectly method.
    /// </summary>
[Test]
    public async Task User_Deserialization_WithAllProperties_PopulatesCorrectly()
    {
        // Arrange
        var json = @"{
                ""id"": ""1234567890"",
                ""name"": ""Test User"",
                ""username"": ""testuser"",
                ""created_at"": ""2020-01-01T00:00:00.000Z"",
                ""description"": ""This is a test user"",
                ""location"": ""Test Location"",
                ""profile_image_url"": ""https://example.com/image.jpg"",
                ""verified"": true,
                ""url"": ""https://example.com"",
                ""verified_type"": ""blue"",
                ""public_metrics"": {
                    ""followers_count"": 100,
                    ""following_count"": 50,
                    ""tweet_count"": 25,
                    ""listed_count"": 10
                }
            }";

        // Act
        var user = JsonSerializer.Deserialize<User>(json);

        // Assert
        Assert.NotNull(user);
        await Assert.That(user.Id).IsEqualTo("1234567890");
        await Assert.That(user.Name).IsEqualTo("Test User");
        await Assert.That(user.Username).IsEqualTo("testuser");
        await Assert.That(user.CreatedAt).IsEqualTo(new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero));
        await Assert.That(user.Description).IsEqualTo("This is a test user");
        await Assert.That(user.Location).IsEqualTo("Test Location");
        await Assert.That(user.ProfileImageUrl).IsEqualTo("https://example.com/image.jpg");
        await Assert.That(user.Verified).IsTrue();
        await Assert.That(user.Url).IsEqualTo("https://example.com");
        await Assert.That(user.VerifiedType).IsEqualTo("blue");
        Assert.NotNull(user.PublicMetrics);
        await Assert.That(user.PublicMetrics.FollowersCount).IsEqualTo(100);
    }

        /// <summary>
    /// User_Deserialization_WithMinimalProperties_PopulatesCorrectly method.
    /// </summary>
[Test]
    public async Task User_Deserialization_WithMinimalProperties_PopulatesCorrectly()
    {
        // Arrange
        var json = @"{
                ""id"": ""1234567890"",
                ""name"": ""Test User"",
                ""username"": ""testuser"",
                ""created_at"": ""2020-01-01T00:00:00.000Z""
            }";

        // Act
        var user = JsonSerializer.Deserialize<User>(json);

        // Assert
        Assert.NotNull(user);
        await Assert.That(user.Id).IsEqualTo("1234567890");
        await Assert.That(user.Name).IsEqualTo("Test User");
        await Assert.That(user.Username).IsEqualTo("testuser");
        await Assert.That(user.CreatedAt).IsEqualTo(new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero));
        Assert.Null(user.Description);
        Assert.Null(user.Location);
        Assert.Null(user.ProfileImageUrl);
        await Assert.That(user.Verified).IsFalse();
        Assert.Null(user.Url);
        Assert.Null(user.VerifiedType);
        Assert.Null(user.PublicMetrics);
    }

        /// <summary>
    /// User_Deserialization_WithNullFields_HandlesCorrectly method.
    /// </summary>
[Test]
    public async Task User_Deserialization_WithNullFields_HandlesCorrectly()
    {
        // Arrange
        var json = @"{
                ""id"": ""1234567890"",
                ""name"": null,
                ""username"": null,
                ""created_at"": ""2020-01-01T00:00:00.000Z"",
                ""description"": null,
                ""public_metrics"": null
            }";

        // Act
        var user = JsonSerializer.Deserialize<User>(json);

        // Assert
        Assert.NotNull(user);
        await Assert.That(user.Id).IsEqualTo("1234567890");
        Assert.Null(user.Name);
        Assert.Null(user.Username);
        Assert.Null(user.Description);
        Assert.Null(user.PublicMetrics);
}
}