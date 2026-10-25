using OpenCodeAnalysis;

namespace OpenCodeAnalysis.Cli;

/// Runs the practice metrics and audits.
public sealed class PracticesCommand : ICommand
{
    public string Name => "practices";

    public async Task RunAsync(CommandContext context)
    {
        var gh = new GitHubFetcher(context.Cache);
        await gh.ReturningAuthorsAsync(context.Run);
        var data = PracticeData.Load(Dataset.Load(context.Cache), context.Run, context.Cache.Root);
        if (data.Dataset.Activities is null)
        {
            Console.Error.WriteLine("practices: data/activities is missing; run GhActivities first.");
            return;
        }

        var team = (await gh.TeamAsync(Pins.Pin)).ToHashSet(StringComparer.OrdinalIgnoreCase);

        var automation = AutomationAudit.Compute(data, team);
        var gates = GateAudit.Compute(data, team);

        PracticeCsv.WriteMetrics(
            [PracticeCsv.AutomationMetrics(automation), PracticeCsv.GateMetrics(gates)], context.Output);
        PracticeCsv.WriteGateRequests(gates, context.Output);

        var migration = await MigrationAudit.ComputeAsync(data, gh,
            context.Arg("--head", Pins.Head), context.Arg("--pin", Pins.Pin));
        PracticeCsv.WriteMigration(migration, context.Output);
        PracticeCsv.WriteExemplars(ExemplarsAudit.Compute(data), context.Output);

        Console.WriteLine($"practices -> {context.Output}");
    }
}
