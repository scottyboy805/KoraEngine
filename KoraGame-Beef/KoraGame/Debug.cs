using System.Diagnostics;
using System.Runtime.InteropServices;

namespace KoraGame
{
    public enum LogSeverity : uint
    {
        Info = 1,
        Warning,
        Error,
        Exception,
    }

    public enum LogFilter : uint
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

    public static unsafe class Debug
    {
        // Methods
        public static void Log(string message, LogFilter filter = 0, object sender = null)
        {
            using(Utf8String rawMessage = message)
            using(Utf8String rawSender = GetSender(sender))
            using(Utf8String rawStackTrace = GetStackTrace())
            {
                Debug_Log(rawMessage.Ptr, filter, rawSender.Ptr, rawStackTrace.Ptr);
            }
        }

        public static void LogWarning(string message, LogFilter filter = 0, object sender = null)
        {
            using (Utf8String rawMessage = message)
            using (Utf8String rawSender = GetSender(sender))
            using (Utf8String rawStackTrace = GetStackTrace())
            {
                Debug_LogWarning(rawMessage.Ptr, filter, rawSender.Ptr, rawStackTrace.Ptr);
            }
        }

        public static void LogError(string message, LogFilter filter = 0, object sender = null)
        {
            using (Utf8String rawMessage = message)
            using (Utf8String rawSender = GetSender(sender))
            using (Utf8String rawStackTrace = GetStackTrace())
            {
                Debug_LogError(rawMessage.Ptr, filter, rawSender.Ptr, rawStackTrace.Ptr);
            }
        }

        public static void LogException(Exception exception, LogFilter filter = 0, object sender = null)
        {
            using (Utf8String rawMessage = exception.Message)
            using (Utf8String rawSender = GetSender(sender))
            using (Utf8String rawStackTrace = exception.StackTrace)
            {
                Debug_LogException(rawMessage.Ptr, filter, rawSender.Ptr, rawStackTrace.Ptr);
            }
        }

        private static string GetSender(object sender)
        {
            return sender != null
                ? sender.ToString()
                : null;
        }

        private static string GetStackTrace()
        {
            // Get the stack trace
            StackTrace trace = new StackTrace(2);

            // Get the full string
            return trace.ToString();
        }

        [DllImport("KoraGame")]
        internal static extern void Debug_Log(byte* message, LogFilter filter, byte* sender, byte* stackTrace);

        [DllImport("KoraGame")]
        internal static extern void Debug_LogWarning(byte* message, LogFilter filter, byte* sender, byte* stackTrace);

        [DllImport("KoraGame")]
        internal static extern void Debug_LogError(byte* message, LogFilter filter, byte* sender, byte* stackTrace);

        [DllImport("KoraGame")]
        internal static extern void Debug_LogException(byte* message, LogFilter filter, byte* sender, byte* stackTrace);
    }
}
