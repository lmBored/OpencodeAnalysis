using System.Globalization;
using GhCore;

namespace OpenCodeAnalysis;

public sealed record ReviewActivity(
    int Pr,
    DateTimeOffset SubmittedAt,
    string State,
    string Author,
    string PrAuthor,
    string Body,
    string Url);

public sealed record ActivitiesData(
    List<DateTimeOffset> Commits,
    List<DateTimeOffset> Releases,
    List<ReviewActivity> Reviews)
{
    public static ActivitiesData? Load(string directory)
    {
        if (!Directory.Exists(directory)) return null;
        return new ActivitiesData(
            Dates(Path.Combine(directory, "commits.csv"), 0),
            Dates(Path.Combine(directory, "releases.csv"), 2),
            ReadReviews(Path.Combine(directory, "reviews.csv")));
    }

    private static IEnumerable<string[]> SkipHeader(List<string[]> rows) => rows.Skip(1);

    private static List<DateTimeOffset> Dates(string path, int column)
    {
        var values = new List<DateTimeOffset>();
        if (!File.Exists(path)) return values;
        foreach (var row in SkipHeader(Csv.Read(path)))
        {
            if (row.Length > column && DateTimeOffset.TryParse(row[column], CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out var value))
            {
                values.Add(value);
            }
        }
        return values;
    }

    private static List<ReviewActivity> ReadReviews(string path)
    {
        var reviews = new List<ReviewActivity>();
        if (!File.Exists(path)) return reviews;
        var rows = Csv.Read(path);
        if (rows.Count == 0) return reviews;
        var header = rows[0];
        foreach (var row in rows.Skip(1))
        {
            var map = Csv.Headered(header, row);
            if (!map.TryGetValue("pull_request", out var pr) || !map.TryGetValue("submitted_at", out var at) ||
                !DateTimeOffset.TryParse(at, CultureInfo.InvariantCulture, DateTimeStyles.None, out var submitted))
            {
                continue;
            }
            reviews.Add(new ReviewActivity(
                int.Parse(pr, CultureInfo.InvariantCulture),
                submitted,
                map.GetValueOrDefault("state", string.Empty),
                map.GetValueOrDefault("author", string.Empty),
                map.GetValueOrDefault("pr_author", string.Empty),
                map.GetValueOrDefault("body", string.Empty),
                map.GetValueOrDefault("url", string.Empty)));
        }
        return reviews;
    }
}
