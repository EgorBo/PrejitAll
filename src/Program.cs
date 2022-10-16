public static class PrejitAllTool
{
    public static readonly string Crossgen2Exe = OperatingSystem.IsWindows() ? "crossgen2.exe" : "crossgen2";

    private static void PrintHelp()
    {
        Console.WriteLine("Run this tool to prejit all *.dll in the current directory");
        Console.WriteLine();
        Console.WriteLine("Usage:");
        Console.WriteLine();
        Console.WriteLine($"  prejitall path/to/{Crossgen2Exe} <args for {Crossgen2Exe}>");
        Console.WriteLine();
        Console.WriteLine($"NOTE: if no args are passed it will try to locate system's {Crossgen2Exe} automatically");
        Console.WriteLine();
    }

    public static async Task Main(string[] args)
    {
        string crossgenPath;
        if (args.Length == 0)
        {
            Logger.LogWarning($"{Crossgen2Exe} path is not specified, trying to find one in the nuget cache...");
            crossgenPath = await DotnetInfo.FindExistingCrossgen2() ?? "";

            if (!File.Exists(crossgenPath))
                Logger.LogError($"I was trying to find {Crossgen2Exe} in your nuget cache but with no luck\nMaybe it's not there yet? Try to run 'dotnet publish -c Release -r <RID> /p:PublishReadyToRun=true' for a console app, maybe that will populate it.");
        }
        else
        {
            crossgenPath = args[0];
        }

        if (!crossgenPath.EndsWith(Crossgen2Exe, StringComparison.OrdinalIgnoreCase) || !File.Exists(crossgenPath))
        {
            PrintHelp();
            return;
        }

        string customArgs = string.Join(" ", args.Skip(1).Select(a => $"\"{a}\""));

        long totalAdditionalSize = 0;

        string[] allFiles = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.dll");

        // Don't waste time on these on Windows:
        string[] knownNativeLibs =
        {
            "clretwrc.dll",
            "clrgc.dll",
            "clrjit.dll",
            "coreclr.dll",
            "hostfxr.dll",
            "hostpolicy.dll",
            "Microsoft.DiaSymReader.Native.amd64.dll",
            "mscordaccore.dll",
            "mscordbi.dll",
            "mscorrc.dll",
            "msquic.dll",
            "System.IO.Compression.Native.dll"
        };

        allFiles = allFiles.Where(f => !knownNativeLibs.Contains(Path.GetFileName(f))).ToArray();

        if (!allFiles.Any())
        {
            Logger.LogWarning("No *.dll files in the current directory");
            return;
        }

        for (var index = 0; index < allFiles.Length; index++)
        {
            string file = allFiles[index];
            string managedLibName = Path.GetFileName(file);

            Logger.LogDebug("==========================================================");
            Logger.LogDebug($"= [{index + 1}/{allFiles.Length}] Prejitting \"{managedLibName}\"");
            Logger.LogDebug("==========================================================");

            string tempR2Rname = managedLibName + ".ni";
            await ProcessUtils.RunProcessAsync(crossgenPath, $"{managedLibName} -r *.dll --resilient -O -o {tempR2Rname} " + customArgs);
            if (File.Exists(tempR2Rname))
            {
                long beforeSize = new FileInfo(managedLibName).Length;
                long afterSize = new FileInfo(tempR2Rname).Length;

                Logger.LogSuccess($"Size delta: {((afterSize - beforeSize) / 1024.0f):F0}Kb");

                totalAdditionalSize += afterSize - beforeSize;

                File.Delete(file);
                File.Move(tempR2Rname, file);
            }
        }

        Logger.LogSuccess(" ");
        Logger.LogSuccess("==========================================================");
        Logger.LogSuccess($"= Done! Total size added: {(totalAdditionalSize / 1024 / 1024):F2}Mb");
        Logger.LogSuccess("==========================================================");
    }
}
