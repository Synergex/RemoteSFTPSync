using System.Diagnostics;

namespace SFTPSyncStop
{
    internal class SFTPSyncStop
    {
        static void Main(string[] args)
        {
            var processes = Process.GetProcessesByName("SFTPSync");
            foreach (var p in processes)
            {
                try
                {
                    // attempt graceful shutdown
                    p.CloseMainWindow();
                    if (!p.WaitForExit(5000))
                    {
                        // fallback
                        p.Kill();
                    }
                }
                catch { /* swallow errors */ }
            }
        }
    }
}
