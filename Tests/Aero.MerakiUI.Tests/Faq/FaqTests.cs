using TUnit.Core;
using Aero.MerakiUI.Faq;
using Bunit;
using Aero.MerakiUI.Faq;

namespace Aero.MerakiUI.Tests.Faq;

/// <summary>
/// Represents a class for FaqTests.
/// </summary>
public class FaqTests : BunitContext
{
        /// <summary>
    /// FaqAccordion_ShouldRenderTitle method.
    /// </summary>
[Test]
    public void FaqAccordion_ShouldRenderTitle()
    {
        var cut = Render<FaqAccordion>(parameters => parameters
            .Add(p => p.Title, "Help Center")
        );

        Assert.Contains("Help Center", cut.Markup);
    }

        /// <summary>
    /// FaqItem_ShouldRenderQuestion method.
    /// </summary>
[Test]
    public void FaqItem_ShouldRenderQuestion()
    {
        var cut = Render<FaqItem>(parameters => parameters
            .Add(p => p.Question, "Is it free?")
            .Add(p => p.Answer, "Yes, absolutely.")
        );

        Assert.Contains("Is it free?", cut.Markup);
        Assert.Contains("Yes, absolutely.", cut.Markup);
}
}
