namespace GhCore;

public static class Bots
{
    static readonly HashSet<string> Automation = new(StringComparer.OrdinalIgnoreCase)
    {
        "actions-user", "github-actions", "dependabot", "renovate", "opencode-agent", "opencode", "copilot",
    };

    public static bool IsBot(string? login, string? type = null)
    {
        if (string.Equals(type, "Bot", StringComparison.OrdinalIgnoreCase)) return true;
        if (string.IsNullOrEmpty(login)) return false;
        return login.EndsWith("[bot]", StringComparison.OrdinalIgnoreCase)
               || Automation.Contains(login.Replace("[bot]", ""));
    }
}
