using TUnit.Core;
using Aero.MerakiUI.Testimonials;
using Bunit;
using Aero.MerakiUI.Testimonials;

namespace Aero.MerakiUI.Tests.Testimonials;

/// <summary>
/// Represents a class for TestimonialTests.
/// </summary>
public class TestimonialTests : BunitContext
{
        /// <summary>
    /// TestimonialCard_ShouldRenderContent method.
    /// </summary>
[Test]
    public void TestimonialCard_ShouldRenderContent()
    {
        var cut = Render<TestimonialCard>(parameters => parameters
            .Add(p => p.Content, "Amazing service!")
        );

        Assert.Contains("Amazing service!", cut.Markup);
}
}
