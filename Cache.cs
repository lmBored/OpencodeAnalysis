using System.Text.Json;

namespace OpenCodeAnalysis;

/// JSON cache under data/, cached page by page.
public class Cache(string root)
{
    static readonly JsonSerializerOptions Json = new() { WriteIndented = false };

    public string Root { get; } = root;

    string PathOf(string name) => Path.Combine(Root, name + ".json");

    public bool Has(string name) => File.Exists(PathOf(name));

    public T Read<T>(string name) =>
        JsonSerializer.Deserialize<T>(File.ReadAllText(PathOf(name)), Json)
        ?? throw new InvalidDataException($"Empty cache entry {name}");

    public void Write<T>(string name, T value)
    {
        var path = PathOf(name);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        var tmp = path + ".tmp";
        File.WriteAllText(tmp, JsonSerializer.Serialize(value, Json));
        File.Move(tmp, path, overwrite: true);
    }

    public async Task<T> GetOrFetch<T>(string name, Func<Task<T>> fetch)
    {
        if (Has(name)) return Read<T>(name);
        var value = await fetch();
        Write(name, value);
        return value;
    }

    /// Fetches pages 1...n until last page.
    /// Each page is cached in data/{name}/p{n}.json.
    public async Task<List<T>> Paged<T>(
        string name,
        Func<int, Task<List<T>>> fetchPage,
        Func<List<T>, bool> isLast)
    {
        if (Has(name)) return Read<List<T>>(name);
        var all = new List<T>();
        for (var page = 1; ; page++)
        {
            var pageName = $"{name}/p{page:D5}";
            var items = await GetOrFetch<List<T>>(pageName, () => fetchPage(page));
            all.AddRange(items);
            if (page % 20 == 0) Console.WriteLine($"  {name}: page {page}, {all.Count} items");
            if (items.Count == 0 || isLast(items)) break;
        }
        Write(name, all);
        return all;
    }

    /// Segment 0 is in data/{name}/, segment s in data/{name}/s{s}/. 
    /// Items at boundary appear twice
    public async Task<List<T>> Segmented<T>(
        string name,
        DateTimeOffset start,
        Func<DateTimeOffset, int, Task<List<T>>> fetchPage,
        Func<T, DateTimeOffset> cursor,
        Func<List<T>, bool> isLast,
        int maxPages = 300)
    {
        if (Has(name)) return Read<List<T>>(name);
        var all = new List<T>();
        var since = start;
        for (var segment = 0; ; segment++)
        {
            var prefix = segment == 0 ? name : $"{name}/s{segment}";
            var finished = false;
            List<T> items = [];
            for (var page = 1; page <= maxPages; page++)
            {
                var (s, p) = (since, page);
                items = await GetOrFetch($"{prefix}/p{page:D5}", () => fetchPage(s, p));
                all.AddRange(items);
                if (page % 20 == 0) Console.WriteLine($"  {name}: segment {segment}, page {page}, {all.Count} items");
                if (items.Count == 0 || isLast(items)) { finished = true; break; }
            }
            if (finished) break;
            since = cursor(items[^1]);
        }
        Write(name, all);
        return all;
    }
}
