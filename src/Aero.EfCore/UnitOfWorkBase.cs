using Aero.Core.Data;

namespace Aero.EfCore;


/// <summary>
/// Represents a class for UnitEfCoreOfWorkEfCore.
/// </summary>
public abstract class UnitEfCoreOfWorkEfCore(DbContext context) : IUnitOfWork, IAsyncUnitOfWork
{
    private Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction? _transaction;
        /// <summary>
    /// Gets or sets the Context.
    /// </summary>
public DbContext Context { get; } = context;
        /// <summary>
    /// SaveChangesAsync method.
    /// </summary>
public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await Context.SaveChangesAsync(cancellationToken);
    }

        /// <summary>
    /// SaveChanges method.
    /// </summary>
public int SaveChanges()
    {
        return Context.SaveChanges();
    }

        /// <summary>
    /// StartTransactionAsync method.
    /// </summary>
public async Task StartTransactionAsync(CancellationToken cancellationToken = default)
    {
        _transaction = await Context.Database.BeginTransactionAsync(cancellationToken);
    }

        /// <summary>
    /// CommitTransactionAsync method.
    /// </summary>
public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

        /// <summary>
    /// RollbackTransactionAsync method.
    /// </summary>
public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

        /// <summary>
    /// Dispose method.
    /// </summary>
public void Dispose()
    {
        _transaction?.Dispose();
        Context.Dispose();
        GC.SuppressFinalize(this);
    }
}


