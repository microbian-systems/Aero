# Agent Instructions: Test Remediation

## Submodule ownership and third-party bug fixes

- Verify the original project's ownership before changing a submodule. A fork
  hosted under `microbian-systems` does not make its third-party upstream code
  ours. For example, `surrealdb.net`, Wolverine, and Marten are third-party
  projects even when checked out from our forks.
- Do not modify third-party submodule code for application features,
  convenience, speculative refactoring, or upgrades. Read-only inspection is
  permitted.
- The only exception is an explicitly approved, legitimate upstream bug fix:
  reproduce the defect with a failing regression test, verify we are working
  in a fork of the original repository, make the smallest correction, and
  verify the regression and relevant existing tests pass. Document the issue,
  fix, and evidence, and plan to submit a PR to the original upstream project.
- If ownership, the bug, or fork provenance is uncertain, stop and ask. Do not
  create a fork or submit a PR without the user's explicit authorization.
- Owned submodules also require explicit permission for the specific change.
  Bug-fix approval does not authorize unrelated edits, branch/remote changes,
  checked-out commit or parent submodule-pointer changes, `.gitmodules` edits,
  commits, pushes, publication, or adoption into the application.


## Objective
Fix all failed tests identified in `test-results.txt`.

## Focus
- Only modify unit tests that require updates due to the recent RavenDB modeling/entity refactor (NoSQL style - normalized -> denormalized).
- Ensure test changes align with the "no relationships" architecture only.
- Do not modify core test logic unless it is strictly necessary to fix a regression caused by the refactor that manifests in these tests.
- modify the non-test code as you see fit to get it working in the new denormazlied refactor + Marten idioms

## Diagnostics
- Use `test-results.txt` as the source of truth for failing tests.
- Use `dotnet test --list-tests` to explore available test cases.
- Refer to `test_results.log` for detailed stack traces of past failures if needed.

## Constraints
- Stay within the scope of the unit test failures.
- Modernize test data and assertions to match the flattened NoSQL entity structures.
