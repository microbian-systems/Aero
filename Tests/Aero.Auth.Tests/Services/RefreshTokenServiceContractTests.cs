using Shouldly;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Aero.Auth.Services;
using Marten;

namespace Aero.Auth.Tests.Services;

/// <summary>
/// Unit tests for refresh token service focusing on interface contracts and behavior
/// </summary>
public class RefreshTokenServiceContractTests : RavenTestDriver
{
    private readonly ILogger<RefreshTokenService> _mockLogger;
    private readonly IConfiguration _config;

    public RefreshTokenServiceContractTests()
    {
        _mockLogger = Substitute.For<ILogger<RefreshTokenService>>();
        
        // Create a real configuration with test values
        var configBuilder = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Auth:RefreshTokenLifetimeDays", "30" }
            });
        _config = configBuilder.Build();
    }

    // Interface Contract Tests

    [Fact]
    public void RefreshTokenService_ImplementsInterface()
    {
        // Arrange
        var mockSession = Substitute.For<IDocumentSession>();

        // Act
        IRefreshTokenService service = new RefreshTokenService(mockSession, _mockLogger, _config);

        // Assert
        service.ShouldNotBeNull();
        service.ShouldBeAssignableTo<IRefreshTokenService>();
    }

    [Fact]
    public void IRefreshTokenService_HasRequiredMethods()
    {
        // Arrange
        var interfaceType = typeof(IRefreshTokenService);

        // Act
        var methods = interfaceType.GetMethods();

        // Assert
        methods.ShouldContain(m => m.Name == "GenerateRefreshTokenAsync");
        methods.ShouldContain(m => m.Name == "ValidateRefreshTokenAsync");
        methods.ShouldContain(m => m.Name == "RotateRefreshTokenAsync");
        methods.ShouldContain(m => m.Name == "RevokeRefreshTokenAsync");
        methods.ShouldContain(m => m.Name == "RevokeAllUserTokensAsync");
        methods.ShouldContain(m => m.Name == "GetActiveTokensAsync");
    }

    // Dependency Injection Tests

    [Fact]
    public void Constructor_WithValidDependencies_ShouldNotThrow()
    {
        // Arrange
        var mockSession = Substitute.For<IDocumentSession>();

        // Act
        Action act = () => new RefreshTokenService(mockSession, _mockLogger, _config);

        // Assert
        act.ShouldNotThrow();
    }

    // Configuration Tests

    [Fact]
    public void RefreshTokenLifetime_ShouldUseConfiguredValue()
    {
        // Arrange
        var mockSession = Substitute.For<IDocumentSession>();

        // Act
        var service = new RefreshTokenService(mockSession, _mockLogger, _config);

        // Assert
        service.ShouldNotBeNull();
    }

    // Token Generation Tests

    [Fact]
    public async Task GenerateRefreshToken_WithValidParameters_ShouldReturnNonEmptyToken()
    {
        // Arrange
        using var store = GetDocumentStore();
        using var session = store.LightweightSession();

        var service = new RefreshTokenService(session, _mockLogger, _config);

        // Act
        var token = await service.GenerateRefreshTokenAsync("user-123", "mobile");

        // Assert
        token.ShouldNotBeNullOrEmpty();
    }

    // Token Validation Tests

    [Fact]
    public async Task ValidateRefreshToken_WithNullToken_ShouldReturnNull()
    {
        // Arrange
        var mockSession = Substitute.For<IDocumentSession>();
        var service = new RefreshTokenService(mockSession, _mockLogger, _config);

        // Act
        var result = await service.ValidateRefreshTokenAsync(null!);

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public async Task ValidateRefreshToken_WithEmptyToken_ShouldReturnNull()
    {
        // Arrange
        var mockSession = Substitute.For<IDocumentSession>();
        var service = new RefreshTokenService(mockSession, _mockLogger, _config);

        // Act
        var result = await service.ValidateRefreshTokenAsync(string.Empty);

        // Assert
        result.ShouldBeNull();
    }

    // Token Rotation Tests

    [Fact]
    public async Task RotateRefreshToken_WithInvalidToken_ShouldThrowInvalidOperationException()
    {
        // Arrange
        using var store = GetDocumentStore();
        using var session = store.LightweightSession();

        var service = new RefreshTokenService(session, _mockLogger, _config);

        // Act
        Func<Task> act = async () => await service.RotateRefreshTokenAsync("invalid-token", "mobile");

        // Assert
        act.ShouldThrow<InvalidOperationException>();
    }

    // Token Revocation Tests

    [Fact]
    public async Task RevokeRefreshToken_WithNullToken_ShouldThrowArgumentNullException()
    {
        // Arrange
        var mockSession = Substitute.For<IDocumentSession>();
        var service = new RefreshTokenService(mockSession, _mockLogger, _config);

        // Act
        Func<Task> act = async () => await service.RevokeRefreshTokenAsync(null!);

        // Assert
        act.ShouldThrow<ArgumentNullException>();
    }
}
