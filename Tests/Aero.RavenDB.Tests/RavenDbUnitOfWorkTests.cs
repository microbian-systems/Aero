using Aero.Core;
using Aero.Models.Entities;
using FakeItEasy;
using Marten;
using Microsoft.Extensions.Logging;

namespace Aero.RavenDB.Tests;

public class RavenDbUnitOfWorkTests : RavenDbTestBase
{
    private readonly ILogger<RavenDbUnitOfWork> _uowLogger;
    private readonly ILoggerFactory _loggerFactory;

    public RavenDbUnitOfWorkTests()
    {
        _uowLogger = A.Fake<ILogger<RavenDbUnitOfWork>>();
        _loggerFactory = A.Fake<ILoggerFactory>();
        A.CallTo(() => _loggerFactory.CreateLogger(A<string>._)).Returns(A.Fake<ILogger>());
    }

    [Fact]
    public async Task SaveChangesAsync_Should_Return_Change_Count()
    {
        // Arrange
        using var session = DocumentStore.LightweightSession();
        var uow = new RavenDbUnitOfWork(session, _uowLogger, _loggerFactory);
        
        var user = new AeroUser { Id = "users/uow1", UserName = "u1", Email = "u1@e.com", FirstName = "f", LastName = "l", CreatedBy = "s" };
        await uow.Users.InsertAsync(user);

        // Act
        var count = await uow.SaveChangesAsync();

        // Assert
        count.ShouldBeGreaterThan(0);
    }

    [Fact]
    public void Users_Property_Should_Be_Lazy_Initialized()
    {
        // Arrange
        using var session = DocumentStore.LightweightSession();
        var uow = new RavenDbUnitOfWork(session, _uowLogger, _loggerFactory);

        // Act & Assert
        // Before access, we can't easily check the private field without reflection,
        // but we can verify it returns a valid instance and subsequent calls return the same instance.
        var users1 = uow.Users;
        var users2 = uow.Users;

        users1.ShouldNotBeNull();
        users1.ShouldBeSameAs(users2);
    }

    [Fact]
    public async Task SaveChangesAsync_Should_Handle_Exceptions()
    {
        // Arrange
        var session = A.Fake<IDocumentSession>();
        A.CallTo(() => session.Advanced.WhatChanged()).Returns(new System.Collections.Generic.Dictionary<string, Raven.Client.Documents.Session.DocumentsChanges[]> { { "key", new Raven.Client.Documents.Session.DocumentsChanges[1] } });
        A.CallTo(() => session.SaveChangesAsync(A<CancellationToken>._)).Throws(new Exception("RavenDB error"));
        
        var uow = new RavenDbUnitOfWork(session, _uowLogger, _loggerFactory);

        // Act
        var result = await uow.SaveChangesAsync();

        // Assert
        result.ShouldBe(0);
        // Verify logging happened
        // Note: Generic loggers are tricky to verify with A.CallTo, but we can verify it didn't crash.
    }

    [Fact]
    public async Task Find_Method_Locates_Users_Should_Success()
    {
        var session = DocumentStore.LightweightSession();
        var uow = new RavenDbUnitOfWork(session, _uowLogger, _loggerFactory);

        var user1 = new AeroUser()
        {
            Id = $"users/{Snowflake.NewId()}",
            CreatedOn = DateTime.UtcNow.AddDays(-1)
        };

        var user2 = new AeroUser()
        {
            Id = $"users/{Snowflake.NewId()}",
            CreatedOn = DateTime.UtcNow.AddDays(-2)
        };
        
        await uow.Users.InsertAsync(user1);
        await uow.Users.InsertAsync(user2);

        var changed =  await uow.SaveChangesAsync();
        changed.ShouldBeGreaterThanOrEqualTo(2);
        var searchData = await uow.Users
            .FindAsync(x => x.CreatedOn > DateTime.UtcNow.AddDays(-10));
        
        searchData.ShouldNotBeNullOrEmpty();
        searchData.Count().ShouldBeGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task RollbackTransactionAsync_Should_Clear_Session()
    {
        // Arrange
        using var session = DocumentStore.LightweightSession();
        var uow = new RavenDbUnitOfWork(session, _uowLogger, _loggerFactory);
        
        var user = new AeroUser { Id = "users/rollback", UserName = "rb", Email = "rb@e.com", FirstName = "f", LastName = "l", CreatedBy = "s" };
        await uow.Users.InsertAsync(user);
        
        session.Advanced.HasChanged(user).ShouldBeTrue();

        // Act
        await uow.RollbackTransactionAsync();

        // Assert
        session.Advanced.HasChanged(user).ShouldBeFalse();
        var count = await uow.SaveChangesAsync();
        count.ShouldBe(0);
    }
}
