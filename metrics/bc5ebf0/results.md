# opencode metrics — bc5ebf0

Commit: `bc5ebf00a4c169ece10f276e6ce02eaf18c326e6` (2026-10-16)

This folder contains the result artifacts for the original measured commit,
the one documented in `../README.md` and `../notes.txt`.

## Results

| Metric | Tool | Result |
| --- | --- | --- |
| Max function CCN | ESLint 9.39.5 | **363** — `packages/core/src/shell/scan.ts:1740`, `scanPowerShellList` |
| Max function CCN (cross-check) | Lizard 1.24.1 | **355** — `scanBashSpan`, same file |
| Linted files / functions reported | ESLint | 5,104 / 83,335 |
| Files analysed | madge 8.0.0 / skott 0.35.12 | 4,161 / 3,847 |
| Max Ca (fan-in) | madge / skott | **145** (`packages/ai/src/schema/index.ts`) / **145** |
| Max Ce (fan-out) | madge / skott | **94** (`packages/core/src/plugin/internal.ts`) / **93** |
| Instability | madge | mean **0.39**, median **0.33** |
| Vulnerable packages | bun audit 1.4.3 | 36 packages / 248 advisories (re-run 2026-10-24; notes: 30 / 200); crypto/auth **0** |
| Vulnerable crypto/auth | OSV-Scanner 2.7.0 | **0** |
| SQL columns / tables | `inventory.mjs` | **191** / **28** |
| Validation sites (packages) | `validation-surfaces.mjs` | **597** sites, 69 yes, 33 review → **11.6%** |
| Validation sites (whole clone) | `validation-surfaces.mjs` | **638** sites, 75 yes → **11.8%** |
| Legacy v1 imports | repo search | `legacy-v1-imports.md` |

## Artifacts

- `analysis/complexity.json`, `analysis/lizard.csv`, `analysis/sortedLizard.csv`
- `analysis/allLanguageLizard.csv`, `analysis/allLanguageSortedLizard.csv`
- `columns.csv`, `percentiles.csv`, `surface-sites.csv`, `osv.json`
- `fandistribution.py` / `.png`, `fanpercentile.py` / `.png`
- `legacy-v1-imports.md`

## Notes

- `osv.json` was regenerated from the `bc5ebf0` lockfile on 2026-10-25 with the
  current OSV advisory database. It has 61 vulnerable package-versions across 37
  package names (320 vulnerability rows). The earlier 2026-10-18 snapshot
  recorded in `../README.md` had 253 advisory entries across 31 packages.
- Advisory totals drift because the Bun/OSV advisory databases are live.
- The fan charts were rendered from the same data hard-coded in the original
  plotting scripts.
