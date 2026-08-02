namespace Aero.Social.Models;

/// <summary>
/// Represents a class for PollDetails.
/// </summary>
public class PollDetails
{
        /// <summary>
    /// Gets or sets the Options.
    /// </summary>
public List<string> Options { get; set; } = new();
        /// <summary>
    /// Gets or sets the Duration Hours.
    /// </summary>
public int DurationHours { get; set; }
}
