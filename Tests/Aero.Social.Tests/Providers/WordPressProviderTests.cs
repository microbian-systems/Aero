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

public class WordPressProviderTests : ProviderTestBase
{
    private readonly Mock<ILogger<WordPressProvider>> _loggerMock = new();

    private WordPressProvider CreateProvider()
    {
        return new WordPressProvider(HttpClient, ConfigurationMock.Object, _loggerMock.Object);
    }

    [Test]
    public void Provider_ShouldHaveCorrectIdentifier()
    {
        var provider = CreateProvider();

        provider.Identifier.ShouldBe("wordpress");
        provider.Name.ShouldBe("WordPress");
        provider.Editor.ShouldBe(EditorType.Html);
        provider.MaxConcurrentJobs.ShouldBe(5);
    }

    [Test]
    public void MaxLength_ShouldReturn100000()
    {
        var provider = CreateProvider();

        provider.MaxLength().ShouldBe(100000);
    }

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
