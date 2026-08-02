namespace Aero.Marten.Extensions;

/// <summary>
/// Represents a class for MartenUtilityExtensions.
/// </summary>
public static class MartenUtilityExtensions
{
    /// <summary>
    /// Gets pending changes count for the current document session
    /// </summary>
    /// <param name="session">Marten document session</param>
    /// <returns>number of changes</returns>
    public static int CountPendingChanges(this IDocumentSession session)
    {
        var pendingDeletions = session.PendingChanges.Deletions().Count();
        var pendingUpdates = session.PendingChanges.Updates().Count();
        var pendingInserts = session.PendingChanges.Inserts().Count();
        var count = pendingInserts + pendingUpdates + pendingInserts;

        return count;
    }
}

