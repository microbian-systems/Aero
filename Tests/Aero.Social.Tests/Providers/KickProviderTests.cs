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
/// Represents a class for KickProviderTests.
/// </summary>
public class KickProviderTests : ProviderTestBase
{
    private readonly Mock<ILogger<KickProvider>> _loggerMock = new();

    private KickProvider CreateProvider()
    {
        SetupConfiguration("KICK_CLIENT_ID", "test_client_id");
        SetupConfiguration("KICK_CLIENT_SECRET", "test_client_secret");
        SetupConfiguration("FRONTEND_URL", "https://localhost");

        return new KickProvider(HttpClient, ConfigurationMock.Object, _loggerMock.Object);
    }

        /// <summary>
    /// Provider_ShouldHaveCorrectIdentifier method.
    /// </summary>
[Test]
    public void Provider_ShouldHaveCorrectIdentifier()
    {
        var provider = CreateProvider();

        provider.Identifier.ShouldBe("kick");
        provider.Name.ShouldBe("Kick");
        provider.MaxConcurrentJobs.ShouldBe(3);
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
        var provider = CreateProvider();

        var authResult = await provider.GenerateAuthUrlAsync();
        authResult.IsSuccess.ShouldBeTrue();
        var result = ((Result<GenerateAuthUrlResponse, AeroError>.Ok)authResult).Value;

        result.Url.ShouldContain("id.kick.com/oauth/authorize");
        result.Url.ShouldContain("client_id=test_client_id");
        result.Url.ShouldContain("redirect_uri=");
        result.Url.ShouldContain("code_challenge=");
        result.State.ShouldNotBeNullOrEmpty();
}
}
