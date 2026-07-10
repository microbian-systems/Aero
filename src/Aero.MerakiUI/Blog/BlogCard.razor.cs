using Microsoft.AspNetCore.Components;

namespace Aero.MerakiUI.Blog;

/// <summary>
/// Represents a class for BlogCard.
/// </summary>
public partial class BlogCard : MerakiComponentBase
{
        /// <summary>
    /// Gets or sets the Title.
    /// </summary>
[Parameter]
    public string Title { get; set; } = "Blog Post Title";

        /// <summary>
    /// Gets or sets the Excerpt.
    /// </summary>
[Parameter]
    public string Excerpt { get; set; } = "Lorem ipsum dolor sit amet consectetur adipisicing elit. Iure veritatis sint autem nesciunt...";

        /// <summary>
    /// Gets or sets the Date.
    /// </summary>
[Parameter]
    public string Date { get; set; } = "21 October 2019";

        /// <summary>
    /// Gets or sets the Url.
    /// </summary>
[Parameter]
    public string Url { get; set; } = "#";

        /// <summary>
    /// Gets or sets the Image Url.
    /// </summary>
[Parameter]
    public string ImageUrl { get; set; } = "https://images.unsplash.com/photo-1644018335954-ab54c83e007f?ixlib=rb-1.2.1&ixid=MnwxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8&auto=format&fit=crop&w=1470&q=80";

        /// <summary>
    /// Gets or sets the Image Alt.
    /// </summary>
[Parameter]
    public string ImageAlt { get; set; } = "Blog Post Image";
}
