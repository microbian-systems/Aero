using TUnit.Core;
using System.Net;
using Aero.Social.Twitter.Client.Exceptions;
using System.Threading.Tasks;

namespace Aero.Social.Twitter.Exceptions;

public class TwitterApiExceptionTests
{
    [Test]
    public async Task TwitterApiException_DefaultConstructor_ShouldCreateException()
    {
        // Act
        var exception = new TwitterApiException();

        // Assert
        Assert.NotNull(exception);
        await Assert.That(exception).IsTypeOf<TwitterApiException>();
    }

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

public class TwitterRateLimitExceptionTests
{
    [Test]
    public async Task TwitterRateLimitException_ShouldInheritFromTwitterApiException()
    {
        // Act
        var exception = new TwitterRateLimitException("Rate limit exceeded");

        // Assert
        await Assert.That(exception).IsAssignableTo<TwitterApiException>();
    }

    [Test]
    public async Task TwitterRateLimitException_ShouldHave429StatusCode()
    {
        // Act
        var exception = new TwitterRateLimitException("Rate limit exceeded");

        // Assert
        await Assert.That(exception.StatusCode).IsEqualTo(HttpStatusCode.TooManyRequests);
    }

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

public class TwitterAuthenticationExceptionTests
{
    [Test]
    public async Task TwitterAuthenticationException_ShouldInheritFromTwitterApiException()
    {
        // Act
        var exception = new TwitterAuthenticationException("Authentication failed");

        // Assert
        await Assert.That(exception).IsAssignableTo<TwitterApiException>();
    }

    [Test]
    public async Task TwitterAuthenticationException_ShouldHave401StatusCode()
    {
        // Act
        var exception = new TwitterAuthenticationException("Authentication failed");

        // Assert
        await Assert.That(exception.StatusCode).IsEqualTo(HttpStatusCode.Unauthorized);
}
}