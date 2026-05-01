using TUnit.Core;
using Aero.MerakiUI.Auth;
using Bunit;
using Aero.MerakiUI.Auth;

namespace Aero.MerakiUI.Tests.Auth;

public class AuthTests : BunitContext
{
    [Test]
    public void SignInCard_ShouldRenderTitle()
    {
        var cut = Render<SignInCard>(parameters => parameters
            .Add(p => p.Title, "Login Now")
        );

        Assert.Contains("Login Now", cut.Markup);
    }

    [Test]
    public void SignUpCard_ShouldRenderSignInUrl()
    {
        var cut = Render<SignUpCard>(parameters => parameters
            .Add(p => p.SignInUrl, "/login")
        );

        Assert.Contains("href=\"/login\"", cut.Markup);
}
}
