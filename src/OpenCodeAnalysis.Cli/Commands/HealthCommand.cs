using OpenCodeAnalysis;

namespace OpenCodeAnalysis.Cli;

/// Writes the contributor filtering, categories and process metrics.
public sealed class HealthCommand : ICommand
{
    public string Name => "health";

    public async Task RunAsync(CommandContext context)
    {
        var overrides = Path.Combine(context.Paths.Root, "data", "identity_overrides.json");
        var team = await new GitHubFetcher(context.Cache).TeamAsync(Pins.Pin);
        HealthReport.Health(Dataset.Load(context.Cache), context.Run, context.Paths.OutDir, overrides,
            team.ToHashSet(StringComparer.OrdinalIgnoreCase));
    }
}
