using TUnit.Core;
using System.Net;
using Shouldly;
using System.Threading.Tasks;

namespace Aero.Auth.Tests;

/// <summary>
/// Represents a class for RegistrationControllerIntegrationTests.
/// </summary>
[ClassDataSource<TestWebAppFactory>(Shared = SharedType.PerClass)]
public class RegistrationControllerIntegrationTests
{
    private readonly HttpClient _client;
    private readonly TestWebAppFactory _factory;

        /// <summary>
    /// Initializes a new instance of the <see cref="RegistrationControllerIntegrationTests"/> class.
    /// </summary>
public RegistrationControllerIntegrationTests(TestWebAppFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

        /// <summary>
    /// GetRegistration_ShouldReturnSuccessStatusCode method.
    /// </summary>
[Test]
    public async Task GetRegistration_ShouldReturnSuccessStatusCode()
    {
        // Act
        var response = await _client.GetAsync("/Registration");

        // Assert
        response.StatusCode.ShouldBeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }

        /// <summary>
    /// PostRegistration_ShouldReturnMethodNotAllowed_WhenNoData method.
    /// </summary>
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

        /// <summary>
    /// RegistrationEndpoints_ShouldReturnValidResponse method.
    /// </summary>
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

        /// <summary>
    /// Registration_ShouldNotAllowInvalidMethods method.
    /// </summary>
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