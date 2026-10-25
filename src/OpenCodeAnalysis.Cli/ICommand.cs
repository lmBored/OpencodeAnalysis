using OpenCodeAnalysis;

namespace OpenCodeAnalysis.Cli;

public interface ICommand
{
    string Name { get; }
    Task RunAsync(CommandContext context);
}

public sealed record CommandContext(
    AppPaths Paths,
    Cache Cache,
    RunInfo Run,
    string Output,
    IReadOnlyList<string> Args,
    int TopN = 10)
{
    public string Arg(string name, string fallback)
    {
        var prefix = name + "=";
        return Args.Select(a => a.StartsWith(prefix) ? a[prefix.Length..] : null)
            .FirstOrDefault(v => v is not null) ?? fallback;
    }
}
