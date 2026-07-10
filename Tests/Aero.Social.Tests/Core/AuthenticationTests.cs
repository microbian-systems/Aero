using TUnit.Core;
using Aero.Core;
using Aero.Core.Railway;
using System.Net;
using Aero.Social.Abstractions;
using Aero.Social.Models;
using Aero.Social.Tests.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Aero.Social.Tests.Core;

/// <summary>
/// Represents a class for AuthenticationTests.
/// </summary>
public class AuthenticationTests : ProviderTestBase
{
    private readonly Mock<ILogger<SocialProviderBase>> _loggerMock = new();

        /// <summary>
    /// GenerateAuthUrlAsync_ShouldReturnValidUrl method.
    /// </summary>
[Test]
    public async Task GenerateAuthUrlAsync_ShouldReturnValidUrl()
    {
        var provider = new TestOAuth2Provider(HttpClient, _loggerMock.Object, ConfigurationMock.Object);
        
        var authResult = await provider.GenerateAuthUrlAsync();
        authResult.IsSuccess.ShouldBeTrue();
        var result = ((Result<GenerateAuthUrlResponse, AeroError>.Ok)authResult).Value;
        result.Url.ShouldNotBeNullOrEmpty();
        result.Url.ShouldContain("https://test.com/oauth/authorize");
        result.Url.ShouldContain("client_id=");
        result.Url.ShouldContain("redirect_uri=");
        result.Url.ShouldContain("response_type=code");
        result.Url.ShouldContain("scope=");
        result.State.ShouldNotBeNullOrEmpty();
    }

        /// <summary>
    /// AuthenticateAsync_ShouldExchangeCodeForToken method.
    /// </summary>
[Test]
    public async Task AuthenticateAsync_ShouldExchangeCodeForToken()
    {
        HttpHandler.WhenPost("*token*")
            .RespondWith(MockResponses.OAuth2.TokenResponse("test_access_token", "test_refresh_token"));
        
        HttpHandler.WhenGet("*userinfo*")
            .RespondWith("{\"id\": \"123\", \"name\": \"Test User\"}");

        var provider = new TestOAuth2Provider(HttpClient, _loggerMock.Object, ConfigurationMock.Object);
        var parameters = new AuthenticateParams("auth_code", "code_verifier");
        
        var result = await provider.AuthenticateAsync(parameters);
        
        result.IsSuccess.ShouldBeTrue();
        var value = ((Result<AuthTokenDetails, AeroError>.Ok)result).Value;
        value.AccessToken.ShouldBe("test_access_token");
        value.RefreshToken.ShouldBe("test_refresh_token");
        value.Id.ShouldBe("123");
        value.Name.ShouldBe("Test User");
    }

        /// <summary>
    /// AuthenticateAsync_OnError_ShouldThrowException method.
    /// </summary>
[Test]
    public async Task AuthenticateAsync_OnError_ShouldThrowException()
    {
        HttpHandler.WhenPost("*token*")
            .RespondWith(MockResponses.OAuth2.ErrorResponse("invalid_grant", "Invalid authorization code"), 
                HttpStatusCode.BadRequest);

        var provider = new TestOAuth2Provider(HttpClient, _loggerMock.Object, ConfigurationMock.Object);
        var parameters = new AuthenticateParams("invalid_code", "code_verifier");
        
        var result = await provider.AuthenticateAsync(parameters);

        result.IsFailure.ShouldBeTrue();
    }

        /// <summary>
    /// RefreshTokenAsync_ShouldReturnNewToken method.
    /// </summary>
[Test]
    public async Task RefreshTokenAsync_ShouldReturnNewToken()
    {
        HttpHandler.WhenPost("*token*")
            .RespondWith(MockResponses.OAuth2.TokenResponse("new_access_token", "new_refresh_token"));

        var provider = new TestOAuth2Provider(HttpClient, _loggerMock.Object, ConfigurationMock.Object);
        
        var result = await provider.RefreshTokenAsync("old_refresh_token");
        
        result.IsSuccess.ShouldBeTrue();
        var value = ((Result<AuthTokenDetails, AeroError>.Ok)result).Value;
        value.AccessToken.ShouldBe("new_access_token");
        value.RefreshToken.ShouldBe("new_refresh_token");
    }

        /// <summary>
    /// AuthenticateAsync_WithPKCE_ShouldIncludeCodeVerifier method.
    /// </summary>
[Test]
    public async Task AuthenticateAsync_WithPKCE_ShouldIncludeCodeVerifier()
    {
        var receivedContent = "";
        HttpHandler.WhenPost("*token*")
            .RespondWith((req) =>
            {
                if (req.Content != null)
                {
                    var task = req.Content.ReadAsStringAsync();
                    task.Wait();
                    receivedContent = task.Result;
                }
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(MockResponses.OAuth2.TokenResponse("access_token"))
                };
            });
        
        HttpHandler.WhenGet("*userinfo*")
            .RespondWith("{\"id\": \"123\", \"name\": \"Test User\"}");

        var provider = new TestOAuth2Provider(HttpClient, _loggerMock.Object, ConfigurationMock.Object);
        var parameters = new AuthenticateParams("auth_code", "my_code_verifier");
        
        var result = await provider.AuthenticateAsync(parameters);

        result.IsSuccess.ShouldBeTrue();
        
        receivedContent.ShouldContain("code_verifier=my_code_verifier");
    }

        /// <summary>
    /// GenerateAuthUrlAsync_WithClientInformation_ShouldUseCustomSettings method.
    /// </summary>
[Test]
    public async Task GenerateAuthUrlAsync_WithClientInformation_ShouldUseCustomSettings()
    {
        var provider = new TestOAuth2Provider(HttpClient, _loggerMock.Object, ConfigurationMock.Object);
        var clientInfo = new ClientInformation
        {
            ClientId = "custom_client_id",
            ClientSecret = "custom_secret",
            InstanceUrl = "https://custom.com"
        };
        
        var authResult = await provider.GenerateAuthUrlAsync(clientInfo);
        authResult.IsSuccess.ShouldBeTrue();
        var result = ((Result<GenerateAuthUrlResponse, AeroError>.Ok)authResult).Value;
        result.Url.ShouldContain("client_id=custom_client_id");
    }
}

/// <summary>
/// Represents a class for TestOAuth2Provider.
/// </summary>
public class TestOAuth2Provider : SocialProviderBase
{
    private readonly IConfiguration _configuration;

        /// <summary>
    /// Initializes a new instance of the <see cref="TestOAuth2Provider"/> class.
    /// </summary>
public TestOAuth2Provider(HttpClient httpClient, ILogger<SocialProviderBase> logger, IConfiguration configuration) 
        : base(httpClient, logger)
    {
        _configuration = configuration;
    }

        /// <summary>
    /// Gets or sets the Identifier.
    /// </summary>
public override string Identifier => "test-oauth2";
        /// <summary>
    /// Gets or sets the Name.
    /// </summary>
public override string Name => "Test OAuth2 Provider";
        /// <summary>
    /// Gets or sets the Scopes.
    /// </summary>
public override string[] Scopes => new[] { "read", "write" };

        /// <summary>
    /// MaxLength method.
    /// </summary>
public override int MaxLength(object? additionalSettings = null) => 1000;

        /// <summary>
    /// GetClientId method.
    /// </summary>
protected string GetClientId() => _configuration["TEST_CLIENT_ID"] ?? "default_client_id";
        /// <summary>
    /// GetClientSecret method.
    /// </summary>
protected string GetClientSecret() => _configuration["TEST_CLIENT_SECRET"] ?? "default_secret";
        /// <summary>
    /// GetRedirectUri method.
    /// </summary>
protected string GetRedirectUri() => _configuration["TEST_REDIRECT_URI"] ?? "https://localhost/callback";

        /// <summary>
    /// PostAsync method.
    /// </summary>
public override Task<Result<PostResponse[], AeroError>> PostAsync(
        string id, string accessToken, List<PostDetails> posts, 
        Integration integration, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<Result<PostResponse[], AeroError>>(Array.Empty<PostResponse>());
    }

        /// <summary>
    /// GenerateAuthUrlAsync method.
    /// </summary>
public override Task<Result<GenerateAuthUrlResponse, AeroError>> GenerateAuthUrlAsync(
        ClientInformation? clientInformation = null,
        CancellationToken cancellationToken = default)
    {
        var state = MakeId(6);
        var clientId = clientInformation?.ClientId ?? GetClientId();
        var redirectUri = GetRedirectUri();

        var url = $"https://test.com/oauth/authorize" +
                  $"?client_id={clientId}" +
                  $"&redirect_uri={Uri.EscapeDataString(redirectUri)}" +
                  $"&response_type=code" +
                  $"&scope={string.Join(" ", Scopes)}" +
                  $"&state={state}";

        return Task.FromResult<Result<GenerateAuthUrlResponse, AeroError>>(new GenerateAuthUrlResponse
        {
            Url = url,
            State = state,
            CodeVerifier = MakeId(10)
        });
    }

        /// <summary>
    /// AuthenticateAsync method.
    /// </summary>
public override async Task<Result<AuthTokenDetails, AeroError>> AuthenticateAsync(
        AuthenticateParams parameters,
        ClientInformation? clientInformation = null,
        CancellationToken cancellationToken = default)
    {
        var clientId = clientInformation?.ClientId ?? GetClientId();
        var clientSecret = clientInformation?.ClientSecret ?? GetClientSecret();
        var redirectUri = GetRedirectUri();

        var tokenRequest = new HttpRequestMessage(HttpMethod.Post, "https://test.com/oauth/token")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "grant_type", "authorization_code" },
                { "code", parameters.Code },
                { "redirect_uri", redirectUri },
                { "client_id", clientId },
                { "client_secret", clientSecret },
                { "code_verifier", parameters.CodeVerifier }
            })
        };

            var tokenResponse = await client.SendAsync(tokenRequest, cancellationToken);
        var tokenContent = await tokenResponse.Content.ReadAsStringAsync(cancellationToken);

        if (!tokenResponse.IsSuccessStatusCode)
        {
            return AeroError.CreateError($"Token request failed: {tokenContent}");
        }

        var tokenData = System.Text.Json.JsonSerializer.Deserialize<TokenResponse>(tokenContent);

        var userRequest = new HttpRequestMessage(HttpMethod.Get, "https://test.com/oauth/userinfo");
        userRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenData!.access_token);
        
        var userResponse = await client.SendAsync(userRequest, cancellationToken);
        var userContent = await userResponse.Content.ReadAsStringAsync(cancellationToken);
        var userData = System.Text.Json.JsonSerializer.Deserialize<UserResponse>(userContent);

        return new AuthTokenDetails
        {
            AccessToken = tokenData.access_token,
            RefreshToken = tokenData.refresh_token ?? "",
            ExpiresIn = tokenData.expires_in,
            Id = userData!.id,
            Name = userData.name ?? ""
        };
    }

        /// <summary>
    /// RefreshTokenAsync method.
    /// </summary>
public override async Task<Result<AuthTokenDetails, AeroError>> RefreshTokenAsync(
        string refreshToken,
        CancellationToken cancellationToken = default)
    {
        var tokenRequest = new HttpRequestMessage(HttpMethod.Post, "https://test.com/oauth/token")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "grant_type", "refresh_token" },
                { "refresh_token", refreshToken },
                { "client_id", GetClientId() },
                { "client_secret", GetClientSecret() }
            })
        };

        var tokenResponse = await client.SendAsync(tokenRequest, cancellationToken);
        var tokenContent = await tokenResponse.Content.ReadAsStringAsync(cancellationToken);
        var tokenData = System.Text.Json.JsonSerializer.Deserialize<TokenResponse>(tokenContent);

        return new AuthTokenDetails
        {
            AccessToken = tokenData!.access_token,
            RefreshToken = tokenData.refresh_token ?? "",
            ExpiresIn = tokenData.expires_in
        };
    }

    private class TokenResponse
    {
                /// <summary>
        /// Gets or sets the access_token.
        /// </summary>
public string access_token { get; set; } = "";
                /// <summary>
        /// Gets or sets the refresh_token.
        /// </summary>
public string? refresh_token { get; set; }
                /// <summary>
        /// Gets or sets the expires_in.
        /// </summary>
public int expires_in { get; set; }
    }

    private class UserResponse
    {
                /// <summary>
        /// Gets or sets the id.
        /// </summary>
public string id { get; set; } = "";
                /// <summary>
        /// Gets or sets the name.
        /// </summary>
public string? name { get; set;
}
    }
}
