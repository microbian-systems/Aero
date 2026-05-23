using Aero.Core;
using Aero.Core.Http;
using Aero.Core.Railway;
using Aero.Social.Models;
using Aero.Social.Plugs;
using Microsoft.Extensions.Logging;

namespace Aero.Social.Abstractions;

/// <summary>
/// Base class for all social media provider implementations.
/// Provides common functionality for HTTP requests, error handling, retry logic, and plug support.
/// </summary>
public abstract class SocialProviderBase : HttpClientBase, ISocialProvider
{
    /// <inheritdoc/>
    public abstract string Identifier { get; }

    /// <inheritdoc/>
    public abstract string Name { get; }

    /// <inheritdoc/>
    public abstract string[] Scopes { get; }

    /// <inheritdoc/>
    public virtual EditorType Editor => EditorType.Normal;

    /// <inheritdoc/>
    public virtual bool IsBetweenSteps => false;

    /// <inheritdoc/>
    public virtual bool IsWeb3 => false;

    /// <inheritdoc/>
    public virtual int MaxConcurrentJobs => 1;

    /// <inheritdoc/>
    public virtual string? Tooltip => null;

    /// <inheritdoc/>
    public virtual bool OneTimeToken => false;

    /// <inheritdoc/>
    public virtual bool RefreshWait => false;

    /// <inheritdoc/>
    public virtual bool ConvertToJpeg => false;

    /// <summary>
    /// Initializes a new instance of the <see cref="SocialProviderBase"/> class.
    /// </summary>
    /// <param name="httpClient">The HTTP client for making API requests.</param>
    /// <param name="log">The logger for this provider.</param>
    protected SocialProviderBase(
        HttpClient httpClient,
        ILogger<SocialProviderBase> log)
        : base(httpClient, log)
    {
    }

    /// <inheritdoc/>
    public abstract int MaxLength(object? additionalSettings = null);

    /// <inheritdoc/>
    public abstract Task<Result<PostResponse[], AeroError>> PostAsync(
        string id,
        string accessToken,
        List<PostDetails> posts,
        Integration integration,
        CancellationToken cancellationToken = default);

    /// <inheritdoc/>
    public virtual Task<Result<PostResponse[]?, AeroError>> CommentAsync(
        string id,
        string postId,
        string? lastCommentId,
        string accessToken,
        List<PostDetails> posts,
        Integration integration,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult<Result<PostResponse[]?, AeroError>>(Array.Empty<PostResponse>());
    }

    /// <inheritdoc/>
    public abstract Task<Result<GenerateAuthUrlResponse, AeroError>> GenerateAuthUrlAsync(
        ClientInformation? clientInformation = null,
        CancellationToken cancellationToken = default);

    /// <inheritdoc/>
    public abstract Task<Result<AuthTokenDetails, AeroError>> AuthenticateAsync(
        AuthenticateParams parameters,
        ClientInformation? clientInformation = null,
        CancellationToken cancellationToken = default);

    /// <inheritdoc/>
    public abstract Task<Result<AuthTokenDetails, AeroError>> RefreshTokenAsync(
        string refreshToken,
        CancellationToken cancellationToken = default);

    /// <inheritdoc/>
    public virtual Task<Result<AuthTokenDetails?, AeroError>> ReConnectAsync(
        string id,
        string requiredId,
        string accessToken,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult<Result<AuthTokenDetails?, AeroError>>(null!);
    }

    /// <inheritdoc/>
    public virtual Task<Result<AnalyticsData[]?, AeroError>> AnalyticsAsync(
        string id,
        string accessToken,
        int days,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult<Result<AnalyticsData[]?, AeroError>>(null!);
    }

    /// <inheritdoc/>
    public virtual Task<Result<AnalyticsData[]?, AeroError>> PostAnalyticsAsync(
        string integrationId,
        string accessToken,
        string postId,
        int days,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult<Result<AnalyticsData[]?, AeroError>>(null!);
    }

    /// <inheritdoc/>
    public virtual Task<Result<object?, AeroError>> MentionAsync(
        string token,
        MentionQuery query,
        string id,
        Integration integration,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult<Result<object?, AeroError>>(new NoMentionResult());
    }

    /// <inheritdoc/>
    public virtual string? MentionFormat(string idOrHandle, string name) => null;

    /// <inheritdoc/>
    public virtual Task<Result<FetchPageInformationResult?, AeroError>> FetchPageInformationAsync(
        string accessToken,
        object data,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult<Result<FetchPageInformationResult?, AeroError>>(null!);
    }

    /// <summary>
    /// Handles provider-specific errors from API responses.
    /// Override this method to provide custom error handling logic.
    /// </summary>
    /// <param name="responseBody">The response body to analyze.</param>
    /// <returns>An error handling result, or null if no specific handling is needed.</returns>
    protected virtual ErrorHandlingResult? HandleErrors(string responseBody)
    {
        return null;
    }

    /// <summary>
    /// Represents the result of error handling analysis.
    /// </summary>
    /// <param name="Type">The type of error handling required.</param>
    /// <param name="Value">Additional context about the error.</param>
    public record ErrorHandlingResult(ErrorHandlingType Type, string Value);

    /// <summary>
    /// Defines the types of error handling actions.
    /// </summary>
    public enum ErrorHandlingType
    {
        /// <summary>
        /// The access token needs to be refreshed.
        /// </summary>
        RefreshToken,

        /// <summary>
        /// The request body was invalid or malformed.
        /// </summary>
        BadBody,

        /// <summary>
        /// The request should be retried after a delay.
        /// </summary>
        Retry
    }

    /// <summary>
    /// Checks if all required OAuth scopes have been granted.
    /// </summary>
    /// <param name="required">The required scopes.</param>
    /// <param name="granted">The granted scopes.</param>
    /// <returns>A Result indicating success or a Forbidden error.</returns>
    protected Result<NoneType, AeroError> CheckScopes(string[] required, string[] granted)
    {
        if (!required.All(scope => granted.Contains(scope, StringComparer.OrdinalIgnoreCase)))
        {
            return AeroError.ForbiddenError("Insufficient scopes granted.");
        }
        return new NoneType();
    }

    /// <summary>
    /// Checks if all required OAuth scopes have been granted from a delimited string.
    /// </summary>
    /// <param name="required">The required scopes.</param>
    /// <param name="grantedScopes">The granted scopes as a comma or space-delimited string.</param>
    /// <returns>A Result indicating success or a Forbidden error.</returns>
    protected Result<NoneType, AeroError> CheckScopes(string[] required, string grantedScopes)
    {
        var delimiter = grantedScopes.Contains(',') ? ',' : ' ';
        var scopes = grantedScopes.Split(delimiter, StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim())
            .ToArray();
        return CheckScopes(required, scopes);
    }

    /// <summary>
    /// Fetches a URL with automatic retry logic for rate limiting and transient errors.
    /// </summary>
    /// <param name="url">The URL to fetch.</param>
    /// <param name="request">The HTTP request message.</param>
    /// <param name="identifier">The provider identifier for error messages.</param>
    /// <param name="maxRetries">Maximum number of retry attempts.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A Result containing the HTTP response message or an AeroError.</returns>
    protected async Task<Result<HttpResponseMessage, AeroError>> FetchWithRetryAsync(
        string url,
        HttpRequestMessage request,
        string identifier = "",
        int maxRetries = 3,
        CancellationToken cancellationToken = default)
    {
        var result = await this.SendRequestAsync(request);

        if (result is Result<HttpResponseMessage, AeroError>.Ok ok)
        {
            return ok;
        }

        var error = (Result<HttpResponseMessage, AeroError>.Failure)result;
        var response = error.Error as AeroError.HttpRequest;

        // If it's not an HTTP request error or we've run out of retries, return the error
        if (response == null || maxRetries <= 0)
        {
            return error;
        }

        var responseBody = response.msg ?? string.Empty;
        var handleError = HandleErrors(responseBody);

        if (response.code == System.Net.HttpStatusCode.TooManyRequests ||
            response.code == System.Net.HttpStatusCode.InternalServerError ||
            responseBody.Contains("rate_limit_exceeded", StringComparison.OrdinalIgnoreCase) ||
            responseBody.Contains("Rate limit", StringComparison.OrdinalIgnoreCase))
        {
            await Task.Delay(5000, cancellationToken);
            var newRequest = await CloneRequestAsync(request);
            return await FetchWithRetryAsync(url, newRequest, identifier, maxRetries - 1, cancellationToken);
        }

        if (handleError?.Type == ErrorHandlingType.Retry)
        {
            await Task.Delay(5000, cancellationToken);
            var newRequest = await CloneRequestAsync(request);
            return await FetchWithRetryAsync(url, newRequest, identifier, maxRetries - 1, cancellationToken);
        }

        // Return the error instead of throwing specialized exceptions
        return error;
    }

    private static async Task<HttpRequestMessage> CloneRequestAsync(HttpRequestMessage request)
    {
        var cloned = new HttpRequestMessage(request.Method, request.RequestUri);

        foreach (var header in request.Headers)
        {
            cloned.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        if (request.Content is not null)
        {
            var content = await request.Content.ReadAsByteArrayAsync();
            cloned.Content = new ByteArrayContent(content);

            foreach (var header in request.Content.Headers)
            {
                cloned.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
        }

        return cloned;
    }

    /// <summary>
    /// Reads a file from disk or downloads it from a URL.
    /// </summary>
    /// <param name="path">The local file path or URL.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A Result containing the file contents as a byte array or an AeroError.</returns>
    protected async Task<Result<byte[], AeroError>> ReadOrFetchAsync(string path, CancellationToken cancellationToken = default)
    {
        if (path.StartsWith("http", StringComparison.OrdinalIgnoreCase))
        {
            var downloadResult = await DownloadBytesAsync(new Uri(path), cancellationToken);
            return downloadResult switch
            {
                Result<byte[]?, AeroError>.Ok(var bytes) when bytes != null => bytes,
                Result<byte[]?, AeroError>.Ok => AeroError.NotFoundError($"Failed to download media from {path}: Content was null"),
                Result<byte[]?, AeroError>.Failure(var err) => err,
                _ => AeroError.CreateError($"Unexpected result downloading {path}")
            };
        }

        try
        {
            return await File.ReadAllBytesAsync(path, cancellationToken);
        }
        catch (Exception ex)
        {
            return AeroError.CreateError($"Failed to read file {path}: {ex.Message}");
        }
    }

    /// <summary>
    /// Generates a random alphanumeric string of the specified length.
    /// </summary>
    /// <param name="length">The length of the string to generate.</param>
    /// <returns>A random alphanumeric string.</returns>
    protected static string MakeId(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        var random = Random.Shared;
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }

    //#region Plug Support

    /// <summary>
    /// When overridden by a provider, returns the plugs this provider declares.
    /// Each <see cref="PlugInfo"/> should have its <see cref="PlugInfo.Execute"/>
    /// delegate set. This replaces the old reflection-based discovery path.
    /// </summary>
    /// <returns>An enumerable of plug information for each declared plug.</returns>
    protected virtual IEnumerable<PlugInfo> GetDeclaredPlugs()
    {
        yield break;
    }

    /// <summary>
    /// Discovers all plugs defined in this provider.
    /// Delegates to <see cref="GetDeclaredPlugs"/> which each provider
    /// implements directly — no runtime reflection or catalog lookup.
    /// </summary>
    /// <returns>An enumerable of plug information for each discovered plug.</returns>
    public IEnumerable<PlugInfo> DiscoverPlugs()
    {
        return GetDeclaredPlugs();
    }

    /// <summary>
    /// Executes a plug with the given context and executor.
    /// Calls the <see cref="PlugInfo.Execute"/> delegate directly —
    /// no <c>MethodInfo.Invoke()</c>.
    /// </summary>
    /// <param name="plug">The plug to execute.</param>
    /// <param name="executor">The plug executor.</param>
    /// <param name="context">The execution context.</param>
    /// <param name="fieldValues">Optional field values for the plug.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The result of the plug execution.</returns>
    public virtual async Task<PlugExecutionResult> ExecutePlugAsync(
        PlugInfo plug,
        IPlugExecutor executor,
        PlugExecutionContext context,
        Dictionary<string, object>? fieldValues = null,
        CancellationToken cancellationToken = default)
    {
        if (plug.IsPostPlug && plug.PostPlugAttribute != null)
        {
            var validationResult = executor.ValidateFields(plug.PostPlugAttribute, fieldValues);
            if (!validationResult.IsValid)
            {
                var errorMessage = string.Join("; ", validationResult.Errors.SelectMany(e => e.Value));
                return PlugExecutionResult.FailedResult($"Validation failed: {errorMessage}");
            }
        }
        else if (!plug.IsPostPlug && plug.Attribute != null)
        {
            var validationResult = executor.ValidateFields(plug.Attribute, fieldValues);
            if (!validationResult.IsValid)
            {
                var errorMessage = string.Join("; ", validationResult.Errors.SelectMany(e => e.Value));
                return PlugExecutionResult.FailedResult($"Validation failed: {errorMessage}");
            }
        }

        try
        {
            return await executor.ExecuteAsync(plug.Execute, this, context, fieldValues, cancellationToken);
        }
        catch (Exception ex)
        {
            return PlugExecutionResult.FailedResult($"Plug execution failed: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Gets a plug by its identifier from the declared plugs.
    /// </summary>
    /// <param name="identifier">The plug identifier to find.</param>
    /// <returns>The plug information, or null if not found.</returns>
    public PlugInfo? GetPlug(string identifier)
    {
        return GetDeclaredPlugs().FirstOrDefault(p =>
            (p.IsPostPlug && p.PostPlugAttribute?.Identifier == identifier) ||
            (!p.IsPostPlug && p.Attribute?.Identifier == identifier));
    }

    //#endregion
}
