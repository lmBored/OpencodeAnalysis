namespace OpenCodeAnalysis;

/// Copy from Workshop 2 Solution
public static class Sampling
{
    /// Random sample of n items
    public static List<T> RandomSample<T, TKey>(IEnumerable<T> frame, int n, int seed, Func<T, TKey> stableOrder)
    {
        var items = frame.OrderBy(stableOrder).ToList();
        var rng = new Random(seed);
        for (var i = items.Count - 1; i > 0; i--)
        {
            var j = rng.Next(i + 1);
            (items[i], items[j]) = (items[j], items[i]);
        }
        return items.Take(n).ToList();
    }

    /// Stratified sample
    public static List<T> StratifiedByQuarter<T, TKey>(
        IEnumerable<T> frame, Func<T, DateTimeOffset> date, int perStratum, int seed, Func<T, TKey> stableOrder) =>
        frame.GroupBy(x => (date(x).Year, Quarter: (date(x).Month - 1) / 3 + 1))
            .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Quarter)
            .SelectMany(g => RandomSample(g, perStratum, seed, stableOrder))
            .ToList();

    /// Targeted sample (matching a predicate)
    public static List<T> Targeted<T>(IEnumerable<T> frame, Func<T, bool> predicate, Func<T, DateTimeOffset> date) =>
        frame.Where(predicate).OrderBy(date).ToList();
}
