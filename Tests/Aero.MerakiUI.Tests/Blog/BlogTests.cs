using TUnit.Core;
using Aero.MerakiUI.Blog;
using Bunit;
using Aero.MerakiUI.Blog;

namespace Aero.MerakiUI.Tests.Blog;

public class BlogTests : BunitContext
{
    [Test]
    public void BlogCard_ShouldRender()
    {
        var cut = Render<BlogCard>(parameters => parameters
            .Add(p => p.Title, "Awesome Post")
        );

        Assert.Contains("Awesome Post", cut.Markup);
    }

    [Test]
    public void BlogSection_ShouldRenderTitle()
    {
        var cut = Render<BlogSection>(parameters => parameters
            .Add(p => p.Title, "Latest News")
        );

        Assert.Contains("Latest News", cut.Markup);
}
}
