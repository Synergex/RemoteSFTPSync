
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

        public static void Log(string message)
        {
            switch (_mode)
            {
                case LoggerMode.Console:
                    Console.WriteLine(message);
                    break;

                case LoggerMode.RaiseEvent:
                    LogUpdated?.Invoke(message);
                    break;
            }
        }
    }
}
