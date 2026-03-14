# Implementation Plan: Cleanup and Compilation Refactor

## Phase 1: Analysis and Environment Setup
- [ ] Task: Identify all compilation errors in the solution.
    - [ ] Run `dotnet build` and capture all errors.
    - [ ] Categorize errors by project and type (missing references, syntax, etc.).
- [ ] Task: Identify obvious "junk" code and unused assets.
    - [ ] Search for classes/methods with zero references (using IDE or grep).
    - [ ] Identify unused `.csproj` files or obsolete configuration.
- [ ] Task: Conductor - User Manual Verification 'Phase 1: Analysis and Environment Setup' (Protocol in workflow.md)

## Phase 2: Resolve Compilation Errors (TDD)
- [ ] Task: Fix Core project compilation errors.
    - [ ] Write/update tests in `Aero.Core.Tests` (if they exist) to reproduce issues where possible.
    - [ ] Apply fixes to `Aero.Core`.
    - [ ] Verify `Aero.Core` builds successfully.
- [ ] Task: Fix Actors and Auth project compilation errors.
    - [ ] Apply fixes to `Aero.Actors` and `Aero.Auth`.
    - [ ] Verify both projects build successfully.
- [ ] Task: Fix data-related project compilation errors (EfCore, RavenDB, etc.).
    - [ ] Apply fixes to `Aero.EfCore`, `Aero.RavenDB`, etc.
    - [ ] Verify data projects build successfully.
- [ ] Task: Final Solution-wide Build.
    - [ ] Run `dotnet build` on `Aero.slnx` and confirm zero errors.
- [ ] Task: Conductor - User Manual Verification 'Phase 2: Resolve Compilation Errors (TDD)' (Protocol in workflow.md)

## Phase 3: Code Cleanup and "Junk" Removal
- [ ] Task: Remove unused C# code.
    - [ ] Safely delete classes/methods identified in Phase 1.
    - [ ] Ensure the solution still builds after each major deletion.
- [ ] Task: Cleanup project files and dependencies.
    - [ ] Remove unused NuGet packages.
    - [ ] Consolidate duplicate dependencies if found.
- [ ] Task: Remove obsolete configuration and documentation.
    - [ ] Delete `appsettings.Example.json` or similar if truly redundant.
    - [ ] Remove temporary backup files (e.g., `.frolic-backup-*`).
- [ ] Task: Conductor - User Manual Verification 'Phase 3: Code Cleanup and "Junk" Removal' (Protocol in workflow.md)

## Phase 4: Final Verification and Stabilization
- [ ] Task: Run all automated tests.
    - [ ] Execute `dotnet test` and ensure 100% pass rate.
- [ ] Task: Verify code style and formatting.
    - [ ] Run `dotnet format` or similar tool if available.
- [ ] Task: Conductor - User Manual Verification 'Phase 4: Final Verification and Stabilization' (Protocol in workflow.md)
