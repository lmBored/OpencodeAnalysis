namespace GhActivities;

public sealed record ActivityWindow(DateTimeOffset From, DateTimeOffset To)
{
    public bool Contains(DateTimeOffset moment) => moment >= From && moment <= To;
}

public sealed record CommitRow(DateTimeOffset CommittedAt, DateTimeOffset AuthoredAt, string Sha);

public sealed record PullRequestRow(
    int Number,
    DateTimeOffset CreatedAt,
    DateTimeOffset? MergedAt,
    string State,
    string Title,
    string Url,
    string Author);

public sealed record ReleaseRow(
    string Tag,
    string Name,
    DateTimeOffset PublishedAt,
    bool Prerelease,
    string Url);

public sealed record ReviewRow(
    int PullRequest,
    DateTimeOffset SubmittedAt,
    string State,
    string Author,
    string PrAuthor,
    string Body,
    string Url);

public sealed record RepoSnapshot(
    long Stars,
    long Forks,
    long OpenIssues,
    long SizeKb,
    bool HasDiscussions,
    DateTimeOffset CreatedAt,
    string DefaultBranch);
