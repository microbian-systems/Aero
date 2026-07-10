namespace Aero.Core.DataStructures.Trees.Persistence.Wal;

/// <summary>
/// Represents a class for CheckpointOptions.
/// </summary>
public sealed class CheckpointOptions
{
        /// <summary>
    /// Gets or sets the Check Interval.
    /// </summary>
public TimeSpan CheckInterval { get; set; } = TimeSpan.FromSeconds(30);
        /// <summary>
    /// Gets or sets the Wal Size Threshold Bytes.
    /// </summary>
public long WalSizeThresholdBytes { get; set; } = 64 * 1024 * 1024;
        /// <summary>
    /// Gets or sets the Wal Entry Count Threshold.
    /// </summary>
public int WalEntryCountThreshold { get; set; } = 10_000;
        /// <summary>
    /// Gets or sets the Checkpoint On Shutdown.
    /// </summary>
public bool CheckpointOnShutdown { get; set; } = true;
}
