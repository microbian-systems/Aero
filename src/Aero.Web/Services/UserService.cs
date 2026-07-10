using Aero.Common.Web.Extensions;
using Aero.Core.Identity;
using Aero.Models.Entities;
using Aero.Services;
using Aero.Services.Geo;
using Aero.Services.Models;
using FluentEmail.Core;
using Microsoft.AspNetCore.Identity;

namespace Aero.Common.Web.Services;

/// <summary>
/// Defines an interface for IAeroUserService.
/// </summary>
public interface IAeroUserService : IAeroUserService<AeroUser> { }

/// <summary>
/// Represents a class for AeroUserService.
/// </summary>
public class AeroUserService : AeroUserServiceBase<AeroUser>, IAeroUserService
{
        /// <summary>
    /// Initializes a new instance of the <see cref="AeroUserService"/> class.
    /// </summary>
public AeroUserService(
        SignInManager<AeroUser> signinManager,
        UserManager<AeroUser> userManager,
        RoleManager<AeroRole> roleManager,
        IPasswordService passwordService,
        IHttpContextAccessor contextAccessor,
        IFluentEmail fluentEmail,
        IZipApiService zipService,
        ILogger<AeroUserService> log)
        : base(signinManager, userManager, roleManager, passwordService, contextAccessor, fluentEmail, zipService, log)
    {
    }
}

/// <summary>
/// Defines an interface for IAeroUserService.
/// </summary>
public interface IAeroUserService<T> : IAeroIdentityService<T>
    where T : AeroUser, new()
{
        /// <summary>
    /// GetCurrentUserId method.
    /// </summary>
string GetCurrentUserId();

        /// <summary>
    /// GetCurrentUserEmail method.
    /// </summary>
string GetCurrentUserEmail();

        /// <summary>
    /// GetCurrentUser method.
    /// </summary>
Task<T> GetCurrentUser();

        /// <summary>
    /// ChangePassword method.
    /// </summary>
Task<bool> ChangePassword(string current, string updated, T user);

        /// <summary>
    /// VerifyPassword method.
    /// </summary>
Task<bool> VerifyPassword(string password, T user);
}

/// <summary>
/// Represents a class for AeroUserServiceBase.
/// </summary>
public class AeroUserServiceBase<T> : AeroIdentityService<T>
    where T : AeroUser, new()
{
        /// <summary>
    /// context.
    /// </summary>
protected readonly HttpContext context;

        /// <summary>
    /// Initializes a new instance of the <see cref="AeroUserServiceBase"/> class.
    /// </summary>
protected AeroUserServiceBase(
        SignInManager<T> signinManager,
        UserManager<T> userManager,
        RoleManager<AeroRole> roleManager,
        IPasswordService passwordService,
        IHttpContextAccessor contextAccessor,
        IFluentEmail fluentEmail,
        IZipApiService zipService,
        ILogger<AeroUserServiceBase<T>> log)
        : base(signinManager, userManager, roleManager, passwordService, contextAccessor, fluentEmail, zipService, log)
    {
        ThrowGuard.Throw.IfNull(contextAccessor?.HttpContext, nameof(contextAccessor));
        context = contextAccessor?.HttpContext!;
    }

        /// <summary>
    /// GetCurrentUserId method.
    /// </summary>
public string GetCurrentUserId() => context.User.GetUserId();

        /// <summary>
    /// GetCurrentUserEmail method.
    /// </summary>
public string GetCurrentUserEmail() => context.User.GetUserEmail();

        /// <summary>
    /// GetCurrentUser method.
    /// </summary>
public async Task<T> GetCurrentUser()
    {
        var id = context.User.GetUserId();
        var user = await userManager.FindByIdAsync(id);

        return user;
    }

        /// <summary>
    /// ChangePassword method.
    /// </summary>
public async Task<bool> ChangePassword(string current, string updated, T user = null)
    {
        user ??= await GetCurrentUser();
        var res = await base.ChangePassword(user, current, updated);

        return res;
    }

        /// <summary>
    /// LoginAsync method.
    /// </summary>
public override async Task<UserViewModel> LoginAsync(string username, string password)
    {
        // todo - implement Login in UserService
        throw new NotImplementedException();
    }

        /// <summary>
    /// VerifyPassword method.
    /// </summary>
public override async Task<bool> VerifyPassword(string password, T user = null)
    {
        user ??= await GetCurrentUser();
        var res = await base.VerifyPassword(password, user);
        return res;
    }
}