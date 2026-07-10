namespace Aero.Core.Data;

/// <summary>
/// Defines an interface for IUnitOfWork.
/// </summary>
public interface IUnitOfWork : IDisposable
{
        /// <summary>
    /// SaveChanges method.
    /// </summary>
public int SaveChanges();
}

/// <summary>
/// Defines an interface for IAsyncUnitOfWork.
/// </summary>
public interface IAsyncUnitOfWork : IDisposable
{
        /// <summary>
    /// SaveChangesAsync method.
    /// </summary>
public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        /// <summary>
    /// StartTransactionAsync method.
    /// </summary>
public Task StartTransactionAsync(CancellationToken cancellationToken = default);
        /// <summary>
    /// CommitTransactionAsync method.
    /// </summary>
public Task CommitTransactionAsync(CancellationToken cancellationToken = default);
        /// <summary>
    /// RollbackTransactionAsync method.
    /// </summary>
public Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}