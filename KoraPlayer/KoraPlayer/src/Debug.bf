using System;
using System.Collections;
using System.Diagnostics;

namespace KoraPlayer;

public enum LogSeverity : uint32
{
    Info = 1,
    Warning,
    Error,
    Exception,
}

public enum LogFilter : uint32
{
    Graphics = 1,
    Assets,
    Input,
    Physics,
    Audio,
    Network,
    Game,
    Script,

    Editor = 31,
}

public static class Debug
{
	// Type
	public interface ILogger : IDisposable
	{
		// Methods
	    void Log(LogSeverity severity, LogFilter filter, Object sender, String message, String stackTrace);
	}

	// Private
	private static List<ILogger> loggers = new .() ~delete _;

	// Public
	#if DEBUG
	public const bool IsDebug = true;
#else
	public const bool IsDebug = false;
#endif

	// Constructor
	static this()
	{
		// Add console logger
		if (IsDebug == true)
		    loggers.Add(new ConsoleLogger());
	}

	// Methods
	public static void Log(String message, LogFilter filter = 0, Object sender = null)
	{
	    Log(LogSeverity.Info, filter, sender, message);
	}

	public static void LogWarning(String message, LogFilter filter = 0, Object sender = null)
	{
	    Log(LogSeverity.Warning, filter, sender, message);
	}

	public static void LogError(String message, LogFilter filter = 0, Object sender = null)
	{
	    Log(LogSeverity.Error, filter, sender, message);
	}

	internal static void Terminate()
	{
	    for (ILogger logger in loggers)
	        logger.Dispose();
	}

	private static void Log(LogSeverity severity, LogFilter filter, Object sender, String message, String stackTrace = null)
	{
        // Report the message
        for (ILogger logger in loggers)
        {
            // Send to logger
            logger.Log(severity, filter, sender, message, stackTrace);
        }
	}

#region Loggers
	private sealed class ConsoleLogger : ILogger
	{
	    // Private
	    private const int padSeverityLength = 12;
	    private const int padFilterLength = 4;
	    private static readonly Dictionary<LogFilter, ConsoleColor> filterColors = new .()
	    {
	        ( LogFilter.Graphics, ConsoleColor.Green ),
	        ( LogFilter.Assets, ConsoleColor.Cyan ),
	        ( LogFilter.Input, ConsoleColor.Magenta ),
	        ( LogFilter.Physics, ConsoleColor.DarkCyan ),
	        ( LogFilter.Audio, ConsoleColor.DarkGreen ),
	        ( LogFilter.Network, ConsoleColor.Blue ),
	        ( LogFilter.Game, ConsoleColor.DarkBlue ),
	        ( LogFilter.Script, ConsoleColor.DarkMagenta ),
	        ( LogFilter.Editor, ConsoleColor.DarkYellow ),
	    };

	    // Methods
	    public void Log(LogSeverity severity, LogFilter filter, Object sender, String message, String stackTrace)
	    {
	        ConsoleColor color = ConsoleColor.White;
	        String prefix = "INFO: ";

	        switch (severity)
	        {
	            case LogSeverity.Warning:
	                {
	                    color = ConsoleColor.DarkYellow;
	                    prefix = "WARNING: ";
	                    break;
	                }

	            case LogSeverity.Error:
	            case LogSeverity.Exception:
	                {
	                    color = ConsoleColor.Red;
	                    prefix = "ERROR: ";
	                    break;
	                }
				default:
	        }

	        // Change color
	        Console.ForegroundColor = color;

	        // Write message
	        Console.Write(prefix);

	        // Apply some padding
	        for (int i = prefix.Length + 2; i < padSeverityLength; i++)
	            Console.Write(' ');

	        if (filter != 0)
	        {
	            String filterText = scope .();
				filter.ToString(filterText);

	            // Get the filter color
	            Console.ForegroundColor = filterColors[filter];
	            Console.Write("[");
	            Console.Write(filterText);
	            Console.Write("]: ");

	            // Apply some padding
	            for (int i = filterText.Length + 2; i < padFilterLength; i++)
	                Console.Write(' ');

	            Console.Write('\t');

	            // Revert back to original message color
	            Console.ForegroundColor = color;
	        }

	        if (sender != null)
	        {
	            Console.Write("(");
	            Console.Write(sender);
	            Console.Write("): ");
	        }

	        Console.WriteLine(message);

	        // Check for exception
	        if(severity == LogSeverity.Exception)
	        {
	            Console.WriteLine(stackTrace);
	        }

	    }

	    public void Dispose() { }            
	}
#endregion
}