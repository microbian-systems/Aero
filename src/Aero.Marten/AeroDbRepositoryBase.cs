using static System.GC;


namespace Aero.MartenDB;

// todo - perform retry on martendb exeption connection type or similar 

public abstract class AeroDbRepositoryBase<TEntity>(
    IDocumentSession session,
    ILogger<AeroDbRepositoryBase<TEntity>> log)
    : MartenGenericRepositoryOption<TEntity>(log)
    where TEntity : ISnowflakeEntity, new()
{
    protected readonly IDocumentSession session = session;

    public override async Task<long> CountAsync()
    {
        return await session.Query<TEntity>().LongCountAsync();
    }

    public override async Task<bool> ExistsAsync(ulong id)
    {
        return await session.Query<TEntity>().AnyAsync(x => x.Id == id);
    }

    public override async Task<IEnumerable<TEntity>> GetAllAsync()
    {
        var res = (await session.Query<TEntity>().ToListAsync()) ?? [] ;

        return res;
    }

    public override async Task<Option<TEntity>> FindByIdAsync(ulong id)
    {
        var entity = await session.Query<TEntity>().FirstOrDefaultAsync(x => x.Id == id);
        var res = entity is not null ? Some(entity) : Prelude.None;
        return res;
    }

    public override async Task<TEntity> InsertAsync(TEntity entity)
    {
        try
        {
            var existing = await FindByIdAsync(entity.Id);
            if (existing.IsSome) throw new Exception($"Entity with id: {entity.Id} already exists");
            session.Store(entity);
            return entity;
        }
        catch (Exception ex)
        {
            log.LogError(ex, "Failed to upsert entity with id: {id}", entity.Id);
           throw;
        }
    }

    public override async Task<TEntity> UpdateAsync(TEntity entity)
    {
        try
        {
            var existing = await FindByIdAsync(entity.Id);
            if (existing.IsNone) throw new Exception($"Update failed - Entity with id: {entity.Id} does not exist");
            session.Store(entity);
            return entity;
        }
        catch (Exception ex)
        {
            log.LogError(ex, "Failed to upsert entity with id: {id}", entity.Id);
           throw;
        }
    }        
    

    public async override Task<TEntity> UpsertAsync(TEntity entity)
    {
        try
        {
            session.Store(entity);
            // todo - return a value task (requires changin signature of marten repository base classes)
            // return new ValueTask<TEntity>(entity);
            return entity;
        }
        catch (Exception ex)
        {
            log.LogError(ex, "Failed to upsert entity with id: {id}", entity.Id);
           throw;
        }
    }

    public override async Task<bool> DeleteAsync(ulong id)
    {
        try
        {
            var entityOption = await FindByIdAsync(id);
            if (entityOption.IsNone)
            {
                log.LogWarning("Entity with id: {id} not found for deletion", id);
                return false;
            }

            entityOption.IfSome(entity => session.Delete(entity));
            return true;
        }
        catch (Exception ex)
        {
            log.LogError(ex, "Failed to delete entity with id: {id}", id);
            throw;
        }
    }

    public override Task<bool> DeleteAsync(TEntity entity)
    {
        try
        {
            session.Delete(entity);
            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            log.LogError(ex, "Failed to delete entity with id: {id}", entity.Id);
            throw;
        }
        
    }

    public override async Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate)
    {
        var results = await session.Query<TEntity>().Where(predicate).ToListAsync();
        return results ?? [];
    }

    public override async Task<Option<TEntity>> GetByIdAsync(ulong id)
    {
        var res = await FindByIdAsync(id);
        return res;
    }

    public override async Task<IEnumerable<TEntity>> GetByIdsAsync(IEnumerable<ulong> ids)
    {
        return await FindAsync(x => ids.Contains(x.Id));
    }


    // todo - implement IAsyncDisposable and its pattern for AeroDbRepository
    public void Dispose()
    {
        session.Dispose();
        SuppressFinalize(this);
    }
}