using System.Text;

namespace OpenCodeAnalysis;

public sealed record DesignRequest(IssueRow Issue, string Login, DateTimeOffset At, string Quote,
    string? AnswerLogin, double AnswerHours, bool Explicit, bool DesignDecision)
{
    public bool TeamReplied => AnswerLogin is not null;
}

public sealed record GateResult(int IssuesWithComments, int IssuesWithTeam,
    IReadOnlyList<DesignRequest> Requests, int TeamSize);

public static class GateAudit
{
    private static readonly HashSet<int> ExplicitRequests =
    [
        3307, 8677, 9089, 16992, 19143, 21227, 22195, 28175, 29437, 33123, 39893,
        39987, 42288, 43223, 43596, 43598, 44660, 46156, 46814, 47600, 47639, 49088,
    ];

    private static readonly HashSet<int> DesignDecisions = [3307];

    public static GateResult Compute(PracticeData d, IReadOnlySet<string> team)
    {
        int withComments = 0, withTeam = 0;
        var requests = new List<DesignRequest>();
        foreach (var issue in d.Issues)
        {
            var list = d.CommentsByNumber.GetValueOrDefault(issue.Number) ?? [];
            if (list.Count > 0) withComments++;
            if (list.Any(c => !c.IsBot && team.Contains(c.Login))) withTeam++;

            var ask = list.FirstOrDefault(c => !c.IsBot && !team.Contains(c.Login) && CommentText.DesignRequest.IsMatch(c.Body));
            if (ask is null) continue;
            var answer = list.FirstOrDefault(c => !c.IsBot && team.Contains(c.Login) && c.CreatedAt > ask.CreatedAt);
            requests.Add(new DesignRequest(issue, ask.Login, ask.CreatedAt, Quote(ask.Body),
                answer?.Login, answer is null ? double.NaN : (answer.CreatedAt - ask.CreatedAt).TotalHours,
                ExplicitRequests.Contains(issue.Number), DesignDecisions.Contains(issue.Number)));
        }
        requests.Sort((a, b) => b.At.CompareTo(a.At));
        return new GateResult(withComments, withTeam, requests, team.Count);
    }

    static string Quote(string body)
    {
        var text = System.Text.RegularExpressions.Regex.Replace(body, "\\s+", " ");
        var sentences = System.Text.RegularExpressions.Regex.Split(text, "(?<=[.!?]) ");
        var hit = sentences.FirstOrDefault(s => CommentText.DesignKeyword.IsMatch(s)) ?? sentences.FirstOrDefault() ?? "";
        return hit.Length <= 260 ? hit.Trim() : hit[..260].Trim();
    }

}
