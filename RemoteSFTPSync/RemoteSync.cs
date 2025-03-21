
using Renci.SshNet;
using Renci.SshNet.Sftp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace RemoteSFTPSync
{
    class SyncDirector
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

    class RemoteSync : IDisposable
    {
        string _host;
        string _username;
        string _password;
        string _searchPattern;
        string _localRootDirectory;
        string _remoteRootDirectory;
        SftpClient _sftp;
        SyncDirector _director;
        HashSet<string> _activeDirSync = new HashSet<string>();

        public Task DoneMakingFolders { get; }

        public RemoteSync(string host, string username, string password, string localRootDirectory, string remoteRootDirectory, string searchPattern, bool createFolders, SyncDirector director)
        {
            _host = host;
            _username = username;
            _password = password;
            _searchPattern = searchPattern;
            _localRootDirectory = localRootDirectory;
            _remoteRootDirectory = remoteRootDirectory;
            _director = director;
            _sftp = new SftpClient(host, username, password);
            _sftp.Connect();
            DoneMakingFolders = createFolders ? CreateDirectories(_localRootDirectory, _remoteRootDirectory) : Task.CompletedTask;
            var tsk = InitialSync(_localRootDirectory, _remoteRootDirectory);
            
            tsk.ContinueWith((tmp) =>
            {
                _director.AddCallback(searchPattern, (args) => Fsw_Changed(null, args));
            });
        }

        public static void SyncFile(SftpClient sftp, string sourcePath, string destinationPath)
        {
            sftp.WriteAllText(sourcePath, destinationPath);
        }

        public static Task<IEnumerable<FileInfo>> SyncDirectoryAsync(SftpClient sftp, string sourcePath, string destinationPath, string searchPattern)
        {
            if (new DirectoryInfo(sourcePath).EnumerateFiles(searchPattern, SearchOption.TopDirectoryOnly).Any())
            {
                Console.WriteLine($"Sync directory started {sourcePath} -> {destinationPath} with search pattern {searchPattern}");
                return Task<IEnumerable<FileInfo>>.Factory.FromAsync(sftp.BeginSynchronizeDirectories,
                                                   sftp.EndSynchronizeDirectories, sourcePath,
                                                   destinationPath, searchPattern, null);
            }
            else
            {
                return Task.FromResult(Enumerable.Empty<FileInfo>());
            }
        }

        private string[] FilteredDirectories(string localPath)
        {
            return Directory.GetDirectories(localPath).Where(
                path =>
                {
                    var relativePath = path.Substring(_localRootDirectory.Length);
                    return !relativePath.Contains(".git") && !relativePath.EndsWith("obj") &&
                    !relativePath.EndsWith("bin") && !relativePath.Contains(".");
                }).ToArray();
        }

        public async Task CreateDirectories(string localPath, string remotePath)
        {
            try
            {
                var localDirectories = FilteredDirectories(localPath);
                var remoteDirectories = (await ListDirectoryAsync(_sftp, remotePath)).Where(item => item.IsDirectory).ToDictionary(item =>
                {
                    if (item.Name.Contains(".DIR", StringComparison.OrdinalIgnoreCase))
                        return item.Name.Remove(item.Name.IndexOf(".DIR", StringComparison.OrdinalIgnoreCase));
                    else
                        return item.Name;
                });
                foreach (var item in localDirectories)
                {
                    var directoryName = item.Split(Path.DirectorySeparatorChar).Last();
                    if (!remoteDirectories.ContainsKey(directoryName))
                    {
                        _sftp.CreateDirectory(remotePath + "/" + directoryName);
                    }
                    await CreateDirectories(localPath + "\\" + directoryName, remotePath + "/" + directoryName);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed while creating directories, did you have the initial root folder on the remote system?");
                Environment.Exit(-1);
            }
        }

        public async Task InitialSync(string localPath, string remotePath)
        {
            await DoneMakingFolders;
            var localDirectories = FilteredDirectories(localPath);
            var remoteDirectories = (await ListDirectoryAsync(_sftp, remotePath)).Where(item => item.IsDirectory).ToDictionary(item =>
            {
                if (item.Name.Contains(".DIR", StringComparison.OrdinalIgnoreCase))
                    return item.Name.Remove(item.Name.IndexOf(".DIR", StringComparison.OrdinalIgnoreCase));
                else
                    return item.Name;
                });
            foreach (var item in localDirectories)
            {
                var directoryName = item.Split(Path.DirectorySeparatorChar).Last();
                await InitialSync(localPath + "\\" + directoryName, remotePath + "/" + directoryName);
            }
            
            await SyncDirectoryAsync(_sftp, localPath, remotePath, _searchPattern);
        }


        public static Task UploadFileAsync(SftpClient sftp, Stream file, string destination)
        {
            Func<Stream, string, AsyncCallback, object, IAsyncResult> begin = (stream, path, callback, state) => sftp.BeginUploadFile(stream, path, true, callback, state, null);
            return Task.Factory.FromAsync(begin, sftp.EndUploadFile, file, destination, null);
        }


        public static Task<IEnumerable<ISftpFile>> ListDirectoryAsync(SftpClient sftp, string path)
        {
            Func<string, AsyncCallback, object, IAsyncResult> begin = (bpath, callback, state) => sftp.BeginListDirectory(bpath, callback, state, null);
            return Task.Factory.FromAsync(begin, sftp.EndListDirectory, path, null);
        }


        public static bool IsFileReady(String sFilename)
        {
            // If the file can be opened for exclusive access it means that the file
            // is no longer locked by another process.
            try
            {
                using (FileStream inputStream = File.Open(sFilename, FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    if (inputStream.Length > 0)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }

                }
            }
            catch (Exception)
            {
                return false;
            }
        }


        private async void Fsw_Changed(object sender, FileSystemEventArgs arg)
        {
            if (arg.ChangeType == WatcherChangeTypes.Changed || arg.ChangeType == WatcherChangeTypes.Created
                || arg.ChangeType == WatcherChangeTypes.Renamed)
            {
                var changedPath = Path.GetDirectoryName(arg.FullPath);
                var relativePath = _localRootDirectory == changedPath ? "" : changedPath.Substring(_localRootDirectory.Length).Replace('\\', '/');
                var fullRemotePath = _remoteRootDirectory + relativePath;
                await Task.Yield();
                bool makeDirectory = true;
                lock (_activeDirSync)
                {
                    if (_activeDirSync.Contains(changedPath))
                        makeDirectory = false;
                    else
                        _activeDirSync.Add(changedPath);
                }

                //check if we're a new directory
                if (makeDirectory && Directory.Exists(arg.FullPath) && !_sftp.Exists(arg.FullPath))
                {
                    _sftp.CreateDirectory(fullRemotePath);
                }

                if (makeDirectory)
                {
                    lock (_activeDirSync)
                    {
                        _activeDirSync.Remove(changedPath);
                    }
                }
                
                while (!IsFileReady(arg.FullPath))
                    await Task.Delay(50);


                lock (_activeDirSync)
                {
                    if (_activeDirSync.Contains(arg.FullPath))
                        return;
                    else
                        _activeDirSync.Add(arg.FullPath);
                }
                SyncFile(_sftp, arg.FullPath, arg.FullPath.Substring(_localRootDirectory.Length).Replace('\\', '/'));

                lock (_activeDirSync)
                {
                    _activeDirSync.Remove(arg.FullPath);
                }
            }
        }

        public void Dispose()
        {
            if (_sftp != null)
            {
                _sftp.Dispose();
            }
            _sftp = null;

           
        }
    }
}
