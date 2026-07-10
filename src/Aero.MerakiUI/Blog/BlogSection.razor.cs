using Microsoft.AspNetCore.Components;

namespace Aero.MerakiUI.Blog;

/// <summary>
/// Represents a class for BlogSection.
/// </summary>
public partial class BlogSection : MerakiComponentBase
{
        /// <summary>
    /// Gets or sets the Title.
    /// </summary>
[Parameter]
    public string Title { get; set; } = "From the blog";

        /// <summary>
    /// Gets or sets the Description.
    /// </summary>
[Parameter]
    public string? Description { get; set; }

        /// <summary>
    /// Gets or sets the Is Centered.
    /// </summary>
[Parameter]
    public bool IsCentered { get; set; } = false;

        /// <summary>
    /// Gets or sets the Columns.
    /// </summary>
[Parameter]
    public int Columns { get; set; } = 2;

    private string GetHeaderAlignmentClass() => IsCentered ? "text-center" : "";

    private string GetGridColsClass() => Columns switch
    {
        1 => "grid-cols-1",
        2 => "md:grid-cols-2",
        3 => "md:grid-cols-3",
        _ => "md:grid-cols-2"
    };
}
