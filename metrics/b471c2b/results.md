# opencode metrics — b471c2b

Commit: `b471c2b4495747353af768fbf2e0790c9d820ce2` (2026-09-26)

This folder contains the result artifacts for the commit requested for the
experimental re-run.

## Results

| Metric | Tool | Result |
| --- | --- | --- |
| Max function CCN | ESLint 9.39.5 | **230** — `packages/opencode/src/provider/provider.ts:1401` (anonymous `Effect.gen` generator) |
| Max function CCN (cross-check) | Lizard 1.24.1 | **230** — same function |
| Linted files / functions reported | ESLint | 3,351 / 55,563 |
| Files analysed | madge 8.0.0 | 2,883 |
| Max Ca (fan-in) | madge | **79** — `packages/core/src/effect/app-node.ts` |
| Max Ce (fan-out) | madge | **63** — `packages/desktop/src/renderer/i18n/index.ts` |
| Instability | madge | mean **0.43**, median **0.50** |
| Fan-in | madge | mean 1.9, median 1, p90 4, p99 20, max 79 |
| Fan-out | madge | mean 1.9, median 1, p90 5, p99 17, max 63 |
| Files analysed | skott 0.35.12 | 2,532 |
| Max Ca / Ce | skott | **78** / **63** |
| Vulnerable packages | bun audit 1.4.3 | **67** packages / **408** advisories (9 critical, 156 high, 192 moderate, 51 low) |
| Vulnerable crypto/auth | bun audit | **0** of 8 installed crypto/auth packages |
| Vulnerable package-versions | OSV-Scanner 2.7.0 | **95** across 57 package names |
| Distinct advisory IDs | OSV-Scanner | **331** |
| Vulnerable crypto/auth | OSV-Scanner | **0** |
| SQL columns / tables | `inventory.mjs` | **314** / **47** |
| Validation sites (packages) | `validation-surfaces.mjs` | **775** sites, 87 yes, 98 typed-only, 44 review, 546 no → **11.2%** |
| Validation sites (whole clone) | `validation-surfaces.mjs` | **802** sites, 91 yes → **11.3%** |
| Legacy v1 references | repo search | **2,239** in 413 files; 268 import/export-like in 163 files |

Top legacy-v1 packages: `opencode` 1,087; `core` 292; `app` 215; `web` 208; `ui` 113.

## Artifacts

- `analysis/complexity.json`, `analysis/lizard.csv`, `analysis/sortedLizard.csv`
- `analysis/allLanguageLizard.csv`, `analysis/allLanguageSortedLizard.csv`
- `columns.csv`, `percentiles.csv`, `surface-sites.csv`,
  `surface-sites.whole-clone.csv`, `osv.json`
- `fandistribution.py` / `.png`, `fanpercentile.py` / `.png`

## Notes

- At this commit `packages/core/src/shell/scan.ts` does not exist, so the
  `363` / `355` values recorded for `bc5ebf0` do not apply here.
- Advisory totals drift because the Bun/OSV advisory databases are live.
- ESLint exited with status 1 because two `custom-elements.d.ts` files fail to
  parse; the JSON output is still complete.
