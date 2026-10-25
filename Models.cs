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

// Item of the issues (an issue or a PR when IsPull) (all time, with author and closing date)
public record IssueRecord(
    int Number,
    string Login,
    bool IsBot,
    bool IsPull,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    DateTimeOffset? ClosedAt,
    string State,
    string? StateReason,
    string Title,
    string Url);

public record UserProfile(string Login, string Type, DateTimeOffset CreatedAt, string? Name, string? Bio, int PublicRepos, int Followers);

// One person after merging aliases
// Login is the primary account, MergedBy lists rules that linked identities
public record Person(string Id, string? Login, string[] Logins, string[] Names, string[] Emails, int RawIdentities, string MergedBy);

// Bot measures M1-M4 for one login
public record BotEvidence(
    string Login,
    int Events,
    int Texts,
    bool TypeBot,
    bool KnownAutomation,
    double TemplateRatio,
    double HourEntropy,
    bool IsBot,
    string Reason);

public record PersonActivity(
    string PersonId,
    string Category,
    bool Key,
    bool Regular,
    bool Newcomer,
    bool Casual,
    bool Dormant,
    int Commits,
    int PrsOpened,
    int PrsMerged,
    int Reviews,
    int ReviewComments,
    int IssueComments,
    int IssuesOpened,
    DateTimeOffset First,
    DateTimeOffset Last,
    int ActiveMonths,
    int ActiveDays);
