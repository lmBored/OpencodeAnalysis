using System.Text.Json;
using GhCore;
using Octokit;

namespace GhActivities;

public sealed class ActivityCollector(
    RestCaller rest,
    string owner,
    string repository,
    ActivityWindow window,
    Action<string> log)
{
    private const int PageSize = 100;
    private const int MaxPages = 700;
    private static readonly TimeSpan PageDelay = TimeSpan.FromMilliseconds(150);

    public async Task<RepoSnapshot> GetRepoAsync()
    {
        var repo = await rest.Call(() => rest.Client.Repository.Get(owner, repository));
        return new RepoSnapshot(
            repo.StargazersCount,
            repo.ForksCount,
            repo.OpenIssuesCount,
            repo.Size,
            repo.HasDiscussions,
            repo.CreatedAt,
            repo.DefaultBranch);
    }

    public async Task<string> GetCommitActivityJsonAsync()
    {
        var activity = await rest.Call(
            () => rest.Client.Repository.Statistics.GetCommitActivity(owner, repository));
        return JsonSerializer.Serialize(
            activity.Activity.Select(week => new { week = week.Week, total = week.Total, days = week.Days }));
    }

    public async Task<IReadOnlyList<CommitRow>> GetCommitsAsync()
    {
        var commits = new List<CommitRow>();
        await foreach (var (page, batch) in PagesAsync(p => rest.Client.Repository.Commit.GetAll(
                           owner,
                           repository,
                           new CommitRequest { Since = window.From, Until = window.To },
                           RestCaller.Page(p))))
        {
            var kept = batch
                .Where(c => window.Contains(c.Commit.Committer.Date))
                .Select(c => new CommitRow(c.Commit.Committer.Date, c.Commit.Author.Date, ShortSha(c.Sha)))
                .ToList();
            commits.AddRange(kept);
            log($"commits {page}: kept {kept.Count} of {batch.Count}");
        }
        return commits;
    }

    public async Task<IReadOnlyList<PullRequestRow>> GetPullRequestsAsync()
    {
        var request = new PullRequestRequest
        {
            State = ItemStateFilter.All,
            SortProperty = PullRequestSort.Created,
            SortDirection = SortDirection.Descending,
        };
        var pullRequests = new List<PullRequestRow>();
        await foreach (var (page, batch) in PagesAsync(p => rest.Client.PullRequest.GetAllForRepository(owner, repository, request, RestCaller.Page(p))))
        {
            pullRequests.AddRange(batch
                .Where(pr => window.Contains(pr.CreatedAt))
                .Select(pr => new PullRequestRow(
                    pr.Number,
                    pr.CreatedAt,
                    pr.MergedAt,
                    pr.State.ToString().ToLowerInvariant(),
                    pr.Title,
                    pr.HtmlUrl.ToString(),
                    pr.User?.Login ?? string.Empty)));
            if (page % 40 == 0)
            {
                log($"pull requests {page}: {pullRequests.Count} in window");
            }
            if (batch.All(pr => pr.CreatedAt < window.From))
            {
                break;
            }
        }
        return pullRequests.DistinctBy(pr => pr.Number).ToList();
    }

    public async Task<IReadOnlyList<ReleaseRow>> GetReleasesAsync()
    {
        var releases = new List<ReleaseRow>();
        await foreach (var (page, batch) in PagesAsync(p => rest.Client.Repository.Release.GetAll(owner, repository, RestCaller.Page(p))))
        {
            releases.AddRange(batch
                .Where(r => !r.Draft && r.PublishedAt is { } published && window.Contains(published))
                .Select(r => new ReleaseRow(r.TagName, r.Name ?? string.Empty, r.PublishedAt!.Value, r.Prerelease, r.HtmlUrl.ToString())));
            if (batch.All(r => r.PublishedAt is null || r.PublishedAt < window.From))
            {
                break;
            }
        }
        log($"releases: {releases.Count}");
        return releases;
    }

    private async IAsyncEnumerable<(int Page, IReadOnlyList<T> Items)> PagesAsync<T>(Func<int, Task<IReadOnlyList<T>>> fetchPage)
    {
        for (var page = 1; page <= MaxPages; page++)
        {
            var items = await rest.Call(() => fetchPage(page));
            if (items.Count == 0)
            {
                yield break;
            }
            yield return (page, items);
            if (items.Count < PageSize)
            {
                yield break;
            }
            await Task.Delay(PageDelay);
        }
    }

    private static string ShortSha(string sha) => sha.Length >= 7 ? sha[..7] : sha;
}
