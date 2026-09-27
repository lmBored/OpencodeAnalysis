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

| Data | Octokit | Scope |
|---|---|---|
| Commits | `Repository.Commit.GetAll` (`since`/`until`) | all commits on `dev` in the window |
| Pull requests | `PullRequest.GetAllForRepository` (created desc) | all PRs created in the window |
| Contributors | `Repository.GetAllContributors(anon: true)` | all time |
| PRs reviewed per person | `Search.SearchIssues` `is:pr reviewed-by:X -author:X created:<window>` | candidates = top 40 committers U (union with) top 40 PR authors |
| Author x reviewer pairs | `Search.SearchIssues` `is:pr author:A reviewed-by:B created:<window>` | all ordered pairs of the top 10 humans |
| Review comments | `PullRequest.ReviewComment.GetAllForRepository` | whole repo, window |
| Conversation comments | `Issue.Comment.GetAllForRepository` | whole repo, window |
| Reviews | `PullRequest.Review.GetAll` | **targeted**: pair PRs where the reviewer also commented |
| Changed files | `PullRequest.Files` | **random**: 150 merged PRs each top 10 human (seed 42) |

## Note

* Squash merges make commit counts mirror merged PRs.
* Search counts reviews on PRs created in the window, so reviews on older PRs are not counted.
* Communication outside of GitHub (Discord) is not taken into account, thus communication scope is limited to this GitHub only.
