using System;
using System.Collections.Generic;
using System.Text;

namespace Aero.MartenDB.Extensions;

public static class MartenDbExtensions
{
    public static int CountPendingChanges(this IDocumentSession session)
    {
        var pendingDeletions = session.PendingChanges.Deletions().Count();
        var pendingUpdates = session.PendingChanges.Updates().Count();
        var pendingInserts = session.PendingChanges.Inserts().Count();
        var count = pendingInserts + pendingUpdates + pendingInserts;

        return count;
    }
}