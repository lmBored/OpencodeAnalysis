using System.Globalization;
using System.Text;
using GhCore;
using static OpenCodeAnalysis.ReportText;

namespace OpenCodeAnalysis;

public static class ActivitiesReport
{
    public static void Write(Dataset data, RunInfo run, string outDir, int topN)
    {
        var source = data.Activities;
        if (source is null)
        {
            Console.WriteLine("activities: data/activities is missing - collect with GhActivities first");
            return;
        }

        static DateOnly Monday(DateTimeOffset value)
        {
            var date = DateOnly.FromDateTime(value.UtcDateTime);
            return date.AddDays(-(((int)date.DayOfWeek + 6) % 7));
        }

        var commits = source.Commits.GroupBy(Monday).ToDictionary(g => g.Key, g => g.Count());
        var releases = source.Releases.GroupBy(Monday).ToDictionary(g => g.Key, g => g.Count());
        var reviews = source.Reviews.GroupBy(r => Monday(r.SubmittedAt)).ToDictionary(g => g.Key, g => g.Count());
        var issues = data.IssueItems
            .Where(issue => !issue.IsPull && issue.CreatedAt >= run.Since && issue.CreatedAt < run.Until)
            .GroupBy(issue => Monday(issue.CreatedAt))
            .ToDictionary(g => g.Key, g => g.Count());
        var prs = data.Pulls
            .GroupBy(pull => Monday(pull.CreatedAt))
            .ToDictionary(g => g.Key, g => g.Count());
        var weeks = issues.Keys.Union(prs.Keys).Union(commits.Keys).Union(releases.Keys).Union(reviews.Keys)
            .OrderBy(w => w).ToList();

        var weekly = new StringBuilder("week_start,commits,prs,issues,releases,reviews\n");
        foreach (var week in weeks)
            weekly.AppendLine($"{week:yyyy-MM-dd},{commits.GetValueOrDefault(week)},{prs.GetValueOrDefault(week)}," +
                              $"{issues.GetValueOrDefault(week)},{releases.GetValueOrDefault(week)},{reviews.GetValueOrDefault(week)}");
        File.WriteAllText(Path.Combine(outDir, "activities_weekly.csv"), weekly.ToString());

        (long Count, int Peak, DateOnly PeakWeek) Stat(long total, IReadOnlyDictionary<DateOnly, int> byWeek)
        {
            var peak = byWeek.OrderByDescending(kv => kv.Value).ThenBy(kv => kv.Key).First();
            return (total, peak.Value, peak.Key);
        }

        var series = new (string Name, long Count, int Peak, DateOnly PeakWeek)[]
        {
            ("Issues opened", issues.Values.Sum(v => (long)v), 0, default),
            ("Pull requests opened", prs.Values.Sum(v => (long)v), 0, default),
            ("Commits (default branch)", source.Commits.Count, 0, default),
            ("Reviews submitted", source.Reviews.Count, 0, default),
            ("Releases published", source.Releases.Count, 0, default),
        };
        var maps = new Dictionary<string, IReadOnlyDictionary<DateOnly, int>>
        {
            ["Issues opened"] = issues,
            ["Pull requests opened"] = prs,
            ["Commits (default branch)"] = commits,
            ["Reviews submitted"] = reviews,
            ["Releases published"] = releases,
        };
        series = series.Select(s =>
        {
            var stat = Stat(s.Count, maps[s.Name]);
            return (s.Name, s.Count, stat.Peak, stat.PeakWeek);
        }).ToArray();

        var mean = series.Select(s => (long)Math.Round(s.Count / (double)weeks.Count)).ToArray();
        var tableRows = series.Select((s, i) => new[]
        {
            s.Name,
            s.Count.ToString(CultureInfo.InvariantCulture),
            mean[i].ToString(CultureInfo.InvariantCulture),
            $"{s.Peak} ({s.PeakWeek:yyyy-MM-dd})",
        }).ToArray();
        var tex = LatexWriter.Longtable(
            $"GitHub activities of anomalyco/opencode from {run.Since:d MMMM yyyy} to {run.Until.AddDays(-1):d MMMM yyyy}; " +
            $"counts cover the full window, means and peaks are per week over {weeks.Count} weeks.",
            "tab:activities-counts",
            ["Activity", "Count", "Mean/week", "Peak week"],
            @">{\raggedright\arraybackslash}p{0.32\textwidth} >{\raggedleft\arraybackslash}p{0.13\textwidth} >{\raggedleft\arraybackslash}p{0.14\textwidth} >{\raggedleft\arraybackslash}p{0.20\textwidth}",
            tableRows);
        File.WriteAllText(Path.Combine(outDir, "activities_counts.tex"), tex);

        var topReviews = new StringBuilder("login,is_bot,approved,commented,changes_requested,other,total\n");
        foreach (var login in data.Top)
        {
            var mine = source.Reviews.Where(r => r.Author.Equals(login, StringComparison.OrdinalIgnoreCase)).ToList();
            var approved = mine.Count(r => r.State == ReviewState.Approved);
            var commented = mine.Count(r => r.State == ReviewState.Commented);
            var changes = mine.Count(r => r.State == ReviewState.ChangesRequested);
            topReviews.AppendLine(
                $"{login},{Bots.IsBot(login)},{approved},{commented},{changes}," +
                $"{mine.Count - approved - commented - changes},{mine.Count}");
        }
        var botReviews = source.Reviews.Where(r => IsReviewBot(r.Author)).ToList();
        var botApproved = botReviews.Count(r => r.State == ReviewState.Approved);
        var botCommented = botReviews.Count(r => r.State == ReviewState.Commented);
        var botChanges = botReviews.Count(r => r.State == ReviewState.ChangesRequested);
        topReviews.AppendLine($"review_bots,true,{botApproved},{botCommented},{botChanges}," +
                              $"{botReviews.Count - botApproved - botCommented - botChanges},{botReviews.Count}");
        File.WriteAllText(Path.Combine(outDir, "reviews_top.csv"), topReviews.ToString());

        foreach (var s in series)
            Console.WriteLine($"{s.Name,-28} {s.Count,7} {mean[Array.IndexOf(series, s)],6} {s.Peak} ({s.PeakWeek:yyyy-MM-dd})");
        Console.WriteLine($"weeks: {weeks.Count}; review bot share: {botReviews.Count}/{source.Reviews.Count}");
        Console.WriteLine(topReviews.ToString());
    }

    static bool IsReviewBot(string login) =>
        Bots.IsBot(login)
        || login.Equals("copilot-pull-request-reviewer", StringComparison.OrdinalIgnoreCase)
        || login.Equals("greptile-apps", StringComparison.OrdinalIgnoreCase);
}
