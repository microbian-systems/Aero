using TUnit.Core;
using Aero.MerakiUI.Sidebars;
using Bunit;
using Aero.MerakiUI.Sidebars;

namespace Aero.MerakiUI.Tests.Sidebars;

/// <summary>
/// Represents a class for SidebarTests.
/// </summary>
public class SidebarTests : BunitContext
{
        /// <summary>
    /// SimpleSidebar_ShouldRenderCorrectStructure method.
    /// </summary>
[Test]
    public void SimpleSidebar_ShouldRenderCorrectStructure()
    {
        var cut = Render<SimpleSidebar>(parameters => parameters
            .Add(p => p.BrandName, "Electra")
            .AddChildContent("<nav>Links</nav>")
        );

        // Verify Brand Name
        Assert.Contains("Electra", cut.Find("h2").TextContent);
        
        // Verify Child Content
        Assert.Contains("Links", cut.Markup);
}
}
