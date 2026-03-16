using Shouldly;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Aero.Auth.Services;

namespace Aero.Auth.Tests.Services;

/// <summary>
/// Simplified unit tests for JWT token service
/// Focuses on configuration, error handling, and token lifetime
/// </summary>
public class JwtTokenServiceSimplifiedTests
{
    private readonly IJwtSigningKeyStore _mockKeyStore;
    private readonly ILogger<JwtTokenService> _mockLogger;
    private readonly IConfiguration _mockConfig;

    public JwtTokenServiceSimplifiedTests()
    {
        _mockKeyStore = Substitute.For<IJwtSigningKeyStore>();
        _mockLogger = Substitute.For<ILogger<JwtTokenService>>();
        _mockConfig = Substitute.For<IConfiguration>();
    }

    //#region Configuration Tests

    [Fact]
    public void Constructor_WithValidConfig_ShouldSetAccessTokenLifetime()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Auth:AccessTokenLifetimeSeconds", "600" }
            })
            .Build();

        // Act
        var service = new JwtTokenService(_mockKeyStore, _mockLogger, config);

        // Assert
        service.AccessTokenLifetime.ShouldBe(600);
    }

    [Fact]
    public void Constructor_WithoutAccessTokenConfig_ShouldUseDefault()
    {
        // Arrange
        var config = new ConfigurationBuilder().Build();

        // Act
        var service = new JwtTokenService(_mockKeyStore, _mockLogger, config);

        // Assert
        service.AccessTokenLifetime.ShouldBe(300);
    }

    [Fact]
    public void Constructor_WithMultipleInstances_ShouldEachHaveOwnConfig()
    {
        // Arrange
        var config1 = Substitute.For<IConfiguration>();
        var config2 = Substitute.For<IConfiguration>();
        
        config1["Auth:AccessTokenLifetimeSeconds"].Returns("300");
        config2["Auth:AccessTokenLifetimeSeconds"].Returns("600");

        // Act
        var service1 = new JwtTokenService(_mockKeyStore, _mockLogger, config1);
        var service2 = new JwtTokenService(_mockKeyStore, _mockLogger, config2);

        // Assert
        service1.AccessTokenLifetime.ShouldBe(300);
        service2.AccessTokenLifetime.ShouldBe(600);
    }

    //#endregion

    //#region Error Handling Tests

    [Fact]
    public async Task GenerateAccessToken_WithNullKeyStore_ShouldThrowNullReferenceException()
    {
        // Arrange
        var service = new JwtTokenService(null!, _mockLogger, _mockConfig);

        // Act
        Func<Task> act = async () => await service.GenerateAccessTokenAsync("user-123", "test@example.com");

        // Assert
        act.ShouldThrow<NullReferenceException>();
    }

    [Fact]
    public async Task GenerateAccessToken_WithKeyStoreThrowing_ShouldPropagateException()
    {
        // Arrange
        var service = new JwtTokenService(_mockKeyStore, _mockLogger, _mockConfig);
        _mockKeyStore.GetSigningCredentialsAsync(Arg.Any<CancellationToken>())
            .ReturnsForAnyArgs(Task.FromException<Microsoft.IdentityModel.Tokens.SigningCredentials>(
                new InvalidOperationException("No signing key")));

        // Act
        Func<Task> act = async () => await service.GenerateAccessTokenAsync("user-123", "test@example.com");

        // Assert
        act.ShouldThrow<InvalidOperationException>();
    }

    //#endregion

    //#region Dependency Injection Tests

    [Fact]
    public void ServiceImplementsInterface_ShouldBeRegistrable()
    {
        // Arrange & Act
        IJwtTokenService service = new JwtTokenService(_mockKeyStore, _mockLogger, _mockConfig);

        // Assert
        service.ShouldNotBeNull();
        service.ShouldBeAssignableTo<IJwtTokenService>();
    }

    //#endregion

    //#region Configuration Value Tests

    [Theory]
    [InlineData("100")]
    [InlineData("300")]
    [InlineData("600")]
    [InlineData("900")]
    public void AccessTokenLifetime_WithVariousConfigs_ShouldReturnCorrectValue(string configValue)
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Auth:AccessTokenLifetimeSeconds", configValue }
            })
            .Build();
        var service = new JwtTokenService(_mockKeyStore, _mockLogger, config);

        // Act
        var lifetime = service.AccessTokenLifetime;

        // Assert
        lifetime.ShouldBe(int.Parse(configValue));
    }

    //#endregion
}
