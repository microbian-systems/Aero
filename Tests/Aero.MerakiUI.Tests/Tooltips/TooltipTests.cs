using TUnit.Core;
using Aero.MerakiUI.Tooltips;
using Bunit;
using Aero.MerakiUI.Tooltips;

namespace Aero.MerakiUI.Tests.Tooltips;

/// <summary>
/// Represents a class for TooltipTests.
/// </summary>
public class TooltipTests : BunitContext
{
        /// <summary>
    /// Tooltip_ShouldRenderText method.
    /// </summary>
[Test]
    public void Tooltip_ShouldRenderText()
    {
        var cut = Render<Tooltip>(parameters => parameters
            .Add(p => p.Text, "Hint info")
        );

        Assert.Contains("Hint info", cut.Markup);
}
}
