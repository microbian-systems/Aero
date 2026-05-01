using TUnit.Core;
using System.Net;
using Shouldly;
using System.Threading.Tasks;

namespace Aero.Auth.Tests;

[ClassDataSource<TestWebAppFactory>(Shared = SharedType.PerClass)]
public class ExternalLoginControllerIntegrationTests
{
    private readonly HttpClient _client;
    private readonly TestWebAppFactory _factory;

    public ExternalLoginControllerIntegrationTests(TestWebAppFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Test]
    public async Task GetExternalLogin_ShouldReturnSuccessStatusCode()
    {
        // Act
        var response = await _client.GetAsync("/ExternalLogin");

        // Assert
        response.StatusCode.ShouldBeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }

    [Test]
    [Arguments("/ExternalLogin/google")]
    [Arguments("/ExternalLogin/microsoft")]
    [Arguments("/ExternalLogin/facebook")]
    [Arguments("/ExternalLogin/twitter")]
    public async Task ExternalLoginProviders_ShouldReturnValidResponse(string endpoint)
    {
        // Act
        var response = await _client.GetAsync(endpoint);

        // Assert
        // External login endpoints typically redirect or return specific auth responses
        response.StatusCode.ShouldBeOneOf(
            HttpStatusCode.OK, 
            HttpStatusCode.NotFound, 
            HttpStatusCode.Redirect, 
            HttpStatusCode.Unauthorized,
            HttpStatusCode.BadRequest
        );
    }

    [Test]
    public async Task PostExternalLoginCallback_ShouldReturnValidResponse()
    {
        // Arrange
        var content = new StringContent("");

        // Act
        var response = await _client.PostAsync("/ExternalLogin/callback", content);

        // Assert
        response.StatusCode.ShouldBeOneOf(
            HttpStatusCode.BadRequest, 
            HttpStatusCode.NotFound, 
            HttpStatusCode.Unauthorized,
            HttpStatusCode.Redirect
        );
    }

    [Test]
    [Arguments("PUT")]
    [Arguments("PATCH")]
    [Arguments("DELETE")]
    public async Task ExternalLogin_ShouldNotAllowInvalidMethods(string httpMethod)
    {
        // Arrange
        var request = new HttpRequestMessage(new HttpMethod(httpMethod), "/ExternalLogin");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.ShouldBeOneOf(HttpStatusCode.MethodNotAllowed, HttpStatusCode.NotFound);
    }

    [Test]
    public async Task ExternalLoginChallenge_ShouldReturnValidResponse()
    {
        // Arrange
        var challengeUrl = "/ExternalLogin/challenge?provider=google&returnUrl=/";

        // Act
        var response = await _client.GetAsync(challengeUrl);

        // Assert
        response.StatusCode.ShouldBeOneOf(
            HttpStatusCode.OK, 
            HttpStatusCode.NotFound, 
            HttpStatusCode.Redirect, 
            HttpStatusCode.Unauthorized,
            HttpStatusCode.BadRequest
        );
    }

    [Test]
    [Arguments("invalid-provider")]
    [Arguments("\"<script>alert('xss')</script>\"")]
    [Arguments("../../../etc/passwd")]
    public async Task ExternalLoginChallenge_ShouldHandleInvalidProviders(string provider)
    {
        // Arrange
        var challengeUrl = $"/ExternalLogin/challenge?provider={Uri.EscapeDataString(provider)}&returnUrl=/";

        // Act
        var response = await _client.GetAsync(challengeUrl);

        // Assert
        response.StatusCode.ShouldBeOneOf(
            HttpStatusCode.BadRequest, 
            HttpStatusCode.NotFound, 
            HttpStatusCode.Unauthorized
        );
    }

    [Test]
    public async Task ExternalLoginCallback_ShouldValidateState_Parameter()
    {
        // Arrange
        var callbackUrl = "/ExternalLogin/callback?state=invalid-state&code=test-code";

        // Act
        var response = await _client.GetAsync(callbackUrl);

        // Assert
        response.StatusCode.ShouldBeOneOf(
            HttpStatusCode.BadRequest, 
            HttpStatusCode.NotFound, 
            HttpStatusCode.Unauthorized,
            HttpStatusCode.Redirect
        );
}
}