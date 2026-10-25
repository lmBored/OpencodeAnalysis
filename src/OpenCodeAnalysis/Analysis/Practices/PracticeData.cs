using System.Text.Json;
using System.Text.RegularExpressions;
using GhCore;

namespace OpenCodeAnalysis;

public sealed record IssueRow(int Number, DateTimeOffset CreatedAt, string State, int Comments, string Title, string Url);

/// Holds the indexed inputs used by the practice analyses.
public sealed class PracticeData
{
    public required Dataset Dataset { get; init; }
    public required RunInfo Run { get; init; }
    public required IReadOnlyList<PullRecord> Pulls { get; init; }
    public required IReadOnlyList<CommentRecord> Comments { get; init; }
    public required IReadOnlyList<IssueRow> Issues { get; init; }
    public required IReadOnlyList<ReviewActivity> Reviews { get; init; }
    public required IReadOnlySet<string> Top { get; init; }
    public required IReadOnlySet<string> Returning { get; init; }

    public required IReadOnlyDictionary<int, List<CommentRecord>> CommentsByNumber { get; init; }
    public required IReadOnlyDictionary<int, List<ReviewActivity>> ReviewsByPr { get; init; }
    public required IReadOnlyDictionary<string, PullRecord> FirstPr { get; init; }
    public required IReadOnlyDictionary<string, PullRecord> SecondPr { get; init; }

    public static PracticeData Load(Dataset data, RunInfo run, string dataDir)
    {
        var pulls = data.Pulls;
        if (data.IssueItems.Count == 0)
            throw new InvalidOperationException(
                "practices: data/issue_items.json missing - run `dotnet run -- fetch-health` first");
        var comments = (JsonSerializer.Deserialize<List<CommentRecord>>(
            File.ReadAllText(Path.Combine(dataDir, "issue_comments.json"))) ?? [])
            .Where(c => c.CreatedAt >= run.Since && c.CreatedAt < run.Until)
            .DistinctBy(c => c.Url).ToList();
        var reviews = data.Activities?.Reviews ?? [];
        var top = data.Top.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var returning = data.Returning.ToHashSet(StringComparer.OrdinalIgnoreCase);

        var byNumber = comments
            .GroupBy(c => c.Number)
            .ToDictionary(g => g.Key, g => g.OrderBy(c => c.CreatedAt).ToList());
        var reviewsByPr = reviews.GroupBy(r => r.Pr).ToDictionary(g => g.Key, g => g.ToList());

        // Tracks the first and second pull request of each human author.
        var first = new Dictionary<string, PullRecord>(StringComparer.OrdinalIgnoreCase);
        var second = new Dictionary<string, PullRecord>(StringComparer.OrdinalIgnoreCase);
        foreach (var p in pulls.OrderBy(p => p.CreatedAt))
        {
            if (IsBot(p)) continue;
            if (!first.TryAdd(p.Login, p)) second.TryAdd(p.Login, p);
        }

        return new PracticeData
        {
            Dataset = data,
            Run = run,
            Pulls = pulls,
            Comments = comments,
            Issues = LoadIssues(data, run, byNumber),
            Reviews = reviews,
            Top = top,
            Returning = returning,
            CommentsByNumber = byNumber,
            ReviewsByPr = reviewsByPr,
            FirstPr = first,
            SecondPr = second,
        };
    }

    public static bool IsBot(PullRecord p) => p.IsBot || Bots.IsBot(p.Login);

    public AuthorClass ClassOf(PullRecord p)
    {
        if (IsBot(p)) return AuthorClass.Bot;
        if (Top.Contains(p.Login)) return AuthorClass.Top10;
        if (Returning.Contains(p.Login)) return AuthorClass.RepeatAuthor;
        return FirstPr.TryGetValue(p.Login, out var first) && first.Number == p.Number
            ? AuthorClass.FirstTimeAuthor : AuthorClass.RepeatAuthor;
    }

    public IEnumerable<PullRecord> OfClass(AuthorClass? cls) =>
        cls is null ? Pulls : Pulls.Where(p => ClassOf(p) == cls);

    public IEnumerable<PullRecord> HumanPulls => Pulls.Where(p => !IsBot(p));

    /// Returns the earliest reply time from another human or NaN when none exists.
    public double FirstHumanReply(PullRecord p)
    {
        var best = double.NaN;
        foreach (var c in CommentsByNumber.GetValueOrDefault(p.Number) ?? [])
        {
            if (c.IsBot || Bots.IsBot(c.Login)) continue;
            if (string.Equals(c.Login, p.Login, StringComparison.OrdinalIgnoreCase)) continue;
            var ms = c.CreatedAt.ToUnixTimeMilliseconds();
            if (!double.IsFinite(best) || ms < best) best = ms;
        }
        foreach (var r in ReviewsByPr.GetValueOrDefault(p.Number) ?? [])
        {
            if (Bots.IsReviewBot(r.Author)) continue;
            if (string.Equals(r.Author, p.Login, StringComparison.OrdinalIgnoreCase)) continue;
            var ms = r.SubmittedAt.ToUnixTimeMilliseconds();
            if (!double.IsFinite(best) || ms < best) best = ms;
        }
        return best;
    }

    public double FirstHumanComment(int number)
    {
        var best = double.NaN;
        foreach (var c in CommentsByNumber.GetValueOrDefault(number) ?? [])
        {
            if (c.IsBot || Bots.IsBot(c.Login)) continue;
            var ms = c.CreatedAt.ToUnixTimeMilliseconds();
            if (!double.IsFinite(best) || ms < best) best = ms;
        }
        return best;
    }

    public bool Answered(PullRecord p)
    {
        var at = FirstHumanReply(p);
        return double.IsFinite(at) && at >= p.CreatedAt.ToUnixTimeMilliseconds();
    }

    /// Returns true when every review on the pull request came from a bot.
    public bool BotOnlyReviews(PullRecord p)
    {
        var rs = ReviewsByPr.GetValueOrDefault(p.Number);
        return rs is { Count: > 0 } && rs.All(r => Bots.IsReviewBot(r.Author));
    }

    static List<IssueRow> LoadIssues(Dataset data, RunInfo run,
        IReadOnlyDictionary<int, List<CommentRecord>> byNumber)
    {
        return data.IssueItems
            .Where(issue => !issue.IsPull && issue.CreatedAt >= run.Since && issue.CreatedAt < run.Until)
            .Select(issue => new IssueRow(issue.Number, issue.CreatedAt, issue.State,
                byNumber.GetValueOrDefault(issue.Number)?.Count ?? 0, issue.Title, issue.Url))
            .ToList();
    }
}

public static class CommentText
{
    public static readonly Regex Cleanup = new("Automated PR Cleanup", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    public static readonly Regex Compliance = new(
        "issue-compliance|within the 2-hour window|within \\*\\*2 hours\\*\\*|doesn't fully meet our \\[contributing guidelines\\]",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);
    public static readonly Regex ComplianceWarn = new("within \\*\\*2 hours\\*\\*|2 hours to edit",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);
    public static readonly Regex ComplianceClose = new("automatically closed because it was not updated",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);
    public static readonly Regex Duplicate = new("might be a duplicate|possible duplicate",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);
    public static readonly Regex StaleClose = new("automatically closed after 60 days",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);
    public static readonly Regex Llm = new("comment was made by an LLM",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);
    public static readonly Regex DesignRequest = new(
        "design review|(approval|review) before (i |submitting|opening)|waiting for (a )?(design review|approval)|needs? (a )?(design|core team) (review|approval)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);
    public static readonly Regex DesignKeyword = new("design review|approval|review before",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);
}
