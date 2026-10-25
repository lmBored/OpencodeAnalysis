using System.Text.RegularExpressions;
using GhCore;

namespace OpenCodeAnalysis;

/// Bot measures each login
/// M1 account type
/// M2 known automation
/// M3 template texts
/// M4 round-the-clock activity
public static partial class BotDetector
{
    public const int MinTexts = 10; // M3 needs at least this many texts
    public const int MinEvents = 50; // M4 needs at least this many timestamps
    public const double TemplateMin = 0.5; // M3: at least half of the texts repeat a template
    public const double EntropyMin = 0.95; // M4: hour-of-day entropy close to uniform (1 = all 24 hours equal)
    public const int TargetedSize = 15;

    public record SharedAccount(string Login, int Commits, int CoAuthoredCommits, int DistinctCoAuthors, int DistinctTriggerers,
        List<(string Name, int Commits)> TopCoAuthors);

    public record Result(
        List<BotEvidence> Evidence,
        HashSet<string> Bots,
        List<string> ReviewList,
        List<BotEvidence> Targeted,
        List<SharedAccount> Shared)
    {
        public bool IsBot(string? login) => login is not null && Bots.Contains(login);
    }

    [GeneratedRegex(@"https?://\S+|@[A-Za-z0-9-]+|\d+|[^a-z#\s]", RegexOptions.IgnoreCase)]
    private static partial Regex Noise();

    [GeneratedRegex(@"\s+")]
    private static partial Regex Spaces();

    [GeneratedRegex(@"^Co-authored-by:\s*(?<name>[^<\n]+?)\s*<(?<email>[^>\n]*)>", RegexOptions.Multiline | RegexOptions.IgnoreCase)]
    private static partial Regex CoAuthor();

    [GeneratedRegex(@"(^|\s)/(opencode|oc)\b", RegexOptions.IgnoreCase)]
    private static partial Regex AgentCommand();

    /// Template key: the first 60 characters without links, mentions, numbers and punctuation.
    public static string TemplateKey(string body)
    {
        var s = Noise().Replace(body.ToLowerInvariant(), m => char.IsDigit(m.Value[0]) ? "#" : " ");
        s = Spaces().Replace(s, " ").Trim();
        return s.Length <= 60 ? s : s[..60];
    }

    /// Shannon entropy of the UTC hour-of-day histogram, divided by log2(24).
    public static double HourEntropy(IReadOnlyCollection<DateTimeOffset> times)
    {
        if (times.Count == 0) return 0;
        return -times.GroupBy(t => t.UtcDateTime.Hour)
            .Select(g => (double)g.Count() / times.Count)
            .Sum(p => p * Math.Log2(p)) / Math.Log2(24);
    }

    /// personOf maps a co-author (name, e-mail) to a person id, so aliases of one human count once.
    public static Result Evaluate(Dataset data, RunInfo run, IdentityOverrides overrides, Func<string, string, string>? personOf = null)
    {
        var issuesOpened = data.IssueItems.Where(i => !i.IsPull && i.CreatedAt >= run.Since && i.CreatedAt < run.Until).ToList();
        var reviews = data.Activities?.Reviews ?? [];

        var events = data.IssueComments.Concat(data.ReviewComments).Select(c => (c.Login, At: c.CreatedAt, Text: (string?)c.Body))
            .Concat(reviews.Select(r => (Login: r.Author, At: r.SubmittedAt, Text: string.IsNullOrWhiteSpace(r.Body) ? null : (string?)r.Body)))
            .Concat(data.Pulls.Select(p => (p.Login, At: p.CreatedAt, Text: (string?)null)))
            .Concat(issuesOpened.Select(i => (i.Login, At: i.CreatedAt, Text: (string?)null)))
            .Concat(data.Commits.Where(c => c.Login is not null).Select(c => (Login: c.Login!, At: c.Date, Text: (string?)null)))
            .Where(e => !string.IsNullOrEmpty(e.Login))
            .GroupBy(e => e.Login, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var evidence = events.Select(g =>
        {
            var login = g.Key;
            var times = g.Select(e => e.At).ToList();
            var texts = g.Where(e => e.Text is not null).Select(e => TemplateKey(e.Text!)).ToList();
            var repeated = texts.GroupBy(t => t).Where(t => t.Count() >= 3).Sum(t => t.Count());
            var template = texts.Count >= MinTexts ? (double)repeated / texts.Count : 0;
            var entropy = times.Count >= MinEvents ? HourEntropy(times) : 0;
            var type = data.Users.GetValueOrDefault(login)?.Type;
            var m1 = Bots.IsBotAccount(login, type);
            var m2 = Bots.IsKnownAutomation(login);
            var m3 = template >= TemplateMin;
            var m4 = entropy >= EntropyMin;
            var isBot = m1 || m2 || (m3 && m4);
            var reasons = new List<string>();
            if (m1) reasons.Add("M1");
            if (m2) reasons.Add("M2");
            if (m3) reasons.Add("M3");
            if (m4) reasons.Add("M4");
            if (overrides.BotVerdicts.TryGetValue(login, out var verdict))
            {
                if (verdict.StartsWith("human", StringComparison.OrdinalIgnoreCase)) isBot = false;
                else if (verdict.StartsWith("bot", StringComparison.OrdinalIgnoreCase)) isBot = true;
                reasons.Add("manual: " + verdict);
            }
            return new BotEvidence(login, times.Count, texts.Count, m1, m2, template, entropy, isBot, string.Join(' ', reasons));
        }).OrderByDescending(e => e.Events).ThenBy(e => e.Login, StringComparer.Ordinal).ToList();

        // Commit-only identities without a login (e.g. "GitHub Action") are covered by M2 on the author name
        var bots = evidence.Where(e => e.IsBot).Select(e => e.Login).ToHashSet(StringComparer.OrdinalIgnoreCase);

        // Targeted sampling
        // Highest behavioural score among accounts not already caught by M1/M2
        var targeted = evidence.Where(e => !e.TypeBot && !e.KnownAutomation && e.Events >= MinEvents)
            .OrderByDescending(e => e.TemplateRatio + e.HourEntropy).ThenBy(e => e.Login, StringComparer.Ordinal)
            .Take(TargetedSize).ToList();

        var review = evidence.Where(e => e.TypeBot || e.KnownAutomation).Select(e => e.Login)
            .Concat(targeted.Select(e => e.Login))
            .Concat(evidence.Where(e => e.Login is "ghost" or "rekram1-node").Select(e => e.Login))
            .Concat(data.Pulls.Where(p => p.IsBot).Select(p => p.Login))
            .Distinct(StringComparer.OrdinalIgnoreCase).Order(StringComparer.Ordinal).ToList();

        // One account, many people: humans credited as co-authors, and humans who trigger the agent by command
        var shared = evidence.Where(e => e.TypeBot || e.KnownAutomation).Select(e =>
        {
            var commits = data.Commits.Where(c => string.Equals(c.Login, e.Login, StringComparison.OrdinalIgnoreCase)).ToList();
            var coAuthors = commits.Select(c => CoAuthor().Matches(c.Message)
                    .Select(m => (Name: m.Groups["name"].Value.Trim(), Email: m.Groups["email"].Value.Trim()))
                    .Where(x => !Bots.IsBot(x.Name) && !x.Name.Contains("[bot]") && !x.Email.Contains("[bot]"))
                    .Select(x => personOf?.Invoke(x.Name, x.Email) ?? x.Name)
                    .Where(k => !Bots.IsBot(k) && k != "unknown")
                    .Distinct(StringComparer.OrdinalIgnoreCase).ToList())
                .ToList();
            var top = coAuthors.SelectMany(x => x).GroupBy(n => n, StringComparer.OrdinalIgnoreCase)
                .Select(g => (g.Key, g.Count())).OrderByDescending(x => x.Item2).ThenBy(x => x.Key).ToList();
            var triggerers = e.Login.Equals("opencode-agent[bot]", StringComparison.OrdinalIgnoreCase)
                ? data.IssueComments.Where(c => !c.IsBot && AgentCommand().IsMatch(c.Body))
                    .Select(c => c.Login).Distinct(StringComparer.OrdinalIgnoreCase).Count()
                : 0;
            return new SharedAccount(e.Login, commits.Count, coAuthors.Count(x => x.Count > 0), top.Count, triggerers, top.Take(8).ToList());
        }).Where(s => s.DistinctCoAuthors > 0 || s.DistinctTriggerers > 0).OrderByDescending(s => s.DistinctCoAuthors).ToList();

        return new Result(evidence, bots, review, targeted, shared);
    }
}
