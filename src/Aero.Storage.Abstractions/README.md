# Aero.Storage.Abstractions

Provider-neutral create-only object storage: small `IObjectWriter`, `IObjectReader`
and combined `IObjectStore` roles. Own key/upload/metadata/read-lease/error types,
FluentValidation boundary validation, cancellation and explicit stream ownership.
No AWS types, credentials, tenant resolution, database persistence or network code.

See [S3 adapter behavior/usage](../Aero.Storage.S3/README.md). Every future adapter
must satisfy the same contract cases: current-position exact bytes, leave upload
source open, atomic absent-only create, owned read lifetime, byte/length bounds,
missing-vs-denied failures, cancellation and trusted scoped routing. Do not add
an overwrite-capable implementation under this create-only interface.

The application must convert exceptions/nullable absence at its explicit F#
boundary into native `Result`/`Option`; these contracts do not depend on Aero.Core
railway types or introduce them into business logic.
