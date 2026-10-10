# Aero.Storage.S3

Small AWS SDK for .NET v4 adapter over `Aero.Storage.Abstractions`. The public
`IObjectReader`/`IObjectWriter` contracts expose only logical keys, metadata,
streams and sanitized errors. AWS requests, buckets, endpoints and credentials
stay in the adapter/host. No Aero.Core or CMS dependency is added.

## Explicit composition

```csharp
using Aero.Storage;
using Aero.Storage.S3;
using Amazon.S3;

// credentials are a host-owned AWSCredentials instance supplied by AeroVault.
// endpoint/bucket/scope come from trusted operator configuration, never a request.
using var sdk = new AmazonS3Client(credentials, R2ClientConfiguration.Create(endpoint));
IObjectStore store = new S3ObjectStore(sdk, new S3StorageOptions
{
    Bucket = bucket,
    ScopePrefix = "shared/public",
    UseR2Compatibility = true
});
var key = ObjectKey.Parse("sha256/" + contentSha256);
var created = await store.CreateAsync(new ObjectUpload(key, originalLength, mediaType),
    originalStream, cancellationToken);
await using var read = await store.OpenReadAsync(key, cancellationToken);
if (read is not null)
    await read.Content.CopyToAsync(destinationStream, cancellationToken);
```

The SDK client stays host-owned; keep it alive for outstanding read leases.
`CreateAsync` consumes the source from its current position and leaves it open.
The readable, seekable source's remaining length must exactly match the descriptor.
R2 hashing/retries can seek within that range; concurrent source mutation is not
supported. Spool nonseekable/unknown-length sources safely in the ingestion worker
before calling this API. The adapter does not buffer entire files in memory.

`OpenReadAsync` returns an owned read-only, nonseekable lease. Dispose the lease
or its Content stream to close the SDK response. Read to EOF to detect short/long
transfers; partial reads do not verify the whole object. Empty objects are allowed.
`HeadAsync` uses no download body. Both reject objects larger than the configured
limit (20 MiB default; operator validation allows 1 byte through 256 MiB).

## Immutability and identity

Every upload uses `If-None-Match: *`. There is no check-then-write, overwrite,
delete, bucket-management or public-link API. `AlreadyExists` is an exception,
not proof that the existing bytes equal this upload. For timeout/connection loss,
the write outcome can be uncertain. The host must reconcile the existing object's
SHA-256 before treating a conflict/retry as successful deduplication; never blindly
overwrite or infer absence from cancellation. Changes get new content-hash keys.

ETag is opaque, not SHA-256 or a reliable content identity. This adapter does not
calculate/store content-addressed identities, provenance or database records.

## Scope and failures

Every provider key is `trusted ScopePrefix + "/" + validated logical key`.
Keys allow letters/digits, slash, dash, underscore and dot, but reject empty, dot
and traversal segments, URLs, encoded escapes, controls and backslashes. The key
budget is 768 UTF-8 bytes plus up to 255 prefix bytes and one separator. Original
filenames belong in host metadata, not raw key construction.

Use separate host bindings for shared public evidence and authorized private
tenant objects. Prefix isolation is **not user authentication**, database namespace
routing or an IAM policy. The host must authorize tenant/owner access and select
the store; clients must never supply prefix, bucket, endpoint or credentials.
Keep buckets private and apply least-privilege credentials/IAM independently.

Object absence returns null. Access denial, write conflicts, bad responses,
oversized objects and transient failures remain distinct sanitized exceptions.
Caller cancellation stays `OperationCanceledException`; internal transport timeout
is `Unavailable`. Raw SDK diagnostics and inner exceptions are not retained or
logged by the adapter. Treat metadata/content as untrusted source data.
Deployment must validate the bucket binding separately: a bare HEAD 404 may be
ambiguous between a missing object and a missing bucket. Explicit `NoSuchBucket`
is treated as configuration failure, never ordinary object absence.

## R2 compatibility profile

`R2ClientConfiguration` requires an operator-approved HTTPS origin, sets signing
region `auto`, path-style access, required-only SDK checksum policies and two SDK
error retries. It never reads credentials, environment variables or global SDK
configuration. Pass credentials explicitly from AeroVault at the composition root.

R2 PUT disables this SDK's streaming SigV4/default checksum framing and chunked
encoding. To retain server-side transfer verification it computes a bounded-memory
Content-MD5 over the remaining stream and restores that starting position before
upload. MD5 is only a transfer checksum, never a security/content-address identity.
The host still computes and verifies SHA-256. Ordinary AWS S3 profile leaves SDK
payload/checksum defaults intact. No global SDK settings are changed.

No multipart, presigned URLs, range reads, nonseekable spooling, automated retries
above the SDK, storage provisioning or live Cloudflare integration is implemented.
The running ContractorAI scanner is unchanged. Real R2 write/read/checksum/conflict
verification must pass in an explicitly selected private test bucket before enabling
the ingestion workflow; the local HTTP-handler test does not prove live R2 access.

## Verification

From ContractorAI root with its installed .NET SDK:

```powershell
dotnet restore Aero/Tests/Aero.Storage.Tests/Aero.Storage.Tests.csproj --locked-mode
dotnet build Aero/Tests/Aero.Storage.Tests/Aero.Storage.Tests.csproj -c Release --no-incremental --no-restore -warnaserror
dotnet run --project Aero/Tests/Aero.Storage.Tests/Aero.Storage.Tests.csproj -c Release --no-build -- --no-progress --output Normal
```

43 TUnit cases cover scoped operations, exact bytes/current position, ownership,
atomic condition, empty/exact/oversize budgets, UTF-8 key boundaries, cancellation,
safe errors and the actual SDK's signed request in a no-network HTTP handler.
This is not measured line coverage or deployed-provider verification.

Pinned dependency: AWSSDK.S3 4.0.104.2 / AWSSDK.Core 4.0.102.9, declared Apache-2.0;
FluentValidation 12.1.1, declared Apache-2.0. Aero's own packages remain MIT.
Retain upstream notices and produce a deployment SBOM before distribution.

Primary references: [Cloudflare .NET SDK example](https://developers.cloudflare.com/r2/examples/aws/aws-sdk-net/),
[R2 S3 compatibility](https://developers.cloudflare.com/r2/api/s3/api/),
[AWS SDK source](https://github.com/aws/aws-sdk-net),
[Stream.CopyToAsync](https://learn.microsoft.com/dotnet/api/system.io.stream.copytoasync?view=net-10.0).
