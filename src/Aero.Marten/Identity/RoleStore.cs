using Aero.Core.Identity;
using Marten;
using Microsoft.AspNetCore.Identity;

namespace Aero.MartenDB.Identity;

/// <summary>
/// AeroDB store for roles.
/// </summary>
/// <typeparam name="TRole">The role type.</typeparam>
public class RoleStore<TRole> :
    IQueryableRoleStore<TRole>,
    IRoleClaimStore<TRole>
    where TRole : AeroRole, new()
{
    private readonly IDocumentSession _session;

    public RoleStore(IDocumentSession session)
    {
        _session = session ?? throw new ArgumentNullException(nameof(session));
    }

    public IQueryable<TRole> Roles => _session.Query<TRole>();

    public async Task<IdentityResult> CreateAsync(TRole role, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _session.Store(role);
        await _session.SaveChangesAsync(cancellationToken);
        return IdentityResult.Success;
    }

    public async Task<IdentityResult> DeleteAsync(TRole role, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _session.Delete(role);
        await _session.SaveChangesAsync(cancellationToken);
        return IdentityResult.Success;
    }

    public async Task<TRole?> FindByIdAsync(string roleId, CancellationToken cancellationToken)
    {
        return await _session.LoadAsync<TRole>(roleId, cancellationToken);
    }

    public async Task<TRole?> FindByNameAsync(string normalizedRoleName, CancellationToken cancellationToken)
    {
        return await _session.Query<TRole>()
            .FirstOrDefaultAsync(r => r.NormalizedName == normalizedRoleName, cancellationToken);
    }

    public Task<string?> GetNormalizedRoleNameAsync(TRole role, CancellationToken cancellationToken) => Task.FromResult(role.NormalizedName);
    // todo - The default ms identity loves to return strings - instead of casting between the two string/long - implement the IRoleStore<T, TKey> completely
    public Task<string> GetRoleIdAsync(TRole role, CancellationToken cancellationToken) => Task.FromResult(role.Id.ToString());
    public Task<string?> GetRoleNameAsync(TRole role, CancellationToken cancellationToken) => Task.FromResult(role.Name);

    public Task SetNormalizedRoleNameAsync(TRole role, string? normalizedName, CancellationToken cancellationToken)
    {
        role.NormalizedName = normalizedName;
        return Task.CompletedTask;
    }

    public Task SetRoleNameAsync(TRole role, string? roleName, CancellationToken cancellationToken)
    {
        role.Name = roleName;
        return Task.CompletedTask;
    }

    public async Task<IdentityResult> UpdateAsync(TRole role, CancellationToken cancellationToken)
    {
        _session.Update(role);
        await _session.SaveChangesAsync(cancellationToken);
        return IdentityResult.Success;
    }

    public Task<IList<System.Security.Claims.Claim>> GetClaimsAsync(TRole role, CancellationToken cancellationToken = default)
    {
        IList<System.Security.Claims.Claim> result = role.Claims
            .Select(c => new System.Security.Claims.Claim(c.ClaimType, c.ClaimValue))
            .ToList();
        return Task.FromResult(result);
    }

    public Task AddClaimAsync(TRole role, System.Security.Claims.Claim claim, CancellationToken cancellationToken = default)
    {
        role.Claims.Add(new IdentityRoleClaim<long> { ClaimType = claim.Type, ClaimValue = claim.Value });
        return Task.CompletedTask;
    }

    public Task RemoveClaimAsync(TRole role, System.Security.Claims.Claim claim, CancellationToken cancellationToken = default)
    {
        var existing = role.Claims.FirstOrDefault(c => c.ClaimType == claim.Type && c.ClaimValue == claim.Value);
        if (existing != null) role.Claims.Remove(existing);
        return Task.CompletedTask;
    }

    public void Dispose() { }
}
