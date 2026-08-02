namespace Aero.Core.DataStructures.Trees.Persistence.Format;

/// <summary>
/// Defines an interface for IHeaderManager.
/// </summary>
public interface IHeaderManager
{
        /// <summary>
    /// ReadAsync method.
    /// </summary>
ValueTask<FileHeader> ReadAsync(CancellationToken ct = default);
        /// <summary>
    /// WriteAsync method.
    /// </summary>
ValueTask WriteAsync(FileHeader header, CancellationToken ct = default);
        /// <summary>
    /// PersistNextTransactionIdAsync method.
    /// </summary>
ValueTask PersistNextTransactionIdAsync(long value, CancellationToken ct = default);
        /// <summary>
    /// PersistMinActiveTxnIdAsync method.
    /// </summary>
ValueTask PersistMinActiveTxnIdAsync(long value, CancellationToken ct = default);
        /// <summary>
    /// SetShutdownStateAsync method.
    /// </summary>
ValueTask SetShutdownStateAsync(ShutdownState state, CancellationToken ct = default);
}
