# SDE

Analysis of [`anomalyco/opencode`](https://github.com/anomalyco/opencode), using GitHub REST API through [Octokit.NET](https://github.com/octokit/octokit.net) and LINQ.

## Usage

| Command | What it does |
|---|---|
| `dotnet run --project src/OpenCodeAnalysis.Cli -- fetch` | Downloads and caches everything page by page. |
| `dotnet run --project src/OpenCodeAnalysis.Cli -- contributors` | Offline: activity metrics, top 10 humans + bots row, scopes, seeded commit samples. |
| `dotnet run --project src/OpenCodeAnalysis.Cli -- collaboration` | Offline: author x reviewer matrix, targeted PR thread list, co-edit counterexample. |
| `dotnet run --project src/OpenCodeAnalysis.Cli -- activities` | Offline: weekly series + counts table (LaTeX) from `data/activities/` (run GhActivities first). |
| `dotnet run --project src/OpenCodeAnalysis.Cli -- practices` | Offline once cached (needs `fetch-health`): automation timing, design-review gate, migration and exemplars. |
| `dotnet run --project src/OpenCodeAnalysis.Cli -- fetch-health` | Downloads all-time issues, pre-window PRs and commits, and profiles for bot review. |
| `dotnet run --project src/OpenCodeAnalysis.Cli -- health` | Offline: identity filtering, contributor categories, activity and process metrics in `out/health_*`. |
| `dotnet run --project src/OpenCodeAnalysis.Cli -- all` | fetch + contributors + collaboration. |

Each command writes into `out/<command>/`; `health` writes the shared `out/health_*` files.

GhActivities (default output dir `data/activities`, run from the repo root):

```text
dotnet run --project src/GhActivities -- [outDir] [--from=yyyy-MM-dd] [--to=yyyy-MM-dd] [--refresh]
```

It writes `commits.csv`, `prs.csv`, `releases.csv`, `reviews.csv`, `repo.json`,
`commit_activity.json`, `summary.txt`.


## Data collected

Seed 42 was used for all random sample. `out/contributors/run_info.txt` records the window, collection time, seed and frame sizes.

| Data | Scope |
|---|---|
| Commits | all commits on `dev` in the window |
| Pull requests | all PRs created in the window |
| Returning authors | authors with a PR created before the analyzed window (PR-history only) |
| Issues | all issues all time in `data/issue_items.json`, filtered to the window by the analyses |
| Contributors | all time |
| PRs reviewed per person | candidates = top 40 committers U (union with) top 40 PR authors |
| Author x reviewer pairs | all ordered pairs of the top 10 humans |
| Review comments | whole repo, window |
| Conversation comments | whole repo, window |
| Reviews | **targeted**: pair PRs where the reviewer also commented |
| Changed files | **random**: 150 merged PRs each top 10 human (seed 42) |
| Team members | `.github/TEAM_MEMBERS` |
| Practice sample | **stratified**: 150 first PR + 100 repeat PR + 50 top-10 activity PRs (seed 42); changed files cached in `data/files/`, trees in `data/practices/` |
| Health sample | 54 newcomer PRs (quarter x team, seed 42) plus identity and bot review samples |

To see what Octokit endpoint was used, read `src/OpenCodeAnalysis/GitHub/GitHubFetcher.cs`

## Note

* Squash merges make commit counts mirror merged PRs.
* Search counts reviews on PRs created in the window, so reviews on older PRs are not counted.
* Communication outside of GitHub (Discord) is not taken into account, thus communication scope is limited to this GitHub only.
