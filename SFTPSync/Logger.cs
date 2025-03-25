
using System;
using System.Collections.Concurrent;
using System.Linq;

namespace SFTPSync
{
    public class Logger
    {
        public Action<string>? LogUpdated { get; set; }
             
        public void Log(string message)
        {
            LogUpdated?.Invoke(message);
        }
    }
}
