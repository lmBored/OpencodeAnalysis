using System.Globalization;
using System.Text;
using GhCore;
using static OpenCodeAnalysis.ReportText;

namespace OpenCodeAnalysis;

public static class ContributorsReport
{
    public static void Write(Dataset data, RunInfo run, string outDir, int topN)
    {
        var resolve = ContributorAnalysis.LoginResolver(data.Commits);
        var resolved = data.Commits.Select(c => (Commit: c, Login: resolve(c))).ToList();
        bool IsBotCommit((CommitRecord Commit, string? Login) x) => x.Commit.IsBot || Bots.IsBot(x.Login);

        var rows = ContributorAnalysis.Rows(data.Commits, data.Pulls, data.ReviewCounts);
        var top = ContributorAnalysis.TopHumans(rows, topN);

        // frame sizes/counts (7.1)
        var unattributed = resolved.Where(x => x.Login is null && !x.Commit.IsBot).ToList();
        var humanAuthors = resolved.Where(x => !IsBotCommit(x))
            .Select(x => x.Login ?? "email:" + x.Commit.AuthorEmail.ToLowerInvariant())
            .Distinct(StringComparer.OrdinalIgnoreCase).Count();
        var prAuthors = data.Pulls.Where(p => !p.IsBot).Select(p => p.Login).Distinct(StringComparer.OrdinalIgnoreCase).Count();
        var activeHumans = resolved.Where(x => !IsBotCommit(x) && x.Login is not null).Select(x => x.Login!)
            .Concat(data.Pulls.Where(p => !p.IsBot).Select(p => p.Login))
            .Distinct(StringComparer.OrdinalIgnoreCase).Count();
        var linked = data.Contributors.Count(c => c.Login is not null);
        var anon = data.Contributors.Count(c => c.Login is null);
        var linkedBots = data.Contributors.Count(c => c.Login is not null && Bots.IsBot(c.Login, c.Type));

        var info = new StringBuilder();
        info.AppendLine($"Repository: {RepoUrl}");
        info.AppendLine($"Collected at: {run.CollectedAt:u}");
        info.AppendLine($"Window: {run.Since:yyyy-MM-dd} (incl.) .. {run.Until:yyyy-MM-dd} (excl.)");
        info.AppendLine($"Seed: {run.Seed}");
        info.AppendLine();
        info.AppendLine("[All-time contributors, GET /repos/.../contributors?anon=1]");
        info.AppendLine($"entries total: {data.Contributors.Count}");
        info.AppendLine($"  linked GitHub accounts: {linked} (of which bots: {linkedBots})");
        info.AppendLine($"  anonymous (unlinked email): {anon}");
        info.AppendLine();
        info.AppendLine("[Last-year frame]");
        info.AppendLine($"commits: {data.Commits.Count}");
        info.AppendLine($"  bot commits: {resolved.Count(IsBotCommit)}");
        info.AppendLine($"  unattributed human commits (no login, email not mappable): {unattributed.Count}");
        info.AppendLine($"  distinct human commit authors (login or email): {humanAuthors}");
        info.AppendLine($"PRs created: {data.Pulls.Count} (bots: {data.Pulls.Count(p => p.IsBot)}, merged: {data.Pulls.Count(p => p.MergedAt is not null)})");
        info.AppendLine($"  distinct human PR authors: {prAuthors}");
        info.AppendLine($"distinct human accounts with >=1 commit or PR: {activeHumans}");
        info.AppendLine($"review comments: {data.ReviewComments.Count}; conversation comments: {data.IssueComments.Count}");
        info.AppendLine($"review count candidates (search API): {data.ReviewCounts.Count}");
        info.AppendLine($"top {topN} humans: {string.Join(", ", top.Select(r => r.Login))}");
        info.AppendLine($"files sample: {data.FilesSample.Count} merged PRs (<=150 per top human, seed {run.Seed}), {data.Files.Count} file entries");
        info.AppendLine($"targeted reviews: PRs with fetched reviews = {data.Reviews.Select(r => r.PrNumber).Distinct().Count()}");
        File.WriteAllText(Path.Combine(outDir, "run_info.txt"), info.ToString());

        // contributors.csv (all accounts)
        var csv = new StringBuilder("login,is_bot,commits,prs_opened,prs_reviewed,total\n");
        foreach (var r in rows)
            csv.AppendLine($"{Csv.Escape(r.Login)},{r.IsBot},{r.Commits},{r.PullsOpened},{(data.ReviewCounts.ContainsKey(r.Login) ? r.PrsReviewed : "")},{r.Total}");
        File.WriteAllText(Path.Combine(outDir, "contributors.csv"), csv.ToString());

        // top 10 table + bots row (7.2)
        var botCommits = resolved.Count(IsBotCommit);
        var botPulls = data.Pulls.Count(p => p.IsBot);
        var botLogins = resolved.Where(IsBotCommit).Select(x => x.Login ?? x.Commit.AuthorName)
            .Concat(data.Pulls.Where(p => p.IsBot).Select(p => p.Login))
            .Distinct(StringComparer.OrdinalIgnoreCase).Order().ToList();
        var tableRows = top.Select((r, i) => new[]
        {
            (i + 1).ToString(CultureInfo.InvariantCulture),
            $"\\href{{https://github.com/{r.Login}}}{{\\texttt{{{LatexWriter.Escape(r.Login)}}}}}",
            r.Commits.ToString(), r.PullsOpened.ToString(), r.PrsReviewed.ToString(), $"\\textbf{{{r.Total}}}",
        });
        var footer = new[]
        {
            new[] { "--", $"All bots ({botLogins.Count} accounts)", botCommits.ToString(), botPulls.ToString(), "--",
                $"\\textbf{{{botCommits + botPulls}}}" },
        };
        var tex = LatexWriter.Longtable(
            $"Top {topN} human contributors of \\texttt{{anomalyco/opencode}} from {run.Since:d MMMM yyyy} to {run.Until.AddDays(-1):d MMMM yyyy}, ranked by the sum of the three activity metrics; bots are aggregated in the last row.",
            "tab:top10-contributors",
            ["\\#", "Contributor", "Commits", "PRs opened", "PRs reviewed", "Total"],
            @">{\raggedleft\arraybackslash}p{0.04\textwidth} >{\raggedright\arraybackslash}p{0.30\textwidth} >{\raggedleft\arraybackslash}p{0.10\textwidth} >{\raggedleft\arraybackslash}p{0.11\textwidth} >{\raggedleft\arraybackslash}p{0.12\textwidth} >{\raggedleft\arraybackslash}p{0.08\textwidth}",
            tableRows, footer);
        File.WriteAllText(Path.Combine(outDir, "contributors_top10.tex"), tex);
        File.WriteAllText(Path.Combine(outDir, "bots.txt"), string.Join('\n', botLogins));

        // skills/nterests (7.3, 7.4)
        var scopes = new StringBuilder("login,conventional_share,top_types,top_scopes,top_packages_in_sampled_prs\n");
        var samples = new StringBuilder();
        foreach (var r in top)
        {
            var mine = resolved.Where(x => string.Equals(x.Login, r.Login, StringComparison.OrdinalIgnoreCase))
                .Select(x => x.Commit).ToList();
            var parsed = mine.Select(c => ContributorAnalysis.ParseConventional(c.Message)).ToList();
            var conventional = parsed.Where(p => p is not null).Select(p => p!.Value).ToList();
            var types = ContributorAnalysis.TopCounts(conventional.Select(p => p.Type), 4);
            var scopeTop = ContributorAnalysis.TopCounts(conventional.Where(p => p.Scope is not null).Select(p => p.Scope!), 6);
            var packages = ContributorAnalysis.TopCounts(
                data.Files.Where(f => string.Equals(f.PrAuthor, r.Login, StringComparison.OrdinalIgnoreCase))
                    .GroupBy(f => f.PrNumber)
                    .SelectMany(g => g.Select(f => ContributorAnalysis.PackageOf(f.Path)).Distinct()), 6);
            string Fmt(List<(string Key, int Count)> xs) => string.Join("; ", xs.Select(x => $"{x.Key} ({x.Count})"));
            var share = mine.Count == 0 ? 0 : 100.0 * conventional.Count / mine.Count;
            scopes.AppendLine($"{r.Login},{share:F0}%,{Csv.Escape(Fmt(types))},{Csv.Escape(Fmt(scopeTop))},{Csv.Escape(Fmt(packages))}");

            var sample = Sampling.RandomSample(mine, CommitSampleSize, run.Seed, c => c.Sha);
            samples.AppendLine($"## {r.Login} ({mine.Count} commits; random sample of {sample.Count}, seed {run.Seed})");
            samples.AppendLine($"types: {Fmt(types)}");
            samples.AppendLine($"scopes: {Fmt(scopeTop)}");
            samples.AppendLine($"packages (sampled merged PRs): {Fmt(packages)}");
            samples.AppendLine();
            foreach (var c in sample.OrderBy(c => c.Date))
                samples.AppendLine($"- {c.Date:yyyy-MM-dd} [{c.Sha[..7]}]({RepoUrl}/commit/{c.Sha}) {FirstLine(c.Message)}");
            samples.AppendLine();
        }
        File.WriteAllText(Path.Combine(outDir, "scopes_by_login.csv"), scopes.ToString());
        File.WriteAllText(Path.Combine(outDir, "commit_samples.md"), samples.ToString());

        Console.WriteLine(info.ToString());
        Console.WriteLine(scopes.ToString());
        foreach (var r in top) Console.WriteLine($"{r.Login,-16} {r.Commits,6} {r.PullsOpened,6} {r.PrsReviewed,6} {r.Total,7}");
        Console.WriteLine($"{"bots",-16} {botCommits,6} {botPulls,6} {"--",6} {botCommits + botPulls,7}");
    }
}
