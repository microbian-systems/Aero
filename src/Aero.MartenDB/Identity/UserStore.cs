using System.Security.Claims;
using Aero.Core;
using Aero.Core.Identity;
using Aero.Models.Entities;
using Marten;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Aero.MartenDB.Identity;

/// <summary>
/// Marten document used to enforce email uniqueness cluster-wide.
/// Configure a unique index on this collection in your Marten schema setup:
///   options.Schema.For&lt;UserEmailReservation&gt;().Identity(x => x.Id);
/// </summary>
public class UserEmailReservation
{
    public string Id { get; set; } = default!; // "email-reservations/{normalizedEmail}"
    public string UserId { get; set; } = default!;
    public DateTimeOffset ReservedAt { get; set; }
}

/// <summary>
/// Marten implementation of the ASP.NET Core Identity UserStore.
/// </summary>
/// <typeparam name="TUser"></typeparam>
/// <typeparam name="TRole"></typeparam>
public class UserStore<TUser, TRole> :
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
    private bool _disposed;
    private readonly Func<IDocumentSession>? getSessionFunc;
    private IDocumentSession? session;
    private readonly ILogger logger;

    /// <summary>
    /// Creates a new user store that uses the Marten document session returned from the specified session fetcher.
    /// </summary>
    public UserStore(Func<IDocumentSession> getSession, ILogger<UserStore<TUser, TRole>> logger)
    {
        this.getSessionFunc = getSession;
        this.logger = logger;
    }

    /// <summary>
    /// Creates a new user store that uses the specified Marten document session.
    /// </summary>
    public UserStore(IDocumentSession session, ILogger<UserStore<TUser, TRole>> logger)
    {
        this.session = session;
        this.logger = logger;
    }

    //#region IDisposable implementation

    public virtual void Dispose()
    {
        _disposed = true;
    }

    //#endregion

    //#region AutoSaveChanges implementation

    private async Task SaveChangesAsync()
    {
        //if (options.Value.AutoSaveChanges)
        {
            await DbSession.SaveChangesAsync();
        }
    }

    //#endregion

    //#region IUserStore implementation

    /// <inheritdoc />
    public virtual Task<string> GetUserIdAsync(TUser user, CancellationToken cancellationToken)
    {
        if (user == null) throw new ArgumentNullException(nameof(user));
        var id = user.Id  ==  0
            ?Snowflake.NewId();
            : user.Id;
        return Task.FromResult(id);
    }

    /// <inheritdoc />
    public virtual Task<string?> GetUserNameAsync(TUser user, CancellationToken cancellationToken) =>
        Task.FromResult(user.UserName);

    /// <inheritdoc />
    public virtual Task SetUserNameAsync(TUser user, string? userName, CancellationToken cancellationToken)
    {
        user.UserName = userName;
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public virtual Task<string?> GetNormalizedUserNameAsync(TUser user, CancellationToken cancellationToken) =>
        Task.FromResult(user.UserName);

    /// <inheritdoc />
    public virtual Task SetNormalizedUserNameAsync(TUser user, string? normalizedName, CancellationToken cancellationToken)
    {
        user.UserName = normalizedName?.ToLowerInvariant();
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public virtual async Task<IdentityResult> CreateAsync(TUser user, CancellationToken cancellationToken)
    {
        ThrowIfNullDisposedCancelled(user, cancellationToken);

        var email = user.Email?.ToLowerInvariant() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("The user's email address can't be null or empty.", nameof(user));
        }

        user.Email = email;
        user.UserName = user.UserName?.ToLowerInvariant() ?? email;

        // Step 1: Reserve the email address via a dedicated document.
        // NOTE: For true atomicity, configure a unique index on UserEmailReservation in your Marten schema.
        logger.LogDebug("Creating email reservation for {UserEmail}", email);
        var reserved = await TryCreateEmailReservationAsync(email, string.Empty, cancellationToken);
        if (!reserved)
        {
            logger.LogError("Error creating email reservation for {Email}", email);
            return IdentityResult.Failed(new IdentityErrorDescriber().DuplicateEmail(email));
        }

        try
        {
            // Step 2: Store and save the user.
            DbSession.Store(user);
            await DbSession.SaveChangesAsync(cancellationToken);

            // Step 3: Update the reservation to point to the real user ID.
            await UpdateEmailReservationAsync(email, user.Id.ToString(), cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during user creation");
            DbSession.Delete(user);
            try
            {
                await DeleteEmailReservationAsync(email, cancellationToken);
            }
            catch (Exception innerEx)
            {
                logger.LogError(innerEx,
                    "Failed to remove email reservation for {Email} after failed user creation. " +
                    "Manually delete the reservation document '{ReservationId}'.",
                    email, EmailReservationIdFor(email));
            }

            return IdentityResult.Failed(new IdentityErrorDescriber().DefaultError());
        }

        return IdentityResult.Success;
    }

    /// <inheritdoc />
    public virtual async Task<IdentityResult> UpdateAsync(TUser user, CancellationToken cancellationToken)
    {
        ThrowIfNullDisposedCancelled(user, cancellationToken);

        if (string.IsNullOrWhiteSpace(user.Email))
            throw new ArgumentException("The user's email address can't be null or empty.", nameof(user));
        if (string.IsNullOrWhiteSpace(user.Id?.ToString()))
            throw new ArgumentException("The user can't have a null ID.");

        // Load the existing user to detect email changes.
        var existing = await DbSession.LoadAsync<TUser>(user.Id.ToString(), cancellationToken);
        var oldEmail = existing?.Email?.ToLowerInvariant() ?? string.Empty;
        var newEmail = user.Email.ToLowerInvariant();

        // If the email has not changed (ignoring case), just save.
        if (string.Equals(oldEmail, newEmail, StringComparison.OrdinalIgnoreCase))
        {
            logger.LogTrace("User {UserId} email unchanged, saving normally", user.Id);
            DbSession.Store(user);
            await SaveChangesAsync();
            return IdentityResult.Success;
        }

        // Update username to match new email if it was previously set to the old email.
        if (string.Equals(user.UserName, oldEmail, StringComparison.OrdinalIgnoreCase))
        {
            logger.LogTrace("Updating username to match modified email for {UserId}", user.Id);
            user.UserName = newEmail;
        }

        // Reserve the new email address.
        var reserved = await TryCreateEmailReservationAsync(newEmail, user.Id.ToString(), cancellationToken);
        if (!reserved)
        {
            logger.LogWarning("Duplicate email detected on update for {UserId}: {Email}", user.Id, newEmail);
            return IdentityResult.Failed(new IdentityErrorDescriber().DuplicateEmail(newEmail));
        }

        // Remove the old email reservation.
        await TryDeleteEmailReservationAsync(oldEmail, cancellationToken);

        DbSession.Store(user);
        await SaveChangesAsync();
        return IdentityResult.Success;
    }

    /// <inheritdoc />
    public virtual async Task<IdentityResult> DeleteAsync(TUser user, CancellationToken cancellationToken)
    {
        ThrowIfNullDisposedCancelled(user, cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();

        DbSession.Delete(user);
        await DbSession.SaveChangesAsync(cancellationToken);

        var deleted = await TryDeleteEmailReservationAsync(user.Email, cancellationToken);
        if (!deleted)
        {
            logger.LogWarning(
                "User was deleted, but there was an error deleting the email reservation for {Email}. " +
                "Manually delete the reservation document '{ReservationId}'.",
                user.Email, EmailReservationIdFor(user.Email!));
        }

        return IdentityResult.Success;
    }

    /// <inheritdoc />
    public virtual Task<TUser> FindByIdAsync(string userId, CancellationToken cancellationToken) =>
        DbSession.LoadAsync<TUser>(userId, cancellationToken);

    /// <inheritdoc />
    public virtual Task<TUser?> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken) =>
        UserQuery()
            .Where(u => u.UserName == normalizedUserName)
            .FirstOrDefaultAsync(cancellationToken);

    //#endregion

    //#region IUserLoginStore implementation

    /// <inheritdoc />
    public virtual async Task AddLoginAsync(TUser user, UserLoginInfo login, CancellationToken cancellationToken)
    {
        ThrowIfNullDisposedCancelled(user, cancellationToken);
        ArgumentNullException.ThrowIfNull(login);

        user.Logins.Add(new IdentityUserLogin<string>
        {
            LoginProvider = login.LoginProvider,
            ProviderKey = login.ProviderKey,
            UserId = user.Id
        });

        await SaveChangesAsync();
    }

    /// <inheritdoc />
    public virtual async Task RemoveLoginAsync(TUser user, string loginProvider, string providerKey, CancellationToken cancellationToken)
    {
        ThrowIfNullDisposedCancelled(user, cancellationToken);

        var login = user.Logins.FirstOrDefault(l => l.LoginProvider == loginProvider && l.ProviderKey == providerKey);
        if (login != null)
        {
            user.Logins.Remove(login);
        }

        await SaveChangesAsync();
    }

    /// <inheritdoc />
    public virtual Task<IList<UserLoginInfo>> GetLoginsAsync(TUser user, CancellationToken cancellationToken)
    {
        ThrowIfNullDisposedCancelled(user, cancellationToken);
        return Task.FromResult(user.Logins as IList<UserLoginInfo>);
    }

    /// <inheritdoc />
    public virtual async Task<TUser?> FindByLoginAsync(string loginProvider, string providerKey, CancellationToken cancellationToken)
    {
        //if (options.Value.UseStaticIndexes)
        {
            return await DbSession
                .Query<TUser>()
                .Where(u => u.Logins.Any(l => l.LoginProvider == loginProvider && l.ProviderKey == providerKey))
                .FirstOrDefaultAsync(cancellationToken);
        }

        var candidates = await DbSession.Query<TUser>()
            .Where(u => u.Logins.Any(l => l.ProviderKey == providerKey))
            .ToListAsync(cancellationToken);

        return candidates.FirstOrDefault(u => u.Logins.Any(l => l.LoginProvider == loginProvider));
    }

    //#endregion

    //#region IUserClaimStore implementation

    /// <inheritdoc />
    public virtual Task<IList<Claim>> GetClaimsAsync(TUser user, CancellationToken cancellationToken)
    {
        ThrowIfNullDisposedCancelled(user, cancellationToken);

        IList<Claim> result = user.Claims
            .Select(c => new Claim(c.ClaimType, c.ClaimValue))
            .ToList();

        return Task.FromResult(result);
    }

    /// <inheritdoc />
    public virtual async Task AddClaimsAsync(TUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken)
    {
        ThrowIfNullDisposedCancelled(user, cancellationToken);

        foreach (var c in claims)
        {
            user.Claims.Add(new IdentityUserClaim<string>
            {
                ClaimType = c.Type,
                ClaimValue = c.Value,
                UserId = user.Id
            });
        }

        await SaveChangesAsync();
    }

    /// <inheritdoc />
    public virtual async Task ReplaceClaimAsync(TUser user, Claim claim, Claim newClaim, CancellationToken cancellationToken)
    {
        ThrowIfNullDisposedCancelled(user, cancellationToken);

        var claimList = user.Claims.As<List<IdentityUserClaim<string>>>();
        var index = claimList.FindIndex(c => c.ClaimType == claim.Type && c.ClaimValue == claim.Value);
        if (index != -1)
        {
            claimList.RemoveAt(index);
            claimList.Add(new IdentityUserClaim<string>
            {
                ClaimType = newClaim.Type,
                ClaimValue = newClaim.Value,
                UserId = user.Id
            });
        }

        await SaveChangesAsync();
    }

    /// <inheritdoc />
    public virtual async Task RemoveClaimsAsync(TUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken)
    {
        ThrowIfNullDisposedCancelled(user, cancellationToken);

        user.Claims.As<List<IdentityUserClaim<string>>>()
            .RemoveAll(ic => claims.Any(c => c.Type == ic.ClaimType && c.Value == ic.ClaimValue));

        await SaveChangesAsync();
    }

    /// <inheritdoc />
    public virtual async Task<IList<TUser>> GetUsersForClaimAsync(Claim claim, CancellationToken cancellationToken)
    {
        ThrowIfDisposedOrCancelled(cancellationToken);
        ArgumentNullException.ThrowIfNull(claim);

        var list = await UserQuery()
            .Where(u => u.Claims.Any(c => c.ClaimType == claim.Type && c.ClaimValue == claim.Value))
            .ToListAsync(cancellationToken);

        return list.ToList();
    }

    //#endregion

    //#region IUserRoleStore implementation

    /// <inheritdoc />
    public virtual async Task AddToRoleAsync(TUser user, string roleName, CancellationToken cancellationToken)
    {
        ThrowIfNullDisposedCancelled(user, cancellationToken);

        var roleId = RoleIdFor(roleName);
        var existingRoleOrNull = await DbSession.LoadAsync<AeroRole>(roleId, cancellationToken);

        if (existingRoleOrNull == null)
        {
            ThrowIfDisposedOrCancelled(cancellationToken);
            existingRoleOrNull = new TRole { Name = roleName.ToLowerInvariant() };
            //DbSession.Store(existingRoleOrNull, roleId);
            DbSession.Store(existingRoleOrNull);
        }

        var roleRealName = existingRoleOrNull.Name;
        if (!user.GetRolesList().Contains(roleRealName, StringComparer.InvariantCultureIgnoreCase))
        {
            user.GetRolesList().Add(roleRealName);
        }

        if (!existingRoleOrNull.Users.Contains(user.Id))
        {
            existingRoleOrNull.Users.Add(user.Id);
        }

        await SaveChangesAsync();
    }

    /// <inheritdoc />
    public virtual async Task RemoveFromRoleAsync(TUser user, string roleName, CancellationToken cancellationToken)
    {
        ThrowIfNullDisposedCancelled(user, cancellationToken);

        user.GetRolesList().RemoveAll(r => string.Equals(r, roleName, StringComparison.InvariantCultureIgnoreCase));

        var roleId = RoleIdFor(roleName);
        var roleOrNull = await DbSession.LoadAsync<TRole>(roleId, cancellationToken);
        if (roleOrNull != null)
        {
            roleOrNull.Users.Remove(user.Id);
        }

        await SaveChangesAsync();
    }

    /// <inheritdoc />
    public virtual Task<IList<string>> GetRolesAsync(TUser user, CancellationToken cancellationToken)
    {
        ThrowIfNullDisposedCancelled(user, cancellationToken);
        return Task.FromResult<IList<string>>(new List<string>(user.GetRolesList()));
    }

    /// <inheritdoc />
    public virtual Task<bool> IsInRoleAsync(TUser user, string roleName, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(roleName)) throw new ArgumentNullException(nameof(roleName));
        return Task.FromResult(user.GetRolesList().Contains(roleName, StringComparer.InvariantCultureIgnoreCase));
    }

    /// <inheritdoc />
    public virtual async Task<IList<TUser>> GetUsersInRoleAsync(string roleName, CancellationToken cancellationToken)
    {
        ThrowIfDisposedOrCancelled(cancellationToken);
        if (string.IsNullOrEmpty(roleName)) throw new ArgumentNullException(nameof(roleName));

        var users = await UserQuery()
            .Where(u => u.RoleNames.Contains(roleName))
            .ToListAsync(cancellationToken);

        return users.ToList();
    }

    //#endregion

    //#region IUserPasswordStore implementation

    /// <inheritdoc />
    public virtual Task SetPasswordHashAsync(TUser user, string? passwordHash, CancellationToken cancellationToken)
    {
        ThrowIfNullDisposedCancelled(user, cancellationToken);
        user.PasswordHash = passwordHash;
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public virtual Task<string?> GetPasswordHashAsync(TUser user, CancellationToken cancellationToken)
    {
        ThrowIfNullDisposedCancelled(user, cancellationToken);
        return Task.FromResult(user.PasswordHash);
    }

    /// <inheritdoc />
    public virtual Task<bool> HasPasswordAsync(TUser user, CancellationToken cancellationToken)
    {
        ThrowIfNullDisposedCancelled(user, cancellationToken);
        return Task.FromResult(user.PasswordHash != null);
    }

    //#endregion

    //#region IUserSecurityStampStore implementation

    /// <inheritdoc />
    public virtual Task SetSecurityStampAsync(TUser user, string? stamp, CancellationToken cancellationToken)
    {
        ThrowIfNullDisposedCancelled(user, cancellationToken);
        user.SecurityStamp = stamp;
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public virtual Task<string?> GetSecurityStampAsync(TUser user, CancellationToken cancellationToken)
    {
        ThrowIfNullDisposedCancelled(user, cancellationToken);
        return Task.FromResult(user.SecurityStamp);
    }

    //#endregion

    //#region IUserEmailStore implementation

    /// <inheritdoc />
    public virtual Task SetEmailAsync(TUser user, string? email, CancellationToken cancellationToken)
    {
        ThrowIfDisposedOrCancelled(cancellationToken);
        user.Email = email?.ToLowerInvariant();
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public virtual Task<string?> GetEmailAsync(TUser user, CancellationToken cancellationToken) =>
        Task.FromResult(user.Email);

    /// <inheritdoc />
    public virtual Task<bool> GetEmailConfirmedAsync(TUser user, CancellationToken cancellationToken) =>
        Task.FromResult(user.EmailConfirmed);

    /// <inheritdoc />
    public virtual Task SetEmailConfirmedAsync(TUser user, bool confirmed, CancellationToken cancellationToken)
    {
        user.EmailConfirmed = confirmed;
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public virtual async Task<TUser?> FindByEmailAsync(string? normalizedEmail, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(normalizedEmail)) return null;

        var email = normalizedEmail.ToLowerInvariant();

        //if (options.Value.UseStaticIndexes)
        {
            return await DbSession
                .Query<AeroUser>()
                .Where(u => u.Email == email)
                .OfType<TUser>()
                .FirstOrDefaultAsync(cancellationToken);
        }

        // Use the email reservation document as the authoritative lookup, bypassing
        // potentially stale indexes. Falls back to a direct query if no reservation exists.
        var reservation = await DbSession.LoadAsync<UserEmailReservation>(EmailReservationIdFor(email), cancellationToken);
        if (reservation != null && !string.IsNullOrEmpty(reservation.UserId))
        {
            return await DbSession.LoadAsync<TUser>(reservation.UserId, cancellationToken);
        }

        return await DbSession.Query<TUser>()
            .Where(u => u.Email == email)
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <inheritdoc />
    public virtual Task<string?> GetNormalizedEmailAsync(TUser user, CancellationToken cancellationToken) =>
        Task.FromResult(user.Email);

    /// <inheritdoc />
    public virtual Task SetNormalizedEmailAsync(TUser user, string? normalizedEmail, CancellationToken cancellationToken)
    {
        user.Email = normalizedEmail?.ToLowerInvariant();
        return Task.CompletedTask;
    }

    //#endregion

    //#region IUserLockoutStore implementation

    /// <inheritdoc />
    public virtual Task<DateTimeOffset?> GetLockoutEndDateAsync(TUser user, CancellationToken cancellationToken)
    {
        ThrowIfNullDisposedCancelled(user, cancellationToken);
        return Task.FromResult(user.LockoutEnd);
    }

    /// <inheritdoc />
    public virtual Task SetLockoutEndDateAsync(TUser user, DateTimeOffset? lockoutEnd, CancellationToken cancellationToken)
    {
        ThrowIfNullDisposedCancelled(user, cancellationToken);
        user.LockoutEnd = lockoutEnd;
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public virtual Task<int> IncrementAccessFailedCountAsync(TUser user, CancellationToken cancellationToken)
    {
        ThrowIfNullDisposedCancelled(user, cancellationToken);
        user.AccessFailedCount++;
        return Task.FromResult(user.AccessFailedCount);
    }

    /// <inheritdoc />
    public virtual Task ResetAccessFailedCountAsync(TUser user, CancellationToken cancellationToken)
    {
        ThrowIfNullDisposedCancelled(user, cancellationToken);
        user.AccessFailedCount = 0;
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public virtual Task<int> GetAccessFailedCountAsync(TUser user, CancellationToken cancellationToken)
    {
        ThrowIfNullDisposedCancelled(user, cancellationToken);
        return Task.FromResult(user.AccessFailedCount);
    }

    /// <inheritdoc />
    public virtual Task<bool> GetLockoutEnabledAsync(TUser user, CancellationToken cancellationToken)
    {
        ThrowIfNullDisposedCancelled(user, cancellationToken);
        return Task.FromResult(user.LockoutEnabled);
    }

    /// <inheritdoc />
    public virtual Task SetLockoutEnabledAsync(TUser user, bool enabled, CancellationToken cancellationToken)
    {
        ThrowIfNullDisposedCancelled(user, cancellationToken);
        user.LockoutEnabled = enabled;
        return Task.CompletedTask;
    }

    //#endregion

    //#region IUserTwoFactorStore implementation

    /// <inheritdoc />
    public virtual Task SetTwoFactorEnabledAsync(TUser user, bool enabled, CancellationToken cancellationToken)
    {
        ThrowIfNullDisposedCancelled(user, cancellationToken);
        user.TwoFactorEnabled = enabled;
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public virtual Task<bool> GetTwoFactorEnabledAsync(TUser user, CancellationToken cancellationToken)
    {
        ThrowIfNullDisposedCancelled(user, cancellationToken);
        return Task.FromResult(user.TwoFactorEnabled);
    }

    //#endregion

    //#region IUserPhoneNumberStore implementation

    /// <inheritdoc />
    public virtual Task SetPhoneNumberAsync(TUser user, string? phoneNumber, CancellationToken cancellationToken)
    {
        user.PhoneNumber = phoneNumber;
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public virtual Task<string?> GetPhoneNumberAsync(TUser user, CancellationToken cancellationToken) =>
        Task.FromResult(user.PhoneNumber);

    /// <inheritdoc />
    public virtual Task<bool> GetPhoneNumberConfirmedAsync(TUser user, CancellationToken cancellationToken) =>
        Task.FromResult(user.PhoneNumberConfirmed);

    /// <inheritdoc />
    public virtual Task SetPhoneNumberConfirmedAsync(TUser user, bool confirmed, CancellationToken cancellationToken)
    {
        user.PhoneNumberConfirmed = confirmed;
        return Task.CompletedTask;
    }

    //#endregion

    //#region IUserAuthenticatorKeyStore implementation

    /// <inheritdoc />
    public virtual Task SetAuthenticatorKeyAsync(TUser user, string? key, CancellationToken cancellationToken)
    {
        user.TwoFactorAuthenticatorKey = key;
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public virtual Task<string?> GetAuthenticatorKeyAsync(TUser user, CancellationToken cancellationToken) =>
        Task.FromResult(user.TwoFactorAuthenticatorKey);

    //#endregion

    //#region IUserAuthenticationTokenStore implementation

    /// <inheritdoc />
    public virtual async Task SetTokenAsync(TUser user, string loginProvider, string name, string? value, CancellationToken cancellationToken)
    {
        var existingToken = user.Tokens.FirstOrDefault(t => t.LoginProvider == loginProvider && t.Name == name);
        if (existingToken != null)
        {
            existingToken.Value = value;
        }
        else
        {
            user.Tokens.Add(new IdentityUserAuthToken
            {
                UserId = user.Id,
                LoginProvider = loginProvider,
                Name = name,
                Value = value
            });
        }

        await SaveChangesAsync();
    }

    /// <inheritdoc />
    public virtual async Task RemoveTokenAsync(TUser user, string loginProvider, string name, CancellationToken cancellationToken)
    {
        if (user.Tokens is List<IdentityUserToken<string>> tokens)
            tokens.RemoveAll(t => t.LoginProvider == loginProvider && t.Name == name);

        await SaveChangesAsync();
    }

    /// <inheritdoc />
    public virtual Task<string?> GetTokenAsync(TUser user, string loginProvider, string name, CancellationToken cancellationToken)
    {
        var tokenOrNull = user.Tokens.FirstOrDefault(t => t.LoginProvider == loginProvider && t.Name == name);
        return Task.FromResult(tokenOrNull?.Value);
    }

    //#endregion

    //#region IUserTwoFactorRecoveryCodeStore implementation

    /// <inheritdoc />
    public virtual Task ReplaceCodesAsync(TUser user, IEnumerable<string> recoveryCodes, CancellationToken cancellationToken)
    {
        user.TwoFactorRecoveryCodes = new List<string>(recoveryCodes);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public virtual Task<bool> RedeemCodeAsync(TUser user, string code, CancellationToken cancellationToken) =>
        Task.FromResult(user.TwoFactorRecoveryCodes.Remove(code));

    /// <inheritdoc />
    public virtual Task<int> CountCodesAsync(TUser user, CancellationToken cancellationToken) =>
        Task.FromResult(user.TwoFactorRecoveryCodes.Count);

    //#endregion

    //#region IQueryableUserStore implementation

    /// <summary>
    /// Gets the users as an IQueryable.
    /// </summary>
    public virtual IQueryable<TUser> Users => DbSession.Query<TUser>();

    //#endregion

    /// <summary>
    /// Gets the current Marten document session.
    /// </summary>
    protected IDocumentSession DbSession
    {
        get
        {
            if (session == null)
            {
                session = getSessionFunc!();
            }
            return session;
        }
    }

    //#region Email reservation helpers

    /// <summary>
    /// Returns the Marten document ID used to reserve an email address.
    /// </summary>
    private static string EmailReservationIdFor(string email) =>
        $"email-reservations/{email.ToLowerInvariant()}";

    /// <summary>
    /// Attempts to reserve an email address. Returns false if it is already taken.
    /// </summary>
    private async Task<bool> TryCreateEmailReservationAsync(string email, string userId, CancellationToken cancellationToken)
    {
        var id = EmailReservationIdFor(email);
        var existing = await DbSession.LoadAsync<UserEmailReservation>(id, cancellationToken);
        if (existing != null)
        {
            return false;
        }

        DbSession.Store(new UserEmailReservation
        {
            Id = id,
            UserId = userId,
            ReservedAt = DateTimeOffset.UtcNow
        });

        return true;
    }

    /// <summary>
    /// Updates an existing email reservation to point to the given user ID.
    /// </summary>
    private async Task UpdateEmailReservationAsync(string email, string userId, CancellationToken cancellationToken)
    {
        var id = EmailReservationIdFor(email);
        var reservation = await DbSession.LoadAsync<UserEmailReservation>(id, cancellationToken);
        if (reservation != null)
        {
            reservation.UserId = userId;
            DbSession.Store(reservation);
            await DbSession.SaveChangesAsync(cancellationToken);
        }
    }

    /// <summary>
    /// Deletes the email reservation document. Throws on failure.
    /// </summary>
    private async Task DeleteEmailReservationAsync(string email, CancellationToken cancellationToken)
    {
        var id = EmailReservationIdFor(email);
        var reservation = await DbSession.LoadAsync<UserEmailReservation>(id, cancellationToken);
        if (reservation != null)
        {
            DbSession.Delete(reservation);
            await DbSession.SaveChangesAsync(cancellationToken);
        }
    }

    /// <summary>
    /// Attempts to delete the email reservation. Logs a warning on failure but does not throw.
    /// </summary>
    private async Task<bool> TryDeleteEmailReservationAsync(string? email, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(email)) return true;

        try
        {
            await DeleteEmailReservationAsync(email, cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex,
                "Failed to remove email reservation for {Email}. " +
                "Manually delete the reservation document '{ReservationId}'.",
                email, EmailReservationIdFor(email));
            return false;
        }
    }

    //#endregion

    //#region Role ID helpers

    /// <summary>
    /// Produces a stable Marten document ID for a given role name.
    /// </summary>
    private static string RoleIdFor(string roleName) =>
        $"roles/{roleName.ToLowerInvariant()}";

    //#endregion

    //#region Guard helpers

    private void ThrowIfNullDisposedCancelled(TUser user, CancellationToken token)
    {
        if (_disposed) throw new ObjectDisposedException(GetType().Name);
        if (user == null) throw new ArgumentNullException(nameof(user));
        token.ThrowIfCancellationRequested();
    }

    private void ThrowIfDisposedOrCancelled(CancellationToken token)
    {
        if (_disposed) throw new ObjectDisposedException(GetType().Name);
        token.ThrowIfCancellationRequested();
    }

    //#endregion

    //#region Query helpers

    /// <summary>
    /// Creates either a static-index query or a dynamic query depending on options.
    /// </summary>
    private IQueryable<TUser> UserQuery() => DbSession.Query<TUser>();

    //#endregion
}