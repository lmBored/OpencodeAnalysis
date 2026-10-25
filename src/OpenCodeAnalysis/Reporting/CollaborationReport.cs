using System.Globalization;
using System.Text;
using GhCore;
using static OpenCodeAnalysis.ReportText;

namespace OpenCodeAnalysis;

public static class CollaborationReport
{
    public static void Write(Dataset data, RunInfo run, string outDir, int topN)
    {
        var top = data.Top;

        // author x actor matrix
        var matrix = new StringBuilder("author \\ actor," + string.Join(',', top) + "\n");
        foreach (var author in top)
        {
            var cells = top.Select(actor => actor == author ? "" :
                CollaborationAnalysis.Directed(data, author, actor) is var i ? $"{i.Reviews}r/{i.Comments}c/{i.Mentions}m" : "");
            matrix.AppendLine(author + "," + string.Join(',', cells));
        }
        File.WriteAllText(Path.Combine(outDir, "review_matrix.csv"), matrix.ToString());

        // ranked pairs
        var pairs = CollaborationAnalysis.RankPairs(data);
        var ranked = new StringBuilder("a,b,b_on_a_reviews,b_on_a_comments,b_mentions_a,a_on_b_reviews,a_on_b_comments,a_mentions_b,weaker_direction,total\n");
        foreach (var p in pairs)
            ranked.AppendLine($"{p.A},{p.B},{p.AtoB.Reviews},{p.AtoB.Comments},{p.AtoB.Mentions},{p.BtoA.Reviews},{p.BtoA.Comments},{p.BtoA.Mentions},{p.Weaker},{p.Total}");
        File.WriteAllText(Path.Combine(outDir, "pairs_ranked.csv"), ranked.ToString());

        // targeted threads of the strongest pairs
        var md = new StringBuilder();
        md.AppendLine($"# Candidate threads (targeted sample; window {run.Since:yyyy-MM-dd}..{run.Until:yyyy-MM-dd})");
        md.AppendLine("Frame: PRs created in the window by either member; filter: >=3 comments by the two members together and >=1 by the non-author.");
        md.AppendLine();
        foreach (var p in pairs.Take(5))
        {
            var threads = CollaborationAnalysis.Threads(data, p.A, p.B);
            md.AppendLine($"## {p.A} <-> {p.B} (weaker direction {p.Weaker}, total {p.Total}; {threads.Count} threads)");
            foreach (var t in threads.OrderByDescending(t => Math.Min(t.ByA, t.ByB)).ThenByDescending(t => t.ByA + t.ByB).Take(12))
            {
                md.AppendLine($"### #{t.Number} by {t.Author} ({t.CreatedAt:yyyy-MM-dd}) {t.Title}");
                md.AppendLine($"{t.Url} | comments {p.A}={t.ByA}, {p.B}={t.ByB}, formal review between them: {(t.Reviews > 0 ? "yes" : "no")}");
                foreach (var c in t.Comments.Take(8))
                    md.AppendLine($"- {c.CreatedAt:yyyy-MM-dd HH:mm} **{c.Login}** ({c.Kind}{(c.Path is null ? "" : " " + c.Path)}): {FirstLine(c.Body, 220)}");
                md.AppendLine();
            }
        }
        File.WriteAllText(Path.Combine(outDir, "pairs_candidates.md"), md.ToString());

        // counterexample: co-editing without interaction
        var coEdits = CollaborationAnalysis.CoEdits(data, pairs);
        var ce = new StringBuilder("a,b,shared_files,files_a,files_b,jaccard,mutual_interactions,example_shared_files\n");
        foreach (var c in coEdits)
            ce.AppendLine($"{c.A},{c.B},{c.SharedFiles},{c.FilesA},{c.FilesB},{c.Jaccard:F3},{c.Interactions},{Csv.Escape(string.Join(" ", c.Examples))}");
        File.WriteAllText(Path.Combine(outDir, "counter_example.csv"), ce.ToString());

        Console.WriteLine(ranked.ToString());
        Console.WriteLine(ce.ToString());
    }

}
