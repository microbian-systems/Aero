using TUnit.Core;
using Aero.MerakiUI.Sections;
using Bunit;
using Aero.MerakiUI.Sections;

namespace Aero.MerakiUI.Tests.Sections;

/// <summary>
/// Represents a class for SectionTests.
/// </summary>
public class SectionTests : BunitContext
{
        /// <summary>
    /// PricingSection_ShouldRenderCorrectStructure method.
    /// </summary>
[Test]
    public void PricingSection_ShouldRenderCorrectStructure()
    {
        var cut = Render<PricingSection>(parameters => parameters
            .Add(p => p.Title, "Our Pricing")
        );

        // Verify Title
        Assert.Contains("Our Pricing", cut.Find("h1").TextContent);
        
        // Verify at least one pricing card exists
        var cards = cut.FindAll("div.flex.flex-col");
        Assert.True(cards.Count > 0);
}
}
