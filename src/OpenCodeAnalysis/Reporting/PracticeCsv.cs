using GhCore;
using System.Globalization;

namespace OpenCodeAnalysis;

/// Writes the practice audit tables as CSV files.
public static class PracticeCsv
{
    public static MetricSet AutomationMetrics(AutomationResult r)
    {
        var set = new MetricSet("automation", [])
            .Add("bot_comments_total", r.Comments, MetricKind.Count)
            .Add("bot_comments_cleanup", r.Cleanup, MetricKind.Count)
            .Add("bot_comments_compliance", r.Compliance, MetricKind.Count)
            .Add("bot_comments_duplicate", r.Duplicate, MetricKind.Count)
            .Add("bot_comments_stale", r.Stale, MetricKind.Count)
            .Add("bot_comments_llm", r.Llm, MetricKind.Count)
            .Add("bot_comments_other", r.BotOther, MetricKind.Count)
            .Add("first_time_prs", r.FirstN, MetricKind.Count)
            .Add("first_time_any_human_comment", r.First.AnyHuman, MetricKind.Count)
            .Add("first_time_bot_only", r.First.BotOnly, MetricKind.Count)
            .Add("first_time_silent", r.First.Silent, MetricKind.Count)
            .Add("first_time_closed", r.FirstClosedN, MetricKind.Count)
            .Add("first_time_closed_any_human", r.FirstClosed.AnyHuman, MetricKind.Count)
            .Add("first_time_closed_bot_only", r.FirstClosed.BotOnly, MetricKind.Count)
            .Add("first_time_closed_silent", r.FirstClosed.Silent, MetricKind.Count)
            .Add("first_time_closed_cleanup", r.FirstClosed.Cleanup, MetricKind.Count)
            .Add("first_time_closed_compliance", r.FirstClosed.Compliance, MetricKind.Count)
            .Add("human_closed", r.HumanClosedN, MetricKind.Count)
            .Add("human_closed_any_human", r.HumanClosed.AnyHuman, MetricKind.Count)
            .Add("human_closed_bot_only", r.HumanClosed.BotOnly, MetricKind.Count)
            .Add("human_closed_silent", r.HumanClosed.Silent, MetricKind.Count)
            .Add("human_closed_cleanup", r.HumanClosed.Cleanup, MetricKind.Count)
            .Add("warned_first_time", r.WarnedFirst, MetricKind.Count)
            .Add("warned_first_time_merged", r.WarnedFirstMerged, MetricKind.Count)
            .Add("warned_all", r.WarnedAll, MetricKind.Count)
            .Add("warned_all_merged", r.WarnedAllMerged, MetricKind.Count)
            .Add("cleanup_days_p10", r.CleanupP10, MetricKind.Days1)
            .Add("cleanup_days_p50", r.CleanupGaps.P50, MetricKind.Days1)
            .Add("cleanup_days_p90", r.CleanupP90, MetricKind.Days1)
            .Add("compliance_warn_hours_p50", r.WarnGaps.P50, MetricKind.Hours2)
            .Add("compliance_close_hours_p10", r.CloseP10, MetricKind.Hours2)
            .Add("compliance_close_hours_p50", r.CloseGaps.P50, MetricKind.Hours2)
            .Add("compliance_close_hours_p90", r.CloseGaps.P90, MetricKind.Hours2)
            .Add("compliance_closures", r.ClosureHours.Sum(), MetricKind.Count)
            .Add("compliance_closures_night_share", r.NightShare, MetricKind.Percent);

        foreach (var (bucket, prs, closed) in r.TimeBuckets)
        {
            var key = "created_" + bucket.Replace(" UTC", "").Replace('-', '_');
            set = set.Add($"{key}_prs", prs, MetricKind.Count)
                .Add($"{key}_closed", closed, MetricKind.Count);
        }
        return set;
    }

    public static MetricSet GateMetrics(GateResult r) =>
        new MetricSet("design_review", [])
            .Add("issues_with_comments", r.IssuesWithComments, MetricKind.Count)
            .Add("issues_with_team_comment", r.IssuesWithTeam, MetricKind.Count)
            .Add("design_requests", r.Requests.Count, MetricKind.Count)
            .Add("design_requests_answered", r.Answered, MetricKind.Count)
            .Add("team_members", r.TeamSize, MetricKind.Count);

    static string FormatMetric(Metric metric) => metric.Kind switch
    {
        MetricKind.Count => ((long)metric.Value).ToString(CultureInfo.InvariantCulture),
        MetricKind.Percent => Format.Pct(metric.Value),
        MetricKind.Days1 or MetricKind.Hours1 => Format.Number(metric.Value, 1),
        MetricKind.Days2 or MetricKind.Hours2 => Format.Number(metric.Value, 2),
        MetricKind.Ratio3 => Format.Number(metric.Value, 3),
        _ => Format.Number(metric.Value),
    };

    public static void WriteMetrics(IEnumerable<MetricSet> sets, string outDir) =>
        Csv.Write(Path.Combine(outDir, "metrics.csv"), ["section", "metric", "value"],
            sets.SelectMany(set => set.Metrics.Select(metric =>
                new[] { set.Name, metric.Key, FormatMetric(metric) })));

    public static void WriteGateRequests(GateResult r, string outDir) =>
        Csv.Write(Path.Combine(outDir, "design_review_requests.csv"),
            ["issue", "created", "login", "answered_by", "answer_hours", "title", "quote", "url"],
            r.Requests.Select(q => new[]
            {
                q.Issue.Number.ToString(),
                q.At.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                q.Login,
                q.AnswerLogin ?? "",
                double.IsFinite(q.AnswerHours) ? Format.Number(q.AnswerHours, 1) : "",
                q.Issue.Title,
                q.Quote,
                q.Issue.Url,
            }));

    public static void WriteMigration(MigrationResult r, string outDir)
    {
        Csv.Write(Path.Combine(outDir, "migration_summary.csv"), ["metric", "value"],
        [
            ["commits_ahead", r.CommitsAhead.ToString()],
            ["sample_prs", r.SampleN.ToString()],
            ["unavailable_prs", r.Skipped.ToString()],
        ]);

        Csv.Write(Path.Combine(outDir, "migration_tree.csv"), ["path", "files_pin", "files_head"],
            r.TreeCounts.Select(kv => new[] { kv.Key, kv.Value.Pin.ToString(), kv.Value.Head.ToString() }));

        Csv.Write(Path.Combine(outDir, "migration.csv"),
            ["class", "prs", "legacy", "v2", "ui", "any_gone", "all_gone", "merged"],
            r.Rows.Select(x => new[]
            {
                AuthorClasses.LabelOf(x.Class), x.N.ToString(), Format.Pct(x.Legacy), Format.Pct(x.V2), Format.Pct(x.Ui),
                Format.Pct(x.AnyGone), Format.Pct(x.AllGone), Format.Pct(x.Merged),
            }));

        Csv.Write(Path.Combine(outDir, "migration_sample.csv"), ["pr"],
            r.SamplePrs.Select(n => new[] { n.ToString() }));

        Csv.Write(Path.Combine(outDir, "migration_examples.csv"),
            ["pr", "login", "paths_gone", "paths_total", "first_gone", "url"],
            r.Examples.Select(x => new[]
            {
                x.Pr.Number.ToString(), x.Pr.Login, x.Gone.ToString(), x.Total.ToString(), x.FirstGone, x.Pr.Url,
            }));
    }

    public static void WriteExemplars(ExemplarsResult r, string outDir)
    {
        var rows = new List<string[]>();
        void Add(string category, PullRecord p, string? hours = null, string? days = null) =>
            rows.Add([category, "pr", p.Number.ToString(), p.Url, p.Login,
                p.CreatedAt.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                p.MergedAt is not null ? "merged" : p.State, hours ?? "", days ?? "", p.Title]);
        void AddIssue(string category, IssueRow i, string? days = null) =>
            rows.Add([category, "issue", i.Number.ToString(), i.Url, "",
                i.CreatedAt.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture), i.State, "", days ?? "", i.Title]);

        foreach (var p in r.ClosedSilent) Add("closed_silent", p);
        foreach (var p in r.MergedSilent) Add("merged_silent", p);
        foreach (var x in r.FastAnswered) Add("answered_fast", x.Pr, hours: Format.Number(x.Hours, 1));
        foreach (var x in r.SlowAnswered) Add("answered_slow", x.Pr, days: Format.Number(x.Hours / 24, 0));
        foreach (var (p, bots) in r.BotOnlyExamples)
            rows.Add(["bot_only_review", "pr", p.Number.ToString(), p.Url, p.Login,
                p.CreatedAt.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                p.MergedAt is not null ? "merged" : p.State, "", "", $"{p.Title} [reviewers: {bots}]"]);
        foreach (var i in r.SilentIssues) AddIssue("issue_silent", i);
        foreach (var (i, days) in r.SlowIssues) AddIssue("issue_slow", i, Format.Number(days, 0));
        foreach (var p in r.QuickMerged) Add("merged_within_a_day", p);

        Csv.Write(Path.Combine(outDir, "exemplars.csv"),
            ["category", "kind", "number", "url", "login", "created", "state", "hours", "days", "title"], rows);
    }
}
