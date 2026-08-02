using TUnit.Core;
using Aero.Social.Twitter.Client.Models;
using System.Threading.Tasks;

namespace Aero.Social.Twitter.Models;

/// <summary>
/// Represents a class for ExpansionOptionsTests.
/// </summary>
public class ExpansionOptionsTests
{
        /// <summary>
    /// ExpansionOptions_ToApiString_WithNone_ReturnsEmptyString method.
    /// </summary>
[Test]
    public async Task ExpansionOptions_ToApiString_WithNone_ReturnsEmptyString()
    {
        // Arrange
        var expansions = ExpansionOptions.None;

        // Act
        var result = expansions.ToApiString();

        // Assert
        await Assert.That(result).IsEqualTo(string.Empty);
    }

        /// <summary>
    /// ExpansionOptions_ToApiString_IndividualOptions_ReturnCorrectValues method.
    /// </summary>
[Test]
    [Arguments(ExpansionOptions.AuthorId, "author_id")]
    [Arguments(ExpansionOptions.ReferencedTweetsId, "referenced_tweets.id")]
    [Arguments(ExpansionOptions.ReferencedTweetsIdAuthorId, "referenced_tweets.id.author_id")]
    [Arguments(ExpansionOptions.AttachmentsMediaKeys, "attachments.media_keys")]
    [Arguments(ExpansionOptions.AttachmentsPollIds, "attachments.poll_ids")]
    [Arguments(ExpansionOptions.EntitiesMentionsUsername, "entities.mentions.username")]
    [Arguments(ExpansionOptions.EntitiesNoteMentionsUsername, "entities.note.mentions.username")]
    public async Task ExpansionOptions_ToApiString_IndividualOptions_ReturnCorrectValues(ExpansionOptions expansion, string expected)
    {
        // Act
        var result = expansion.ToApiString();

        // Assert
        await Assert.That(result).IsEqualTo(expected);
    }

        /// <summary>
    /// ExpansionOptions_ToApiString_WithMultipleOptions_ReturnsCommaSeparatedString method.
    /// </summary>
[Test]
    public async Task ExpansionOptions_ToApiString_WithMultipleOptions_ReturnsCommaSeparatedString()
    {
        // Arrange
        var expansions = ExpansionOptions.AuthorId | ExpansionOptions.AttachmentsMediaKeys;

        // Act
        var result = expansions.ToApiString();

        // Assert
        await Assert.That(result).Contains("author_id");
        await Assert.That(result).Contains("attachments.media_keys");
        await Assert.That(result).Contains(",");
    }

        /// <summary>
    /// ExpansionOptions_ToApiString_WithAllOptions_ReturnsAllExpansionNames method.
    /// </summary>
[Test]
    public async Task ExpansionOptions_ToApiString_WithAllOptions_ReturnsAllExpansionNames()
    {
        // Arrange
        var expansions = ExpansionOptions.All;

        // Act
        var result = expansions.ToApiString();

        // Assert
        await Assert.That(result).Contains("author_id");
        await Assert.That(result).Contains("referenced_tweets.id");
        await Assert.That(result).Contains("referenced_tweets.id.author_id");
        await Assert.That(result).Contains("attachments.media_keys");
        await Assert.That(result).Contains("attachments.poll_ids");
        await Assert.That(result).Contains("entities.mentions.username");
        await Assert.That(result).Contains("entities.note.mentions.username");
    }

        /// <summary>
    /// ExpansionOptions_ToApiString_WithComplexCombination_ReturnsCorrectString method.
    /// </summary>
[Test]
    public async Task ExpansionOptions_ToApiString_WithComplexCombination_ReturnsCorrectString()
    {
        // Arrange
        var expansions = ExpansionOptions.AuthorId | ExpansionOptions.ReferencedTweetsId | ExpansionOptions.AttachmentsMediaKeys;

        // Act
        var result = expansions.ToApiString();

        // Assert
        await Assert.That(result).Contains("author_id");
        await Assert.That(result).Contains("referenced_tweets.id");
        await Assert.That(result).Contains("attachments.media_keys");
}
}