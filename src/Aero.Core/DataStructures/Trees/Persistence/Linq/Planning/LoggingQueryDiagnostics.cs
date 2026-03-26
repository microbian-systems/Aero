using Aero.DataStructures.Trees.Persistence.Linq.Translation;

namespace Aero.DataStructures.Trees.Persistence.Linq.Planning;

public sealed class LoggingQueryDiagnostics(ILogger<LoggingQueryDiagnostics> logger) : IQueryDiagnostics
{
    public void ReportIndexScan(string collectionName, IndexScanSpec spec, bool hasResidual) =>
        logger.LogDebug(
            "[{Collection}] Index scan on '{Index}' " +
            "(point={IsPoint}, residual={HasResidual})",
            collectionName, spec.Index.Name, spec.IsPoint, hasResidual);

    public void ReportFullScan(string collectionName, object query) =>
        logger.LogWarning(
            "[{Collection}] Full collection scan — consider adding an index.",
            collectionName);
}
