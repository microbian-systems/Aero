namespace Aero.Core.DataStructures.Trees.Persistence.Concurrency;

/// <summary>
/// Defines an interface for IReadSnapshot.
/// </summary>
public interface IReadSnapshot : IDisposable
{
        /// <summary>
    /// Gets or sets the Snapshot Transaction Id.
    /// </summary>
long SnapshotTransactionId { get; }
        /// <summary>
    /// IsVisible method.
    /// </summary>
bool IsVisible(long xmin, long xmax);
}
