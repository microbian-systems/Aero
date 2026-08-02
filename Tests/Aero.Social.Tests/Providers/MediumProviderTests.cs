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
/// Represents a class for MediumProviderTests.
/// </summary>
public class MediumProviderTests : ProviderTestBase
{
    private readonly Mock<ILogger<MediumProvider>> _loggerMock = new();

    private MediumProvider CreateProvider()
    {
        return new MediumProvider(HttpClient, ConfigurationMock.Object, _loggerMock.Object);
    }

        /// <summary>
    /// Provider_ShouldHaveCorrectIdentifier method.
    /// </summary>
[Test]
    public void Provider_ShouldHaveCorrectIdentifier()
    {
        var provider = CreateProvider();

        provider.Identifier.ShouldBe("medium");
        provider.Name.ShouldBe("Medium");
        provider.Editor.ShouldBe(EditorType.Markdown);
        provider.MaxConcurrentJobs.ShouldBe(3);
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
