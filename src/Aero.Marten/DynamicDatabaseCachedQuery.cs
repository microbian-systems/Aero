using Aero.Core.Entities;
using Aero.Caching;
using Serilog;
using Aero.Core.Railway;

namespace Aero.Marten;

public class DynamicDbCachedQuery<T>(ICacheService cache, IDynamicDatabaseQuery<T> query, ILogger<DynamicDbCachedQuery<T>> log)
    : IDynamicDbCachedQuery<T>
    where T : class, IEntity<Guid>
{
    public async Task<IEnumerable<T>> ExecuteAsync(Expression<Func<T, bool>> parameter)
    {
        log.LogInformation("attempting to retrieved cached query....");
        var key = parameter.ToString();
        var cached = await cache.GetAsync<IEnumerable<T>>(key);
        
        if (cached.IsSome)
        {
            log.LogInformation("cache hit. results found");
            return cached.GetOrElse([]);
        }
            
        log.LogInformation("cache miss. attempting to get and store results");
        var results = await query.ExecuteAsync(parameter);
        log.LogInformation($"results found: {results.Count()}");
        await cache.SetAsync(key, results, TimeSpan.FromMinutes(5));
        log.LogInformation("added results to cache");
        return results;
    }
}
