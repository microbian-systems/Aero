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
/// Represents a class for LemmyProviderTests.
/// </summary>
public class LemmyProviderTests : ProviderTestBase
{
    private readonly Mock<ILogger<LemmyProvider>> _loggerMock = new();

    private LemmyProvider CreateProvider()
    {
        return new LemmyProvider(HttpClient, ConfigurationMock.Object, _loggerMock.Object);
    }

        /// <summary>
    /// Provider_ShouldHaveCorrectIdentifier method.
    /// </summary>
[Test]
    public void Provider_ShouldHaveCorrectIdentifier()
    {
        var provider = CreateProvider();

        provider.Identifier.ShouldBe("lemmy");
        provider.Name.ShouldBe("Lemmy");
        provider.MaxConcurrentJobs.ShouldBe(3);
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
    /// GenerateAuthUrlAsync_ShouldReturnEmptyUrl method.
    /// </summary>
[Test]
    public async Task GenerateAuthUrlAsync_ShouldReturnEmptyUrl()
    {
        var provider = CreateProvider();

        var authResult = await provider.GenerateAuthUrlAsync();
        authResult.IsSuccess.ShouldBeTrue();
        var result = ((Result<GenerateAuthUrlResponse, AeroError>.Ok)authResult).Value;

        result.Url.ShouldBeEmpty();
        result.State.ShouldNotBeNullOrEmpty();
}
}
