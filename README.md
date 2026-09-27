# SDE

Analysis of [`anomalyco/opencode`](https://github.com/anomalyco/opencode), using GitHub REST API through [Octokit.NET](https://github.com/octokit/octokit.net) and LINQ.

## Usage

| Command | What it does |
|---|---|
| `dotnet run -- fetch` | Downloads and caches everything page by page. |
| `dotnet run -- contributors` | Offline: activity metrics, top 10 humans + bots row, scopes, seeded commit samples. |
| `dotnet run -- collaboration` | Offline: author x reviewer matrix, targeted PR thread list, co-edit counterexample. |
| `dotnet run -- all` | All of the above. |

## Data collected

Seed 42 was used for all random sample. `out/run_info.txt` records the window, collection time, seed and frame sizes.

| Data | Scope |
|---|---|
| Commits | all commits on `dev` in the window |
| Pull requests | all PRs created in the window |
| Contributors | all time |
| PRs reviewed per person | candidates = top 40 committers U (union with) top 40 PR authors |
| Author x reviewer pairs | all ordered pairs of the top 10 humans |
| Review comments | whole repo, window |
| Conversation comments | whole repo, window |
| Reviews | **targeted**: pair PRs where the reviewer also commented |
| Changed files | **random**: 150 merged PRs each top 10 human (seed 42) |

To see what Octokit endpoint was used, read `GitHubFetcher.cs`

## Note

* Squash merges make commit counts mirror merged PRs.
* Search counts reviews on PRs created in the window, so reviews on older PRs are not counted.
* Communication outside of GitHub (Discord) is not taken into account, thus communication scope is limited to this GitHub only.
