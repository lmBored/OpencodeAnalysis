using System.Text.RegularExpressions;
using GhCore;

namespace OpenCodeAnalysis;

public record ContributorRow(string Login, bool IsBot, int Commits, int PullsOpened, int PrsReviewed)
{
    public int Total => Commits + PullsOpened + PrsReviewed;
}

public static partial class ContributorAnalysis
{
    [GeneratedRegex(@"^(?:\d+\+)?(?<login>[A-Za-z0-9-]+(?:\[bot\])?)@users\.noreply\.github\.com$", RegexOptions.IgnoreCase)]
    private static partial Regex NoReply();

    /// Maps commit to GitHub login: Linked account, else e-mail in linked commits, else noreply e-mail, else else null.
    public static Func<CommitRecord, string?> LoginResolver(IReadOnlyList<CommitRecord> commits)
    {
        var byEmail = commits
            .Where(c => c.Login is not null)
            .GroupBy(c => c.AuthorEmail.ToLowerInvariant())
            .ToDictionary(g => g.Key, g => g.GroupBy(c => c.Login!).OrderByDescending(x => x.Count()).First().Key);
        return c =>
        {
            if (c.Login is not null) return c.Login;
            var email = c.AuthorEmail.ToLowerInvariant();
            if (byEmail.TryGetValue(email, out var login)) return login;
            var m = NoReply().Match(email);
            return m.Success ? m.Groups["login"].Value : null;
        };
    }

    public static Dictionary<string, int> CommitsByLogin(IReadOnlyList<CommitRecord> commits)
    {
        var resolve = LoginResolver(commits);
        return commits
            .Select(c => resolve(c))
            .Where(l => l is not null)
            .GroupBy(l => l!, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.Count(), StringComparer.OrdinalIgnoreCase);
    }

    public static Dictionary<string, int> PullsByLogin(IReadOnlyList<PullRecord> pulls) =>
        pulls.GroupBy(p => p.Login, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.Count(), StringComparer.OrdinalIgnoreCase);

    /// Possible top 10 members (top n by commits with the top n by PRs opened)
    public static List<string> Candidates(IReadOnlyList<CommitRecord> commits, IReadOnlyList<PullRecord> pulls, int n = 40)
    {
        var byCommits = CommitsByLogin(commits).Where(kv => !Bots.IsBot(kv.Key))
            .OrderByDescending(kv => kv.Value).Take(n).Select(kv => kv.Key);
        var byPulls = PullsByLogin(pulls).Where(kv => !Bots.IsBot(kv.Key))
            .OrderByDescending(kv => kv.Value).Take(n).Select(kv => kv.Key);
        return byCommits.Union(byPulls, StringComparer.OrdinalIgnoreCase).Order(StringComparer.OrdinalIgnoreCase).ToList();
    }

    public static List<ContributorRow> Rows(
        IReadOnlyList<CommitRecord> commits, IReadOnlyList<PullRecord> pulls, IReadOnlyDictionary<string, int> reviews)
    {
        var c = CommitsByLogin(commits);
        var p = PullsByLogin(pulls);
        return c.Keys.Union(p.Keys, StringComparer.OrdinalIgnoreCase).Union(reviews.Keys, StringComparer.OrdinalIgnoreCase)
            .Select(l => new ContributorRow(l, Bots.IsBot(l), c.GetValueOrDefault(l), p.GetValueOrDefault(l),
                reviews.GetValueOrDefault(l)))
            .OrderByDescending(r => r.Total).ThenBy(r => r.Login, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public static List<ContributorRow> TopHumans(IEnumerable<ContributorRow> rows, int n = 10) =>
        rows.Where(r => !r.IsBot).Take(n).ToList();

    [GeneratedRegex(@"^(?<type>[a-z]+)(?:\((?<scope>[^)]+)\))?!?:", RegexOptions.IgnoreCase)]
    private static partial Regex Conventional();

    /// Conventional commit with format "type(scope)", or null if doesnt follow convention.
    public static (string Type, string? Scope)? ParseConventional(string message)
    {
        var m = Conventional().Match(message.Split('\n')[0].Trim());
        if (!m.Success) return null;
        return (m.Groups["type"].Value.ToLowerInvariant(),
            m.Groups["scope"].Success ? m.Groups["scope"].Value.ToLowerInvariant() : null);
    }

    /// Package a file belongs to packages/x/ or packages/x/y/, else root.
    public static string PackageOf(string path)
    {
        var parts = path.Split('/');
        if (parts[0] == "packages" && parts.Length > 2)
            return parts[1] is "console" or "stats" or "sdk" && parts.Length > 3
                ? $"packages/{parts[1]}/{parts[2]}"
                : $"packages/{parts[1]}";
        return parts.Length > 1 ? parts[0] + "/" : "(root)";
    }

    public static List<(string Key, int Count)> TopCounts(IEnumerable<string> keys, int n) =>
        keys.GroupBy(k => k).Select(g => (g.Key, g.Count()))
            .OrderByDescending(x => x.Item2).ThenBy(x => x.Key).Take(n).ToList();
}
