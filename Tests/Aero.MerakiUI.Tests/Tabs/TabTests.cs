using TUnit.Core;
using Aero.MerakiUI.Tabs;
using Bunit;
using Aero.MerakiUI.Tabs;

namespace Aero.MerakiUI.Tests.Tabs;

/// <summary>
/// Represents a class for TabTests.
/// </summary>
public class TabTests : BunitContext
{
        /// <summary>
    /// SimpleTabs_ShouldRenderCorrectStructure method.
    /// </summary>
[Test]
    public void SimpleTabs_ShouldRenderCorrectStructure()
    {
        var cut = Render<SimpleTabs>(parameters => parameters
            .Add(p => p.Tabs, new[] { "Account", "Company", "Team", "Billing" })
            .AddChildContent("<div x-show='activeTab === 0'>Account Content</div><div x-show='activeTab === 1'>Company Content</div>")
        );

        // Verify Alpine data initialization (activeTab state)
        var container = cut.Find("div[x-data]");
        
        // Verify tab buttons are rendered
        var buttons = cut.FindAll("button");
        Assert.Equal(4, buttons.Count);
        Assert.Contains("Account", buttons[0].TextContent);
        
        // Verify content container
        Assert.Contains("Account Content", cut.Markup);
    }

        /// <summary>
    /// TabWithIcons_ShouldRenderCorrectStructure method.
    /// </summary>
[Test]
    public void TabWithIcons_ShouldRenderCorrectStructure()
    {
        var items = new List<Aero.MerakiUI.Tabs.TabWithIcons.TabItem>
        {
            new() { Title = "Profile", Icon = "<svg>...</svg>" },
            new() { Title = "Dashboard", Icon = "<svg>...</svg>" }
        };

        var cut = Render<TabWithIcons>(parameters => parameters
            .Add(p => p.Items, items)
            .AddChildContent("<div>Content</div>")
        );

        // Verify Alpine data initialization
        var container = cut.Find("div[x-data]");
        
        // Verify tab buttons
        var buttons = cut.FindAll("button");
        Assert.Equal(2, buttons.Count);
        Assert.Contains("Profile", buttons[0].TextContent);
        Assert.Contains("<svg>...</svg>", buttons[0].InnerHtml);
}
}
