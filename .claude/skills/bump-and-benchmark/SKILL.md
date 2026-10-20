---
name: bump-and-benchmark
description: Bump the benchmarked libraries to their latest NuGet versions, re-run the suite at full fidelity, publish the results, log significant performance swings to CHANGELOG.md, and open a PR. Use for a library bump, a results refresh, or a dependabot follow-up that needs fresh published numbers.
---

# Bump and benchmark

One publish cycle: every library on its latest version, measured at full fidelity, documented, and up for review as a single PR. Run each step to its criterion before starting the next.

## 1. Branch from fresh main

`git fetch`, then `git switch -c bump/<short-summary> origin/main`. Unset the upstream so a push creates the branch rather than targeting main. A dirty `src/XLBench/XLBench.csproj` may hold bumps the user already made: carry it over (stash, switch, pop, keep the stashed side on conflict) and do not drop the stash.

Done when the branch sits on `origin/main` with only the carried-over changes.

## 2. Apply the bumps

```powershell
./scripts/update-libraries.ps1 -Apply
dotnet build -c Release src/XLBench
```

A bump can break NuGet restore with `NU1107` when a library's new transitive dependency falls outside another library's declared range. Example: IronXL 2026.10.5 needs SixLabors.Fonts ≥ 3.2, and ClosedXML 0.105.1 caps it below 3.0. Hold the bumped library at its previous version. Pinning the transitive package outside a library's range lets the restore pass but can break that library at runtime, which corrupts its numbers. Note every held-back package for the PR and changelog.

Done when the Release build has 0 errors and every package is either bumped or recorded as held back with its reason.

## 3. Update the version tables

The version tables in `README.md` and `docs/index.md` (both under "Libraries under test") are maintained by hand. Set each row to the version now in `XLBench.csproj`. The other `docs/` files are generated: `results.md` and `results-data.js` by the run, `charts.html` by the `docs-charts` workflow after merge, and `results-ci.md` by CI. Leave them to their generators.

Done when every library row in both tables matches the csproj.

## 4. Run the suite at full fidelity

```powershell
./scripts/run-benchmarks.ps1 -FullFidelity
```

Start the run in the background. It takes over an hour, and you will be notified when it exits. With no `XLBENCH_IRONXL_KEY`, IronXL is replayed from `snapshots/ironxl.json` and marked ⧗. Its numbers then describe the snapshot's version, not the pinned one. Say so in the PR. If the key is set, commit the refreshed snapshot.

Done when the run exits 0 and `docs/results.md` / `docs/results-data.js` carry today's timestamp. Then check convergence: any row whose Error is above ~5% of its Mean has not settled. List those rows in the PR as low-confidence.

## 5. Find the swings

```powershell
./.claude/skills/bump-and-benchmark/compare-results.ps1
```

It compares the new `docs/results-data.js` with the copy on `origin/main` (the last publish). It flags rows that moved by ≥10% in time (beyond the combined Error) or in allocation. `SIGNIFICANT` rows belong to libraries whose version changed. `drift` rows belong to unchanged libraries, and their size is the noise floor for this run. A `SIGNIFICANT` swing no larger than the drift is not attributable to the bump.

Done when every flagged row is classified as attributable, within drift, or snapshot.

## 6. Write the changelog entry

Add an entry at the top of `CHANGELOG.md` at the repo root (create it with a `# Changelog` heading if missing):

```markdown
## YYYY-MM-DD — <one-line summary of the bumps>

**Versions:** <Library old → new, …>. Held back: <package (reason)> or none.
**Not measured:** <IronXL replayed from snapshot <version>, …> or none.

### Significant swings

| Scenario | Library | Version | Time | Allocated |
| --- | --- | --- | --- | --- |
| EditAndRecalculate | XLibur | 0.630.0 → 0.630.1 | 10.94 → 8.13 ms (−26%) | 2.77 → 2.68 MB (−3%) |

<One or two sentences on the drift seen in unchanged libraries, so readers can judge the table.>
```

List only attributable swings in the table. If there are none, say so in one line.

Done when the entry names every bumped and held-back package and every attributable swing.

## 7. Commit and open the PR

Commit the csproj, `README.md`, `docs/index.md`, `docs/results.md`, `docs/results-data.js`, `CHANGELOG.md` and any refreshed snapshot. Leave out `docs/charts.html`. Push and `gh pr create` against `main`. Title: `Bump <libraries> and publish full-fidelity results`. Body: the versions, the held-back packages, the swing table and drift note from the changelog, low-confidence rows, and what was not measured.

Done when the PR URL is reported back to the user.
