using System.Diagnostics;
using System.Text;

public static class ProcessUtils
{
    public static async Task<string> RunProcessAsync(
        string path,
        string args = "",
        Dictionary<string, string>? envVars = null,
        string? workingDir = null)
    {
        var sb = new StringBuilder();
        Logger.LogDebug($"{path} {args}");

        Process? process = null;
        try
        {
            var processStartInfo = new ProcessStartInfo
            {
                FileName = path,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                Arguments = args,
            };

            if (workingDir != null)
                processStartInfo.WorkingDirectory = workingDir;

            if (envVars != null)
            {
                foreach (var envVar in envVars)
                    processStartInfo.EnvironmentVariables[envVar.Key] = envVar.Value;
            }

            process = Process.Start(processStartInfo)!;
            process.ErrorDataReceived += (_, e) => Logger.LogError(e.Data);
            process.OutputDataReceived += (_, e) =>
            {
                sb.Append(e.Data);
                if (e.Data?.Contains("Emitting R2R") == true)
                {
                    Logger.LogSuccess("Successfully prejitted.");
                }
                else if (e.Data?.Contains("No input files are loadable") == true)
                {
                    Logger.LogWarning("Not a managed lib");
                }
                else
                {
                    Logger.LogDebug(e.Data);
                }
            };
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            await process.WaitForExitAsync();
        }
        catch (Exception e)
        {
            Logger.LogError(e.Message);
        }
        finally
        {
            if (process is { HasExited: false })
                process.Kill();
        }
        return sb.ToString();
    }
}
