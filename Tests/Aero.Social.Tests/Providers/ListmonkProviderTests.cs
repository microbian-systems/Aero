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
/// Represents a class for ListmonkProviderTests.
/// </summary>
public class ListmonkProviderTests : ProviderTestBase
{
    private readonly Mock<ILogger<ListmonkProvider>> _loggerMock = new();

    private ListmonkProvider CreateProvider()
    {
        return new ListmonkProvider(HttpClient, ConfigurationMock.Object, _loggerMock.Object);
    }

        /// <summary>
    /// Provider_ShouldHaveCorrectIdentifier method.
    /// </summary>
[Test]
    public void Provider_ShouldHaveCorrectIdentifier()
    {
        var provider = CreateProvider();

        provider.Identifier.ShouldBe("listmonk");
        provider.Name.ShouldBe("ListMonk");
        provider.Editor.ShouldBe(EditorType.Html);
        provider.MaxConcurrentJobs.ShouldBe(100);
    }

        /// <summary>
    /// MaxLength_ShouldReturn100000000 method.
    /// </summary>
[Test]
    public void MaxLength_ShouldReturn100000000()
    {
        var provider = CreateProvider();

        provider.MaxLength().ShouldBe(100000000);
}
}
