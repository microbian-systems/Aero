using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Text;
using Aero.Auth.Persistence;
using Aero.Auth.Services;
using Aero.Auth.Sable.Tests.TestSupport;
using AeroDB.Sable;
using AeroDB.Sable.Metadata;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.Extensions;
using Shouldly;
using TUnit.Core;

namespace Aero.Auth.Sable.Tests.Services;

public class RefreshTokenServiceTests
{
    [Test]
    public async Task Generate_inserts_a_version_one_hashed_server_document()
    {
        var (service, session, writes) = CreateService();
        using var cancellation = new CancellationTokenSource();

        var token = await service.GenerateRefreshTokenAsync(41, "desktop", "203.0.113.8", "Aero Desktop", cancellation.Token);

        var persisted = writes.Inserted.ShouldHaveSingleItem();
        persisted.UserId.ShouldBe(41);
        persisted.Version.ShouldBe(1);
        persisted.TokenHash.ShouldBe(Hash(token));
        persisted.TokenHash.ShouldNotBe(token);
        persisted.ClientType.ShouldBe("desktop");
        persisted.IssuedFromIpAddress.ShouldBe("203.0.113.8");
        persisted.UserAgent.ShouldBe("Aero Desktop");
        await session.Received(1).SaveChangesAsync(cancellation.Token);
    }

    [Test]
    public async Task Generate_honors_the_configured_lifetime()
    {
        var before = DateTimeOffset.UtcNow;
        var (service, _, writes) = CreateService(lifetimeDays: 7);

        await service.GenerateRefreshTokenAsync(42, "web");

        writes.Inserted.ShouldHaveSingleItem().ExpiresAt.ShouldBeInRange(
            before.AddDays(7),
            DateTimeOffset.UtcNow.AddDays(7));
    }

    [Test]
    public async Task Generate_propagates_a_save_failure_without_retrying()
    {
        var expected = new InvalidOperationException("write failed");
        var (service, session, writes) = CreateService(saveFailure: expected);

        var actual = await Should.ThrowAsync<InvalidOperationException>(() => service.GenerateRefreshTokenAsync(42, "web"));

        actual.ShouldBeSameAs(expected);
        writes.Inserted.ShouldHaveSingleItem();
        await session.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Validate_returns_null_without_querying_for_blank_tokens()
    {
        var (service, session, _) = CreateService();

        (await service.ValidateRefreshTokenAsync(string.Empty)).ShouldBeNull();
        session.ReceivedCalls().ShouldBeEmpty();
    }

    [Test]
    public async Task Validate_returns_the_user_and_forwards_cancellation_for_an_active_document()
    {
        var token = ActiveDocument("active", 43);
        var query = QueryReturning(token);
        var (service, session, _) = CreateService(query);
        using var cancellation = new CancellationTokenSource();

        (await service.ValidateRefreshTokenAsync("active", cancellation.Token)).ShouldBe(43);
        query.FirstOrDefaultCalls.ShouldBe(1);
        query.LastCancellationToken.ShouldBe(cancellation.Token);
    }

    [Test]
    public async Task Validate_rejects_expired_revoked_and_replaced_documents()
    {
        var expired = ActiveDocument("expired", 44);
        expired.ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(-1);
        var revoked = ActiveDocument("revoked", 45);
        revoked.RevokedAt = DateTimeOffset.UtcNow;
        var replaced = ActiveDocument("replaced", 46);
        replaced.ReplacedByTokenId = 99;
        foreach (var token in new[] { expired, revoked, replaced })
        {
            var (service, _, _) = CreateService(QueryReturning(token));
            (await service.ValidateRefreshTokenAsync(token.TokenHash == Hash("expired") ? "expired" : token.TokenHash == Hash("revoked") ? "revoked" : "replaced")).ShouldBeNull();
        }
    }

    [Test]
    public async Task Rotate_reads_once_updates_the_observed_version_and_inserts_a_clean_successor()
    {
        var predecessor = ActiveDocument("old", 47);
        predecessor.Id = 470;
        predecessor.Version = 2;
        var query = QueryReturning(predecessor);
        var (service, session, writes) = CreateService(query);

        var plaintextSuccessor = await service.RotateRefreshTokenAsync("old", "mobile");

        predecessor.ReplacedByTokenId.ShouldNotBeNull();
        writes.ExpectedUpdates.ShouldHaveSingleItem().ShouldBe((predecessor, 2));
        var successor = writes.Inserted.ShouldHaveSingleItem();
        successor.ReplacedByTokenId.ShouldBeNull();
        successor.Version.ShouldBe(1);
        successor.TokenHash.ShouldBe(Hash(plaintextSuccessor));
        query.FirstOrDefaultCalls.ShouldBe(1);
        await session.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Revoke_updates_the_observed_version_before_the_single_save()
    {
        var token = ActiveDocument("revoke", 48);
        token.Id = 480;
        token.Version = 3;
        var (service, session, writes) = CreateService(QueryReturning(token));

        await service.RevokeRefreshTokenAsync("revoke");

        token.RevokedAt.ShouldNotBeNull();
        writes.ExpectedUpdates.ShouldHaveSingleItem().ShouldBe((token, 3));
        await session.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Revoke_does_not_save_when_the_token_is_missing()
    {
        var (service, session, writes) = CreateService(new TestSableQueryable<RefreshTokenDocument>([]));

        await service.RevokeRefreshTokenAsync("missing");

        writes.ExpectedUpdates.ShouldBeEmpty();
        await session.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Revoke_all_updates_each_active_token_and_saves_once()
    {
        var first = ActiveDocument("first", 51);
        first.Version = 2;
        var second = ActiveDocument("second", 51);
        second.Id = 512;
        second.Version = 3;
        var otherUser = ActiveDocument("other-user", 53);
        var expired = ActiveDocument("expired", 51);
        expired.ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(-1);
        var revoked = ActiveDocument("revoked", 51);
        revoked.RevokedAt = DateTimeOffset.UtcNow;
        var replaced = ActiveDocument("replaced", 51);
        replaced.ReplacedByTokenId = 991;
        var query = new TestSableQueryable<RefreshTokenDocument>(
            [first, second, otherUser, expired, revoked, replaced]);
        var (service, session, writes) = CreateService(query);
        using var cancellation = new CancellationTokenSource();

        await service.RevokeAllUserTokensAsync(51, cancellation.Token);

        writes.ExpectedUpdates.ShouldBe([(first, 2), (second, 3)]);
        first.RevokedAt.ShouldNotBeNull();
        second.RevokedAt.ShouldNotBeNull();
        otherUser.RevokedAt.ShouldBeNull();
        expired.RevokedAt.ShouldBeNull();
        revoked.RevokedAt.ShouldNotBeNull();
        replaced.RevokedAt.ShouldBeNull();
        query.ToListCalls.ShouldBe(1);
        query.LastCancellationToken.ShouldBe(cancellation.Token);
        await session.Received(1).SaveChangesAsync(cancellation.Token);
    }

    [Test]
    public async Task Get_active_tokens_returns_the_persisted_active_document_fields()
    {
        var token = ActiveDocument("active-list", 52);
        token.ClientType = "desktop";
        token.IssuedFromIpAddress = "203.0.113.52";
        var otherUser = ActiveDocument("other-list", 54);
        var expired = ActiveDocument("expired-list", 52);
        expired.ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(-1);
        var revoked = ActiveDocument("revoked-list", 52);
        revoked.RevokedAt = DateTimeOffset.UtcNow;
        var replaced = ActiveDocument("replaced-list", 52);
        replaced.ReplacedByTokenId = 992;
        var legacy = ActiveDocument("legacy-list", 52);
        legacy.Version = 0;
        var (service, _, _) = CreateService(
            new TestSableQueryable<RefreshTokenDocument>([token, otherUser, expired, revoked, replaced, legacy]));

        var active = (await service.GetActiveTokensAsync(52)).ToList();

        active.ShouldBe([(token.Id, "desktop", token.CreatedOn, "203.0.113.52")]);
    }

    [Test]
    public void Generated_metadata_exposes_the_versioned_server_document_without_reflection()
    {
        var metadata = MetadataRegistry.TryGet<RefreshTokenDocument>().ShouldNotBeNull();

        metadata.HasVersion.ShouldBeTrue();
        metadata.VersionFieldName.ShouldBe("Version");
        metadata.GetVersionAccessor.ShouldNotBeNull();
        metadata.SetVersionAccessor.ShouldNotBeNull();
        MetadataRegistry.GetShimType(typeof(RefreshTokenDocument)).ShouldNotBeNull();
        var document = ActiveDocument("metadata", 49);
        document.Version = 7;
        metadata.GetVersionAccessor(document).ShouldBe(7);
    }

    private static (RefreshTokenService Service, IDocumentSession Session, Writes Writes) CreateService(
        TestSableQueryable<RefreshTokenDocument>? query = null,
        Exception? saveFailure = null,
        int lifetimeDays = 30)
    {
        var session = Substitute.For<IDocumentSession>();
        var writes = new Writes();
        session.When(s => s.Insert(Arg.Any<RefreshTokenDocument[]>()))
            .Do(call => writes.Inserted.AddRange(call.Arg<RefreshTokenDocument[]>()));
        session.When(s => s.UpdateExpectedVersion(Arg.Any<RefreshTokenDocument>(), Arg.Any<long>()))
            .Do(call => writes.ExpectedUpdates.Add((call.Arg<RefreshTokenDocument>(), call.ArgAt<long>(1))));
        session.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(_ => saveFailure is null ? Task.FromResult(1) : Task.FromException<int>(saveFailure));
        // Configure the generic return type before Query<T> is invoked. This
        // keeps NSubstitute from attempting to auto-proxy the internal server
        // document query type.
        session.ReturnsForAll<ISableQueryable<RefreshTokenDocument>>(
            query ?? new TestSableQueryable<RefreshTokenDocument>([]));

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["Auth:RefreshTokenLifetimeDays"] = lifetimeDays.ToString() })
            .Build();
        return (new RefreshTokenService(session, Substitute.For<ILogger<RefreshTokenService>>(), configuration), session, writes);
    }

    private static TestSableQueryable<RefreshTokenDocument> QueryReturning(RefreshTokenDocument document) => new([document]);

    private static RefreshTokenDocument ActiveDocument(string token, long userId) => new()
    {
        Id = userId * 10,
        UserId = userId,
        TokenHash = Hash(token),
        ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(5),
        Version = 1
    };

    private static string Hash(string value) => Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(value)));

    private sealed class Writes
    {
        public List<RefreshTokenDocument> Inserted { get; } = [];
        public List<(RefreshTokenDocument Token, long ExpectedVersion)> ExpectedUpdates { get; } = [];
    }
}
