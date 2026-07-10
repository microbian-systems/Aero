using TUnit.Core;
using Aero.MerakiUI.Footers;
using Bunit;
using Aero.MerakiUI.Footers;

namespace Aero.MerakiUI.Tests.Footers;

/// <summary>
/// Represents a class for FooterTests.
/// </summary>
public class FooterTests : BunitContext
{
        /// <summary>
    /// SimpleFooter_ShouldRenderCorrectStructure method.
    /// </summary>
[Test]
    public void SimpleFooter_ShouldRenderCorrectStructure()
    {
        var cut = Render<SimpleFooter>(parameters => parameters
            .Add(p => p.BrandName, "Electra")
            .AddChildContent("<nav>Links</nav>")
        );

        // Verify Brand Name
        Assert.Contains("Electra", cut.Find("a").TextContent);
        
        // Verify Child Content
        Assert.Contains("Links", cut.Markup);
}
}
