# Implementation Plan: Cleanup and Compilation Refactor

## Phase 1: Analysis and Environment Setup
- [x] Task: Identify all compilation errors in the solution.
    - [x] Run `dotnet build` and capture all errors.
    - [x] Categorize errors by project and type (missing references, syntax, etc.).
- [x] Task: Identify obvious "junk" code and unused assets.
    - [x] Search for classes/methods with zero references (using IDE or grep).
    - [x] Identify unused `.csproj` files or obsolete configuration.
- [x] Task: Conductor - User Manual Verification 'Phase 1: Analysis and Environment Setup' (Protocol in workflow.md)

## Phase 2: Resolve Compilation Errors (TDD)
- [x] Task: Fix Core project compilation errors.
    - [x] Write/update tests in `Aero.Core.Tests` (if they exist) to reproduce issues where possible.
    - [x] Apply fixes to `Aero.Core`.
    - [x] Verify `Aero.Core` builds successfully.
- [x] Task: Fix Actors and Auth project compilation errors.
    - [x] Apply fixes to `Aero.Actors` and `Aero.Auth`.
    - [x] Verify both projects build successfully.
- [x] Task: Fix data-related project compilation errors (EfCore, RavenDB, etc.).
    - [x] Apply fixes to `Aero.EfCore`, `Aero.RavenDB`, etc.
    - [x] Verify data projects build successfully.
- [x] Task: Final Solution-wide Build.
    - [x] Run `dotnet build` on `Aero.slnx` and confirm zero errors.
- [x] Task: Conductor - User Manual Verification 'Phase 2: Resolve Compilation Errors (TDD)' (Protocol in workflow.md)

## Phase 3: Code Cleanup and "Junk" Removal
- [x] Task: Remove unused C# code.
    - [x] Safely delete classes/methods identified in Phase 1 (e.g., `MultipleResultSet` helpers).
    - [x] Ensure the solution still builds after each major deletion.
- [x] Task: Cleanup project files and dependencies.
    - [x] Remove unused NuGet packages and broken binary references.
    - [x] Consolidate duplicate dependencies if found.
- [x] Task: Remove obsolete configuration and documentation.
    - [x] Delete `Aero.Graph` directory (unreferenced and unused).
    - [x] Remove temporary backup files (e.g., `.frolic-backup-*`).
    - [x] Delete empty `Aero.Common.Tests` project.
- [x] Task: Conductor - User Manual Verification 'Phase 3: Code Cleanup and "Junk" Removal' (Protocol in workflow.md)

## Phase 4: Final Verification and Stabilization
- [x] Task: Run all automated tests.
    - [x] Execute `dotnet test` and ensure 100% pass rate.
- [x] Task: Verify code style and formatting.
    - [x] Run `dotnet format` or similar tool if available.
- [x] Task: Conductor - User Manual Verification 'Phase 4: Final Verification and Stabilization' (Protocol in workflow.md)
