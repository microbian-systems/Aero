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

public class PinterestProviderTests : ProviderTestBase
{
    private readonly Mock<ILogger<PinterestProvider>> _loggerMock = new();

    private PinterestProvider CreateProvider()
    {
        SetupConfiguration("PINTEREST_CLIENT_ID", "test_client_id");
        SetupConfiguration("PINTEREST_CLIENT_SECRET", "test_client_secret");
        SetupConfiguration("FRONTEND_URL", "https://localhost");

        return new PinterestProvider(HttpClient, ConfigurationMock.Object, _loggerMock.Object);
    }

    [Test]
    public void Provider_ShouldHaveCorrectIdentifier()
    {
        var provider = CreateProvider();

        provider.Identifier.ShouldBe("pinterest");
        provider.Name.ShouldBe("Pinterest");
        provider.MaxConcurrentJobs.ShouldBe(3);
    }

    [Test]
    public void MaxLength_ShouldReturn500()
    {
        var provider = CreateProvider();

        provider.MaxLength().ShouldBe(500);
    }

    [Test]
    public async Task GenerateAuthUrlAsync_ShouldReturnValidUrl()
    {
        var provider = CreateProvider();

        var authResult = await provider.GenerateAuthUrlAsync();
        authResult.IsSuccess.ShouldBeTrue();
        var result = ((Result<GenerateAuthUrlResponse, AeroError>.Ok)authResult).Value;

        result.Url.ShouldContain("pinterest.com/oauth");
        result.Url.ShouldContain("client_id=test_client_id");
        result.Url.ShouldContain("redirect_uri=");
        result.Url.ShouldContain("response_type=code");
        result.State.ShouldNotBeNullOrEmpty();
}
}
