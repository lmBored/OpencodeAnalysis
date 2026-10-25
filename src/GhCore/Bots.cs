namespace GhCore;

public static class Bots
{
    /// Known automation identities that are not marked as GitHub Apps (M2)
    static readonly HashSet<string> Automation = new(StringComparer.OrdinalIgnoreCase)
    {
        "actions-user", "github-actions", "dependabot", "renovate", "opencode-agent", "opencode", "copilot",
        "copilot-pull-request-reviewer", "greptile-apps",
    };

    /// M1: GitHub account type is Bot, or the login carries the [bot] suffix of GitHub Apps
    public static bool IsBotAccount(string? login, string? type = null) =>
        string.Equals(type, "Bot", StringComparison.OrdinalIgnoreCase)
        || (login?.EndsWith("[bot]", StringComparison.OrdinalIgnoreCase) ?? false);

    /// M2: login (or commit author name) of a known automation identity.
    public static bool IsKnownAutomation(string? login) =>
        !string.IsNullOrEmpty(login)
        && (Automation.Contains(login.Replace("[bot]", "")) || login.Equals("GitHub Action", StringComparison.OrdinalIgnoreCase));

    public static bool IsBot(string? login, string? type = null)
    {
        if (string.Equals(type, "Bot", StringComparison.OrdinalIgnoreCase)) return true;
        if (string.IsNullOrEmpty(login)) return false;
        return login.EndsWith("[bot]", StringComparison.OrdinalIgnoreCase)
               || Automation.Contains(login.Replace("[bot]", ""));
    }

    /// Review bots are already covered by the automation set.
    public static bool IsReviewBot(string? login, string? type = null) => IsBot(login, type);
}
