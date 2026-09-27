using Octokit.GraphQL;
using Octokit.GraphQL.Model;
using static Octokit.GraphQL.Variable;

namespace GhActivities;

/// <summary>
/// Fetches reviews of in-window pull requests through Octokit's GraphQL client.
/// One nested query walks PRs (created newest-first, stopping at the window start) together
/// with their reviews, so the whole traversal costs a few hundred GraphQL points instead of
/// one REST call per pull request.
/// </summary>
public sealed class ReviewFetcher(string owner, string repository, string token, Action<string> log)
{
    private const int PageSize = 100;
    private const int MaxAttempts = 5;

    public async Task<IReadOnlyList<ReviewRow>> GetReviewsAsync(
        ActivityWindow window,
        IReadOnlyDictionary<int, string> prAuthors)
    {
        var connection = new Connection(new ProductHeaderValue("capstone-ch6"), token);
        var query = new Query()
            .Repository(repository, owner)
            .PullRequests(
                first: PageSize,
                after: Var("after"),
                orderBy: new IssueOrder
                {
                    Field = IssueOrderField.CreatedAt,
                    Direction = OrderDirection.Desc,
                })
            .Select(page => new
            {
                page.PageInfo.EndCursor,
                page.PageInfo.HasNextPage,
                Items = page.Nodes.Select(pr => new
                {
                    pr.Number,
                    pr.CreatedAt,
                    Reviews = pr.Reviews(null, null, null, null, null, null).AllPages().Select(review => new
                    {
                        review.SubmittedAt,
                        review.State,
                        Author = review.Author.Login,
                        review.Body,
                        review.Url,
                    }).ToList(),
                }).ToList(),
            })
            .Compile();

        var variables = new Dictionary<string, object> { ["after"] = null! };
        var reviews = new List<ReviewRow>();
        var pulled = 0;
        for (var page = 1; ; page++)
        {
            var result = await RunAsync(connection, query, variables);
            foreach (var pr in result.Items)
            {
                pulled++;
                foreach (var review in pr.Reviews)
                {
                    if (review.SubmittedAt is { } submitted && window.Contains(submitted))
                    {
                        reviews.Add(new ReviewRow(
                            pr.Number,
                            submitted,
                            StateName(review.State),
                            review.Author ?? string.Empty,
                            prAuthors.GetValueOrDefault(pr.Number) ?? string.Empty,
                            review.Body ?? string.Empty,
                            review.Url?.ToString() ?? string.Empty));
                    }
                }
            }

            log($"reviews page {page}: {pulled} PRs, {reviews.Count} reviews in window");
            if (result.Items.Count == 0 || result.Items[^1].CreatedAt < window.From || !result.HasNextPage)
            {
                break;
            }

            variables["after"] = result.EndCursor!;
            await Task.Delay(100);
        }

        return reviews.OrderBy(review => review.SubmittedAt).ToList();
    }

    private async Task<T> RunAsync<T>(Connection connection, ICompiledQuery<T> query, Dictionary<string, object> variables)
    {
        for (var attempt = 1; ; attempt++)
        {
            try
            {
                return await connection.Run(query, variables);
            }
            catch (Exception exception) when (attempt < MaxAttempts && IsTransient(exception))
            {
                var backoff = TimeSpan.FromSeconds(Math.Min(300, 30 * Math.Pow(2, attempt - 1)));
                log($"graphql: {exception.Message.Split('\n')[0]} - retry {attempt} in {backoff.TotalSeconds:0}s");
                await Task.Delay(backoff);
            }
        }
    }

    private static string StateName(PullRequestReviewState state) => state switch
    {
        PullRequestReviewState.Approved => "approved",
        PullRequestReviewState.ChangesRequested => "changes_requested",
        PullRequestReviewState.Commented => "commented",
        PullRequestReviewState.Dismissed => "dismissed",
        PullRequestReviewState.Pending => "pending",
        _ => state.ToString().ToLowerInvariant(),
    };

    private static bool IsTransient(Exception exception) =>
        exception is HttpRequestException or TaskCanceledException
        || exception.Message.Contains("rate limit", StringComparison.OrdinalIgnoreCase);
}
