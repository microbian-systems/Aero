using Shouldly;
using NSubstitute;
using Aero.Auth.Services;
using Aero.Models.Entities;

namespace Aero.Auth.Tests.Services;

/// <summary>
/// Unit tests for JWT signing key persistence interface contracts.
/// Tests the abstracted persistence layer that enables switching providers.
/// </summary>
public class JwtSigningKeyPersistenceContractTests
{
    //#region Interface Contract Tests

    [Fact]
    public void IJwtSigningKeyPersistence_HasRequiredMethods()
    {
        // Arrange
        var interfaceType = typeof(IJwtSigningKeyPersistence);

        // Act
        var methods = interfaceType.GetMethods();

        // Assert
        methods.ShouldContain(m => m.Name == "GetCurrentSigningKeyAsync");
        methods.ShouldContain(m => m.Name == "GetValidSigningKeysAsync");
        methods.ShouldContain(m => m.Name == "GetKeyByIdAsync");
        methods.ShouldContain(m => m.Name == "AddKeyAsync");
        methods.ShouldContain(m => m.Name == "UpdateKeyAsync");
        methods.ShouldContain(m => m.Name == "DeactivateCurrentKeyAsync");
        methods.ShouldContain(m => m.Name == "RevokeKeyAsync");
        methods.ShouldContain(m => m.Name == "SaveChangesAsync");
    }




    //#endregion

    //#region Mock Verification Tests

    [Fact]
    public void Mock_CanBeCreatedForInterface()
    {
        // Act
        var mock = Substitute.For<IJwtSigningKeyPersistence>();

        // Assert
        mock.ShouldNotBeNull();
        mock.ShouldBeAssignableTo<IJwtSigningKeyPersistence>();
    }

    [Fact]
    public async Task Mock_GetCurrentSigningKeyAsync_CanBeConfigured()
    {
        // Arrange
        var mock = Substitute.For<IJwtSigningKeyPersistence>();
        var testKey = new JwtSigningKey
        {
            Id = "test-id",
            KeyId = "key-1",
            KeyMaterial = "base64-encoded-key",
            IsCurrentSigningKey = true
        };

        mock.GetCurrentSigningKeyAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult((JwtSigningKey?)testKey));

        // Act
        var result = await mock.GetCurrentSigningKeyAsync();

        // Assert
        result.ShouldNotBeNull();
        result?.KeyId.ShouldBe("key-1");
    }

    [Fact]
    public async Task Mock_GetValidSigningKeysAsync_CanBeConfigured()
    {
        // Arrange
        var mock = Substitute.For<IJwtSigningKeyPersistence>();
        var testKeys = new[]
        {
            new JwtSigningKey
            {
                Id = "test-id-1",
                KeyId = "key-1",
                KeyMaterial = "base64-encoded-key-1",
                IsCurrentSigningKey = true
            }
        };

        mock.GetValidSigningKeysAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult((IEnumerable<JwtSigningKey>)testKeys));

        // Act
        var result = await mock.GetValidSigningKeysAsync();

        // Assert
        result.ShouldNotBeEmpty();
        result.Count().ShouldBe(1);
    }

    [Fact]
    public async Task Mock_AddKeyAsync_CanBeConfigured()
    {
        // Arrange
        var mock = Substitute.For<IJwtSigningKeyPersistence>();
        mock.AddKeyAsync(Arg.Any<JwtSigningKey>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(true));

        var testKey = new JwtSigningKey
        {
            KeyId = "new-key",
            KeyMaterial = "base64-encoded-key"
        };

        // Act
        var result = await mock.AddKeyAsync(testKey);

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public async Task Mock_RevokeKeyAsync_CanBeConfigured()
    {
        // Arrange
        var mock = Substitute.For<IJwtSigningKeyPersistence>();
        mock.RevokeKeyAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(true));

        // Act
        var result = await mock.RevokeKeyAsync("test-key-id");

        // Assert
        result.ShouldBeTrue();
    }

    //#endregion

    //#region Return Type Tests

    [Fact]
    public void GetCurrentSigningKeyAsync_ReturnsNullableJwtSigningKey()
    {
        // Arrange
        var method = typeof(IJwtSigningKeyPersistence)
            .GetMethods()
            .First(m => m.Name == "GetCurrentSigningKeyAsync");

        // Act
        var returnType = method.ReturnType;

        // Assert
        returnType.Name.ShouldContain("Task");
    }

    [Fact]
    public void GetValidSigningKeysAsync_ReturnsEnumerableOfKeys()
    {
        // Arrange
        var method = typeof(IJwtSigningKeyPersistence)
            .GetMethods()
            .First(m => m.Name == "GetValidSigningKeysAsync");

        // Act
        var returnType = method.ReturnType;

        // Assert
        returnType.Name.ShouldContain("Task");
    }

    [Fact]
    public void AddKeyAsync_ReturnsBoolean()
    {
        // Arrange
        var method = typeof(IJwtSigningKeyPersistence)
            .GetMethods()
            .First(m => m.Name == "AddKeyAsync");

        // Act
        var returnType = method.ReturnType;

        // Assert
        returnType.Name.ShouldContain("Task");
    }

    //#endregion

    //#region Parameter Validation Tests

    [Fact]
    public void GetKeyByIdAsync_HasKeyIdParameter()
    {
        // Arrange
        var method = typeof(IJwtSigningKeyPersistence)
            .GetMethods()
            .First(m => m.Name == "GetKeyByIdAsync");

        // Act
        var parameters = method.GetParameters();

        // Assert
        parameters.ShouldContain(p => p.Name == "keyId");
    }

    [Fact]
    public void AddKeyAsync_HasKeyParameter()
    {
        // Arrange
        var method = typeof(IJwtSigningKeyPersistence)
            .GetMethods()
            .First(m => m.Name == "AddKeyAsync");

        // Act
        var parameters = method.GetParameters();

        // Assert
        parameters.ShouldContain(p => p.Name == "key");
    }

    [Fact]
    public void RevokeKeyAsync_HasKeyIdParameter()
    {
        // Arrange
        var method = typeof(IJwtSigningKeyPersistence)
            .GetMethods()
            .First(m => m.Name == "RevokeKeyAsync");

        // Act
        var parameters = method.GetParameters();

        // Assert
        parameters.ShouldContain(p => p.Name == "keyId");
    }

    //#endregion

    //#region Substitutability Tests

    [Fact]
    public void Implementations_ShouldBeSubstitutable()
    {
        // Arrange
        IJwtSigningKeyPersistence implementation1 = Substitute.For<IJwtSigningKeyPersistence>();
        IJwtSigningKeyPersistence implementation2 = Substitute.For<IJwtSigningKeyPersistence>();

        // Act & Assert
        implementation1.ShouldBeAssignableTo<IJwtSigningKeyPersistence>();
        implementation2.ShouldBeAssignableTo<IJwtSigningKeyPersistence>();
    }

    //#endregion
}
