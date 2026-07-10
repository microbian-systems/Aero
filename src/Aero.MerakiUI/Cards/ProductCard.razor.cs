using Microsoft.AspNetCore.Components;

namespace Aero.MerakiUI.Cards;

/// <summary>
/// Represents a class for ProductCard.
/// </summary>
public partial class ProductCard : MerakiComponentBase
{
        /// <summary>
    /// Gets or sets the Title.
    /// </summary>
[Parameter]
    public string Title { get; set; } = "Product Name";

        /// <summary>
    /// Gets or sets the Price.
    /// </summary>
[Parameter]
    public string Price { get; set; } = "$0";

        /// <summary>
    /// Gets or sets the Image Url.
    /// </summary>
[Parameter]
    public string ImageUrl { get; set; } = "";
}
