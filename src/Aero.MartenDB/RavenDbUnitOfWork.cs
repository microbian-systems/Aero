using Microsoft.Extensions.Logging;

namespace Aero.MartenDB;

public class AeroUnitOfWork : IAeroDbUnitOfWork
{
    private readonly IDocumentSession _session;
    private readonly ILogger<AeroUnitOfWork> _log;
    private readonly ILoggerFactory _loggerFactory;

    // Backing field for the repository
    private IAeroUserRepository? _users;

    public AeroUnitOfWork(
        IDocumentSession session, 
        ILogger<AeroUnitOfWork> log,
        ILoggerFactory loggerFactory) 
    {
        _session = session;
        _log = log;
        _loggerFactory = loggerFactory;
    }

    public IDocumentSession Session => _session;

    // Lazy initialization ensures the repo is only created when accessed
    // and guarantees it uses the UoW's specific session.
    public IAeroUserRepository Users
    {
        get
        {
            if (_users == null)
            {
                var repoLogger = _loggerFactory.CreateLogger<AeroUserRepository>();
                _users = new AeroUserRepository(_session, repoLogger);
            }
            return _users;
        }
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Your existing logic
        // var changes = _session.Advanced.WhatChanged();
        // var count = changes.Count;
        try
        {
            var count = _session.PendingChanges.Deletions().Count()
                + _session.PendingChanges.Inserts().Count()
                + _session.PendingChanges.Updates().Count();
            await _session.SaveChangesAsync(cancellationToken);
            return count;
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Failed to save changes to AeroDB");
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
        _session.Dispose();
        GC.SuppressFinalize(this);
    }
}