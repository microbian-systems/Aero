using Microsoft.AspNetCore.Components;

namespace Aero.MerakiUI.Pagination;

/// <summary>
/// Represents a class for PaginationControl.
/// </summary>
public partial class PaginationControl : MerakiComponentBase
{
        /// <summary>
    /// Gets or sets the Current Page.
    /// </summary>
[Parameter]
    public int CurrentPage { get; set; } = 1;

        /// <summary>
    /// Gets or sets the Total Pages.
    /// </summary>
[Parameter]
    public int TotalPages { get; set; } = 5;

        /// <summary>
    /// Gets or sets the Previous Button Text.
    /// </summary>
[Parameter]
    public string PreviousButtonText { get; set; } = "previous";

        /// <summary>
    /// Gets or sets the Next Button Text.
    /// </summary>
[Parameter]
    public string NextButtonText { get; set; } = "Next";

        /// <summary>
    /// Gets or sets the On Page Change.
    /// </summary>
[Parameter]
    public EventCallback<int> OnPageChange { get; set; }
}
