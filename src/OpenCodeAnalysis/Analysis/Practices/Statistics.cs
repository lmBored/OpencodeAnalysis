namespace OpenCodeAnalysis;

public readonly record struct Summary(int N, double P50, double P75, double P90, double Mean)
{
    public static readonly Summary Empty = new(0, double.NaN, double.NaN, double.NaN, double.NaN);
}

public static class Statistics
{
    /// Returns a nearest-rank percentile from an already sorted list.
    public static double Percentile(IReadOnlyList<double> sorted, double p)
    {
        if (sorted.Count == 0) return double.NaN;
        var index = Math.Max(0, (int)Math.Ceiling(p / 100d * sorted.Count) - 1);
        return sorted[index];
    }

    public static Summary Summarize(IEnumerable<double> values)
    {
        var sorted = values.Where(double.IsFinite).Order().ToList();
        return sorted.Count == 0
            ? Summary.Empty
            : new Summary(sorted.Count, Percentile(sorted, 50), Percentile(sorted, 75),
                Percentile(sorted, 90), sorted.Average());
    }
}
