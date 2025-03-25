
using System.Text.RegularExpressions;

namespace SFTPSync
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
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName
            };

            _fsw.Changed += Fsw_Changed;
            _fsw.Created += Fsw_Created;
            _fsw.Renamed += Fsw_Renamed;

            _fsw.EnableRaisingEvents = true;
        }

        private void Fsw_Renamed(object sender, RenamedEventArgs e)
        {
            foreach (var (regex, callback) in callbacks)
            {
                if (regex.IsMatch(e.FullPath))
                {
                    callback(e);
                }
            }
        }

        private void Fsw_Created(object sender, FileSystemEventArgs e)
        {
            foreach (var (regex, callback) in callbacks)
            {
                if (regex.IsMatch(e.FullPath))
                {
                    callback(e);
                }
            }
        }

        public void AddCallback(string match, Action<FileSystemEventArgs> handler)
        {
            string regexPattern = "^" + Regex.Escape(match).Replace("\\*", ".*") + "$";
            callbacks.Add((new Regex(regexPattern), handler));
        }

        private void Fsw_Changed(object sender, FileSystemEventArgs e)
        {
            foreach (var (regex, callback) in callbacks)
            {
                if (regex.IsMatch(e.FullPath))
                {
                    callback(e);
                }
            }
        }
    }
}
