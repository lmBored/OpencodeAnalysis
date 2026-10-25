using System.Text;
using GhCore;

namespace OpenCodeAnalysis;

public sealed record BotDisposition(int AnyHuman, int BotOnly, int Silent, int Cleanup, int Compliance);

public sealed record AutomationResult(
    int Comments, int Cleanup, int Compliance, int Duplicate, int Stale, int Llm, int BotOther,
    int FirstN, BotDisposition First,
    int FirstClosedN, BotDisposition FirstClosed,
    int HumanClosedN, BotDisposition HumanClosed,
    int WarnedFirst, int WarnedFirstMerged, int WarnedAll, int WarnedAllMerged,
    Summary CleanupGaps, double CleanupP10, double CleanupP90, Summary WarnGaps, Summary CloseGaps,
    double CloseP10,
    int StaleClosed, int StaleNoTeam, int StaleNoHuman, double StaleMedianAge,
    IReadOnlyList<(string Bucket, int N, int Closed)> TimeBuckets, double NightShare, int[] ClosureHours);

/// Counts how bots dispose of incoming pull requests.
public static class AutomationAudit
{
    public static AutomationResult Compute(PracticeData d, IReadOnlySet<string> team)
    {
        // Each bot comment is counted in the first category it matches.
        int cleanup = 0, compliance = 0, duplicate = 0, stale = 0, llm = 0, other = 0;
        foreach (var c in d.Comments)
        {
            if (!c.IsBot && !Bots.IsBot(c.Login)) continue;
            var body = c.Body ?? "";
            if (CommentText.Cleanup.IsMatch(body)) cleanup++;
            else if (CommentText.Compliance.IsMatch(body)) compliance++;
            else if (CommentText.Duplicate.IsMatch(body)) duplicate++;
            else if (CommentText.StaleClose.IsMatch(body)) stale++;
            else if (CommentText.Llm.IsMatch(body)) llm++;
            else other++;
        }

        var firstTimers = d.Pulls.Where(p => d.ClassOf(p) == AuthorClass.FirstTimeAuthor).ToList();
        var humanPulls = d.HumanPulls.ToList();
        var firstClosed = firstTimers.Where(p => p.MergedAt is null && p.State == ItemState.Closed).ToList();
        var humanClosed = humanPulls.Where(p => p.MergedAt is null && p.State == ItemState.Closed).ToList();

        BotDisposition Dispatch(PullRecord p)
        {
            var cs = d.CommentsByNumber.GetValueOrDefault(p.Number) ?? [];
            var human = cs.Any(c => !c.IsBot && !string.Equals(c.Login, p.Login, StringComparison.OrdinalIgnoreCase));
            var bots = cs.Where(c => c.IsBot || Bots.IsBot(c.Login)).ToList();
            return new BotDisposition(
                human ? 1 : 0,
                !human && bots.Count > 0 ? 1 : 0,
                !human && bots.Count == 0 ? 1 : 0,
                bots.Any(c => CommentText.Cleanup.IsMatch(c.Body)) ? 1 : 0,
                bots.Any(c => CommentText.Compliance.IsMatch(c.Body)) ? 1 : 0);
        }
        BotDisposition Tally(IEnumerable<PullRecord> list) =>
            list.Select(Dispatch).Aggregate(new BotDisposition(0, 0, 0, 0, 0),
                (a, x) => new BotDisposition(a.AnyHuman + x.AnyHuman, a.BotOnly + x.BotOnly,
                    a.Silent + x.Silent, a.Cleanup + x.Cleanup, a.Compliance + x.Compliance));

        var warnedFirst = firstTimers.Where(p => Dispatch(p).Compliance > 0).ToList();
        var warnedAll = humanPulls.Where(p => Dispatch(p).Compliance > 0).ToList();

        var cleanupGaps = new List<double>();
        var warnGaps = new List<double>();
        var closeGaps = new List<double>();
        foreach (var p in firstClosed)
        {
            foreach (var c in d.CommentsByNumber.GetValueOrDefault(p.Number) ?? [])
            {
                if (!c.IsBot && !Bots.IsBot(c.Login)) continue;
                if (CommentText.Cleanup.IsMatch(c.Body)) cleanupGaps.Add((c.CreatedAt - p.CreatedAt).TotalDays);
                if (CommentText.ComplianceWarn.IsMatch(c.Body)) warnGaps.Add((c.CreatedAt - p.CreatedAt).TotalHours);
                if (CommentText.ComplianceClose.IsMatch(c.Body)) closeGaps.Add((c.CreatedAt - p.CreatedAt).TotalHours);
            }
        }

        var staleClosed = 0;
        var staleNoTeam = 0;
        var staleNoHuman = 0;
        var staleAges = new List<double>();
        foreach (var issue in d.Issues)
        {
            var list = d.CommentsByNumber.GetValueOrDefault(issue.Number) ?? [];
            var close = list.FirstOrDefault(c => HealthAnalysis.AutoCloseKind(c.Body) == "issue-stale");
            if (close is null) continue;
            var before = list.Where(c => c.CreatedAt <= close.CreatedAt).ToList();
            var human = before.Where(c => !c.IsBot && !Bots.IsBot(c.Login)).ToList();
            staleClosed++;
            if (!human.Any(c => team.Contains(c.Login))) staleNoTeam++;
            if (human.Count == 0) staleNoHuman++;
            staleAges.Add((close.CreatedAt - issue.CreatedAt).TotalDays);
        }

        string Bucket(int hour) => hour < 6 ? "00-06 UTC" : hour < 12 ? "06-12 UTC" : hour < 18 ? "12-18 UTC" : "18-24 UTC";
        var buckets = new[] { "00-06 UTC", "06-12 UTC", "12-18 UTC", "18-24 UTC" }
            .ToDictionary(b => b, b => (N: 0, Closed: 0));
        var closureHours = new int[24];
        foreach (var p in firstTimers)
        {
            var key = Bucket(p.CreatedAt.UtcDateTime.Hour);
            buckets[key] = (buckets[key].N + 1, buckets[key].Closed);
            var close = (d.CommentsByNumber.GetValueOrDefault(p.Number) ?? [])
                .FirstOrDefault(c => (c.IsBot || Bots.IsBot(c.Login)) && CommentText.ComplianceClose.IsMatch(c.Body));
            if (close is null) continue;
            buckets[key] = (buckets[key].N, buckets[key].Closed + 1);
            closureHours[close.CreatedAt.UtcDateTime.Hour]++;
        }
        var allClosures = closureHours.Sum();
        var nightShare = allClosures == 0 ? double.NaN : closureHours.Take(8).Sum() / (double)allClosures;

        return new AutomationResult(
            d.Comments.Count, cleanup, compliance, duplicate, stale, llm, other,
            firstTimers.Count, Tally(firstTimers),
            firstClosed.Count, Tally(firstClosed),
            humanClosed.Count, Tally(humanClosed),
            warnedFirst.Count, warnedFirst.Count(p => p.MergedAt is not null),
            warnedAll.Count, warnedAll.Count(p => p.MergedAt is not null),
            Statistics.Summarize(cleanupGaps), Statistics.Percentile(cleanupGaps.Order().ToList(), 10), Statistics.Percentile(cleanupGaps.Order().ToList(), 90), Statistics.Summarize(warnGaps), Statistics.Summarize(closeGaps),
            Statistics.Percentile(closeGaps.Order().ToList(), 10),
            staleClosed, staleNoTeam, staleNoHuman, Statistics.Percentile(staleAges.Order().ToList(), 50),
            buckets.Select(kv => (kv.Key, kv.Value.N, kv.Value.Closed)).ToList(), nightShare, closureHours);
    }

}
