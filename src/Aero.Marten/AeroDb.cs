using Aero.Core.Data;
using Aero.MartenDB.Extensions;

namespace Aero.MartenDB;

/// <summary>
/// Database facade containing all domains for Aero CMS (unified db api)
/// </summary>
public interface IAeroDb : IAsyncUnitOfWork
{
    IDocumentSession Session { get; }

    IAeroUserRepository Users { get; }

    // todo - add all AeroCMS repositories as properties on AeroDb class + IAeroDB interface
    // todo - add Posts data access
    // todo - add Pages data access
    // todo - add Modules data access
}


public class AeroDb(
    IDocumentSession session,
    IAeroUserRepository users,
    ILogger<AeroDb> log,
    ILoggerFactory loggerFactory)
    : IAeroDb
{
    public IDocumentSession Session => session;

    // Lazy initialization ensures the repo is only created when accessed
    // and guarantees it uses the UoW's specific session.
    public IAeroUserRepository Users => users;

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

    public Task StartTransactionAsync(CancellationToken cancellationToken = default)
    {
        // AeroDB sessions are transactional by default. 
        // We could use ClusterTransaction if needed, but for standard session-level transactions,
        // just having the session is enough.
        return Task.CompletedTask;
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        await SaveChangesAsync(cancellationToken);
    }

    public Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        // To rollback in AeroDB session, we clear the session state.
        // todo - rollback marten transaction
        //_session.Advanced.Clear();
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        session.Dispose();
        GC.SuppressFinalize(this);
    }
}