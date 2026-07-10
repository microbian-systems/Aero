using TUnit.Core;
using Shouldly;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Aero.Auth.Services;
using System.Threading.Tasks;

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

        /// <summary>
    /// Initializes a new instance of the <see cref="JwtTokenServiceSimplifiedTests"/> class.
    /// </summary>
public JwtTokenServiceSimplifiedTests()
    {
        _mockKeyStore = Substitute.For<IJwtSigningKeyStore>();
        _mockLogger = Substitute.For<ILogger<JwtTokenService>>();
        _mockConfig = Substitute.For<IConfiguration>();
    }

    //#region Configuration Tests

        /// <summary>
    /// Constructor_WithValidConfig_ShouldSetAccessTokenLifetime method.
    /// </summary>
[Test]
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

        /// <summary>
    /// Constructor_WithoutAccessTokenConfig_ShouldUseDefault method.
    /// </summary>
[Test]
    public void Constructor_WithoutAccessTokenConfig_ShouldUseDefault()
    {
        // Arrange
        var config = new ConfigurationBuilder().Build();

        // Act
        var service = new JwtTokenService(_mockKeyStore, _mockLogger, config);

        // Assert
        service.AccessTokenLifetime.ShouldBe(300);
    }

        /// <summary>
    /// Constructor_WithMultipleInstances_ShouldEachHaveOwnConfig method.
    /// </summary>
[Test]
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

        /// <summary>
    /// GenerateAccessToken_WithNullKeyStore_ShouldThrowNullReferenceException method.
    /// </summary>
[Test]
    public async Task GenerateAccessToken_WithNullKeyStore_ShouldThrowNullReferenceException()
    {
        // Arrange
        var service = new JwtTokenService(null!, _mockLogger, _mockConfig);

        // Act
        Func<Task> act = async () => await service.GenerateAccessTokenAsync(12345, "test@example.com");

        // Assert
        act.ShouldThrow<NullReferenceException>();
    }

        /// <summary>
    /// GenerateAccessToken_WithKeyStoreThrowing_ShouldPropagateException method.
    /// </summary>
[Test]
    public async Task GenerateAccessToken_WithKeyStoreThrowing_ShouldPropagateException()
    {
        // Arrange
        var service = new JwtTokenService(_mockKeyStore, _mockLogger, _mockConfig);
        _mockKeyStore.GetSigningCredentialsAsync(Arg.Any<CancellationToken>())
            .ReturnsForAnyArgs(Task.FromException<Microsoft.IdentityModel.Tokens.SigningCredentials>(
                new InvalidOperationException("No signing key")));

        // Act
        Func<Task> act = async () => await service.GenerateAccessTokenAsync(12345, "test@example.com");

        // Assert
        act.ShouldThrow<InvalidOperationException>();
    }

    //#endregion

    //#region Dependency Injection Tests

        /// <summary>
    /// ServiceImplementsInterface_ShouldBeRegistrable method.
    /// </summary>
[Test]
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

        /// <summary>
    /// AccessTokenLifetime_WithVariousConfigs_ShouldReturnCorrectValue method.
    /// </summary>
[Test]
    [Arguments("100")]
    [Arguments("300")]
    [Arguments("600")]
    [Arguments("900")]
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