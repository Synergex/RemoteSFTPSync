
namespace SFTPSyncLib
{
    public enum LoggerMode
    {
        Console,
        RaiseEvent
    }

    public static class Logger
    {
        static LoggerMode _mode;
        static Action<string>? _logUpdated;

        static Logger()
        {
            _mode = LoggerMode.Console;
        }

        public static Action<string>? LogUpdated 
        { 
            get => _logUpdated;
            set
            {
                if (value != null)
                {
                    _mode = LoggerMode.RaiseEvent;
                    _logUpdated = value;
                }
                else
                {
                    _mode = LoggerMode.Console;
                    _logUpdated = null;
                }
            }
        }

        private static void Log(string message)
        {
            switch (_mode)
            {
                case LoggerMode.Console:
                    Console.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} {message}");
                    break;

                case LoggerMode.RaiseEvent:
                    LogUpdated?.Invoke($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} {message}");
                    break;
            }
        }
        public static void LogInfo(string message)
        {
            Log($"INF: {message}");
        }

        public static void LogWarnig(string message)
        {
            Log($"WRN: {message}");
        }

        public static void LogError(string message)
        {
            Log($"ERR: {message}");
        }
    }
}
