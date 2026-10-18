using System.Diagnostics;
using Aero.Auth.Persistence;
using Aero.Auth.Services;
using Aero.Core;
using Aero.Core.Entities;
using AeroDB.Sable;
using AeroDB.Sable.Diagnostics;
using AeroDB.Sable.Metadata;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Shouldly;
using SurrealDb.Net.Exceptions;
using TUnit.Core;

namespace Aero.Auth.Sable.Tests.Persistence;

/// <summary>
/// Server proof for the generated refresh-token mapping. Every test starts an
/// authenticated, disposable SurrealDB 3.2.4 container on a random loopback
/// port; it never connects to the developer catalog on port 8000.
/// </summary>
public sealed class RefreshTokenPersistenceIntegrationTests
{
    [Test]
    public void Configure_uses_the_expected_table_and_generated_materializer()
    {
        var options = new StoreOptions();

        RefreshTokenPersistence.Configure(options);

        options.Schema.TableNameFor<RefreshTokenDocument>().ShouldBe("refresh_token");
        options.Schema.For<RefreshTokenDocument>().UseOptimisticConcurrency.ShouldBeTrue();
        var metadata = MetadataRegistry.TryGet<RefreshTokenDocument>().ShouldNotBeNull();
        metadata.HasVersion.ShouldBeTrue();
        MetadataRegistry.GetShimType(typeof(RefreshTokenDocument)).ShouldNotBeNull();
    }

    [Test]
    public async Task Concurrent_rotations_commit_exactly_one_successor_and_reject_replay()
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromMinutes(2));
        await using var fixture = await IsolatedSurrealFixture.StartAsync(timeout.Token);
        await using var leftStore = await fixture.CreateStoreAsync(timeout.Token);
        await using var rightStore = await fixture.CreateStoreAsync(timeout.Token);

        string predecessorToken;
        await using (var seed = await leftStore.LightweightSessionAsync(timeout.Token))
        {
            predecessorToken = await CreateService(seed).GenerateRefreshTokenAsync(7101, "test", cancellationToken: timeout.Token);
        }

        RefreshTokenDocument predecessor;
        var predecessorHash = Hash(predecessorToken);
        await using (var seedQuery = await leftStore.QuerySessionAsync(timeout.Token))
        {
            predecessor = (await seedQuery.Query<RefreshTokenDocument>()
                .FirstOrDefaultAsync(token => token.TokenHash == predecessorHash, timeout.Token)).ShouldNotBeNull();
        }

        var barrier = new PostVersionReadBarrier(predecessor.Id);
        await using var left = await leftStore.OpenSessionAsync(new SessionOptions
        {
            Listeners = [barrier]
        }, timeout.Token);
        await using var right = await rightStore.OpenSessionAsync(new SessionOptions
        {
            Listeners = [barrier]
        }, timeout.Token);
        var leftAttempt = RotateServiceAttemptAsync(CreateService(left), predecessorToken, timeout.Token);
        var rightAttempt = RotateServiceAttemptAsync(CreateService(right), predecessorToken, timeout.Token);
        (string? Token, Exception? Failure)[] results;
        try
        {
            await barrier.WaitUntilBothWritersReachedThePostReadBarrierAsync(timeout.Token);
            barrier.ReleaseWriters();
            results = await Task.WhenAll(leftAttempt, rightAttempt);
        }
        finally
        {
            barrier.ReleaseWriters();
        }

        results.Count(result => result.Token is not null).ShouldBe(1);
        var rejected = results.Single(result => result.Failure is not null).Failure!;
        IsKnownConcurrencyConflict(rejected).ShouldBeTrue();

        await using var verify = await leftStore.QuerySessionAsync(timeout.Token);
        var documents = await verify.Query<RefreshTokenDocument>().ToListAsync(timeout.Token);
        var persistedPredecessor = documents.Single(token => token.Id == predecessor.Id);
        var successors = documents.Where(token => token.Id != predecessor.Id).ToList();
        persistedPredecessor.Version.ShouldBe(2);
        persistedPredecessor.ReplacedByTokenId.ShouldBe(successors.ShouldHaveSingleItem().Id);
        successors[0].Version.ShouldBe(1);

        var winner = results.Single(result => result.Token is not null).Token!;
        await using (var validateSession = await rightStore.LightweightSessionAsync(timeout.Token))
        {
            var validation = CreateService(validateSession);
            (await validation.ValidateRefreshTokenAsync(predecessorToken, timeout.Token)).ShouldBeNull();
            (await validation.ValidateRefreshTokenAsync(winner, timeout.Token)).ShouldBe(7101);
        }

        await using (var revokeSession = await leftStore.LightweightSessionAsync(timeout.Token))
        {
            await CreateService(revokeSession).RevokeRefreshTokenAsync(winner, timeout.Token);
        }

        await using (var afterRevokeSession = await rightStore.LightweightSessionAsync(timeout.Token))
        {
            (await CreateService(afterRevokeSession).ValidateRefreshTokenAsync(winner, timeout.Token)).ShouldBeNull();
        }

        await using var replay = await leftStore.LightweightSessionAsync(timeout.Token);
        var stale = (await replay.Query<RefreshTokenDocument>()
            .FirstOrDefaultAsync(token => token.Id == predecessor.Id, timeout.Token)).ShouldNotBeNull();
        replay.UpdateExpectedVersion(stale, 1);
        await Should.ThrowAsync<ConcurrencyException>(() => replay.SaveChangesAsync(timeout.Token));
    }

    [Test]
    public async Task Unique_hash_and_precommit_failure_do_not_leave_partial_writes()
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromMinutes(2));
        await using var fixture = await IsolatedSurrealFixture.StartAsync(timeout.Token);
        await using var store = await fixture.CreateStoreAsync(timeout.Token);
        var predecessor = NewDocument(7201, "rollback-predecessor");
        await InsertAsync(store, predecessor, timeout.Token);

        await using (var duplicate = await store.LightweightSessionAsync(timeout.Token))
        {
            duplicate.Insert(NewDocument(7202, "rollback-predecessor"));
            // Sable intentionally redacts database write details. The later
            // fresh query proves this rejected duplicate made no durable row.
            await Should.ThrowAsync<InvalidOperationException>(() => duplicate.SaveChangesAsync(timeout.Token));
        }

        await using (var failing = await store.OpenSessionAsync(new SessionOptions
        {
            Listeners = [new ThrowBeforeCommitListener()]
        }, timeout.Token))
        {
            var loaded = (await failing.Query<RefreshTokenDocument>()
                .FirstOrDefaultAsync(token => token.Id == predecessor.Id, timeout.Token)).ShouldNotBeNull();
            var successor = NewDocument(7201, "rollback-successor");
            loaded.ReplacedByTokenId = successor.Id;
            failing.UpdateExpectedVersion(loaded, loaded.Version);
            failing.Insert(successor);

            await Should.ThrowAsync<InvalidOperationException>(() => failing.SaveChangesAsync(timeout.Token));
        }

        await using var verify = await store.QuerySessionAsync(timeout.Token);
        var documents = await verify.Query<RefreshTokenDocument>().ToListAsync(timeout.Token);
        documents.Count.ShouldBe(1);
        documents[0].Id.ShouldBe(predecessor.Id);
        documents[0].Version.ShouldBe(1);
        documents[0].ReplacedByTokenId.ShouldBeNull();
    }

    private static async Task InsertAsync(IDocumentStore store, RefreshTokenDocument document, CancellationToken ct)
    {
        await using var session = await store.LightweightSessionAsync(ct);
        session.Insert(document);
        await session.SaveChangesAsync(ct);
    }

    private static RefreshTokenService CreateService(IDocumentSession session)
        => new(
            session,
            NullLogger<RefreshTokenService>.Instance,
            new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Auth:RefreshTokenLifetimeDays"] = "30"
            }).Build());

    private static async Task<(string? Token, Exception? Failure)> RotateServiceAttemptAsync(
        RefreshTokenService service,
        string token,
        CancellationToken ct)
    {
        try
        {
            return (await service.RotateRefreshTokenAsync(token, "test", cancellationToken: ct), null);
        }
        catch (Exception exception)
        {
            return (null, exception);
        }
    }

    private static bool IsKnownConcurrencyConflict(Exception exception)
    {
        for (var current = exception; current is not null; current = current.InnerException)
        {
            if (current is ConcurrencyException)
            {
                return true;
            }

            if (current is SurrealDbException
                && current.Message.Contains("conflict", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static RefreshTokenDocument NewDocument(long userId, string tokenHash) => new()
    {
        Id = Snowflake.NewId(),
        UserId = userId,
        TokenHash = tokenHash,
        ClientType = "test",
        CreatedOn = DateTimeOffset.UtcNow,
        ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(30),
        Version = 1
    };

    private static string Hash(string token)
        => Convert.ToBase64String(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(token)));

    private sealed class ThrowBeforeCommitListener : DocumentSessionListenerBase
    {
        public override Task BeforeCommitAsync(IDocumentSession session, CancellationToken ct)
            => Task.FromException(new InvalidOperationException("Injected pre-commit failure."));
    }

    private sealed class PostVersionReadBarrier(long predecessorId) : DocumentSessionListenerBase
    {
        private readonly TaskCompletionSource<bool> bothWritersArrived = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly TaskCompletionSource<bool> releaseWriters = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private int arrivals;

        public override async Task BeforeStoreAsync(IDocumentSession session, object entity, CancellationToken ct)
        {
            if (entity is not RefreshTokenDocument token || token.Id != predecessorId)
            {
                return;
            }

            if (Interlocked.Increment(ref arrivals) == 2)
            {
                bothWritersArrived.TrySetResult(true);
            }

            await releaseWriters.Task.WaitAsync(ct);
        }

        public Task WaitUntilBothWritersReachedThePostReadBarrierAsync(CancellationToken ct)
            => bothWritersArrived.Task.WaitAsync(TimeSpan.FromSeconds(20), ct);

        public void ReleaseWriters() => releaseWriters.TrySetResult(true);
    }

    private sealed class IsolatedSurrealFixture : IAsyncDisposable
    {
        private const string Username = "refresh_token_test";
        private const string Password = "refresh-token-local-only";
        private readonly string containerName;
        private readonly int port;
        private readonly string database;

        private IsolatedSurrealFixture(string containerName, int port, string database)
        {
            this.containerName = containerName;
            this.port = port;
            this.database = database;
        }

        public static async Task<IsolatedSurrealFixture> StartAsync(CancellationToken ct)
        {
            var fixture = new IsolatedSurrealFixture(
                $"aero-auth-refresh-token-{Guid.NewGuid():N}",
                GetAvailableLoopbackPort(),
                $"auth_refresh_{Guid.NewGuid():N}");
            await RunDockerAsync(
                $"run -d --rm --name {fixture.containerName} --label aerodb.testfixture=auth-refresh-token " +
                $"-p 127.0.0.1:{fixture.port}:8000 surrealdb/surrealdb:v3.2.4 " +
                $"start --user {Username} --pass {Password} memory",
                ct);
            try
            {
                await fixture.WaitUntilReadyAsync(ct);
                return fixture;
            }
            catch
            {
                await fixture.DisposeAsync();
                throw;
            }
        }

        public async Task<IDocumentStore> CreateStoreAsync(CancellationToken ct)
        {
            var store = Documents.For(options =>
            {
                options.Endpoint = $"ws://127.0.0.1:{port}/rpc";
                options.Namespace = "aero_auth_refresh_token_test";
                options.Database = database;
                options.Username = Username;
                options.Password = Password;
                options.Schema.AutoCreate = true;
                RefreshTokenPersistence.Configure(options);
            });

            try
            {
                await store.InitializeAsync(ct);
                return store;
            }
            catch
            {
                await store.DisposeAsync();
                throw;
            }
        }

        public async ValueTask DisposeAsync()
        {
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(15));
            await RunDockerAsync($"rm -f {containerName}", timeout.Token, tolerateFailure: true);
        }

        private async Task WaitUntilReadyAsync(CancellationToken ct)
        {
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
            timeout.CancelAfter(TimeSpan.FromSeconds(30));
            Exception? lastFailure = null;
            while (!timeout.IsCancellationRequested)
            {
                try
                {
                    await using var store = await CreateStoreAsync(timeout.Token);
                    if ((await store.GetVersionAsync(timeout.Token))?.Contains("3.2.4", StringComparison.Ordinal) == true)
                        return;
                }
                catch (Exception exception)
                {
                    lastFailure = exception;
                }

                await Task.Delay(TimeSpan.FromMilliseconds(250), timeout.Token);
            }

            throw new InvalidOperationException(
                $"The isolated authenticated SurrealDB fixture did not become ready. Last failure: {lastFailure?.GetType().Name ?? "none"}.");
        }

        private static int GetAvailableLoopbackPort()
        {
            var listener = new System.Net.Sockets.TcpListener(System.Net.IPAddress.Loopback, 0);
            listener.Start();
            var result = ((System.Net.IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();
            return result;
        }

        private static async Task RunDockerAsync(string arguments, CancellationToken ct, bool tolerateFailure = false)
        {
            using var process = Process.Start(new ProcessStartInfo("docker", arguments)
            {
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            }) ?? throw new InvalidOperationException("Could not start Docker for the isolated SurrealDB fixture.");
            var output = process.StandardOutput.ReadToEndAsync(ct);
            var error = process.StandardError.ReadToEndAsync(ct);
            await process.WaitForExitAsync(ct);
            if (!tolerateFailure && process.ExitCode != 0)
                throw new InvalidOperationException($"Docker command failed ({process.ExitCode}): {await error}{await output}");
        }
    }
}
