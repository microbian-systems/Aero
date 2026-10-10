# Storage acceptance mapping

- "Create-only originals (recommended)": CreateOnlyUsesCurrentPositionWithoutClosingCaller;
  AlreadyExistsIsAConflictNotSuccessfulDeduplication; WireRequestUsesScopedConditionalPut.
- "create a light wrapper over the amazon s3 library to abstract the details in case we want to switch":
  Contracts.cs hides Amazon/buckets/credentials; HeadAndReadShareScopeAndMetadata;
  ReadLeaseOwnsResponseAndPreservesBytes; separate two packages/provider roles.
- "use the existing oss amazon s3 library with cloudflare R2 as its S3 compatible":
  R2ConfigurationIsExplicitAndRequiresHttps; R2PutPreservesTransferChecksum;
  actual SDK wire-handler assertion. Live R2 explicitly unverified.
- Safety: UploadValidationPrecedesSdkCalls; KeysRejectTraversalAndUrls;
  ReadLengthMismatchIsExplicit; InvalidReadResponseIsDisposed;
  MissingDeniedAndMissingBucketAreDifferent; ProviderFailuresAreSanitized;
  CallerCancellationIsNotStorageFailure; zero-byte/exact-boundary cases.

Research -> plan -> implementation inline; prescribed generator agent unavailable.
Final: locked Release restore/build/tests, assertion/gap review, deliberate atomic
condition mutation and restore, earlier document suite regression, full Aero build
one bounded attempt; no unrelated repairs.
