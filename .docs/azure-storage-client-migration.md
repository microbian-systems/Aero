# Azure storage client migration

Implemented locally 2026-10-12, with explicit user approval for the migration
and the SDK-specific public helper compatibility break. No commits, branch
changes, package publication, submodule pins, or live Azure operations.

## Dependency and public surface

`Aero.Core` now references the pinned `Azure.Storage.Blobs 12.30.1` instead of
`WindowsAzure.Storage 9.3.3`. Fresh focused restore/assets/runtime dependencies
contain neither WindowsAzure.Storage nor Newtonsoft.Json. This removes the
legacy storage dependency's Newtonsoft.Json 10.0.2 NU1903 finding from this
closure. Other Aero/CMS packages can still require newer Newtonsoft versions;
this is not a whole-application absence claim. Application JSON and the test
dependency reader use System.Text.Json; no replacement JSON package was added.

The constructor, class/interface Post and PostAsync signatures, parameter names
and defaults, and protected ParseContainerPath signature remain unchanged.
Post still uses GetAwaiter().GetResult() to preserve unwrapped exceptions.

**Approved breaking change:** the public extension
`GetLastDirectoryReference(CloudBlobContainer, List<string>) -> CloudBlobDirectory`
cannot survive removal of its defining SDK. It is replaced with
`GetLastDirectoryBlobClient(BlobContainerClient, IReadOnlyList<string>, string)
-> BlobClient`. External consumers of that helper must migrate and recompile;
do not claim full binary/API compatibility. No other in-repository consumer of
the old helper was found. SDK-specific failures now use modern SDK exception
types, so callers catching StorageException must also migrate. Service errors
still propagate instead of being suppressed.

## Preserved historical behavior (not new storage recommendations)

- Create container if absent; tolerate existing container; explicitly reapply
  Blob public access before every upload, including existing containers.
  The ACL call uses no stored identifiers, as before.
- Overwrite existing blobs using BlobUploadOptions without create-only
  conditions; preserve content type (default text/xml).
- `compress=true` only labels the supplied bytes gzip. It does not compress
  them. `compress=false` omits content encoding.
- Consume from the supplied MemoryStream's current position; do not rewind or
  dispose it. Successful upload ends at its length.
- Filename lowercasing is invariant; path lowercasing uses current culture.
  Empty/null container paths default to feeds.
- Path parsing performs one double-slash replacement and trims trailing slash.
  Multi-folder resolution retains the last-directory-only legacy behavior.
- Null filename with forceLowerCase=true still throws NullReferenceException;
  with false it throws ArgumentException. Empty filename fails before parsing
  the connection string.

These quirks deserve separately reviewed corrections. In particular, this
public/overwrite media client is **not** the private create-only ContractorAI
attachment-original storage adapter. No real data or ACL was changed.
Connection strings, SAS query strings, and request authentication headers are
not logged by the replacement client.

Modern SDK wire formatting, retry defaults, and exception details are not
promised byte-for-byte identical to the obsolete SDK. The observed public
upload behavior is the compatibility target.

## Test-first evidence

`Tests/Aero.Core.AzureStorage.Tests` uses TUnit 1.58.0 and a synthetic loopback
HTTP endpoint. The real SDK sends requests to that endpoint; it does not need
Azure credentials, Azurite, or internet service access. This is a bounded local
integration suite, not a live Azure acceptance test.

Receipts under ContractorAI's `artifacts/azure-storage-migration/results/`:

- Initial setup attempt incorrectly assumed v9 HTTP metadata header names;
  corrected to assert equivalent storage metadata across SDK generations.
  That fixture correction is not the dependency regression evidence.
- `legacy-characterization-confirmed.trx`: 10 behavior cases pass; the
  dependency-absence guard fails, proving the old dependencies are present.
- `modern-characterization-green.trx`: unchanged behavior cases and guard
  pass, 11/11, zero skipped.
- `modern-final-green.trx`: 13/13, zero skipped, with additional null-path
  and synchronous Post checks.
- A second locked restore passes. Fresh core rebuild succeeds with existing
  source documentation/nullability warnings (309); there are no changed-file
  warnings or NU1903 in that focused graph. Final incremental build exits 0
  with zero warnings/errors. Do not describe the entire Aero solution as clean.

Representative exact tests: Default_upload_preserves_bytes_gzip_label_and_public_access,
Existing_container_still_sets_acl_and_uploads_without_create_only_condition,
Custom_metadata_case_and_stream_position_are_preserved,
Nested_paths_preserve_legacy_last_directory_only_resolution,
Acl_failure_propagates_and_prevents_upload,
Turkish_culture_preserves_invariant_filename_and_current_culture_path,
Synchronous_Post_preserves_unwrapped_validation_error, and
Core_dependency_graph_does_not_require_legacy_storage_or_newtonsoft.

Verification uses project references to the actual Aero.Core implementation
and isolated SDK artifact/lock paths. No DLL substitution or warning suppression.
The root ContractorAiWolverineBackport opt-in selects per-project locks under
`artifacts/wolverine-backport/locks/`; generation explicitly sets
ContractorAiWolverineGenerateLocks=true, the second restore omits it and has
RestoreLockedMode=true. Retained files include
`D__proj_microbians_ContractorAI_AeroCMS_Aero_Tests_Aero.Core.AzureStorage.Tests_Aero.Core.AzureStorage.Tests.csproj.packages.lock.json`
and `D__proj_microbians_ContractorAI_AeroCMS_Aero_src_Aero.Core_Aero.Core.csproj.packages.lock.json`.
Neither command rewrites tracked upstream locks.
The suite is registered in src/Aero.slnx; unrelated older test-project path
issues are outside this change.

Independent read-only verification: ACCEPTED WITH ADVISORIES. The advisories
are the intentionally preserved public/overwrite behavior, approved helper
API break, need for live Azure acceptance before production, and unrelated
wider application dependency gates. `git diff --check` passes. The 24 tracked
application baseline lock hashes remain unchanged.

## Official SDK references

- [BlobClient.UploadAsync](https://learn.microsoft.com/dotnet/api/azure.storage.blobs.blobclient.uploadasync?view=azure-dotnet)
  documents overwrite semantics for the BlobUploadOptions overload.
- [BlobContainerClient.SetAccessPolicyAsync](https://learn.microsoft.com/dotnet/api/azure.storage.blobs.blobcontainerclient.setaccesspolicyasync?view=azure-dotnet)
  documents Blob public access and explicit ACL application.
