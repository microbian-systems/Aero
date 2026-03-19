namespace Aero.Marten;

public class DynamicDatabaseQuery<T>(IDocumentSession db, ILogger<DynamicDatabaseQuery<T>> log)
    : IDynamicDatabaseQuery<T>
    where T : class, IEntity<Guid>
{
    public async Task<IEnumerable<T>> ExecuteAsync(Expression<Func<T, bool>> parameter)
    {
        log.LogInformation($"querying database ...");
        var results = await db.Query<T>().Where(parameter).ToListAsync();
        log.LogInformation($"finished query database with {results.Count} results");
        return results;
    }
}