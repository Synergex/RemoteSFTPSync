
using SFTPSyncLib;

namespace SFTPSync
{
    class SFTPSync
    {
        static async Task Main(string[] args)
        {
            if (args.Length != 6)
            {
                Console.WriteLine("usage: SFTPSync host username password localRootDir remoteRootDir searchPattern");
            }
            else
            {
                var director = new SyncDirector(args[3]);
                List<RemoteSync> remoteSyncWorkers = new List<RemoteSync>();

                Logger.LogInfo("Starting initial sync...");

                foreach (var pattern in args[5].Split(';', StringSplitOptions.RemoveEmptyEntries))
                {
                    if (remoteSyncWorkers.Count > 0)
                    {
                        await remoteSyncWorkers[0].DoneMakingFolders;
                    }
                    remoteSyncWorkers.Add(new RemoteSync(args[0], args[1], args[2], args[3], args[4], pattern, remoteSyncWorkers.Count == 0, director, null));

                    Logger.LogInfo($"Started sync worker {remoteSyncWorkers.Count} for pattern {pattern}");
                }

                //Wait for all sync workers to finish initial sync then tell the user
                await Task.WhenAll(remoteSyncWorkers.Select(rsw => rsw.DoneInitialSync));

                Logger.LogInfo("Initial sync complete, real-time sync active");

                Console.Write("Press Ctrl+C to exit: ");

                while (true)
                {
                    var key = Console.ReadKey(intercept: true);
                    if (key.Modifiers == ConsoleModifiers.Control && key.Key == ConsoleKey.C)
                    {
                        Console.WriteLine();
                        break;
                    }
                }

                foreach (var remoteSync in remoteSyncWorkers)
                {
                    try
                    {
                        remoteSync.Dispose();
                    }
                    catch { }
                }
            }
        }
    }
}
