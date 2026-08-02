namespace Aero.Models.ViewModels;

/// <summary>
/// Represents a record for ServiceRequestResult.
/// </summary>
public abstract record ServiceRequestResult<T>
{
        /// <summary>
    /// Gets or sets the Success.
    /// </summary>
public bool Success { get; set; }
        /// <summary>
    /// Gets or sets the Result.
    /// </summary>
public virtual T Result { get; set; }
        /// <summary>
    /// Gets or sets the Errors.
    /// </summary>
public HashSet<string> Errors { get; set; } = new();
        /// <summary>
    /// Gets or sets the Validation Errors.
    /// </summary>
public HashSet<string> ValidationErrors { get; set; } = new();
}