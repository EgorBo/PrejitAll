using System.Text.RegularExpressions;

public static class DotnetInfo
{
    private static async Task<string[]> GetRuntimeVersions()
    {
        List<string> versions = new();
        string result = await ProcessUtils.RunProcessAsync("dotnet", "--list-runtimes");
        foreach (Match regex in Regex.Matches(result, "Microsoft.NETCore.App (.*?) \\["))
        {
            if (regex.Success)
                versions.Add(regex.Groups[1].Value);
        }

        return versions.ToArray();
    }

    public static async Task<string?> FindExistingCrossgen2()
    {
        // TODO: detect exact runtime version needed for current dir
        // TODO: run 'dotnet nuget locals all -l' to get nuget caches
        string[] runtimeVersions = await GetRuntimeVersions();
        foreach (var version in runtimeVersions.Reverse())
        {
            string crossgenPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                ".nuget", "packages", "microsoft.netcore.app.crossgen2.win-x64", version, "tools", "crossgen2.exe");
         
            if (File.Exists(crossgenPath))
                return crossgenPath;
        }
        return null;
    }
}
