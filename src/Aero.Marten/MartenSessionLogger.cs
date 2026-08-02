using Marten.Services;
using Npgsql;
using Serilog;
using ILogger = Serilog.ILogger;

namespace Aero.Marten;

/// <summary>
/// Represents a class for MartenSessionLogger.
/// </summary>
public class MartenSessionLogger : IMartenSessionLogger
{
    // todo - consider DI for the Serilog.ILogger
    private readonly ILogger log = Log.Logger;

        /// <summary>
    /// LogSuccess method.
    /// </summary>
public void LogSuccess(NpgsqlCommand command)
    {
        log.Information($"npgsql command successful {command.CommandType}: {command.CommandText}");
    }

        /// <summary>
    /// LogFailure method.
    /// </summary>
public void LogFailure(NpgsqlCommand command, Exception ex)
    {
        log.Error($"npgsql command successful {command.CommandType}: {command.CommandText}");
    }

        /// <summary>
    /// LogSuccess method.
    /// </summary>
public void LogSuccess(NpgsqlBatch batch)
    {
        log.Information("batch update success");
    }

        /// <summary>
    /// LogFailure method.
    /// </summary>
public void LogFailure(NpgsqlBatch batch, Exception ex)
    {
        log.Error(ex, "error with batch command");
    }

        /// <summary>
    /// LogFailure method.
    /// </summary>
public void LogFailure(Exception ex, string message)
    {
        log.Error(ex, message);
    }

        /// <summary>
    /// RecordSavedChanges method.
    /// </summary>
public void RecordSavedChanges(IDocumentSession session, IChangeSet commit)
    {
        log.Information($@"saved changes successful
                                 Inserts: {commit.Inserted}
                                 Updates: {commit.Updated}
                                 Deletes: {commit.Deleted}
                             ");
    }

        /// <summary>
    /// OnBeforeExecute method.
    /// </summary>
public void OnBeforeExecute(NpgsqlCommand command)
    {
    }

        /// <summary>
    /// OnBeforeExecute method.
    /// </summary>
public void OnBeforeExecute(NpgsqlBatch batch)
    {
    }
}