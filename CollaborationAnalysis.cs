using System.Text.RegularExpressions;

namespace OpenCodeAnalysis;

/// Directed interaction of Actor to Author's work: reviews on Author's PRs, comments on Author's PRs, @ mentions of Author.
public record Interaction(string Author, string Actor, int Reviews, int Comments, int Mentions)
{
    public int Total => Reviews + Comments + Mentions;
}

public record PairScore(string A, string B, Interaction AtoB, Interaction BtoA)
{
    public int Total => AtoB.Total + BtoA.Total;
    public int Weaker => Math.Min(AtoB.Total, BtoA.Total);
}

public record PrThread(int Number, string Author, string Title, string Url, DateTimeOffset CreatedAt,
    int ByA, int ByB, int Reviews, List<CommentRecord> Comments);

public record CoEdit(string A, string B, int SharedFiles, int FilesA, int FilesB, double Jaccard, int Interactions,
    List<string> Examples);

public static class CollaborationAnalysis
{
    // Generated or bulk files that everybody touches and that say nothing about working on the same thing.
    static readonly Regex Noise = new(@"(^|/)(bun\.lock|package\.json|.*\.snap|.*\.gen\.ts|openapi\.json|CHANGELOG\.md)$|^packages/sdk/|/generated/",
        RegexOptions.Compiled);

    static bool Same(string a, string b) => string.Equals(a, b, StringComparison.OrdinalIgnoreCase);

    public static List<CommentRecord> AllComments(Dataset d) => d.ReviewComments.Concat(d.IssueComments).ToList();

    public static Interaction Directed(Dataset d, string author, string actor)
    {
        var authorsPrs = d.Pulls.Where(p => Same(p.Login, author)).Select(p => p.Number).ToHashSet();
        var reviews = d.Pairs.FirstOrDefault(p => p.Author == author && p.Reviewer == actor)?.Count ?? 0;
        var comments = AllComments(d).Count(c => Same(c.Login, actor) && authorsPrs.Contains(c.Number));
        var mention = new Regex($@"@{Regex.Escape(author)}\b", RegexOptions.IgnoreCase);
        var mentions = AllComments(d).Count(c => Same(c.Login, actor) && mention.IsMatch(c.Body));
        return new Interaction(author, actor, reviews, comments, mentions);
    }

    /// All unordered pairs of the top humans, strongest bidirectional interaction first.
    public static List<PairScore> RankPairs(Dataset d) =>
        d.Top.SelectMany((a, i) => d.Top.Skip(i + 1).Select(b => new PairScore(a, b, Directed(d, a, b), Directed(d, b, a))))
            .OrderByDescending(p => p.Weaker).ThenByDescending(p => p.Total)
            .ToList();

    /// Targeted sampling
    /// Here we use PRs authored by one member of the pair on which the two of them wrote at least minComments comments.
    public static List<PrThread> Threads(Dataset d, string a, string b, int minComments = 3)
    {
        var byNumber = AllComments(d).ToLookup(c => c.Number);
        var reviewed = d.Pairs.Where(p => (p.Author == a && p.Reviewer == b) || (p.Author == b && p.Reviewer == a))
            .SelectMany(p => p.PrNumbers).ToHashSet();
        return Sampling.Targeted(
                d.Pulls.Where(p => Same(p.Login, a) || Same(p.Login, b)),
                p => byNumber[p.Number].Count(c => Same(c.Login, a) || Same(c.Login, b)) >= minComments
                     && byNumber[p.Number].Any(c => Same(c.Login, Same(p.Login, a) ? b : a)),
                p => p.CreatedAt)
            .Select(p =>
            {
                var cs = byNumber[p.Number].Where(c => Same(c.Login, a) || Same(c.Login, b)).OrderBy(c => c.CreatedAt).ToList();
                return new PrThread(p.Number, p.Login, p.Title, p.Url, p.CreatedAt,
                    cs.Count(c => Same(c.Login, a)), cs.Count(c => Same(c.Login, b)),
                    reviewed.Contains(p.Number) ? 1 : 0, cs);
            })
            .ToList();
    }

    /// Pairs whose sampled merged PRs touch the same files, with their total interaction (reviews+comments+mentions).
    public static List<CoEdit> CoEdits(Dataset d, IReadOnlyList<PairScore> pairs)
    {
        var filesBy = d.Files.Where(f => !Noise.IsMatch(f.Path))
            .GroupBy(f => f.PrAuthor, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.Select(f => f.Path).ToHashSet(), StringComparer.OrdinalIgnoreCase);
        return pairs.Select(p =>
            {
                var fa = filesBy.GetValueOrDefault(p.A) ?? [];
                var fb = filesBy.GetValueOrDefault(p.B) ?? [];
                var shared = fa.Intersect(fb).Order().ToList();
                var union = fa.Union(fb).Count();
                return new CoEdit(p.A, p.B, shared.Count, fa.Count, fb.Count, union == 0 ? 0 : (double)shared.Count / union,
                    p.Total, shared.Take(8).ToList());
            })
            .OrderBy(c => c.Interactions).ThenByDescending(c => c.SharedFiles)
            .ToList();
    }
}
