namespace OpenCodeAnalysis;

/// Everything `fetch` cached, loaded back for offline LINQ analysis.
public record Dataset(
    List<CommitRecord> Commits,
    List<PullRecord> Pulls,
    List<ContributorRecord> Contributors,
    Dictionary<string, int> ReviewCounts,
    List<string> Top,
    List<PairReviews> Pairs,
    List<CommentRecord> ReviewComments,
    List<CommentRecord> IssueComments,
    List<ReviewRecord> Reviews,
    List<PrFileRecord> Files,
    List<int> FilesSample,
    ActivitiesData? Activities)
{
    public static Dataset Load(Cache cache)
    {
        var run = cache.Read<RunInfo>("run");
        bool InWindow(DateTimeOffset d) => d >= run.Since && d < run.Until;

        var top = cache.Read<List<string>>("top");
        var reviewCounts = Directory.EnumerateFiles(Path.Combine(cache.Root, "review_counts"), "*.json")
            .Select(f => cache.Read<ReviewCount>($"review_counts/{Path.GetFileNameWithoutExtension(f)}"))
            .ToDictionary(r => r.Login, r => r.PrsReviewed, StringComparer.OrdinalIgnoreCase);
        var pairs = top.SelectMany(a => top.Where(b => b != a).Select(b => cache.Read<PairReviews>($"pairs/{a}__{b}"))).ToList();
        var targeted = cache.Read<List<int>>("targeted");
        var sample = cache.Read<List<int>>("files_sample");

        return new Dataset(
            cache.Read<List<CommitRecord>>("commits"),
            cache.Read<List<PullRecord>>("pulls").Where(p => InWindow(p.CreatedAt)).DistinctBy(p => p.Number).ToList(),
            cache.Read<List<ContributorRecord>>("contributors"),
            reviewCounts,
            top,
            pairs,
            cache.Read<List<CommentRecord>>("review_comments").Where(c => InWindow(c.CreatedAt)).DistinctBy(c => c.Url).ToList(),
            cache.Read<List<CommentRecord>>("issue_comments").Where(c => InWindow(c.CreatedAt)).DistinctBy(c => c.Url).ToList(),
            targeted.SelectMany(n => cache.Read<List<ReviewRecord>>($"reviews/{n}")).ToList(),
            sample.SelectMany(n => cache.Read<List<PrFileRecord>>($"files/{n}")).ToList(),
            sample,
            ActivitiesData.Load(Path.Combine(cache.Root, "activities")));
    }
}
