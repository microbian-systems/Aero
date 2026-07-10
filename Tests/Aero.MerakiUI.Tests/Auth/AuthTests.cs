using TUnit.Core;
using Aero.MerakiUI.Auth;
using Bunit;
using Aero.MerakiUI.Auth;

namespace Aero.MerakiUI.Tests.Auth;

/// <summary>
/// Represents a class for AuthTests.
/// </summary>
public class AuthTests : BunitContext
{
        /// <summary>
    /// SignInCard_ShouldRenderTitle method.
    /// </summary>
[Test]
    public void SignInCard_ShouldRenderTitle()
    {
        var cut = Render<SignInCard>(parameters => parameters
            .Add(p => p.Title, "Login Now")
        );

        Assert.Contains("Login Now", cut.Markup);
    }

        /// <summary>
    /// SignUpCard_ShouldRenderSignInUrl method.
    /// </summary>
[Test]
    public void SignUpCard_ShouldRenderSignInUrl()
    {
        var cut = Render<SignUpCard>(parameters => parameters
            .Add(p => p.SignInUrl, "/login")
        );

        Assert.Contains("href=\"/login\"", cut.Markup);
}
}
