using System.Diagnostics;

namespace KoraGame
{
    public enum LogFilter
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
        public enum LogSeverity
        {
            Info = 1,
            Warning,
            Error,
            Exception,
        }

        public interface ILogger : IDisposable
        {
            void Log(LogSeverity severity, LogFilter filter, object sender, string message, string stackTrace);
        }

        // Private
        private static List<ILogger> loggers = new();
        private static object syncLock = new object();

        // Public
#if DEBUG
        public const bool IsDebug = true;
#else
        public const bool IsDebug = false;
#endif

        // Properties
        public static IList<ILogger> Loggers => loggers;

        // Constructor
        static Debug()
        {
            // Add console logger
            if (IsDebug == true || Environment.UserInteractive == true)
                loggers.Add(new ConsoleLogger());
        }

        // Methods
        public static void Log(string message, LogFilter filter = 0, object sender = null)
        {
            Log(LogSeverity.Info, filter, sender, message);
        }

        public static void LogWarning(string message, LogFilter filter = 0, object sender = null)
        {
            Log(LogSeverity.Warning, filter, sender, message);
        }

        public static void LogError(string message, LogFilter filter = 0, object sender = null)
        {
            Log(LogSeverity.Error, filter, sender, message);
        }

        public static void LogException(Exception e, LogFilter filter = 0, object sender = null)
        {
            Log(LogSeverity.Exception, filter, sender, e.Message, e.StackTrace);
        }

        internal static void Terminate()
        {
            foreach (ILogger logger in loggers)
                logger.Dispose();
        }

        private static void Log(LogSeverity severity, LogFilter filter, object sender, string message, string stackTrace = null)
        {
            if (stackTrace == null)
            {
                // Get the stack trace
                StackTrace trace = new StackTrace(2);

                // Get the full string
                stackTrace = trace.ToString();
            }

            lock (syncLock)
            {
                // Report the message
                foreach (ILogger logger in loggers)
                {
                    // Send to logger
                    logger.Log(severity, filter, sender, message, stackTrace);
                }
            }
        }

        #region Loggers
        private sealed class ConsoleLogger : ILogger
        {
            // Private
            private const int padSeverityLength = 12;
            private const int padFilterLength = 4;
            private static readonly Dictionary<LogFilter, ConsoleColor> filterColors = new()
            {
                { LogFilter.Graphics, ConsoleColor.Green },
                { LogFilter.Assets, ConsoleColor.Cyan },
                { LogFilter.Input, ConsoleColor.Magenta },
                { LogFilter.Physics, ConsoleColor.DarkCyan },
                { LogFilter.Audio, ConsoleColor.DarkGreen },
                { LogFilter.Network, ConsoleColor.Blue },
                { LogFilter.Game, ConsoleColor.DarkBlue },
                { LogFilter.Script, ConsoleColor.DarkMagenta },
                { LogFilter.Editor, ConsoleColor.DarkYellow },
            };

            // Methods
            public void Log(LogSeverity severity, LogFilter filter, object sender, string message, string stackTrace)
            {
                ConsoleColor color = ConsoleColor.White;
                string prefix = "INFO: ";

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
                    string filterText = filter.ToString();

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
}
