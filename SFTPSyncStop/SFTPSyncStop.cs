using System.Diagnostics;

namespace SFTPSyncStop
{
    internal class SFTPSyncStop
    {
        static void Main(string[] args)
        {
            // Look for and stop any running instances of the SFTPSyncUI process
            var processes = Process.GetProcessesByName("SFTPSyncUI");
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

            // Look for and stop any running instances of the SFTPSync process
            processes = Process.GetProcessesByName("SFTPSync");
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
