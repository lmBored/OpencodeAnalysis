using OpenCodeAnalysis;

namespace OpenCodeAnalysis.Cli;

/// Downloads the extra inputs needed by the health report.
public sealed class FetchHealthCommand : ICommand
{
    const int Seed = 42;
    static readonly DateTimeOffset RepoCreated = new(2025, 4, 30, 0, 0, 0, TimeSpan.Zero);

    public string Name => "fetch-health";

    public async Task RunAsync(CommandContext context)
    {
        var gh = new GitHubFetcher(context.Cache);

        Console.WriteLine("Issues and PRs, all time (issues endpoint) ...");
        var issues = await gh.IssuesAsync(context.Run, RepoCreated);
        var prior = await gh.PullsBeforeWindowAsync(context.Run, RepoCreated);
        Console.WriteLine($"  {issues.Count} issues opened in window, {prior.Count} PRs opened before the window");

        Console.WriteLine("Commits before the window ...");
        var priorCommits = await gh.PriorCommitsAsync(RepoCreated, context.Run.Since);
        Console.WriteLine($"  {priorCommits.Count} commits before {context.Run.Since:yyyy-MM-dd}");

        var data = Dataset.Load(context.Cache);
        var overrides = IdentityOverrides.Load(Path.Combine(context.Paths.Root, "data", "identity_overrides.json"));
        var review = BotDetector.Evaluate(data, context.Run, overrides).ReviewList;
        Console.WriteLine($"Profiles of {review.Count} accounts under review ...");
        foreach (var login in review)
            await gh.UserAsync(login);
        Console.WriteLine($"Health fetch complete; API calls made in this run: {gh.ApiCalls}");
    }

}
