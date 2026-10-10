using System.Net;
using System.Security.Cryptography;
using Aero.Storage;
using Aero.Storage.S3;
using Amazon.Runtime;
using Amazon.S3;
using Shouldly;
using TUnit.Core;

namespace Aero.Storage.Tests;

public sealed class WireProtocolTests
{
    [Test]
    public async Task WireRequestUsesScopedConditionalPut()
    {
        using var handler = new CaptureHandler();
        var config = R2ClientConfiguration.Create(new("https://r2.example.invalid"));
        config.HttpClientFactory = new LocalHttpFactory(handler);
        using var client = new AmazonS3Client(new BasicAWSCredentials("test-only-access", "test-only-secret"), config);
        IObjectStore store = new S3ObjectStore(client, new() { Bucket = "private-test", ScopePrefix = "tenants/123", UseR2Compatibility = true });
        using var input = new MemoryStream([99, 1, 2, 3]);
        input.Position = 1;
        var created = await store.CreateAsync(new(ObjectKey.Parse("sha256/abc123.pdf"), 3, "application/pdf"), input);
        handler.Method.ShouldBe(HttpMethod.Put);
        handler.Path.ShouldBe("/private-test/tenants/123/sha256/abc123.pdf");
        handler.Condition.ShouldBe("*");
        handler.Payload.ShouldBe(new byte[] { 1, 2, 3 });
        handler.ContentMd5.ShouldBe(MD5.HashData(new byte[] { 1, 2, 3 }));
        handler.Length.ShouldBe(3);
        handler.ContentType.ShouldBe("application/pdf");
        handler.Signature.ShouldContain("/auto/s3/aws4_request");
        handler.RequestCount.ShouldBe(1);
        created.ETag.ShouldBe("\"opaque-etag\"");
        input.CanRead.ShouldBeTrue();
        input.Position.ShouldBe(4);
    }

    private sealed class LocalHttpFactory(HttpMessageHandler handler) : Amazon.Runtime.HttpClientFactory
    {
        public override HttpClient CreateHttpClient(IClientConfig clientConfig) => new(handler, disposeHandler: false);
        public override bool UseSDKHttpClientCaching(IClientConfig clientConfig) => false;
        public override bool DisposeHttpClientsAfterUse(IClientConfig clientConfig) => true;
    }
    private sealed class CaptureHandler : HttpMessageHandler
    {
        public HttpMethod? Method { get; private set; }
        public string? Path { get; private set; }
        public string? Condition { get; private set; }
        public byte[]? Payload { get; private set; }
        public byte[]? ContentMd5 { get; private set; }
        public long? Length { get; private set; }
        public string? ContentType { get; private set; }
        public string Signature { get; private set; } = "";
        public int RequestCount { get; private set; }
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            RequestCount++;
            Method = request.Method;
            Path = request.RequestUri!.AbsolutePath;
            Condition = request.Headers.GetValues("If-None-Match").Single();
            Payload = await request.Content!.ReadAsByteArrayAsync(cancellationToken);
            ContentMd5 = request.Content.Headers.ContentMD5;
            Length = request.Content.Headers.ContentLength;
            ContentType = request.Content.Headers.ContentType?.ToString();
            Signature = request.Headers.GetValues("Authorization").Single();
            var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("") };
            response.Headers.TryAddWithoutValidation("ETag", "\"opaque-etag\"");
            return response;
        }
    }
}
