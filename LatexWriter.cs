using System.Text;

namespace OpenCodeAnalysis;

public static class LatexWriter
{
    public static string Escape(string s) => s
        .Replace(@"\", @"\textbackslash{}")
        .Replace("_", @"\_").Replace("&", @"\&").Replace("%", @"\%").Replace("#", @"\#").Replace("$", @"\$");

    public static string Longtable(string caption, string label, string[] header, string colSpec, IEnumerable<string[]> rows,
        IEnumerable<string[]>? footerRows = null)
    {
        var head = string.Join(" & ", header.Select(h => $"\\textbf{{{h}}}")) + @" \\";
        var sb = new StringBuilder();
        sb.AppendLine(@"\begingroup");
        sb.AppendLine(@"\footnotesize");
        sb.AppendLine(@"\setlength{\tabcolsep}{4pt}");
        sb.AppendLine(@"\renewcommand{\arraystretch}{1.2}");
        sb.AppendLine($@"\begin{{longtable}}{{{colSpec}}}");
        sb.AppendLine($@"\caption{{{caption}}}");
        sb.AppendLine($@"\label{{{label}}}\\");
        sb.AppendLine(@"\hline");
        sb.AppendLine(head);
        sb.AppendLine(@"\hline");
        sb.AppendLine(@"\endfirsthead");
        sb.AppendLine($@"\multicolumn{{{header.Length}}}{{l}}{{\textit{{Table \thetable\ continued from previous page}}}}\\");
        sb.AppendLine(@"\hline");
        sb.AppendLine(head);
        sb.AppendLine(@"\hline");
        sb.AppendLine(@"\endhead");
        sb.AppendLine(@"\hline");
        sb.AppendLine(@"\endfoot");
        foreach (var r in rows) sb.AppendLine(string.Join(" & ", r) + @" \\");
        if (footerRows is not null)
        {
            sb.AppendLine(@"\hline");
            foreach (var r in footerRows) sb.AppendLine(string.Join(" & ", r) + @" \\");
        }
        sb.AppendLine(@"\end{longtable}");
        sb.AppendLine(@"\endgroup");
        return sb.ToString();
    }
}
