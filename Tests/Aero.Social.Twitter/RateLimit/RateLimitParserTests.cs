using TUnit.Core;
using System.Net;
using Aero.Social.Twitter.Client.RateLimit;
using System.Threading.Tasks;

namespace Aero.Social.Twitter.RateLimit;

public class RateLimitParserTests
{
    [Test]
    public async Task ParseRateLimitHeaders_AllHeadersPresent_ReturnsParsedInfo()
    {
        // Arrange
        var response = new HttpResponseMessage(HttpStatusCode.OK);
        response.Headers.Add("X-Rate-Limit-Limit", "150");
        response.Headers.Add("X-Rate-Limit-Remaining", "75");
        response.Headers.Add("X-Rate-Limit-Reset", "1234567890");
        response.Headers.Add("Retry-After", "60");

        // Act
        var info = RateLimitParser.ParseRateLimitHeaders(response);

        // Assert
        Assert.NotNull(info);
        await Assert.That(info.Limit).IsEqualTo(150);
        await Assert.That(info.Remaining).IsEqualTo(75);
        await Assert.That(info.ResetTimestamp).IsEqualTo(1234567890);
        await Assert.That(info.RetryAfter).IsEqualTo(TimeSpan.FromSeconds(60));
    }

    [Test]
    public void ParseRateLimitHeaders_NoHeaders_ReturnsNull()
    {
        // Arrange
        var response = new HttpResponseMessage(HttpStatusCode.OK);

        // Act
        var info = RateLimitParser.ParseRateLimitHeaders(response);

        // Assert
        Assert.Null(info);
    }

    [Test]
    public async Task ParseRateLimitHeaders_PartialHeaders_ReturnsPartialInfo()
    {
        // Arrange
        var response = new HttpResponseMessage(HttpStatusCode.OK);
        response.Headers.Add("X-Rate-Limit-Limit", "100");
        response.Headers.Add("X-Rate-Limit-Remaining", "50");
        // Missing Reset header

        // Act
        var info = RateLimitParser.ParseRateLimitHeaders(response);

        // Assert
        Assert.NotNull(info);
        await Assert.That(info.Limit).IsEqualTo(100);
        await Assert.That(info.Remaining).IsEqualTo(50);
        await Assert.That(info.ResetTimestamp).IsEqualTo(0);
    }

    [Test]
    public async Task ParseRateLimitHeaders_InvalidValues_ReturnsPartialInfo()
    {
        // Arrange
        var response = new HttpResponseMessage(HttpStatusCode.OK);
        response.Headers.Add("X-Rate-Limit-Limit", "invalid");
        response.Headers.Add("X-Rate-Limit-Remaining", "75");
        response.Headers.Add("X-Rate-Limit-Reset", "1234567890");

        // Act
        var info = RateLimitParser.ParseRateLimitHeaders(response);

        // Assert
        Assert.NotNull(info);
        await Assert.That(info.Limit).IsEqualTo(0);
        await Assert.That(info.Remaining).IsEqualTo(75);
        await Assert.That(info.ResetTimestamp).IsEqualTo(1234567890);
    }

    [Test]
    public void ParseRateLimitHeaders_NullResponse_ReturnsNull()
    {
        // Act
        var info = RateLimitParser.ParseRateLimitHeaders(null);

        // Assert
        Assert.Null(info);
    }

    [Test]
    public async Task GetRateLimitDescription_WithValidInfo_ReturnsDescriptiveString()
    {
        // Arrange
        var info = new RateLimitInfo
        {
            Limit = 100,
            Remaining = 50,
            ResetTimestamp = DateTimeOffset.UtcNow.AddMinutes(15).ToUnixTimeSeconds()
        };

        // Act
        var description = RateLimitParser.GetRateLimitDescription(info);

        // Assert
        await Assert.That(description).Contains("50 of 100");
        await Assert.That(description).Contains("50% consumed");
    }

    [Test]
    public async Task GetRateLimitDescription_WithRateLimit_ReturnsRateLimitedMessage()
    {
        // Arrange
        var info = new RateLimitInfo
        {
            Limit = 100,
            Remaining = 0,
            ResetTimestamp = DateTimeOffset.UtcNow.AddMinutes(15).ToUnixTimeSeconds()
        };

        // Act
        var description = RateLimitParser.GetRateLimitDescription(info);

        // Assert
        await Assert.That(description).Contains("Rate limit exceeded");
    }

    [Test]
    public async Task GetRateLimitDescription_WithApproachingLimit_ReturnsApproachingMessage()
    {
        // Arrange
        var info = new RateLimitInfo
        {
            Limit = 100,
            Remaining = 15,  // Less than 20%
            ResetTimestamp = DateTimeOffset.UtcNow.AddMinutes(15).ToUnixTimeSeconds()
        };

        // Act
        var description = RateLimitParser.GetRateLimitDescription(info);

        // Assert
        await Assert.That(description).Contains("Approaching rate limit");
    }

    [Test]
    public async Task GetRateLimitDescription_WithNullInfo_ReturnsNotAvailableMessage()
    {
        // Act
        var description = RateLimitParser.GetRateLimitDescription(null);

        // Assert
        await Assert.That(description).IsEqualTo("Rate limit information not available.");
    }

    [Test]
    [Arguments(100, 5, true)]   // Less than 10% remaining
    [Arguments(100, 0, true)]   // Rate limited
    [Arguments(100, 15, false)] // Above 10% threshold
    [Arguments(100, 50, false)] // Well above threshold
    public async Task ShouldLogWarning_VariousScenarios_ReturnsExpectedResult(int limit, int remaining, bool expected)
    {
        // Arrange
        var info = new RateLimitInfo
        {
            Limit = limit,
            Remaining = remaining,
            ResetTimestamp = DateTimeOffset.UtcNow.AddMinutes(15).ToUnixTimeSeconds()
        };

        // Act & Assert
        await Assert.That(RateLimitParser.ShouldLogWarning(info)).IsEqualTo(expected);
    }

    [Test]
    public async Task ShouldLogWarning_WithNullInfo_ReturnsFalse()
    {
        // Act & Assert
        await Assert.That(RateLimitParser.ShouldLogWarning(null)).IsFalse();
    }

    [Test]
    public async Task ParseRateLimitHeaders_OnlyRetryAfter_ReturnsInfoWithRetryAfter()
    {
        // Arrange
        var response = new HttpResponseMessage((HttpStatusCode)429);
        response.Headers.Add("Retry-After", "900");  // 15 minutes

        // Act
        var info = RateLimitParser.ParseRateLimitHeaders(response);

        // Assert
        Assert.NotNull(info);
        await Assert.That(info.Limit).IsEqualTo(0);
        await Assert.That(info.Remaining).IsEqualTo(0);
        await Assert.That(info.RetryAfter).IsEqualTo(TimeSpan.FromSeconds(900));
}
}