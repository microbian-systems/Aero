using TUnit.Core;
using Aero.Social.Twitter.Client.Models;
using System.Threading.Tasks;

namespace Aero.Social.Twitter.Models;

/// <summary>
/// Represents a class for UserFieldsTests.
/// </summary>
public class UserFieldsTests
{
        /// <summary>
    /// UserFields_ToApiString_WithSingleField_ReturnsCorrectString method.
    /// </summary>
[Test]
    public async Task UserFields_ToApiString_WithSingleField_ReturnsCorrectString()
    {
        // Arrange
        var fields = UserFields.Description;

        // Act
        var result = fields.ToApiString();

        // Assert
        await Assert.That(result).IsEqualTo("description");
    }

        /// <summary>
    /// UserFields_ToApiString_WithMultipleFields_ReturnsCommaSeparatedString method.
    /// </summary>
[Test]
    public async Task UserFields_ToApiString_WithMultipleFields_ReturnsCommaSeparatedString()
    {
        // Arrange
        var fields = UserFields.Description | UserFields.Location | UserFields.Verified;

        // Act
        var result = fields.ToApiString();

        // Assert
        await Assert.That(result).Contains("description");
        await Assert.That(result).Contains("location");
        await Assert.That(result).Contains("verified");
        await Assert.That(result).Contains(",");
    }

        /// <summary>
    /// UserFields_ToApiString_WithAllFields_ReturnsAllFieldNames method.
    /// </summary>
[Test]
    public async Task UserFields_ToApiString_WithAllFields_ReturnsAllFieldNames()
    {
        // Arrange
        var fields = UserFields.All;

        // Act
        var result = fields.ToApiString();

        // Assert
        await Assert.That(result).Contains("created_at");
        await Assert.That(result).Contains("description");
        await Assert.That(result).Contains("location");
        await Assert.That(result).Contains("verified");
        await Assert.That(result).Contains("public_metrics");
        await Assert.That(result).Contains("profile_image_url");
    }

        /// <summary>
    /// UserFields_ToApiString_WithNone_ReturnsEmptyString method.
    /// </summary>
[Test]
    public async Task UserFields_ToApiString_WithNone_ReturnsEmptyString()
    {
        // Arrange
        var fields = UserFields.None;

        // Act
        var result = fields.ToApiString();

        // Assert
        await Assert.That(result).IsEqualTo(string.Empty);
    }

        /// <summary>
    /// UserFields_ToApiString_IndividualFields_ReturnCorrectValues method.
    /// </summary>
[Test]
    [Arguments(UserFields.CreatedAt, "created_at")]
    [Arguments(UserFields.Description, "description")]
    [Arguments(UserFields.Location, "location")]
    [Arguments(UserFields.Verified, "verified")]
    [Arguments(UserFields.PublicMetrics, "public_metrics")]
    [Arguments(UserFields.ProfileImageUrl, "profile_image_url")]
    [Arguments(UserFields.Url, "url")]
    [Arguments(UserFields.Username, "username")]
    public async Task UserFields_ToApiString_IndividualFields_ReturnCorrectValues(UserFields field, string expected)
    {
        // Act
        var result = field.ToApiString();

        // Assert
        await Assert.That(result).IsEqualTo(expected);
}
}