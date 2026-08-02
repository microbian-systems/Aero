using Aero.Core.Data;
using Aero.Marten.Extensions;
using Aero.Models.Entities;

namespace Aero.Marten;

/// <summary>
/// Database facade containing all domains for Aero CMS (unified db api)
/// </summary>
public interface IAeroDb : IAsyncUnitOfWork
{
        /// <summary>
    /// Gets or sets the Session.
    /// </summary>
IDocumentSession Session { get; }

        /// <summary>
    /// Gets or sets the Users.
    /// </summary>
IQueryable<AeroUser> Users { get; }


    // todo - add all AeroCMS repositories as properties on AeroDb class + IAeroDB interface
    // todo - add Posts data access
    // todo - add Pages data access
    // todo - add Modules data access
}


/// <summary>
/// Represents a class for AeroDb.
/// </summary>
public class AeroDb(
    IDocumentSession session,
    ILogger<AeroDb> log)
    : IAeroDb
{
        /// <summary>
    /// Gets or sets the Session.
    /// </summary>
public IDocumentSession Session => session;

    // Lazy initialization ensures the repo is only created when accessed
    // and guarantees it uses the UoW's specific session.
        /// <summary>
    /// Gets or sets the Users.
    /// </summary>
public IQueryable<AeroUser> Users => session.Query<AeroUser>();

        /// <summary>
    /// SaveChangesAsync method.
    /// </summary>
public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Your existing logic
        // var changes = _session.Advanced.WhatChanged();
        // var count = changes.Count;
        try
        {
            var count = session.CountPendingChanges();
            await session.SaveChangesAsync(cancellationToken);
            return count;
        }
        catch (Exception ex)
        {
            log.LogError(ex, "Failed to save changes to AeroDB");
            return 0; // Or re-throw, depending on your error handling strategy
        }
    }

        /// <summary>
    /// StartTransactionAsync method.
    /// </summary>
public Task StartTransactionAsync(CancellationToken cancellationToken = default)
    {
        // AeroDB sessions are transactional by default. 
        // We could use ClusterTransaction if needed, but for standard session-level transactions,
        // just having the session is enough.
        return Task.CompletedTask;
    }

        /// <summary>
    /// CommitTransactionAsync method.
    /// </summary>
public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        await SaveChangesAsync(cancellationToken);
    }

        /// <summary>
    /// RollbackTransactionAsync method.
    /// </summary>
public Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        // To rollback in AeroDB session, we clear the session state.
        // todo - rollback marten transaction
        //_session.Advanced.Clear();
        return Task.CompletedTask;
    }

        /// <summary>
    /// Dispose method.
    /// </summary>
public void Dispose()
    {
        session.Dispose();
        GC.SuppressFinalize(this);
    }
}