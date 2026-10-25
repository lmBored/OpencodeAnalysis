using GhCore;

namespace OpenCodeAnalysis;

public sealed record MigrationRow(AuthorClass? Class, int N, double Legacy, double V2, double Ui,
    double AnyGone, double AllGone, double Merged);

public sealed record MigrationResult(string HeadRef, string PinRef, int CommitsAhead, int SampleN, int Skipped,
    IReadOnlyList<MigrationRow> Rows, IReadOnlyDictionary<string, (int Pin, int Head)> TreeCounts,
    IReadOnlyList<(PullRecord Pr, int Gone, int Total, string FirstGone)> Examples,
    IReadOnlyList<int> SamplePrs);

/// Checks whether sampled pull requests touch files that still exist in a later tree.
public static class MigrationAudit
{
    const int Seed = 42;
    static readonly (AuthorClass Class, int Want)[] Strata =
    [
        (AuthorClass.FirstTimeAuthor, 150), (AuthorClass.RepeatAuthor, 100), (AuthorClass.Top10, 50),
    ];
    static readonly string[] Counted =
    [
        "packages/opencode", "packages/core/src/v1", "packages/core", "packages/cli", "packages/app",
    ];

    public static async Task<MigrationResult> ComputeAsync(PracticeData d, GitHubFetcher gh, string headRef, string pinRef)
    {
        var sample = Sample(d);
        var paths = new Dictionary<int, List<string>>();
        var skipped = 0;
        foreach (var p in sample)
        {
            try { paths[p.Number] = (await gh.FilesAsync(p.Number, p.Login)).Select(f => f.Path).ToList(); }
            catch (Exception e)
            {
                skipped++;
                Console.Error.WriteLine($"migration: PR {p.Number} unavailable: {e.Message}");
            }
        }

        var commitsAhead = await gh.PracticeCommitsAheadAsync(pinRef, headRef);
        var head = (await gh.PracticeTreeAsync(headRef)).ToHashSet(StringComparer.Ordinal);
        var pin = await gh.PracticeTreeAsync(pinRef);
        var counts = Counted.ToDictionary(
            prefix => prefix,
            prefix => (Pin: pin.Count(p => p.StartsWith(prefix + "/", StringComparison.Ordinal)),
                Head: head.Count(p => p.StartsWith(prefix + "/", StringComparison.Ordinal))),
            StringComparer.Ordinal);

        var rows = new List<MigrationRow>();
        var examples = new List<(PullRecord, int, int, string)>();
        foreach (var (cls, _) in Strata.Select(s => ((AuthorClass?)s.Class, s.Want)).Append((null, 0)))
        {
            var list = sample.Where(p => paths.ContainsKey(p.Number) && (cls is null || d.ClassOf(p) == cls)).ToList();
            if (list.Count == 0) continue;
            var details = list.Select(p =>
            {
                var files = paths[p.Number];
                var gone = files.Where(f => !head.Contains(f)).ToList();
                return (p, files, gone);
            }).ToList();
            rows.Add(new MigrationRow(cls, list.Count,
                Frac(details.Count(x => x.files.Any(IsLegacy)), list.Count),
                Frac(details.Count(x => x.files.Any(IsV2)), list.Count),
                Frac(details.Count(x => x.files.Any(IsUi)), list.Count),
                Frac(details.Count(x => x.gone.Count > 0), list.Count),
                Frac(details.Count(x => x.files.Count > 0 && x.gone.Count == x.files.Count), list.Count),
                Frac(details.Count(x => x.p.MergedAt is not null), list.Count)));
            if (cls == AuthorClass.FirstTimeAuthor)
                examples = details.Where(x => x.gone.Count > 0 && x.p.MergedAt is null)
                    .Take(6).Select(x => (x.p, x.gone.Count, x.files.Count, x.gone[0])).ToList();
        }
        return new MigrationResult(headRef, pinRef, commitsAhead, sample.Count, skipped, rows, counts, examples,
            sample.Select(p => p.Number).ToList());
    }

    static double Frac(int a, int b) => b == 0 ? double.NaN : (double)a / b;

    static bool IsLegacy(string path) =>
        path.StartsWith("packages/opencode/", StringComparison.Ordinal) ||
        path.StartsWith("packages/core/src/v1/", StringComparison.Ordinal);

    static bool IsV2(string path) =>
        (path.StartsWith("packages/core/", StringComparison.Ordinal) &&
         !path.StartsWith("packages/core/src/v1/", StringComparison.Ordinal)) ||
        new[] { "packages/server/", "packages/tui/", "packages/cli/", "packages/protocol/", "packages/schema/", "packages/session-ui/" }
            .Any(p => path.StartsWith(p, StringComparison.Ordinal));

    static bool IsUi(string path) =>
        new[] { "packages/app/", "packages/desktop/", "packages/ui/" }
            .Any(p => path.StartsWith(p, StringComparison.Ordinal));

    /// Builds a deterministic stratified sample from the input data.
    static List<PullRecord> Sample(PracticeData d)
    {
        var picked = new List<PullRecord>();
        foreach (var (cls, want) in Strata)
        {
            var pool = d.Pulls.Where(p => !PracticeData.IsBot(p) && d.ClassOf(p) == cls);
            picked.AddRange(Sampling.RandomSample(pool, want, Seed, p => p.Number));
        }
        return picked;
    }
}
