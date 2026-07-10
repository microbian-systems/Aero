using TUnit.Core;
using Aero.MerakiUI.Cookies;
using Bunit;
using Aero.MerakiUI.Cookies;

namespace Aero.MerakiUI.Tests.Cookies;

/// <summary>
/// Represents a class for CookieTests.
/// </summary>
public class CookieTests : BunitContext
{
        /// <summary>
    /// CookieBanner_ShouldRenderMessage method.
    /// </summary>
[Test]
    public void CookieBanner_ShouldRenderMessage()
    {
        var cut = Render<CookieBanner>(parameters => parameters
            .Add(p => p.Message, "Test Cookie Message")
        );

        Assert.Contains("Test Cookie Message", cut.Markup);
}
}
