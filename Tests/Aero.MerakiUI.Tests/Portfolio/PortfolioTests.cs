using TUnit.Core;
using Aero.MerakiUI.Portfolio;
using Bunit;
using Aero.MerakiUI.Portfolio;

namespace Aero.MerakiUI.Tests.Portfolio;

/// <summary>
/// Represents a class for PortfolioTests.
/// </summary>
public class PortfolioTests : BunitContext
{
        /// <summary>
    /// PortfolioCard_ShouldRenderTitle method.
    /// </summary>
[Test]
    public void PortfolioCard_ShouldRenderTitle()
    {
        var cut = Render<PortfolioCard>(parameters => parameters
            .Add(p => p.Title, "Project X")
        );

        Assert.Contains("Project X", cut.Markup);
}
}
