using System.Diagnostics;
using Octokit;

namespace OpenCodeAnalysis;

/// Octokit wrapper for auth, rate limit, one method per REST endpoint.
public class GitHubFetcher
{
    public const string Owner = "anomalyco";
    public const string Repo = "opencode";
    const int PageSize = 100;

    readonly GitHubClient _client;
    readonly Cache _cache;
    DateTimeOffset _lastSearch = DateTimeOffset.MinValue;

    public int ApiCalls { get; private set; }

    public GitHubFetcher(Cache cache)
    {
        _cache = cache;
        _client = new GitHubClient(new ProductHeaderValue("OpenCodeAnalysis-2IRR80"))
        {
            Credentials = new Credentials(ResolveToken()),
        };
    }

    static string ResolveToken()
    {
        var env = Environment.GetEnvironmentVariable("GITHUB_TOKEN");
        if (!string.IsNullOrWhiteSpace(env)) return env.Trim();
        var psi = new ProcessStartInfo("gh", "auth token") { RedirectStandardOutput = true, UseShellExecute = false };
        using var p = Process.Start(psi) ?? throw new InvalidOperationException("`gh` not working.");
        var token = p.StandardOutput.ReadToEnd().Trim();
        p.WaitForExit();
        if (string.IsNullOrEmpty(token)) throw new InvalidOperationException("Set GITHUB_TOKEN or run `gh auth login`.");
        return token;
    }

    /// One API call, waiting for the rate limit reset and retrying.
    async Task<T> Call<T>(Func<Task<T>> call, bool search = false)
    {
        for (var attempt = 1; ; attempt++)
        {
            if (search)
            {
                // Search API: 30 requests/minute.
                var wait = _lastSearch.AddSeconds(2.2) - DateTimeOffset.UtcNow;
                if (wait > TimeSpan.Zero) await Task.Delay(wait);
                _lastSearch = DateTimeOffset.UtcNow;
            }
            try
            {
                ApiCalls++;
                var result = await call();
                var rate = _client.GetLastApiInfo()?.RateLimit;
                if (!search && rate is { Remaining: < 25 }) await SleepUntil(rate.Reset, "primary rate limit low");
                return result;
            }
            catch (RateLimitExceededException e)
            {
                await SleepUntil(e.Reset, "primary rate limit exceeded");
            }
            catch (SecondaryRateLimitExceededException) when (attempt < 8)
            {
                await SleepFor(TimeSpan.FromSeconds(60 * attempt), "secondary rate limit");
            }
            catch (AbuseException e) when (attempt < 8)
            {
                await SleepFor(TimeSpan.FromSeconds(e.RetryAfterSeconds ?? 60), "abuse detection");
            }
            catch (ApiException e) when (attempt < 5 && (int)e.StatusCode >= 500)
            {
                await SleepFor(TimeSpan.FromSeconds(5 * attempt), $"server error {(int)e.StatusCode}");
            }
            catch (HttpRequestException) when (attempt < 5)
            {
                await SleepFor(TimeSpan.FromSeconds(5 * attempt), "network error");
            }
        }
    }

    static Task SleepUntil(DateTimeOffset reset, string why) =>
        SleepFor(reset - DateTimeOffset.UtcNow + TimeSpan.FromSeconds(5), why);

    static async Task SleepFor(TimeSpan span, string why)
    {
        if (span < TimeSpan.Zero) return;
        Console.WriteLine($"  [{why}] sleeping {span.TotalMinutes:F1} min");
        await Task.Delay(span);
    }

    static ApiOptions Page(int page) => new() { PageSize = PageSize, PageCount = 1, StartPage = page };

    static string Truncate(string? s, int max = 4000) => s is null ? "" : s.Length <= max ? s : s[..max];

    /// Extracts the issue/PR number from an html_url, like .../pull/123#discussion_r1 or .../issues/45#issuecomment-2.
    static int NumberFromUrl(string url)
    {
        var parts = url.Split('#')[0].Split('/');
        return int.Parse(parts[^1]);
    }

    public Task<List<CommitRecord>> CommitsAsync(RunInfo run) =>
        _cache.Paged<CommitRecord>("commits",
            async page => (await Call(() => _client.Repository.Commit.GetAll(Owner, Repo,
                    new CommitRequest { Since = run.Since, Until = run.Until }, Page(page))))
                .Select(c => new CommitRecord(
                    c.Sha,
                    c.Author?.Login,
                    Bots.IsBot(c.Author?.Login ?? c.Commit.Author.Name, c.Author?.Type?.ToString()),
                    c.Commit.Author.Name,
                    c.Commit.Author.Email,
                    c.Commit.Author.Date,
                    Truncate(c.Commit.Message, 1000)))
                .ToList(),
            items => items.Count < PageSize);

    public async Task<List<PullRecord>> PullsAsync(RunInfo run)
    {
        var raw = await _cache.Paged<PullRecord>("pulls",
            async page => (await Call(() => _client.PullRequest.GetAllForRepository(Owner, Repo,
                    new PullRequestRequest
                    {
                        State = ItemStateFilter.All,
                        SortProperty = PullRequestSort.Created,
                        SortDirection = SortDirection.Descending,
                    }, Page(page))))
                .Select(p => new PullRecord(
                    p.Number,
                    p.User.Login,
                    Bots.IsBot(p.User.Login, p.User.Type?.ToString()),
                    p.CreatedAt,
                    p.MergedAt,
                    p.State.StringValue,
                    p.Title,
                    p.HtmlUrl))
                .ToList(),
            items => items.Count < PageSize || items[^1].CreatedAt < run.Since);
        // New PRs shift pages during long fetch, which produces duplicates, so we drop them.
        return raw.Where(p => p.CreatedAt >= run.Since && p.CreatedAt < run.Until)
            .DistinctBy(p => p.Number)
            .ToList();
    }

    public async Task<List<CommentRecord>> ReviewCommentsAsync(RunInfo run)
    {
        var raw = await _cache.Segmented<CommentRecord>("review_comments", run.Since,
            async (since, page) => (await Call(() => _client.PullRequest.ReviewComment.GetAllForRepository(Owner, Repo,
                    new PullRequestReviewCommentRequest
                    {
                        Since = since,
                        Sort = PullRequestReviewCommentSort.Created,
                        Direction = SortDirection.Ascending,
                    }, Page(page))))
                .Where(c => c.User is not null)
                .Select(c => new CommentRecord("review", NumberFromUrl(c.PullRequestUrl), c.User.Login,
                    Bots.IsBot(c.User.Login, c.User.Type?.ToString()), c.Path, Truncate(c.Body), c.CreatedAt, c.HtmlUrl))
                .ToList(),
            c => c.CreatedAt,
            items => items.Count < PageSize || items[^1].CreatedAt >= run.Until);
        return raw.Where(c => c.CreatedAt >= run.Since && c.CreatedAt < run.Until).DistinctBy(c => c.Url).ToList();
    }

    public async Task<List<CommentRecord>> IssueCommentsAsync(RunInfo run)
    {
        var raw = await _cache.Segmented<CommentRecord>("issue_comments", run.Since,
            async (since, page) => (await Call(() => _client.Issue.Comment.GetAllForRepository(Owner, Repo,
                    new IssueCommentRequest
                    {
                        Since = since,
                        Sort = IssueCommentSort.Created,
                        Direction = SortDirection.Ascending,
                    }, Page(page))))
                .Where(c => c.User is not null)
                .Select(c => new CommentRecord("issue", NumberFromUrl(c.HtmlUrl), c.User.Login,
                    Bots.IsBot(c.User.Login, c.User.Type?.ToString()), null, Truncate(c.Body), c.CreatedAt, c.HtmlUrl))
                .ToList(),
            c => c.CreatedAt,
            items => items.Count < PageSize || items[^1].CreatedAt >= run.Until);
        return raw.Where(c => c.CreatedAt >= run.Since && c.CreatedAt < run.Until).DistinctBy(c => c.Url).ToList();
    }

    /// All time contributors as reported by GitHub
    /// (anonymous = commit emails not linked to account)
    public Task<List<ContributorRecord>> ContributorsAsync() =>
        _cache.Paged<ContributorRecord>("contributors",
            async page => (await Call(() => _client.Repository.GetAllContributors(Owner, Repo, true, Page(page))))
                .Select(c => new ContributorRecord(c.Login, null, c.Type?.ToString(), c.Contributions))
                .ToList(),
            items => items.Count < PageSize);

    static string Window(RunInfo run) => $"created:{run.Since:yyyy-MM-dd}..{run.Until.AddDays(-1):yyyy-MM-dd}";

    /// Number of PRs login submitted at least one review on
    public Task<ReviewCount> ReviewCountAsync(string login, RunInfo run) =>
        _cache.GetOrFetch($"review_counts/{login}", async () =>
        {
            var q = $"repo:{Owner}/{Repo} is:pr reviewed-by:{login} -author:{login} {Window(run)}";
            var result = await Call(() => _client.Search.SearchIssues(new SearchIssuesRequest(q) { PerPage = 1 }), search: true);
            return new ReviewCount(login, result.TotalCount);
        });

    /// PRs authored by `author` and reviewed by `reviewer`
    public Task<PairReviews> PairReviewsAsync(string author, string reviewer, RunInfo run) =>
        _cache.GetOrFetch($"pairs/{author}__{reviewer}", async () =>
        {
            var q = $"repo:{Owner}/{Repo} is:pr author:{author} reviewed-by:{reviewer} {Window(run)}";
            var numbers = new List<int>();
            var total = 0;
            for (var page = 1; page <= 10; page++)
            {
                var p = page;
                var result = await Call(() => _client.Search.SearchIssues(
                    new SearchIssuesRequest(q) { PerPage = PageSize, Page = p }), search: true);
                total = result.TotalCount;
                numbers.AddRange(result.Items.Select(i => i.Number));
                if (result.Items.Count < PageSize || numbers.Count >= total) break;
            }
            return new PairReviews(author, reviewer, total, numbers.Distinct().Order().ToList());
        });

    public Task<List<ReviewRecord>> ReviewsAsync(int number, string prAuthor) =>
        _cache.GetOrFetch($"reviews/{number}", async () =>
            (await Call(() => _client.PullRequest.Review.GetAll(Owner, Repo, number)))
            .Where(r => r.User is not null)
            .Select(r => new ReviewRecord(number, prAuthor, r.User.Login, r.State.StringValue,
                r.SubmittedAt, Truncate(r.Body), r.HtmlUrl))
            .ToList());

    public Task<List<PrFileRecord>> FilesAsync(int number, string prAuthor) =>
        _cache.GetOrFetch($"files/{number}", async () =>
            (await Call(() => _client.PullRequest.Files(Owner, Repo, number)))
            .Select(f => new PrFileRecord(number, prAuthor, f.FileName, f.Additions, f.Deletions))
            .ToList());
}
