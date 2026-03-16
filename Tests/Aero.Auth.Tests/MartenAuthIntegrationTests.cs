using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Shouldly;

namespace Aero.Auth.Tests;

public class MartenAuthIntegrationTests(TestWebAppFactory factory) : IClassFixture<TestWebAppFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Registration_And_Login_ShouldWork_WithMarten()
    {
        var email = $"marten_test_{Guid.NewGuid()}@example.com";
        var password = "Password123!";

        // 1. Register
        var registerRequest = new
        {
            Email = email,
            Password = password,
            ConfirmPassword = password
        };
        
        var regResponse = await _client.PostAsJsonAsync("/api/Auth/register", registerRequest);
        regResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        // 2. Login
        var loginRequest = new
        {
            Email = email,
            Password = password
        };
        
        var loginResponse = await _client.PostAsJsonAsync("/api/Auth/login", loginRequest);
        loginResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<JsonElement>();
        loginResult.TryGetProperty("accessToken", out _).ShouldBeTrue();
    }
}
