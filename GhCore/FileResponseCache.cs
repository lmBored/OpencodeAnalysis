using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Octokit;
using Octokit.Caching;
using Octokit.Internal;

namespace GhCore;

/// <summary>
/// Disk-backed response cache for GitHub's conditional requests (ETag / If-None-Match).
/// Backs Octokit's <c>CachingHttpClient</c> via <c>GitHubClient.ResponseCache</c>; the raw
/// issues endpoint uses the same store. Revalidated 304 answers are served from disk and do
/// not count against the REST rate limit, so re-runs skip work whose content did not change.
/// </summary>
public sealed class FileResponseCache(string directory, bool refresh) : IResponseCache
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    private readonly string directory = directory;
    private readonly bool refresh = refresh;

    public async Task<CachedResponse.V1?> GetAsync(IRequest request)
    {
        if (refresh)
        {
            return null;
        }

        var entry = await LoadAsync(PathFor(request.Method.Method, AbsoluteUrl(request)));
        return entry is null ? null : ToV1(entry);
    }

    public async Task SetAsync(IRequest request, CachedResponse.V1 response)
    {
        if (refresh)
        {
            return;
        }

        var entry = new Entry
        {
            StatusCode = (int)response.StatusCode,
            ContentType = response.ContentType,
            Headers = new Dictionary<string, string>(response.Headers, StringComparer.OrdinalIgnoreCase),
            Body = response.Body as string ?? string.Empty,
            StoredAt = DateTimeOffset.UtcNow,
        };
        await SaveAsync(PathFor(request.Method.Method, AbsoluteUrl(request)), entry);
    }

    public async Task<(string Etag, Entry Entry)?> TryGetRawAsync(string url)
    {
        if (refresh)
        {
            return null;
        }

        var entry = await LoadAsync(PathFor("GET", url));
        var etag = Header(entry, "ETag");
        return entry is null || string.IsNullOrEmpty(etag) ? null : (etag, entry);
    }

    public async Task SetRawAsync(string url, HttpResponseMessage response, string body)
    {
        if (refresh)
        {
            return;
        }

        var etag = response.Headers.TryGetValues("ETag", out var values) ? values.FirstOrDefault() : null;
        if (string.IsNullOrEmpty(etag))
        {
            return;
        }

        var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var (key, value) in response.Headers)
        {
            headers[key] = value.FirstOrDefault() ?? string.Empty;
        }

        foreach (var (key, value) in response.Content.Headers)
        {
            headers[key] = value.FirstOrDefault() ?? string.Empty;
        }

        await SaveAsync(
            PathFor("GET", url),
            new Entry
            {
                StatusCode = (int)response.StatusCode,
                ContentType = response.Content.Headers.ContentType?.ToString(),
                Headers = headers,
                Body = body,
                StoredAt = DateTimeOffset.UtcNow,
            });
    }

    public void Prepare()
    {
        Directory.CreateDirectory(directory);
        if (refresh)
        {
            foreach (var file in Directory.EnumerateFiles(directory, "*.json"))
            {
                File.Delete(file);
            }
            return;
        }

        var cutoff = DateTimeOffset.UtcNow.AddDays(-60);
        foreach (var file in Directory.EnumerateFiles(directory, "*.json"))
        {
            if (File.GetLastWriteTimeUtc(file) < cutoff.UtcDateTime)
            {
                File.Delete(file);
            }
        }
    }

    private static string AbsoluteUrl(IRequest request) =>
        request.Endpoint is { IsAbsoluteUri: true } endpoint
            ? endpoint.ToString()
            : new Uri(request.BaseAddress!, request.Endpoint).ToString();

    private string PathFor(string method, string url)
    {
        var key = Encoding.UTF8.GetBytes($"{method.ToUpperInvariant()} {url}");
        var hash = Convert.ToHexString(SHA256.HashData(key)).ToLowerInvariant();
        return Path.Combine(directory, $"{hash}.json");
    }

    private async Task<Entry?> LoadAsync(string path)
    {
        try
        {
            if (!File.Exists(path))
            {
                return null;
            }

            return JsonSerializer.Deserialize<Entry>(await File.ReadAllTextAsync(path), JsonOptions);
        }
        catch (Exception exception) when (exception is IOException or JsonException)
        {
            return null;
        }
    }

    private async Task SaveAsync(string path, Entry entry)
    {
        var json = JsonSerializer.Serialize(entry, JsonOptions);
        var temp = $"{path}.{Guid.NewGuid():N}.tmp";
        await File.WriteAllTextAsync(temp, json);
        File.Move(temp, path, overwrite: true);
    }

    private static CachedResponse.V1 ToV1(Entry entry) =>
        new(
            entry.Body,
            new Dictionary<string, string>(entry.Headers, StringComparer.OrdinalIgnoreCase),
            new ApiInfo(
                ParseLinks(Header(entry, "Link")),
                ParseScopes(Header(entry, "x-oauth-scopes")),
                ParseScopes(Header(entry, "x-accepted-oauth-scopes")),
                Header(entry, "ETag"),
                ParseRateLimit(entry),
                TimeSpan.Zero),
            (System.Net.HttpStatusCode)entry.StatusCode,
            entry.ContentType);

    private static string? Header(Entry? entry, string name) =>
        entry is not null && entry.Headers.TryGetValue(name, out var value) ? value : null;

    private static RateLimit ParseRateLimit(Entry entry)
    {
        try
        {
            return new RateLimit(new Dictionary<string, string>(entry.Headers, StringComparer.OrdinalIgnoreCase));
        }
        catch (Exception)
        {
            return new RateLimit();
        }
    }

    private static IList<string> ParseScopes(string? header) =>
        string.IsNullOrWhiteSpace(header)
            ? Array.Empty<string>()
            : header.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    private static IDictionary<string, Uri> ParseLinks(string? header)
    {
        var links = new Dictionary<string, Uri>(StringComparer.OrdinalIgnoreCase);
        if (string.IsNullOrWhiteSpace(header))
        {
            return links;
        }

        foreach (var part in header.Split(','))
        {
            var segment = part.Trim();
            var open = segment.IndexOf('<');
            var close = segment.IndexOf('>');
            if (open != 0 || close < 0)
            {
                continue;
            }

            var relIndex = segment.IndexOf("rel=", StringComparison.OrdinalIgnoreCase);
            if (relIndex < 0)
            {
                continue;
            }

            var rel = segment[(relIndex + 4)..].Trim().Trim('"', '\'');
            if (Uri.TryCreate(segment[1..close], UriKind.Absolute, out var uri))
            {
                links[rel] = uri;
            }
        }

        return links;
    }

    public sealed class Entry
    {
        public int StatusCode { get; set; }
        public string? ContentType { get; set; }
        public Dictionary<string, string> Headers { get; set; } = new(StringComparer.OrdinalIgnoreCase);
        public string Body { get; set; } = string.Empty;
        public DateTimeOffset StoredAt { get; set; }
    }
}
