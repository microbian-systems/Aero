# PDF/OCR dependency inventory (2026-10-10)

The user approved keeping PdfPig and prioritizing MIT-licensed additions,
not an MIT-only runtime. Lockfiles pin the NuGet dependency graph.

| Component | Version / selection | Declared upstream license |
| --- | --- | --- |
| PdfPig | 0.1.16, existing text inspection/extraction | Apache-2.0 |
| PDFtoImage | 5.4.0, optional .NET renderer wrapper | MIT |
| SkiaSharp | 4.150.1, transitive managed graphics wrapper | MIT |
| bblanchon.PDFium native packages | 152.0.7961, transitive renderer distribution | NuGet packaging declares Apache-2.0; PDFium itself is BSD-3-Clause with third-party notices |
| Native Skia | supplied by SkiaSharp native assets | BSD-style; bundled third-party notices apply |
| Tesseract / English trained data | installed by worker operator, not NuGet | Apache-2.0; inspect actual installed package/data notices |

PDFtoImage's MIT license does **not** relicense its native dependencies. This
inventory is not a complete SBOM, redistribution notice bundle or legal review.
Retain all required upstream/native notices and verify the deployed artifact,
including distro/native libraries and language-data files, before distribution.
No Poppler/GPL renderer was added. No PDFSharp dependency was needed.

Sources: [PdfPig](https://github.com/UglyToad/PdfPig),
[PDFtoImage](https://github.com/sungaila/PDFtoImage),
[SkiaSharp](https://github.com/mono/SkiaSharp),
[PDFium binaries](https://github.com/bblanchon/pdfium-binaries),
[Tesseract](https://github.com/tesseract-ocr/tesseract).
