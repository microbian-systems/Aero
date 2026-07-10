using Aero.Core.Identity;
using Microsoft.AspNetCore.Identity;

namespace Aero.Marten.Identity;

/// <summary>
/// AeroDB store for roles.
/// </summary>
/// <typeparam name="TRole">The role type.</typeparam>
public class RoleStore<TRole>(IDocumentSession session) :
    IQueryableRoleStore<TRole>,
    IRoleClaimStore<TRole>
    where TRole : AeroRole, new()
{
    private readonly IDocumentSession _session = session ?? throw new ArgumentNullException(nameof(session));

        /// <summary>
    /// Gets or sets the Roles.
    /// </summary>
public IQueryable<TRole> Roles => _session.Query<TRole>();

        /// <summary>
    /// CreateAsync method.
    /// </summary>
public async Task<IdentityResult> CreateAsync(TRole role, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _session.Store(role);
        await _session.SaveChangesAsync(cancellationToken);
        return IdentityResult.Success;
    }

        /// <summary>
    /// DeleteAsync method.
    /// </summary>
public async Task<IdentityResult> DeleteAsync(TRole role, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _session.Delete(role);
        await _session.SaveChangesAsync(cancellationToken);
        return IdentityResult.Success;
    }

        /// <summary>
    /// FindByIdAsync method.
    /// </summary>
public async Task<TRole?> FindByIdAsync(string roleId, CancellationToken cancellationToken)
    {
        return await _session.LoadAsync<TRole>(roleId, cancellationToken);
    }

        /// <summary>
    /// FindByNameAsync method.
    /// </summary>
public async Task<TRole?> FindByNameAsync(string normalizedRoleName, CancellationToken cancellationToken)
    {
        return await _session.Query<TRole>()
            .FirstOrDefaultAsync(r => r.NormalizedName == normalizedRoleName, cancellationToken);
    }

        /// <summary>
    /// GetNormalizedRoleNameAsync method.
    /// </summary>
public Task<string?> GetNormalizedRoleNameAsync(TRole role, CancellationToken cancellationToken) => Task.FromResult(role.NormalizedName);
    // todo - The default ms identity loves to return strings - instead of casting between the two string/long - implement the IRoleStore<T, TKey> completely
        /// <summary>
    /// GetRoleIdAsync method.
    /// </summary>
public Task<string> GetRoleIdAsync(TRole role, CancellationToken cancellationToken) => Task.FromResult(role.Id.ToString());
        /// <summary>
    /// GetRoleNameAsync method.
    /// </summary>
public Task<string?> GetRoleNameAsync(TRole role, CancellationToken cancellationToken) => Task.FromResult(role.Name);

        /// <summary>
    /// SetNormalizedRoleNameAsync method.
    /// </summary>
public Task SetNormalizedRoleNameAsync(TRole role, string? normalizedName, CancellationToken cancellationToken)
    {
        role.NormalizedName = normalizedName;
        return Task.CompletedTask;
    }

        /// <summary>
    /// SetRoleNameAsync method.
    /// </summary>
public Task SetRoleNameAsync(TRole role, string? roleName, CancellationToken cancellationToken)
    {
        role.Name = roleName;
        return Task.CompletedTask;
    }

        /// <summary>
    /// UpdateAsync method.
    /// </summary>
public async Task<IdentityResult> UpdateAsync(TRole role, CancellationToken cancellationToken)
    {
        _session.Update(role);
        await _session.SaveChangesAsync(cancellationToken);
        return IdentityResult.Success;
    }

        /// <summary>
    /// GetClaimsAsync method.
    /// </summary>
public Task<IList<System.Security.Claims.Claim>> GetClaimsAsync(TRole role, CancellationToken cancellationToken = default)
    {
        IList<System.Security.Claims.Claim> result = role.Claims
            .Select(c => new System.Security.Claims.Claim(c.ClaimType, c.ClaimValue))
            .ToList();
        return Task.FromResult(result);
    }

        /// <summary>
    /// AddClaimAsync method.
    /// </summary>
public Task AddClaimAsync(TRole role, System.Security.Claims.Claim claim, CancellationToken cancellationToken = default)
    {
        role.Claims.Add(new IdentityRoleClaim<long> { ClaimType = claim.Type, ClaimValue = claim.Value });
        return Task.CompletedTask;
    }

        /// <summary>
    /// RemoveClaimAsync method.
    /// </summary>
public Task RemoveClaimAsync(TRole role, System.Security.Claims.Claim claim, CancellationToken cancellationToken = default)
    {
        var existing = role.Claims.FirstOrDefault(c => c.ClaimType == claim.Type && c.ClaimValue == claim.Value);
        if (existing != null) role.Claims.Remove(existing);
        return Task.CompletedTask;
    }

        /// <summary>
    /// Dispose method.
    /// </summary>
public void Dispose() { }
}
