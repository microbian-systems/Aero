# Refresh-token Sable test mapping

`Aero.Auth.Sable.Tests` is a focused, database-free characterization suite for
`RefreshTokenService`. It substitutes the production `AeroDB.Sable`
`IDocumentSession` and `ISableQueryable<RefreshToken>` interfaces directly;
the project has no direct Marten or Npgsql reference and uses no WebHost or
database fixture. Transitive dependencies remain outside this test boundary.

| Behavior | Focused test coverage |
| --- | --- |
| Plaintext protection and request context | Generated and rotated tokens are submitted to `Store` only as SHA-256 hashes with user, client, IP, and user-agent context. |
| Lifetime configuration | Configured seven-day and default thirty-day expiry behavior. |
| Validation | Null/empty, missing, hash mismatch, revoked, expired, and active records; active query cancellation is forwarded. |
| Rotation | Missing-token rejection plus replacement linkage, a distinct new token, hashed `Store` submission, and cancellation forwarding. |
| Revocation | Resolved token mutation and `SaveChangesAsync` boundary, null rejection, missing-token no-save behavior, and cancellation forwarding. |
| Boundary failures | Query exceptions propagate without being transformed. |

The legacy `Aero.Auth.Tests/Services/RefreshTokenServiceContractTests.cs` was
replaced after an approved compile-only probe captured its Marten-to-Sable
constructor incompatibility at eight unchanged call sites. Independent review
accepted that narrow RED evidence. The old source remains recoverable in Git.
The obsolete WebHost import was removed, but broader legacy compilation still
stops on unrelated references in `DefaultApiKeyFactoryTests` and
`MartenJwtSigningKeyPersistenceTests`; those files are unchanged.
This focused suite does not verify database tracking, database
persistence, transaction semantics, or refresh-token one-time-use security.
