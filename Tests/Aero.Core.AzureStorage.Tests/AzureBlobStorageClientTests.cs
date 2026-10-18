using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging.Abstractions;
using TUnit.Assertions;
using TUnit.Core;

namespace Aero.Core.AzureStorage.Tests;

[Timeout(20_000)]
public sealed class AzureBlobStorageClientTests
{
    private static AzureBlobStorageClient Client() =>
        new(NullLogger<AzureBlobStorageClient>.Instance);

    [Test]
    public async Task Default_upload_preserves_bytes_gzip_label_and_public_access(CancellationToken cancellationToken)
    {
        await using var endpoint = await BlobEndpoint.StartAsync();
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("not compressed"));
        await Client().PostAsync(stream, "BID.XML", endpoint.ConnectionString, "BIDS").WaitAsync(cancellationToken);
        var upload = endpoint.Requests.Single(request => request.Query == "");
        var acl = endpoint.Requests.Single(request => request.Query.Contains("comp=acl"));
        await Assert.That(upload.Path).IsEqualTo("/demotest/bids/bid.xml");
        await Assert.That(Header(upload, "Content-Type", "x-ms-blob-content-type")).IsEqualTo("text/xml");
        await Assert.That(Header(upload, "Content-Encoding", "x-ms-blob-content-encoding")).IsEqualTo("gzip");
        await Assert.That(Encoding.UTF8.GetString(upload.Body)).IsEqualTo("not compressed");
        await Assert.That(acl.Headers["x-ms-blob-public-access"]).IsEqualTo("blob");
        await Assert.That(upload.Headers.ContainsKey("If-None-Match")).IsFalse();
        await Assert.That(stream.CanRead).IsTrue();
        await Assert.That(stream.Position).IsEqualTo(stream.Length);
        await Assert.That(endpoint.Requests.Select(request => request.Query)
            .SequenceEqual(new[] { "?restype=container", "?restype=container&comp=acl", "" })).IsTrue();
    }

    [Test]
    public async Task Existing_container_still_sets_acl_and_uploads_without_create_only_condition(CancellationToken cancellationToken)
    {
        await using var endpoint = await BlobEndpoint.StartAsync();
        endpoint.ContainerExists = true;
        using var stream = new MemoryStream([1, 2, 3]);
        await Client().PostAsync(stream, "bid.txt", endpoint.ConnectionString, "bids", false).WaitAsync(cancellationToken);
        var upload = endpoint.Requests.Single(request => request.Query == "");
        await Assert.That(upload.Body.SequenceEqual(new byte[] { 1, 2, 3 })).IsTrue();
        await Assert.That(upload.Headers.ContainsKey("If-None-Match")).IsFalse();
        await Assert.That(endpoint.Requests.Count(request => request.Query.Contains("comp=acl"))).IsEqualTo(1);
        await Assert.That(endpoint.Requests.ToArray()[0].Query).IsEqualTo("?restype=container");
        await Assert.That(endpoint.Requests.ToArray()[1].Query.Contains("comp=acl")).IsTrue();
        await Assert.That(endpoint.Requests.ToArray()[2].Query).IsEqualTo("");
    }

    [Test]
    public async Task Custom_metadata_case_and_stream_position_are_preserved(CancellationToken cancellationToken)
    {
        await using var endpoint = await BlobEndpoint.StartAsync();
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("skipPAYLOAD"));
        stream.Position = 4;
        await Client().PostAsync(stream, "Bid.PDF", endpoint.ConnectionString, "BIDS", false, "application/pdf", false).WaitAsync(cancellationToken);
        var upload = endpoint.Requests.Single(request => request.Query == "");
        await Assert.That(upload.Path).IsEqualTo("/demotest/bids/Bid.PDF");
        await Assert.That(Header(upload, "Content-Type", "x-ms-blob-content-type")).IsEqualTo("application/pdf");
        await Assert.That(Header(upload, "Content-Encoding", "x-ms-blob-content-encoding")).IsNull();
        await Assert.That(Encoding.UTF8.GetString(upload.Body)).IsEqualTo("PAYLOAD");
        await Assert.That(stream.CanRead).IsTrue();
        await Assert.That(stream.Position).IsEqualTo(stream.Length);
    }

    [Test]
    [Arguments("")]
    [Arguments(null)]
    public async Task Empty_container_defaults_to_feeds(string? path, CancellationToken cancellationToken)
    {
        await using var endpoint = await BlobEndpoint.StartAsync();
        using var stream = new MemoryStream([1]);
        await Client().PostAsync(stream, "bid.txt", endpoint.ConnectionString, path!, false).WaitAsync(cancellationToken);
        await Assert.That(endpoint.Requests.Single(request => request.Query == "").Path)
            .IsEqualTo("/demotest/feeds/bid.txt");
    }

    [Test]
    public async Task Nested_paths_preserve_legacy_last_directory_only_resolution(CancellationToken cancellationToken)
    {
        await using var endpoint = await BlobEndpoint.StartAsync();
        using var stream = new MemoryStream([1]);
        await Client().PostAsync(stream, "bid.txt", endpoint.ConnectionString, "BIDS//FIRST/LAST/", false).WaitAsync(cancellationToken);
        await Assert.That(endpoint.Requests.Single(request => request.Query == "").Path)
            .IsEqualTo("/demotest/bids/last/bid.txt");
    }

    [Test]
    public async Task Acl_failure_propagates_and_prevents_upload(CancellationToken cancellationToken)
    {
        await using var endpoint = await BlobEndpoint.StartAsync();
        endpoint.RejectAcl = true;
        using var stream = new MemoryStream([1]);
        Exception? failure = null;
        try { await Client().PostAsync(stream, "bid.txt", endpoint.ConnectionString, "bids").WaitAsync(cancellationToken); }
        catch (Exception exception) { failure = exception; }
        await Assert.That(failure).IsNotNull();
        await Assert.That(failure is OperationCanceledException or TimeoutException).IsFalse();
        await Assert.That(endpoint.Requests.Any(request => request.Query.Contains("comp=acl"))).IsTrue();
        await Assert.That(endpoint.Requests.Count(request => request.Query == "")).IsEqualTo(0);
        await Assert.That(stream.CanRead).IsTrue();
    }

    [Test]
    public async Task Empty_filename_fails_before_parsing_connection_string(CancellationToken cancellationToken)
    {
        Exception? failure = null;
        using var stream = new MemoryStream();
        try { await Client().PostAsync(stream, "", "invalid", "bids").WaitAsync(cancellationToken); }
        catch (Exception exception) { failure = exception; }
        await Assert.That(failure is ArgumentException).IsTrue();
        await Assert.That(failure!.Message.Contains("filename")).IsTrue();
    }

    [Test]
    public async Task Core_dependency_graph_does_not_require_legacy_storage_or_newtonsoft(CancellationToken cancellationToken)
    {
        using var deps = JsonDocument.Parse(await File.ReadAllTextAsync(
            Path.Combine(AppContext.BaseDirectory, "Aero.Core.AzureStorage.Tests.deps.json"), cancellationToken));
        var libraries = deps.RootElement.GetProperty("libraries").EnumerateObject()
            .Select(library => library.Name).ToArray();
        await Assert.That(libraries.Any(name => name.StartsWith("WindowsAzure.Storage/"))).IsFalse();
        await Assert.That(libraries.Any(name => name.StartsWith("Newtonsoft.Json/"))).IsFalse();
    }

    private static string? Header(BlobRequest request, string standard, string storage) =>
        request.Headers.GetValueOrDefault(storage) ?? request.Headers.GetValueOrDefault(standard);

    [Test]
    public async Task Synchronous_Post_preserves_unwrapped_validation_error(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        using var stream = new MemoryStream();
        Exception? failure = null;
        try { Client().Post(stream, ""); }
        catch (Exception exception) { failure = exception; }
        await Assert.That(failure is ArgumentException).IsTrue();
        await Assert.That(stream.CanRead).IsTrue();
    }

    [Test]
    public async Task Turkish_culture_preserves_invariant_filename_and_current_culture_path(CancellationToken cancellationToken)
    {
        var originalCulture = System.Globalization.CultureInfo.CurrentCulture;
        try
        {
            System.Globalization.CultureInfo.CurrentCulture = System.Globalization.CultureInfo.GetCultureInfo("tr-TR");
            await using var endpoint = await BlobEndpoint.StartAsync();
            using var stream = new MemoryStream([1]);
            await Client().PostAsync(stream, "BID.XML", endpoint.ConnectionString, "BIDS", false).WaitAsync(cancellationToken);
            await Assert.That(endpoint.Requests.Single(request => request.Query == "").Path)
                .IsEqualTo("/demotest/bıds/bid.xml");
        }
        finally { System.Globalization.CultureInfo.CurrentCulture = originalCulture; }
    }

    [Test]
    [Arguments(true)]
    [Arguments(false)]
    public async Task Null_filename_preserves_existing_validation_behavior(bool lowerCase, CancellationToken cancellationToken)
    {
        using var stream = new MemoryStream();
        Exception? failure = null;
        try { await Client().PostAsync(stream, null!, "invalid", "bids", forceLowerCase: lowerCase).WaitAsync(cancellationToken); }
        catch (Exception exception) { failure = exception; }
        await Assert.That(lowerCase ? failure is NullReferenceException : failure is ArgumentException).IsTrue();
    }
}
