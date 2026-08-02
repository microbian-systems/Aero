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
/// Represents a class for MastodonProviderTests.
/// </summary>
public class MastodonProviderTests : ProviderTestBase
{
    private readonly Mock<ILogger<MastodonProvider>> _loggerMock = new();
    
    private MastodonProvider CreateProvider()
    {
        SetupConfiguration("MASTODON_CLIENT_ID", "test_client_id");
        SetupConfiguration("MASTODON_CLIENT_SECRET", "test_client_secret");
        SetupConfiguration("FRONTEND_URL", "https://localhost");
        
        return new MastodonProvider(HttpClient, ConfigurationMock.Object, _loggerMock.Object);
    }

        /// <summary>
    /// Provider_ShouldHaveCorrectIdentifier method.
    /// </summary>
[Test]
    public void Provider_ShouldHaveCorrectIdentifier()
    {
        var provider = CreateProvider();
        
        provider.Identifier.ShouldBe("mastodon");
        provider.Name.ShouldBe("Mastodon");
        provider.MaxConcurrentJobs.ShouldBe(5);
    }

        /// <summary>
    /// MaxLength_ShouldReturn500 method.
    /// </summary>
[Test]
    public void MaxLength_ShouldReturn500()
    {
        var provider = CreateProvider();
        
        provider.MaxLength().ShouldBe(500);
    }

        /// <summary>
    /// GenerateAuthUrlAsync_ShouldReturnValidUrl method.
    /// </summary>
[Test]
    public async Task GenerateAuthUrlAsync_ShouldReturnValidUrl()
    {
        HttpHandler.WhenGet("*instance*")
            .RespondWith(MockResponses.Mastodon.InstanceResponse("mastodon.social"));

        var provider = CreateProvider();
        var clientInfo = new ClientInformation { InstanceUrl = "https://mastodon.social" };
        
        var authResult = await provider.GenerateAuthUrlAsync(clientInfo);
        authResult.IsSuccess.ShouldBeTrue();
        var result = ((Result<GenerateAuthUrlResponse, AeroError>.Ok)authResult).Value;

        result.Url.ShouldContain("mastodon.social/oauth/authorize");
        result.Url.ShouldContain("client_id=");
        result.Url.ShouldContain("redirect_uri=");
        result.State.ShouldNotBeNullOrEmpty();
}
}
