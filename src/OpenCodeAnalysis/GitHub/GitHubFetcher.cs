using Octokit;
using GhCore;

namespace OpenCodeAnalysis;

/// Octokit wrapper for auth, rate limit, one method per REST endpoint.
public class GitHubFetcher
{
    public const string Owner = "anomalyco";
    public const string Repo = "opencode";
    const int PageSize = RestCaller.PageSize;

    readonly RestCaller _rest;
    readonly Cache _cache;

    public int ApiCalls => _rest.ApiCalls;

    public GitHubFetcher(Cache cache)
    {
        _cache = cache;
        var etag = new FileResponseCache(Path.Combine(cache.Root, "http-cache"), refresh: false);
        etag.Prepare();
        _rest = new RestCaller("OpenCodeAnalysis-2IRR80", TokenResolver.Resolve(), etag, Console.WriteLine);
    }

    static string Truncate(string? s, int max = 4000) => s is null ? "" : s.Length <= max ? s : s[..max];

    /// Extracts the issue/PR number from an html_url, like .../pull/123#discussion_r1 or .../issues/45#issuecomment-2.
    static int NumberFromUrl(string url)
    {
        var parts = url.Split('#')[0].Split('/');
        return int.Parse(parts[^1]);
    }

    public async Task<List<CommitRecord>> CommitsAsync(RunInfo run) =>
        (await _cache.Paged<CommitRecord>("commits",
            async page => (await _rest.Call(() => _rest.Client.Repository.Commit.GetAll(Owner, Repo,
                    new CommitRequest { Since = run.Since, Until = run.Until }, RestCaller.Page(page))))
                .Select(c => new CommitRecord(
                    c.Sha,
                    c.Author?.Login,
                    Bots.IsBot(c.Author?.Login ?? c.Commit.Author.Name, c.Author?.Type?.ToString()),
                    c.Commit.Author.Name,
                    c.Commit.Author.Email,
                    c.Commit.Author.Date,
                    Truncate(c.Commit.Message, 1000)))
                .ToList(),
            items => items.Count < PageSize)).DistinctBy(c => c.Sha).ToList();

    /// Commits before the analysis window, used to identify returning contributors.
    public Task<List<CommitRecord>> PriorCommitsAsync(DateTimeOffset from, DateTimeOffset until) =>
        _cache.GetOrFetch("prior_commits", async () =>
        {
            var commits = await _cache.Paged<CommitRecord>("prior_commits_pages",
                async page => (await _rest.Call(() => _rest.Client.Repository.Commit.GetAll(Owner, Repo,
                        new CommitRequest { Since = from, Until = until }, RestCaller.Page(page))))
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
            return commits.Where(c => c.Date >= from && c.Date < until)
                .DistinctBy(c => c.Sha).OrderBy(c => c.Date).ToList();
        });

    public async Task<List<PullRecord>> PullsAsync(RunInfo run)
    {
        var raw = await _cache.Paged<PullRecord>("pulls",
            async page => (await _rest.Call(() => _rest.Client.PullRequest.GetAllForRepository(Owner, Repo,
                    new PullRequestRequest
                    {
                        State = ItemStateFilter.All,
                        SortProperty = PullRequestSort.Created,
                        SortDirection = SortDirection.Descending,
                    }, RestCaller.Page(page))))
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
            async (since, page) => (await _rest.Call(() => _rest.Client.PullRequest.ReviewComment.GetAllForRepository(Owner, Repo,
                    new PullRequestReviewCommentRequest
                    {
                        Since = since,
                        Sort = PullRequestReviewCommentSort.Created,
                        Direction = SortDirection.Ascending,
                    }, RestCaller.Page(page))))
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
            async (since, page) => (await _rest.Call(() => _rest.Client.Issue.Comment.GetAllForRepository(Owner, Repo,
                    new IssueCommentRequest
                    {
                        Since = since,
                        Sort = IssueCommentSort.Created,
                        Direction = SortDirection.Ascending,
                    }, RestCaller.Page(page))))
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
            async page => (await _rest.Call(() => _rest.Client.Repository.GetAllContributors(Owner, Repo, true, RestCaller.Page(page))))
                .Select(c => new ContributorRecord(c.Login, null, c.Type?.ToString(), c.Contributions))
                .ToList(),
            items => items.Count < PageSize);

    static string Window(RunInfo run) => $"created:{run.Since:yyyy-MM-dd}..{run.Until.AddDays(-1):yyyy-MM-dd}";

    /// Number of PRs login submitted at least one review on
    public Task<ReviewCount> ReviewCountAsync(string login, RunInfo run) =>
        _cache.GetOrFetch($"review_counts/{login}", async () =>
        {
            var q = $"repo:{Owner}/{Repo} is:pr reviewed-by:{login} -author:{login} {Window(run)}";
            var result = await _rest.Call(() => _rest.Client.Search.SearchIssues(new SearchIssuesRequest(q) { PerPage = 1 }), search: true);
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
                var result = await _rest.Call(() => _rest.Client.Search.SearchIssues(
                    new SearchIssuesRequest(q) { PerPage = PageSize, Page = p }), search: true);
                total = result.TotalCount;
                numbers.AddRange(result.Items.Select(i => i.Number));
                if (result.Items.Count < PageSize || numbers.Count >= total) break;
            }
            return new PairReviews(author, reviewer, total, numbers.Distinct().Order().ToList());
        });

    public Task<List<ReviewRecord>> ReviewsAsync(int number, string prAuthor) =>
        _cache.GetOrFetch($"reviews/{number}", async () =>
            (await _rest.Call(() => _rest.Client.PullRequest.Review.GetAll(Owner, Repo, number)))
            .Where(r => r.User is not null)
            .Select(r => new ReviewRecord(number, prAuthor, r.User.Login, r.State.StringValue,
                r.SubmittedAt, Truncate(r.Body), r.HtmlUrl))
            .ToList());

    /// Issues and PRs of issues since repo was created, ordered by last update.
    public async Task<List<IssueRecord>> IssueItemsAsync(DateTimeOffset repoCreated)
    {
        var raw = await _cache.Segmented<IssueRecord>("issue_items", repoCreated,
            async (since, page) => (await _rest.Call(() => _rest.Client.Issue.GetAllForRepository(Owner, Repo,
                    new RepositoryIssueRequest
                    {
                        State = ItemStateFilter.All,
                        Since = since,
                        SortProperty = IssueSort.Updated,
                        SortDirection = SortDirection.Ascending,
                    }, RestCaller.Page(page))))
                .Select(i => new IssueRecord(
                    i.Number,
                    i.User?.Login ?? "ghost",
                    Bots.IsBot(i.User?.Login, i.User?.Type?.ToString()),
                    i.PullRequest is not null,
                    i.CreatedAt,
                    i.UpdatedAt ?? i.CreatedAt,
                    i.ClosedAt,
                    i.State.StringValue,
                    i.StateReason?.StringValue,
                    Truncate(i.Title, 200),
                    i.HtmlUrl))
                .ToList(),
            i => i.UpdatedAt,
            items => items.Count < PageSize,
            maxPages: 99); // api refuses page numbers beyond 10,000 items, so continue from the last update
        // Items updated during the fetch appear twice and the last copy is the newest
        return raw.AsEnumerable().Reverse().DistinctBy(i => i.Number).OrderBy(i => i.Number).ToList();
    }

    /// Issues opened inside the window.
    public async Task<List<IssueRecord>> IssuesAsync(RunInfo run, DateTimeOffset repoCreated) =>
        (await IssueItemsAsync(repoCreated))
        .Where(i => !i.IsPull && i.CreatedAt >= run.Since && i.CreatedAt < run.Until)
        .ToList();

    /// PRs opened before the window, to differentiate newcomers from returning contributors
    public async Task<List<IssueRecord>> PullsBeforeWindowAsync(RunInfo run, DateTimeOffset repoCreated) =>
        (await IssueItemsAsync(repoCreated))
        .Where(i => i.IsPull && i.CreatedAt < run.Since)
        .ToList();

    /// Account profile (type, creation date, bio) for accounts under review
    public Task<UserProfile> UserAsync(string login) =>
        _cache.GetOrFetch($"users/{login}", async () =>
        {
            // GraphQL review data drops the [bot] suffix of apps
            foreach (var name in new[] { login, login + "[bot]" }.Distinct())
            {
                // Deleted or renamed accounts return 404
                try
                {
                    var u = await _rest.Call(() => _rest.Client.User.Get(name));
                    return new UserProfile(login, u.Type?.ToString() ?? "", u.CreatedAt, u.Name, Truncate(u.Bio, 300),
                        u.PublicRepos, u.Followers);
                }
                catch (NotFoundException)
                {
                }
            }
            return new UserProfile(login, "NotFound", default, null, null, 0, 0);
        });

    public Task<List<PrFileRecord>> FilesAsync(int number, string prAuthor) =>
        _cache.GetOrFetch($"files/{number}", () => FetchFiles(number, prAuthor));

    public Task<List<string>> PracticeTreeAsync(string reference) =>
        _cache.GetOrFetch($"practices/trees/{reference.Replace('/', '_')}", () => FetchTree(reference));

    public Task<List<string>> ReturningAuthorsAsync(RunInfo run) =>
        _cache.GetOrFetch("returning_authors", async () =>
        {
            var query = $"repo:{Owner}/{Repo} is:pr created:<{run.Since:yyyy-MM-dd}";
            var logins = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (var page = 1; page <= 10; page++)
            {
                var p = page;
                var result = await _rest.Call(() => _rest.Client.Search.SearchIssues(
                    new SearchIssuesRequest(query) { PerPage = PageSize, Page = p }), search: true);
                if (result.TotalCount > 1000)
                    throw new InvalidOperationException("returning-author search exceeds the 1000-result cap");
                foreach (var item in result.Items)
                {
                    if (!string.IsNullOrEmpty(item.User?.Login) &&
                        !Bots.IsBot(item.User.Login, item.User.Type?.ToString()))
                        logins.Add(item.User.Login);
                }
                if (result.Items.Count < PageSize) break;
            }
            return logins.OrderBy(l => l, StringComparer.OrdinalIgnoreCase).ToList();
        });

    public Task<List<string>> TeamAsync(string reference) =>
        _cache.GetOrFetch($"practices/team/{reference}", async () =>
        {
            var file = (await _rest.Call(() => _rest.Client.Repository.Content.GetAllContentsByRef(
                Owner, Repo, ".github/TEAM_MEMBERS", reference))).Single();
            return file.Content
                .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(l => l.ToLowerInvariant())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        });

    public Task<int> PracticeCommitsAheadAsync(string baseRef, string headRef) =>
        _cache.GetOrFetch($"practices/compare/{baseRef}_{headRef}",
            async () => (await _rest.Call(() => _rest.Client.Repository.Commit.Compare(Owner, Repo, baseRef, headRef))).AheadBy);

    async Task<List<PrFileRecord>> FetchFiles(int number, string prAuthor) =>
        (await _rest.Call(() => _rest.Client.PullRequest.Files(Owner, Repo, number)))
        .Select(f => new PrFileRecord(number, prAuthor, f.FileName, f.Additions, f.Deletions))
        .ToList();

    async Task<List<string>> FetchTree(string reference) =>
        (await _rest.Call(() => _rest.Client.Git.Tree.GetRecursive(Owner, Repo, reference)))
        .Tree.Where(t => t.Type.Value == TreeType.Blob).Select(t => t.Path).ToList();
}
