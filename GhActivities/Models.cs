namespace GhActivities;

public sealed record ActivityWindow(DateTimeOffset From, DateTimeOffset To)
{
    public bool Contains(DateTimeOffset moment) => moment >= From && moment <= To;

    public IEnumerable<DateOnly> Weeks()
    {
        var start = DateOnly.FromDateTime(From.UtcDateTime);
        var week = start.AddDays(-(((int)start.DayOfWeek + 6) % 7));
        var end = DateOnly.FromDateTime(To.UtcDateTime);
        for (var day = week; day <= end; day = day.AddDays(7))
            yield return day;
    }
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

public sealed record IssueRow(
    int Number,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    string State,
    int Comments,
    string Title,
    string Url);

public sealed record ReleaseRow(
    string Tag,
    string Name,
    DateTimeOffset PublishedAt,
    bool Prerelease,
    string Url);

public sealed record WeeklyCounts(DateOnly WeekStart, int Issues, int PullRequests);

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
