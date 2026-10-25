using System.Text;

namespace OpenCodeAnalysis;

public sealed record Exemplar(PullRecord Pr, double Hours, string Note);

public sealed record ExemplarsResult(
    int FirstTimers, int Answered, int Silent,
    IReadOnlyList<PullRecord> ClosedSilent, IReadOnlyList<PullRecord> MergedSilent,
    IReadOnlyList<Exemplar> FastAnswered, IReadOnlyList<Exemplar> SlowAnswered,
    int BotOnlyPrs, IReadOnlyList<(PullRecord Pr, string Bots)> BotOnlyExamples,
    IReadOnlyList<IssueRow> SilentIssues, IReadOnlyList<(IssueRow Issue, double Days)> SlowIssues,
    IReadOnlyList<PullRecord> QuickMerged);

public static class ExemplarsAudit
{
    public static ExemplarsResult Compute(PracticeData d)
    {
        var firstTimers = d.Pulls.Where(p => !PracticeData.IsBot(p) && d.ClassOf(p) == AuthorClass.FirstTimeAuthor).ToList();
        var answered = firstTimers.Where(d.Answered).ToList();
        var silent = firstTimers.Where(p => !d.Answered(p)).ToList();
        double Hours(PullRecord p) => (d.FirstHumanReply(p) - p.CreatedAt.ToUnixTimeMilliseconds()) / 3_600_000d;

        var closedSilent = silent.Where(p => p.MergedAt is null && p.State == ItemState.Closed)
            .OrderByDescending(p => p.CreatedAt).Take(8).ToList();
        var mergedSilent = silent.Where(p => p.MergedAt is not null)
            .OrderByDescending(p => p.CreatedAt).Take(6).ToList();
        var fast = answered.Select(p => new Exemplar(p, Hours(p), ""))
            .Where(x => x.Hours is >= 0 and <= 6).OrderBy(x => x.Pr.CreatedAt).Take(6).ToList();
        var slow = answered.Select(p => new Exemplar(p, Hours(p), ""))
            .Where(x => x.Hours > 24 * 14).OrderByDescending(x => x.Pr.CreatedAt).Take(6).ToList();

        var botOnly = d.HumanPulls.Where(d.BotOnlyReviews).ToList();
        var botOnlyExamples = botOnly.Take(6).Select(p => (p,
            string.Join(", ", (d.ReviewsByPr.GetValueOrDefault(p.Number) ?? [])
                .Select(r => r.Author).Distinct()))).ToList();

        var silentIssues = d.Issues.Where(i => i.Comments == 0).OrderByDescending(i => i.CreatedAt).Take(8).ToList();
        var slowIssues = new List<(IssueRow, double)>();
        foreach (var i in d.Issues)
        {
            var at = d.FirstHumanComment(i.Number);
            if (!double.IsFinite(at)) continue;
            var days = (at - i.CreatedAt.ToUnixTimeMilliseconds()) / 86_400_000d;
            if (days > 30) slowIssues.Add((i, days));
        }
        slowIssues.Sort((a, b) => b.Item2.CompareTo(a.Item2));
        var quick = firstTimers.Where(p => p.MergedAt is not null)
            .Where(p => (p.MergedAt!.Value - p.CreatedAt).TotalDays <= 1)
            .OrderBy(p => p.CreatedAt).Take(8).ToList();

        return new ExemplarsResult(firstTimers.Count, answered.Count, silent.Count,
            closedSilent, mergedSilent, fast, slow, botOnly.Count, botOnlyExamples,
            silentIssues, slowIssues.Take(6).ToList(), quick);
    }

    static string Line(PullRecord p, string note = "") =>
        $"- [#{p.Number}]({p.Url}) - {p.CreatedAt:yyyy-MM-dd} - `{p.Login}` - " +
        $"{(p.MergedAt is not null ? "merged " + p.MergedAt.Value.ToString("yyyy-MM-dd") : p.State)}{note} - '{p.Title}'";

}
