using TUnit.Core;
using Aero.MerakiUI.Buttons;
using Bunit;
using Aero.MerakiUI.Buttons;

namespace Aero.MerakiUI.Tests.Buttons;

/// <summary>
/// Represents a class for ButtonTests.
/// </summary>
public class ButtonTests : BunitContext
{
        /// <summary>
    /// PrimaryButton_ShouldRenderCorrectClasses method.
    /// </summary>
[Test]
    public void PrimaryButton_ShouldRenderCorrectClasses()
    {
        var cut = Render<PrimaryButton>(parameters => parameters
            .AddChildContent("Click Me")
        );

        cut.Find("button.bg-blue-600");
        Assert.Contains("Click Me", cut.Markup);
}
}
