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

                var remoteSync = new RemoteSync(
                    new RemoteSyncOptions
                    {
                        Host = args[0],
                        Username = args[1],
                        Password = args[2],
                        LocalRootDirectory = args[3],
                        RemoteRootDirectory = args[4],
                        SearchPatterns = args[5].Split(';', StringSplitOptions.RemoveEmptyEntries)
                            .Select(pattern => pattern.Trim())
                            .Where(pattern => pattern.Length > 0)
                            .ToArray()
                    },
                    director);

                director.Start();

                Logger.LogInfo("Starting initial sync...");

                await remoteSync.InitialSyncAsync();

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

                try
                {
                    remoteSync.Dispose();
                }
                catch { }
            }
        }
    }
}
