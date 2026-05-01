using TUnit.Core;
using Aero.Social.Twitter.Client.Models;
using System.Threading.Tasks;

namespace Aero.Social.Twitter.Models;

public class TweetFieldsTests
{
    [Test]
    public async Task TweetFields_ToApiString_WithNone_ReturnsEmptyString()
    {
        // Arrange
        var fields = TweetFields.None;

        // Act
        var result = fields.ToApiString();

        // Assert
        await Assert.That(result).IsEqualTo(string.Empty);
    }

    [Test]
    [Arguments(TweetFields.AuthorId, "author_id")]
    [Arguments(TweetFields.CreatedAt, "created_at")]
    [Arguments(TweetFields.Text, "text")]
    [Arguments(TweetFields.Entities, "entities")]
    [Arguments(TweetFields.Geo, "geo")]
    [Arguments(TweetFields.InReplyToUserId, "in_reply_to_user_id")]
    [Arguments(TweetFields.Lang, "lang")]
    [Arguments(TweetFields.NonPublicMetrics, "non_public_metrics")]
    [Arguments(TweetFields.OrganicMetrics, "organic_metrics")]
    [Arguments(TweetFields.PromotedMetrics, "promoted_metrics")]
    [Arguments(TweetFields.PublicMetrics, "public_metrics")]
    [Arguments(TweetFields.ReferencedTweets, "referenced_tweets")]
    [Arguments(TweetFields.Source, "source")]
    public async Task TweetFields_ToApiString_IndividualFields_ReturnCorrectValues(TweetFields field, string expected)
    {
        // Act
        var result = field.ToApiString();

        // Assert
        await Assert.That(result).IsEqualTo(expected);
    }

    [Test]
    public async Task TweetFields_ToApiString_WithMultipleFields_ReturnsCommaSeparatedString()
    {
        // Arrange
        var fields = TweetFields.AuthorId | TweetFields.CreatedAt | TweetFields.Text;

        // Act
        var result = fields.ToApiString();

        // Assert
        await Assert.That(result).Contains("author_id");
        await Assert.That(result).Contains("created_at");
        await Assert.That(result).Contains("text");
        await Assert.That(result).Contains(",");
    }

    [Test]
    public async Task TweetFields_ToApiString_WithAllFields_ReturnsAllFieldNames()
    {
        // Arrange
        var fields = TweetFields.All;

        // Act
        var result = fields.ToApiString();

        // Assert
        await Assert.That(result).Contains("author_id");
        await Assert.That(result).Contains("created_at");
        await Assert.That(result).Contains("text");
        await Assert.That(result).Contains("entities");
        await Assert.That(result).Contains("geo");
        await Assert.That(result).Contains("in_reply_to_user_id");
        await Assert.That(result).Contains("lang");
        await Assert.That(result).Contains("non_public_metrics");
        await Assert.That(result).Contains("organic_metrics");
        await Assert.That(result).Contains("promoted_metrics");
        await Assert.That(result).Contains("public_metrics");
        await Assert.That(result).Contains("referenced_tweets");
        await Assert.That(result).Contains("source");
    }

    [Test]
    public async Task TweetFields_ToApiString_WithComplexCombination_ReturnsCorrectString()
    {
        // Arrange
        var fields = TweetFields.AuthorId | TweetFields.PublicMetrics | TweetFields.ReferencedTweets;

        // Act
        var result = fields.ToApiString();

        // Assert
        await Assert.That(result).Contains("author_id");
        await Assert.That(result).Contains("public_metrics");
        await Assert.That(result).Contains("referenced_tweets");
}
}