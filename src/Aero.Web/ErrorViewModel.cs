namespace Aero.Web;

/// <summary>
/// Represents a class for ErrorViewModel.
/// </summary>
public class ErrorViewModel
{
        /// <summary>
    /// Gets or sets the Request Id.
    /// </summary>
public string RequestId { get; set; }

        /// <summary>
    /// Gets or sets the Show Request Id.
    /// </summary>
public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
}