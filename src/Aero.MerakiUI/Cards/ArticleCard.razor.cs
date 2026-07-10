using Microsoft.AspNetCore.Components;

namespace Aero.MerakiUI.Cards;

/// <summary>
/// Represents a class for ArticleCard.
/// </summary>
public partial class ArticleCard : MerakiComponentBase
{
        /// <summary>
    /// Gets or sets the Category.
    /// </summary>
[Parameter]
    public string Category { get; set; } = "Category";

        /// <summary>
    /// Gets or sets the Title.
    /// </summary>
[Parameter]
    public string Title { get; set; } = "Title";

        /// <summary>
    /// Gets or sets the Description.
    /// </summary>
[Parameter]
    public string Description { get; set; } = "";

        /// <summary>
    /// Gets or sets the Date.
    /// </summary>
[Parameter]
    public string Date { get; set; } = "";

        /// <summary>
    /// Gets or sets the Href.
    /// </summary>
[Parameter]
    public string Href { get; set; } = "#";
}
