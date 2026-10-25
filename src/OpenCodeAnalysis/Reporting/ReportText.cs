namespace OpenCodeAnalysis;

/// Small shared helpers for the chapter reports.
internal static class ReportText
{
    internal const int CommitSampleSize = 30;
    internal static readonly string RepoUrl = $"https://github.com/{GitHubFetcher.Owner}/{GitHubFetcher.Repo}";

    internal static string FirstLine(string s, int max = 110)
    {
        var line = s.Split('\n')[0].Trim();
        return line.Length <= max ? line : line[..max] + "...";
    }
}
