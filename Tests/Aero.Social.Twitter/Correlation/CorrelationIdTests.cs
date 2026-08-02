using TUnit.Core;
using Aero.Social.Twitter.Client.Correlation;
using Aero.Social.Twitter.Client.Logging;
using System.Threading.Tasks;

namespace Aero.Social.Twitter.Correlation;

/// <summary>
/// Represents a class for CorrelationIdProviderTests.
/// </summary>
public class CorrelationIdProviderTests
{
        /// <summary>
    /// GuidCorrelationIdProvider_ShouldGenerateUniqueIds method.
    /// </summary>
[Test]
    public async Task GuidCorrelationIdProvider_ShouldGenerateUniqueIds()
    {
        // Arrange
        var provider = new GuidCorrelationIdProvider();

        // Act
        var id1 = provider.GenerateCorrelationId();
        var id2 = provider.GenerateCorrelationId();

        // Assert
        await Assert.That(id2).IsNotEqualTo(id1);
        await Assert.That(id1.Length).IsEqualTo(16); // Should be 16 chars
    }

        /// <summary>
    /// GuidCorrelationIdProvider_ShouldGenerateValidIds method.
    /// </summary>
[Test]
    public async Task GuidCorrelationIdProvider_ShouldGenerateValidIds()
    {
        // Arrange
        var provider = new GuidCorrelationIdProvider();

        // Act
        var id = provider.GenerateCorrelationId();

        // Assert
        Assert.NotNull(id);
        await Assert.That(id).IsNotEmpty();
        await Assert.That(id).DoesNotContain("-"); // Should not contain hyphens
    }
}

/// <summary>
/// Represents a class for CorrelationIdHandlerTests.
/// </summary>
public class CorrelationIdHandlerTests
{
        /// <summary>
    /// SendAsync_ShouldAddCorrelationIdHeader method.
    /// </summary>
[Test]
    public async Task SendAsync_ShouldAddCorrelationIdHeader()
    {
        // Arrange
        var provider = new GuidCorrelationIdProvider();
        var handler = new CorrelationIdHandler(provider)
        {
            InnerHandler = new TestHandler()
        };

        var client = new HttpClient(handler);
        var request = new HttpRequestMessage(HttpMethod.Get, "https://api.twitter.com/test");

        // Act
        var response = await client.SendAsync(request);

        // Assert
        await Assert.That(request.Headers.Contains("X-Correlation-Id")).IsTrue();
        var correlationId = request.Headers.GetValues("X-Correlation-Id").FirstOrDefault();
        Assert.NotNull(correlationId);
        await Assert.That(correlationId).IsNotEmpty();
    }

        /// <summary>
    /// SendAsync_ShouldNotOverwriteExistingCorrelationId method.
    /// </summary>
[Test]
    public async Task SendAsync_ShouldNotOverwriteExistingCorrelationId()
    {
        // Arrange
        var provider = new GuidCorrelationIdProvider();
        var handler = new CorrelationIdHandler(provider)
        {
            InnerHandler = new TestHandler()
        };

        var client = new HttpClient(handler);
        var request = new HttpRequestMessage(HttpMethod.Get, "https://api.twitter.com/test");
        request.Headers.Add("X-Correlation-Id", "existing-id-123");

        // Act
        var response = await client.SendAsync(request);

        // Assert
        var correlationId = request.Headers.GetValues("X-Correlation-Id").FirstOrDefault();
        await Assert.That(correlationId).IsEqualTo("existing-id-123");
    }

    private class TestHandler : HttpMessageHandler
    {
                /// <summary>
        /// SendAsync method.
        /// </summary>
protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK));
}
    }
}