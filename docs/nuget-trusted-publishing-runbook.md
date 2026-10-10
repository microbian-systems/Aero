# Draft: NuGet Trusted Publishing for AeroCMS and AeroDB

Status: Draft  
Last verified: 2026-08-02

## Purpose

Use NuGet.org Trusted Publishing to publish packages from GitHub Actions without
storing a long-lived NuGet API key. GitHub issues an OIDC token for a specific
repository and workflow. NuGet.org validates that token against a trusted
publishing policy and returns a short-lived API key to the workflow.

This runbook covers:

- `microbian-systems/AeroCMS`
- `microbian-systems/AeroDB`

The working `microbian-systems/Aero` preview workflow is the verified reference.

Official guidance: [Trusted Publishing on nuget.org](https://learn.microsoft.com/nuget/nuget-org/trusted-publishing)

## Important field rules

The following values must match the GitHub OIDC token exactly:

- **Package Owner:** `microbian-systems`
- **CI/CD Provider:** `GitHub Actions`
- **Repository Owner:** `microbian-systems`
- **Repository:** the repository name only, such as `AeroCMS` or `AeroDB`
- **Workflow File:** the filename only, such as `publish-develop.yml`
- **Environment:** blank unless the workflow job declares a GitHub environment

Do not enter a repository URL. For example, use `AeroDB`, not
`https://github.com/microbian-systems/AeroDB`.

Repository owner and repository fields cannot currently be corrected through
the policy edit form. Delete and recreate a policy if either value is wrong.

## Shared GitHub configuration

Create a repository variable named `NUGET_USER`. It is configuration, not a
secret, and should contain the NuGet.org profile that owns the policies:

```powershell
gh variable set NUGET_USER `
  --repo microbian-systems/AeroCMS `
  --body "microbian-systems"

gh variable set NUGET_USER `
  --repo microbian-systems/AeroDB `
  --body "microbian-systems"
```

Verify the values:

```powershell
gh variable get NUGET_USER --repo microbian-systems/AeroCMS
gh variable get NUGET_USER --repo microbian-systems/AeroDB
```

The expected output is `microbian-systems`. Do not store a NuGet API key in
GitHub for these workflows.

Each publishing job needs OIDC permission and must use the repository variable:

```yaml
permissions:
  id-token: write
  contents: read

steps:
  - name: NuGet login (OIDC)
    uses: NuGet/login@v1
    id: login
    with:
      user: ${{ vars.NUGET_USER }}

  - name: Push to NuGet
    shell: pwsh
    run: ./build/nuget-publish.ps1 -ApiKey "${{ steps.login.outputs.NUGET_API_KEY }}"
```

The temporary API key is requested immediately before publishing. It should not
be printed, persisted, or copied into another job.

## AeroDB

### Current repository state

AeroDB already contains these publishing workflows:

| Purpose | Trigger | Workflow file | GitHub environment |
|---|---|---|---|
| Automatic develop preview | Push to `develop` | `publish-develop.yml` | blank |
| Manual preview | `workflow_dispatch` | `publish-preview.yml` | blank |
| Stable release | Push tag matching `v*` | `publish-release.yml` | blank |

The repository variable `NUGET_USER=microbian-systems` already exists.

### NuGet.org policies to create

Create one policy for every workflow that is allowed to publish:

| Suggested policy name | Repository | Workflow file | Environment |
|---|---|---|---|
| `aerodb-develop-preview` | `AeroDB` | `publish-develop.yml` | blank |
| `aerodb-manual-preview` | `AeroDB` | `publish-preview.yml` | blank |
| `aerodb-release` | `AeroDB` | `publish-release.yml` | blank |

All three policies use package owner and repository owner
`microbian-systems`.

Optional hardening: add `environment: release` to the release job and configure
required reviewers in GitHub. If this is done, the NuGet policy must also use
environment `release`. Change the workflow and policy together; a mismatch will
cause OIDC token exchange to fail.

### Existing non-authentication blocker

The latest inspected AeroDB develop run failed during recursive checkout because
the `surrealdb.net` submodule did not contain gitlink commit
`5117062c38186caa21ac302e150b4f0c5fdf34e7`. Trusted Publishing cannot be
validated until checkout and packing reach the `NuGet/login@v1` step. Resolve
that submodule commit separately; it is not a NuGet policy failure.

## AeroCMS

### Current repository state

AeroCMS has working `build/nuget-pack.ps1` and `build/nuget-publish.ps1`
scripts, but the checked-out `develop` branch currently contains no files under
`.github/workflows/`. It also has no `NUGET_USER` repository variable.

Trusted Publishing policies must reference real workflow filenames. Add and
merge the workflows before attempting policy validation.

For consistency with AeroDB, use these proposed filenames:

| Purpose | Trigger | Proposed workflow file | Recommended environment |
|---|---|---|---|
| Automatic develop preview | Push to `develop` | `publish-develop.yml` | blank |
| Manual preview | `workflow_dispatch` | `publish-preview.yml` | blank |
| Stable release | Push tag matching `v*` | `publish-release.yml` | `release` |

The develop workflow should use a unique prerelease suffix:

```yaml
- name: Pack preview packages
  shell: pwsh
  run: ./build/nuget-pack.ps1 -VersionSuffix "alpha.${{ github.run_number }}"
```

This produces versions such as `0.0.9.7-alpha.10`. Reruns retain the same
`github.run_number`, allowing `--skip-duplicate` to recover safely after a
partial publication.

The stable workflow can use the existing tag-based interface:

```yaml
- name: Pack stable packages
  shell: pwsh
  run: ./build/nuget-pack.ps1 -Stable -VersionPrefix "${{ steps.version.outputs.VERSION }}"
```

The current AeroCMS publish script intentionally publishes `Aero.Cms.*`
packages and excludes standalone `Aero.Cms.Modules.*` packages except
`Aero.Cms.Modules.Meta`. Trusted Publishing changes authentication only; it
does not change that package-selection boundary.

### NuGet.org policies to create

After the workflow filenames are committed, create:

| Suggested policy name | Repository | Workflow file | Environment |
|---|---|---|---|
| `aerocms-develop-preview` | `AeroCMS` | `publish-develop.yml` | blank |
| `aerocms-manual-preview` | `AeroCMS` | `publish-preview.yml` | blank |
| `aerocms-release` | `AeroCMS` | `publish-release.yml` | `release` |

All three policies use package owner and repository owner
`microbian-systems`.

If the release workflow is created without `environment: release`, leave the
policy environment blank instead. The workflow and policy must agree.

## Create and activate each NuGet.org policy

1. Sign in to NuGet.org.
2. Open the account menu and select **Trusted Publishing**.
3. Select **Create**.
4. Choose package owner `microbian-systems`.
5. Choose provider **GitHub Actions**.
6. Enter repository owner `microbian-systems`.
7. Enter the repository name only: `AeroCMS` or `AeroDB`.
8. Enter the workflow filename only.
9. Enter the environment only when the workflow job declares that environment.
10. Create the policy.
11. Select **Activate for 7 days** if the policy is pending activation.
12. Run the matching workflow within the seven-day window.

A successful OIDC publication supplies GitHub's repository identifiers and
makes the policy permanently active. An expired policy can be reactivated for
another seven-day window.

## Validation

### Before triggering publication

- Confirm `NUGET_USER` returns `microbian-systems`.
- Confirm the workflow requests `id-token: write`.
- Confirm `NuGet/login@v1` uses `${{ vars.NUGET_USER }}`.
- Confirm repository owner, repository, workflow filename, and environment match
  the saved NuGet.org policy.
- Run the pack script locally or in CI before login.
- Ensure the package version has not already been published.

### Trigger and monitor

For a develop workflow, push or merge a change into `develop`. For a manual
workflow, use GitHub Actions or `gh workflow run`.

```powershell
gh run list --repo microbian-systems/AeroDB --limit 10
gh run list --repo microbian-systems/AeroCMS --limit 10
gh run watch <run-id> --repo microbian-systems/<repository> --exit-status
```

The run is successful only when all of these steps pass:

1. Checkout, including required submodules
2. Pack
3. NuGet OIDC login
4. Package and symbol publication

NuGet.org can accept a package before it appears in the public V3 index. Treat
upload acceptance and index propagation as separate checks.

## Common failures

| Failure | Meaning | Fix |
|---|---|---|
| `Input required and not supplied: user` | `NUGET_USER` is missing or referenced from the wrong context | Set the repository variable and use `${{ vars.NUGET_USER }}` |
| `No matching trust policy ... was found` | The policy is missing, expired, or one of its matching fields differs | Check package owner, repository owner, repository name, workflow filename, environment, activation, and `NUGET_USER` |
| Policy shows expired | The initial seven-day activation window elapsed without a successful run | Reactivate it and run the matching workflow within seven days |
| Repository owner or repository is wrong | These fields cannot be corrected in the current edit form | Delete and recreate the policy |
| Pack succeeds but login fails | OIDC policy or workflow permissions are wrong | Check `id-token: write` and all policy fields |
| Checkout fails before login | Repository or submodule problem, not Trusted Publishing | Repair checkout first, then rerun |
| Package version already exists | NuGet versions are immutable | Increment the version or use a unique preview suffix such as `alpha.${{ github.run_number }}` |

## Completion checklist

### AeroDB

- [ ] Create and activate policy for `publish-develop.yml`
- [ ] Create and activate policy for `publish-preview.yml`
- [ ] Decide whether release uses a protected `release` environment
- [ ] Create and activate policy for `publish-release.yml`
- [ ] Resolve the unavailable `surrealdb.net` gitlink commit
- [ ] Complete one successful develop preview publication
- [ ] Confirm policies become permanently active

### AeroCMS

- [ ] Add `NUGET_USER=microbian-systems` as a repository variable
- [ ] Add publishing workflows with finalized filenames
- [ ] Create the GitHub `release` environment if used
- [ ] Create and activate matching NuGet.org policies
- [ ] Complete one successful develop preview publication
- [ ] Confirm expected package IDs and symbols were accepted
- [ ] Confirm policies become permanently active
