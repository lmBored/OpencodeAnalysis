namespace OpenCodeAnalysis.Cli;

/// Repository-relative locations: data/, out/, and the solution root.
public sealed record AppPaths(string Root, string DataDir, string OutDir)
{
    public static AppPaths Find(string[] args)
    {
        var rootArg = args.FirstOrDefault(a => a.StartsWith("--root="));
        var root = rootArg is null
            ? Discover()
            : Path.GetFullPath(rootArg["--root=".Length..]);
        return new AppPaths(root, Path.Combine(root, "data"), Path.Combine(root, "out"));
    }

    public string OutputFor(string command) =>
        Directory.CreateDirectory(Path.Combine(OutDir, command)).FullName;

    static string Discover()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "OpencodeAnalysis.sln")))
            dir = dir.Parent;
        return dir?.FullName ?? Directory.GetCurrentDirectory();
    }
}
