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
/// Represents a class for VkProviderTests.
/// </summary>
public class VkProviderTests : ProviderTestBase
{
    private readonly Mock<ILogger<VkProvider>> _loggerMock = new();

    private VkProvider CreateProvider()
    {
        SetupConfiguration("VK_ID", "test_client_id");
        SetupConfiguration("VK_CLIENT_SECRET", "test_client_secret");
        SetupConfiguration("FRONTEND_URL", "https://localhost");

        return new VkProvider(HttpClient, ConfigurationMock.Object, _loggerMock.Object);
    }

        /// <summary>
    /// Provider_ShouldHaveCorrectIdentifier method.
    /// </summary>
[Test]
    public void Provider_ShouldHaveCorrectIdentifier()
    {
        var provider = CreateProvider();

        provider.Identifier.ShouldBe("vk");
        provider.Name.ShouldBe("VK");
        provider.MaxConcurrentJobs.ShouldBe(2);
    }

        /// <summary>
    /// MaxLength_ShouldReturn2048 method.
    /// </summary>
[Test]
    public void MaxLength_ShouldReturn2048()
    {
        var provider = CreateProvider();

        provider.MaxLength().ShouldBe(2048);
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

        result.Url.ShouldContain("id.vk.com");
        result.Url.ShouldContain("client_id=test_client_id");
        result.Url.ShouldContain("redirect_uri=");
        result.State.ShouldNotBeNullOrEmpty();
}
}
