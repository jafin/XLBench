# Changelog

Significant performance swings between published full-fidelity runs. A swing is a change of
10% or more in mean time (beyond the two runs' combined Error) or in allocation. A swing counts
as caused by a version bump only when it is larger than the drift seen in libraries whose version
did not change in the same run.

## 2026-11-28 — Publish OfficeIMO 3.6.2, MiniExcel 2.0.0, XLibur 0.900.0, EPPlus 8.7.3, Telerik 2026.4.1127

The versions were bumped in #63 and #67 without a full-fidelity run. This run publishes them.

**Versions:** OfficeIMO 3.5.3 → 3.6.2, MiniExcel 1.46.0 → 2.0.0, XLibur 0.800.0 → 0.900.0,
EPPlus 8.7.2 → 8.7.3, Telerik 2026.3.1027 → 2026.4.1127. Held back: none.
**Not measured:** IronXL is replayed from the 2026.8.1 snapshot (no licence key). The pinned
version is 2026.11.1.
**Environment:** .NET runtime 10.0.14 → 10.0.15 and SDK 10.0.403 → 10.0.404, same host.

### Significant swings

| Scenario | Library | Version | Time | Allocated |
| --- | --- | --- | --- | --- |
| OpenAmendPropertiesAndSave | OfficeIMO | 3.5.3 → 3.6.2 | 24.72 → 0.35 ms (−99%) | 7.01 → 0.47 MB (−93%) |
| OpenAndReadAll | OfficeIMO | 3.5.3 → 3.6.2 | 3039.47 → 135.63 ms (−96%) | 822.85 → 49.67 MB (−94%) |
| OpenAndReadAllSheets | OfficeIMO | 3.5.3 → 3.6.2 | 1166.46 → 50.99 ms (−96%) | 509.39 → 12.03 MB (−98%) |
| CreateAndSave | OfficeIMO | 3.5.3 → 3.6.2 | 1034.28 → 19.84 ms (−98%) | 406.69 → 9.38 MB (−98%) |
| CreateStockReport | OfficeIMO | 3.5.3 → 3.6.2 | 208.36 → 3.80 ms (−98%) | 905.82 → 2.92 MB (−100%) |
| EditAndRecalculate | OfficeIMO | 3.5.3 → 3.6.2 | 2734.66 → 6.62 ms (−100%) | 494.08 → 3.63 MB (−99%) |
| InsertColumnsAndRecalculate | OfficeIMO | 3.5.3 → 3.6.2 | 149.61 → 7.76 ms (−95%) | 70.13 → 3.48 MB (−95%) |
| FillTemplateAndSave | OfficeIMO | 3.5.3 → 3.6.2 | 82.62 → 15.46 ms (−81%) | 31.34 → 6.49 MB (−79%) |
| OpenAndReadAll | MiniExcel | 1.46.0 → 2.0.0 | 696.43 → 558.19 ms (−20%) | 806.67 → 221.18 MB (−73%) |
| OpenAndReadAllSheets | MiniExcel | 1.46.0 → 2.0.0 | 1333.84 → 1200.06 ms (−10%) | 1081.32 → 499.72 MB (−54%) |

The OfficeIMO rows also reflect #63. That PR rewrote OfficeIMO's adapters for the 3.6 cell-handle
API (`Cell(row, col).SetValue`, `Properties`, `AddSheet`, `ExcelChartOptions`). Each row measures the
3.6.2 library together with its new API, not the library alone. OfficeIMO is back in
ChangePricesAndRecalculate for the first time since its exclusion: 36.11 ms and 15.25 MB, the fastest
engine there. That row has no previous result to compare.

The largest drift in unchanged libraries was −6.0% in time (OpenXML SDK, InsertColumnsAndRecalculate).
Allocation in unchanged libraries moved by at most 3.2%. The OfficeIMO and MiniExcel swings are well
beyond that drift. The XLibur, EPPlus and Telerik bumps moved time by at most 6.6% (XLibur,
InsertColumnsAndRecalculate) and allocation by at most 0.8%, which is within drift.

## 2026-11-02 — XLibur 0.800.0

**Versions:** XLibur 0.730.0 → 0.800.0. IronXL is pinned at 2026.11.1 in its new project (see #54); it is
not part of these numbers. Held back: none.
**Not measured:** IronXL is replayed from the 2026.8.1 snapshot (no licence key).
**Environment:** unchanged, same host (.NET 10.0.14).

### Significant swings

| Scenario | Library | Version | Time | Allocated |
| --- | --- | --- | --- | --- |
| OpenAndReadAllSheets | XLibur | 0.730.0 → 0.800.0 | 906.04 → 419.44 ms (−54%) | 830.44 → 177.16 MB (−79%) |
| ChangePricesAndRecalculate | XLibur | 0.730.0 → 0.800.0 | 56.54 → 55.44 ms (−2%) | 11.97 → 10.76 MB (−10%) |

The largest drift in unchanged libraries was +17.9% in time (NPOI, InsertColumnsAndRecalculate) and
−15.6% (EPPlus, OpenAndReadAll). Allocation in unchanged libraries moved by at most 2.9%. The
OpenAndReadAllSheets time and allocation swings are well beyond that drift, so they come from the bump.
The ChangePricesAndRecalculate time change is within drift; only its allocation change is attributable.

## 2026-11-01 — OfficeIMO 3.5.3, Telerik 2026.3.1027, XLibur 0.730.0

**Versions:** OfficeIMO 3.5.1 → 3.5.3, Telerik 2026.3.1014 → 2026.3.1027, XLibur 0.720.0 → 0.730.0.
Held back: IronXL at 2026.10.1. 2026.11.1 needs SixLabors.Fonts ≥ 3.2, and ClosedXML 0.105.1 needs a
version below 3.0, so the packages cannot be restored together.
**Not measured:** IronXL is replayed from the 2026.8.1 snapshot (no licence key).
**Environment:** unchanged, same host.

### Significant swings

None attributable to the bumps. The only flagged row on a bumped library is Telerik
`InsertColumnsAndRecalculate`, +10% in time (182.91 → 201.91 ms) and +0.2% in allocation. That is
inside the drift in unchanged libraries: EPPlus +15% on `OpenAndReadAll`, NPOI −20% and OpenXML SDK
+11% on `InsertColumnsAndRecalculate`. OfficeIMO and XLibur showed no flagged swings.

## 2026-10-20 — XLibur 0.720.0, EPPlus 8.7.2, OfficeIMO 3.5.1, Telerik 2026.3.1014

**Versions:** XLibur 0.630.1-rc.18 → 0.720.0, EPPlus 8.7.1 → 8.7.2, OfficeIMO 3.4.4 → 3.5.1,
Telerik 2026.3.923 → 2026.3.1014. Held back: IronXL at 2026.10.1. 2026.10.5 needs
SixLabors.Fonts ≥ 3.2, and ClosedXML 0.105.1 needs a version below 3.0, so the packages cannot be
restored together.
**Not measured:** IronXL is replayed from the 2026.8.1 snapshot (no licence key).
**Environment:** .NET runtime 10.0.12 → 10.0.13 (SDK 10.0.401 → 10.0.402), same host.

### Significant swings

| Scenario | Library | Version | Time | Allocated |
| --- | --- | --- | --- | --- |
| CreateAndSave | OfficeIMO | 3.4.4 → 3.5.1 | 938.79 → 1075.46 ms (+15%) | 361.06 → 406.69 MB (+13%) |

The OfficeIMO write regression is the only swing that rises above drift. Its allocation rise
matters most, because allocation barely moves in libraries whose version did not change.

Unchanged libraries drifted by up to 20% in time: ClosedXML −21% on `EditAndRecalculate`, Sylvan
−14% and OpenXML SDK +13% on `OpenAndReadAll`, and OpenXML SDK −12% on
`InsertColumnsAndRecalculate`. These swings on bumped libraries are therefore within drift and
are not counted as caused by the bump:

- XLibur `OpenAndReadAll` −13%.
- EPPlus `EditAndRecalculate` −19%. This returns to the 2026-09-27 level (88 ms).
- Telerik `EditAndRecalculate` −21% and `InsertColumnsAndRecalculate` −16%. The insert result
  returns to the 2026-09-27 level. Telerik's allocations fell 5–6% on `OpenAndReadAll` and
  `CreateAndSave`, which is a small but real reduction.
