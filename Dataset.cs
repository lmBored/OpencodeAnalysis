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
    ActivitiesData? Activities,
    List<IssueRecord> IssueItems,
    List<CommitRecord> PriorCommits,
    Dictionary<string, UserProfile> Users)
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

        // Items updated during the fetch appear twice, the last copy is the newest
        var issueItems = cache.Has("issue_items")
            ? cache.Read<List<IssueRecord>>("issue_items").AsEnumerable().Reverse().DistinctBy(i => i.Number).OrderBy(i => i.Number).ToList()
            : [];
        var priorCommits = cache.Has("prior_commits") ? cache.Read<List<CommitRecord>>("prior_commits") : [];
        var usersDir = Path.Combine(cache.Root, "users");
        var users = Directory.Exists(usersDir)
            ? Directory.EnumerateFiles(usersDir, "*.json")
                .Select(f => cache.Read<UserProfile>($"users/{Path.GetFileNameWithoutExtension(f)}"))
                .ToDictionary(u => u.Login, StringComparer.OrdinalIgnoreCase)
            : new Dictionary<string, UserProfile>(StringComparer.OrdinalIgnoreCase);

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
            ActivitiesData.Load(Path.Combine(cache.Root, "activities")),
            issueItems,
            priorCommits,
            users);
    }
}
