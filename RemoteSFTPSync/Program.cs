
using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteSFTPSync
{
    class Program
    {
        static async Task Main(string[] args)
        {
            if (args.Length != 6)
            {
                Console.WriteLine("usage: RemoteSFTPSyncCore host username password localRootDir remoteRootDir searchPattern");
            }
            else
            {
                var director = new SyncDirector(args[3]);
                List<RemoteSync> remoteSyncs = new List<RemoteSync>();
                foreach (var split in args[5].Split(';', StringSplitOptions.RemoveEmptyEntries))
                {
                    if (remoteSyncs.Count > 0)
                    {
                        await remoteSyncs[0].DoneMakingFolders;
                    }
                    remoteSyncs.Add(new RemoteSync(args[0], args[1], args[2], args[3], args[4], split, remoteSyncs.Count == 0, director));
                }

                Console.Write("Remote SFTP file sync. Press Ctrl+C to exit: ");
                while (true)
                {
                    var key = Console.ReadKey(intercept: true);
                    if (key.Modifiers == ConsoleModifiers.Control && key.Key == ConsoleKey.C)
                    {
                        Console.WriteLine();
                        break;
                    }
                }

                foreach (var remoteSync in remoteSyncs)
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
