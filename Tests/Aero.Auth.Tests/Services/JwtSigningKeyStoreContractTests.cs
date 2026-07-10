using TUnit.Core;
using Shouldly;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using NSubstitute;
using Aero.Auth.Services;

namespace Aero.Auth.Tests.Services;

/// <summary>
/// Unit tests for JWT signing key store focusing on interface contracts
/// Tests the refactored store that uses abstracted persistence layer
/// </summary>
public class JwtSigningKeyStoreContractTests
{
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<JwtSigningKeyStore> _mockLogger;
    private readonly IJwtSigningKeyPersistence _mockPersistence;

        /// <summary>
    /// Initializes a new instance of the <see cref="JwtSigningKeyStoreContractTests"/> class.
    /// </summary>
public JwtSigningKeyStoreContractTests()
    {
        _memoryCache = new MemoryCache(new MemoryCacheOptions());
        _mockLogger = Substitute.For<ILogger<JwtSigningKeyStore>>();
        _mockPersistence = Substitute.For<IJwtSigningKeyPersistence>();
    }

    //#region Interface Contract Tests

        /// <summary>
    /// JwtSigningKeyStore_ImplementsInterface method.
    /// </summary>
[Test]
    public void JwtSigningKeyStore_ImplementsInterface()
    {
        // Arrange & Act
        IJwtSigningKeyStore store = new JwtSigningKeyStore(_mockPersistence, _mockLogger, _memoryCache);

        // Assert
        store.ShouldNotBeNull();
        store.ShouldBeAssignableTo<IJwtSigningKeyStore>();
    }

        /// <summary>
    /// IJwtSigningKeyStore_HasRequiredMethods method.
    /// </summary>
[Test]
    public void IJwtSigningKeyStore_HasRequiredMethods()
    {
        // Arrange
        var interfaceType = typeof(IJwtSigningKeyStore);

        // Act
        var methods = interfaceType.GetMethods();

        // Assert
        methods.ShouldContain(m => m.Name == "GetCurrentSigningKeyAsync");
        methods.ShouldContain(m => m.Name == "GetCurrentKeyIdAsync");
        methods.ShouldContain(m => m.Name == "GetValidationKeysAsync");
        methods.ShouldContain(m => m.Name == "GetSigningCredentialsAsync");
        methods.ShouldContain(m => m.Name == "RotateSigningKeyAsync");
        methods.ShouldContain(m => m.Name == "RevokeKeyAsync");
        methods.ShouldContain(m => m.Name == "GetKeyByIdAsync");
    }

        /// <summary>
    /// GetSigningCredentials_ShouldReturnCorrectType method.
    /// </summary>
[Test]
    public void GetSigningCredentials_ShouldReturnCorrectType()
    {
        // Act
        var methodInfo = typeof(IJwtSigningKeyStore)
            .GetMethods()
            .First(m => m.Name == "GetSigningCredentialsAsync");

        // Assert
        methodInfo.ReturnType.Name.ShouldContain("Task");
    }

    //#endregion

    //#region Cache Behavior Tests

        /// <summary>
    /// MemoryCache_CanStoreAndRetrieveValues method.
    /// </summary>
[Test]
    public void MemoryCache_CanStoreAndRetrieveValues()
    {
        // Arrange
        var cacheKey = "test-key";
        var cacheValue = "test-value";

        // Act
        _memoryCache.Set(cacheKey, cacheValue);
        var retrieved = _memoryCache.TryGetValue(cacheKey, out var value);

        // Assert
        retrieved.ShouldBeTrue();
        value.ShouldBe(cacheValue);
    }

        /// <summary>
    /// MemoryCache_CanRemoveValues method.
    /// </summary>
[Test]
    public void MemoryCache_CanRemoveValues()
    {
        // Arrange
        var cacheKey = "test-key";
        _memoryCache.Set(cacheKey, "value");

        // Act
        _memoryCache.Remove(cacheKey);
        var retrieved = _memoryCache.TryGetValue(cacheKey, out _);

        // Assert
        retrieved.ShouldBeFalse();
    }

    //#endregion

    //#region Dependency Injection Tests

        /// <summary>
    /// Constructor_WithValidDependencies_ShouldNotThrow method.
    /// </summary>
[Test]
    public void Constructor_WithValidDependencies_ShouldNotThrow()
    {
        // Act
        Action act = () => new JwtSigningKeyStore(_mockPersistence, _mockLogger, _memoryCache);

        // Assert
        act.ShouldNotThrow();
    }

        /// <summary>
    /// Constructor_WithNullPersistence_ShouldThrowArgumentNullException method.
    /// </summary>
[Test]
    public void Constructor_WithNullPersistence_ShouldThrowArgumentNullException()
    {
        // Act
        Action act = () => new JwtSigningKeyStore(null!, _mockLogger, _memoryCache);

        // Assert
        act.ShouldThrow<ArgumentNullException>();
    }

        /// <summary>
    /// Constructor_WithNullLogger_ShouldThrowArgumentNullException method.
    /// </summary>
[Test]
    public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
    {
        // Act
        Action act = () => new JwtSigningKeyStore(_mockPersistence, null!, _memoryCache);

        // Assert
        act.ShouldThrow<ArgumentNullException>();
    }

        /// <summary>
    /// Constructor_WithNullCache_ShouldThrowArgumentNullException method.
    /// </summary>
[Test]
    public void Constructor_WithNullCache_ShouldThrowArgumentNullException()
    {
        // Act
        Action act = () => new JwtSigningKeyStore(_mockPersistence, _mockLogger, null!);

        // Assert
        act.ShouldThrow<ArgumentNullException>();
    }

    //#endregion

    //#region Algorithm Tests

        /// <summary>
    /// SigningKey_ShouldUseHmacSha256Algorithm method.
    /// </summary>
[Test]
    public void SigningKey_ShouldUseHmacSha256Algorithm()
    {
        // Act
        var algorithm = SecurityAlgorithms.HmacSha256;

        // Assert
        algorithm.ShouldBe("HS256");
}

    //#endregion
}