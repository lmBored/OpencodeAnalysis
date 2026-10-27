namespace OpenCodeAnalysis;

public enum MetricKind
{
    Count,
    /// Percent values are stored as fractions between 0 and 1.
    Percent,
    Days1,
    Days2,
    Hours1,
    Hours2,
    Ratio3,
}

public sealed record Metric(string Key, double Value, MetricKind Kind);

public sealed record MetricSet(string Name, IReadOnlyList<Metric> Metrics)
{
    public MetricSet Add(string key, double value, MetricKind kind) =>
        new(Name, [.. Metrics, new Metric(key, value, kind)]);
}
