using OpenCodeAnalysis;

const int Seed = 42;
const int TopN = 10;
const int FilesSamplePerAuthor = 150;
var RepoCreated = new DateTimeOffset(2025, 4, 1, 0, 0, 0, TimeSpan.Zero);

var projectDir = FindProjectDir();
var cache = new Cache(Path.Combine(projectDir, "data"));
var outDir = Path.Combine(projectDir, "out");
Directory.CreateDirectory(outDir);

var command = args.FirstOrDefault() ?? "all";

var run = cache.GetOrFetch("run", () =>
{
    var until = new DateTimeOffset(DateTime.UtcNow.Date, TimeSpan.Zero);
    return Task.FromResult(new RunInfo(DateTimeOffset.UtcNow, until.AddDays(-365), until, Seed));
}).Result;
Console.WriteLine($"Window: {run.Since:yyyy-MM-dd} .. {run.Until:yyyy-MM-dd} (exclusive), collected {run.CollectedAt:u}, seed {run.Seed}");

switch (command)
{
    case "fetch":
        await Fetch();
        break;
    case "contributors":
        Reports.Contributors(Dataset.Load(cache), run, outDir, TopN);
        break;
    case "collaboration":
        Reports.Collaboration(Dataset.Load(cache), run, outDir, TopN);
        break;
    case "activities":
        Reports.Activities(Dataset.Load(cache), run, outDir, TopN);
        break;
    case "fetch-health":
        await FetchHealth();
        break;
    case "all":
        await Fetch();
        var data = Dataset.Load(cache);
        Reports.Contributors(data, run, outDir, TopN);
        Reports.Collaboration(data, run, outDir, TopN);
        break;
    default:
        Console.Error.WriteLine("usage: dotnet run -- [fetch|contributors|collaboration|activities|fetch-health|all]");
        return 1;
}
return 0;

async Task Fetch()
{
    var gh = new GitHubFetcher(cache);

    Console.WriteLine("Commits ...");
    var commits = await gh.CommitsAsync(run);
    Console.WriteLine($"  {commits.Count} commits");

    Console.WriteLine("Pull requests ...");
    var pulls = await gh.PullsAsync(run);
    Console.WriteLine($"  {pulls.Count} PRs created in window");

    Console.WriteLine("Contributors (all time) ...");
    var contributors = await gh.ContributorsAsync();
    Console.WriteLine($"  {contributors.Count} contributor entries");

    var candidates = ContributorAnalysis.Candidates(commits, pulls);
    Console.WriteLine($"Review counts for {candidates.Count} candidates (search API) ...");
    var reviewCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
    foreach (var login in candidates)
        reviewCounts[login] = (await gh.ReviewCountAsync(login, run)).PrsReviewed;

    var top = ContributorAnalysis.TopHumans(ContributorAnalysis.Rows(commits, pulls, reviewCounts), TopN)
        .Select(r => r.Login).ToList();
    Console.WriteLine($"Top-{TopN} humans: {string.Join(", ", top)}");

    Console.WriteLine("Author x reviewer pairs among the top humans (search API) ...");
    var pairs = new List<PairReviews>();
    foreach (var author in top)
    foreach (var reviewer in top.Where(r => r != author))
        pairs.Add(await gh.PairReviewsAsync(author, reviewer, run));

    Console.WriteLine("Review comments (repo-wide) ...");
    var reviewComments = await gh.ReviewCommentsAsync(run);
    Console.WriteLine($"  {reviewComments.Count} review comments");

    Console.WriteLine("Issue/PR conversation comments (repo-wide) ...");
    var issueComments = await gh.IssueCommentsAsync(run);
    Console.WriteLine($"  {issueComments.Count} conversation comments");

    // Targeted sampling
    // Full review lists only for pair PRs where the reviewer also wrote at least one comment.
    var commented = reviewComments.Concat(issueComments)
        .Select(c => (c.Number, c.Login)).ToHashSet();
    var authorOf = pulls.ToDictionary(p => p.Number, p => p.Login);
    var targeted = pairs
        .SelectMany(p => p.PrNumbers.Where(n => commented.Contains((n, p.Reviewer))))
        .Distinct().Order().ToList();
    Console.WriteLine($"Reviews for {targeted.Count} targeted pair PRs ...");
    foreach (var n in targeted)
        await gh.ReviewsAsync(n, authorOf.GetValueOrDefault(n, "?"));

    // Random sample of merged PRs each top contirbutor for data on co-editing (editing the same file)
    var sample = top.SelectMany(login => Sampling.RandomSample(
            pulls.Where(p => p.Login.Equals(login, StringComparison.OrdinalIgnoreCase) && p.MergedAt is not null),
            FilesSamplePerAuthor, Seed, p => p.Number))
        .ToList();
    Console.WriteLine($"Changed files for {sample.Count} sampled merged PRs ...");
    foreach (var p in sample)
        await gh.FilesAsync(p.Number, p.Login);

    cache.Write("top", top);
    cache.Write("files_sample", sample.Select(p => p.Number).ToList());
    cache.Write("targeted", targeted);
    Console.WriteLine($"Fetch complete; API calls made in this run: {gh.ApiCalls}");
}

/// Health part: issues, PRs before the window, commits before the window, profiles of reviewed accounts.
async Task FetchHealth()
{
    var gh = new GitHubFetcher(cache);

    Console.WriteLine("Issues and PRs, all time (issues endpoint) ...");
    var issues = await gh.IssuesAsync(run, RepoCreated);
    var prior = await gh.PullsBeforeWindowAsync(run, RepoCreated);
    Console.WriteLine($"  {issues.Count} issues opened in window, {prior.Count} PRs opened before the window");

    Console.WriteLine("Commits before the window (local clone) ...");
    var priorCommits = cache.GetOrFetch("prior_commits", () => Task.FromResult(LocalCommits(run.Since))).Result;
    Console.WriteLine($"  {priorCommits.Count} commits before {run.Since:yyyy-MM-dd}");

    var data = Dataset.Load(cache);
    var overrides = IdentityOverrides.Load(Path.Combine(projectDir, "data", "identity_overrides.json"));
    var review = BotDetector.Evaluate(data, run, overrides).ReviewList;
    Console.WriteLine($"Profiles of {review.Count} accounts under review ...");
    foreach (var login in review)
        await gh.UserAsync(login);
    Console.WriteLine($"Health fetch complete; API calls made in this run: {gh.ApiCalls}");
}

/// Commits of local clone authored before `until`
List<CommitRecord> LocalCommits(DateTimeOffset until)
{
    var psi = new System.Diagnostics.ProcessStartInfo("git",
        ["-C", Path.Combine(projectDir, "opencode"), "log", $"--before={until:yyyy-MM-ddTHH:mm:ssZ}",
            "--format=%H%x1f%aN%x1f%aE%x1f%aI%x1f%s"])
    {
        RedirectStandardOutput = true,
    };
    using var git = System.Diagnostics.Process.Start(psi)!;
    var lines = git.StandardOutput.ReadToEnd().Split('\n', StringSplitOptions.RemoveEmptyEntries);
    git.WaitForExit();
    return lines.Select(l => l.Split('\u001f'))
        .Where(f => f.Length >= 5 && DateTimeOffset.Parse(f[3]) < until)
        .Select(f => new CommitRecord(f[0], null, GhCore.Bots.IsBot(f[1]), f[1], f[2], DateTimeOffset.Parse(f[3]), f[4]))
        .ToList();
}

static string FindProjectDir()
{
    var dir = new DirectoryInfo(AppContext.BaseDirectory);
    while (dir is not null && !dir.EnumerateFiles("*.csproj").Any()) dir = dir.Parent;
    return dir?.FullName ?? Directory.GetCurrentDirectory();
}
