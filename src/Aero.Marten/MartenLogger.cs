using Serilog;
using ILogger = Serilog.ILogger;

namespace Aero.Marten;

/// <summary>
/// Represents a class for MartenLogger.
/// </summary>
public class MartenLogger(IMartenSessionLogger? sessionLog) : IMartenLogger
{
    private readonly ILogger log = Log.Logger;
        /// <summary>
    /// Gets or sets the Session Log.
    /// </summary>
public IMartenSessionLogger SessionLog { get; } = sessionLog ?? new MartenSessionLogger();

        /// <summary>
    /// StartSession method.
    /// </summary>
public IMartenSessionLogger StartSession(IQuerySession session)
    {
        // todo - figure out how to use IQuerySession obj in MartenLogger
        return SessionLog;
    }

        /// <summary>
    /// SchemaChange method.
    /// </summary>
public void SchemaChange(string sql)
    {
        Log.Information($"there was a session chagne {sql}");
    }
}