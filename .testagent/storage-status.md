# Storage verification - 2026-10-10

- Scope: create-only provider-neutral contracts + AWS SDK S3/R2 adapter, no live
  host/client/background worker changes. All edits are in approved Aero develop
  slice and its parent feature docs. No other submodule edits, commits or pushes.
- Static discovery: ran Roslyn once against Aero.Storage.S3 before tests existed;
  four sources, zero tests under that source-only root; all suggested test paths
  null. This is a baseline pairing heuristic, not measured coverage. Tests follow
  existing Aero/Tests convention. No coverage percentage claimed.
- Research/plan implemented inline; prescribed code-testing-generator unavailable.
- Locked restore and nonincremental Release build with warnings as errors passed,
  zero warnings/errors. Final storage suite: 43/43 passed, zero skipped. Existing
  document/OCR regression suite: 58/58 passed, zero skipped. 101 passing cases total
  across two separate runs, not a repository-wide passing test claim.
- Actual AWS SDK request serialization/signing tested through an in-process HTTP
  handler: exact remaining bytes (not preceding prefix byte), Content-MD5,
  known length/type, scope path, If-None-Match and auto signing region. Uses dummy
  test credentials and no network/provider. This does not prove live R2 support.
- Atomic-condition mutation: replaced IfNoneMatch="*" with null; four cases failed
  (39 passed). Immediately reverted, clean Release build +43 cases passed again.
  One mutation empirically killed; not a comprehensive mutation score. No mutation
  remains in final source/binaries/packages.
- Assertion review: 21 declared test methods, 118 source assertion/verification
  calls (including awaited exceptions and callback assertions); no assertion-free
  methods. Equality, Boolean, null/absence, exceptions, strings, collections,
  negative, state/side-effect and structural-record assertions present. Absence-only
  branch intentionally tests missing-object semantics. Parameterized key/length/
  provider-error cases and callback verifications cover actual transformation,
  noncalls, ownership and safe classification, not only round-trip identity.
- Gap review: exact size/UTF-8/current-position bounds, empty objects, transfer
  under/overflow, two prefix bindings, create race header, late cancellation/read
  disposal, transient/denied/missing-bucket failures and R2 checksum configuration
  covered. Gaps remain: live R2/S3 provider behavior, ambiguous bare HEAD 404,
  provider retries/concurrent races, FIPS MD5 runtime configuration, hostile-input
  fuzzing, real AeroVault integration and tenant authorization. No unexecuted
  mutation survivor is reported as empirically verified.
- Both storage NuGet/symbol packages packed locally; nothing published. Apache-2.0
  SDK dependencies locked; own packages inherit Aero MIT metadata.
- Full Aero nonincremental no-restore solution attempt still fails with 24 errors:
  missing unrelated restore assets and preexisting blank Twitter-test TargetFramework.
  No unrelated repairs or SDK pin changes. Commands run from ContractorAI root
  with installed 10.0.401 SDK. Aero stays develop at
  a78b8b9c7182ad4bec629fe2337d496b56999d28.

Acceptance mapping: storage-plan.md. Exact API semantics/limitations and current
primary documentation: src/Aero.Storage.S3/README.md.
