# Bounded scope: Aero.Documents

User requirements: "For PDF - i think there is PDF Pig and  PDFSharp";
"for excel, word and other files lets make use of the official microsoft open xml libraries";
"identify text files by the .txt and chunk those appropriately";
"each file type should have its own processor";
"the excel spreadsheets may need a bit more context as the cols and rows have more meaning".

Implement PDF via PdfPig, DOCX/XLSX via Open XML SDK, TXT via strict decoding,
explicit extension routing, and character-bounded source-anchored chunking.
AI spreadsheet interpretation is not automatic or implemented. Model-specific
token chunking, OCR, legacy Office, PPTX and storage adapters remain separate.

Test conventions: net10.0 executable TUnit 1.58.0, Shouldly 4.3.0; generated
in-memory PDF and Office fixtures, no live network or file storage. All new source
is bounded to Aero/src/Aero.Documents; no Aero.Core, CMS or F# runtime changes.
Static pairing is heuristic, not measured coverage.

## OCR slice (2026-10-10)

Requirements: "lets stsart the implementation", "use SOLID and whatever design
patterns best fit here", "use tesseract only if we detect ocr images". Linux is
required. User approved local Tesseract process and retaining PdfPig while
prioritizing MIT additions. Add role-specific inspector/policy/renderer/engine
contracts, an async facade, bounded process runner and TSV parser in
Aero.Documents.Ocr. Optional MIT PDFtoImage 5.4.0 renderer is a separate adapter
with native BSD PDFium/Skia dependencies. No Poppler/GPL dependency added.

Existing TUnit/Shouldly fixture project remains authoritative. Inject NSubstitute
providers for orchestration and process boundary assertions; use real in-memory
PDF/native-renderer checks separately. No paid AI, database, cloud, default
registry or running application behavior changes. Tesseract not installed on
Windows initially. Linux smoke verification requires a test container/runtime;
unit tests alone do not prove actual OCR.
