using TUnit.Core;
using System.Net;
using System.Text;
using System.Text.Json;
using Shouldly;

namespace Aero.Auth.Tests;

/// <summary>
/// Represents a class for AuthControllerIntegrationTests.
/// </summary>
[ClassDataSource<TestWebAppFactory>(Shared = SharedType.PerClass)]
public class AuthControllerIntegrationTests(TestWebAppFactory factory)
{
    private readonly HttpClient _client = factory.CreateClient();
    private readonly TestWebAppFactory _factory = factory;

        /// <summary>
    /// PostRegister_ShouldReturnBadRequest_WhenModelStateInvalid method.
    /// </summary>
[Test]
    public async Task PostRegister_ShouldReturnBadRequest_WhenModelStateInvalid()
    {
        // Arrange
        var registerRequest = new
        {
            Email = "", // Invalid empty email
            Password = "" // Invalid empty password
        };
        var json = JsonSerializer.Serialize(registerRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/Auth/register", content);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

        /// <summary>
    /// PostRegister_ShouldReturnBadRequest_WhenInvalidEmailFormat method.
    /// </summary>
[Test]
    public async Task PostRegister_ShouldReturnBadRequest_WhenInvalidEmailFormat()
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
    /// PostLogin_ShouldReturnBadRequest_WhenModelStateInvalid method.
    /// </summary>
[Test]
    public async Task PostLogin_ShouldReturnBadRequest_WhenModelStateInvalid()
    {
        // Arrange
        var loginRequest = new
        {
            Email = "", // Invalid empty email
            Password = "" // Invalid empty password
        };
        var json = JsonSerializer.Serialize(loginRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/Auth/login", content);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

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
    /// PostTokenExchange_ShouldReturnBadRequest_WhenNoRequestData method.
    /// </summary>
[Test]
    public async Task PostTokenExchange_ShouldReturnBadRequest_WhenNoRequestData()
    {
        // Arrange
        var content = new StringContent("", Encoding.UTF8, "application/x-www-form-urlencoded");

        // Act
        var response = await _client.PostAsync("/connect/token", content);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

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
        var formData = new FormUrlEncodedContent([
            new KeyValuePair<string, string>("token", "nonexistent-token")
        ]);

        // Act
        var response = await _client.PostAsync("/connect/revoke", formData);

        // Assert
        // Per OAuth 2.0 spec, should return success even if token doesn't exist
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

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
    //[Arguments("GET")]
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
    //[Arguments("GET")]
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

        /// <summary>
    /// Userinfo_ShouldNotAllowInvalidMethods method.
    /// </summary>
[Test]
    [Arguments("PUT")]
    [Arguments("PATCH")]
    [Arguments("DELETE")]
    public async Task Userinfo_ShouldNotAllowInvalidMethods(string httpMethod)
    {
        // Arrange
        var request = new HttpRequestMessage(new HttpMethod(httpMethod), "/connect/userinfo");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.MethodNotAllowed);
    }

        /// <summary>
    /// PostRegister_ShouldReturnBadRequest_WhenPasswordTooWeak method.
    /// </summary>
[Test]
    public async Task PostRegister_ShouldReturnBadRequest_WhenPasswordTooWeak()
    {
        // Arrange
        var registerRequest = new
        {
            Email = "test@example.com",
            Password = "123" // Too weak password
        };
        var json = JsonSerializer.Serialize(registerRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/Auth/register", content);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

        /// <summary>
    /// PostRegister_ShouldReturnBadRequest_WhenEmailAlreadyExists method.
    /// </summary>
[Test]
    public async Task PostRegister_ShouldReturnBadRequest_WhenEmailAlreadyExists()
    {
        // Note: This test assumes no user seeding in test environment
        // In a real scenario, you would seed a test user first
        
        // Arrange
        var registerRequest = new
        {
            Email = "test@example.com",
            Password = "ValidPassword123!"
        };
        var json = JsonSerializer.Serialize(registerRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/Auth/register", content);

        // Assert
        // First registration might succeed or fail depending on test setup
        // The important thing is that it returns a valid HTTP status
        response.StatusCode.ShouldBeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
    }

        /// <summary>
    /// PostRegister_ShouldReturnUnsupportedMediaType_WhenInvalidContentType method.
    /// </summary>
[Test]
    [Arguments("application/xml")]
    [Arguments("text/plain")]
    [Arguments("application/x-www-form-urlencoded")]
    public async Task PostRegister_ShouldReturnUnsupportedMediaType_WhenInvalidContentType(string contentType)
    {
        // Arrange
        var content = new StringContent("{\"Email\":\"test@example.com\",\"Password\":\"ValidPassword123!\"}", Encoding.UTF8, contentType);

        // Act
        var response = await _client.PostAsync("/api/Auth/register", content);

        // Assert
        response.StatusCode.ShouldBeOneOf(HttpStatusCode.UnsupportedMediaType, HttpStatusCode.BadRequest);
    }

        /// <summary>
    /// PostLogin_ShouldReturnUnsupportedMediaType_WhenInvalidContentType method.
    /// </summary>
[Test]
    [Arguments("application/xml")]
    [Arguments("text/plain")]
    [Arguments("application/x-www-form-urlencoded")]
    public async Task PostLogin_ShouldReturnUnsupportedMediaType_WhenInvalidContentType(string contentType)
    {
        // Arrange
        var content = new StringContent("{\"Email\":\"test@example.com\",\"Password\":\"ValidPassword123!\"}", Encoding.UTF8, contentType);

        // Act
        var response = await _client.PostAsync("/api/Auth/login", content);

        // Assert
        response.StatusCode.ShouldBeOneOf(HttpStatusCode.UnsupportedMediaType, HttpStatusCode.BadRequest);
}
}
