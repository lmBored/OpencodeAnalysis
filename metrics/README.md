# Opencode metrics workspace

Reproducible measurements for the software-quality assessment of
[opencode](https://github.com/anomalyco/opencode). This README is the operational
companion to [`notes.txt`](./notes.txt): `notes.txt` holds the findings and the
reasoning, this file explains how to regenerate every number in it.

All commands below assume Windows 11 + PowerShell and are run from this
directory (`...\OpencodeAnalysis\metrics`) unless stated otherwise. The analysed
clone lives in `./opencode`.

---

## 1. Layout

| Path | Kind | Purpose |
| --- | --- | --- |
| `opencode/` | clone | The repository under analysis (`git` checkout, pinned below). |
| `eslint.config.mjs` | tool config | Flat ESLint config that enables `complexity: ["warn", 0]` for every `.ts`/`.tsx` file. |
| `package.json` | tool deps | `madge`, `ts-morph`, `typescript` (used by the scripts below). |
| `analysis/package.json` | tool deps | `eslint`, `typescript-eslint`, `dependency-cruiser`. |
| `analysis/max-complexity.mjs` | tool | Summarises `analysis/complexity.json` (max CCN, top functions/files). |
| `madge-coupling.mjs` | tool | File-level Ca / Ce / instability, fan-in/out distributions, percentile table. Writes `percentiles.csv`. |
| `skott-coupling.mjs` | tool | Independent Ca / Ce measurement for cross-checking madge. |
| `inventory.mjs` | tool | Extracts every SQL column definition into `columns.csv`. |
| `validation-surfaces.mjs` | tool | Counts input-validation surfaces and whether the enclosing scope validates. Writes `surface-sites.csv`. |
| `audit-count.mjs` | tool | `bun audit` summary (vulnerable packages, advisories, severities). |
| `audit-crypto.mjs` | tool | Filters the Bun audit to crypto/auth packages. |
| `osv-filter.mjs` | tool | Filters an OSV-Scanner JSON report (`osv.json`) to crypto/auth packages. |
| `fandistribution.py` | plot | Bar chart of the fan-in / fan-out buckets. |
| `fanpercentile.py` | plot | Bar chart of the fan-in / fan-out summary statistics. |
| `bus.ts`, `subscriber.ts` | reference copy | `packages/core/src/bus.ts` with `Subscriber`/`observe` extracted (observer pattern). Not built. |
| `storage.tsx` | reference copy | `packages/tui/src/context/storage.tsx` with `Schema` validation added to the persisted-JSON paths (0/2 → 2/2 surfaces). Not built. |
| `legacy-v1-imports.md` | report | Every import/re-export of legacy `v1` modules across the clone. |
| `analysis/complexity.json` | output | Raw ESLint JSON (large, ~50 MB). |
| `analysis/lizard.csv`, `analysis/sortedLizard.csv` | output | Lizard CCN rows, TypeScript only; `sorted` is sorted descending by CCN. |
| `analysis/allLanguageLizard.csv`, `analysis/allLanguageSortedLizard.csv` | output | Same without `-l typescript` (all languages Lizard knows). |
| `columns.csv` | output | SQL tables and columns (`inventory.mjs`). |
| `percentiles.csv` | output | Ca/Ce percentile table (`madge-coupling.mjs`). |
| `surface-sites.csv` | output | One row per validation surface (`validation-surfaces.mjs`). |
| `osv.json` | output | OSV-Scanner JSON for `opencode/bun.lock`. |
| `depcruise.json` | output (stub) | Empty `dependency-cruiser` result from an aborted run; nothing reads it. |
| `notes.txt` | source | Findings, reasoning and the specification for the measurements. |

---

## 2. Prerequisites

Recorded toolchain (from `notes.txt`):

| Tool | Version |
| --- | --- |
| Node.js | 22.x |
| Bun | 1.4.3 |
| ESLint | 9.39.5 |
| typescript-eslint | 8.71.2 |
| Lizard | 1.24.1 (Python) |
| madge | 8.0.0 |
| skott | 0.35.12 |
| OSV-Scanner | 2.7.0 (osv-scalibr 0.5.3) |
| dependency-cruiser | 18.6.0 |
| Python | 3.x + `matplotlib`, `numpy` |

One-time setup:

```powershell
git clone https://github.com/anomalyco/opencode opencode
git -C opencode checkout bc5ebf00a4c169ece10f276e6ce02eaf18c326e6

npm ci --legacy-peer-deps                   # madge, ts-morph, typescript
npm --prefix analysis ci                    # eslint, typescript-eslint, dependency-cruiser
npm install --no-save --legacy-peer-deps eslint@9.39.5 typescript-eslint@8.71.2 skott@0.35.12

pip install lizard==1.24.1
```

`--legacy-peer-deps` is required because `madge@8` declares an optional peer
`typescript@^5.4.4` while this workspace uses `typescript@6.0.3`; a plain
`npm install`/`npm ci` fails with `ERESOLVE` on a clean machine.

`npm install --no-save` installs the ESLint/skott toolchain without touching
`package.json` or the lockfile; re-run it after a plain `npm ci` (npm removes
packages it does not know about).

`notes.txt` cites commit `b471c2b…`, while the clone is pinned at `bc5ebf0…`;
line numbers in the notes can therefore be a few lines off.

### Which directory do I scan?

- `.\opencode` — the whole clone, including repo-level `script/`, `services/`,
  `github/`, `nix/` and `.opencode/` tooling.
- `.\opencode\packages` — only the published packages. The checked-in
  `surface-sites.csv` and `columns.csv` were produced this way, so their `file`
  column starts with the package name (`core/src/...`).

The extracted `bus.ts` / `subscriber.ts` / `storage.tsx` copies sit in this
workspace on purpose: scanning `.\opencode` (rather than `.`) keeps them out of
the measurements.

---

## 3. Cyclomatic complexity

### 3.1 ESLint (per-function CCN, max = 363)

The config sets `complexity: ["warn", 0]`, so **every** function is reported with
its own complexity; the largest value is the project maximum. `warningCount` is
therefore huge (~83k) and is not a defect count.

```powershell
npx eslint .\opencode --format json --output-file analysis\complexity.json
node analysis\max-complexity.mjs
```

`max-complexity.mjs` reads `analysis/complexity.json` relative to itself (it can
be run from any directory) and prints the linted file count, the number of
reported functions, the maximum, the top 10 functions and the top 10 files.

Expected (recorded):

```text
Max complexity: 363 (packages\core\src\shell\scan.ts:1740, scanPowerShellList)
```

The run covers roughly 5.1k files. ESLint exits with status 1 because
`packages/app/src/custom-elements.d.ts` fails to parse under the parser-only
config; the JSON is still written in full and no `complexity` result is lost, so
treat the non-zero exit as expected.

Runners-up: `packages/ui/src/theme/resolve.ts` (181),
`packages/tui/src/mini/stream-v2.transport.ts` (146),
`packages/client/src/solid/data.ts` (131).

> The checked-in `analysis/complexity.json` was produced from an older workspace
> path (`...\CapstoneSoftDev\opencode\...`), so the absolute paths inside it are
> stale. Regenerate it with the command above before quoting paths.

> The script was repaired on 2026-10-24: it now parses the ESLint JSON that the
> `npx eslint …` step produces. It previously read `depcruise.json` and could not
> run because of a syntax error (`top[0].[key]`).

### 3.2 Lizard (independent CCN, max = 355)

```powershell
lizard .\opencode -l typescript -x "*/node_modules/*" -x "*/dist/*" --csv -o analysis\lizard.csv
```

Lizard's CSV columns are:
`nloc, CCN, token_count, parameter_count, length, location, file, function, long_name, start_line, end_line`
(the **second** column is the CCN). It has no header row.

To sort descending by CCN:

```powershell
Import-Csv analysis\lizard.csv -Header nloc,ccn,tokens,params,length,location,file,function,long_name,start_line,end_line |
  Sort-Object { [int]$_.ccn } -Descending |
  Export-Csv analysis\sortedLizard.csv -NoTypeInformation -Encoding utf8
```

Expected maximum: **355** for `scanBashSpan` in
`.\opencode\packages\core\src\shell\scan.ts` (same file as ESLint, different
function). Next: 245 (`parallelLaneAxis`, merman routing), 212
(`addVerticalElbowTransition`, merman routing), 146 (`convertHTMLToMarkdown`),
112 (`consume`, CLI non-interactive).

For the all-language run, drop `-l typescript`:

```powershell
lizard .\opencode -x "*/node_modules/*" -x "*/dist/*" --csv -o analysis\allLanguageLizard.csv
```

> `analysis\lizard.csv` and `analysis\sortedLizard.csv` in this folder were
> re-saved by Excel (tab/semicolon delimiters, added header). Regenerate them
> with the commands above for clean, comma-separated data.
> `allLanguageLizard.csv` is an untouched `--csv` output.

The ESLint/Lizard difference is expected (they count nesting and logical
operators differently) and is discussed in `notes.txt`.

---

## 4. Coupling

### 4.1 madge — Ca, Ce, instability, fan distributions

```powershell
node madge-coupling.mjs .\opencode
```

Input: the clone. Output: console tables + `percentiles.csv` (written to the
current directory, so run from here to overwrite the checked-in file).

What it prints:

1. Files analysed, max **Ce** (efferent coupling) and max **Ca** (afferent
   coupling) with the top 5 files.
2. Instability `I = Ce / (Ca + Ce)` (`null` for isolated files): count of files
   with defined `I`, mean, median, the most unstable (`I = 1`) and most stable
   (`I ≈ 0`) files.
3. Fan-in/fan-out mean, median, p90, p99, max and the bucket histogram
   (`0`, `1-2`, `3-5`, `6-10`, `11+`).
4. A percentile table (`p50 … max` with growth factors), also written to
   `percentiles.csv`.

Recorded: 4160 files, max Ca **145** (`packages/ai/src/schema/index.ts`, a barrel
file), max Ce **94** (`packages/core/src/plugin/internal.ts`), mean `I` 0.39,
median 0.33, fan-in max 145, fan-out max 94, p99 Ca 27 / Ce 20.

`tsConfig` is intentionally commented out in the script (path aliases are not
resolved), mirroring the recorded run; uncommenting it changes the graph.

### 4.2 skott — independent cross-check

`skott` is not part of `package.json`; install the pinned version first (see
setup), then:

```powershell
node skott-coupling.mjs .\opencode
```

Prints files analysed and the max Ca/Ce with top 5 files. Recorded: 3847 files,
max Ca **145** (same barrel file), max Ce **93**. skott treats type-only imports
differently and resolves aliases differently from madge, hence the small value
differences and larger placement differences.

### 4.3 dependency-cruiser (optional)

`analysis/package.json` includes `dependency-cruiser@18.6.0`, and
`analysis/max-complexity.mjs` used to read its JSON. The checked-in
`depcruise.json` is an empty stub (`"modules": []`) from an aborted run; no
script consumes it. Use madge/skott for coupling.

### 4.4 Fan charts

`fandistribution.py` and `fanpercentile.py` hard-code the numbers from the madge
output (bucket histogram and percentile table). Update the arrays in the script
after re-running madge, then:

```powershell
python fandistribution.py
python fanpercentile.py
```

Both end in `plt.show()`, which opens a window and blocks. For headless/exported
images use `$env:MPLBACKEND="Agg"` and add `fig.savefig("fan.png")` before
`plt.show()`.

---

## 5. Dependency vulnerabilities

### 5.1 Bun audit summary

These two scripts use `Bun.spawnSync`, so run them with **bun**, not node:

```powershell
bun audit-count.mjs .\opencode
bun audit-crypto.mjs .\opencode
```

`audit-count.mjs` prints vulnerable packages, advisories, advisories by severity
and unique advisory IDs. `audit-crypto.mjs` additionally lists installed
crypto/auth packages (`bun pm ls --all` filtered by a regex) and how many of them
are vulnerable.

Recorded (`notes.txt`): 0 vulnerable crypto/auth dependencies; 200 advisories /
30 vulnerable packages at the time. Re-run on 2026-10-24: 0 vulnerable
crypto/auth, 36 vulnerable packages, 248 advisories (7 critical, 83 high, 123
moderate, 35 low), installed crypto/auth = `cookie`, `jose`, `@noble/hashes`,
`jwa`, `jws`, `openid-client`. Counts drift because the advisory database is
live.

### 5.2 OSV-Scanner

```powershell
osv-scanner scan source --lockfile=.\opencode\bun.lock --format json --output-file=.\osv.json
node osv-filter.mjs
```

`osv-filter.mjs` reads `osv.json` from the current directory, so run it from
here. It prints vulnerable crypto/auth packages only (recorded: **0**).

The checked-in `osv.json` (2026-10-18) contains 253 advisory entries across 31
distinct packages; a re-run on 2026-10-24 produced 320 / 37. None are
crypto/auth in either run.

---

## 6. SQL / credential storage inventory

```powershell
node inventory.mjs .\opencode\packages
```

Finds `<name>Table(...)` calls, reads the column object literal and writes
`columns.csv` (`file,table,column,definition`) in the current directory.
Recorded: **191 columns** across **28 tables**.

The `table` column contains the literal first argument (including quotes, e.g.
`"credential"`), and `definition` is the column expression truncated to 100
characters. To check the confidentiality claim in `notes.txt`, filter
`core/src/credential/sql.ts`; the `value` row shows the plain JSON text column.
The complementary search for at-rest encryption (OS keychain or cipher APIs):

```powershell
Get-ChildItem .\opencode\packages\core\src, .\opencode\packages\server\src -Recurse -Filter *.ts |
  Select-String -Pattern 'keychain|keytar|safeStorage|createCipheriv|createDecipheriv|pbkdf2|scrypt'
```

Expected: no matches. Searching for the bare word `encrypt` is noisy (it also
matches protocol fields such as encrypted reasoning tokens and test fixtures).

---

## 7. Input-validation coverage

```powershell
node validation-surfaces.mjs .\opencode\packages   # reproduces the checked-in CSV
node validation-surfaces.mjs .\opencode            # whole clone
```

Prints a per-surface summary and writes `surface-sites.csv`
(`surface,file,line,status`) to the current directory.

How the heuristic works:

- **Surfaces**: `db-write` (`.values`/`.set` on `insert`/`update`),
  `file-config` (`readFile*`, `Bun.file`, `JSON.parse`), `cli`
  (`.option`/`.positional`), `tui` (JSX `onInput`/`onChange`/`onSubmit`/`onPaste`,
  `useInput`/`useKeyboard`, `createInterface`), and `process.argv`.
- **Scope**: the nearest enclosing function (declaration/expression/arrow/method),
  otherwise the whole file. The scope text is regex-tested for schema validation
  (`.parse`, `.safeParse`, `zValidator`, `schema`, `.check`) or, for CLI, for
  strong option constraints (`choices`, `coerce`, `check`, `demandOption`,
  `conflicts`, `implies`, `requiresArg`).
- **Status**: `yes` (scope validates), `typed-only` (CLI option only declares a
  type), `review` (TUI handler is an identifier that cannot be resolved
  statically), `no`.

Recorded (packages only): 597 sites — 69 `yes`, 33 `review`, 495 `no`
(11.6% coverage). The whole-clone run adds 41 sites in repo-level tooling:
638 sites, 75 `yes` (11.8%).

Caveats: this is a screening heuristic. Validation performed by a caller or a
helper outside the enclosing function is missed, and any occurrence of the word
`schema` (even `options.schema`) counts as validation. Use it to find candidates,
then read the code.

Paths: `add()` strips everything up to `packages/`, so package rows are relative
(`core/src/...`) while rows outside `packages/` keep their absolute path.

---

## 8. Legacy v1 imports report

`legacy-v1-imports.md` is a grouped, file-by-file list of imports/re-exports of
legacy `v1` modules (theme, schema, server, core, tui, cli). It was produced by a
repo-wide search, for example:

```powershell
Get-ChildItem .\opencode\packages -Recurse -Include *.ts, *.tsx |
  Where-Object { $_.FullName -notmatch 'node_modules' } |
  Select-String -Pattern 'v1\.js|V1'
```

Review the matches and group them by package in the Markdown file. The report is
a snapshot; re-run the search after updating the clone.

---

## 9. Reference copies (not tools)

- `bus.ts` + `subscriber.ts`: a copy of `packages/core/src/bus.ts` with the
  `Subscriber` type and `observe` helper moved to `subscriber.ts` and re-imported
  (`./subscriber.js`). Used to discuss the observer pattern; it is not wired into
  the build and its `@opencode/*` imports resolve only inside the clone.
- `storage.tsx`: a copy of `packages/tui/src/context/storage.tsx` with an optional
  `Schema.Codec<Value>` added to `Options` and `Schema.decodeUnknownSync` used on
  both persisted-JSON paths (`clone` and `load`), plus validation before writing.
  Under the section 7 heuristic the two `file-config` surfaces go from `no`/`no`
  (0.0%) to `yes`/`yes` (100.0%).

These are illustrative artifacts, not part of any pipeline.

---

## 10. Mapping `notes.txt` claims to tools

| Claim in `notes.txt` | How to reproduce |
| --- | --- |
| Installability: one setup step, many package managers | Manual: read the `install` script, `README.md` install section and `flake.nix`; count the steps. |
| 39 model providers | Manual: list `opencode/packages/ai/src/providers/*.ts`, drop `index.ts` and shared option/helper modules, merge variants of one provider (e.g. `zai.ts` + `zai-coding-plan.ts`, `google-vertex*.ts`). |
| Max CCN 363 in `core/src/shell/scan.ts` | §3.1 (ESLint + `max-complexity.mjs`). |
| Cross-check: Lizard max 355 in the same file | §3.2. |
| Coupling, instability, barrel file (`ai/src/schema/index.ts`) | §4.1 (madge) and §4.2 (skott); `percentiles.csv`. |
| 0 vulnerable crypto/auth dependencies; 30 vulnerable packages / 200 advisories | §5.1 and §5.2. |
| Credentials stored as plain JSON text, no encryption at rest | §6 (`columns.csv` + `Select-String` search). |
| Input validation / security surfaces | §7. |
| No sandbox; permission system is UX only | Manual: `SECURITY.md`, `packages/core/src/permission.ts`, `packages/plugin/src/index.ts`. |
| Flexibility: model catalog, plugin hooks | Manual: `packages/core/src/models-dev.ts`, `packages/plugin/src/index.ts`. |
| Legacy v1 usage | §8. |

---

## 11. Recorded results

Captured from the artifacts in this folder (clone pinned at `bc5ebf0`,
2026-10-16) unless noted.

| Metric | Tool | Result |
| --- | --- | --- |
| Max function CCN | ESLint 9.39.5 + `max-complexity.mjs` | 363, `scanPowerShellList` @ `packages/core/src/shell/scan.ts:1740` |
| Max function CCN (cross-check) | Lizard 1.24.1 | 355, `scanBashSpan` @ same file |
| Files analysed | madge 8.0.0 / skott 0.35.12 | 4160 / 3847 |
| Max Ca (fan-in) | madge / skott | 145 (`packages/ai/src/schema/index.ts`) / 145 |
| Max Ce (fan-out) | madge / skott | 94 (`packages/core/src/plugin/internal.ts`) / 93 |
| Instability | madge | mean 0.39, median 0.33 |
| Vulnerable packages | bun audit 1.4.3 | 36 packages, 248 advisories (re-run 2026-10-24; notes: 30 / 200); 0 crypto/auth |
| Vulnerable crypto/auth | OSV-Scanner 2.7.0 | 0 |
| SQL columns / tables | `inventory.mjs` | 191 / 28 |
| Validation sites (packages) | `validation-surfaces.mjs` | 597 sites, 69 yes, 33 review → 11.6% |
| Validation sites (whole clone) | `validation-surfaces.mjs` | 638 sites, 75 yes → 11.8% |

Advisory counts change over time; the values above are a snapshot, not a fixed
property of the repository.
