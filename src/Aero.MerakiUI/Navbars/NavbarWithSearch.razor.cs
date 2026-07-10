using Microsoft.AspNetCore.Components;

namespace Aero.MerakiUI.Navbars;

/// <summary>
/// Represents a class for NavbarWithSearch.
/// </summary>
public partial class NavbarWithSearch : MerakiComponentBase
{
        /// <summary>
    /// Gets or sets the Brand Name.
    /// </summary>
[Parameter]
    public string? BrandName { get; set; }

        /// <summary>
    /// Gets or sets the Brand Href.
    /// </summary>
[Parameter]
    public string? BrandHref { get; set; } = "/";

        /// <summary>
    /// Gets or sets the Links.
    /// </summary>
[Parameter]
    public RenderFragment? Links { get; set; }

        /// <summary>
    /// Gets or sets the Search Placeholder.
    /// </summary>
[Parameter]
    public string? SearchPlaceholder { get; set; } = "Search";

        /// <summary>
    /// Gets or sets the Is Mobile Menu Open.
    /// </summary>
protected bool IsMobileMenuOpen { get; set; }

        /// <summary>
    /// ToggleMobileMenu method.
    /// </summary>
protected void ToggleMobileMenu()
    {
        IsMobileMenuOpen = !IsMobileMenuOpen;
    }
}
