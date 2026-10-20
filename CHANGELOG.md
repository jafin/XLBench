# Changelog

Significant performance swings between published full-fidelity runs. A swing is a change of
10% or more in mean time (beyond the two runs' combined Error) or in allocation. A swing counts
as caused by a version bump only when it is larger than the drift seen in libraries whose version
did not change in the same run.

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
