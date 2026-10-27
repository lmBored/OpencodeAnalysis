using OpenCodeAnalysis;

namespace OpenCodeAnalysis.Cli;

public sealed class ContributorsCommand : ICommand
{
    public string Name => "contributors";

    public Task RunAsync(CommandContext context)
    {
        ContributorsReport.Write(Dataset.Load(context.Cache), context.Run, context.Output, context.TopN);
        return Task.CompletedTask;
    }
}

public sealed class CollaborationCommand : ICommand
{
    public string Name => "collaboration";

    public Task RunAsync(CommandContext context)
    {
        CollaborationReport.Write(Dataset.Load(context.Cache), context.Run, context.Output, context.TopN);
        return Task.CompletedTask;
    }
}

public sealed class ActivitiesCommand : ICommand
{
    public string Name => "activities";

    public Task RunAsync(CommandContext context)
    {
        ActivitiesReport.Write(Dataset.Load(context.Cache), context.Run, context.Output, context.TopN);
        return Task.CompletedTask;
    }
}

/// `all` = fetch + contributors + collaboration, each into its own out/ folder.
public sealed class AllCommand : ICommand
{
    public string Name => "all";

    public async Task RunAsync(CommandContext context)
    {
        ICommand[] pipeline = [new FetchCommand(), new ContributorsCommand(), new CollaborationCommand()];
        foreach (var command in pipeline)
            await command.RunAsync(context with { Output = context.Paths.OutputFor(command.Name) });
    }
}
