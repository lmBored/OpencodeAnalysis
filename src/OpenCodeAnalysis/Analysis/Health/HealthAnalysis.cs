namespace OpenCodeAnalysis;

/// Contributor categories, activity and process metrics over filtered persons (pure LINQ, no I/O).
public static class HealthAnalysis
{
    public const double KeyShare = 0.8; // key: smallest set of persons with 80% of merged human PRs
    public const int RegularMonths = 3; // regular: active in at least 3 calendar months
    public const int RetentionDays = 30; // retained newcomer: active again 30 to 90 days after the first contribution
    public const int RetentionHorizonDays = 90; // fixed horizon so early and late cohorts are comparable
    public const double DormantQuantile = 0.9; // dormant: silent for longer than the 90th percentile gap of regulars

    public static readonly string[] Categories = ["key", "regular", "occasional", "casual"];

    public record Event(string Person, string Kind, DateTimeOffset At, int Number);

    public record Result(
        List<Event> Events,
        List<PersonActivity> People,
        Dictionary<string, PersonActivity> ById,
        int KeyRank,
        double KeyShareReached,
        double DormantGapDays,
        List<(int Rank, string Person, int Merged, double CumShare)> Curve,
        List<double> Gaps,
        HashSet<string> PriorContributors,
        ILookup<string, DateTimeOffset> Times)
    {
        /// Active again between RetentionDays and RetentionHorizonDays after the first contribution
        public bool Retained(PersonActivity p) =>
            Times[p.PersonId].Any(t => t >= p.First.AddDays(RetentionDays) && t < p.First.AddDays(RetentionHorizonDays));

        /// Only newcomers with the full horizon left in the window can be judged
        public static bool Eligible(PersonActivity p, RunInfo run) => p.First <= run.Until.AddDays(-RetentionHorizonDays);
    }

    /// Auto-close comments of opencode workflows and scripts (texts from .github/workflows and script/github)
    public static string? AutoCloseKind(string body)
    {
        if (body.Contains("This pull request has been automatically closed because it was not updated to meet our")) return "pr-compliance";
        if (body.Contains("Automated PR Cleanup")) return "pr-cleanup";
        if (body.Contains("Closing this pull request because it has had no updates for more than")) return "pr-stale";
        if (body.Contains("This issue has been automatically closed because it was not updated to meet our")) return "issue-compliance";
        if (body.Contains("To stay organized issues are automatically closed after")) return "issue-stale";
        return null;
    }

    static string Month(DateTimeOffset d) => d.UtcDateTime.ToString("yyyy-MM");

    public static List<Event> Events(Dataset data, RunInfo run, IdentityResolver who)
    {
        bool In(DateTimeOffset d) => d >= run.Since && d < run.Until;
        var reviews = data.Activities?.Reviews ?? [];
        return data.Commits.Where(c => In(c.Date)).Select(c => new Event(who.OfCommit(c).Id, "commit", c.Date, 0))
            .Concat(data.Pulls.Select(p => new Event(who.OfLogin(p.Login).Id, "pr", p.CreatedAt, p.Number)))
            .Concat(reviews.Where(r => In(r.SubmittedAt) && r.Author.Length > 0)
                .Select(r => new Event(who.OfLogin(r.Author).Id, "review", r.SubmittedAt, r.Pr)))
            .Concat(data.ReviewComments.Select(c => new Event(who.OfLogin(c.Login).Id, "review_comment", c.CreatedAt, c.Number)))
            .Concat(data.IssueComments.Select(c => new Event(who.OfLogin(c.Login).Id, "issue_comment", c.CreatedAt, c.Number)))
            .Concat(data.IssueItems.Where(i => !i.IsPull && In(i.CreatedAt))
                .Select(i => new Event(who.OfLogin(i.Login).Id, "issue", i.CreatedAt, i.Number)))
            .ToList();
    }

    static double Quantile(IReadOnlyList<double> sorted, double q)
    {
        if (sorted.Count == 0) return double.NaN;
        var pos = q * (sorted.Count - 1);
        var lo = (int)Math.Floor(pos);
        var hi = Math.Min(lo + 1, sorted.Count - 1);
        return sorted[lo] + (pos - lo) * (sorted[hi] - sorted[lo]);
    }

    public static double Median(IEnumerable<double> xs) => Quantile(xs.Order().ToList(), 0.5);

    public static Result Categorize(Dataset data, RunInfo run, IdentityResolver who, Func<string, bool> isBotPerson)
    {
        bool Human(string id) => !isBotPerson(id) && id != "ghost";
        var events = Events(data, run, who);
        var human = events.Where(e => Human(e.Person)).ToList();

        // Key: cumulative share of merged human PRs by person
        var merged = data.Pulls.Where(p => p.MergedAt is not null)
            .Select(p => who.OfLogin(p.Login).Id).Where(Human)
            .GroupBy(id => id).Select(g => (Person: g.Key, Merged: g.Count()))
            .OrderByDescending(x => x.Merged).ThenBy(x => x.Person, StringComparer.Ordinal).ToList();
        var total = merged.Sum(x => x.Merged);
        var running = 0;
        var curve = merged.Select((x, i) => (Rank: i + 1, x.Person, x.Merged, CumShare: (running += x.Merged) / (double)total)).ToList();
        var keyRank = curve.First(c => c.CumShare >= KeyShare).Rank;
        var key = curve.Take(keyRank).Select(c => c.Person).ToHashSet();

        // Earlier contributions: PRs, issues and commits before the window (comments before the window are not fetched)
        var prior = data.IssueItems.Where(i => i.CreatedAt < run.Since).Select(i => who.OfLogin(i.Login).Id)
            .Concat(data.PriorCommits.Select(c => who.OfCommit(c).Id))
            .ToHashSet();

        var byPerson = human.GroupBy(e => e.Person).ToDictionary(g => g.Key, g => g.OrderBy(e => e.At).ToList());
        int Months(List<Event> es) => es.Select(e => Month(e.At)).Distinct().Count();

        // Dormant threshold: gaps between consecutive active days of regulars
        var regularIds = byPerson.Where(kv => !key.Contains(kv.Key) && Months(kv.Value) >= RegularMonths).Select(kv => kv.Key).ToHashSet();
        var gaps = regularIds.SelectMany(id =>
        {
            var days = byPerson[id].Select(e => e.At.UtcDateTime.Date).Distinct().Order().ToList();
            return days.Zip(days.Skip(1), (a, b) => (b - a).TotalDays);
        }).Order().ToList();
        var dormantGap = Quantile(gaps, DormantQuantile);

        var mergedBy = merged.ToDictionary(x => x.Person, x => x.Merged);
        var people = byPerson.Select(kv =>
        {
            var (id, es) = (kv.Key, kv.Value);
            var months = Months(es);
            var isKey = key.Contains(id);
            var isRegular = !isKey && months >= RegularMonths;
            var isCasual = !isKey && !isRegular && es.Count == 1;
            var category = isKey ? "key" : isRegular ? "regular" : isCasual ? "casual" : "occasional";
            var dormant = !isCasual && (run.Until - es[^1].At).TotalDays > dormantGap;
            int Count(string kind) => es.Count(e => e.Kind == kind);
            return new PersonActivity(id, category, isKey, isRegular, !prior.Contains(id), isCasual, dormant,
                Count("commit"), Count("pr"), mergedBy.GetValueOrDefault(id), Count("review"), Count("review_comment"),
                Count("issue_comment"), Count("issue"), es[0].At, es[^1].At, months,
                es.Select(e => e.At.UtcDateTime.Date).Distinct().Count());
        }).OrderBy(p => Array.IndexOf(Categories, p.Category)).ThenByDescending(p => p.PrsMerged).ThenBy(p => p.PersonId, StringComparer.Ordinal).ToList();

        return new Result(events, people, people.ToDictionary(p => p.PersonId), keyRank, curve[keyRank - 1].CumShare,
            dormantGap, curve, gaps, prior, human.ToLookup(e => e.Person, e => e.At));
    }

    public record PrFacts(PullRecord Pr, string Author, string Group, bool Newcomer, bool Merged, string? AutoClosed,
        double? FirstHumanResponseHours, bool BotResponded);

    /// Each PR of a human author: outcome, auto-close, first response by another human (comment, review comment or review)
    public static List<PrFacts> Prs(Dataset data, IdentityResolver who, Result cat, Func<string, bool> isBotPerson)
    {
        var reviews = data.Activities?.Reviews ?? [];
        var responses = data.IssueComments.Concat(data.ReviewComments)
            .Select(c => (c.Number, Person: who.OfLogin(c.Login).Id, c.CreatedAt, Body: c.Body))
            .Concat(reviews.Where(r => r.Author.Length > 0).Select(r => (Number: r.Pr, Person: who.OfLogin(r.Author).Id, CreatedAt: r.SubmittedAt, Body: r.Body)))
            .GroupBy(x => x.Number)
            .ToDictionary(g => g.Key, g => g.OrderBy(x => x.CreatedAt).ToList());
        return data.Pulls
            .Select(p => (Pr: p, Author: who.OfLogin(p.Login).Id))
            .Where(x => cat.ById.ContainsKey(x.Author))
            .Select(x =>
            {
                var rs = responses.GetValueOrDefault(x.Pr.Number) ?? [];
                var others = rs.Where(r => r.Person != x.Author && r.CreatedAt >= x.Pr.CreatedAt).ToList();
                var firstHuman = others.FirstOrDefault(r => !isBotPerson(r.Person) && r.Person != "ghost");
                var auto = x.Pr.MergedAt is null
                    ? rs.Select(r => AutoCloseKind(r.Body)).FirstOrDefault(k => k is not null && k.StartsWith("pr-"))
                    : null;
                var person = cat.ById[x.Author];
                return new PrFacts(x.Pr, x.Author, person.Category, person.Newcomer, x.Pr.MergedAt is not null, auto,
                    firstHuman.Person is null ? null : (firstHuman.CreatedAt - x.Pr.CreatedAt).TotalHours,
                    others.Any(r => isBotPerson(r.Person)));
            }).ToList();
    }

    public record Retention(int Frame, int Retained, double Share);

    /// Newcomers whose first contribution leaves the full horizon in the window
    /// Retained if active again 30-90 days later
    public static Retention NewcomerRetention(Result cat, RunInfo run, Func<PersonActivity, bool>? filter = null)
    {
        var frame = cat.People.Where(p => p.Newcomer && (filter?.Invoke(p) ?? true) && Result.Eligible(p, run)).ToList();
        var retained = frame.Count(cat.Retained);
        return new Retention(frame.Count, retained, frame.Count == 0 ? 0 : (double)retained / frame.Count);
    }

    public record Concentration(int Reviews, int Reviewers, double Top3Share, List<(string Person, int Reviews)> Top);

    /// Human reviews on someone else's PR and the share of the three most active reviewers.
    public static Concentration ReviewConcentration(Dataset data, IdentityResolver who, Func<string, bool> isBotPerson)
    {
        var reviews = (data.Activities?.Reviews ?? [])
            .Where(r => r.Author.Length > 0)
            .Select(r => (Reviewer: who.OfLogin(r.Author).Id, Author: r.PrAuthor.Length > 0 ? who.OfLogin(r.PrAuthor).Id : ""))
            .Where(r => !isBotPerson(r.Reviewer) && r.Reviewer != "ghost" && r.Reviewer != r.Author)
            .ToList();
        var top = reviews.GroupBy(r => r.Reviewer).Select(g => (Person: g.Key, Reviews: g.Count()))
            .OrderByDescending(x => x.Reviews).ThenBy(x => x.Person, StringComparer.Ordinal).ToList();
        return new Concentration(reviews.Count, top.Count,
            reviews.Count == 0 ? 0 : top.Take(3).Sum(x => x.Reviews) / (double)reviews.Count, top.Take(10).ToList());
    }

    public record MonthRow(string Month, Dictionary<string, int> ActiveByCategory, int Newcomers, int NewcomersEligible,
        int NewcomersRetained, int HumanPrs, int AutoClosedPrs, int Events, int BotEvents);

    /// Retention each month counts only newcomers who still had the full horizon left in the window
    public static List<MonthRow> Monthly(Result cat, List<PrFacts> prs, Func<string, bool> isBotPerson, RunInfo run)
    {
        var months = cat.Events.Select(e => Month(e.At)).Distinct().Order().ToList();
        return months.Select(m =>
        {
            var inMonth = cat.Events.Where(e => Month(e.At) == m).ToList();
            var active = inMonth.Select(e => e.Person).Distinct().Where(cat.ById.ContainsKey).Select(id => cat.ById[id]).ToList();
            var newcomers = cat.People.Where(p => p.Newcomer && Month(p.First) == m).ToList();
            var eligible = newcomers.Where(p => Result.Eligible(p, run)).ToList();
            var monthPrs = prs.Where(p => Month(p.Pr.CreatedAt) == m).ToList();
            return new MonthRow(m,
                Categories.ToDictionary(c => c, c => active.Count(p => p.Category == c)),
                newcomers.Count,
                eligible.Count,
                eligible.Count(cat.Retained),
                monthPrs.Count,
                monthPrs.Count(p => p.AutoClosed is not null),
                inMonth.Count,
                inMonth.Count(e => isBotPerson(e.Person)));
        }).ToList();
    }
}
