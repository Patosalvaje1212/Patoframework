
namespace PatoframeWork;

public static class ErrorManager
{
    static TextWriter ErrorTW => Console.Error;
    public static void LogError(string args)
    {
        TextWriter.CreateBroadcasting(ErrorTW).WriteLine("ERROR: " + args);
    }
}

