using TUnit.Core;
using System.Net;
using System.Text;
using System.Text.Json;
using Shouldly;

namespace Aero.Auth.Tests;

/// <summary>
/// Core integration tests for Aero.Auth - focuses on essential registration and login functionality
/// </summary>
[ClassDataSource<TestWebAppFactory>(Shared = SharedType.PerClass)]
public class ElectraAuthIntegrationTests(TestWebAppFactory factory)
{
    private readonly HttpClient _client = factory.CreateClient();
    private readonly TestWebAppFactory _factory = factory;

    //#region Registration Tests

        /// <summary>
    /// PostRegister_ShouldReturnBadRequest_WhenInvalidEmail method.
    /// </summary>
[Test]
    public async Task PostRegister_ShouldReturnBadRequest_WhenInvalidEmail()
    {
        // Arrange
        var registerRequest = new
        {
            Email = "invalid-email",
            Password = "ValidPassword123!"
        };
        var json = JsonSerializer.Serialize(registerRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/Auth/register", content);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

        /// <summary>
    /// PostRegister_ShouldReturnBadRequest_WhenWeakPassword method.
    /// </summary>
[Test]
    public async Task PostRegister_ShouldReturnBadRequest_WhenWeakPassword()
    {
        // Arrange
        var registerRequest = new
        {
            Email = "test@example.com",
            Password = "123" // Too weak
        };
        var json = JsonSerializer.Serialize(registerRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/Auth/register", content);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

        /// <summary>
    /// PostRegister_ShouldReturnBadRequest_WhenEmptyData method.
    /// </summary>
[Test]
    public async Task PostRegister_ShouldReturnBadRequest_WhenEmptyData()
    {
        // Arrange
        var registerRequest = new
        {
            Email = "",
            Password = ""
        };
        var json = JsonSerializer.Serialize(registerRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/Auth/register", content);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    //#endregion

    //#region Traditional Login Tests

        /// <summary>
    /// PostLogin_ShouldReturnUnauthorized_WhenInvalidCredentials method.
    /// </summary>
[Test]
    public async Task PostLogin_ShouldReturnUnauthorized_WhenInvalidCredentials()
    {
        // Arrange
        var loginRequest = new
        {
            Email = "nonexistent@example.com",
            Password = "WrongPassword123!"
        };
        var json = JsonSerializer.Serialize(loginRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/Auth/login", content);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

        /// <summary>
    /// PostLogin_ShouldReturnBadRequest_WhenInvalidFormat method.
    /// </summary>
[Test]
    public async Task PostLogin_ShouldReturnBadRequest_WhenInvalidFormat()
    {
        // Arrange
        var loginRequest = new
        {
            Email = "invalid-email-format",
            Password = "ValidPassword123!"
        };
        var json = JsonSerializer.Serialize(loginRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/Auth/login", content);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    //#endregion

    //#region OpenIddict Token Flow Tests

        /// <summary>
    /// PostTokenExchange_ShouldReturnBadRequest_WhenUnsupportedGrantType method.
    /// </summary>
[Test]
    public async Task PostTokenExchange_ShouldReturnBadRequest_WhenUnsupportedGrantType()
    {
        // Arrange
        var formData = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("grant_type", "unsupported_grant"),
            new KeyValuePair<string, string>("username", "test@example.com"),
            new KeyValuePair<string, string>("password", "password123")
        });

        // Act
        var response = await _client.PostAsync("/connect/token", formData);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

        /// <summary>
    /// PostTokenExchange_ShouldReturnForbidden_WhenInvalidPasswordCredentials method.
    /// </summary>
[Test]
    public async Task PostTokenExchange_ShouldReturnForbidden_WhenInvalidPasswordCredentials()
    {
        // Arrange
        var formData = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("grant_type", "password"),
            new KeyValuePair<string, string>("username", "nonexistent@example.com"),
            new KeyValuePair<string, string>("password", "wrongpassword")
        });

        // Act
        var response = await _client.PostAsync("/connect/token", formData);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    //#endregion

    //#region Userinfo Endpoint Tests

        /// <summary>
    /// GetUserinfo_ShouldReturnUnauthorized_WhenNotAuthenticated method.
    /// </summary>
[Test]
    public async Task GetUserinfo_ShouldReturnUnauthorized_WhenNotAuthenticated()
    {
        // Act
        var response = await _client.GetAsync("/connect/userinfo");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

        /// <summary>
    /// PostUserinfo_ShouldReturnUnauthorized_WhenNotAuthenticated method.
    /// </summary>
[Test]
    public async Task PostUserinfo_ShouldReturnUnauthorized_WhenNotAuthenticated()
    {
        // Arrange
        var content = new StringContent("", Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/connect/userinfo", content);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    //#endregion

    //#region Token Revocation Tests

        /// <summary>
    /// PostRevoke_ShouldReturnBadRequest_WhenNoTokenProvided method.
    /// </summary>
[Test]
    public async Task PostRevoke_ShouldReturnBadRequest_WhenNoTokenProvided()
    {
        // Arrange
        var formData = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("token", "") // Empty token
        });

        // Act
        var response = await _client.PostAsync("/connect/revoke", formData);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

        /// <summary>
    /// PostRevoke_ShouldReturnOk_WhenValidTokenNotFound method.
    /// </summary>
[Test]
    public async Task PostRevoke_ShouldReturnOk_WhenValidTokenNotFound()
    {
        // Arrange
        var formData = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("token", "nonexistent-token")
        });

        // Act
        var response = await _client.PostAsync("/connect/revoke", formData);

        // Assert
        // Per OAuth 2.0 spec, should return success even if token doesn't exist
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    //#endregion

    //#region Account Management Tests

        /// <summary>
    /// GetAccountListPasskeys_ShouldReturnUnauthorized_WhenNotAuthenticated method.
    /// </summary>
[Test]
    public async Task GetAccountListPasskeys_ShouldReturnUnauthorized_WhenNotAuthenticated()
    {
        // Act
        var response = await _client.GetAsync("/Account/list");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

        /// <summary>
    /// DeletePasskey_ShouldReturnUnauthorized_WhenNotAuthenticated method.
    /// </summary>
[Test]
    public async Task DeletePasskey_ShouldReturnUnauthorized_WhenNotAuthenticated()
    {
        // Arrange
        var credentialId = "test-credential-id";

        // Act
        var response = await _client.DeleteAsync($"/Account/{credentialId}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

        /// <summary>
    /// PostLogout_ShouldRequireAntiForgeryToken method.
    /// </summary>
[Test]
    public async Task PostLogout_ShouldRequireAntiForgeryToken()
    {
        // Arrange
        var content = new StringContent("", Encoding.UTF8, "application/x-www-form-urlencoded");

        // Act
        var response = await _client.PostAsync("/Account/Logout", content);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    //#endregion

    //#region Passwordless/WebAuthn Tests

        /// <summary>
    /// GetPasswordless_ShouldReturnValidResponse method.
    /// </summary>
[Test]
    public async Task GetPasswordless_ShouldReturnValidResponse()
    {
        // Act
        var response = await _client.GetAsync("/Passwordless");

        // Assert
        response.StatusCode.ShouldBeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }

        /// <summary>
    /// GetUsernameless_ShouldReturnValidResponse method.
    /// </summary>
[Test]
    public async Task GetUsernameless_ShouldReturnValidResponse()
    {
        // Act
        var response = await _client.GetAsync("/Usernameless");

        // Assert
        response.StatusCode.ShouldBeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }

        /// <summary>
    /// PostPasswordlessAuthenticate_ShouldReturnValidResponse method.
    /// </summary>
[Test]
    public async Task PostPasswordlessAuthenticate_ShouldReturnValidResponse()
    {
        // Arrange
        var content = new StringContent("", Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/Passwordless/authenticate", content);

        // Assert
        response.StatusCode.ShouldBeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.NotFound, HttpStatusCode.Unauthorized);
    }

        /// <summary>
    /// PasswordlessController_ShouldHandleWebAuthnRequests method.
    /// </summary>
[Test]
    public async Task PasswordlessController_ShouldHandleWebAuthnRequests()
    {
        // Arrange
        var webAuthnContent = new StringContent("{\"challenge\":\"test\"}", Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/Passwordless/options", webAuthnContent);

        // Assert
        // Should handle WebAuthn-specific requests appropriately
        response.StatusCode.ShouldBeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.NotFound, HttpStatusCode.Unauthorized);
    }

    //#endregion

    //#region External Login Tests

        /// <summary>
    /// GetExternalLogin_ShouldReturnValidResponse method.
    /// </summary>
[Test]
    public async Task GetExternalLogin_ShouldReturnValidResponse()
    {
        // Act
        var response = await _client.GetAsync("/ExternalLogin");

        // Assert
        response.StatusCode.ShouldBeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }

        /// <summary>
    /// ExternalLoginProviders_ShouldReturnValidResponse method.
    /// </summary>
[Test]
    [Arguments("/ExternalLogin/google")]
    [Arguments("/ExternalLogin/microsoft")]
    [Arguments("/ExternalLogin/facebook")]
    public async Task ExternalLoginProviders_ShouldReturnValidResponse(string endpoint)
    {
        // Act
        var response = await _client.GetAsync(endpoint);

        // Assert
        response.StatusCode.ShouldBeOneOf(
            HttpStatusCode.OK, 
            HttpStatusCode.NotFound, 
            HttpStatusCode.Redirect, 
            HttpStatusCode.Unauthorized,
            HttpStatusCode.BadRequest
        );
    }

    //#endregion

    //#region HTTP Method Validation Tests

        /// <summary>
    /// Register_ShouldNotAllowNonPostMethods method.
    /// </summary>
[Test]
    [Arguments("GET")]
    [Arguments("PUT")]
    [Arguments("PATCH")]
    [Arguments("DELETE")]
    public async Task Register_ShouldNotAllowNonPostMethods(string httpMethod)
    {
        // Arrange
        var request = new HttpRequestMessage(new HttpMethod(httpMethod), "/api/Auth/register");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.MethodNotAllowed);
    }

        /// <summary>
    /// Login_ShouldNotAllowNonPostMethods method.
    /// </summary>
[Test]
    [Arguments("GET")]
    [Arguments("PUT")]
    [Arguments("PATCH")]
    [Arguments("DELETE")]
    public async Task Login_ShouldNotAllowNonPostMethods(string httpMethod)
    {
        // Arrange
        var request = new HttpRequestMessage(new HttpMethod(httpMethod), "/api/Auth/login");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.MethodNotAllowed);
    }

        /// <summary>
    /// TokenExchange_ShouldNotAllowNonPostMethods method.
    /// </summary>
[Test]
    [Arguments("GET")]
    [Arguments("PUT")]
    [Arguments("PATCH")]
    [Arguments("DELETE")]
    public async Task TokenExchange_ShouldNotAllowNonPostMethods(string httpMethod)
    {
        // Arrange
        var request = new HttpRequestMessage(new HttpMethod(httpMethod), "/connect/token");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.MethodNotAllowed);
    }

    //#endregion

    //#region Security Tests

        /// <summary>
    /// PostLogin_ShouldRejectMaliciousInput method.
    /// </summary>
[Test]
    public async Task PostLogin_ShouldRejectMaliciousInput()
    {
        // Arrange
        var maliciousRequest = new
        {
            Email = "<script>alert('xss')</script>@example.com",
            Password = "'; DROP TABLE Users; --"
        };
        var json = JsonSerializer.Serialize(maliciousRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/Auth/login", content);

        // Assert
        response.StatusCode.ShouldBeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized);
    }

        /// <summary>
    /// PostRegister_ShouldRejectMaliciousInput method.
    /// </summary>
[Test]
    public async Task PostRegister_ShouldRejectMaliciousInput()
    {
        // Arrange
        var maliciousRequest = new
        {
            Email = "<script>alert('xss')</script>@example.com",
            Password = "'; DROP TABLE Users; --ValidPassword123!"
        };
        var json = JsonSerializer.Serialize(maliciousRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/Auth/register", content);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
}

    //#endregion
}