# Implementation Plan: Solution Build Stabilization (Aero.Cms*)

## Phase 1: Analysis and Discovery
- [ ] Task: Identify all `Aero.Cms*` projects currently on disk and verify their inclusion in `Aero.slnx`.
- [ ] Task: Run a full solution build and capture all error logs to identify missing dependencies and broken paths.
- [ ] Task: Map out the dependency tree for the new projects to identify circular references or missing core projects.
- [ ] Task: Conductor - User Manual Verification 'Phase 1: Analysis and Discovery' (Protocol in workflow.md)

## Phase 2: Solution and Project Configuration
- [ ] Task: Add missing projects and test suites to `Aero.slnx`.
- [ ] Task: Correct project-to-project references in `.csproj` files to use relative paths consistent with the current workspace.
- [ ] Task: Verify that all projects target the correct framework (.NET 10.0+).
- [ ] Task: Conductor - User Manual Verification 'Phase 2: Solution and Project Configuration' (Protocol in workflow.md)

## Phase 3: Resolution of Compilation Errors
- [ ] Task: Fix syntax errors and missing namespace references in implementation projects.
- [ ] Task: Resolve NuGet package version conflicts between new and existing projects.
- [ ] Task: Ensure that all generated assets or scoped files are correctly located.
- [ ] Task: Conductor - User Manual Verification 'Phase 3: Resolution of Compilation Errors' (Protocol in workflow.md)

## Phase 4: Test Verification and Warning Cleanup
- [ ] Task: Execute the full test suite and fix any tests failing due to the merge.
- [ ] Task: Review and address build warnings to ensure a "clean" build environment.
- [ ] Task: Verify that `Aero.MerakiUI` components within the CMS are correctly rendered and tested.
- [ ] Task: Conductor - User Manual Verification 'Phase 4: Test Verification and Warning Cleanup' (Protocol in workflow.md)
