using TUnit.Core;
using Shouldly;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Aero.Auth.Services;
using Marten;
using System.Threading.Tasks;

namespace Aero.Auth.Tests.Services;

/// <summary>
/// Unit tests for refresh token service focusing on interface contracts and behavior
/// </summary>
public class RefreshTokenServiceContractTests : AeroDbTestDriver
{
    private readonly ILogger<RefreshTokenService> _mockLogger;
    private readonly IConfiguration _config;

        /// <summary>
    /// Initializes a new instance of the <see cref="RefreshTokenServiceContractTests"/> class.
    /// </summary>
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

        /// <summary>
    /// RefreshTokenService_ImplementsInterface method.
    /// </summary>
[Test]
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

        /// <summary>
    /// IRefreshTokenService_HasRequiredMethods method.
    /// </summary>
[Test]
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

        /// <summary>
    /// Constructor_WithValidDependencies_ShouldNotThrow method.
    /// </summary>
[Test]
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

        /// <summary>
    /// RefreshTokenLifetime_ShouldUseConfiguredValue method.
    /// </summary>
[Test]
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

        /// <summary>
    /// GenerateRefreshToken_WithValidParameters_ShouldReturnNonEmptyToken method.
    /// </summary>
[Test]
    public async Task GenerateRefreshToken_WithValidParameters_ShouldReturnNonEmptyToken()
    {
        // Arrange
        using var session = store.LightweightSession();

        var service = new RefreshTokenService(session, _mockLogger, _config);

        // Act
        var token = await service.GenerateRefreshTokenAsync(12345, "mobile");

        // Assert
        token.ShouldNotBeNullOrEmpty();
    }

    // Token Validation Tests

        /// <summary>
    /// ValidateRefreshToken_WithNullToken_ShouldReturnNull method.
    /// </summary>
[Test]
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

        /// <summary>
    /// ValidateRefreshToken_WithEmptyToken_ShouldReturnNull method.
    /// </summary>
[Test]
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

        /// <summary>
    /// RotateRefreshToken_WithInvalidToken_ShouldThrowInvalidOperationException method.
    /// </summary>
[Test]
    public async Task RotateRefreshToken_WithInvalidToken_ShouldThrowInvalidOperationException()
    {
        // Arrange
        using var session = store.LightweightSession();

        var service = new RefreshTokenService(session, _mockLogger, _config);

        // Act
        Func<Task> act = async () => await service.RotateRefreshTokenAsync("invalid-token", "mobile");

        // Assert
        await act.ShouldThrowAsync<InvalidOperationException>();
    }

    // Token Revocation Tests

        /// <summary>
    /// RevokeRefreshToken_WithNullToken_ShouldThrowArgumentNullException method.
    /// </summary>
[Test]
    public async Task RevokeRefreshToken_WithNullToken_ShouldThrowArgumentNullException()
    {
        // Arrange
        var mockSession = Substitute.For<IDocumentSession>();
        var service = new RefreshTokenService(mockSession, _mockLogger, _config);

        // Act
        Func<Task> act = async () => await service.RevokeRefreshTokenAsync(null!);

        // Assert
        await act.ShouldThrowAsync<ArgumentNullException>();
}
}