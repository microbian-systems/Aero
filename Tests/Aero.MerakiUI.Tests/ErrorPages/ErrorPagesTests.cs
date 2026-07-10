using TUnit.Core;
using Aero.MerakiUI.ErrorPages;
using Bunit;
using Aero.MerakiUI.ErrorPages;

namespace Aero.MerakiUI.Tests.ErrorPages;

/// <summary>
/// Represents a class for ErrorPagesTests.
/// </summary>
public class ErrorPagesTests : BunitContext
{
        /// <summary>
    /// Simple404_ShouldRender method.
    /// </summary>
[Test]
    public void Simple404_ShouldRender()
    {
        var cut = Render<Simple404>(parameters => parameters
            .Add(p => p.Title, "Page Not Found")
        );

        Assert.Contains("Page Not Found", cut.Markup);
    }

        /// <summary>
    /// Centered404_ShouldRender method.
    /// </summary>
[Test]
    public void Centered404_ShouldRender()
    {
        var cut = Render<Centered404>(parameters => parameters
            .Add(p => p.Title, "Centered Not Found")
        );

        Assert.Contains("Centered Not Found", cut.Markup);
    }

        /// <summary>
    /// Illustration404_ShouldRender method.
    /// </summary>
[Test]
    public void Illustration404_ShouldRender()
    {
        var cut = Render<Illustration404>(parameters => parameters
            .Add(p => p.IllustrationUrl, "test.svg")
        );

        Assert.Contains("src=\"test.svg\"", cut.Markup);
    }

        /// <summary>
    /// Image404_ShouldRender method.
    /// </summary>
[Test]
    public void Image404_ShouldRender()
    {
        var cut = Render<Image404>(parameters => parameters
            .Add(p => p.ImageUrl, "test.jpg")
        );

        Assert.Contains("src=\"test.jpg\"", cut.Markup);
}
}
