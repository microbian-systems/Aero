using TUnit.Core;
using Aero.MerakiUI.Breadcrumbs;
using Bunit;
using Aero.MerakiUI.Breadcrumbs;

namespace Aero.MerakiUI.Tests.Breadcrumbs;

/// <summary>
/// Represents a class for BreadcrumbTests.
/// </summary>
public class BreadcrumbTests : BunitContext
{
        /// <summary>
    /// Breadcrumb_ShouldRenderItems method.
    /// </summary>
[Test]
    public void Breadcrumb_ShouldRenderItems()
    {
        var cut = Render<Breadcrumb>(parameters => parameters
            .AddChildContent<BreadcrumbItem>(p => p.Add(i => i.Text, "Home").Add(i => i.ShowSeparator, false))
            .AddChildContent<BreadcrumbItem>(p => p.Add(i => i.Text, "Profile"))
        );

        Assert.Contains("Home", cut.Markup);
        Assert.Contains("Profile", cut.Markup);
}
}
