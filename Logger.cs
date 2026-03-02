using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PF;

public static class Logger
{
    public enum MessageType
    {
        Info,
        Other,
        Warning,
        Error,
    }

    public static void Log(string message, MessageType type = MessageType.Info)
    {
        TextWriter target = Console.Out;
        StringBuilder builder = new();
        builder.Append('[');

        switch (type)
        {
            case MessageType.Info:
            builder.Append("Info");
            break;

            case MessageType.Other:
            Console.ForegroundColor = ConsoleColor.Blue;
            builder.Append("Other");
            break;

            case MessageType.Warning:
            Console.ForegroundColor = ConsoleColor.Yellow;
            builder.Append("Warning");
            break;
            
            case MessageType.Error:
            target = Console.Error;
            builder.Append("Error");
            Console.ForegroundColor = ConsoleColor.Red;
            break;
        }

        builder.Append("]: ");
        builder.Append(message);


        target.WriteLine(builder.ToString());

        Console.ResetColor();
    }
}
