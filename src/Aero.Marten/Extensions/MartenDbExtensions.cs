using System;
using System.Collections.Generic;
using System.Text;

namespace Aero.MartenDB.Extensions;

public static class MartenDbExtensions
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