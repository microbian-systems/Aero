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

public class FarcasterProviderTests : ProviderTestBase
{
    private readonly Mock<ILogger<FarcasterProvider>> _loggerMock = new();

    private FarcasterProvider CreateProvider()
    {
        SetupConfiguration("NEYNAR_CLIENT_ID", "test_client_id");
        SetupConfiguration("NEYNAR_CLIENT_SECRET", "test_client_secret");

        return new FarcasterProvider(HttpClient, ConfigurationMock.Object, _loggerMock.Object);
    }

    [Test]
    public void Provider_ShouldHaveCorrectIdentifier()
    {
        var provider = CreateProvider();

        provider.Identifier.ShouldBe("wrapcast");
        provider.Name.ShouldBe("Farcaster");
        provider.IsWeb3.ShouldBeTrue();
        provider.MaxConcurrentJobs.ShouldBe(3);
    }

    [Test]
    public void MaxLength_ShouldReturn800()
    {
        var provider = CreateProvider();

        provider.MaxLength().ShouldBe(800);
    }

    [Test]
    public async Task GenerateAuthUrlAsync_ShouldReturnClientIdAndState()
    {
        var provider = CreateProvider();

        var authResult = await provider.GenerateAuthUrlAsync();
        authResult.IsSuccess.ShouldBeTrue();
        var result = ((Result<GenerateAuthUrlResponse, AeroError>.Ok)authResult).Value;

        result.Url.ShouldContain("test_client_id");
        result.State.ShouldNotBeNullOrEmpty();
}
}
