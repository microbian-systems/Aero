# Aero.Documents

Reusable, read-only text extraction and source-anchored chunking. This package
does not download URLs, access object storage, select tenants, run embedding
models or call paid AI services. Processor registration is explicit, not reflective.

## First-release format support

| Extension | Processor | Retained source context |
| --- | --- | --- |
| `.pdf` | PdfPig 0.1.16 | Text per page; one-based page anchors |
| `.docx` | Microsoft Open XML SDK 3.5.1 | Main-body paragraphs, heading context and table rows |
| `.xlsx` | Microsoft Open XML SDK 3.5.1 | Worksheet/row/cell anchors, raw values, types, styles, formulas and cached results |
| `.txt` | Strict UTF decoder | Paragraph blocks and line-range anchors |

Excel blocks carry the initial nonempty row and preceding row as bounded context,
explicitly labelled as source content rather than inferred column headers. Formula
results are **not recalculated**; cached results may be missing or stale. Error
values such as `#REF!` remain visible. Numeric date serials retain style metadata;
this is not an Excel rendering engine.

PDF layout/images/forms, Word headers/footnotes/drawings/tracked-change semantics,
and Excel merged-cell/visual/external-link semantics are not comprehensively
interpreted. Those coverage limits are returned as structured issues, even when
useful text was extracted. OCR, legacy binary `.doc`/`.xls`, PowerPoint, CSV and
macro-bearing packages are not supported by the default registry. No macro or
external link is executed. PDFSharp is not a dependency of this extraction slice.

Opt-in PDF OCR is available separately in [Aero.Documents.Ocr](../Aero.Documents.Ocr/README.md).
It preserves the text-first default and requires an explicitly composed renderer
and a trusted local Tesseract runtime. It is not enabled by this registry.

TXT supports strict UTF-8 plus BOM-signalled UTF-16/UTF-32. Invalid encodings and
binary control characters are rejected; there is no lossy replacement fallback.

## Usage

```csharp
using Aero.Documents;

var result = await DocumentProcessorRegistry.CreateDefault().ExtractAsync(
    "pricing.xlsx", inputStream, new ExtractionLimits(), cancellationToken);

// Store/inspect result.Issues alongside the extracted blocks, including on failure.
var chunks = new DocumentChunker().Chunk(result,
    new ChunkingOptions { MaxCharacters = 1500, MaxContextCharacters = 300 },
    cancellationToken);
```

The registry consumes the caller's stream from its current position and leaves it
open. Non-seekable streams are supported. Input is buffered within the configured
byte limit. Invalid configuration raises FluentValidation exceptions; cancellation
propagates. Parser failures return sanitized structured issues, not raw exceptions.
I/O failures are not disguised as successful extraction.

Chunks retain block/location identity and UTF-16 source offsets. `Content` and
bounded repeated `Context` are separate fields; their combined character count
does not exceed `MaxCharacters`. Context can be shortened, so retain original
blocks for detailed source inspection. Assemble and label context at the embedding
boundary and enforce the selected model's **token** budget there. Character limits
are not token limits. Never silently truncate through a tokenizer.

## Resource/security boundary

Input bytes, advertised Office ZIP expansion/entry counts, pages/paragraphs/rows,
workbook cells and retained text are bounded. Limits or unextractable content
produce coverage issues rather than silent completion. `IsComplete` means only
that no issues were reported; it is not a semantic or visual fidelity guarantee.

Parser libraries can allocate while reading a document and cannot necessarily
observe cancellation during an internal operation. ZIP metadata checks are not
hard memory limits. Run untrusted attachments in a resource-limited worker with
timeouts and isolation before enabling automatic public ingestion. The host owns
authorization, MIME/extension verification, original-byte hashing, provenance,
storage access, durable retries and tenant/public-catalog separation.

## Verification

From the enclosing ContractorAI checkout (its SDK is installed locally):

```powershell
dotnet restore Aero/Tests/Aero.Documents.Tests/Aero.Documents.Tests.csproj --locked-mode
dotnet build Aero/Tests/Aero.Documents.Tests/Aero.Documents.Tests.csproj -c Release --no-incremental -warnaserror
dotnet run --project Aero/Tests/Aero.Documents.Tests/Aero.Documents.Tests.csproj -c Release --no-build -- --no-progress --output Normal
```

The standalone Aero checkout currently pins SDK 10.0.301 in `global.json`; install
that SDK when running from within that checkout. This feature does not change the
existing SDK pin. Dependency lockfiles are included for this package and its tests.

30 TUnit cases use real in-memory PDF/Open XML fixtures. They cover processor
selection, context/anchors, decoding, limits, cancellation, stream ownership,
spreadsheet errors and lossless bounded chunk splitting. This is not measured
line coverage or verification against every real-world document variation.

Primary references: [PdfPig](https://github.com/UglyToad/PdfPig),
[Open XML SDK](https://github.com/dotnet/Open-XML-SDK),
[cell values/shared strings](https://learn.microsoft.com/office/open-xml/spreadsheet/how-to-retrieve-the-values-of-cells-in-a-spreadsheet).
