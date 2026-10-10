# Aero.Documents.PdfToImage

Optional `IPdfPageRenderer` adapter using PDFtoImage 5.4.0 (MIT). See
[OCR composition and worker safeguards](../Aero.Documents.Ocr/README.md) and
[dependency license inventory](LICENSES.md).

Pages are one-based at the Aero boundary and translated to the renderer's
zero-based page index. Requested dimensions, pixel count and encoded PNG bytes
are bounded; native rendering remains synchronous/in-process and must be hosted
inside a resource-limited isolated worker for untrusted attachments.

No network, storage, tenant selection or OCR is performed by this adapter alone.
Native PDFium/Skia dependencies are not all MIT. Windows native rendering has
integration tests; Linux rendering plus genuine Tesseract recognition is exercised
by `Tests/Aero.Documents.Ocr.Smoke`.
