using Aero.Models;
using Aero.Models.Entities;

namespace Aero.Web.Services;

/// <summary>
/// Defines an interface for IApiService.
/// </summary>
public interface IApiService<T, TKey> //where T : IAuthRequestModel
    where TKey : IEquatable<TKey>, IComparable<TKey>
{
        /// <summary>
    /// GetAccountById method.
    /// </summary>
Task<ApiAccountModel?> GetAccountById(TKey id);
        /// <summary>
    /// GetAccountByApiKey method.
    /// </summary>
Task<ApiAccountModel?> GetAccountByApiKey(string key);
        /// <summary>
    /// GetAccountsByEmail method.
    /// </summary>
Task<List<ApiAccountModel>> GetAccountsByEmail(string email);
        /// <summary>
    /// Authenticate method.
    /// </summary>
Task<ApiAccountModel?> Authenticate(T model);
        /// <summary>
    /// TryGetRefreshToken method.
    /// </summary>
bool TryGetRefreshToken(RefreshTokenRequest request, out RefreshTokenResponse response);
        /// <summary>
    /// Register method.
    /// </summary>
Task<ApiAccountModel> Register(ApiRegistrationRequest request);
        /// <summary>
    /// Register method.
    /// </summary>
Task<ApiAccountModel> Register(ApiAccountModel model);
        /// <summary>
    /// Update method.
    /// </summary>
Task<ApiAccountModel> Update(ApiAccountModel model);
        /// <summary>
    /// Revoke method.
    /// </summary>
Task Revoke(string apiKey);
        /// <summary>
    /// RevokeAll method.
    /// </summary>
Task RevokeAll(string email);
}


/// <summary>
/// Represents a class for ApiServiceBase.
/// </summary>
public abstract class ApiServiceBase<T, TKey>(ILogger<ApiServiceBase<T, TKey>> log)
    : IApiService<T, TKey>
    where TKey : IEquatable<TKey>, IComparable<TKey>
{
        /// <summary>
    /// log.
    /// </summary>
protected readonly ILogger<ApiServiceBase<T, TKey>> log = log;

        /// <summary>
    /// GetAccountById method.
    /// </summary>
public abstract Task<ApiAccountModel?> GetAccountById(TKey id);
        /// <summary>
    /// GetAccountByApiKey method.
    /// </summary>
public abstract Task<ApiAccountModel?> GetAccountByApiKey(string key);
        /// <summary>
    /// GetAccountsByEmail method.
    /// </summary>
public abstract Task<List<ApiAccountModel>> GetAccountsByEmail(string email);
        /// <summary>
    /// Authenticate method.
    /// </summary>
public abstract Task<ApiAccountModel?> Authenticate(T model);
        /// <summary>
    /// TryGetRefreshToken method.
    /// </summary>
public abstract bool TryGetRefreshToken(RefreshTokenRequest request, out RefreshTokenResponse response);
        /// <summary>
    /// Register method.
    /// </summary>
public abstract Task<ApiAccountModel> Register(ApiRegistrationRequest request);
        /// <summary>
    /// Register method.
    /// </summary>
public abstract Task<ApiAccountModel> Register(ApiAccountModel model);
        /// <summary>
    /// Update method.
    /// </summary>
public abstract Task<ApiAccountModel> Update(ApiAccountModel model);
        /// <summary>
    /// Revoke method.
    /// </summary>
public abstract Task Revoke(string apiKey);
        /// <summary>
    /// RevokeAll method.
    /// </summary>
public abstract Task RevokeAll(string email);
}