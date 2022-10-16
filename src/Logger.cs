public static class Logger
{
    public static void LogDebug(string? msg) => LogMessage(msg);

    public static void LogSuccess(string? msg) => LogMessage(msg, ConsoleColor.Green);

    public static void LogWarning(string? msg) => LogMessage(msg, ConsoleColor.DarkYellow);

    public static void LogError(string? msg) => LogMessage(msg, ConsoleColor.Red);

    private static void LogMessage(string? msg) => Console.WriteLine(msg);

    private static readonly object SyncObj = new();

    private static void LogMessage(string? msg, ConsoleColor color)
    {
        lock (SyncObj)
        {
            var currentColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(msg);
            Console.ForegroundColor = currentColor;
        }
    }
}
