using TUnit.Core;
using System.Net;
using Aero.Social.Twitter.Client.Exceptions;
using System.Threading.Tasks;

namespace Aero.Social.Twitter.Exceptions;

/// <summary>
/// Represents a class for TwitterApiExceptionTests.
/// </summary>
public class TwitterApiExceptionTests
{
        /// <summary>
    /// TwitterApiException_DefaultConstructor_ShouldCreateException method.
    /// </summary>
[Test]
    public async Task TwitterApiException_DefaultConstructor_ShouldCreateException()
    {
        // Act
        var exception = new TwitterApiException();

        // Assert
        Assert.NotNull(exception);
        await Assert.That(exception).IsTypeOf<TwitterApiException>();
    }

        /// <summary>
    /// TwitterApiException_MessageConstructor_ShouldSetMessage method.
    /// </summary>
[Test]
    public async Task TwitterApiException_MessageConstructor_ShouldSetMessage()
    {
        // Arrange
        var message = "Test error message";

        // Act
        var exception = new TwitterApiException(message);

        // Assert
        await Assert.That(exception.Message).IsEqualTo(message);
    }

        /// <summary>
    /// TwitterApiException_FullConstructor_ShouldSetAllProperties method.
    /// </summary>
[Test]
    public async Task TwitterApiException_FullConstructor_ShouldSetAllProperties()
    {
        // Arrange
        var message = "Test error message";
        var innerException = new InvalidOperationException("Inner error");
        var statusCode = HttpStatusCode.BadRequest;

        // Act
        var exception = new TwitterApiException(message, innerException, statusCode);

        // Assert
        await Assert.That(exception.Message).IsEqualTo(message);
        await Assert.That(exception.InnerException).IsEqualTo(innerException);
        await Assert.That(exception.StatusCode).IsEqualTo(statusCode);
    }

        /// <summary>
    /// TwitterApiException_StatusCode_ShouldBeAccessible method.
    /// </summary>
[Test]
    public async Task TwitterApiException_StatusCode_ShouldBeAccessible()
    {
        // Arrange
        var statusCode = HttpStatusCode.NotFound;

        // Act
        var exception = new TwitterApiException("Not found", null, statusCode);

        // Assert
        await Assert.That(exception.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
    }
}

/// <summary>
/// Represents a class for TwitterRateLimitExceptionTests.
/// </summary>
public class TwitterRateLimitExceptionTests
{
        /// <summary>
    /// TwitterRateLimitException_ShouldInheritFromTwitterApiException method.
    /// </summary>
[Test]
    public async Task TwitterRateLimitException_ShouldInheritFromTwitterApiException()
    {
        // Act
        var exception = new TwitterRateLimitException("Rate limit exceeded");

        // Assert
        await Assert.That(exception).IsAssignableTo<TwitterApiException>();
    }

        /// <summary>
    /// TwitterRateLimitException_ShouldHave429StatusCode method.
    /// </summary>
[Test]
    public async Task TwitterRateLimitException_ShouldHave429StatusCode()
    {
        // Act
        var exception = new TwitterRateLimitException("Rate limit exceeded");

        // Assert
        await Assert.That(exception.StatusCode).IsEqualTo(HttpStatusCode.TooManyRequests);
    }

        /// <summary>
    /// TwitterRateLimitException_ShouldStoreRetryAfter method.
    /// </summary>
[Test]
    public async Task TwitterRateLimitException_ShouldStoreRetryAfter()
    {
        // Arrange
        var retryAfter = TimeSpan.FromMinutes(15);

        // Act
        var exception = new TwitterRateLimitException("Rate limit exceeded", retryAfter);

        // Assert
        await Assert.That(exception.RetryAfter).IsEqualTo(retryAfter);
    }
}

/// <summary>
/// Represents a class for TwitterAuthenticationExceptionTests.
/// </summary>
public class TwitterAuthenticationExceptionTests
{
        /// <summary>
    /// TwitterAuthenticationException_ShouldInheritFromTwitterApiException method.
    /// </summary>
[Test]
    public async Task TwitterAuthenticationException_ShouldInheritFromTwitterApiException()
    {
        // Act
        var exception = new TwitterAuthenticationException("Authentication failed");

        // Assert
        await Assert.That(exception).IsAssignableTo<TwitterApiException>();
    }

        /// <summary>
    /// TwitterAuthenticationException_ShouldHave401StatusCode method.
    /// </summary>
[Test]
    public async Task TwitterAuthenticationException_ShouldHave401StatusCode()
    {
        // Act
        var exception = new TwitterAuthenticationException("Authentication failed");

        // Assert
        await Assert.That(exception.StatusCode).IsEqualTo(HttpStatusCode.Unauthorized);
}
}