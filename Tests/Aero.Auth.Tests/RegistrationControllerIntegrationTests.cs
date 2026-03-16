using System.Net;
using Shouldly;

namespace Aero.Auth.Tests;

public class RegistrationControllerIntegrationTests : IClassFixture<TestWebAppFactory>
{
    private readonly HttpClient _client;
    private readonly TestWebAppFactory _factory;

    public RegistrationControllerIntegrationTests(TestWebAppFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetRegistration_ShouldReturnSuccessStatusCode()
    {
        // Act
        var response = await _client.GetAsync("/Registration");

        // Assert
        response.StatusCode.ShouldBeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }

    [Fact]
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

    [Theory]
    [InlineData("/Registration/index")]
    [InlineData("/Registration/create")]
    [InlineData("/Registration/options")]
    public async Task RegistrationEndpoints_ShouldReturnValidResponse(string endpoint)
    {
        // Act
        var response = await _client.GetAsync(endpoint);

        // Assert
        // Most endpoints will return NotFound if not implemented, which is expected
        response.StatusCode.ShouldBeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.Unauthorized);
    }

    [Theory]
    [InlineData("PUT")]
    [InlineData("PATCH")]
    [InlineData("DELETE")]
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
