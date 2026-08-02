using System.Security.Claims;
using Aero.Core.Identity;
using Aero.Models.Entities;
using Microsoft.AspNetCore.Identity;

namespace Aero.Marten.Identity;

/// <summary>
/// Represents a class for UserStore.
/// </summary>
public class UserStore<TUser, TRole>(IDocumentSession session) :
    IUserStore<TUser>,
    IUserLoginStore<TUser>,
    IUserClaimStore<TUser>,
    IUserRoleStore<TUser>,
    IUserPasswordStore<TUser>,
    IUserSecurityStampStore<TUser>,
    IUserEmailStore<TUser>,
    IUserLockoutStore<TUser>,
    IUserTwoFactorStore<TUser>,
    IUserPhoneNumberStore<TUser>,
    IUserAuthenticatorKeyStore<TUser>,
    IUserAuthenticationTokenStore<TUser>,
    IUserTwoFactorRecoveryCodeStore<TUser>,
    IQueryableUserStore<TUser>
    where TUser : AeroUser, new()
    where TRole : AeroRole, new()
{
    private readonly IDocumentSession _session = session ?? throw new ArgumentNullException(nameof(session));

        /// <summary>
    /// Gets or sets the Users.
    /// </summary>
public IQueryable<TUser> Users => _session.Query<TUser>();

        /// <summary>
    /// CreateAsync method.
    /// </summary>
public async Task<IdentityResult> CreateAsync(TUser user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _session.Store(user);
        await _session.SaveChangesAsync(cancellationToken);
        return IdentityResult.Success;
    }

        /// <summary>
    /// DeleteAsync method.
    /// </summary>
public async Task<IdentityResult> DeleteAsync(TUser user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _session.Delete(user);
        await _session.SaveChangesAsync(cancellationToken);
        return IdentityResult.Success;
    }

        /// <summary>
    /// FindByIdAsync method.
    /// </summary>
public async Task<TUser?> FindByIdAsync(string userId, CancellationToken cancellationToken) => await _session.LoadAsync<TUser>(userId, cancellationToken);
        /// <summary>
    /// FindByNameAsync method.
    /// </summary>
public async Task<TUser?> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken) => await _session.Query<TUser>().FirstOrDefaultAsync(u => u.NormalizedUserName == normalizedUserName, cancellationToken);
    // todo - default ms identity loves to use string - implement the full IUserStore to get around this temporary fix
        /// <summary>
    /// GetUserIdAsync method.
    /// </summary>
public Task<string> GetUserIdAsync(TUser user, CancellationToken cancellationToken) => Task.FromResult(user.Id.ToString());
        /// <summary>
    /// GetUserNameAsync method.
    /// </summary>
public Task<string?> GetUserNameAsync(TUser user, CancellationToken cancellationToken) => Task.FromResult(user.UserName);
        /// <summary>
    /// SetNormalizedUserNameAsync method.
    /// </summary>
public Task SetNormalizedUserNameAsync(TUser user, string? normalizedName, CancellationToken cancellationToken) { user.NormalizedUserName = normalizedName; return Task.CompletedTask; }
        /// <summary>
    /// SetUserNameAsync method.
    /// </summary>
public Task SetUserNameAsync(TUser user, string? userName, CancellationToken cancellationToken) { user.UserName = userName; return Task.CompletedTask; }
        /// <summary>
    /// UpdateAsync method.
    /// </summary>
public async Task<IdentityResult> UpdateAsync(TUser user, CancellationToken cancellationToken) { _session.Update(user); await _session.SaveChangesAsync(cancellationToken); return IdentityResult.Success; }
        /// <summary>
    /// GetNormalizedUserNameAsync method.
    /// </summary>
public Task<string?> GetNormalizedUserNameAsync(TUser user, CancellationToken cancellationToken) => Task.FromResult(user.NormalizedUserName);

    // IUserPasswordStore
        /// <summary>
    /// SetPasswordHashAsync method.
    /// </summary>
public Task SetPasswordHashAsync(TUser user, string? passwordHash, CancellationToken cancellationToken) { user.PasswordHash = passwordHash; return Task.CompletedTask; }
        /// <summary>
    /// GetPasswordHashAsync method.
    /// </summary>
public Task<string?> GetPasswordHashAsync(TUser user, CancellationToken cancellationToken) => Task.FromResult(user.PasswordHash);
        /// <summary>
    /// HasPasswordAsync method.
    /// </summary>
public Task<bool> HasPasswordAsync(TUser user, CancellationToken cancellationToken) => Task.FromResult(user.PasswordHash != null);

    // IUserEmailStore
        /// <summary>
    /// SetEmailAsync method.
    /// </summary>
public Task SetEmailAsync(TUser user, string? email, CancellationToken cancellationToken) { user.Email = email; return Task.CompletedTask; }
        /// <summary>
    /// GetEmailAsync method.
    /// </summary>
public Task<string?> GetEmailAsync(TUser user, CancellationToken cancellationToken) => Task.FromResult(user.Email);
        /// <summary>
    /// GetEmailConfirmedAsync method.
    /// </summary>
public Task<bool> GetEmailConfirmedAsync(TUser user, CancellationToken cancellationToken) => Task.FromResult(user.EmailConfirmed);
        /// <summary>
    /// SetEmailConfirmedAsync method.
    /// </summary>
public Task SetEmailConfirmedAsync(TUser user, bool confirmed, CancellationToken cancellationToken) { user.EmailConfirmed = confirmed; return Task.CompletedTask; }
        /// <summary>
    /// FindByEmailAsync method.
    /// </summary>
public async Task<TUser?> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken) => await _session.Query<TUser>().FirstOrDefaultAsync(u => u.NormalizedEmail == normalizedEmail, cancellationToken);
        /// <summary>
    /// GetNormalizedEmailAsync method.
    /// </summary>
public Task<string?> GetNormalizedEmailAsync(TUser user, CancellationToken cancellationToken) => Task.FromResult(user.NormalizedEmail);
        /// <summary>
    /// SetNormalizedEmailAsync method.
    /// </summary>
public Task SetNormalizedEmailAsync(TUser user, string? normalizedEmail, CancellationToken cancellationToken) { user.NormalizedEmail = normalizedEmail; return Task.CompletedTask; }

    // IUserLoginStore
        /// <summary>
    /// AddLoginAsync method.
    /// </summary>
public Task AddLoginAsync(TUser user, UserLoginInfo login, CancellationToken cancellationToken) { user.Logins.Add(new IdentityLogin { LoginProvider = login.LoginProvider, ProviderKey = login.ProviderKey, ProviderDisplayName = login.ProviderDisplayName }); return Task.CompletedTask; }
        /// <summary>
    /// RemoveLoginAsync method.
    /// </summary>
public Task RemoveLoginAsync(TUser user, string loginProvider, string providerKey, CancellationToken cancellationToken) { var login = user.Logins.FirstOrDefault(l => l.LoginProvider == loginProvider && l.ProviderKey == providerKey); if (login != null) user.Logins.Remove(login); return Task.CompletedTask; }
        /// <summary>
    /// GetLoginsAsync method.
    /// </summary>
public Task<IList<UserLoginInfo>> GetLoginsAsync(TUser user, CancellationToken cancellationToken) => Task.FromResult<IList<UserLoginInfo>>(user.Logins.Select(l => new UserLoginInfo(l.LoginProvider, l.ProviderKey, l.ProviderDisplayName)).ToList());
        /// <summary>
    /// FindByLoginAsync method.
    /// </summary>
public async Task<TUser?> FindByLoginAsync(string loginProvider, string providerKey, CancellationToken cancellationToken) => await _session.Query<TUser>().FirstOrDefaultAsync(u => u.Logins.Any(l => l.LoginProvider == loginProvider && l.ProviderKey == providerKey), cancellationToken);

    // IUserClaimStore
        /// <summary>
    /// GetClaimsAsync method.
    /// </summary>
public Task<IList<Claim>> GetClaimsAsync(TUser user, CancellationToken cancellationToken) => Task.FromResult<IList<Claim>>(user.Claims.Select(c => new Claim(c.ClaimType, c.ClaimValue)).ToList());
        /// <summary>
    /// AddClaimsAsync method.
    /// </summary>
public Task AddClaimsAsync(TUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken) { foreach (var claim in claims) user.Claims.Add(new IdentityUserClaim<long> { ClaimType = claim.Type, ClaimValue = claim.Value }); return Task.CompletedTask; }
        /// <summary>
    /// ReplaceClaimAsync method.
    /// </summary>
public Task ReplaceClaimAsync(TUser user, Claim claim, Claim newClaim, CancellationToken cancellationToken) { var existing = user.Claims.FirstOrDefault(c => c.ClaimType == claim.Type && c.ClaimValue == claim.Value); if (existing != null) { existing.ClaimType = newClaim.Type; existing.ClaimValue = newClaim.Value; } return Task.CompletedTask; }
        /// <summary>
    /// RemoveClaimsAsync method.
    /// </summary>
public Task RemoveClaimsAsync(TUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken) { foreach (var claim in claims) { var existing = user.Claims.FirstOrDefault(c => c.ClaimType == claim.Type && c.ClaimValue == claim.Value); if (existing != null) user.Claims.Remove(existing); } return Task.CompletedTask; }
        /// <summary>
    /// GetUsersForClaimAsync method.
    /// </summary>
public async Task<IList<TUser>> GetUsersForClaimAsync(Claim claim, CancellationToken cancellationToken) => (await _session.Query<TUser>().Where(u => u.Claims.Any(c => c.ClaimType == claim.Type && c.ClaimValue == claim.Value)).ToListAsync(cancellationToken)).ToList();

    // IUserRoleStore
        /// <summary>
    /// AddToRoleAsync method.
    /// </summary>
public Task AddToRoleAsync(TUser user, string roleName, CancellationToken cancellationToken) { if (!user.Roles.Any(r => r.Name == roleName)) user.Roles.Add(new AeroRole(roleName)); return Task.CompletedTask; }
        /// <summary>
    /// RemoveFromRoleAsync method.
    /// </summary>
public Task RemoveFromRoleAsync(TUser user, string roleName, CancellationToken cancellationToken) { var role = user.Roles.FirstOrDefault(r => r.Name == roleName); if (role != null) user.Roles.Remove(role); return Task.CompletedTask; }
        /// <summary>
    /// GetRolesAsync method.
    /// </summary>
public Task<IList<string>> GetRolesAsync(TUser user, CancellationToken cancellationToken) => Task.FromResult<IList<string>>(user.Roles.Select(r => r.Name).ToList());
        /// <summary>
    /// IsInRoleAsync method.
    /// </summary>
public Task<bool> IsInRoleAsync(TUser user, string roleName, CancellationToken cancellationToken) => Task.FromResult(user.Roles.Any(r => r.Name == roleName));
        /// <summary>
    /// GetUsersInRoleAsync method.
    /// </summary>
public async Task<IList<TUser>> GetUsersInRoleAsync(string roleName, CancellationToken cancellationToken) => (await _session.Query<TUser>().Where(u => u.Roles.Any(r => r.Name == roleName)).ToListAsync(cancellationToken)).ToList();

    // IUserSecurityStampStore
        /// <summary>
    /// SetSecurityStampAsync method.
    /// </summary>
public Task SetSecurityStampAsync(TUser user, string stamp, CancellationToken cancellationToken) { user.SecurityStamp = stamp; return Task.CompletedTask; }
        /// <summary>
    /// GetSecurityStampAsync method.
    /// </summary>
public Task<string?> GetSecurityStampAsync(TUser user, CancellationToken cancellationToken) => Task.FromResult(user.SecurityStamp);

    // IUserLockoutStore
        /// <summary>
    /// GetLockoutEndDateAsync method.
    /// </summary>
public Task<DateTimeOffset?> GetLockoutEndDateAsync(TUser user, CancellationToken cancellationToken) => Task.FromResult(user.LockoutEnd);
        /// <summary>
    /// SetLockoutEndDateAsync method.
    /// </summary>
public Task SetLockoutEndDateAsync(TUser user, DateTimeOffset? lockoutEnd, CancellationToken cancellationToken) { user.LockoutEnd = lockoutEnd; return Task.CompletedTask; }
        /// <summary>
    /// IncrementAccessFailedCountAsync method.
    /// </summary>
public Task<int> IncrementAccessFailedCountAsync(TUser user, CancellationToken cancellationToken) => Task.FromResult(++user.AccessFailedCount);
        /// <summary>
    /// ResetAccessFailedCountAsync method.
    /// </summary>
public Task ResetAccessFailedCountAsync(TUser user, CancellationToken cancellationToken) { user.AccessFailedCount = 0; return Task.CompletedTask; }
        /// <summary>
    /// GetAccessFailedCountAsync method.
    /// </summary>
public Task<int> GetAccessFailedCountAsync(TUser user, CancellationToken cancellationToken) => Task.FromResult(user.AccessFailedCount);
        /// <summary>
    /// GetLockoutEnabledAsync method.
    /// </summary>
public Task<bool> GetLockoutEnabledAsync(TUser user, CancellationToken cancellationToken) => Task.FromResult(user.LockoutEnabled);
        /// <summary>
    /// SetLockoutEnabledAsync method.
    /// </summary>
public Task SetLockoutEnabledAsync(TUser user, bool enabled, CancellationToken cancellationToken) { user.LockoutEnabled = enabled; return Task.CompletedTask; }

    // IUserTwoFactorStore
        /// <summary>
    /// SetTwoFactorEnabledAsync method.
    /// </summary>
public Task SetTwoFactorEnabledAsync(TUser user, bool enabled, CancellationToken cancellationToken) { user.TwoFactorEnabled = enabled; return Task.CompletedTask; }
        /// <summary>
    /// GetTwoFactorEnabledAsync method.
    /// </summary>
public Task<bool> GetTwoFactorEnabledAsync(TUser user, CancellationToken cancellationToken) => Task.FromResult(user.TwoFactorEnabled);

    // IUserPhoneNumberStore
        /// <summary>
    /// SetPhoneNumberAsync method.
    /// </summary>
public Task SetPhoneNumberAsync(TUser user, string? phoneNumber, CancellationToken cancellationToken) { user.PhoneNumber = phoneNumber; return Task.CompletedTask; }
        /// <summary>
    /// GetPhoneNumberAsync method.
    /// </summary>
public Task<string?> GetPhoneNumberAsync(TUser user, CancellationToken cancellationToken) => Task.FromResult(user.PhoneNumber);
        /// <summary>
    /// GetPhoneNumberConfirmedAsync method.
    /// </summary>
public Task<bool> GetPhoneNumberConfirmedAsync(TUser user, CancellationToken cancellationToken) => Task.FromResult(user.PhoneNumberConfirmed);
        /// <summary>
    /// SetPhoneNumberConfirmedAsync method.
    /// </summary>
public Task SetPhoneNumberConfirmedAsync(TUser user, bool confirmed, CancellationToken cancellationToken) { user.PhoneNumberConfirmed = confirmed; return Task.CompletedTask; }

    // IUserAuthenticatorKeyStore
        /// <summary>
    /// SetAuthenticatorKeyAsync method.
    /// </summary>
public Task SetAuthenticatorKeyAsync(TUser user, string key, CancellationToken cancellationToken) { user.TwoFactorAuthenticatorKey = key; return Task.CompletedTask; }
        /// <summary>
    /// GetAuthenticatorKeyAsync method.
    /// </summary>
public Task<string?> GetAuthenticatorKeyAsync(TUser user, CancellationToken cancellationToken) => Task.FromResult(user.TwoFactorAuthenticatorKey);

    // IUserAuthenticationTokenStore
        /// <summary>
    /// SetTokenAsync method.
    /// </summary>
public Task SetTokenAsync(TUser user, string loginProvider, string name, string? value, CancellationToken cancellationToken) { var token = user.Tokens.FirstOrDefault(t => t.LoginProvider == loginProvider && t.Name == name); if (token != null) token.Value = value; else user.Tokens.Add(new IdentityToken { LoginProvider = loginProvider, Name = name, Value = value }); return Task.CompletedTask; }
        /// <summary>
    /// RemoveTokenAsync method.
    /// </summary>
public Task RemoveTokenAsync(TUser user, string loginProvider, string name, CancellationToken cancellationToken) { var token = user.Tokens.FirstOrDefault(t => t.LoginProvider == loginProvider && t.Name == name); if (token != null) user.Tokens.Remove(token); return Task.CompletedTask; }
        /// <summary>
    /// GetTokenAsync method.
    /// </summary>
public Task<string?> GetTokenAsync(TUser user, string loginProvider, string name, CancellationToken cancellationToken) => Task.FromResult(user.Tokens.FirstOrDefault(t => t.LoginProvider == loginProvider && t.Name == name)?.Value);

    // IUserTwoFactorRecoveryCodeStore
        /// <summary>
    /// ReplaceCodesAsync method.
    /// </summary>
public Task ReplaceCodesAsync(TUser user, IEnumerable<string> recoveryCodes, CancellationToken cancellationToken) { user.TwoFactorRecoveryCodes = recoveryCodes.ToList(); return Task.CompletedTask; }
        /// <summary>
    /// RedeemCodeAsync method.
    /// </summary>
public Task<bool> RedeemCodeAsync(TUser user, string code, CancellationToken cancellationToken) => Task.FromResult(user.TwoFactorRecoveryCodes.Remove(code));
        /// <summary>
    /// CountCodesAsync method.
    /// </summary>
public Task<int> CountCodesAsync(TUser user, CancellationToken cancellationToken) => Task.FromResult(user.TwoFactorRecoveryCodes.Count);

        /// <summary>
    /// Dispose method.
    /// </summary>
public void Dispose() { }
}
