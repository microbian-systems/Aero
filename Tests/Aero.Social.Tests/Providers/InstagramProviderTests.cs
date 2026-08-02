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
/// Represents a class for InstagramProviderTests.
/// </summary>
public class InstagramProviderTests : ProviderTestBase
{
    private readonly Mock<ILogger<InstagramProvider>> _loggerMock = new();
    
    private InstagramProvider CreateProvider()
    {
        SetupConfiguration("FACEBOOK_APP_ID", "test_app_id");
        SetupConfiguration("FACEBOOK_APP_SECRET", "test_app_secret");
        SetupConfiguration("FRONTEND_URL", "https://localhost");
        
        return new InstagramProvider(HttpClient, ConfigurationMock.Object, _loggerMock.Object);
    }

        /// <summary>
    /// Provider_ShouldHaveCorrectIdentifier method.
    /// </summary>
[Test]
    public void Provider_ShouldHaveCorrectIdentifier()
    {
        var provider = CreateProvider();
        
        provider.Identifier.ShouldBe("instagram");
        provider.Name.ShouldBe("Instagram (Facebook Business)");
        provider.IsBetweenSteps.ShouldBeTrue();
        provider.MaxConcurrentJobs.ShouldBe(200);
    }

        /// <summary>
    /// MaxLength_ShouldReturn2200 method.
    /// </summary>
[Test]
    public void MaxLength_ShouldReturn2200()
    {
        var provider = CreateProvider();
        
        provider.MaxLength().ShouldBe(2200);
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

        result.Url.ShouldContain("facebook.com");
        result.Url.ShouldContain("client_id=test_app_id");
        result.Url.ShouldContain("redirect_uri=");
        result.Url.ShouldContain("scope=");
        result.State.ShouldNotBeNullOrEmpty();
}
}
