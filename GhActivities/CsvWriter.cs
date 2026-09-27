using System.Text;

namespace GhActivities;

public static class CsvWriter
{
    public static void Write(string path, IEnumerable<string> header, IEnumerable<IEnumerable<string>> rows)
    {
        using var writer = new StreamWriter(path, false, new UTF8Encoding(false));
        writer.WriteLine(Line(header));
        foreach (var row in rows)
            writer.WriteLine(Line(row));
    }

    private static string Line(IEnumerable<string> fields) => string.Join(",", fields.Select(Escape));

    private static string Escape(string field) =>
        field.IndexOfAny(['"', ',', '\n', '\r']) >= 0 ? $"\"{field.Replace("\"", "\"\"")}\"" : field;
}
