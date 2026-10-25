using System.Diagnostics;

namespace GhCore;

/// Resolves the GitHub token from GITHUB_TOKEN or the `gh` CLI; never stores it.
public static class TokenResolver
{
    public static string? TryResolve()
    {
        var env = Environment.GetEnvironmentVariable("GITHUB_TOKEN");
        if (!string.IsNullOrWhiteSpace(env)) return env.Trim();

        var psi = new ProcessStartInfo("gh", "auth token")
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };
        using var process = Process.Start(psi);
        if (process is null) return null;
        var output = process.StandardOutput.ReadToEnd().Trim();
        process.WaitForExit();
        return process.ExitCode == 0 && output.Length > 0 ? output : null;
    }

    public static string Resolve() =>
        TryResolve() ?? throw new InvalidOperationException("Set GITHUB_TOKEN or run `gh auth login`.");
}
