using System.Text.Json;
using GhCore;
using Octokit;

namespace GhActivities;

public sealed class ActivityCollector(
    RestCaller rest,
    HttpClient http,
    FileResponseCache cache,
    string owner,
    string repository,
    ActivityWindow window,
    Action<string> log)
{
    private const int PageSize = 100;
    private const int MaxPages = 400;
    private static readonly TimeSpan PageDelay = TimeSpan.FromMilliseconds(150);
    private readonly EtagHttp raw = new(http, cache, log);

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
        return pullRequests;
    }

    // The issues endpoint drops page-based pagination on large repositories, so Octokit's
    // StartPage paging cannot walk it; this raw fetch follows Link cursors instead and is
    // made conditional (ETag) through the shared response cache. Authoritative counts come
    // from GetWeeklyCountsAsync.
    public async Task<(IReadOnlyList<IssueRow> Issues, int Items)> GetIssuesAsync()
    {
        var issues = new List<IssueRow>();
        var items = 0;
        var url = $"https://api.github.com/repos/{owner}/{repository}/issues?state=all&sort=created&direction=desc&per_page={PageSize}";
        for (var page = 1; url is not null && page <= MaxPages; page++)
        {
            using var response = await raw.GetAsync($"issues {page}", url);
            if (!response.IsSuccessStatusCode)
            {
                log($"issues {page}: HTTP {(int)response.StatusCode} {response.ReasonPhrase}, stopping");
                break;
            }
            using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            var parsed = document.RootElement.EnumerateArray().Select(ParseIssue).ToList();
            var inWindow = parsed.Where(entry => window.Contains(entry.Issue.CreatedAt)).ToList();
            items += inWindow.Count;
            issues.AddRange(inWindow.Where(entry => !entry.IsPullRequest).Select(entry => entry.Issue));
            log($"issues {page}: {inWindow.Count} items, {issues.Count} issues");
            if (parsed.Any(entry => entry.Issue.CreatedAt < window.From))
            {
                break;
            }
            url = EtagHttp.NextLink(response);
            await Task.Delay(PageDelay);
        }
        return (issues, items);
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

    public async Task<IReadOnlyList<WeeklyCounts>> GetWeeklyCountsAsync()
    {
        var counts = new List<WeeklyCounts>();
        foreach (var week in window.Weeks())
        {
            var range = Range(week);
            var issues = await SearchCountAsync($"repo:{owner}/{repository} type:issue created:{range}");
            var pullRequests = await SearchCountAsync($"repo:{owner}/{repository} type:pr created:{range}");
            counts.Add(new WeeklyCounts(week, issues, pullRequests));
            log($"week {week:yyyy-MM-dd}: issues {issues}, prs {pullRequests}");
        }
        return counts;
    }

    private Task<int> SearchCountAsync(string term) =>
        rest.Call(
            async () =>
            {
                var result = await rest.Client.Search.SearchIssues(new SearchIssuesRequest(term) { PerPage = 1 });
                return result.TotalCount;
            },
            search: true);

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

    private (IssueRow Issue, bool IsPullRequest) ParseIssue(JsonElement element)
    {
        var issue = new IssueRow(
            element.GetProperty("number").GetInt32(),
            element.GetProperty("created_at").GetDateTimeOffset(),
            element.TryGetProperty("updated_at", out var updated) && updated.ValueKind == JsonValueKind.String
                ? updated.GetDateTimeOffset()
                : null,
            element.TryGetProperty("state", out var state) ? state.GetString() ?? string.Empty : string.Empty,
            element.TryGetProperty("comments", out var comments) ? comments.GetInt32() : 0,
            element.TryGetProperty("title", out var title) ? title.GetString() ?? string.Empty : string.Empty,
            element.TryGetProperty("html_url", out var html) ? html.GetString() ?? string.Empty : string.Empty);
        return (issue, element.TryGetProperty("pull_request", out _));
    }

    private string Range(DateOnly week)
    {
        var first = DateOnly.FromDateTime(window.From.UtcDateTime);
        var last = DateOnly.FromDateTime(window.To.UtcDateTime);
        var start = week < first ? first : week;
        var end = week.AddDays(6) > last ? last : week.AddDays(6);
        return $"{start:yyyy-MM-dd}..{end:yyyy-MM-dd}";
    }

    private static string ShortSha(string sha) => sha.Length >= 7 ? sha[..7] : sha;
}
