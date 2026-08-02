using Aero.Core.DataStructures.Trees.Persistence.Linq.Translation;

namespace Aero.Core.DataStructures.Trees.Persistence.Linq.Planning;

/// <summary>
/// Represents a class for LoggingQueryDiagnostics.
/// </summary>
public sealed class LoggingQueryDiagnostics(ILogger<LoggingQueryDiagnostics> logger) : IQueryDiagnostics
{
        /// <summary>
    /// ReportIndexScan method.
    /// </summary>
public void ReportIndexScan(string collectionName, IndexScanSpec spec, bool hasResidual) =>
        logger.LogDebug(
            "[{Collection}] Index scan on '{Index}' " +
            "(point={IsPoint}, residual={HasResidual})",
            collectionName, spec.Index.Name, spec.IsPoint, hasResidual);

        /// <summary>
    /// ReportFullScan method.
    /// </summary>
public void ReportFullScan(string collectionName, object query) =>
        logger.LogWarning(
            "[{Collection}] Full collection scan — consider adding an index.",
            collectionName);
}
