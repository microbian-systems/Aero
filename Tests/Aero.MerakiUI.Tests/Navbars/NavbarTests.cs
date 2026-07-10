using TUnit.Core;
using Aero.MerakiUI.Navbars;
using Bunit;
using Aero.MerakiUI.Navbars;

namespace Aero.MerakiUI.Tests.Navbars;

/// <summary>
/// Represents a class for NavbarTests.
/// </summary>
public class NavbarTests : BunitContext
{
        /// <summary>
    /// SimpleNavbar_ShouldRenderCorrectClasses method.
    /// </summary>
[Test]
    public void SimpleNavbar_ShouldRenderCorrectClasses()
    {
        var cut = Render<SimpleNavbar>(parameters => parameters
            .Add(p => p.BrandName, "MyBrand")
        );

        Assert.Contains("MyBrand", cut.Markup);
        cut.Find("nav");
}
}
