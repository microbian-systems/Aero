using System.Linq.Expressions;
using Aero.Core.Data;
using Aero.Core.Entities;
using Marten;

namespace Aero.Services;

public interface IAeroUserProfileService : IUserProfileService<AeroUserProfile>{}

public class AeroUserProfileService(IUserProfileRepository userRepo, ILogger<AeroUserProfileService> log)
    : AeroUserProfileService<AeroUserProfile>(userRepo, log), IAeroUserProfileService;

public interface IUserProfileService<T> where T : AeroUserProfile, IEntity
{
    Task<T> GetById(ulong id);
    Task<T> GetByEmail(string email);
    Task InsertAsync(T model);
    Task UpdateAsync(T model);
    Task UpsertAsync(T model);
    Task DeleteAsync(T model);
    Task DeleteAsync(ulong id);
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
}
    
public class AeroUserProfileService<T>(IUserProfileRepository db, ILogger<AeroUserProfileService<T>> log)
    : IUserProfileService<T>
    where T : AeroUserProfile, new()
{

    public async Task<T> GetById(ulong id)
    {
        var results = await db.session.Query<T>()
            .FirstOrDefaultAsync(x => x.Id == id);
        return results;
    }

    public async Task<T> GetByEmail(string email)
    {
        ThrowGuard.Throw.NotImplemented("have to figure out how to get the user profile with marten");
        return await Task.FromResult<T>(default!);
    }

    public async Task InsertAsync(T model)
    {
        var res = await db.InsertAsync(model);
    }

    public async Task UpdateAsync(T model)
    {
        var res = await db.UpdateAsync(model);
    }

    public async Task UpsertAsync(T model)
    {
        var res = await db.UpsertAsync(model);
    }

    public async Task DeleteAsync(T model)
    {
        await DeleteAsync(model.Id);
    }

    public async Task DeleteAsync(ulong id)
    {
        await db.DeleteAsync(id);
    }

    public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
    {
        var results = await db.FindAsync((Expression<Func<AeroUserProfile, bool>>)(object)predicate);
        return results.Cast<T>();
    }
}