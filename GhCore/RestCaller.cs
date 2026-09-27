using Octokit;
using Octokit.Caching;

namespace GhCore;

/// Octokit wrapper for auth, rate limit and one call site per REST endpoint:
/// search throttling, primary/secondary/abuse rate limits, server and network errors.
public sealed class RestCaller
{
    public const int PageSize = 100;

    private readonly GitHubClient _client;
    private readonly Action<string> _log;
    private readonly TimeSpan _searchDelay;
    private DateTimeOffset _lastSearch = DateTimeOffset.MinValue;

    public int ApiCalls { get; private set; }

    public GitHubClient Client => _client;

    public RestCaller(
        string productHeader,
        string token,
        IResponseCache? responseCache = null,
        Action<string>? log = null,
        double searchDelaySeconds = 2.2)
    {
        _log = log ?? Console.WriteLine;
        _searchDelay = TimeSpan.FromSeconds(searchDelaySeconds);
        _client = new GitHubClient(new ProductHeaderValue(productHeader))
        {
            Credentials = new Credentials(token),
        };
        if (responseCache is not null)
        {
            _client.ResponseCache = responseCache;
        }
    }

    /// One API call, waiting for the rate limit reset and retrying.
    public async Task<T> Call<T>(Func<Task<T>> call, bool search = false)
    {
        for (var attempt = 1; ; attempt++)
        {
            if (search)
            {
                var wait = _lastSearch.Add(_searchDelay) - DateTimeOffset.UtcNow;
                if (wait > TimeSpan.Zero) await Task.Delay(wait);
                _lastSearch = DateTimeOffset.UtcNow;
            }

            try
            {
                ApiCalls++;
                var result = await call();
                var rate = _client.GetLastApiInfo()?.RateLimit;
                if (!search && rate is { Remaining: < 25 }) await SleepUntil(rate.Reset, "primary rate limit low");
                return result;
            }
            catch (RateLimitExceededException e)
            {
                await SleepUntil(e.Reset, "primary rate limit exceeded");
            }
            catch (SecondaryRateLimitExceededException) when (attempt < 8)
            {
                await SleepFor(TimeSpan.FromSeconds(60 * attempt), "secondary rate limit");
            }
            catch (AbuseException e) when (attempt < 8)
            {
                await SleepFor(TimeSpan.FromSeconds(e.RetryAfterSeconds ?? 60), "abuse detection");
            }
            catch (ApiException e) when (attempt < 5 && (int)e.StatusCode >= 500)
            {
                await SleepFor(TimeSpan.FromSeconds(5 * attempt), $"server error {(int)e.StatusCode}");
            }
            catch (HttpRequestException) when (attempt < 5)
            {
                await SleepFor(TimeSpan.FromSeconds(5 * attempt), "network error");
            }
            catch (TaskCanceledException) when (attempt < 5)
            {
                await SleepFor(TimeSpan.FromSeconds(5 * attempt), "timeout");
            }
        }
    }

    public static ApiOptions Page(int page) =>
        // PageCount must stay 1: without it Octokit silently downloads every page per call.
        new() { PageSize = PageSize, StartPage = page, PageCount = 1 };

    private async Task SleepUntil(DateTimeOffset reset, string why) =>
        await SleepFor(reset - DateTimeOffset.UtcNow + TimeSpan.FromSeconds(5), why);

    private async Task SleepFor(TimeSpan span, string why)
    {
        if (span < TimeSpan.Zero) return;
        _log($"  [{why}] sleeping {span.TotalMinutes:F1} min");
        await Task.Delay(span);
    }
}
