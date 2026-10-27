using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace OpenCodeAnalysis;

/// Exceptions to the merge rules (data/identity_overrides.json)
/// These are checked manually
public record IdentityOverrides(
    List<string> BlockedEmails, // shared or placeholder e-mails, not used to link identities
    List<string> BlockedNames, // common names, not used to link identities
    List<List<string>> MergeLogins, // several accounts of one person, first login is the primary one
    Dictionary<string, string> MergeEmails, // commit e-mail -> login, for e-mails no rule can link (e.g. old company domain)
    Dictionary<string, string> GroupVerdicts, // person id -> "same" | "different" (manual check of merged groups)
    Dictionary<string, string> BotVerdicts) // login -> "bot" | "human" | "borderline: ..." (manual check of bot scores)
{
    static readonly JsonSerializerOptions Json = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
    };

    public static IdentityOverrides Load(string path)
    {
        var o = File.Exists(path) ? JsonSerializer.Deserialize<IdentityOverrides>(File.ReadAllText(path), Json) : null;
        return new IdentityOverrides(
            o?.BlockedEmails ?? [],
            o?.BlockedNames ?? [],
            o?.MergeLogins ?? [],
            new Dictionary<string, string>(o?.MergeEmails ?? [], StringComparer.OrdinalIgnoreCase),
            new Dictionary<string, string>(o?.GroupVerdicts ?? [], StringComparer.OrdinalIgnoreCase),
            new Dictionary<string, string>(o?.BotVerdicts ?? [], StringComparer.OrdinalIgnoreCase));
    }
}

/// GitHub link: commit e-mail -> linked account
/// A1: one e-mail is one person, whatever names it is used with
/// A2: e-mails sharing a distinctive name (or a name equal to a login)
/// A3: login read from a noreply e-mail
/// A merge that joins two different logins is refused and reported, unless an override confirms it.
public sealed partial class IdentityResolver
{
    [GeneratedRegex(@"^(?:\d+\+)?(?<login>[A-Za-z0-9-]+(?:\[bot\])?)@users\.noreply\.github\.com$", RegexOptions.IgnoreCase)]
    private static partial Regex NoReply();

    public record Conflict(string Rule, string Key, string LoginsA, string LoginsB);

    // noreply e-mail keeps the old login after an account rename. GitHub links the e-mail to the new login
    public record Rename(string OldLogin, string NewLogin, string Email);

    public record RuleStats(
        int RawIdentities, int Emails, int Logins, int Persons,
        int GitHubLinks, int EmailsWithSeveralNames, int NamesWithSeveralEmails, int NameLinks, int NoReplyLinks,
        int OverrideMerges, int MultiIdentityPersons, int Renames);

    readonly Dictionary<string, string> _parent = new();
    readonly Dictionary<string, SortedSet<string>> _logins = new();
    readonly Dictionary<string, SortedSet<string>> _rules = new();
    readonly Dictionary<string, Person> _personOfRoot = new();
    readonly Dictionary<string, string> _display = new(); // lower-case login -> login as GitHub spells it
    readonly HashSet<string> _blockedEmails;
    readonly HashSet<string> _blockedNames;

    public List<Person> People { get; }
    public List<Conflict> Conflicts { get; } = [];
    public List<Rename> Renames { get; } = [];
    public RuleStats Stats { get; }

    /// NFKC, lower case, letters and digits only (keeps CJK and decorated Unicode names readable).
    public static string NormName(string name)
    {
        var sb = new StringBuilder();
        foreach (var ch in name.Normalize(NormalizationForm.FormKC).ToLowerInvariant())
            sb.Append(char.IsLetterOrDigit(ch) ? ch : ' ');
        return string.Join(' ', sb.ToString().Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }

    /// A name is distinctive with at least two words, like "luke parker". "jack" or "alex" are not.
    bool Distinctive(string norm) => norm.Contains(' ') && norm.Length >= 6 && !_blockedNames.Contains(norm);

    string EmailNode(CommitRecord c)
    {
        var email = c.AuthorEmail.ToLowerInvariant();
        return _blockedEmails.Contains(email) ? $"R:{NormName(c.AuthorName)}|{email}" : $"E:{email}";
    }

    public IdentityResolver(IReadOnlyList<CommitRecord> commits, IEnumerable<string> activityLogins, IdentityOverrides overrides)
    {
        _blockedEmails = overrides.BlockedEmails.Select(e => e.ToLowerInvariant()).ToHashSet();
        _blockedNames = overrides.BlockedNames.Select(NormName).ToHashSet();

        var rows = commits
            .Select(c => (Node: EmailNode(c), Name: NormName(c.AuthorName), Display: c.AuthorName.Trim(),
                Email: c.AuthorEmail.ToLowerInvariant(), Login: c.Login?.ToLowerInvariant()))
            .ToList();
        foreach (var r in rows) Add(r.Node);
        foreach (var l in activityLogins.Concat(commits.Where(c => c.Login is not null).Select(c => c.Login!)))
        {
            Add("L:" + l.ToLowerInvariant());
            _display.TryAdd(l.ToLowerInvariant(), l);
        }

        // GitHub's own link between commit e-mail and account
        var gitHubLinks = rows.Where(r => r.Login is not null).Select(r => (r.Node, r.Login)).Distinct()
            .Count(r => Union(r.Node, "L:" + r.Login, "github", force: false));

        // A3: noreply e-mail carries the login (an e-mail already linked to another login is a renamed account)
        var noReplyLinks = 0;
        foreach (var (node, email, login) in rows.Select(r => (r.Node, r.Email, Match: NoReply().Match(r.Email)))
                     .Where(x => x.Match.Success)
                     .Select(x => (x.Node, x.Email, Login: x.Match.Groups["login"].Value.ToLowerInvariant())).Distinct())
        {
            var linked = _logins[Find(node)];
            if (linked.Count > 0 && !linked.Contains(login))
            {
                Renames.Add(new Rename(login, string.Join(' ', linked), email));
                continue;
            }
            if (Union(node, "L:" + login, "A3", force: false)) noReplyLinks++;
        }

        // A2: same distinctive name or a name that equals a known login
        var byName = rows.Where(r => r.Name.Length > 0).GroupBy(r => r.Name)
            .Select(g => (Name: g.Key, Nodes: g.Select(r => r.Node).Distinct().Order().ToList()))
            .OrderBy(g => g.Name, StringComparer.Ordinal)
            .ToList();
        var nameLinks = 0;
        foreach (var (name, nodes) in byName)
        {
            if (Distinctive(name))
                nameLinks += nodes.Skip(1).Count(n => Union(nodes[0], n, "A2", force: false, key: name));
            var asLogin = "L:" + name;
            if (!name.Contains(' ') && name.Length >= 4 && !_blockedNames.Contains(name) && _parent.ContainsKey(asLogin))
                nameLinks += nodes.Count(n => Union(n, asLogin, "A2", force: false, key: name));
        }

        // confirmed accounts of one person
        var overrideMerges = 0;
        foreach (var group in overrides.MergeLogins.Where(g => g.Count > 1))
        {
            foreach (var l in group)
            {
                Add("L:" + l.ToLowerInvariant());
                _display.TryAdd(l.ToLowerInvariant(), l);
            }
            overrideMerges += group.Skip(1).Count(l => Union("L:" + group[0].ToLowerInvariant(), "L:" + l.ToLowerInvariant(), "override", force: true));
        }
        foreach (var (email, login) in overrides.MergeEmails.OrderBy(kv => kv.Key, StringComparer.Ordinal))
        {
            var node = "E:" + email.ToLowerInvariant();
            if (!_parent.ContainsKey(node)) continue;
            Add("L:" + login.ToLowerInvariant());
            _display.TryAdd(login.ToLowerInvariant(), login);
            if (Union("L:" + login.ToLowerInvariant(), node, "override", force: true)) overrideMerges++;
        }
        // remove an override resolved are no longer open
        Conflicts.RemoveAll(c => Find("L:" + c.LoginsA.Split(' ')[0]) == Find("L:" + c.LoginsB.Split(' ')[0]));

        // Build persons
        var primaryOverride = overrides.MergeLogins.Where(g => g.Count > 0).Select(g => g[0].ToLowerInvariant()).ToHashSet();
        var commitsByRoot = rows.GroupBy(r => Find(r.Node)).ToDictionary(g => g.Key, g => g.ToList());
        var loginCommits = rows.Where(r => r.Login is not null).GroupBy(r => r.Login!).ToDictionary(g => g.Key, g => g.Count());
        People = _parent.Keys.ToList().GroupBy(Find).Select(g =>
        {
            var root = g.Key;
            var mine = commitsByRoot.GetValueOrDefault(root) ?? [];
            var logins = _logins[root]
                .OrderByDescending(l => primaryOverride.Contains(l))
                .ThenByDescending(l => loginCommits.GetValueOrDefault(l))
                .ThenBy(l => l, StringComparer.Ordinal)
                .Select(l => _display.GetValueOrDefault(l, l)).ToArray();
            var emails = mine.GroupBy(r => r.Email).OrderByDescending(e => e.Count()).ThenBy(e => e.Key).Select(e => e.Key).ToArray();
            var names = mine.Select(r => r.Display).Distinct(StringComparer.OrdinalIgnoreCase).Order(StringComparer.Ordinal).ToArray();
            var raw = mine.Select(r => (r.Name, r.Email)).Distinct().Count()
                      + logins.Count(l => !loginCommits.ContainsKey(l.ToLowerInvariant()));
            var id = logins.Length > 0 ? logins[0] : "email:" + (emails.FirstOrDefault() ?? root);
            var person = new Person(id, logins.FirstOrDefault(), logins, names, emails, raw, string.Join('+', _rules[root]));
            _personOfRoot[root] = person;
            return person;
        }).OrderBy(p => p.Id, StringComparer.Ordinal).ToList();

        Stats = new RuleStats(
            rows.Select(r => (r.Name, r.Email)).Distinct().Count(),
            rows.Select(r => r.Email).Distinct().Count(),
            _parent.Keys.Count(k => k.StartsWith("L:")),
            People.Count,
            gitHubLinks,
            rows.Where(r => r.Node.StartsWith("E:")).GroupBy(r => r.Email).Count(g => g.Select(r => r.Name).Distinct().Count() > 1),
            byName.Count(g => Distinctive(g.Name) && g.Nodes.Count > 1),
            nameLinks,
            noReplyLinks,
            overrideMerges,
            People.Count(p => p.RawIdentities > 1),
            Renames.Count);
    }

    /// Person key for a (name, e-mail) seen outside author fields, e.g. in Co-authored-by
    public string KeyOf(string name, string email)
    {
        var e = "E:" + email.ToLowerInvariant();
        if (_parent.ContainsKey(e)) return _personOfRoot[Find(e)].Id;
        var m = NoReply().Match(email);
        if (m.Success && _parent.ContainsKey("L:" + m.Groups["login"].Value.ToLowerInvariant()))
            return OfLogin(m.Groups["login"].Value).Id;
        var norm = NormName(name);
        if (norm.Length == 0 || _blockedNames.Contains(norm)) return "unknown";
        return _parent.ContainsKey("L:" + norm) ? OfLogin(norm).Id : "name:" + norm;
    }

    /// Person of a login; a login never seen while building gets its own one-login person.
    public Person OfLogin(string login) =>
        _parent.ContainsKey("L:" + login.ToLowerInvariant())
            ? _personOfRoot[Find("L:" + login.ToLowerInvariant())]
            : new Person(login, login, [login], [], [], 1, "");

    public Person OfCommit(CommitRecord c) =>
        c.Login is not null ? OfLogin(c.Login) : _personOfRoot[Find(EmailNode(c))];

    void Add(string node)
    {
        if (_parent.ContainsKey(node)) return;
        _parent[node] = node;
        _logins[node] = node.StartsWith("L:") ? new SortedSet<string>(StringComparer.Ordinal) { node[2..] } : new SortedSet<string>(StringComparer.Ordinal);
        _rules[node] = new SortedSet<string>(StringComparer.Ordinal);
    }

    string Find(string node)
    {
        while (_parent[node] != node)
        {
            _parent[node] = _parent[_parent[node]];
            node = _parent[node];
        }
        return node;
    }

    /// Returns true when two different groups were merged
    bool Union(string a, string b, string rule, bool force, string? key = null)
    {
        Add(a);
        Add(b);
        var (ra, rb) = (Find(a), Find(b));
        if (ra == rb) return false;
        if (!force && _logins[ra].Count > 0 && _logins[rb].Count > 0 && !_logins[ra].Overlaps(_logins[rb]))
        {
            Conflicts.Add(new Conflict(rule, key ?? a, string.Join(' ', _logins[ra]), string.Join(' ', _logins[rb])));
            return false;
        }
        // keep the root with a login so person ids stay stable
        if (_logins[ra].Count == 0 && _logins[rb].Count > 0) (ra, rb) = (rb, ra);
        _parent[rb] = ra;
        _logins[ra].UnionWith(_logins[rb]);
        _rules[ra].UnionWith(_rules[rb]);
        _rules[ra].Add(rule);
        return true;
    }
}
