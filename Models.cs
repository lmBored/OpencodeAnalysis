namespace OpenCodeAnalysis;

public record RunInfo(DateTimeOffset CollectedAt, DateTimeOffset Since, DateTimeOffset Until, int Seed);

public record CommitRecord(
    string Sha,
    string? Login,
    bool IsBot,
    string AuthorName,
    string AuthorEmail,
    DateTimeOffset Date,
    string Message);

public record PullRecord(
    int Number,
    string Login,
    bool IsBot,
    DateTimeOffset CreatedAt,
    DateTimeOffset? MergedAt,
    string State,
    string Title,
    string Url);

public record ReviewRecord(
    int PrNumber,
    string PrAuthor,
    string Reviewer,
    string State,
    DateTimeOffset? SubmittedAt,
    string Body,
    string Url);

public record CommentRecord(
    string Kind, // Kind is "review" (inline review comment on a PR diff) or "issue" (conversation comment on an issue or PR)
    int Number,
    string Login,
    bool IsBot,
    string? Path,
    string Body,
    DateTimeOffset CreatedAt,
    string Url);

public record PrFileRecord(int PrNumber, string PrAuthor, string Path, int Additions, int Deletions);

public record ContributorRecord(string? Login, string? Name, string? Type, int Contributions);

public record ReviewCount(string Login, int PrsReviewed);

// PRs authored by Author on which Reviewer submitted at least one review
public record PairReviews(string Author, string Reviewer, int Count, List<int> PrNumbers);
