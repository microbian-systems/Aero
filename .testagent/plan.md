# Processor acceptance/test mapping

- PdfPig: PdfTextHasPageAnchor, PdfPageLimitIsReported, PdfEmptyPageIsNotComplete.
- Open XML Word: WordParagraphsAndTableRowsHaveAnchors.
- Open XML Excel/context: SpreadsheetKeepsRowsCoordinatesFormulasAndContext.
- TXT routing/decoding/chunking: TextRoutingIsCaseInsensitiveAndStreamStaysOpen,
  TextEncodingsAreDecoded, ChunksKeepAllTextAndAnchors.
- Separate processors: RegistrySelectsOneProcessorPerSupportedExtension.
- Boundaries: UnsupportedAndMismatchedFormatsAreExplicit, InputLimitIsExact,
  OfficeExpansionLimitIsEnforced, CancellationPropagates, InvalidLimitsAreRejected,
  InvalidUtf8IsNotReplacedSilently, SpreadsheetCellLimitIsReported,
  MacroPayloadsAndEntryLimitsAreRejected, NonSeekableStreamsAreReadWithoutOwnershipTransfer,
  TextAndUnitLimitsAreCoverageGaps, ProcessorsShareLimitValidationAndCancellationContract.
- Further spreadsheet/chunk checks:
  SpreadsheetInitialRowSurvivesLaterRowsAndMissingCachesAreExplicit,
  InvalidSpreadsheetReferencesAreAnchoredCoverageGaps,
  ChunksDoNotSplitSurrogatePairsAtExactBoundary.
- Optional paid AI reinterpretation: deliberately not enabled. No processor has
  network, tenant, secrets, model or storage dependencies.

## OCR acceptance mapping

- Select only image-only pages: PolicySelectsOnlyImagePagesWithoutUsableText,
  MixedDocumentUsesOcrOnlyOnScannedPage.
- Preserve text, anchors and confidence: MixedDocumentUsesOcrOnlyOnScannedPage,
  TsvPreservesLinesBoxesAndConfidence.
- No images/no text: BlankPagesAreReportedWithoutOcr.
- Limits/failures/cancellation: OcrBudgetsKeepCoverageExplicit,
  AdapterFailuresAreAnchoredAndCancellationPropagates,
  ProcessOutputIsBounded, ProcessTimeoutIsSanitizedAndChildIsTerminated,
  RunningProcessCancellationTerminatesChild,
  InvalidUtf8StopsRunningProcessWithoutWaitingForDeadline, MissingRuntimeIsExplicit.
- SOLID/Strategy/Adapter: role-specific injected interfaces in Contracts.cs;
  tests assert which collaborators were called and which were not.
- MIT-first: separate PDFtoImage package/license manifest, no Poppler dependency.
- Linux: local CLI supported; actual integration smoke result to be recorded
  separately, never inferred from mocked process tests.
