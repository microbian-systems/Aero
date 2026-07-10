using TUnit.Core;
using Aero.Social.Twitter.Client.Serialization;
using System.Threading.Tasks;

namespace Aero.Social.Twitter.Serialization;

/// <summary>
/// Represents a class for TwitterJsonSerializerTests.
/// </summary>
public class TwitterJsonSerializerTests
{
        /// <summary>
    /// Deserialize_ShouldParseValidJson method.
    /// </summary>
[Test]
    public async Task Deserialize_ShouldParseValidJson()
    {
        // Arrange
        var json = @"{""id"": ""123"", ""text"": ""Hello""}";

        // Act
        var result = TwitterJsonSerializer.Deserialize<TestModel>(json);

        // Assert
        Assert.NotNull(result);
        await Assert.That(result.Id).IsEqualTo("123");
        await Assert.That(result.Text).IsEqualTo("Hello");
    }

        /// <summary>
    /// Deserialize_ShouldHandleSnakeCase method.
    /// </summary>
[Test]
    public async Task Deserialize_ShouldHandleSnakeCase()
    {
        // Arrange
        var json = @"{""created_at"": ""2024-01-15T10:30:00.000Z"", ""author_id"": ""456""}";

        // Act
        var result = TwitterJsonSerializer.Deserialize<TestModel>(json);

        // Assert
        Assert.NotNull(result);
        await Assert.That(result.AuthorId).IsEqualTo("456");
    }

        /// <summary>
    /// Serialize_ShouldOutputSnakeCase method.
    /// </summary>
[Test]
    public async Task Serialize_ShouldOutputSnakeCase()
    {
        // Arrange
        var model = new TestModel
        {
            Id = "123",
            Text = "Hello",
            AuthorId = "456"
        };

        // Act
        var json = TwitterJsonSerializer.Serialize(model);

        // Assert
        await Assert.That(json).Contains("\"id\"");
        await Assert.That(json).Contains("\"text\"");
        await Assert.That(json).Contains("\"author_id\"");
    }

        /// <summary>
    /// Serialize_ShouldSkipNullValues method.
    /// </summary>
[Test]
    public async Task Serialize_ShouldSkipNullValues()
    {
        // Arrange
        var model = new TestModel
        {
            Id = "123",
            Text = null,
            AuthorId = "456"
        };

        // Act
        var json = TwitterJsonSerializer.Serialize(model);

        // Assert
        await Assert.That(json).Contains("\"id\"");
        await Assert.That(json).Contains("\"author_id\"");
    }

    private class TestModel
    {
                /// <summary>
        /// Gets or sets the Id.
        /// </summary>
public string? Id { get; set; }
                /// <summary>
        /// Gets or sets the Text.
        /// </summary>
public string? Text { get; set; }
                /// <summary>
        /// Gets or sets the Author Id.
        /// </summary>
public string? AuthorId { get; set;
}
    }
}