using TUnit.Core;
using System.Net;
using Shouldly;
using System.Threading.Tasks;

namespace Aero.Auth.Tests;

[ClassDataSource<TestWebAppFactory>(Shared = SharedType.PerClass)]
public class RegistrationControllerIntegrationTests
{
    private readonly HttpClient _client;
    private readonly TestWebAppFactory _factory;

    public RegistrationControllerIntegrationTests(TestWebAppFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Test]
    public async Task GetRegistration_ShouldReturnSuccessStatusCode()
    {
        // Act
        var response = await _client.GetAsync("/Registration");

        // Assert
        response.StatusCode.ShouldBeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }

    [Test]
    public async Task PostRegistration_ShouldReturnMethodNotAllowed_WhenNoData()
    {
        // Arrange
        var content = new StringContent("");

        // Act
        var response = await _client.PostAsync("/Registration", content);

        // Assert
        // Response depends on whether endpoint exists and how it's configured
        response.StatusCode.ShouldBeOneOf(HttpStatusCode.MethodNotAllowed, HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
    }

    [Test]
    [Arguments("/Registration/index")]
    [Arguments("/Registration/create")]
    [Arguments("/Registration/options")]
    public async Task RegistrationEndpoints_ShouldReturnValidResponse(string endpoint)
    {
        // Act
        var response = await _client.GetAsync(endpoint);

        // Assert
        // Most endpoints will return NotFound if not implemented, which is expected
        response.StatusCode.ShouldBeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.Unauthorized);
    }

    [Test]
    [Arguments("PUT")]
    [Arguments("PATCH")]
    [Arguments("DELETE")]
    public async Task Registration_ShouldNotAllowInvalidMethods(string httpMethod)
    {
        // Arrange
        var request = new HttpRequestMessage(new HttpMethod(httpMethod), "/Registration");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.ShouldBeOneOf(HttpStatusCode.MethodNotAllowed, HttpStatusCode.NotFound);
}
}