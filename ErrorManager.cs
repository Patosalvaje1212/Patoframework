
namespace PatoFramework;


/// <summary>
/// Simple class to easily log Errors to the Console
/// </summary>

public static class LogManager
{
    static TextWriter ErrorTW => Console.Error;
    public static void LogError(string args)
    {
        Console.ForegroundColor = ConsoleColor.Red;

        TextWriter.CreateBroadcasting(ErrorTW).WriteLine("ERROR: " + args);
     
        Console.ForegroundColor = ConsoleColor.White;
    }

    public static void LogWarning(string args)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;

        Console.WriteLine("WARNING: " + args);

        Console.ForegroundColor = ConsoleColor.White;
    }

    public static void LogSuccess(string args)
    {
        Console.ForegroundColor = ConsoleColor.Green;

        Console.WriteLine("SUCCESS: " + args);

        Console.ForegroundColor = ConsoleColor.White;
    }
}

