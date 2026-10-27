using System.Globalization;

namespace OpenCodeAnalysis;

/// Formats metric values for the practice CSV files.
public static class Format
{
    public static string Number(double value, int digits = 2) =>
        double.IsFinite(value)
            ? Math.Round(value, digits, MidpointRounding.AwayFromZero)
                .ToString("0." + new string('#', digits), CultureInfo.InvariantCulture)
            : "";

    public static string Pct(double fraction) =>
        double.IsFinite(fraction)
            ? (100 * fraction).ToString("0.0", CultureInfo.InvariantCulture) + "%"
            : "n/a";

    public static string Pct(long part, long whole) => whole == 0 ? "n/a" : Pct((double)part / whole);

    public static string Opt(double? value, int digits = 1) => value is { } v ? Number(v, digits) : "";
}
