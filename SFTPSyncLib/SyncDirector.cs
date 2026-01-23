
using System.Text.RegularExpressions;

namespace SFTPSyncLib
{
    public class SyncDirector
    {
        FileSystemWatcher _fsw;
        List<(Regex, Action<FileSystemEventArgs>)> callbacks = new List<(Regex, Action<FileSystemEventArgs>)>();

        public SyncDirector(string rootFolder)
        {
            _fsw = new FileSystemWatcher(rootFolder, "*.*")
            {
                IncludeSubdirectories = true,
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName,
                // Max allowed size; helps reduce missed events during bursts.
                InternalBufferSize = 64 * 1024
            };

            _fsw.Created += Fsw_Created;
            _fsw.Changed += Fsw_Changed;
            _fsw.Renamed += Fsw_Renamed;
            _fsw.Error += Fsw_Error;

            _fsw.EnableRaisingEvents = true;
        }

        public void AddCallback(string match, Action<FileSystemEventArgs> handler)
        {
            string regexPattern = "^" + Regex.Escape(match)
                .Replace("\\*", ".*")
                .Replace("\\?", ".") + "$";
            callbacks.Add((new Regex(regexPattern, RegexOptions.IgnoreCase), handler));
        }

        private void Fsw_Created(object sender, FileSystemEventArgs e)
        {
            var name = Path.GetFileName(e.FullPath);
            foreach (var (regex, callback) in callbacks)
            {
                if (regex.IsMatch(name))
                {
                    callback(e);
                }
            }
        }

        private void Fsw_Changed(object sender, FileSystemEventArgs e)
        {
            var name = Path.GetFileName(e.FullPath);
            foreach (var (regex, callback) in callbacks)
            {
                if (regex.IsMatch(name))
                {
                    callback(e);
                }
            }
        }

        private void Fsw_Renamed(object sender, RenamedEventArgs e)
        {
            var name = Path.GetFileName(e.FullPath);
            foreach (var (regex, callback) in callbacks)
            {
                if (regex.IsMatch(name))
                {
                    callback(e);
                }
            }
        }

        private void Fsw_Error(object sender, ErrorEventArgs e)
        {
            Logger.LogError($"FileSystemWatcher error: {e.GetException()?.Message}");
        }
    }
}
