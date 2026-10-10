# Aero.Documents verification - 2026-10-10

Implemented seven source files, a narrow net10.0 package and generated-fixture
TUnit test project. No application runtime, storage, model or paid-service changes.

- Static source/test pairing baseline: tool ran against the source folder before
  tests were written; seven sources, zero tests at that point. This was discovery,
  not measured coverage. No line/branch coverage threshold was requested or measured.
- Debug: 30 cases passed, zero failed/skipped.
- Release: nonincremental build with `-warnaserror` passed, zero warnings/errors;
  30 cases passed, zero failed/skipped. Locked dependency restore passed.
- Release NuGet packing succeeded locally (package and symbols); nothing published.
- Actual mutation check: temporarily changed registry input comparison from `>`
  to `>=`; InputLimitIsExact(3, True) failed (29 passed/1 failed). Reverted that
  sole mutation and reran clean Release tests. One tested mutation killed; this
  is not a comprehensive mutation score.
- Assertion review: tests assert values, locations, context, issue codes, exception
  propagation, stream ownership and bounds. No assertion-free cases. Chunk round-trip
  checks are accompanied by independent count/size/location/offset assertions.
  TUnit uses source-generated discovery; fixtures use actual parsers, not mocked
  parser outcomes. Async exception assertions are awaited.
- Gap review: shared validation/input/cancellation behavior tested for all four
  processors; macros, ZIP entry/expansion limits, cached/missing formula results,
  shared strings and exact surrogate boundaries covered. Real-world PDFs, scanned
  documents/OCR, hostile-input fuzzing, all Word parts/Excel visual semantics and
  hard worker resource limits remain outside this slice. No live R2/provider checks.
- Full Aero solution no-restore build attempted: blocked by missing restore assets
  for unrelated projects and an existing blank TargetFramework in Aero.Social.Twitter
  tests. Left unrelated code and SDK 10.0.301 pin unchanged. Package commands were
  run from the enclosing ContractorAI root using installed SDK 10.0.401.

Required user-feature acceptance mapping is in plan.md; API/format limitations are
in src/Aero.Documents/README.md. No commits or deployments made.

## Opt-in PDF OCR verification - 2026-10-10

- Added separate page inspector/policy/renderer/recognizer Strategy interfaces,
  local Tesseract TSV/process adapters and optional PDFtoImage renderer package.
  Default extraction registry and running ContractorAI scanner are unchanged.
- Test research/plan completed inline because the skill's prescribed generator
  subagent was not available. No substitute agent was spawned.
- Static source/test pairing discovery ran before OCR tests existed (six sources,
  zero tests under the selected source-only root at that moment). It is not a
  coverage metric; no line/branch coverage threshold was requested or measured.
- Final Windows Release: locked restore passed; nonincremental warnings-as-errors
  build passed with zero warnings/errors; all 58 TUnit cases passed, zero skipped.
  This includes 30 original extraction cases and 28 OCR/process cases.
- Actual Windows PDFium/Skia rendering verified page numbering, PNG geometry and
  byte limits. Actual owned child processes verified argument integrity/UTF-8,
  bounded output, timeout termination, running cancellation and malformed-output
  termination. These are separate from mocked Tesseract-adapter boundary tests.
- A real temporary mutation inverted the native-text condition in ImageOnlyOcrPolicy:
  6 of 58 cases failed, including the explicit policy assertions. Mutation reverted;
  clean build and 58-case run passed again. One tested mutation killed, not a
  comprehensive mutation score. No mutant remains in source or final binaries.
- Assertion review: expected text, source anchors, engine evidence, boxes/confidence,
  issue codes, budget edges, explicit collaborator calls/noncalls, CLI arguments,
  owned-file cleanup, cancellation and actual child termination asserted. No
  assertion-free cases. Async exception assertions awaited; native behavior and
  process behavior have integration checks rather than only mocked outcomes.
- Gap review: synthetic English scan recognition is covered by the Linux smoke
  fixture; realistic low-quality scans, rotated/multilingual documents, mixed
  scanned regions, OCR tables/handwriting, hostile fuzzing and production hard
  isolation remain unverified. Confidence is not an accuracy guarantee. The CLI
  adapter does not itself impose OS memory/CPU limits or guarantee descendant cleanup.
- Both new library NuGet/symbol packages packed locally; nothing published.
- Full Aero no-restore solution build attempted again: 24 errors from missing
  unrelated restore assets and existing blank Twitter-test TargetFramework.
  No unrelated repairs made. Aero remains develop at a78b8b9c7182ad4bec629fe2337d496b56999d28;
  nested AeroCMS/Aero remains at the same unchanged SHA; SDK pin unchanged.
- Final-source Linux smoke: locked restore and warnings-as-errors build inside the
  pinned .NET SDK Ubuntu image passed. Genuine PDFium rendering and Tesseract 5.3.4
  recognition passed on Ubuntu 24.04.5 LTS: 2 pages, 1 OCR page, 2 recognized words,
  source-anchored chunks. Ran non-root (65532), read-only root filesystem, no network,
  dropped capabilities, no-new-privileges, 2 GiB RAM/2 CPU/128 PID limits, bounded
  temporary filesystem. Synthetic fixture only; not a throughput/accuracy benchmark.
  Image tag aero-documents-ocr-smoke:20261010, manifest list
  sha256:67c2bfa69ef998b2b8266d212f811a4d118b5d07ceb61db712365b979e9c1036.
  Windows host does not have Tesseract installed; no global runtime installation
  was performed. No real contract attachment, secret or database record was used.
