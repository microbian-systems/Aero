# Aero.Documents.Ocr

Opt-in page-level PDF OCR. Native text is read with the existing PdfPig extractor
first. `ImageOnlyOcrPolicy` selects only image-bearing pages without any native
letters or digits. Other pages keep their native text, without starting Tesseract.
An image is evidence of a possible scan, not proof that it contains text.

Inspection, page-selection policy, rendering and recognition are separate small
Strategy interfaces. Providers are explicitly composed; there is no reflection,
automatic discovery, network fallback, tenant routing or paid AI call.

```csharp
using Aero.Documents;
using Aero.Documents.Ocr;
using Aero.Documents.PdfToImage;

var processor = new PdfOcrDocumentProcessor(
    new PdfPigPageInspector(), new ImageOnlyOcrPolicy(),
    new PdfToImagePageRenderer(),
    new TesseractProcessEngine(new LocalProcessRunner(), new TesseractTsvParser()));
var extracted = await processor.ExtractAsync(pdfBytes, new ExtractionLimits(), cancellationToken);
var chunks = new DocumentChunker().Chunk(extracted.Document, cancellationToken: cancellationToken);
// Retain Document.Issues and OcrPages alongside chunks for source review.
```

The renderer is an optional separate package, `Aero.Documents.PdfToImage`.
`DocumentProcessorRegistry.CreateDefault()` is unchanged: ordinary extraction
does not require PDFium, Skia or Tesseract. The host must explicitly choose this
OCR facade and bound buffering before passing a byte array.

## Coverage and evidence

- One-based PDF page anchors survive OCR and chunking.
- OCR words retain top-left pixel boxes and engine confidence scores. Scores are
  not probabilities or accuracy guarantees. Low scores are flagged, not hidden.
- OCR text is always marked `OcrDerived`; amounts, deadlines and pricing require
  source review even when confidence is high.
- Pages with both images and usable native text receive `MixedPageCoverage`:
  image regions are not separately recognized in this first policy. A scanned
  body with a native header can therefore require manual review. Existing OCR
  layers are retained, but their correctness is not automatically verified.
- Blank pages, missing runtime/data, malformed TSV, oversized rasters, exhausted
  page/text/output budgets and failed cleanup produce explicit issues.
- Layout, forms, annotations and table meaning are not reconstructed. Native
  rendering disables form filling/annotations; this is not a visual completeness
  guarantee. There is no OCR for DOCX/XLSX or standalone image registration yet.

Default budgets: 25 OCR pages/document, 300 DPI, 16 million pixels/page, 20 MiB
PNG/page, 45 seconds/Tesseract invocation and four million TSV characters/page.
Oversized rasters are rejected rather than silently downscaled. Overall native
and OCR block text obeys `ExtractionLimits`; bounded OCR evidence is separate and
can still exist when a page's block text exceeds that retention budget.

## Runtime and security

Install a trusted Tesseract executable and English language data on the worker.
The CLI uses `eng`, LSTM (`--oem 1`) and TSV output. Linux is supported. The optional
PDFtoImage adapter supports server/desktop Windows, Linux and macOS; Android/iOS
are not worker targets. Required Linux native dependencies include fontconfig.

Tesseract is launched without a shell using `ArgumentList`, a private temporary
PNG, strict UTF-8 stdout, bounded output, a deadline and `OMP_THREAD_LIMIT=1`.
Stderr is drained but not retained/logged. Caller cancellation remains cancellation;
runtime failures become sanitized issues. Termination of the process tree and
temporary-directory cleanup are best-effort, not an OS security sandbox. A host
scavenger is required for interrupted jobs/failed cleanup; temporary files must
reside on appropriately protected worker storage.

PdfPig inspection and native PDFium/Skia rendering are in-process. A synchronous
native call cannot be forcefully cancelled by this API. **Use a resource-limited,
isolated worker before enabling untrusted automatic ingestion.** The library
does not provide hard CPU/memory limits or isolation. Do not accept executable
paths/CLI flags from tenants. No runtime or language models are downloaded by it.

The `:1` identities describe adapter schemas, not installed native/model versions.
The host must record actual Tesseract/PDFium versions and trained-data hashes in
pipeline provenance/cache keys before shared embedding ingestion is enabled.

See [license inventory](../Aero.Documents.PdfToImage/LICENSES.md) for the approved
MIT-first, not MIT-only, dependency mix.

## Verification commands

From ContractorAI's root, using its installed SDK:

```powershell
dotnet restore Aero/Tests/Aero.Documents.Tests/Aero.Documents.Tests.csproj --locked-mode
dotnet build Aero/Tests/Aero.Documents.Tests/Aero.Documents.Tests.csproj -c Release --no-incremental --no-restore -warnaserror
dotnet run --project Aero/Tests/Aero.Documents.Tests/Aero.Documents.Tests.csproj -c Release --no-build -- --no-progress --output Normal
docker build -f Aero/Tests/Aero.Documents.Ocr.Smoke/Dockerfile -t aero-documents-ocr-smoke:20261010 Aero
docker run --rm --network none --memory 2g --cpus 2 --pids-limit 128 --cap-drop ALL --security-opt no-new-privileges --read-only --tmpfs /tmp:rw,noexec,nosuid,size=128m --user 65532:65532 aero-documents-ocr-smoke:20261010
```

The Docker test generates a native page and a genuinely image-only page, runs
real PDFium rendering and local Tesseract, then verifies recognized text, word
evidence and page-anchored chunks. It sends no document to an external API.
This test image installs distro packages; it is not the production worker image.

Primary references: [Tesseract installation](https://tesseract-ocr.github.io/tessdoc/Installation.html),
[TSV output](https://tesseract-ocr.github.io/tessdoc/Command-Line-Usage.html),
[PDFtoImage](https://github.com/sungaila/PDFtoImage),
[process termination limitations](https://learn.microsoft.com/dotnet/api/system.diagnostics.process.kill?view=net-10.0).
