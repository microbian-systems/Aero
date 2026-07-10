using TUnit.Core;
using Aero.Social.Twitter.Client.RateLimit;
using System.Threading.Tasks;

namespace Aero.Social.Twitter.RateLimit;

/// <summary>
/// Represents a class for RateLimitInfoTests.
/// </summary>
public class RateLimitInfoTests
{
        /// <summary>
    /// IsRateLimited_RemainingIsZero_ReturnsTrue method.
    /// </summary>
[Test]
    public async Task IsRateLimited_RemainingIsZero_ReturnsTrue()
    {
        // Arrange
        var info = new RateLimitInfo
        {
            Limit = 100,
            Remaining = 0,
            ResetTimestamp = DateTimeOffset.UtcNow.AddMinutes(15).ToUnixTimeSeconds()
        };

        // Act & Assert
        await Assert.That(info.IsRateLimited).IsTrue();
    }

        /// <summary>
    /// IsRateLimited_RemainingIsGreaterThanZero_ReturnsFalse method.
    /// </summary>
[Test]
    public async Task IsRateLimited_RemainingIsGreaterThanZero_ReturnsFalse()
    {
        // Arrange
        var info = new RateLimitInfo
        {
            Limit = 100,
            Remaining = 1,
            ResetTimestamp = DateTimeOffset.UtcNow.AddMinutes(15).ToUnixTimeSeconds()
        };

        // Act & Assert
        await Assert.That(info.IsRateLimited).IsFalse();
    }

        /// <summary>
    /// IsApproachingLimit_VariousScenarios_ReturnsExpectedResult method.
    /// </summary>
[Test]
    [Arguments(100, 20, false)]  // 80% consumed, above 20% threshold
    [Arguments(100, 19, true)]   // 81% consumed, below 20% threshold
    [Arguments(100, 10, true)]   // 90% consumed
    [Arguments(100, 0, false)]   // 100% consumed but remaining is 0
    public async Task IsApproachingLimit_VariousScenarios_ReturnsExpectedResult(int limit, int remaining, bool expected)
    {
        // Arrange
        var info = new RateLimitInfo
        {
            Limit = limit,
            Remaining = remaining,
            ResetTimestamp = DateTimeOffset.UtcNow.AddMinutes(15).ToUnixTimeSeconds()
        };

        // Act & Assert
        await Assert.That(info.IsApproachingLimit).IsEqualTo(expected);
    }

        /// <summary>
    /// PercentConsumed_VariousScenarios_ReturnsExpectedPercentage method.
    /// </summary>
[Test]
    [Arguments(100, 50, 50)]   // 50% consumed
    [Arguments(100, 0, 100)]   // 100% consumed
    [Arguments(100, 100, 0)]   // 0% consumed
    [Arguments(0, 0, 0)]       // Edge case: limit is 0
    public async Task PercentConsumed_VariousScenarios_ReturnsExpectedPercentage(int limit, int remaining, double expected)
    {
        // Arrange
        var info = new RateLimitInfo
        {
            Limit = limit,
            Remaining = remaining,
            ResetTimestamp = DateTimeOffset.UtcNow.AddMinutes(15).ToUnixTimeSeconds()
        };

        // Act & Assert
        await Assert.That(info.PercentConsumed).IsEqualTo(expected);
    }

        /// <summary>
    /// ResetTime_ValidTimestamp_ReturnsCorrectDateTimeOffset method.
    /// </summary>
[Test]
    public async Task ResetTime_ValidTimestamp_ReturnsCorrectDateTimeOffset()
    {
        // Arrange
        var futureTime = DateTimeOffset.UtcNow.AddMinutes(15);
        var info = new RateLimitInfo
        {
            ResetTimestamp = futureTime.ToUnixTimeSeconds()
        };

        // Act
        var resetTime = info.ResetTime;

        // Assert
        await Assert.That(resetTime.ToUnixTimeSeconds()).IsEqualTo(futureTime.ToUnixTimeSeconds());
    }

        /// <summary>
    /// TimeUntilReset_FutureResetTime_ReturnsPositiveTimeSpan method.
    /// </summary>
[Test]
    public async Task TimeUntilReset_FutureResetTime_ReturnsPositiveTimeSpan()
    {
        // Arrange
        var info = new RateLimitInfo
        {
            ResetTimestamp = DateTimeOffset.UtcNow.AddMinutes(15).ToUnixTimeSeconds()
        };

        // Act
        var timeUntilReset = info.TimeUntilReset;

        // Assert
        await Assert.That(timeUntilReset > TimeSpan.Zero).IsTrue();
        await Assert.That(timeUntilReset.TotalMinutes < 16).IsTrue();
    }

        /// <summary>
    /// TimeUntilReset_PastResetTime_ReturnsZero method.
    /// </summary>
[Test]
    public async Task TimeUntilReset_PastResetTime_ReturnsZero()
    {
        // Arrange
        var info = new RateLimitInfo
        {
            ResetTimestamp = DateTimeOffset.UtcNow.AddMinutes(-5).ToUnixTimeSeconds()
        };

        // Act
        var timeUntilReset = info.TimeUntilReset;

        // Assert
        await Assert.That(timeUntilReset).IsEqualTo(TimeSpan.Zero);
    }

        /// <summary>
    /// Properties_CanBeSetAndRetrieved method.
    /// </summary>
[Test]
    public async Task Properties_CanBeSetAndRetrieved()
    {
        // Arrange
        var info = new RateLimitInfo
        {
            Limit = 150,
            Remaining = 75,
            ResetTimestamp = 1234567890,
            RetryAfter = TimeSpan.FromSeconds(60)
        };

        // Act & Assert
        await Assert.That(info.Limit).IsEqualTo(150);
        await Assert.That(info.Remaining).IsEqualTo(75);
        await Assert.That(info.ResetTimestamp).IsEqualTo(1234567890);
        await Assert.That(info.RetryAfter).IsEqualTo(TimeSpan.FromSeconds(60));
}
}