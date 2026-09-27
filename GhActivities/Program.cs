using System.Globalization;
using System.Text;
using System.Text.Json;
using GhActivities;
using GhCore;

const string Owner = "anomalyco";
const string Repository = "opencode";
const string ProductHeader = "capstone-ch6";

var outDir = args.FirstOrDefault(argument => !argument.StartsWith("--", StringComparison.Ordinal)) ?? "data/activities";
Directory.CreateDirectory(outDir);
using var log = new RunLog(Path.Combine(outDir, "run.log"));
var started = DateTime.UtcNow;

try
{
    var token = TokenResolver.TryResolve();
    if (token is null)
    {
        log.Write("no GitHub token found (set GITHUB_TOKEN or authenticate with gh)");
        return 1;
    }

    var window = ParseWindow(args);
    var refresh = args.Any(argument => argument.Equals("--refresh", StringComparison.Ordinal));
    log.Write($"window {window.From:yyyy-MM-dd}..{window.To:yyyy-MM-dd} UTC{(refresh ? " (refresh: cache bypassed)" : string.Empty)}");

    using var http = new HttpClient();
    http.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
    http.DefaultRequestHeaders.Add("User-Agent", ProductHeader);
    http.DefaultRequestHeaders.Add("Accept", "application/vnd.github+json");

    var cache = new FileResponseCache(Path.Combine(outDir, "http-cache"), refresh);
    cache.Prepare();

    var rest = new RestCaller(ProductHeader, token, cache, log.Write);
    var collector = new ActivityCollector(rest, http, cache, Owner, Repository, window, log.Write);

    var repo = await collector.GetRepoAsync();
    File.WriteAllText(Path.Combine(outDir, "repo.json"), JsonSerializer.Serialize(
        new
        {
            stargazers_count = repo.Stars,
            forks_count = repo.Forks,
            open_issues_count = repo.OpenIssues,
            size = repo.SizeKb,
            has_discussions = repo.HasDiscussions,
            created_at = repo.CreatedAt,
            default_branch = repo.DefaultBranch,
        },
        new JsonSerializerOptions { WriteIndented = true }));
    log.Write($"repo {Owner}/{Repository}: {repo.Stars} stars, {repo.Forks} forks, discussions={repo.HasDiscussions}, default branch {repo.DefaultBranch}");

    var commitActivity = await GetCommitActivityAsync(collector, log);
    if (commitActivity is not null)
    {
        File.WriteAllText(Path.Combine(outDir, "commit_activity.json"), commitActivity);
    }

    var commits = await collector.GetCommitsAsync();
    WriteCommits(outDir, commits);

    var pullRequests = await collector.GetPullRequestsAsync();
    WritePullRequests(outDir, pullRequests);

    var (issues, issueItems) = await collector.GetIssuesAsync();
    WriteIssues(outDir, issues);

    var releases = await collector.GetReleasesAsync();
    WriteReleases(outDir, releases);

    var weeklyCounts = await collector.GetWeeklyCountsAsync();
    WriteWeeklyCounts(outDir, weeklyCounts);

    var prAuthors = pullRequests.ToDictionary(pr => pr.Number, pr => pr.Author);
    var reviews = await new ReviewFetcher(Owner, Repository, token, log.Write).GetReviewsAsync(window, prAuthors);
    WriteReviews(outDir, reviews);

    var summary = BuildSummary(repo, window, commits, pullRequests, issues, issueItems, releases, reviews, started);
    File.WriteAllText(Path.Combine(outDir, "summary.txt"), summary);
    log.Write(summary);
    log.Write("done");
    return 0;
}
catch (Exception exception)
{
    log.Write($"fatal: {exception.GetType().Name}: {exception.Message}");
    return 1;
}

static ActivityWindow ParseWindow(string[] args)
{
    var from = new DateTimeOffset(2025, 9, 27, 0, 0, 0, TimeSpan.Zero);
    var to = new DateTimeOffset(2026, 9, 27, 23, 59, 59, TimeSpan.Zero);
    foreach (var argument in args.Where(argument => argument.StartsWith("--", StringComparison.Ordinal)))
    {
        if (argument.StartsWith("--from=", StringComparison.Ordinal))
            from = StartOfDay(argument[7..]);
        else if (argument.StartsWith("--to=", StringComparison.Ordinal))
            to = EndOfDay(argument[5..]);
    }
    return new ActivityWindow(from, to);
}

static DateTimeOffset StartOfDay(string value) =>
    new(DateOnly.Parse(value, CultureInfo.InvariantCulture).ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);

static DateTimeOffset EndOfDay(string value) =>
    new(DateOnly.Parse(value, CultureInfo.InvariantCulture).ToDateTime(new TimeOnly(23, 59, 59)), TimeSpan.Zero);

static async Task<string?> GetCommitActivityAsync(ActivityCollector collector, RunLog log)
{
    try
    {
        return await collector.GetCommitActivityJsonAsync();
    }
    catch (Exception exception)
    {
        log.Write(
            $"commit_activity unavailable: {exception.GetType().Name}: {exception.Message.Split('\n')[0]} - keeping any existing file");
        return null;
    }
}

static void WriteCommits(string outDir, IReadOnlyList<CommitRow> commits) =>
    CsvWriter.Write(
        Path.Combine(outDir, "commits.csv"),
        ["committer_date", "author_date", "sha"],
        commits.Select(commit => new[] { Iso(commit.CommittedAt), Iso(commit.AuthoredAt), commit.Sha }));

static void WritePullRequests(string outDir, IReadOnlyList<PullRequestRow> pullRequests) =>
    CsvWriter.Write(
        Path.Combine(outDir, "prs.csv"),
        ["number", "created_at", "merged_at", "state", "merged", "title", "url", "author"],
        pullRequests.Select(pr => new[]
        {
            pr.Number.ToString(CultureInfo.InvariantCulture),
            Iso(pr.CreatedAt),
            pr.MergedAt is { } merged ? Iso(merged) : string.Empty,
            pr.State,
            pr.MergedAt is null ? string.Empty : "merged",
            pr.Title,
            pr.Url,
            pr.Author,
        }));

static void WriteIssues(string outDir, IReadOnlyList<IssueRow> issues) =>
    CsvWriter.Write(
        Path.Combine(outDir, "issues.csv"),
        ["number", "created_at", "updated_at", "state", "comments", "title", "url"],
        issues.Select(issue => new[]
        {
            issue.Number.ToString(CultureInfo.InvariantCulture),
            Iso(issue.CreatedAt),
            issue.UpdatedAt is { } updated ? Iso(updated) : string.Empty,
            issue.State,
            issue.Comments.ToString(CultureInfo.InvariantCulture),
            issue.Title,
            issue.Url,
        }));

static void WriteReleases(string outDir, IReadOnlyList<ReleaseRow> releases) =>
    CsvWriter.Write(
        Path.Combine(outDir, "releases.csv"),
        ["tag", "name", "published_at", "prerelease", "url"],
        releases.Select(release => new[]
        {
            release.Tag,
            release.Name,
            Iso(release.PublishedAt),
            release.Prerelease ? "true" : "false",
            release.Url,
        }));

static void WriteWeeklyCounts(string outDir, IReadOnlyList<WeeklyCounts> counts) =>
    CsvWriter.Write(
        Path.Combine(outDir, "search_weekly.csv"),
        ["week_start", "issues", "prs"],
        counts.Select(count => new[]
        {
            count.WeekStart.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            count.Issues.ToString(CultureInfo.InvariantCulture),
            count.PullRequests.ToString(CultureInfo.InvariantCulture),
        }));

static void WriteReviews(string outDir, IReadOnlyList<ReviewRow> reviews) =>
    CsvWriter.Write(
        Path.Combine(outDir, "reviews.csv"),
        ["pull_request", "submitted_at", "state", "author", "pr_author", "body", "url"],
        reviews.Select(review => new[]
        {
            review.PullRequest.ToString(CultureInfo.InvariantCulture),
            Iso(review.SubmittedAt),
            review.State,
            review.Author,
            review.PrAuthor,
            review.Body,
            review.Url,
        }));

static string Iso(DateTimeOffset value) => value.ToString("yyyy-MM-dd'T'HH:mm:sszzz", CultureInfo.InvariantCulture);

string BuildSummary(
    RepoSnapshot repo,
    ActivityWindow window,
    IReadOnlyList<CommitRow> commits,
    IReadOnlyList<PullRequestRow> pullRequests,
    IReadOnlyList<IssueRow> issues,
    int issueItems,
    IReadOnlyList<ReleaseRow> releases,
    IReadOnlyList<ReviewRow> reviews,
    DateTime started)
{
    var merged = pullRequests.Count(pr => pr.MergedAt is not null);
    var reviewedPrs = reviews.Select(review => review.PullRequest).Distinct().Count();
    var lines = new[]
    {
        $"repo: {Owner}/{Repository} (default branch {repo.DefaultBranch})",
        $"window: {window.From:yyyy-MM-dd}..{window.To:yyyy-MM-dd} (UTC)",
        $"commits: {commits.Count}",
        $"pull_requests: {pullRequests.Count} ({merged} merged)",
        $"issue_details: {issues.Count}",
        $"issue_endpoint_items: {issueItems} (issues and PRs seen by the issues endpoint)",
        $"releases: {releases.Count}",
        $"reviews: {reviews.Count} submitted in window on {reviewedPrs} pull requests (GraphQL, in-window PRs only)",
        $"discussions: {(repo.HasDiscussions ? "feature enabled" : "0 (has_discussions=false)")}",
        $"repo_created_at: {repo.CreatedAt:yyyy-MM-dd}",
        $"stars: {repo.Stars}, forks: {repo.Forks}, open_issues_count: {repo.OpenIssues}, size_kb: {repo.SizeKb}",
        $"elapsed: {(DateTime.UtcNow - started).TotalMinutes:0.0} min",
    };
    return string.Join(Environment.NewLine, lines);
}

sealed class RunLog : IDisposable
{
    private readonly StreamWriter writer;

    public RunLog(string path) =>
        writer = new StreamWriter(path, false, new UTF8Encoding(false)) { AutoFlush = true };

    public void Write(string message)
    {
        var line = $"[{DateTime.Now:HH:mm:ss}] {message}";
        try
        {
            Console.WriteLine(line);
        }
        catch (IOException)
        {
        }
        writer.WriteLine(line);
    }

    public void Dispose() => writer.Dispose();
}
