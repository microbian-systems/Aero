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
/// Represents a class for NostrProviderTests.
/// </summary>
public class NostrProviderTests : ProviderTestBase
{
    private readonly Mock<ILogger<NostrProvider>> _loggerMock = new();

    private NostrProvider CreateProvider()
    {
        return new NostrProvider(HttpClient, ConfigurationMock.Object, _loggerMock.Object);
    }

        /// <summary>
    /// Provider_ShouldHaveCorrectIdentifier method.
    /// </summary>
[Test]
    public void Provider_ShouldHaveCorrectIdentifier()
    {
        var provider = CreateProvider();

        provider.Identifier.ShouldBe("nostr");
        provider.Name.ShouldBe("Nostr");
        provider.IsWeb3.ShouldBeTrue();
        provider.MaxConcurrentJobs.ShouldBe(5);
    }

        /// <summary>
    /// MaxLength_ShouldReturn100000 method.
    /// </summary>
[Test]
    public void MaxLength_ShouldReturn100000()
    {
        var provider = CreateProvider();

        provider.MaxLength().ShouldBe(100000);
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
