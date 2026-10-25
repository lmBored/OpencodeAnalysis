using OpenCodeAnalysis;
using OpenCodeAnalysis.Cli;

var paths = AppPaths.Find(args);
Directory.CreateDirectory(paths.OutDir);

var cache = new Cache(paths.DataDir);
var run = await cache.GetOrFetch("run", () =>
{
    var until = new DateTimeOffset(DateTime.UtcNow.Date, TimeSpan.Zero);
    return Task.FromResult(new RunInfo(DateTime.UtcNow, until.AddDays(-365), until, Seed: 42));
});
Console.WriteLine($"Window: {run.Since:yyyy-MM-dd} .. {run.Until:yyyy-MM-dd} (exclusive), collected {run.CollectedAt:u}, seed {run.Seed}");

ICommand[] commands =
[
    new FetchCommand(),
    new FetchHealthCommand(),
    new ContributorsCommand(),
    new CollaborationCommand(),
    new ActivitiesCommand(),
    new PracticesCommand(),
    new HealthCommand(),
    new AllCommand(),
];

var name = args.FirstOrDefault(a => !a.StartsWith("--")) ?? "all";
if (name is "help" or "-h" or "--help")
{
    Usage(commands);
    return 0;
}

var command = commands.FirstOrDefault(c => c.Name == name);
if (command is null)
{
    Usage(commands);
    return 1;
}

var context = new CommandContext(paths, cache, run, paths.OutputFor(command.Name), args);
await command.RunAsync(context);
return 0;

static void Usage(IReadOnlyList<ICommand> commands) =>
    Console.Error.WriteLine($"usage: dotnet run --project src/OpenCodeAnalysis.Cli -- " +
                            $"[{string.Join("|", commands.Select(c => c.Name))}] [--root=<path>]");
