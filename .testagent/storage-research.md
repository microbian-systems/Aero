# Bounded storage slice - 2026-10-10

Approved: "use the existing oss amazon s3 library with cloudflare R2 as its S3 compatible";
"create a light wrapper over the amazon s3 library to abstract the details in case we want to switch";
"Create-only originals (recommended)". Retain original bytes; content-hash identity
and private/shared routing remain ContractorAI-owned. No paid APIs or cloud provisioning.

Sources: Aero.Storage.Abstractions/Contracts.cs; Aero.Storage.S3/S3StorageOptions.cs,
S3Errors.cs, S3ObjectStore.cs, OwnedReadStream.cs. No Aero.Core/AeroCMS runtime edits.
No reflection/DI discovery. Interface segregation + Strategy/Adapter; owned-stream
decorator expresses response lifetime and transfer bounds, not a speculative pattern.

Conventions: net10.0, TUnit 1.58.0, Shouldly, NSubstitute, generated discovery,
locked central dependencies. AWSSDK.S3 4.0.104.2 stable resolved from NuGet index;
SDK installed XML and current Cloudflare compatibility docs inspected.

Acceptance: scoped keys on every operation; atomic absent-only PUT; known length
from current seekable-stream position; caller stream stays open; bounded read lease
owns response; exact bytes/length/EOF; null only for absence, safe exceptions for
denied/conflict/invalid/transport; cancellation propagates; R2 HTTPS/region/checksum
settings explicit; no credentials in configs/diagnostics; no overwrite/delete API.
Tests mock the SDK boundary; wire-level test uses an in-process HTTP handler, no
actual URL/network/cloud account. Neither proves live R2 compatibility.

Validation exception messages must not echo bucket/prefix/user input. Head's bare
404 can be ambiguous for an absent bucket; deployment must validate its private
bucket binding first. Original filename is metadata, not the restricted logical key.

Coverage target: exercise nontrivial surface/boundaries; no measured percentage
claimed. Cloud live checks, multipart, non-seekable spooling, host authorization,
tenant integration, digest reconciliation and isolated workers remain outside scope.
