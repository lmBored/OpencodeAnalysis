using System.Globalization;
using System.Text;
using GhCore;

namespace OpenCodeAnalysis;

public static class Reports
{
    const int CommitSampleSize = 30;
    static readonly string RepoUrl = $"https://github.com/{GitHubFetcher.Owner}/{GitHubFetcher.Repo}";

    static string Csv(string s) => s.Contains(',') || s.Contains('"') || s.Contains('\n') ? $"\"{s.Replace("\"", "\"\"")}\"" : s;

    static string FirstLine(string s, int max = 110)
    {
        var line = s.Split('\n')[0].Trim();
        return line.Length <= max ? line : line[..max] + "...";
    }

    public static void Contributors(Dataset data, RunInfo run, string outDir, int topN)
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
            csv.AppendLine($"{Csv(r.Login)},{r.IsBot},{r.Commits},{r.PullsOpened},{(data.ReviewCounts.ContainsKey(r.Login) ? r.PrsReviewed : "")},{r.Total}");
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
            scopes.AppendLine($"{r.Login},{share:F0}%,{Csv(Fmt(types))},{Csv(Fmt(scopeTop))},{Csv(Fmt(packages))}");

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

    public static void Collaboration(Dataset data, RunInfo run, string outDir, int topN)
    {
        var top = data.Top;

        // author x actor matrix
        var matrix = new StringBuilder("author \\ actor," + string.Join(',', top) + "\n");
        foreach (var author in top)
        {
            var cells = top.Select(actor => actor == author ? "" :
                CollaborationAnalysis.Directed(data, author, actor) is var i ? $"{i.Reviews}r/{i.Comments}c/{i.Mentions}m" : "");
            matrix.AppendLine(author + "," + string.Join(',', cells));
        }
        File.WriteAllText(Path.Combine(outDir, "review_matrix.csv"), matrix.ToString());

        // ranked pairs
        var pairs = CollaborationAnalysis.RankPairs(data);
        var ranked = new StringBuilder("a,b,b_on_a_reviews,b_on_a_comments,b_mentions_a,a_on_b_reviews,a_on_b_comments,a_mentions_b,weaker_direction,total\n");
        foreach (var p in pairs)
            ranked.AppendLine($"{p.A},{p.B},{p.AtoB.Reviews},{p.AtoB.Comments},{p.AtoB.Mentions},{p.BtoA.Reviews},{p.BtoA.Comments},{p.BtoA.Mentions},{p.Weaker},{p.Total}");
        File.WriteAllText(Path.Combine(outDir, "pairs_ranked.csv"), ranked.ToString());

        // targeted threads of the strongest pairs
        var md = new StringBuilder();
        md.AppendLine($"# Candidate threads (targeted sample; window {run.Since:yyyy-MM-dd}..{run.Until:yyyy-MM-dd})");
        md.AppendLine("Frame: PRs created in the window by either member; filter: >=3 comments by the two members together and >=1 by the non-author.");
        md.AppendLine();
        foreach (var p in pairs.Take(5))
        {
            var threads = CollaborationAnalysis.Threads(data, p.A, p.B);
            md.AppendLine($"## {p.A} <-> {p.B} (weaker direction {p.Weaker}, total {p.Total}; {threads.Count} threads)");
            foreach (var t in threads.OrderByDescending(t => Math.Min(t.ByA, t.ByB)).ThenByDescending(t => t.ByA + t.ByB).Take(12))
            {
                md.AppendLine($"### #{t.Number} by {t.Author} ({t.CreatedAt:yyyy-MM-dd}) {t.Title}");
                md.AppendLine($"{t.Url} | comments {p.A}={t.ByA}, {p.B}={t.ByB}, formal review between them: {(t.Reviews > 0 ? "yes" : "no")}");
                foreach (var c in t.Comments.Take(8))
                    md.AppendLine($"- {c.CreatedAt:yyyy-MM-dd HH:mm} **{c.Login}** ({c.Kind}{(c.Path is null ? "" : " " + c.Path)}): {FirstLine(c.Body, 220)}");
                md.AppendLine();
            }
        }
        File.WriteAllText(Path.Combine(outDir, "pairs_candidates.md"), md.ToString());

        // counterexample: co-editing without interaction
        var coEdits = CollaborationAnalysis.CoEdits(data, pairs);
        var ce = new StringBuilder("a,b,shared_files,files_a,files_b,jaccard,mutual_interactions,example_shared_files\n");
        foreach (var c in coEdits)
            ce.AppendLine($"{c.A},{c.B},{c.SharedFiles},{c.FilesA},{c.FilesB},{c.Jaccard:F3},{c.Interactions},{Csv(string.Join(" ", c.Examples))}");
        File.WriteAllText(Path.Combine(outDir, "counter_example.csv"), ce.ToString());

        Console.WriteLine(ranked.ToString());
        Console.WriteLine(ce.ToString());
    }

    /// Weekly series, counts table and per-human review breakdown from GhActivities' data/activities/.
    public static void Activities(Dataset data, RunInfo run, string outDir, int topN)
    {
        var source = data.Activities;
        if (source is null)
        {
            Console.WriteLine("activities: data/activities/search_weekly.csv not found - collect with GhActivities first");
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
        var issues = source.Weeks.ToDictionary(w => w.Week, w => w.Issues);
        var prs = source.Weeks.ToDictionary(w => w.Week, w => w.Prs);
        var weeks = issues.Keys.Union(commits.Keys).Union(releases.Keys).Union(reviews.Keys).OrderBy(w => w).ToList();

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
            ("Issues opened", source.Weeks.Sum(w => (long)w.Issues), 0, default),
            ("Pull requests opened", source.Weeks.Sum(w => (long)w.Prs), 0, default),
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
            var approved = mine.Count(r => r.State == "approved");
            var commented = mine.Count(r => r.State == "commented");
            var changes = mine.Count(r => r.State == "changes_requested");
            topReviews.AppendLine(
                $"{login},{Bots.IsBot(login)},{approved},{commented},{changes}," +
                $"{mine.Count - approved - commented - changes},{mine.Count}");
        }
        var botReviews = source.Reviews.Where(r => IsReviewBot(r.Author)).ToList();
        var botApproved = botReviews.Count(r => r.State == "approved");
        var botCommented = botReviews.Count(r => r.State == "commented");
        var botChanges = botReviews.Count(r => r.State == "changes_requested");
        topReviews.AppendLine($"review_bots,true,{botApproved},{botCommented},{botChanges}," +
                              $"{botReviews.Count - botApproved - botCommented - botChanges},{botReviews.Count}");
        File.WriteAllText(Path.Combine(outDir, "reviews_top.csv"), topReviews.ToString());

        foreach (var s in series)
            Console.WriteLine($"{s.Name,-28} {s.Count,7} {mean[Array.IndexOf(series, s)],6} {s.Peak} ({s.PeakWeek:yyyy-MM-dd})");
        Console.WriteLine($"weeks: {weeks.Count}; review bot share: {botReviews.Count}/{source.Reviews.Count}");
        Console.WriteLine(topReviews.ToString());
    }

    const int GroupSampleSize = 30;
    static string Pct(double x) => (100 * x).ToString("F0", CultureInfo.InvariantCulture) + "\\%";
    static string N(int x) => x.ToString("N0", CultureInfo.InvariantCulture).Replace(",", "{,}");
    static string Href(string url, string text) => $"\\href{{{url}}}{{\\texttt{{{LatexWriter.Escape(text)}}}}}";
    static string F(double x, string fmt = "F2") => x.ToString(fmt, CultureInfo.InvariantCulture);

    /// Project health, items 1-3: filtering (bots, aliases), categories, activity and process metrics, samples.
    public static void Health(Dataset data, RunInfo run, string outDir, string overridesPath, string teamPath)
    {
        if (data.IssueItems.Count == 0)
            throw new InvalidOperationException("health: data/issue_items.json missing - run `dotnet run -- fetch-health` first");
        var overrides = IdentityOverrides.Load(overridesPath);
        var filter = HealthFiltering(data, run, outDir, overrides);
        Console.WriteLine($"health: {filter.Resolver.Stats}");
        var team = File.ReadAllLines(teamPath).Select(l => l.Trim()).Where(l => l.Length > 0)
            .Select(l => filter.Resolver.OfLogin(l).Id).ToHashSet();
        HealthMetrics(data, run, outDir, filter, team);
    }

    const int NewcomerPrsPerStratum = 6; // 5 calendar quarters x team/non-team x 6 = 60 PRs

    /// Categories with data thresholds, activity per category, process metrics, T1, T3 and the PR sample.
    static void HealthMetrics(Dataset data, RunInfo run, string outDir, Filtering filter, HashSet<string> team)
    {
        var who = filter.Resolver;
        var isBot = filter.IsBotPerson;
        var cat = HealthAnalysis.Categorize(data, run, who, isBot);
        var prs = HealthAnalysis.Prs(data, who, cat, isBot);
        var retention = HealthAnalysis.NewcomerRetention(cat, run);
        var conc = HealthAnalysis.ReviewConcentration(data, who, isBot);
        var monthly = HealthAnalysis.Monthly(cat, prs, isBot, run);
        var people = cat.People;
        string GroupOf(string id) => isBot(id) ? "bot" : id == "ghost" ? "deleted" : cat.ById.TryGetValue(id, out var p) ? p.Category : "none";
        var inv = CultureInfo.InvariantCulture;

        // health_activity.csv (F1): activity type x group, plus the newcomer subset of humans
        var groups = new[] { "bot", "key", "regular", "occasional", "casual", "deleted" };
        var kinds = new[] { "commit", "pr", "pr_merged", "review", "review_comment", "issue_comment", "issue" };
        var mergedEvents = data.Pulls.Where(p => p.MergedAt is not null)
            .Select(p => new HealthAnalysis.Event(who.OfLogin(p.Login).Id, "pr_merged", p.MergedAt!.Value, p.Number));
        var all = cat.Events.Concat(mergedEvents).ToList();
        var activity = new StringBuilder("kind," + string.Join(',', groups) + ",newcomer,total\n");
        foreach (var k in kinds)
        {
            var es = all.Where(e => e.Kind == k).ToList();
            var byGroup = es.GroupBy(e => GroupOf(e.Person)).ToDictionary(g => g.Key, g => g.Count());
            var newcomer = es.Count(e => cat.ById.TryGetValue(e.Person, out var p) && p.Newcomer);
            activity.AppendLine($"{k},{string.Join(',', groups.Select(g => byGroup.GetValueOrDefault(g)))},{newcomer},{es.Count}");
        }
        File.WriteAllText(Path.Combine(outDir, "health_activity.csv"), activity.ToString());

        // health_people.csv: identities plus category and activity (bots and deleted accounts included)
        var counts = all.GroupBy(e => e.Person).ToDictionary(g => g.Key, g => g.GroupBy(e => e.Kind).ToDictionary(x => x.Key, x => x.Count()));
        var peopleCsv = new StringBuilder("person_id,logins,is_bot,team,category,newcomer,dormant,commits,prs_opened,prs_merged,reviews,review_comments,issue_comments,issues_opened,first,last,active_months,active_days,merged_by\n");
        foreach (var p in who.People)
        {
            var c = counts.GetValueOrDefault(p.Id) ?? [];
            var a = cat.ById.GetValueOrDefault(p.Id);
            peopleCsv.AppendLine($"{Csv(p.Id)},{Csv(string.Join(' ', p.Logins))},{isBot(p.Id)},{team.Contains(p.Id)},{GroupOf(p.Id)},{a?.Newcomer},{a?.Dormant}," +
                                 string.Join(',', kinds.Select(k => c.GetValueOrDefault(k))) +
                                 $",{a?.First:yyyy-MM-dd},{a?.Last:yyyy-MM-dd},{a?.ActiveMonths},{a?.ActiveDays},{p.MergedBy}");
        }
        File.WriteAllText(Path.Combine(outDir, "health_people.csv"), peopleCsv.ToString());

        // F2 inputs: cumulative merged-PR share by rank, and inactivity gaps of regulars
        var rank = new StringBuilder("rank,person,merged,cum_share,team\n");
        foreach (var r in cat.Curve)
            rank.AppendLine($"{r.Rank},{Csv(r.Person)},{r.Merged},{r.CumShare.ToString("F4", inv)},{team.Contains(r.Person)}");
        File.WriteAllText(Path.Combine(outDir, "health_rank.csv"), rank.ToString());
        File.WriteAllText(Path.Combine(outDir, "health_gaps.csv"),
            "gap_days\n" + string.Join('\n', cat.Gaps.Select(g => g.ToString("F0", inv))) + "\n");

        // health_monthly.csv (F3)
        var month = new StringBuilder("month," + string.Join(',', HealthAnalysis.Categories) + ",newcomers,newcomers_eligible,newcomers_retained,human_prs,auto_closed_prs,events,bot_events\n");
        foreach (var m in monthly)
            month.AppendLine($"{m.Month},{string.Join(',', HealthAnalysis.Categories.Select(c => m.ActiveByCategory[c]))},{m.Newcomers}," +
                             $"{m.NewcomersEligible},{m.NewcomersRetained},{m.HumanPrs},{m.AutoClosedPrs},{m.Events},{m.BotEvents}");
        File.WriteAllText(Path.Combine(outDir, "health_monthly.csv"), month.ToString());

        // health_process.csv: merge rate, first human response, auto-close share per author group
        var process = new StringBuilder("metric,group,value,n\n");
        var prGroups = new List<(string Name, Func<HealthAnalysis.PrFacts, bool> In)>
        {
            ("all", _ => true),
            ("key", p => p.Group == "key"), ("regular", p => p.Group == "regular"),
            ("occasional", p => p.Group == "occasional"), ("casual", p => p.Group == "casual"),
            ("newcomer", p => p.Newcomer), ("returning", p => !p.Newcomer),
            ("outside-newcomer", p => p.Newcomer && !team.Contains(p.Author)),
            ("team", p => team.Contains(p.Author)), ("non-team", p => !team.Contains(p.Author)),
        };
        var stats = prGroups.Select(g =>
        {
            var xs = prs.Where(g.In).ToList();
            var responded = xs.Where(p => p.FirstHumanResponseHours is not null).ToList();
            return (g.Name, N: xs.Count,
                Merge: xs.Count == 0 ? 0 : xs.Count(p => p.Merged) / (double)xs.Count,
                Responded: xs.Count == 0 ? 0 : responded.Count / (double)xs.Count,
                Median: responded.Count == 0 ? double.NaN : HealthAnalysis.Median(responded.Select(p => p.FirstHumanResponseHours!.Value)),
                Auto: xs.Count == 0 ? 0 : xs.Count(p => p.AutoClosed is not null) / (double)xs.Count);
        }).ToList();
        foreach (var s in stats)
        {
            process.AppendLine($"merge_rate,{s.Name},{s.Merge.ToString("F3", inv)},{s.N}");
            process.AppendLine($"human_response_share,{s.Name},{s.Responded.ToString("F3", inv)},{s.N}");
            process.AppendLine($"first_human_response_median_h,{s.Name},{s.Median.ToString("F1", inv)},{s.N}");
            process.AppendLine($"auto_closed_share,{s.Name},{s.Auto.ToString("F3", inv)},{s.N}");
        }
        foreach (var g in prs.Where(p => p.AutoClosed is not null).GroupBy(p => p.AutoClosed!).OrderBy(g => g.Key))
            process.AppendLine($"auto_closed_kind,{g.Key},{(g.Count() / (double)prs.Count).ToString("F3", inv)},{g.Count()}");
        var windowIssues = data.IssueItems.Where(i => !i.IsPull && i.CreatedAt >= run.Since && i.CreatedAt < run.Until).Select(i => i.Number).ToHashSet();
        var issueAuto = data.IssueComments.Where(c => windowIssues.Contains(c.Number))
            .Select(c => (c.Number, Kind: HealthAnalysis.AutoCloseKind(c.Body))).Where(x => x.Kind is not null && x.Kind.StartsWith("issue-"))
            .DistinctBy(x => x.Number).ToList();
        foreach (var g in issueAuto.GroupBy(x => x.Kind!).OrderBy(g => g.Key))
            process.AppendLine($"issue_auto_closed_kind,{g.Key},{(g.Count() / (double)windowIssues.Count).ToString("F3", inv)},{g.Count()}");
        process.AppendLine($"newcomer_retention,all,{retention.Share.ToString("F3", inv)},{retention.Frame}");
        // retention by the kind of the first contribution (categories would be circular: they depend on the active span)
        var firstKind = human(cat.Events).GroupBy(e => e.Person).ToDictionary(g => g.Key, g => g.MinBy(e => e.At)!.Kind);
        IEnumerable<HealthAnalysis.Event> human(IEnumerable<HealthAnalysis.Event> es) => es.Where(e => cat.ById.ContainsKey(e.Person));
        var retentionByKind = new[] { "pr", "issue", "issue_comment" }
            .Select(k => (Kind: k, R: HealthAnalysis.NewcomerRetention(cat, run, p => firstKind.GetValueOrDefault(p.PersonId) == k))).ToList();
        foreach (var (k, r) in retentionByKind)
            process.AppendLine($"newcomer_retention_first_{k},all,{r.Share.ToString("F3", inv)},{r.Frame}");
        process.AppendLine($"review_top3_share,all,{conc.Top3Share.ToString("F3", inv)},{conc.Reviews}");
        foreach (var t in conc.Top)
            process.AppendLine($"reviews_by,{Csv(t.Person)},{t.Reviews},{conc.Reviews}");
        File.WriteAllText(Path.Combine(outDir, "health_process.csv"), process.ToString());

        // T1: metrics with definition, threshold, rationale and value
        var byCat = HealthAnalysis.Categories.ToDictionary(c => c, c => people.Count(p => p.Category == c));
        var humans = people.Count;
        var newcomers = people.Count(p => p.Newcomer);
        var dormant = people.Count(p => p.Dormant);
        var nonCasual = people.Count(p => p.Category != "casual");
        var keyTeam = cat.Curve.Take(cat.KeyRank).Count(r => team.Contains(r.Person));
        var st = stats.ToDictionary(s => s.Name);
        var top3 = string.Join(", ", conc.Top.Take(3).Select(t => $"\\texttt{{{LatexWriter.Escape(t.Person)}}}"));
        List<string[]> t1 =
        [
            ["Key", "Smallest set of persons who author 80\\% of merged human PRs", $"top {cat.KeyRank} (knee of the curve)",
                $"{N(byCat["key"])} persons, {Pct(cat.KeyShareReached)} of merged PRs; {keyTeam} in \\texttt{{TEAM\\_MEMBERS}}"],
            ["Regular", "Not key; active in $\\geq$3 calendar months", "3 months = one quarter", $"{N(byCat["regular"])} persons"],
            ["Occasional", "Not key or regular; $\\geq$2 contributions", "--", $"{N(byCat["occasional"])} persons"],
            ["Casual", "Exactly one contribution of any kind", "1", $"{N(byCat["casual"])} persons ({Pct(byCat["casual"] / (double)humans)})"],
            ["Newcomer", "No PR, issue or commit before the window", "window start", $"{N(newcomers)} persons ({Pct(newcomers / (double)humans)})"],
            ["Dormant", "Not casual; silent at window end for longer than the usual gap", $"{cat.DormantGapDays.ToString("F0", inv)} days (P90 gap of regulars)",
                $"{N(dormant)} of {N(nonCasual)} ({Pct(dormant / (double)nonCasual)})"],
            ["Merge rate", "Merged / opened PRs, by author group", "--", $"key {Pct(st["key"].Merge)}, outside newcomer {Pct(st["outside-newcomer"].Merge)}, casual {Pct(st["casual"].Merge)}"],
            ["First response", "Hours from PR opened to the first comment or review by another human", "median",
                $"key {st["key"].Median.ToString("F1", inv)} h, outside newcomer {st["outside-newcomer"].Median.ToString("F1", inv)} h; " +
                $"never answered: {Pct(1 - st["outside-newcomer"].Responded)} of outside-newcomer PRs"],
            ["Auto-closed", "PR closed with a workflow close comment", "3 templates", $"key {Pct(st["key"].Auto)}, outside newcomer {Pct(st["outside-newcomer"].Auto)}, all {Pct(st["all"].Auto)}"],
            ["Retention", "Newcomer active again 30--90 days after the first contribution", "30 days, 90-day horizon",
                $"{Pct(retention.Share)} of {N(retention.Frame)}; first contribution a PR: {Pct(retentionByKind[0].R.Share)}"],
            ["Review load", "Share of human reviews done by the top 3 reviewers", "top 3", $"{Pct(conc.Top3Share)} of {N(conc.Reviews)} ({top3})"],
        ];
        File.WriteAllText(Path.Combine(outDir, "health_metrics.tex"), LatexWriter.Table(
            $"Contributor categories and process metrics for {N(humans)} human persons active between {run.Since:d MMMM yyyy} and {run.Until.AddDays(-1):d MMMM yyyy}; thresholds are derived from the data.",
            "tab:health-metrics",
            ["Metric", "Definition", "Threshold", "Value"],
            @">{\raggedright\arraybackslash}p{0.12\textwidth} >{\raggedright\arraybackslash}p{0.33\textwidth} >{\raggedright\arraybackslash}p{0.17\textwidth} >{\raggedright\arraybackslash}p{0.30\textwidth}",
            t1));

        // Stratified sample of newcomer PRs (quarter x team) for qualitative coding
        var newcomerPrs = prs.Where(p => p.Newcomer).ToList();
        var sample = Sampling.StratifiedByQuarter(newcomerPrs.Where(p => team.Contains(p.Author)), p => p.Pr.CreatedAt, NewcomerPrsPerStratum, run.Seed, p => p.Pr.Number)
            .Concat(Sampling.StratifiedByQuarter(newcomerPrs.Where(p => !team.Contains(p.Author)), p => p.Pr.CreatedAt, NewcomerPrsPerStratum, run.Seed, p => p.Pr.Number))
            .OrderBy(p => p.Pr.CreatedAt).ToList();
        var comments = data.IssueComments.Concat(data.ReviewComments).GroupBy(c => c.Number).ToDictionary(g => g.Key, g => g.OrderBy(c => c.CreatedAt).ToList());
        var md = new StringBuilder();
        md.AppendLine($"# Stratified sample of newcomer PRs (calendar quarter x team, {NewcomerPrsPerStratum} per stratum, seed {run.Seed})");
        md.AppendLine($"Frame: {newcomerPrs.Count} PRs by newcomers ({newcomerPrs.Count(p => team.Contains(p.Author))} by team members). Sample: {sample.Count}.");
        md.AppendLine();
        // Coding: outcome and who answered first (team member, other human, only bots, nobody); each code was checked by reading the thread
        string Outcome(HealthAnalysis.PrFacts p) => p.Merged ? "merged" : p.AutoClosed ?? (p.Pr.State == "open" ? "open" : "closed");
        string Responder(HealthAnalysis.PrFacts p)
        {
            var firstHuman = (comments.GetValueOrDefault(p.Pr.Number) ?? [])
                .Select(c => who.OfLogin(c.Login).Id)
                .FirstOrDefault(id => id != p.Author && !isBot(id) && id != "ghost");
            return firstHuman is null ? (p.BotResponded ? "bots only" : "nobody") : team.Contains(firstHuman) ? "team member" : "other human";
        }
        md.AppendLine("| group | PRs | merged | auto-closed | closed (no bot note) | open | first reply: team | other human | bots only | nobody |");
        md.AppendLine("|---|---|---|---|---|---|---|---|---|---|");
        foreach (var g in sample.GroupBy(p => team.Contains(p.Author) ? "team newcomer" : "outside newcomer").OrderBy(g => g.Key))
        {
            var xs = g.ToList();
            int O(string o) => xs.Count(p => Outcome(p) == o);
            int R(string r) => xs.Count(p => Responder(p) == r);
            md.AppendLine($"| {g.Key} | {xs.Count} | {O("merged")} | {xs.Count(p => p.AutoClosed is not null)} | {O("closed")} | {O("open")} | " +
                          $"{R("team member")} | {R("other human")} | {R("bots only")} | {R("nobody")} |");
        }
        md.AppendLine();
        foreach (var p in sample)
        {
            var q = $"{p.Pr.CreatedAt:yyyy}Q{(p.Pr.CreatedAt.Month - 1) / 3 + 1}";
            md.AppendLine($"## #{p.Pr.Number} {q} {(team.Contains(p.Author) ? "team" : "non-team")} {p.Author} ({p.Group}) -> {Outcome(p)}; " +
                          $"first reply: {Responder(p)}; first human response {(p.FirstHumanResponseHours is { } h ? h.ToString("F1", inv) + " h" : "none")}");
            md.AppendLine($"{p.Pr.Title} | {p.Pr.Url}");
            foreach (var c in (comments.GetValueOrDefault(p.Pr.Number) ?? []).Where(c => who.OfLogin(c.Login).Id != p.Author).Take(4))
                md.AppendLine($"- {c.CreatedAt:yyyy-MM-dd HH:mm} {(isBot(who.OfLogin(c.Login).Id) ? "[bot] " : "")}{c.Login}: {FirstLine(c.Body.Replace('\r', ' '), 160)}");
            md.AppendLine();
        }
        File.WriteAllText(Path.Combine(outDir, "health_newcomer_prs.md"), md.ToString());

        // T3: sampling plan
        var strata = newcomerPrs.Select(p => (p.Pr.CreatedAt.Year, (p.Pr.CreatedAt.Month - 1) / 3, team.Contains(p.Author))).Distinct().Count();
        var merges = people.Count(p => who.OfLogin(p.PersonId).RawIdentities > 1);
        List<string[]> t3 =
        [
            ["All metrics in Table~\\ref{tab:health-metrics}", "None (full population)", $"{N(data.Pulls.Count)} PRs, {N(windowIssues.Count)} issues, {N(data.IssueComments.Count + data.ReviewComments.Count)} comments, {N(data.Commits.Count)} commits", "--",
                "All data is available, so sampling would only add error"],
            ["Precision of merged identities", "Simple random", "human persons with $>$1 identity", $"30 (seed {run.Seed})", "Every group has the same chance; checks the rules, not one person"],
            ["Bot flags", "Targeted", "logins with $\\geq$50 events, not M1/M2", "top 15 by M3+M4, plus 3 borderline", "Errors hide at the top of the score"],
            ["Fate of newcomer PRs", "Stratified (quarter $\\times$ team)", $"{N(newcomerPrs.Count)} newcomer PRs", $"{sample.Count} ({NewcomerPrsPerStratum} in each of {strata} non-empty strata, seed {run.Seed})",
                "Covers every policy period and the rare team newcomers"],
        ];
        File.WriteAllText(Path.Combine(outDir, "health_sampling.tex"), LatexWriter.Table(
            "Sampling plan: where we sample, how, and why; quantitative metrics use the full population.",
            "tab:health-sampling",
            ["Question", "Approach", "Frame", "Size", "Why"],
            @">{\raggedright\arraybackslash}p{0.17\textwidth} >{\raggedright\arraybackslash}p{0.13\textwidth} >{\raggedright\arraybackslash}p{0.22\textwidth} >{\raggedright\arraybackslash}p{0.15\textwidth} >{\raggedright\arraybackslash}p{0.23\textwidth}",
            t3));

        // Sanity checks and summary
        Check(byCat.Values.Sum() == humans, $"categories add up: {string.Join(" + ", byCat.Select(kv => $"{kv.Key} {kv.Value}"))} = {humans}");
        Check(cat.KeyRank is >= 15 and <= 25, $"key cut-off at rank {cat.KeyRank} (expected about 20)");
        Check(st["key"].Merge > 2 * st["outside-newcomer"].Merge, $"merge rate key {st["key"].Merge:P0} vs outside newcomer {st["outside-newcomer"].Merge:P0}");
        Check(sample.Count <= 60, $"newcomer PR sample {sample.Count} <= 60");
        Console.WriteLine($"metrics: humans {humans}, newcomers {newcomers}, dormant {dormant}/{nonCasual} (gap {cat.DormantGapDays:F0} d), " +
                          $"key {cat.KeyRank} ({keyTeam} team), retention {retention.Share:P1} of {retention.Frame}, review top3 {conc.Top3Share:P1} of {conc.Reviews}, " +
                          $"identities merged into persons with activity: {merges}");
        foreach (var s in stats)
            Console.WriteLine($"  {s.Name,-10} PRs {s.N,6} merged {s.Merge,6:P1} human reply {s.Responded,6:P1} median {s.Median,7:F1} h auto-closed {s.Auto,6:P1}");
    }

    public record Filtering(BotDetector.Result Bots, IdentityResolver Resolver, Func<string, bool> IsBotPerson);

    /// Item 2: bot measures M1-M4, alias rules A1-A3 with overrides; writes health_bots.csv, T2 and the manual checks.
    static Filtering HealthFiltering(Dataset data, RunInfo run, string outDir, IdentityOverrides overrides)
    {
        var reviews = data.Activities?.Reviews ?? [];
        var activityLogins = data.Pulls.Select(p => p.Login)
            .Concat(data.IssueComments.Select(c => c.Login))
            .Concat(data.ReviewComments.Select(c => c.Login))
            .Concat(reviews.Select(r => r.Author))
            .Concat(data.IssueItems.Select(i => i.Login))
            .Where(l => !string.IsNullOrEmpty(l))
            .ToList();
        var resolver = new IdentityResolver(data.Commits.Concat(data.PriorCommits).ToList(), activityLogins, overrides);
        var bots = BotDetector.Evaluate(data, run, overrides, resolver.KeyOf);
        var botPeople = resolver.People
            .Where(p => p.Logins.Any(bots.IsBot) || p.Logins.Any(l => Bots.IsBot(l))
                        || (p.Logins.Length == 0 && p.Names.Any(n => Bots.IsKnownAutomation(n) || Bots.IsBot(n))))
            .Select(p => p.Id).ToHashSet(StringComparer.OrdinalIgnoreCase);
        bool IsBotPerson(string id) => botPeople.Contains(id);

        // health_bots.csv: every login with its evidence
        var comments = data.IssueComments.Concat(data.ReviewComments).ToList();
        var commentsBy = comments.GroupBy(c => c.Login, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.Count(), StringComparer.OrdinalIgnoreCase);
        var csv = new StringBuilder("login,events,texts,comments,m1_type_bot,m2_known_automation,template_ratio,hour_entropy,m3,m4,is_bot,reason,profile_type,profile_created\n");
        foreach (var e in bots.Evidence)
        {
            var profile = data.Users.GetValueOrDefault(e.Login);
            csv.AppendLine($"{Csv(e.Login)},{e.Events},{e.Texts},{commentsBy.GetValueOrDefault(e.Login)},{e.TypeBot},{e.KnownAutomation}," +
                           $"{F(e.TemplateRatio, "F3")},{F(e.HourEntropy, "F3")},{e.TemplateRatio >= BotDetector.TemplateMin}," +
                           $"{e.HourEntropy >= BotDetector.EntropyMin},{e.IsBot},{Csv(e.Reason)},{profile?.Type},{profile?.CreatedAt:yyyy-MM-dd}");
        }
        File.WriteAllText(Path.Combine(outDir, "health_bots.csv"), csv.ToString());

        // health_people.csv: one line per person after merging (e-mails are counted, not listed)
        var peopleCsv = new StringBuilder("person_id,logins,names,emails,raw_identities,merged_by,is_bot\n");
        foreach (var p in resolver.People)
            peopleCsv.AppendLine($"{Csv(p.Id)},{Csv(string.Join(' ', p.Logins))},{Csv(string.Join(" / ", p.Names))},{p.Emails.Length}," +
                                 $"{p.RawIdentities},{p.MergedBy},{IsBotPerson(p.Id)}");
        File.WriteAllText(Path.Combine(outDir, "health_people.csv"), peopleCsv.ToString());

        // health_identities.csv: commit author (name, e-mail) -> person, used to count persons per package (und/package_persons.py)
        var identities = new StringBuilder("name,email,person_id,is_bot\n");
        foreach (var x in data.Commits.Concat(data.PriorCommits)
                     .GroupBy(c => (c.AuthorName, Email: c.AuthorEmail.ToLowerInvariant()))
                     .Select(g => (g.Key.AuthorName, g.Key.Email, Person: resolver.OfCommit(g.First()).Id))
                     .OrderBy(x => x.Person, StringComparer.Ordinal).ThenBy(x => x.AuthorName, StringComparer.Ordinal))
            identities.AppendLine($"{Csv(x.AuthorName)},{Csv(x.Email)},{Csv(x.Person)},{IsBotPerson(x.Person)}");
        File.WriteAllText(Path.Combine(outDir, "health_identities.csv"), identities.ToString());

        // numbers for T2
        var botComments = comments.Count(c => bots.IsBot(c.Login));
        var gha = comments.Count(c => c.Login.Equals("github-actions[bot]", StringComparison.OrdinalIgnoreCase));
        var m1 = bots.Evidence.Where(e => e.TypeBot).ToList();
        var m2 = bots.Evidence.Where(e => e.KnownAutomation && !e.TypeBot).ToList();
        var m3 = bots.Evidence.Where(e => e.TemplateRatio >= BotDetector.TemplateMin && !e.TypeBot && !e.KnownAutomation).ToList();
        var m4 = bots.Evidence.Where(e => e.HourEntropy >= BotDetector.EntropyMin && !e.TypeBot && !e.KnownAutomation).ToList();
        var m34 = m3.Where(e => e.HourEntropy >= BotDetector.EntropyMin).ToList();
        var actionCommits = data.Commits.Count(c => Bots.IsKnownAutomation(c.Login) && !Bots.IsBotAccount(c.Login));
        var agent = bots.Shared.FirstOrDefault(s => s.Login.StartsWith("opencode-agent", StringComparison.OrdinalIgnoreCase));
        var people = resolver.People;
        var humans = people.Where(p => !IsBotPerson(p.Id) && p.Id != "ghost").ToList();
        var st = resolver.Stats;
        var a1Example = humans.Where(p => p.Names.Length > 1 && p.Emails.Length > 0)
            .OrderByDescending(p => p.Names.Length).ThenBy(p => p.Id, StringComparer.Ordinal).FirstOrDefault();
        var a2Example = humans.Where(p => p.MergedBy.Contains("A2") && p.Login is not null)
            .OrderByDescending(p => p.Emails.Length).ThenBy(p => p.Id, StringComparer.Ordinal).FirstOrDefault();
        var a3Example = humans.Where(p => p.MergedBy.Contains("A3") && p.Login is not null)
            .OrderByDescending(p => p.RawIdentities).ThenBy(p => p.Id, StringComparer.Ordinal).FirstOrDefault();
        static string Plural(int n, string word) => $"{n} {word}{(n == 1 ? "" : "s")}";
        string Person(Person? p) => p is null ? "--" : Href($"https://github.com/{p.Login}", p.Login ?? p.Id) +
            $" ({Plural(p.Names.Length, "name")}, {Plural(p.Emails.Length, "e-mail")})";
        var manualBots = bots.Evidence.Where(e => e.IsBot && !e.TypeBot && !e.KnownAutomation).ToList();

        List<string[]> rows =
        [
            ["M1", "Account type is \\texttt{Bot} or login ends in \\texttt{[bot]}", $"{m1.Count} logins",
                Href("https://github.com/apps/github-actions", "github-actions[bot]") + $": {N(gha)} comments ({Pct((double)gha / comments.Count)} of all)"],
            ["M2", "Known automation identity", $"{m2.Count} logins",
                Href("https://github.com/actions-user", "actions-user") + $" (``GitHub Action''): {N(actionCommits)} commits"],
            ["M3", $"$\\geq${Pct(BotDetector.TemplateMin)} of $\\geq${BotDetector.MinTexts} texts repeat a template", $"{m3.Count} logins (not alone)",
                m3.FirstOrDefault() is { } x3 ? Href($"https://github.com/{x3.Login}", x3.Login) + $": {Pct(x3.TemplateRatio)} templated" : "--"],
            ["M4", $"Hour-of-day entropy $\\geq${F(BotDetector.EntropyMin)} over $\\geq${BotDetector.MinEvents} events", $"{m4.Count} logins (not alone)",
                m4.FirstOrDefault() is { } x4 ? Href($"https://github.com/{x4.Login}", x4.Login) + $": entropy {F(x4.HourEntropy)}" : "--"],
            ["M3$\\wedge$M4", "Bot-like on both, or manual verdict on the top-15 scores", $"{m34.Count} + {manualBots.Count} manual",
                manualBots.FirstOrDefault() is { } xm ? Href($"https://github.com/{xm.Login}", xm.Login) +
                    $": {Pct(xm.TemplateRatio)} templated ``automated review'' comments" : "none"],
            ["A1", "One e-mail used under several names", $"{st.EmailsWithSeveralNames} e-mails", Person(a1Example)],
            ["A2", "One distinctive name used with several e-mails", $"{st.NameLinks} links", Person(a2Example)],
            ["A3", "Login read from a \\texttt{noreply} e-mail", $"{st.NoReplyLinks} links", Person(a3Example)],
            ["Manual", "Reviewed overrides: merged logins and e-mails, blocked names", $"{overrides.MergeLogins.Count + overrides.MergeEmails.Count} merges, {resolver.Conflicts.Count} refusals",
                Href("https://github.com/ryanwyler", "ryanwyler") + " = " + Href("https://github.com/ryanwyler", "ryangamerdev") + " (renamed)"],
            ["Renamed", "\\texttt{noreply} login differs from the linked login", $"{resolver.Renames.Count} accounts",
                resolver.Renames.OrderBy(r => r.NewLogin, StringComparer.Ordinal).FirstOrDefault() is { } rn
                    ? Href($"https://github.com/{rn.NewLogin}", rn.NewLogin) + $" (was \\texttt{{{LatexWriter.Escape(rn.OldLogin)}}})" : "--"],
            ["Shared", "One bot account used by many humans", agent is null ? "--" : $"{agent.DistinctCoAuthors} co-authors",
                Href("https://github.com/apps/opencode-agent", "opencode-agent[bot]") + (agent is null ? "" : $": {agent.DistinctTriggerers} humans call it with \\texttt{{/opencode}}")],
        ];
        var footer = new[]
        {
            new[] { "Result", "Bots removed; aliases merged", $"{N(people.Count - humans.Count)} bot or deleted persons",
                $"{N(people.Sum(p => p.RawIdentities))} identities $\\rightarrow$ {N(humans.Count)} human persons (all time)" },
        };
        var tex = LatexWriter.Table(
            "Filtering measures for bots (M1--M4) and multi-account use (A1--A3), with what each one flagged and an example account.",
            "tab:health-filtering",
            ["", "Measure", "Flagged", "Example"],
            @">{\raggedright\arraybackslash}p{0.09\textwidth} >{\raggedright\arraybackslash}p{0.33\textwidth} >{\raggedright\arraybackslash}p{0.15\textwidth} >{\raggedright\arraybackslash}p{0.35\textwidth}",
            rows, footer);
        File.WriteAllText(Path.Combine(outDir, "health_filtering.tex"), tex);

        // Manual checks: random merged groups, targeted bot scores, borderline cases, refused merges
        var md = new StringBuilder();
        md.AppendLine($"# Manual checks for filtering (window {run.Since:yyyy-MM-dd}..{run.Until:yyyy-MM-dd}, seed {run.Seed})");
        md.AppendLine();
        var merged = humans.Where(p => p.RawIdentities > 1).ToList();
        var groupSample = Sampling.RandomSample(merged, GroupSampleSize, run.Seed, p => p.Id);
        var judged = groupSample.Select(p => overrides.GroupVerdicts.GetValueOrDefault(p.Id)).Where(v => v is not null).ToList();
        var same = judged.Count(v => v!.StartsWith("same", StringComparison.OrdinalIgnoreCase));
        md.AppendLine($"## Random sample of {groupSample.Count} merged identity groups (frame: {merged.Count} human persons with >1 raw identity)");
        md.AppendLine($"Precision (same person): {same}/{judged.Count} judged");
        md.AppendLine();
        foreach (var p in groupSample)
            md.AppendLine($"- **{p.Id}** [{p.MergedBy}] logins: {string.Join(", ", p.Logins)}; names: {string.Join(" / ", p.Names)}; " +
                          $"e-mails: {string.Join(", ", p.Emails)} -> verdict: {overrides.GroupVerdicts.GetValueOrDefault(p.Id, "TODO")}");
        md.AppendLine();

        var rng = run.Seed;
        void Texts(string login, int n)
        {
            var mine = comments.Where(c => c.Login.Equals(login, StringComparison.OrdinalIgnoreCase)).ToList();
            foreach (var c in Sampling.RandomSample(mine, n, rng, c => c.Url).OrderBy(c => c.CreatedAt))
                md.AppendLine($"  - {c.CreatedAt:yyyy-MM-dd HH:mm} [{c.Kind} #{c.Number}]({c.Url}): {FirstLine(c.Body.Replace('\r', ' '), 160)}");
        }
        md.AppendLine($"## Targeted sample: {bots.Targeted.Count} accounts with the highest M3+M4 score (not caught by M1/M2)");
        foreach (var e in bots.Targeted)
        {
            var profile = data.Users.GetValueOrDefault(e.Login);
            md.AppendLine($"- **{e.Login}** events {e.Events}, texts {e.Texts}, template {F(e.TemplateRatio)}, entropy {F(e.HourEntropy)}, " +
                          $"flag {e.IsBot} ({e.Reason}); profile: {profile?.Type} since {profile?.CreatedAt:yyyy-MM-dd}, " +
                          $"\"{FirstLine(profile?.Bio ?? "", 80)}\" -> verdict: {overrides.BotVerdicts.GetValueOrDefault(e.Login, "TODO")}");
            Texts(e.Login, 3);
        }
        md.AppendLine();
        md.AppendLine("## Borderline cases");
        foreach (var login in new[] { "opencode-agent[bot]", "ghost", "rekram1-node" })
        {
            var e = bots.Evidence.FirstOrDefault(x => x.Login.Equals(login, StringComparison.OrdinalIgnoreCase));
            if (e is null) continue;
            md.AppendLine($"- **{e.Login}** events {e.Events}, comments {commentsBy.GetValueOrDefault(e.Login)}, template {F(e.TemplateRatio)}, " +
                          $"entropy {F(e.HourEntropy)}, flag {e.IsBot} ({e.Reason}) -> verdict: {overrides.BotVerdicts.GetValueOrDefault(e.Login, "TODO")}");
            Texts(e.Login, 5);
        }
        md.AppendLine();
        md.AppendLine("## Shared bot accounts (co-authors in commits, humans triggering by command)");
        foreach (var s in bots.Shared)
            md.AppendLine($"- **{s.Login}** commits {s.Commits}, co-authored {s.CoAuthoredCommits}, distinct co-authors {s.DistinctCoAuthors}, " +
                          $"triggerers {s.DistinctTriggerers}; top: {string.Join(", ", s.TopCoAuthors.Select(t => $"{t.Name} ({t.Commits})"))}");
        md.AppendLine();
        md.AppendLine($"## Refused merges ({resolver.Conflicts.Count}): same name or e-mail, different logins");
        foreach (var c in resolver.Conflicts)
            md.AppendLine($"- [{c.Rule}] `{c.Key}`: {c.LoginsA} vs {c.LoginsB}");
        md.AppendLine();
        md.AppendLine($"## Renamed accounts ({resolver.Renames.Count}): noreply e-mail keeps the old login");
        foreach (var r in resolver.Renames)
            md.AppendLine($"- {r.OldLogin} -> {r.NewLogin} ({r.Email})");
        md.AppendLine();
        md.AppendLine($"## Overrides: {overrides.MergeLogins.Count} login merges, {overrides.MergeEmails.Count} e-mail merges, " +
                      $"{overrides.BlockedNames.Count} blocked names, {overrides.BlockedEmails.Count} blocked e-mails");
        foreach (var g in overrides.MergeLogins)
            md.AppendLine($"- merge {string.Join(" + ", g)}");
        foreach (var (email, login) in overrides.MergeEmails)
            md.AppendLine($"- merge {email} -> {login}");
        File.WriteAllText(Path.Combine(outDir, "health_samples.md"), md.ToString());

        // Sanity checks for filtering
        var botShare = (double)botComments / comments.Count;
        Check(botShare is > 0.40 and < 0.50, $"bot share of comments {botShare:P1} (expected about 44%)");
        Check(humans.Count <= st.Logins + st.Emails, $"persons {humans.Count} <= logins {st.Logins} + e-mails {st.Emails}");
        Check(people.Count(p => p.Id == "ghost") <= 1, "ghost is one line");
        Console.WriteLine($"filtering: {bots.Bots.Count} bot logins, comments by bots {botShare:P1}, merged groups {merged.Count}, " +
                          $"sample precision {same}/{judged.Count}, refused merges {resolver.Conflicts.Count}");
        return new Filtering(bots, resolver, IsBotPerson);
    }

    static void Check(bool ok, string what)
    {
        Console.WriteLine($"  [{(ok ? "ok" : "FAIL")}] {what}");
        if (!ok) throw new InvalidOperationException("health sanity check failed: " + what);
    }

    static bool IsReviewBot(string login) =>
        Bots.IsBot(login)
        || login.Equals("copilot-pull-request-reviewer", StringComparison.OrdinalIgnoreCase)
        || login.Equals("greptile-apps", StringComparison.OrdinalIgnoreCase);
}
