using TUnit.Core;
using System.Net;
using Aero.Social.Twitter.Client.Errors;
using System.Threading.Tasks;

namespace Aero.Social.Twitter.Errors;

public class TwitterErrorInfoTests
{
    [Test]
    [Arguments(32, "Could not authenticate you")]
    [Arguments(34, "Sorry, that page does not exist")]
    [Arguments(88, "Rate limit exceeded")]
    [Arguments(144, "No status found with that ID")]
    [Arguments(187, "Status is a duplicate")]
    [Arguments(215, "Bad authentication data")]
    public async Task GetErrorTitle_KnownErrorCode_ReturnsExpectedTitle(int code, string expectedTitle)
    {
        // Act
        var title = TwitterErrorInfo.GetErrorTitle(code);

        // Assert
        await Assert.That(title).IsEqualTo(expectedTitle);
    }

    [Test]
    public async Task GetErrorTitle_UnknownErrorCode_ReturnsUnknownError()
    {
        // Act
        var title = TwitterErrorInfo.GetErrorTitle(99999);

        // Assert
        await Assert.That(title).IsEqualTo("Unknown Error");
    }

    [Test]
    [Arguments(32)]
    [Arguments(88)]
    [Arguments(144)]
    public async Task GetSuggestedAction_KnownErrorCode_ReturnsNonEmptyAction(int code)
    {
        // Act
        var action = TwitterErrorInfo.GetSuggestedAction(code);

        // Assert
        await Assert.That(string.IsNullOrEmpty(action)).IsFalse();
        await Assert.That(action).DoesNotContain("unexpected error");
    }

    [Test]
    [Arguments(32)]
    [Arguments(88)]
    [Arguments(144)]
    public async Task GetDocumentationUrl_KnownErrorCode_ReturnsValidUrl(int code)
    {
        // Act
        var url = TwitterErrorInfo.GetDocumentationUrl(code);

        // Assert
        await Assert.That(string.IsNullOrEmpty(url)).IsFalse();
        await Assert.That(url).StartsWith("https://");
    }

    [Test]
    public async Task BuildEnhancedMessage_KnownErrorCode_IncludesAllComponents()
    {
        // Arrange
        int code = 88;
        string apiMessage = "Rate limit exceeded";

        // Act
        var message = TwitterErrorInfo.BuildEnhancedMessage(code, apiMessage);

        // Assert
        await Assert.That(message).Contains("Twitter API Error 88");
        await Assert.That(message).Contains("Rate limit exceeded");
        await Assert.That(message).Contains("API Message:");
        await Assert.That(message).Contains("Suggested Action:");
        await Assert.That(message).Contains("Documentation:");
        await Assert.That(message).Contains("https://");
    }

    [Test]
    public async Task BuildEnhancedMessage_NullApiMessage_DoesNotIncludeApiMessage()
    {
        // Arrange
        int code = 88;

        // Act
        var message = TwitterErrorInfo.BuildEnhancedMessage(code, null);

        // Assert
        await Assert.That(message).Contains("Twitter API Error 88");
        await Assert.That(message).DoesNotContain("API Message:");
    }

    [Test]
    [Arguments(400, true)]   // Bad Request
    [Arguments(404, true)]   // Not Found
    [Arguments(429, true)]   // Too Many Requests
    [Arguments(499, true)]   // Client closed request
    [Arguments(399, false)]  // Just below 4xx
    [Arguments(500, false)]  // Server error
    [Arguments(200, false)]  // OK
    public async Task IsClientError_VariousStatusCodes_ReturnsExpectedResult(int statusCode, bool expected)
    {
        // Act
        var result = TwitterErrorInfo.IsClientError((HttpStatusCode)statusCode);

        // Assert
        await Assert.That(result).IsEqualTo(expected);
    }

    [Test]
    [Arguments(500, true)]   // Internal Server Error
    [Arguments(503, true)]   // Service Unavailable
    [Arguments(504, true)]   // Gateway Timeout
    [Arguments(599, true)]   // Unknown server error
    [Arguments(499, false)]  // Just below 5xx
    [Arguments(400, false)]  // Client error
    [Arguments(200, false)]  // OK
    public async Task IsServerError_VariousStatusCodes_ReturnsExpectedResult(int statusCode, bool expected)
    {
        // Act
        var result = TwitterErrorInfo.IsServerError((HttpStatusCode)statusCode);

        // Assert
        await Assert.That(result).IsEqualTo(expected);
    }

    [Test]
    [Arguments(429, true)]   // Too Many Requests
    [Arguments(428, false)]  // Precondition Required
    [Arguments(430, false)]  // Unknown
    [Arguments(200, false)]  // OK
    public async Task IsRateLimitError_VariousStatusCodes_ReturnsExpectedResult(int statusCode, bool expected)
    {
        // Act
        var result = TwitterErrorInfo.IsRateLimitError((HttpStatusCode)statusCode);

        // Assert
        await Assert.That(result).IsEqualTo(expected);
    }

    [Test]
    public async Task GetErrorInfo_KnownErrorCode_ReturnsAllComponents()
    {
        // Arrange
        int code = 32;

        // Act
        var (title, action, docUrl) = TwitterErrorInfo.GetErrorInfo(code);

        // Assert
        await Assert.That(title).IsEqualTo("Could not authenticate you");
        await Assert.That(string.IsNullOrEmpty(action)).IsFalse();
        await Assert.That(docUrl).StartsWith("https://");
    }

    [Test]
    public async Task GetErrorInfo_UnknownErrorCode_ReturnsDefaultComponents()
    {
        // Arrange
        int code = 99999;

        // Act
        var (title, action, docUrl) = TwitterErrorInfo.GetErrorInfo(code);

        // Assert
        await Assert.That(title).IsEqualTo("Unknown Error");
        await Assert.That(action).Contains("unexpected error");
        await Assert.That(docUrl).StartsWith("https://");
}
}