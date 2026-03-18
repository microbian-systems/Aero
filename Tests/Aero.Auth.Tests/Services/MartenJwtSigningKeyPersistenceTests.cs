using Shouldly;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Aero.Auth.Services;
using Aero.Models.Entities;
using Marten;
using Aero.MartenDB;

namespace Aero.Auth.Tests.Services;

/// <summary>
/// Unit tests for Marten JWT signing key persistence implementation.
/// </summary>
public class MartenJwtSigningKeyPersistenceTests : AeroDbTestDriver
{
    private readonly IAeroDbUnitOfWork _mockUow;
    private readonly ILogger<MartenJwtSigningKeyPersistence> _mockLogger;

    public MartenJwtSigningKeyPersistenceTests()
    {
        _mockUow = Substitute.For<IAeroDbUnitOfWork>();
        _mockLogger = Substitute.For<ILogger<MartenJwtSigningKeyPersistence>>();
    }

    //#region Constructor Tests

    [Fact]
    public void Constructor_WithValidDependencies_ShouldNotThrow()
    {
        // Act
        Action act = () => new MartenJwtSigningKeyPersistence(_mockUow, _mockLogger);

        // Assert
        act.ShouldNotThrow();
    }

    [Fact]
    public void Constructor_WithNullUow_ShouldThrowArgumentNullException()
    {
        // Act
        Action act = () => new MartenJwtSigningKeyPersistence(null!, _mockLogger);

        // Assert
        var ex = act.ShouldThrow<ArgumentNullException>();
        ex.ParamName.ShouldBe("uow");
    }

    [Fact]
    public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
    {
        // Act
        Action act = () => new MartenJwtSigningKeyPersistence(_mockUow, null!);

        // Assert
        var ex = act.ShouldThrow<ArgumentNullException>();
        ex.ParamName.ShouldBe("logger");
    }

    //#endregion

    //#region Interface Implementation Tests

    [Fact]
    public void MartenJwtSigningKeyPersistence_ImplementsInterface()
    {
        // Arrange & Act
        IJwtSigningKeyPersistence persistence = new MartenJwtSigningKeyPersistence(_mockUow, _mockLogger);

        // Assert
        persistence.ShouldBeAssignableTo<IJwtSigningKeyPersistence>();
    }

    //#endregion

    //#region AddKey Tests

    [Fact]
    public async Task AddKeyAsync_WithValidKey_ShouldReturnTrue()
    {
        // Arrange
        using var session = store.LightweightSession();
        var uow = new AeroUnitOfWork(session, Substitute.For<ILogger<AeroUnitOfWork>>(), Substitute.For<ILoggerFactory>());
        var persistence = new MartenJwtSigningKeyPersistence(uow, _mockLogger);
        
        var keyToAdd = new JwtSigningKey
        {
            KeyId = "new-key",
            KeyMaterial = Convert.ToBase64String(new byte[32]),
            IsCurrentSigningKey = true
        };

        // Act
        var result = await persistence.AddKeyAsync(keyToAdd);
        await uow.SaveChangesAsync(); // Ensure it persists
        

        // Assert
        result.ShouldBeTrue();

        // Verify it was actually added
        using var verifySession = store.LightweightSession();
        var savedKey = await verifySession.Query<JwtSigningKey>().FirstOrDefaultAsync(k => k.KeyId == "new-key");
        savedKey.ShouldNotBeNull();
        savedKey!.IsCurrentSigningKey.ShouldBeTrue();
    }

    [Fact]
    public async Task AddKeyAsync_WithNullKey_ShouldThrowArgumentNullException()
    {
        // Arrange
        var persistence = new MartenJwtSigningKeyPersistence(_mockUow, _mockLogger);

        // Act
        Func<Task> act = () => persistence.AddKeyAsync(null!);

        // Assert
        await act.ShouldThrowAsync<ArgumentNullException>();
    }

    //#endregion

    //#region UpdateKey Tests

    [Fact]
    public async Task UpdateKeyAsync_WithValidKey_ShouldReturnTrue()
    {
        // Arrange
        using var session = store.LightweightSession();
        var uow = new AeroUnitOfWork(session, Substitute.For<ILogger<AeroUnitOfWork>>(), Substitute.For<ILoggerFactory>());
        var persistence = new MartenJwtSigningKeyPersistence(uow, _mockLogger);

        var existingKey = new JwtSigningKey
        {
            KeyId = "existing-key",
            KeyMaterial = Convert.ToBase64String(new byte[32]),
            IsCurrentSigningKey = true
        };
        await persistence.AddKeyAsync(existingKey);
        await uow.SaveChangesAsync();
        

        var keyToUpdate = new JwtSigningKey
        {
            Id = existingKey.Id, // Marten assigned ID
            KeyId = "existing-key",
            KeyMaterial = Convert.ToBase64String(new byte[32]),
            IsCurrentSigningKey = false
        };

        // Act
        var result = await persistence.UpdateKeyAsync(keyToUpdate);
        await uow.SaveChangesAsync();
        

        // Assert
        result.ShouldBeTrue();
        
        using var verifySession = store.LightweightSession();
        var updatedKey = await verifySession.LoadAsync<JwtSigningKey>(existingKey.Id);
        updatedKey.ShouldNotBeNull();
        updatedKey.IsCurrentSigningKey.ShouldBeFalse();
    }

    //#endregion

    //#region RevokeKey Tests

    [Fact]
    public async Task RevokeKeyAsync_WithValidKeyId_ShouldReturnTrue()
    {
        // Arrange
        using var session = store.LightweightSession();
        var uow = new AeroUnitOfWork(session, Substitute.For<ILogger<AeroUnitOfWork>>(), Substitute.For<ILoggerFactory>());
        var persistence = new MartenJwtSigningKeyPersistence(uow, _mockLogger);

        var existingKey = new JwtSigningKey
        {
            KeyId = "valid-key-id",
            KeyMaterial = Convert.ToBase64String(new byte[32]),
            IsCurrentSigningKey = true
        };
        await persistence.AddKeyAsync(existingKey);
        await uow.SaveChangesAsync();
        

        // Act
        var result = await persistence.RevokeKeyAsync("valid-key-id");
        await uow.SaveChangesAsync();

        // Assert
        result.ShouldBeTrue();

        using var verifySession = store.LightweightSession();
        var revokedKey = await verifySession.LoadAsync<JwtSigningKey>(existingKey.Id);
        revokedKey.ShouldNotBeNull();
        revokedKey.RevokedAt.ShouldNotBeNull();
    }

    //#endregion

    //#region GetKeyById Tests

    [Fact]
    public async Task GetKeyByIdAsync_WithNullKeyId_ShouldThrowArgumentException()
    {
        // Arrange
        var persistence = new MartenJwtSigningKeyPersistence(_mockUow, _mockLogger);

        // Act
        Func<Task> act = () => persistence.GetKeyByIdAsync(null!);

        // Assert
        await act.ShouldThrowAsync<ArgumentException>();
    }

    //#endregion

    //#region SaveChanges Tests

    [Fact]
    public async Task SaveChangesAsync_ShouldCallUowSaveChanges()
    {
        // Arrange
        _mockUow.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(1));

        var persistence = new MartenJwtSigningKeyPersistence(_mockUow, _mockLogger);

        // Act
        var result = await persistence.SaveChangesAsync();

        // Assert
        result.ShouldBeTrue();
        await _mockUow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    //#endregion

    //#region Error Handling Tests

    [Fact]
    public async Task AddKeyAsync_ShouldReturnFalseOnException()
    {
        // Arrange
        var mockUow = Substitute.For<IAeroDbUnitOfWork>();
        var mockSession = Substitute.For<IDocumentSession>();
        mockUow.Session.Returns(mockSession);
        mockSession.When(x => x.Store(Arg.Any<JwtSigningKey>()))
            .Do(x => throw new Exception("Database error"));
            
        var persistence = new MartenJwtSigningKeyPersistence(mockUow, _mockLogger);
        
        var keyToAdd = new JwtSigningKey
        {
            KeyId = "key",
            KeyMaterial = Convert.ToBase64String(new byte[32])
        };

        // Act
        var result = await persistence.AddKeyAsync(keyToAdd);

        // Assert
        result.ShouldBeFalse();
    }

    //#endregion
}
