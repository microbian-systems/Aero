using Microsoft.AspNetCore.Components;

namespace Aero.MerakiUI.Portfolio;

/// <summary>
/// Represents a class for PortfolioCard.
/// </summary>
public partial class PortfolioCard : MerakiComponentBase
{
        /// <summary>
    /// Gets or sets the Title.
    /// </summary>
[Parameter]
    public string Title { get; set; } = "Project Title";

        /// <summary>
    /// Gets or sets the Category.
    /// </summary>
[Parameter]
    public string Category { get; set; } = "Category";

        /// <summary>
    /// Gets or sets the Image Url.
    /// </summary>
[Parameter]
    public string ImageUrl { get; set; } = "https://images.unsplash.com/photo-1621111848501-8d3634f82336?ixlib=rb-1.2.1&ixid=MnwxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8&auto=format&fit=crop&w=1565&q=80";
}
