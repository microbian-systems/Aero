using TUnit.Core;
using System.Net;
using Aero.Core;
using Aero.Core.Railway;
using Aero.Social.Abstractions;
using Aero.Social.Models;
using Aero.Social.Providers;
using Aero.Social.Tests.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Aero.Social.Tests.Providers;

/// <summary>
/// Represents a class for RedditProviderTests.
/// </summary>
public class RedditProviderTests : ProviderTestBase
{
    private readonly Mock<ILogger<RedditProvider>> _loggerMock = new();

    private RedditProvider CreateProvider()
    {
        SetupConfiguration("REDDIT_CLIENT_ID", "test_client_id");
        SetupConfiguration("REDDIT_CLIENT_SECRET", "test_client_secret");
        SetupConfiguration("FRONTEND_URL", "https://localhost");

        return new RedditProvider(HttpClient, ConfigurationMock.Object, _loggerMock.Object);
    }

        /// <summary>
    /// Provider_ShouldHaveCorrectIdentifier method.
    /// </summary>
[Test]
    public void Provider_ShouldHaveCorrectIdentifier()
    {
        var provider = CreateProvider();

        provider.Identifier.ShouldBe("reddit");
        provider.Name.ShouldBe("Reddit");
        provider.Scopes.ShouldBe(new[] { "read", "identity", "submit", "flair" });
        provider.MaxConcurrentJobs.ShouldBe(1);
    }

        /// <summary>
    /// MaxLength_ShouldReturn10000 method.
    /// </summary>
[Test]
    public void MaxLength_ShouldReturn10000()
    {
        var provider = CreateProvider();

        provider.MaxLength().ShouldBe(10000);
    }

        /// <summary>
    /// GenerateAuthUrlAsync_ShouldReturnValidUrl method.
    /// </summary>
[Test]
    public async Task GenerateAuthUrlAsync_ShouldReturnValidUrl()
    {
        var provider = CreateProvider();

        var authResult = await provider.GenerateAuthUrlAsync();
        authResult.IsSuccess.ShouldBeTrue();
        var result = ((Result<GenerateAuthUrlResponse, AeroError>.Ok)authResult).Value;

        result.Url.ShouldContain("reddit.com/api/v1/authorize");
        result.Url.ShouldContain("client_id=test_client_id");
        result.Url.ShouldContain("redirect_uri=");
        result.Url.ShouldContain("duration=permanent");
        result.State.ShouldNotBeNullOrEmpty();
}
}
