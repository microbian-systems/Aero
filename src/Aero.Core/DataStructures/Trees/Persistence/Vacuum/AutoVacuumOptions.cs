namespace Aero.Core.DataStructures.Trees.Persistence.Vacuum;

/// <summary>
/// Represents a class for AutoVacuumOptions.
/// </summary>
public sealed class AutoVacuumOptions
{
        /// <summary>
    /// Gets or sets the Check Interval.
    /// </summary>
public TimeSpan CheckInterval { get; set; } = TimeSpan.FromMinutes(5);

        /// <summary>
    /// Gets or sets the Fragmentation Threshold.
    /// </summary>
public double FragmentationThreshold { get; set; } = 0.5;
}
