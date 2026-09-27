using System.Net;
using System.Text;

namespace GhCore;

/// Raw HTTP GET with conditional revalidation (If-None-Match / 304) against FileResponseCache,
/// retrying transient statuses. Used for endpoints Octokit cannot walk with page options,
/// where the request goes through HttpClient directly.
public sealed class EtagHttp(HttpClient http, FileResponseCache cache, Action<string>? log = null)
{
    private const int MaxAttempts = 5;
    private readonly Action<string> _log = log ?? Console.WriteLine;

    public async Task<HttpResponseMessage> GetAsync(string label, string url)
    {
        for (var attempt = 1; ; attempt++)
        {
            var cached = await cache.TryGetRawAsync(url);
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            if (cached is { } hit)
            {
                request.Headers.TryAddWithoutValidation("If-None-Match", hit.Etag);
            }

            var response = await http.SendAsync(request);
            if (response.StatusCode == HttpStatusCode.NotModified && cached is not null)
            {
                _log($"{label}: unchanged (304), served from cache");
                response.Dispose();
                return Buffered(cached.Value.Entry);
            }

            if (response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                await cache.SetRawAsync(url, response, body);
                var buffered = Buffered(response, body);
                response.Dispose();
                return buffered;
            }

            if (attempt >= MaxAttempts || !IsTransient(response.StatusCode))
            {
                return response;
            }

            _log($"{label}: HTTP {(int)response.StatusCode} - retry {attempt} in {Backoff(attempt).TotalSeconds:0}s");
            response.Dispose();
            await Task.Delay(Backoff(attempt));
        }
    }

    public static string? NextLink(HttpResponseMessage response) =>
        response.Headers.TryGetValues("Link", out var links)
            ? links
                .SelectMany(value => value.Split(','))
                .Select(part => part.Trim())
                .Where(part => part.StartsWith('<') && part.EndsWith("rel=\"next\"", StringComparison.Ordinal))
                .Select(part => part[1..part.IndexOf('>')])
                .FirstOrDefault()
            : null;

    private static HttpResponseMessage Buffered(FileResponseCache.Entry entry)
    {
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(entry.Body, Encoding.UTF8, "application/json"),
        };
        foreach (var (key, value) in entry.Headers)
        {
            response.Headers.TryAddWithoutValidation(key, value);
        }
        return response;
    }

    private static HttpResponseMessage Buffered(HttpResponseMessage source, string body)
    {
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json"),
        };
        foreach (var (key, value) in source.Headers)
        {
            response.Headers.TryAddWithoutValidation(key, value);
        }
        return response;
    }

    private static TimeSpan Backoff(int attempt) => TimeSpan.FromSeconds(Math.Min(300, 30 * Math.Pow(2, attempt - 1)));

    private static bool IsTransient(HttpStatusCode status) => (int)status is 403 or 429 or 500 or 502 or 503;
}
