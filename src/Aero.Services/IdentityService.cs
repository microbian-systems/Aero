using Aero.Core;
using Aero.Core.Extensions;

namespace Aero.Services;

/// <summary>
/// Defines an interface for IAeroIdentityService.
/// </summary>
public interface IAeroIdentityService : IAeroIdentityService<AeroUser> { }

/// <summary>
/// Defines an interface for IAeroIdentityService.
/// </summary>
public interface IAeroIdentityService<T> : IAeroIdentityService<T, long>
    where T : AeroUser, new()
{ }

/// <summary>
/// Defines an interface for IAeroIdentityService.
/// </summary>
public interface IAeroIdentityService<T, TKey>
    where TKey : IEquatable<TKey>, IComparable<TKey>
{
        /// <summary>
    /// LoginAsync method.
    /// </summary>
Task<UserViewModel> LoginAsync(UserLoginRequest model);
        /// <summary>
    /// LoginAsync method.
    /// </summary>
Task<UserViewModel> LoginAsync(UserLoginRequest model, string password);
        /// <summary>
    /// LoginAsync method.
    /// </summary>
Task<UserViewModel> LoginAsync(string username, string password);
        /// <summary>
    /// LogoutAsync method.
    /// </summary>
Task LogoutAsync(UserViewModel model);
        /// <summary>
    /// LogoutAsync method.
    /// </summary>
Task LogoutAsync(string username);
        /// <summary>
    /// AddUserAsync method.
    /// </summary>
Task<(T user, IdentityResult identityReuslt)> AddUserAsync(T model, string password = "");
        /// <summary>
    /// UpdateUserAsync method.
    /// </summary>
Task<(T user, IdentityResult identityReuslt)> UpdateUserAsync(T model);
        /// <summary>
    /// DeleteUserAsync method.
    /// </summary>
Task<(T user, IdentityResult identityReuslt)> DeleteUserAsync(T model);
        /// <summary>
    /// DeleteUserAsync method.
    /// </summary>
Task<(T user, IdentityResult identityReuslt)> DeleteUserAsync(long id);
        /// <summary>
    /// ChangePassword method.
    /// </summary>
Task<bool> ChangePassword(T user, string current, string updated);
        /// <summary>
    /// GenerateResetPasswordToken method.
    /// </summary>
Task<(bool success, string token, string errorMessage)> GenerateResetPasswordToken(string email);
        /// <summary>
    /// ResetPassword method.
    /// </summary>
Task<(bool success, string token, string[] errors)> ResetPassword(string email, string fromEmail, string url, string subject, string scheme = "https");
        /// <summary>
    /// ResetPasswordConfirmation method.
    /// </summary>
Task<(bool success, string[] errors)> ResetPasswordConfirmation(string email, string token, string password);
        /// <summary>
    /// GetByIdAsync method.
    /// </summary>
Task<T> GetByIdAsync(long id);
        /// <summary>
    /// GetByUsernameAsync method.
    /// </summary>
Task<T> GetByUsernameAsync(string username);
        /// <summary>
    /// GetByEmailAsync method.
    /// </summary>
Task<T> GetByEmailAsync(string email);
        /// <summary>
    /// GetRoles method.
    /// </summary>
Task<IEnumerable<string>> GetRoles(string userId);
        /// <summary>
    /// AddToRole method.
    /// </summary>
Task<IdentityResult> AddToRole(T user, string role);
        /// <summary>
    /// AddToRole method.
    /// </summary>
Task<IdentityResult> AddToRole(long userId, string role);
        /// <summary>
    /// AddToRoles method.
    /// </summary>
Task<IdentityResult> AddToRoles(T user, IEnumerable<string> roles);
        /// <summary>
    /// AddToRoles method.
    /// </summary>
Task<IdentityResult> AddToRoles(string userId, IEnumerable<string> roles);
        /// <summary>
    /// AddClaim method.
    /// </summary>
Task<IdentityResult> AddClaim(T user, Claim claim);
        /// <summary>
    /// AddClaim method.
    /// </summary>
Task<IdentityResult> AddClaim(long userId, Claim claim);
        /// <summary>
    /// AddClaimsAsync method.
    /// </summary>
Task<IdentityResult> AddClaimsAsync(T user, IEnumerable<Claim> claims);
        /// <summary>
    /// AddClaimsAsync method.
    /// </summary>
Task<IdentityResult> AddClaimsAsync(long userId, IEnumerable<Claim> claims);
        /// <summary>
    /// GetClaims method.
    /// </summary>
Task<IDictionary<string, string>> GetClaims(long userId);
        /// <summary>
    /// Register method.
    /// </summary>
Task<(T model, IdentityResult identityResult)> Register(RegistrationRequestModel model, string createdBy = "User");
        /// <summary>
    /// Register method.
    /// </summary>
Task<(T model, IdentityResult identityResult)> Register(T user, string password, string createdBy = "User");
        /// <summary>
    /// SaveRefreshTokenAsync method.
    /// </summary>
Task<bool> SaveRefreshTokenAsync(string username, string token);
        /// <summary>
    /// DeleteRefreshTokenAsync method.
    /// </summary>
Task<bool> DeleteRefreshTokenAsync(string username, string refreshToken);
        /// <summary>
    /// VerifyPassword method.
    /// </summary>
Task<bool> VerifyPassword(string username, string password);
}

/// <summary>
/// Represents a class for AeroIdentityService.
/// </summary>
public class AeroIdentityService : AeroIdentityService<AeroUser, long>, IAeroIdentityService
{
    // todo - fix Aero.Services.IdentityService.cs (do we still even need?) 
        /// <summary>
    /// Initializes a new instance of the <see cref="AeroIdentityService"/> class.
    /// </summary>
public AeroIdentityService(
        // SignInManager<AeroUser> signinManager,
        // UserManager<AeroUser> userManager,
        // RoleManager<AeroRole> roleManager,
        // IPasswordService passwordService,
        // IHttpContextAccessor contextAccessor,
        // IFluentEmail fluentEmail,
        // IZipApiService zipService,
        ILogger<AeroIdentityService> log)
        : base(default, default, default, default, default, default, default, log)
    {
    }

        /// <summary>
    /// LoginAsync method.
    /// </summary>
public override async Task<UserViewModel> LoginAsync(string username, string password)
    {
        var result = await signinManager
            .PasswordSignInAsync(username, password, false, true);

        // return null if user not found
        if (!result.Succeeded)
            return null;

        var identity = userManager.Users.First(u => string.Equals(u.UserName,
            username, StringComparison.InvariantCultureIgnoreCase));

        var roles = await userManager.GetRolesAsync(identity);
        var claims = await userManager.GetClaimsAsync(identity);

        // var jwt = tokenService.GenerateToken(account,
        //     roles?.Select(role => new Claim(ClaimTypes.Role, role)));
        //
        // var refresh = tokenService.GenerateRefreshToken();
        //var res = await SaveRefreshTokenAsync(account.UserName, refresh);

        var user = new UserViewModel()
        {
            Id = identity.Id,
            FirstName = identity.FirstName,
            LastName = identity.LastName,
            Username = identity.UserName ??= string.Empty,
            Email = identity.Email ??= string.Empty
            // Token = jwt,
            // RefreshToken = refresh
        };

        user.Roles.AddRange(roles ?? new List<string>());
        user.Claims.AddRange(claims);

        return user;
    }
}

/// <summary>
/// Represents a class for AeroIdentityService.
/// </summary>
public abstract class AeroIdentityService<T>(
    SignInManager<T> signinManager,
    UserManager<T> userManager,
    RoleManager<AeroRole> roleManager,
    IPasswordService passwordService,
    IHttpContextAccessor contextAccessor,
    IFluentEmail fluentEmail,
    IZipApiService zipService,
    ILogger<AeroIdentityService<T>> log)
    : AeroIdentityService<T, string>(signinManager, userManager, roleManager, passwordService, contextAccessor,
        fluentEmail, zipService, log)
    where T : AeroUser, new();

/// <summary>
/// Represents a class for AeroIdentityService.
/// </summary>
public abstract class AeroIdentityService<T, TKey> : IAeroIdentityService<T, TKey>
    where T : AeroUser, new()
    where TKey : IEquatable<TKey>, IComparable<TKey>
{
        /// <summary>
    /// userManager.
    /// </summary>
protected readonly UserManager<T> userManager;
        /// <summary>
    /// signinManager.
    /// </summary>
protected readonly SignInManager<T> signinManager;
        /// <summary>
    /// roleManager.
    /// </summary>
protected readonly RoleManager<AeroRole> roleManager;
        /// <summary>
    /// log.
    /// </summary>
protected readonly ILogger<AeroIdentityService<T, TKey>> log;
        /// <summary>
    /// passwordService.
    /// </summary>
protected readonly IPasswordService passwordService;
        /// <summary>
    /// fluentEmail.
    /// </summary>
protected readonly IFluentEmail fluentEmail;
        /// <summary>
    /// zipService.
    /// </summary>
protected readonly IZipApiService zipService;

        /// <summary>
    /// Initializes a new instance of the <see cref="AeroIdentityService"/> class.
    /// </summary>
protected AeroIdentityService(
        SignInManager<T> signinManager,
        UserManager<T> userManager,
        RoleManager<AeroRole> roleManager,
        IPasswordService passwordService,
        IHttpContextAccessor contextAccessor,
        IFluentEmail fluentEmail,
        IZipApiService zipService,
        ILogger<AeroIdentityService<T, TKey>> log)
    {
        this.log = log;
        this.userManager = userManager;
        this.signinManager = signinManager;
        this.roleManager = roleManager;
        this.passwordService = passwordService;
        this.zipService = zipService;
        this.fluentEmail = fluentEmail;
    }

        /// <summary>
    /// LoginAsync method.
    /// </summary>
public async Task<UserViewModel> LoginAsync(UserLoginRequest model)
        => await LoginAsync(model.Username, model.Password);

    // todo - add external auth here as well
    async Task<UserViewModel> IAeroIdentityService<T, TKey>.LoginAsync(UserLoginRequest model)
    {
        throw new NotImplementedException();
    }

        /// <summary>
    /// LoginAsync method.
    /// </summary>
public async Task<UserViewModel> LoginAsync(UserLoginRequest model, string password) =>
        await LoginAsync(model.Username, password);

        /// <summary>
    /// LogoutAsync method.
    /// </summary>
public async Task LogoutAsync(UserViewModel model)
        => await LogoutAsync(model.Username);

        /// <summary>
    /// LogoutAsync method.
    /// </summary>
public async Task LogoutAsync(string username)
    {
        // todo - verify this logout code actually works...
        await signinManager.SignOutAsync();
    }

        /// <summary>
    /// LoginAsync method.
    /// </summary>
public abstract Task<UserViewModel> LoginAsync(string username, string password);

        /// <summary>
    /// AddUserAsync method.
    /// </summary>
public async Task<(T user, IdentityResult identityReuslt)> AddUserAsync(T model, string password = "")
    {
        if (string.IsNullOrEmpty(model.UserName))
            model.UserName = model.Email;

        if (string.IsNullOrEmpty(password))
            password = passwordService.GeneratePassword();

        var res = await userManager.CreateAsync(model, password);

        if (!res.Succeeded)
        {
            log.LogError($"unable to create user {model.ToJson()}");
            log.LogError($"information: {res.Errors.ToJson()}");
            return (null, res);
        }

        log.LogInformation($"successfully created user");

        return (model, res);
    }

        /// <summary>
    /// UpdateUserAsync method.
    /// </summary>
public async Task<(T user, IdentityResult identityReuslt)> UpdateUserAsync(T model)
    {
        var res = await userManager.UpdateAsync(model);

        return (model, res);
    }

        /// <summary>
    /// DeleteUserAsync method.
    /// </summary>
public async Task<(T user, IdentityResult identityReuslt)> DeleteUserAsync(T model)
    {
        var res = await userManager.DeleteAsync(model);

        return (model, res);
    }


        /// <summary>
    /// DeleteUserAsync method.
    /// </summary>
public async Task<(T user, IdentityResult identityReuslt)> DeleteUserAsync(long id)
    {
        var user = await userManager.FindByIdAsync(id.ToString());
        var res = await userManager.DeleteAsync(user);

        return (user, res);
    }
        /// <summary>
    /// ChangePassword method.
    /// </summary>
public async Task<bool> ChangePassword(T user, string current, string updated)
    {
        log.LogInformation($"changing password for user: {user.ToJson()}");
        var res = await userManager.ChangePasswordAsync(user, current, updated);

        return res.Succeeded;
    }
        /// <summary>
    /// GenerateResetPasswordToken method.
    /// </summary>
public async Task<(bool success, string token, string errorMessage)> GenerateResetPasswordToken(string email)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user == null)
            return (false, "", "user not found");

        var token = await userManager.GeneratePasswordResetTokenAsync(user);

        return (!string.IsNullOrEmpty(token), token, string.Empty);
    }

        /// <summary>
    /// ResetPassword method.
    /// </summary>
public async Task<(bool success, string token, string[] errors)> ResetPassword(string email, string fromEmail, string url, string subject, string scheme = "https")
    {
        log.LogInformation($"generating password reset link for {email}");

        if (string.IsNullOrEmpty(email) || !email.IsValidEmail())
            return (false, string.Empty, new[] { $"email must be in a valid format {email}" });

        var passGenRes = await GenerateResetPasswordToken(email);
        if (!passGenRes.success)
            return (false, string.Empty, new[] { "email address not found" });

        var token = passGenRes.token;

        var rawUrl = url.Split("?").First();
        url = string.Join(rawUrl, $"?token={token}&email={email}");

        log.LogInformation($"generated reset link: {url}");

        var template = $@"
                    Click here to reset your email: <a href=""@Model.Url"">Reset your password</a>
                    <br/><br/>
                    <p>If you are having trouble clicking the link above - copy and paste the following URL into your browser:</p>
                    <br/>
                    <p>{HttpUtility.HtmlEncode(url)}</p>
            ";

        var res = await fluentEmail
                .To(email)
                .Subject(subject)
                .UsingTemplate(template, new { Url = url })
                .SendAsync();

        if (!res.Successful)
        {
            log.LogError($"sending email failed with error(s): {res.ErrorMessages.ToJson()}");
            return (false, token, res.ErrorMessages.ToArray());
        }

        log.LogInformation($"successfully sent password reset email to {email}");


        return (true, token, []);
    }

        /// <summary>
    /// ResetPasswordConfirmation method.
    /// </summary>
public async Task<(bool success, string[] errors)> ResetPasswordConfirmation(string email, string token, string password)
    {
        if (!email.IsValidEmail() || string.IsNullOrEmpty(token))
            return (false, new[] { $"must have a valid email address and token" });

        var user = await userManager.FindByEmailAsync(email);

        if (user == null)
            return (false, new[] { $"unable to find user with email {email}" });

        var res = await userManager.ResetPasswordAsync(user, token, password);

        if (!res.Succeeded)
            return (false, res.Errors.Select(x => x.Description).ToArray());

        log.LogInformation($"successfully reset password for {email}");

        return (true, []);
    }

        /// <summary>
    /// GetByIdAsync method.
    /// </summary>
public async Task<T> GetByIdAsync(long id) => await userManager.FindByIdAsync(id);


        /// <summary>
    /// GetByUsernameAsync method.
    /// </summary>
public async Task<T> GetByUsernameAsync(string username) => await userManager.FindByNameAsync(username);

        /// <summary>
    /// GetByEmailAsync method.
    /// </summary>
public async Task<T> GetByEmailAsync(string email) => await userManager.FindByEmailAsync((email));

        /// <summary>
    /// GetRoles method.
    /// </summary>
public async Task<IEnumerable<string>> GetRoles(long userId)
    {
        var user = await userManager.FindByIdAsync(userId);

        if (user == null)
            return [];

        var roles = await userManager.GetRolesAsync(user);

        return roles;
    }

        /// <summary>
    /// AddToRole method.
    /// </summary>
public async Task<IdentityResult> AddToRole(T user, string role)
    {
        var result = await userManager.AddToRoleAsync(user, role);
        return result;
    }

        /// <summary>
    /// AddToRole method.
    /// </summary>
public async Task<IdentityResult> AddToRole(string userId, string role)
    {
        var user = await userManager.FindByIdAsync(userId);
        return await AddToRole(user, role);
    }

        /// <summary>
    /// AddToRoles method.
    /// </summary>
public async Task<IdentityResult> AddToRoles(T user, IEnumerable<string> roles)
    {
        var result = await userManager.AddToRolesAsync(user, roles);
        return result;
    }

        /// <summary>
    /// AddToRoles method.
    /// </summary>
public async Task<IdentityResult> AddToRoles(string userId, IEnumerable<string> roles)
    {
        var user = await userManager.FindByIdAsync(userId);
        return await AddToRoles(user, roles);
    }

        /// <summary>
    /// AddClaim method.
    /// </summary>
public async Task<IdentityResult> AddClaim(T user, Claim claim)
    {
        var result = await userManager.AddClaimAsync(user, claim);
        return result;
    }

    // todo - add a add roles and claim by id and byEmail and by Username
        /// <summary>
    /// AddClaim method.
    /// </summary>
public async Task<IdentityResult> AddClaim(string userId, Claim claim)
    {
        var user = await userManager.FindByIdAsync(userId);
        return await AddClaim(user, claim);
    }

        /// <summary>
    /// AddClaimsAsync method.
    /// </summary>
public async Task<IdentityResult> AddClaimsAsync(T user, IEnumerable<Claim> claims)
    {
        var result = await userManager.AddClaimsAsync(user, claims);
        return result;
    }

        /// <summary>
    /// AddClaimsAsync method.
    /// </summary>
public async Task<IdentityResult> AddClaimsAsync(string userId, IEnumerable<Claim> claims)
    {
        var user = await userManager.FindByIdAsync(userId);
        return await AddClaimsAsync(user, claims);
    }

        /// <summary>
    /// GetClaims method.
    /// </summary>
public async Task<IDictionary<string, string>> GetClaims(string userId)
    {
        var user = await userManager.FindByIdAsync(userId);

        if (user == null)
            return new Dictionary<string, string>();

        var roles = await userManager.GetClaimsAsync(user);

        var kvps = roles.Any()
            ? roles.Select(x => new KeyValuePair<string, string>(x.Type, x.Value))
            : Array.Empty<KeyValuePair<string, string>>();

        return new Dictionary<string, string>(kvps);
    }

        /// <summary>
    /// Register method.
    /// </summary>
public virtual async Task<(T model, IdentityResult identityResult)> Register(RegistrationRequestModel model, string createdBy = "User")
    {
        var user = RegistrationModelToUser(model, createdBy);

        return await Register(user, model.Password, createdBy);
    }

        /// <summary>
    /// Register method.
    /// </summary>
public virtual async Task<(T model, IdentityResult identityResult)> Register(T user, string password, string createdBy = "User")
    {
        var res = await AddUserAsync(user, password);

        return res;
    }


        /// <summary>
    /// RegistrationModelToUser method.
    /// </summary>
protected virtual T RegistrationModelToUser(RegistrationRequestModel model, string createdBy = "User") => new()
    {
        Id = Snowflake.NewId(),
        Email = model.Email,
        FirstName = model.Firstname,
        LastName = model.Lastname,
        UserName = model.Username,
        PhoneNumber = model.MobileNumber,
        CreatedBy = createdBy,
    };

        /// <summary>
    /// SaveRefreshTokenAsync method.
    /// </summary>
public async Task<bool> SaveRefreshTokenAsync(long id, string token)
    {
        //throw new NotImplementedException();
        var request = new SaveRefreshTokenRequest(id, token);
        //var success = await saveHandler.ExecuteAsync(request);
        var entity = new RefreshToken
        {
            TokenHash = token,
            UserId = id,
            //DateCreated = DateTime.UtcNow,
            //DateModified =  DateTime.UtcNow
        };

        var success = true;
        return await Task.FromResult(success);
    }

        /// <summary>
    /// DeleteRefreshTokenAsync method.
    /// </summary>
public async Task<bool> DeleteRefreshTokenAsync(string username, string refreshToken)
    {
        await Task.CompletedTask;
        throw new NotImplementedException();
        // var request = new SaveRefreshTokenRequest()
        // {
        //     Username = username,
        //     Token = refreshToken
        // };
        //var success = await delHandler.ExecuteAsync(request);
        //return success;
    }

        /// <summary>
    /// VerifyPassword method.
    /// </summary>
public virtual async Task<bool> VerifyPassword(string password, T user)
    {
        var res = await userManager.CheckPasswordAsync(user, password);
        return res;
    }

        /// <summary>
    /// VerifyPassword method.
    /// </summary>
public virtual async Task<bool> VerifyPassword(string username, string password)
    {
        var user = await userManager.FindByNameAsync(username);
        var validCredentials = await signinManager.UserManager.CheckPasswordAsync(user, password);

        return validCredentials;
    }
}

/// <summary>
/// Represents a record for SaveRefreshTokenRequest.
/// </summary>
public record SaveRefreshTokenRequest(long userId, string token);