using System.Net;
using System.Security.Cryptography;
using Aero.Storage;
using Aero.Storage.S3;
using Amazon.S3;
using Amazon.S3.Model;
using FluentValidation;
using NSubstitute;
using Shouldly;
using TUnit.Core;

namespace Aero.Storage.Tests;

public sealed class S3StorageTests
{
    private static readonly ObjectKey Key = ObjectKey.Parse("sha256/abc123.pdf");
    private static S3ObjectStore Store(IAmazonS3 client, bool r2 = false, long limit = 3) => new(client,
        new() { Bucket = "private-test", ScopePrefix = "shared/public", MaxObjectBytes = limit, UseR2Compatibility = r2 });
    private static GetObjectResponse ReadResponse(Stream body, long length = 3, HttpStatusCode status = HttpStatusCode.OK)
    {
        var response = new GetObjectResponse { ResponseStream = body, ContentLength = length, ETag = "opaque-etag",
            LastModified = new DateTime(2026, 10, 10, 12, 0, 0, DateTimeKind.Utc), HttpStatusCode = status };
        response.Headers.ContentType = "application/pdf";
        return response;
    }

    [Test]
    public async Task CreateOnlyUsesCurrentPositionWithoutClosingCaller()
    {
        var client = Substitute.For<IAmazonS3>();
        using var source = new MemoryStream([99, 1, 2, 3]);
        source.Position = 1;
        client.PutObjectAsync(Arg.Any<PutObjectRequest>(), Arg.Any<CancellationToken>()).Returns(call =>
        {
            var request = call.Arg<PutObjectRequest>();
            request.BucketName.ShouldBe("private-test");
            request.Key.ShouldBe("shared/public/sha256/abc123.pdf");
            request.IfNoneMatch.ShouldBe("*");
            request.AutoCloseStream.ShouldBeFalse();
            request.AutoResetStreamPosition.ShouldBeFalse();
            request.UseChunkEncoding.ShouldBeFalse();
            request.Headers.ContentLength.ShouldBe(3);
            request.ContentType.ShouldBe("application/pdf");
            using var uploaded = new MemoryStream();
            request.InputStream.CopyTo(uploaded);
            uploaded.ToArray().ShouldBe(new byte[] { 1, 2, 3 });
            return Task.FromResult(new PutObjectResponse { ETag = "opaque-etag", HttpStatusCode = HttpStatusCode.OK });
        });
        var created = await Store(client).CreateAsync(new(Key, 3, "application/pdf"), source);
        created.ShouldBe(new ObjectCreated(Key, 3, "opaque-etag"));
        source.CanRead.ShouldBeTrue();
        source.Position.ShouldBe(4);
        await client.Received(1).PutObjectAsync(Arg.Any<PutObjectRequest>(), Arg.Any<CancellationToken>());
        client.DidNotReceive().Dispose();
    }

    [Test]
    public async Task R2PutPreservesTransferChecksum()
    {
        var client = Substitute.For<IAmazonS3>();
        using var source = new MemoryStream([99, 1, 2, 3]);
        source.Position = 1;
        client.PutObjectAsync(Arg.Any<PutObjectRequest>(), Arg.Any<CancellationToken>()).Returns(call =>
        {
            var request = call.Arg<PutObjectRequest>();
            request.DisablePayloadSigning.ShouldBe(true);
            request.DisableDefaultChecksumValidation.ShouldBe(true);
            request.ChecksumSHA256.ShouldBe(Convert.ToBase64String(SHA256.HashData(new byte[] { 1, 2, 3 })));
            request.MD5Digest.ShouldBeNull();
            request.InputStream.Position.ShouldBe(1);
            return Task.FromResult(new PutObjectResponse { HttpStatusCode = HttpStatusCode.OK });
        });
        var created = await Store(client, r2: true).CreateAsync(new(Key, 3, "application/pdf"), source);
        created.Length.ShouldBe(3);
        source.CanRead.ShouldBeTrue();
    }

    [Test]
    [Arguments(0)]
    [Arguments(3)]
    public async Task EmptyAndExactLimitUploadsAreSupported(int length)
    {
        var client = Substitute.For<IAmazonS3>();
        client.PutObjectAsync(Arg.Any<PutObjectRequest>(), Arg.Any<CancellationToken>())
            .Returns(new PutObjectResponse { HttpStatusCode = HttpStatusCode.OK });
        using var source = new MemoryStream(new byte[length]);
        var result = await Store(client, true).CreateAsync(new(Key, length, "application/octet-stream"), source);
        result.Length.ShouldBe(length);
        source.CanRead.ShouldBeTrue();
        await client.Received(1).PutObjectAsync(Arg.Is<PutObjectRequest>(r => r.IfNoneMatch == "*" && r.Headers.ContentLength == length), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task AlreadyExistsIsAConflictNotSuccessfulDeduplication()
    {
        var client = Substitute.For<IAmazonS3>();
        client.PutObjectAsync(Arg.Any<PutObjectRequest>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<PutObjectResponse>(new AmazonS3Exception("private data") { StatusCode = HttpStatusCode.PreconditionFailed }));
        using var source = new MemoryStream([1, 2, 3]);
        var error = await Should.ThrowAsync<ObjectStorageException>(() => Store(client).CreateAsync(new(Key, 3, "application/pdf"), source));
        error.Error.ShouldBe(StorageError.AlreadyExists);
        error.InnerException.ShouldBeNull();
        error.ToString().ShouldNotContain("private data");
        source.CanRead.ShouldBeTrue();
        await client.DidNotReceive().GetObjectMetadataAsync(Arg.Any<GetObjectMetadataRequest>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task UploadValidationPrecedesSdkCalls()
    {
        var client = Substitute.For<IAmazonS3>();
        using var source = new MemoryStream([1, 2, 3]);
        await Should.ThrowAsync<ValidationException>(() => Store(client).CreateAsync(new(Key, 4, "application/pdf"), source));
        await Should.ThrowAsync<ValidationException>(() => Store(client).CreateAsync(new(Key, 3, "application/pdf\r\nInjected: value"), source));
        var lengthError = await Should.ThrowAsync<ObjectStorageException>(() => Store(client).CreateAsync(new(Key, 2, "application/pdf"), source));
        lengthError.Error.ShouldBe(StorageError.LengthMismatch);
        var nonseek = Substitute.For<Stream>();
        nonseek.CanRead.Returns(true);
        nonseek.CanSeek.Returns(false);
        await Should.ThrowAsync<NotSupportedException>(() => Store(client).CreateAsync(new(Key, 3, "application/pdf"), nonseek));
        await client.DidNotReceive().PutObjectAsync(Arg.Any<PutObjectRequest>(), Arg.Any<CancellationToken>());
        source.Position.ShouldBe(0);
        source.CanRead.ShouldBeTrue();
    }

    [Test]
    [Arguments("../escape")]
    [Arguments("/absolute")]
    [Arguments("a//b")]
    [Arguments("a/./b")]
    [Arguments("https://host/file")]
    [Arguments("a\\b")]
    [Arguments("a/%2e%2e/b")]
    [Arguments("a\r\nb")]
    public void KeysRejectTraversalAndUrls(string value) => Should.Throw<ValidationException>(() => ObjectKey.Parse(value));

    [Test]
    public async Task HeadAndReadShareScopeAndMetadata()
    {
        var client = Substitute.For<IAmazonS3>();
        var head = new GetObjectMetadataResponse { ContentLength = 3, ETag = "opaque-etag",
            LastModified = new DateTime(2026, 10, 10, 12, 0, 0, DateTimeKind.Utc), HttpStatusCode = HttpStatusCode.OK };
        head.Headers.ContentType = "application/pdf";
        client.GetObjectMetadataAsync(Arg.Any<GetObjectMetadataRequest>(), Arg.Any<CancellationToken>()).Returns(head);
        client.GetObjectAsync(Arg.Any<GetObjectRequest>(), Arg.Any<CancellationToken>()).Returns(ReadResponse(new MemoryStream([1, 2, 3])));
        var store = Store(client);
        var metadata = await store.HeadAsync(Key);
        await using var read = await store.OpenReadAsync(Key);
        metadata.ShouldBe(new ObjectMetadata(Key, 3, "application/pdf", "opaque-etag", new DateTimeOffset(2026, 10, 10, 12, 0, 0, TimeSpan.Zero)));
        read!.Metadata.ShouldBe(metadata);
        await client.Received(1).GetObjectMetadataAsync(Arg.Is<GetObjectMetadataRequest>(r => r.BucketName == "private-test" && r.Key == "shared/public/sha256/abc123.pdf"), Arg.Any<CancellationToken>());
        await client.Received(1).GetObjectAsync(Arg.Is<GetObjectRequest>(r => r.BucketName == "private-test" && r.Key == "shared/public/sha256/abc123.pdf"), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task ReadLeaseOwnsResponseAndPreservesBytes()
    {
        var client = Substitute.For<IAmazonS3>();
        var body = new MemoryStream([1, 2, 3]);
        client.GetObjectAsync(Arg.Any<GetObjectRequest>(), Arg.Any<CancellationToken>()).Returns(ReadResponse(body));
        var read = (await Store(client).OpenReadAsync(Key))!;
        read.Content.CanSeek.ShouldBeFalse();
        read.Content.CanWrite.ShouldBeFalse();
        using var output = new MemoryStream();
        await read.Content.CopyToAsync(output);
        output.ToArray().ShouldBe(new byte[] { 1, 2, 3 });
        read.Content.Position.ShouldBe(3);
        body.CanRead.ShouldBeTrue();
        read.Content.Dispose();
        await read.DisposeAsync();
        body.CanRead.ShouldBeFalse();
        read.Content.CanRead.ShouldBeFalse();
        Should.Throw<ObjectDisposedException>(() => read.Content.ReadByte());
        client.DidNotReceive().Dispose();
    }

    [Test]
    [Arguments(2, false)]
    [Arguments(4, false)]
    [Arguments(2, true)]
    [Arguments(4, true)]
    public async Task ReadLengthMismatchIsExplicit(int actualLength, bool synchronous)
    {
        var client = Substitute.For<IAmazonS3>();
        var body = new MemoryStream(new byte[actualLength]);
        client.GetObjectAsync(Arg.Any<GetObjectRequest>(), Arg.Any<CancellationToken>()).Returns(ReadResponse(body));
        await using var read = (await Store(client).OpenReadAsync(Key))!;
        using var destination = new MemoryStream();
        var error = synchronous ? Should.Throw<ObjectStorageException>(() => read.Content.CopyTo(destination))
            : await Should.ThrowAsync<ObjectStorageException>(() => read.Content.CopyToAsync(destination));
        error.Error.ShouldBe(StorageError.LengthMismatch);
        destination.Length.ShouldBe(Math.Min(3, actualLength));
        body.CanRead.ShouldBeTrue();
    }

    [Test]
    [Arguments(4, 200, StorageError.TooLarge)]
    [Arguments(-1, 200, StorageError.InvalidResponse)]
    [Arguments(3, 206, StorageError.InvalidResponse)]
    public async Task InvalidReadResponseIsDisposed(long length, int status, StorageError expected)
    {
        var client = Substitute.For<IAmazonS3>();
        var body = new MemoryStream([1, 2, 3]);
        client.GetObjectAsync(Arg.Any<GetObjectRequest>(), Arg.Any<CancellationToken>()).Returns(ReadResponse(body, length, (HttpStatusCode)status));
        var error = await Should.ThrowAsync<ObjectStorageException>(() => Store(client).OpenReadAsync(Key));
        error.Error.ShouldBe(expected);
        body.CanRead.ShouldBeFalse();
    }

    [Test]
    [Arguments(404, "NoSuchKey", null)]
    [Arguments(404, "NoSuchBucket", StorageError.Configuration)]
    [Arguments(403, "NoSuchKey", StorageError.AccessDenied)]
    public async Task MissingDeniedAndMissingBucketAreDifferent(int status, string code, StorageError? expected)
    {
        var client = Substitute.For<IAmazonS3>();
        var error = new AmazonS3Exception("secret diagnostics") { StatusCode = (HttpStatusCode)status, ErrorCode = code };
        client.GetObjectAsync(Arg.Any<GetObjectRequest>(), Arg.Any<CancellationToken>()).Returns(Task.FromException<GetObjectResponse>(error));
        client.GetObjectMetadataAsync(Arg.Any<GetObjectMetadataRequest>(), Arg.Any<CancellationToken>()).Returns(Task.FromException<GetObjectMetadataResponse>(error));
        var store = Store(client);
        if (expected is null)
        {
            (await store.OpenReadAsync(Key)).ShouldBeNull();
            (await store.HeadAsync(Key)).ShouldBeNull();
        }
        else
        {
            (await Should.ThrowAsync<ObjectStorageException>(() => store.OpenReadAsync(Key))).Error.ShouldBe(expected.Value);
            (await Should.ThrowAsync<ObjectStorageException>(() => store.HeadAsync(Key))).Error.ShouldBe(expected.Value);
        }
    }

    [Test]
    [Arguments(409, StorageError.Conflict)]
    [Arguments(429, StorageError.Throttled)]
    [Arguments(500, StorageError.Unavailable)]
    [Arguments(400, StorageError.InvalidResponse)]
    public async Task ProviderFailuresAreSanitized(int status, StorageError expected)
    {
        var client = Substitute.For<IAmazonS3>();
        client.PutObjectAsync(Arg.Any<PutObjectRequest>(), Arg.Any<CancellationToken>()).Returns(Task.FromException<PutObjectResponse>(new AmazonS3Exception("secret-signed-url") { StatusCode = (HttpStatusCode)status }));
        using var source = new MemoryStream([1, 2, 3]);
        var error = await Should.ThrowAsync<ObjectStorageException>(() => Store(client).CreateAsync(new(Key, 3, "application/pdf"), source));
        error.Error.ShouldBe(expected);
        error.ToString().ShouldNotContain("secret-signed-url");
        error.InnerException.ShouldBeNull();
        source.CanRead.ShouldBeTrue();
    }

    [Test]
    public async Task CallerCancellationIsNotStorageFailure()
    {
        var client = Substitute.For<IAmazonS3>();
        using var cancellation = new CancellationTokenSource();
        var store = Store(client);
        client.GetObjectAsync(Arg.Any<GetObjectRequest>(), cancellation.Token).Returns(_ =>
        {
            cancellation.Cancel();
            return Task.FromException<GetObjectResponse>(new AmazonS3Exception("private") { StatusCode = HttpStatusCode.ServiceUnavailable });
        });
        await Should.ThrowAsync<OperationCanceledException>(() => store.OpenReadAsync(Key, cancellation.Token));
        using var source = new MemoryStream([1, 2, 3]);
        await Should.ThrowAsync<OperationCanceledException>(() => store.CreateAsync(new(Key, 3, "application/pdf"), source, cancellation.Token));
        await Should.ThrowAsync<OperationCanceledException>(() => store.HeadAsync(Key, cancellation.Token));
        await client.DidNotReceive().PutObjectAsync(Arg.Any<PutObjectRequest>(), Arg.Any<CancellationToken>());
        await client.DidNotReceive().GetObjectMetadataAsync(Arg.Any<GetObjectMetadataRequest>(), Arg.Any<CancellationToken>());
        source.CanRead.ShouldBeTrue();
    }

    [Test]
    public void R2ConfigurationIsExplicitAndRequiresHttps()
    {
        var config = R2ClientConfiguration.Create(new("https://r2.example.invalid"));
        config.AuthenticationRegion.ShouldBe("auto");
        config.ForcePathStyle.ShouldBeTrue();
        config.RequestChecksumCalculation.ShouldBe(Amazon.Runtime.RequestChecksumCalculation.WHEN_REQUIRED);
        config.ResponseChecksumValidation.ShouldBe(Amazon.Runtime.ResponseChecksumValidation.WHEN_REQUIRED);
        config.MaxErrorRetry.ShouldBe(2);
        foreach (string endpoint in new[] { "http://r2.example.invalid", "https://user:password@r2.example.invalid", "https://r2.example.invalid/path", "https://r2.example.invalid?key=secret" })
            Should.Throw<ArgumentException>(() => R2ClientConfiguration.Create(new(endpoint))).Message.ShouldNotContain("secret");
        var client = Substitute.For<IAmazonS3>();
        Should.Throw<ValidationException>(() => new S3ObjectStore(client, new() { Bucket = "private-test", ScopePrefix = "../tenant" }));
        Should.Throw<ValidationException>(() => new S3ObjectStore(client, new() { Bucket = "", ScopePrefix = "shared/public" }));
    }

    [Test]
    public async Task ScopedBindingsUseDistinctProviderKeys()
    {
        var client = Substitute.For<IAmazonS3>();
        client.GetObjectMetadataAsync(Arg.Any<GetObjectMetadataRequest>(), Arg.Any<CancellationToken>())
            .Returns(new GetObjectMetadataResponse { ContentLength = 0, HttpStatusCode = HttpStatusCode.OK });
        IObjectReader first = new S3ObjectStore(client, new() { Bucket = "private-test", ScopePrefix = "tenants/123" });
        IObjectReader second = new S3ObjectStore(client, new() { Bucket = "private-test", ScopePrefix = "tenants/456" });
        (await first.HeadAsync(Key))!.Key.ShouldBe(Key);
        (await second.HeadAsync(Key))!.Key.ShouldBe(Key);
        await client.Received(1).GetObjectMetadataAsync(Arg.Is<GetObjectMetadataRequest>(r => r.Key == "tenants/123/sha256/abc123.pdf"), Arg.Any<CancellationToken>());
        await client.Received(1).GetObjectMetadataAsync(Arg.Is<GetObjectMetadataRequest>(r => r.Key == "tenants/456/sha256/abc123.pdf"), Arg.Any<CancellationToken>());
    }

    [Test]
    public void KeyUtf8BudgetIsExact()
    {
        ObjectKey.Parse(new string('漢', 256)).Value.Length.ShouldBe(256);
        Should.Throw<ValidationException>(() => ObjectKey.Parse(new string('漢', 257)));
    }

    [Test]
    public async Task ZeroLengthReadHasNoBytesAndStillOwnsResponse()
    {
        var client = Substitute.For<IAmazonS3>();
        var body = new MemoryStream();
        client.GetObjectAsync(Arg.Any<GetObjectRequest>(), Arg.Any<CancellationToken>()).Returns(ReadResponse(body, 0));
        var read = (await Store(client).OpenReadAsync(Key))!;
        (await read.Content.ReadAsync(new byte[1])).ShouldBe(0);
        read.Content.Length.ShouldBe(0);
        await read.DisposeAsync();
        body.CanRead.ShouldBeFalse();
    }

    [Test]
    [Arguments("create")]
    [Arguments("head")]
    [Arguments("read")]
    public async Task LateCancellationNeverReturnsSuccessAndDisposesRead(string operation)
    {
        var client = Substitute.For<IAmazonS3>();
        using var cancellation = new CancellationTokenSource();
        var body = new MemoryStream([1, 2, 3]);
        client.GetObjectAsync(Arg.Any<GetObjectRequest>(), cancellation.Token).Returns(_ =>
        { cancellation.Cancel(); return Task.FromResult(ReadResponse(body)); });
        client.GetObjectMetadataAsync(Arg.Any<GetObjectMetadataRequest>(), cancellation.Token).Returns(_ =>
        { cancellation.Cancel(); return Task.FromResult(new GetObjectMetadataResponse { HttpStatusCode = HttpStatusCode.OK }); });
        client.PutObjectAsync(Arg.Any<PutObjectRequest>(), cancellation.Token).Returns(_ =>
        { cancellation.Cancel(); return Task.FromResult(new PutObjectResponse { HttpStatusCode = HttpStatusCode.OK }); });
        using var source = new MemoryStream([1, 2, 3]);
        var store = Store(client);
        Func<Task> action = operation switch
        {
            "create" => () => store.CreateAsync(new(Key, 3, "application/pdf"), source, cancellation.Token),
            "head" => () => store.HeadAsync(Key, cancellation.Token),
            _ => () => store.OpenReadAsync(Key, cancellation.Token)
        };
        await Should.ThrowAsync<OperationCanceledException>(action);
        if (operation == "read") body.CanRead.ShouldBeFalse();
        source.CanRead.ShouldBeTrue();
        body.Dispose();
    }

    [Test]
    [Arguments("transport")]
    [Arguments("client")]
    [Arguments("timeout")]
    public async Task TransportAndInternalTimeoutsAreSanitized(string kind)
    {
        var client = Substitute.For<IAmazonS3>();
        Exception failure = kind switch
        {
            "transport" => new HttpRequestException("secret-endpoint"),
            "client" => new Amazon.Runtime.AmazonClientException("secret-endpoint"),
            _ => new TaskCanceledException("secret-endpoint")
        };
        client.GetObjectAsync(Arg.Any<GetObjectRequest>(), Arg.Any<CancellationToken>()).Returns(Task.FromException<GetObjectResponse>(failure));
        client.GetObjectMetadataAsync(Arg.Any<GetObjectMetadataRequest>(), Arg.Any<CancellationToken>()).Returns(Task.FromException<GetObjectMetadataResponse>(failure));
        client.PutObjectAsync(Arg.Any<PutObjectRequest>(), Arg.Any<CancellationToken>()).Returns(Task.FromException<PutObjectResponse>(failure));
        using var source = new MemoryStream([1, 2, 3]);
        var store = Store(client);
        foreach (var action in new Func<Task>[] { () => store.OpenReadAsync(Key), () => store.HeadAsync(Key), () => store.CreateAsync(new(Key, 3, "application/pdf"), source) })
        {
            var error = await Should.ThrowAsync<ObjectStorageException>(action);
            error.Error.ShouldBe(StorageError.Unavailable);
            error.ToString().ShouldNotContain("secret-endpoint");
            error.InnerException.ShouldBeNull();
        }
    }

    [Test]
    public async Task ReadTransportFailureAndCancellationAreSafe()
    {
        var client = Substitute.For<IAmazonS3>();
        var body = Substitute.For<Stream>();
        body.CanRead.Returns(true);
        body.ReadAsync(Arg.Any<Memory<byte>>(), Arg.Any<CancellationToken>())
            .Returns(ValueTask.FromException<int>(new IOException("secret-endpoint")));
        client.GetObjectAsync(Arg.Any<GetObjectRequest>(), Arg.Any<CancellationToken>()).Returns(ReadResponse(body));
        await using var read = (await Store(client).OpenReadAsync(Key))!;
        var error = await Should.ThrowAsync<ObjectStorageException>(() => read.Content.ReadAsync(new byte[3]).AsTask());
        error.Error.ShouldBe(StorageError.Unavailable);
        error.ToString().ShouldNotContain("secret-endpoint");
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        await Should.ThrowAsync<OperationCanceledException>(() => read.Content.ReadAsync(new byte[3], cancellation.Token).AsTask());
        read.Content.Position.ShouldBe(0);
    }
}
