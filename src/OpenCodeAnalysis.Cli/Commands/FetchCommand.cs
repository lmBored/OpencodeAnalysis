using OpenCodeAnalysis;

namespace OpenCodeAnalysis.Cli;

/// Downloads everything the reports need, page by page, into the JSON cache.
public sealed class FetchCommand : ICommand
{
    const int FilesSamplePerAuthor = 150;

    public string Name => "fetch";

    public async Task RunAsync(CommandContext context)
    {
        var cache = context.Cache;
        var run = context.Run;
        var gh = new GitHubFetcher(cache);

        Console.WriteLine("Returning authors ...");
        var returning = await gh.ReturningAuthorsAsync(run);
        Console.WriteLine($"  {returning.Count} authors with PRs before the window");

        Console.WriteLine("Team members ...");
        var team = await gh.TeamAsync(Pins.Pin);
        Console.WriteLine($"  {team.Count} members");

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

        var top = ContributorAnalysis.TopHumans(ContributorAnalysis.Rows(commits, pulls, reviewCounts), context.TopN)
            .Select(r => r.Login).ToList();
        Console.WriteLine($"Top-{context.TopN} humans: {string.Join(", ", top)}");

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

        // Full review lists only for pair PRs where the reviewer also commented.
        var commented = reviewComments.Concat(issueComments)
            .Select(c => (c.Number, c.Login)).ToHashSet();
        var authorOf = pulls.ToDictionary(p => p.Number, p => p.Login);
        var targeted = pairs
            .SelectMany(p => p.PrNumbers.Where(n => commented.Contains((n, p.Reviewer))))
            .Distinct().Order().ToList();
        Console.WriteLine($"Reviews for {targeted.Count} targeted pair PRs ...");
        foreach (var n in targeted)
            await gh.ReviewsAsync(n, authorOf.GetValueOrDefault(n, "?"));

        // Random sample of merged PRs per top contributor, for the co-editing analysis.
        var sample = top.SelectMany(login => Sampling.RandomSample(
                pulls.Where(p => p.Login.Equals(login, StringComparison.OrdinalIgnoreCase) && p.MergedAt is not null),
                FilesSamplePerAuthor, run.Seed, p => p.Number))
            .ToList();
        Console.WriteLine($"Changed files for {sample.Count} sampled merged PRs ...");
        foreach (var p in sample)
            await gh.FilesAsync(p.Number, p.Login);

        cache.Write("top", top);
        cache.Write("files_sample", sample.Select(p => p.Number).ToList());
        cache.Write("targeted", targeted);
        Console.WriteLine($"Fetch complete; API calls made in this run: {gh.ApiCalls}");
    }
}
