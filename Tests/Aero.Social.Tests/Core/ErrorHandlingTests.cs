using TUnit.Core;
using Aero.Core;
using Aero.Core.Railway;
using System.Net;
using Aero.Social.Abstractions;
using Aero.Social.Models;
using Aero.Social.Tests.Infrastructure;
using Microsoft.Extensions.Logging;

namespace Aero.Social.Tests.Core;

/// <summary>
/// Represents a class for ErrorHandlingTests.
/// </summary>
public class ErrorHandlingTests : ProviderTestBase
{
    private readonly Mock<ILogger<SocialProviderBase>> _loggerMock = new();

        /// <summary>
    /// FetchWithRetryAsync_OnSuccess_ShouldReturnResponse method.
    /// </summary>
[Test]
    public async Task FetchWithRetryAsync_OnSuccess_ShouldReturnResponse()
    {
        HttpHandler.WhenPost("*")
            .RespondWith("{\"id\": \"123\"}", HttpStatusCode.OK);

        var provider = new TestErrorHandlingProvider(HttpClient, _loggerMock.Object);
        var request = new HttpRequestMessage(HttpMethod.Post, "https://api.test.com/post");
        
        var response = await provider.TestFetchWithRetryAsync("https://api.test.com/post", request);
        
        response.IsSuccess.ShouldBeTrue();
        ((Result<HttpResponseMessage, AeroError>.Ok)response).Value.IsSuccessStatusCode.ShouldBeTrue();
    }

        /// <summary>
    /// FetchWithRetryAsync_OnTooManyRequests_ShouldRetry method.
    /// </summary>
[Test]
    public async Task FetchWithRetryAsync_OnTooManyRequests_ShouldRetry()
    {
        var callCount = 0;
        HttpHandler.WhenPost("*")
            .RespondWith((req) =>
            {
                callCount++;
                return (callCount < 3) 
                    ? new HttpResponseMessage(HttpStatusCode.TooManyRequests)
                    : new HttpResponseMessage(HttpStatusCode.OK);
            });

        var provider = new TestErrorHandlingProvider(HttpClient, _loggerMock.Object);
        var request = new HttpRequestMessage(HttpMethod.Post, "https://api.test.com/post");
        
        var response = await provider.TestFetchWithRetryAsync("https://api.test.com/post", request);
        
        response.IsSuccess.ShouldBeTrue();
        ((Result<HttpResponseMessage, AeroError>.Ok)response).Value.IsSuccessStatusCode.ShouldBeTrue();
        callCount.ShouldBe(3);
    }

        /// <summary>
    /// FetchWithRetryAsync_OnInternalServerError_ShouldRetry method.
    /// </summary>
[Test]
    public async Task FetchWithRetryAsync_OnInternalServerError_ShouldRetry()
    {
        var callCount = 0;
        HttpHandler.WhenPost("*")
            .RespondWith((req) =>
            {
                callCount++;
                return (callCount < 2) 
                    ? new HttpResponseMessage(HttpStatusCode.InternalServerError)
                    : new HttpResponseMessage(HttpStatusCode.OK);
            });

        var provider = new TestErrorHandlingProvider(HttpClient, _loggerMock.Object);
        var request = new HttpRequestMessage(HttpMethod.Post, "https://api.test.com/post");
        
        var response = await provider.TestFetchWithRetryAsync("https://api.test.com/post", request);
        
        response.IsSuccess.ShouldBeTrue();
        ((Result<HttpResponseMessage, AeroError>.Ok)response).Value.IsSuccessStatusCode.ShouldBeTrue();
    }

        /// <summary>
    /// FetchWithRetryAsync_OnRateLimitExceeded_ShouldRetry method.
    /// </summary>
[Test]
    public async Task FetchWithRetryAsync_OnRateLimitExceeded_ShouldRetry()
    {
        var callCount = 0;
        HttpHandler.WhenPost("*")
            .RespondWith((req) =>
            {
                callCount++;
                return (callCount < 2) 
                    ? new HttpResponseMessage(HttpStatusCode.OK) 
                    { 
                        Content = new StringContent("{\"error\": \"rate_limit_exceeded\"}") 
                    }
                    : new HttpResponseMessage(HttpStatusCode.OK) 
                    { 
                        Content = new StringContent("{\"id\": \"123\"}") 
                    };
            });

        var provider = new TestErrorHandlingProvider(HttpClient, _loggerMock.Object);
        var request = new HttpRequestMessage(HttpMethod.Post, "https://api.test.com/post");
        
        var response = await provider.TestFetchWithRetryAsync("https://api.test.com/post", request);
        
        response.IsSuccess.ShouldBeTrue();
        ((Result<HttpResponseMessage, AeroError>.Ok)response).Value.IsSuccessStatusCode.ShouldBeTrue();
    }

        /// <summary>
    /// FetchWithRetryAsync_OnUnauthorized_ShouldThrowRefreshTokenException method.
    /// </summary>
[Test]
    public async Task FetchWithRetryAsync_OnUnauthorized_ShouldThrowRefreshTokenException()
    {
        HttpHandler.WhenPost("*")
            .RespondWith("{\"error\": \"invalid_token\"}", HttpStatusCode.Unauthorized);

        var provider = new TestErrorHandlingProvider(HttpClient, _loggerMock.Object);
        var request = new HttpRequestMessage(HttpMethod.Post, "https://api.test.com/post");
        
        var response = await provider.TestFetchWithRetryAsync("https://api.test.com/post", request);

        response.IsFailure.ShouldBeTrue();
        ((Result<HttpResponseMessage, AeroError>.Failure)response).Error.ShouldBeOfType<AeroError.HttpRequest>();
    }

        /// <summary>
    /// FetchWithRetryAsync_OnMaxRetriesExceeded_ShouldThrowBadBodyException method.
    /// </summary>
[Test]
    public async Task FetchWithRetryAsync_OnMaxRetriesExceeded_ShouldThrowBadBodyException()
    {
        HttpHandler.WhenPost("*")
            .RespondWith("{\"error\": \"server_error\"}", HttpStatusCode.InternalServerError);

        var provider = new TestErrorHandlingProvider(HttpClient, _loggerMock.Object);
        var request = new HttpRequestMessage(HttpMethod.Post, "https://api.test.com/post");
        
        var response = await provider.TestFetchWithRetryAsync("https://api.test.com/post", request, maxRetries: 1);

        response.IsFailure.ShouldBeTrue();
        ((Result<HttpResponseMessage, AeroError>.Failure)response).Error.ShouldBeOfType<AeroError.HttpRequest>();
    }

        /// <summary>
    /// FetchWithRetryAsync_WithCustomErrorHandler_ShouldReturnCorrectErrorType method.
    /// </summary>
[Test]
    public async Task FetchWithRetryAsync_WithCustomErrorHandler_ShouldReturnCorrectErrorType()
    {
        HttpHandler.WhenPost("*")
            .RespondWith("{\"error\": \"Error validating access token\"}", HttpStatusCode.BadRequest);

        var provider = new TestErrorHandlingProvider(HttpClient, _loggerMock.Object);
        provider.SetErrorHandlingType(SocialProviderBase.ErrorHandlingType.RefreshToken);
        var request = new HttpRequestMessage(HttpMethod.Post, "https://api.test.com/post");
        
        var response = await provider.TestFetchWithRetryAsync("https://api.test.com/post", request);

        response.IsFailure.ShouldBeTrue();
        ((Result<HttpResponseMessage, AeroError>.Failure)response).Error.ShouldBeOfType<AeroError.HttpRequest>();
    }
}

/// <summary>
/// Represents a class for TestErrorHandlingProvider.
/// </summary>
public class TestErrorHandlingProvider : SocialProviderBase
{
    private SocialProviderBase.ErrorHandlingType? _errorHandlingType;
    private string _errorHandlingValue = "";

        /// <summary>
    /// Initializes a new instance of the <see cref="TestErrorHandlingProvider"/> class.
    /// </summary>
public TestErrorHandlingProvider(HttpClient httpClient, ILogger<SocialProviderBase> logger) 
        : base(httpClient, logger)
    {
    }

        /// <summary>
    /// SetErrorHandlingType method.
    /// </summary>
public void SetErrorHandlingType(SocialProviderBase.ErrorHandlingType type, string value = "")
    {
        _errorHandlingType = type;
        _errorHandlingValue = value;
    }

        /// <summary>
    /// HandleErrors method.
    /// </summary>
protected override ErrorHandlingResult? HandleErrors(string responseBody)
    {
        if (_errorHandlingType.HasValue)
        {
            return new ErrorHandlingResult(_errorHandlingType.Value, _errorHandlingValue);
        }
        return base.HandleErrors(responseBody);
    }

        /// <summary>
    /// TestFetchWithRetryAsync method.
    /// </summary>
public async Task<Result<HttpResponseMessage, AeroError>> TestFetchWithRetryAsync(
        string url, 
        HttpRequestMessage request, 
        string identifier = "", 
        int maxRetries = 3)
        {
        return await FetchWithRetryAsync(url, request, identifier, maxRetries);
    }

        /// <summary>
    /// Gets or sets the Identifier.
    /// </summary>
public override string Identifier => "test-error";
        /// <summary>
    /// Gets or sets the Name.
    /// </summary>
public override string Name => "Test Error Provider";
        /// <summary>
    /// Gets or sets the Scopes.
    /// </summary>
public override string[] Scopes => Array.Empty<string>();
        /// <summary>
    /// MaxLength method.
    /// </summary>
public override int MaxLength(object? additionalSettings = null) => 1000;

        /// <summary>
    /// PostAsync method.
    /// </summary>
public override Task<Result<PostResponse[], AeroError>> PostAsync(
        string id, string accessToken, List<PostDetails> posts, 
        Integration integration, CancellationToken cancellationToken = default)
        => Task.FromResult<Result<PostResponse[], AeroError>>(Array.Empty<PostResponse>());

        /// <summary>
    /// GenerateAuthUrlAsync method.
    /// </summary>
public override Task<Result<GenerateAuthUrlResponse, AeroError>> GenerateAuthUrlAsync(
        ClientInformation? clientInformation = null,
        CancellationToken cancellationToken = default)
        => Task.FromResult<Result<GenerateAuthUrlResponse, AeroError>>(new GenerateAuthUrlResponse());

        /// <summary>
    /// AuthenticateAsync method.
    /// </summary>
public override Task<Result<AuthTokenDetails, AeroError>> AuthenticateAsync(
        AuthenticateParams parameters,
        ClientInformation? clientInformation = null,
        CancellationToken cancellationToken = default)
        => Task.FromResult<Result<AuthTokenDetails, AeroError>>(new AuthTokenDetails());

        /// <summary>
    /// RefreshTokenAsync method.
    /// </summary>
public override Task<Result<AuthTokenDetails, AeroError>> RefreshTokenAsync(
        string refreshToken,
        CancellationToken cancellationToken = default)
        => Task.FromResult<Result<AuthTokenDetails, AeroError>>(new AuthTokenDetails());
}
