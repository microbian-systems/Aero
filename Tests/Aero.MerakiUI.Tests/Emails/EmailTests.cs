using TUnit.Core;
using Aero.MerakiUI.Emails;
using Bunit;
using Aero.MerakiUI.Emails;

namespace Aero.MerakiUI.Tests.Emails;

/// <summary>
/// Represents a class for EmailTests.
/// </summary>
public class EmailTests : BunitContext
{
        /// <summary>
    /// EmailVerification_ShouldRenderCode method.
    /// </summary>
[Test]
    public void EmailVerification_ShouldRenderCode()
    {
        var cut = Render<EmailVerification>(parameters => parameters
            .Add(p => p.VerificationCode, "1234")
        );

        Assert.Contains("1", cut.Markup);
        Assert.Contains("2", cut.Markup);
        Assert.Contains("3", cut.Markup);
        Assert.Contains("4", cut.Markup);
}
}
