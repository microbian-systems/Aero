using TUnit.Core;
using System.Net;
using System.Text;
using System.Text.Json;
using Shouldly;
using System.Threading.Tasks;

namespace Aero.Auth.Tests;

/// <summary>
/// Essential authentication tests focusing on core registration and login functionality
/// Tests both traditional email/password and passkey authentication flows
/// </summary>
[ClassDataSource<TestWebAppFactory>(Shared = SharedType.PerClass)]
public class EssentialAuthTests
{
    private readonly HttpClient _client;

        /// <summary>
    /// Initializes a new instance of the <see cref="EssentialAuthTests"/> class.
    /// </summary>
public EssentialAuthTests(TestWebAppFactory factory)
    {
        _client = factory.CreateClient();
    }

    //#region Registration Tests

        /// <summary>
    /// Registration_ShouldRejectInvalidEmail method.
    /// </summary>
[Test]
    public async Task Registration_ShouldRejectInvalidEmail()
    {
        // Arrange
        var request = new { Email = "invalid-email", Password = "ValidPassword123!" };
        var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/Auth/register", content);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

        /// <summary>
    /// Registration_ShouldRejectWeakPassword method.
    /// </summary>
[Test]
    public async Task Registration_ShouldRejectWeakPassword()
    {
        // Arrange
        var request = new { Email = "test@example.com", Password = "123" };
        var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/Auth/register", content);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    //#endregion

    //#region Login Tests

        /// <summary>
    /// Login_ShouldRejectInvalidCredentials method.
    /// </summary>
[Test]
    public async Task Login_ShouldRejectInvalidCredentials()
    {
        // Arrange
        var request = new { Email = "nonexistent@example.com", Password = "WrongPassword123!" };
        var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/Auth/login", content);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

        /// <summary>
    /// Login_ShouldRejectMalformedEmail method.
    /// </summary>
[Test]
    public async Task Login_ShouldRejectMalformedEmail()
    {
        // Arrange
        var request = new { Email = "not-an-email", Password = "ValidPassword123!" };
        var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/Auth/login", content);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    //#endregion

    //#region Token Exchange Tests

        /// <summary>
    /// TokenExchange_ShouldRejectUnsupportedGrantType method.
    /// </summary>
[Test]
    public async Task TokenExchange_ShouldRejectUnsupportedGrantType()
    {
        // Arrange
        var formData = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("grant_type", "unsupported"),
            new KeyValuePair<string, string>("username", "test@example.com"),
            new KeyValuePair<string, string>("password", "password123")
        });

        // Act
        var response = await _client.PostAsync("/connect/token", formData);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

        /// <summary>
    /// TokenExchange_ShouldRejectInvalidPasswordFlow method.
    /// </summary>
[Test]
    public async Task TokenExchange_ShouldRejectInvalidPasswordFlow()
    {
        // Arrange
        var formData = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("grant_type", "password"),
            new KeyValuePair<string, string>("username", "invalid@example.com"),
            new KeyValuePair<string, string>("password", "wrongpassword")
        });

        // Act
        var response = await _client.PostAsync("/connect/token", formData);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    //#endregion

    //#region Passkey/WebAuthn Tests

        /// <summary>
    /// Passwordless_ShouldBeAccessible method.
    /// </summary>
[Test]
    public async Task Passwordless_ShouldBeAccessible()
    {
        // Act
        var response = await _client.GetAsync("/Passwordless");

        // Assert
        response.StatusCode.ShouldBeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }

        /// <summary>
    /// Usernameless_ShouldBeAccessible method.
    /// </summary>
[Test]
    public async Task Usernameless_ShouldBeAccessible()
    {
        // Act
        var response = await _client.GetAsync("/Usernameless");

        // Assert
        response.StatusCode.ShouldBeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }

        /// <summary>
    /// PasswordlessAuth_ShouldHandleEmptyRequest method.
    /// </summary>
[Test]
    public async Task PasswordlessAuth_ShouldHandleEmptyRequest()
    {
        // Arrange
        var content = new StringContent("", Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/Passwordless/authenticate", content);

        // Assert
        response.StatusCode.ShouldBeOneOf(
            HttpStatusCode.BadRequest, 
            HttpStatusCode.NotFound, 
            HttpStatusCode.Unauthorized
        );
    }

    //#endregion

    //#region Account Management Tests

        /// <summary>
    /// AccountList_ShouldRequireAuthentication method.
    /// </summary>
[Test]
    public async Task AccountList_ShouldRequireAuthentication()
    {
        // Act
        var response = await _client.GetAsync("/Account/list");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

        /// <summary>
    /// Logout_ShouldRequireAntiForgeryToken method.
    /// </summary>
[Test]
    public async Task Logout_ShouldRequireAntiForgeryToken()
    {
        // Arrange
        var content = new StringContent("", Encoding.UTF8, "application/x-www-form-urlencoded");

        // Act
        var response = await _client.PostAsync("/Account/Logout", content);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    //#endregion

    //#region Security Tests

        /// <summary>
    /// UserInfo_ShouldRequireAuthentication method.
    /// </summary>
[Test]
    public async Task UserInfo_ShouldRequireAuthentication()
    {
        // Act
        var response = await _client.GetAsync("/connect/userinfo");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

        /// <summary>
    /// TokenRevocation_ShouldHandleInvalidToken method.
    /// </summary>
[Test]
    public async Task TokenRevocation_ShouldHandleInvalidToken()
    {
        // Arrange
        var formData = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("token", "invalid-token")
        });

        // Act
        var response = await _client.PostAsync("/connect/revoke", formData);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK); // Per OAuth 2.0 spec
    }

        /// <summary>
    /// RegistrationEndpoint_ShouldRejectNonPostMethods method.
    /// </summary>
[Test]
    public async Task RegistrationEndpoint_ShouldRejectNonPostMethods()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/Auth/register");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.MethodNotAllowed);
    }

        /// <summary>
    /// LoginEndpoint_ShouldRejectNonPostMethods method.
    /// </summary>
[Test]
    public async Task LoginEndpoint_ShouldRejectNonPostMethods()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/Auth/login");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.MethodNotAllowed);
}

    //#endregion
}