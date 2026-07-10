using TUnit.Core;
using Aero.MerakiUI.Pagination;
using Bunit;
using Aero.MerakiUI.Pagination;

namespace Aero.MerakiUI.Tests.Pagination;

/// <summary>
/// Represents a class for PaginationTests.
/// </summary>
public class PaginationTests : BunitContext
{
        /// <summary>
    /// Pagination_ShouldRenderPages method.
    /// </summary>
[Test]
    public void Pagination_ShouldRenderPages()
    {
        var cut = Render<PaginationControl>(parameters => parameters
            .Add(p => p.TotalPages, 3)
            .Add(p => p.CurrentPage, 1)
        );

        Assert.Contains("1", cut.Markup);
        Assert.Contains("2", cut.Markup);
        Assert.Contains("3", cut.Markup);
}
}
