using TUnit.Core;
using Aero.MerakiUI.Blog;
using Bunit;
using Aero.MerakiUI.Blog;

namespace Aero.MerakiUI.Tests.Blog;

/// <summary>
/// Represents a class for BlogTests.
/// </summary>
public class BlogTests : BunitContext
{
        /// <summary>
    /// BlogCard_ShouldRender method.
    /// </summary>
[Test]
    public void BlogCard_ShouldRender()
    {
        var cut = Render<BlogCard>(parameters => parameters
            .Add(p => p.Title, "Awesome Post")
        );

        Assert.Contains("Awesome Post", cut.Markup);
    }

        /// <summary>
    /// BlogSection_ShouldRenderTitle method.
    /// </summary>
[Test]
    public void BlogSection_ShouldRenderTitle()
    {
        var cut = Render<BlogSection>(parameters => parameters
            .Add(p => p.Title, "Latest News")
        );

        Assert.Contains("Latest News", cut.Markup);
}
}
