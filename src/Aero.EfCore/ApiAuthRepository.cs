using System.Linq.Expressions;

namespace Aero.EfCore;

/// <summary>
/// Defines an interface for IApiAuthRepository.
/// </summary>
public interface IApiAuthRepository : IGenericEntityFrameworkRepository<ApiAccountModel>
{
        /// <summary>
    /// GetByApiKey method.
    /// </summary>
Task<ApiAccountModel?> GetByApiKey(string apiKey);
}

/// <summary>
/// Represents a class for ApiAuthRepository.
/// </summary>
public sealed class ApiAuthRepository(AeroDbContext context, ILogger<ApiAuthRepository> log)
    : GenericEntityFrameworkRepository<ApiAccountModel>(context, log), IApiAuthRepository
{
    private readonly DbSet<ApiAccountModel> apiAccountsDb = context.ApiAccounts;
    private readonly DbSet<ApiClaimsModel> apiClaimsDb = context.ApiClaims;

        /// <summary>
    /// GetAllAsync method.
    /// </summary>
public override Task<IEnumerable<ApiAccountModel>> GetAllAsync()
    {
        var accounts = apiAccountsDb.AsQueryable()
            .Include(a => a.Claims)
            .AsEnumerable();

        return Task.FromResult(accounts);
    }

        /// <summary>
    /// GetByKeyAsync method.
    /// </summary>
public async Task<ApiAccountModel?> GetByKeyAsync(long key)
    {
        var account = await apiAccountsDb
            .Include(x => x.Claims)
            .SingleOrDefaultAsync(x => x.Id == key);

        return account;
    }

        /// <summary>
    /// InsertAsync method.
    /// </summary>
public override async Task<ApiAccountModel> InsertAsync(ApiAccountModel model)
    {
        await apiAccountsDb.AddAsync(model);

        return model;
    }

        /// <summary>
    /// UpdateAsync method.
    /// </summary>
public override Task<ApiAccountModel> UpdateAsync(ApiAccountModel model)
    {
        apiAccountsDb.Update(model);

        return Task.FromResult(model);
    }

        /// <summary>
    /// DeleteAsync method.
    /// </summary>
public override Task DeleteAsync(ApiAccountModel model)
    {
        apiAccountsDb.Remove(model);

        return Task.CompletedTask;
    }

        /// <summary>
    /// DeleteAsync method.
    /// </summary>
public override async Task DeleteAsync(long id)
    {
        var account = await apiAccountsDb
            .Include(x => x.Claims)
            .SingleOrDefaultAsync(x => x.Id == id);
        await DeleteAsync(account!);
    }

        /// <summary>
    /// FindAsync method.
    /// </summary>
public override Task<IEnumerable<ApiAccountModel>> FindAsync(Expression<Func<ApiAccountModel, bool>> predicate)
    {
        var accounts = apiAccountsDb.Where(predicate)
            .Include(x => x.Claims)
            .AsEnumerable();

        return Task.FromResult(accounts);
    }

        /// <summary>
    /// SaveChangesAsync method.
    /// </summary>
public async Task<int> SaveChangesAsync()
        => await context.SaveChangesAsync();

        /// <summary>
    /// GetByApiKey method.
    /// </summary>
public async Task<ApiAccountModel?> GetByApiKey(string apiKey)
    {
        var model = await apiAccountsDb
            .Include(x => x.Claims)
            .FirstOrDefaultAsync(x => x.ApiKey == apiKey);

        return model;
    }
}