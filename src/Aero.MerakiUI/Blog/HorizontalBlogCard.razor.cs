using Microsoft.AspNetCore.Components;

namespace Aero.MerakiUI.Blog;

/// <summary>
/// Represents a class for HorizontalBlogCard.
/// </summary>
public partial class HorizontalBlogCard : MerakiComponentBase
{
        /// <summary>
    /// Gets or sets the Title.
    /// </summary>
[Parameter]
    public string Title { get; set; } = "Blog Post Title";

        /// <summary>
    /// Gets or sets the Date.
    /// </summary>
[Parameter]
    public string Date { get; set; } = "20 October 2019";

        /// <summary>
    /// Gets or sets the Url.
    /// </summary>
[Parameter]
    public string Url { get; set; } = "#";

        /// <summary>
    /// Gets or sets the Image Url.
    /// </summary>
[Parameter]
    public string ImageUrl { get; set; } = "https://images.unsplash.com/photo-1515378960530-7c0da6231fb1?ixlib=rb-1.2.1&ixid=MnwxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8&auto=format&fit=crop&w=1470&q=80";

        /// <summary>
    /// Gets or sets the Image Alt.
    /// </summary>
[Parameter]
    public string ImageAlt { get; set; } = "Blog Post Image";
}
