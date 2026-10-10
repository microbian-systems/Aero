using System.Security.Cryptography;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using FluentValidation;

namespace Aero.Storage.S3;

/// <summary>Scoped create-only adapter. Injected SDK client remains host-owned.</summary>
public sealed class S3ObjectStore : IObjectStore
{
    private readonly IAmazonS3 client;
    private readonly S3StorageOptions options;
    /// <summary>Explicit construction; no credential discovery, bucket provisioning or tenant selection.</summary>
    public S3ObjectStore(IAmazonS3 client, S3StorageOptions options)
    {
        this.client = client ?? throw new ArgumentNullException(nameof(client));
        this.options = options ?? throw new ArgumentNullException(nameof(options));
        new S3StorageOptionsValidator().ValidateAndThrow(options);
    }
    private string ProviderKey(ObjectKey key) => options.ScopePrefix + "/" + key.Value;
    /// <inheritdoc />
    public async Task<ObjectCreated> CreateAsync(ObjectUpload upload, Stream source, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(upload);
        ArgumentNullException.ThrowIfNull(source);
        new ObjectUploadValidator(options.MaxObjectBytes).ValidateAndThrow(upload);
        cancellationToken.ThrowIfCancellationRequested();
        if (!source.CanRead || !source.CanSeek)
            throw new NotSupportedException("Object upload requires a readable seekable stream; spool unknown-length sources within host limits first.");
        long start = source.Position;
        if (start < 0 || start > source.Length || source.Length - start != upload.Length)
            throw new ObjectStorageException(StorageError.LengthMismatch);
        var request = new PutObjectRequest
        {
            BucketName = options.Bucket, Key = ProviderKey(upload.Key), InputStream = source,
            ContentType = upload.ContentType, IfNoneMatch = "*", AutoCloseStream = false,
            AutoResetStreamPosition = false, UseChunkEncoding = false
        };
        request.Headers.ContentLength = upload.Length;
        try
        {
            if (options.UseR2Compatibility)
            {
                // R2 does not support this SDK's streaming SigV4/checksum framing.
                // Supply a precomputed SHA-256 header without unsupported streaming trailers.
                byte[] digest;
                try { digest = await SHA256.HashDataAsync(source, cancellationToken); }
                finally { source.Position = start; }
                request.ChecksumSHA256 = Convert.ToBase64String(digest);
                request.DisablePayloadSigning = true;
                request.DisableDefaultChecksumValidation = true;
            }
            var response = await client.PutObjectAsync(request, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            if (response is null || (int)response.HttpStatusCode is < 200 or >= 300)
                throw new ObjectStorageException(StorageError.InvalidResponse);
            return new(upload.Key, upload.Length, response.ETag);
        }
        catch (AmazonS3Exception error) { cancellationToken.ThrowIfCancellationRequested(); throw S3Errors.Map(error); }
        catch (AmazonClientException) { cancellationToken.ThrowIfCancellationRequested(); throw new ObjectStorageException(StorageError.Unavailable); }
        catch (HttpRequestException) { cancellationToken.ThrowIfCancellationRequested(); throw new ObjectStorageException(StorageError.Unavailable); }
        catch (IOException) { cancellationToken.ThrowIfCancellationRequested(); throw new ObjectStorageException(StorageError.Unavailable); }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        { throw new ObjectStorageException(StorageError.Unavailable); }
    }
    /// <inheritdoc />
    public async Task<ObjectRead?> OpenReadAsync(ObjectKey key, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(key);
        cancellationToken.ThrowIfCancellationRequested();
        GetObjectResponse? response = null;
        bool transferred = false;
        try
        {
            response = await client.GetObjectAsync(new GetObjectRequest
                { BucketName = options.Bucket, Key = ProviderKey(key) }, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            if (response is null || response.HttpStatusCode != System.Net.HttpStatusCode.OK || response.ResponseStream is null)
                throw new ObjectStorageException(StorageError.InvalidResponse);
            var metadata = Metadata(key, response.ContentLength, response.Headers.ContentType, response.ETag, response.LastModified);
            var content = new OwnedReadStream(response.ResponseStream, metadata.Length, response.Dispose);
            var read = new ObjectRead(metadata, content);
            transferred = true;
            return read;
        }
        catch (AmazonS3Exception error)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (S3Errors.Missing(error)) return null;
            throw S3Errors.Map(error);
        }
        catch (AmazonClientException) { cancellationToken.ThrowIfCancellationRequested(); throw new ObjectStorageException(StorageError.Unavailable); }
        catch (HttpRequestException) { cancellationToken.ThrowIfCancellationRequested(); throw new ObjectStorageException(StorageError.Unavailable); }
        catch (IOException) { cancellationToken.ThrowIfCancellationRequested(); throw new ObjectStorageException(StorageError.Unavailable); }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        { throw new ObjectStorageException(StorageError.Unavailable); }
        finally { if (!transferred) response?.Dispose(); }
    }
    /// <inheritdoc />
    public async Task<ObjectMetadata?> HeadAsync(ObjectKey key, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(key);
        cancellationToken.ThrowIfCancellationRequested();
        try
        {
            var response = await client.GetObjectMetadataAsync(new GetObjectMetadataRequest
                { BucketName = options.Bucket, Key = ProviderKey(key) }, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            if (response is null || response.HttpStatusCode != System.Net.HttpStatusCode.OK)
                throw new ObjectStorageException(StorageError.InvalidResponse);
            return Metadata(key, response.ContentLength, response.Headers.ContentType, response.ETag, response.LastModified);
        }
        catch (AmazonS3Exception error)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (S3Errors.Missing(error)) return null;
            throw S3Errors.Map(error);
        }
        catch (AmazonClientException) { cancellationToken.ThrowIfCancellationRequested(); throw new ObjectStorageException(StorageError.Unavailable); }
        catch (HttpRequestException) { cancellationToken.ThrowIfCancellationRequested(); throw new ObjectStorageException(StorageError.Unavailable); }
        catch (IOException) { cancellationToken.ThrowIfCancellationRequested(); throw new ObjectStorageException(StorageError.Unavailable); }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        { throw new ObjectStorageException(StorageError.Unavailable); }
    }
    private ObjectMetadata Metadata(ObjectKey key, long length, string? type, string? etag, DateTime? modified)
    {
        if (length < 0) throw new ObjectStorageException(StorageError.InvalidResponse);
        if (length > options.MaxObjectBytes) throw new ObjectStorageException(StorageError.TooLarge);
        DateTimeOffset? utc = null;
        if (modified is { } date)
        {
            DateTime normalized = date.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(date, DateTimeKind.Utc) : date.ToUniversalTime();
            utc = new DateTimeOffset(normalized);
        }
        return new(key, length, type, etag, utc);
    }
}
