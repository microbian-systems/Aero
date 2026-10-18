using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Text;
using Aero.Auth.Persistence;
using Aero.Auth.Services;
using Aero.Auth.Sable.Tests.TestSupport;
using Aero.Models.Entities;
using AeroDB.Sable;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.Extensions;
using Shouldly;
using TUnit.Core;

namespace Aero.Auth.Sable.Tests.Services;

/// <summary>Security behavior at the service/session boundary.</summary>
public class RefreshTokenSecurityTests
{
    [Test]
    public void Replaced_public_token_is_not_active()
    {
        var token = new RefreshToken
        {
            ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(5),
            ReplacedByTokenId = 1
        };

        token.IsActive.ShouldBeFalse();
    }

    [Test]
    [Arguments(TokenState.Expired)]
    [Arguments(TokenState.Revoked)]
    [Arguments(TokenState.Replaced)]
    public async Task Rotate_rejects_non_active_documents_without_writes(TokenState state)
    {
        var token = ActiveDocument("old", 81);
        switch (state)
        {
            case TokenState.Expired: token.ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(-1); break;
            case TokenState.Revoked: token.RevokedAt = DateTimeOffset.UtcNow; break;
            case TokenState.Replaced: token.ReplacedByTokenId = 99; break;
        }

        var (service, session, writes) = CreateService(QueryReturning(token));

        await Should.ThrowAsync<InvalidOperationException>(() => service.RotateRefreshTokenAsync("old", "web"));

        writes.All.ShouldBeEmpty();
        await session.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Rotate_propagates_an_unknown_commit_outcome_without_retrying()
    {
        var expected = new CommitOutcomeUnknownException(new TimeoutException("ack lost"));
        var (service, session, writes) = CreateService(QueryReturning(ActiveDocument("old", 82)), expected);

        var actual = await Should.ThrowAsync<CommitOutcomeUnknownException>(() => service.RotateRefreshTokenAsync("old", "web"));

        actual.ShouldBeSameAs(expected);
        writes.All.Count.ShouldBe(2);
        await session.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Revoke_propagates_an_unknown_commit_outcome_without_retrying()
    {
        var expected = new CommitOutcomeUnknownException(new TimeoutException("ack lost"));
        var token = ActiveDocument("revoke", 83);
        var (service, session, writes) = CreateService(QueryReturning(token), expected);

        var actual = await Should.ThrowAsync<CommitOutcomeUnknownException>(() => service.RevokeRefreshTokenAsync("revoke"));

        actual.ShouldBeSameAs(expected);
        writes.All.ShouldHaveSingleItem().ShouldBe(token);
        await session.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Rotate_forwards_query_cancellation_without_a_write_or_retry()
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        var query = new TestSableQueryable<RefreshTokenDocument>([]);
        var (service, session, writes) = CreateService(query);

        await Should.ThrowAsync<OperationCanceledException>(() => service.RotateRefreshTokenAsync("cancelled", "web", cancellationToken: cancellation.Token));

        writes.All.ShouldBeEmpty();
        await session.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        query.FirstOrDefaultCalls.ShouldBe(1);
        query.LastCancellationToken.ShouldBe(cancellation.Token);
    }

    [Test]
    public async Task Revoke_forwards_query_cancellation_without_a_write_or_retry()
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        var query = new TestSableQueryable<RefreshTokenDocument>([]);
        var (service, session, writes) = CreateService(query);

        await Should.ThrowAsync<OperationCanceledException>(() => service.RevokeRefreshTokenAsync("cancelled", cancellation.Token));

        writes.All.ShouldBeEmpty();
        await session.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        query.FirstOrDefaultCalls.ShouldBe(1);
        query.LastCancellationToken.ShouldBe(cancellation.Token);
    }

    [Test]
    public async Task Revoke_rejects_unversioned_legacy_documents_without_writing()
    {
        var legacy = ActiveDocument("legacy", 84);
        legacy.Version = 0;
        var (service, session, writes) = CreateService(QueryReturning(legacy));

        await Should.ThrowAsync<InvalidOperationException>(() => service.RevokeRefreshTokenAsync("legacy"));

        writes.All.ShouldBeEmpty();
        await session.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Validate_rejects_unversioned_legacy_documents()
    {
        var legacy = ActiveDocument("legacy-validate", 85);
        legacy.Version = 0;
        var (service, session, _) = CreateService(QueryReturning(legacy));

        (await service.ValidateRefreshTokenAsync("legacy-validate")).ShouldBeNull();

        await session.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    private static (RefreshTokenService Service, IDocumentSession Session, Writes Writes) CreateService(
        TestSableQueryable<RefreshTokenDocument>? query = null,
        Exception? saveFailure = null)
    {
        var session = Substitute.For<IDocumentSession>();
        var writes = new Writes();
        session.When(s => s.Insert(Arg.Any<RefreshTokenDocument[]>()))
            .Do(call => writes.All.AddRange(call.Arg<RefreshTokenDocument[]>()));
        session.When(s => s.UpdateExpectedVersion(Arg.Any<RefreshTokenDocument>(), Arg.Any<long>()))
            .Do(call => writes.All.Add(call.Arg<RefreshTokenDocument>()));
        session.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(_ => saveFailure is null ? Task.FromResult(1) : Task.FromException<int>(saveFailure));
        // Configure the generic return type before Query<T> is invoked. This
        // keeps NSubstitute from attempting to auto-proxy the internal server
        // document query type.
        session.ReturnsForAll<ISableQueryable<RefreshTokenDocument>>(
            query ?? new TestSableQueryable<RefreshTokenDocument>([]));

        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Auth:RefreshTokenLifetimeDays"] = "30"
        }).Build();
        return (new RefreshTokenService(session, Substitute.For<ILogger<RefreshTokenService>>(), config), session, writes);
    }

    private static TestSableQueryable<RefreshTokenDocument> QueryReturning(RefreshTokenDocument document) => new([document]);

    private static RefreshTokenDocument ActiveDocument(string plaintext, long userId) => new()
    {
        Id = userId * 10,
        UserId = userId,
        TokenHash = Hash(plaintext),
        ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(5),
        Version = 1
    };

    private static string Hash(string token) => Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(token)));

    public enum TokenState { Expired, Revoked, Replaced }

    private sealed class Writes
    {
        public List<RefreshTokenDocument> All { get; } = [];
    }
}
