using TUnit.Core;
// Basic Account Controller tests - main tests are in ElectraAuthIntegrationTests.cs
using System.Net;
using Shouldly;
using System.Threading.Tasks;

namespace Aero.Auth.Tests;

[ClassDataSource<TestWebAppFactory>(Shared = SharedType.PerClass)]
public class AccountControllerIntegrationTests
{
    private readonly HttpClient _client;

    public AccountControllerIntegrationTests(TestWebAppFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Test]
    public async Task GetAccountListPasskeys_ShouldReturnUnauthorized_WhenNotAuthenticated()
    {
        // Act
        var response = await _client.GetAsync("/Account/list");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task PostLogout_ShouldRequireAntiForgeryToken()
    {
        // Arrange
        var content = new StringContent("", System.Text.Encoding.UTF8, "application/x-www-form-urlencoded");

        // Act
        var response = await _client.PostAsync("/Account/Logout", content);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
}
}