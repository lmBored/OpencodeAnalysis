namespace GhCore;

/// Minimal RFC 4180 CSV reader (quoted fields, embedded commas/newlines/escaped quotes).
public static class Csv
{
    public static List<string[]> Read(string path)
    {
        var rows = new List<string[]>();
        using var reader = new StreamReader(path);
        var field = new System.Text.StringBuilder();
        var row = new List<string>();
        var inQuotes = false;
        while (true)
        {
            var next = reader.Read();
            if (next < 0)
            {
                if (inQuotes) throw new InvalidDataException($"Unterminated quoted field in {path}");
                if (field.Length > 0 || row.Count > 0)
                {
                    row.Add(field.ToString());
                    rows.Add(row.ToArray());
                }
                return rows;
            }

            var c = (char)next;
            if (inQuotes)
            {
                if (c == '"')
                {
                    if (reader.Peek() == '"')
                    {
                        reader.Read();
                        field.Append('"');
                    }
                    else
                    {
                        inQuotes = false;
                    }
                }
                else
                {
                    field.Append(c);
                }
            }
            else if (c == '"' && field.Length == 0)
            {
                inQuotes = true;
            }
            else if (c == ',')
            {
                row.Add(field.ToString());
                field.Clear();
            }
            else if (c is '\n' or '\r')
            {
                if (c == '\r' && reader.Peek() == '\n') reader.Read();
                row.Add(field.ToString());
                field.Clear();
                rows.Add(row.ToArray());
                row = new List<string>();
            }
            else
            {
                field.Append(c);
            }
        }
    }

    public static Dictionary<string, string> Headered(string[] header, string[] row)
    {
        var map = new Dictionary<string, string>(StringComparer.Ordinal);
        for (var i = 0; i < header.Length && i < row.Length; i++)
        {
            map[header[i]] = row[i];
        }
        return map;
    }
}
