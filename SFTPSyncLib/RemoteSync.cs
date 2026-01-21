using Renci.SshNet;
using Renci.SshNet.Sftp;

namespace SFTPSyncLib
{
    public class RemoteSync : IDisposable
    {
        string _host;
        string _username;
        string _password;
        string _searchPattern;
        string _localRootDirectory;
        string _remoteRootDirectory;
        readonly List<string>? _excludedFolders;

        SftpClient _sftp;
        SyncDirector _director;
        HashSet<string> _activeDirSync = new HashSet<string>();
        readonly SemaphoreSlim _sftpLock = new SemaphoreSlim(1, 1);
        bool _disposed;

        
        public Task DoneMakingFolders { get; }

        public Task DoneInitialSync { get; }

        public RemoteSync(string host, string username, string password,
            string localRootDirectory, string remoteRootDirectory, 
            string searchPattern, bool createFolders, SyncDirector director, List<string>? excludedFolders)
        {
            _host = host;
            _username = username;
            _password = password;
            _searchPattern = searchPattern;
            _localRootDirectory = localRootDirectory;
            _remoteRootDirectory = remoteRootDirectory;
            _director = director;
            _excludedFolders = excludedFolders ?? new List<string>();
            _sftp = new SftpClient(host, username, password);
            _sftp.Connect();

            //Our first instance is responsible for creating all of the the directories
            //Subsequent instances will not be created until this is done
            DoneMakingFolders = createFolders ? CreateDirectories(_localRootDirectory, _remoteRootDirectory) : Task.CompletedTask;

            //Now perform the initial sync for the pattern this instance is responsible for
            DoneInitialSync = InitialSync(_localRootDirectory, _remoteRootDirectory);

            //Once the initial sync is done, we can start watching the file system for changes
            DoneInitialSync.ContinueWith((tmp) =>
            {
                _director.AddCallback(searchPattern, (args) => Fsw_Changed(null, args));
            });
        }

        /// <summary>
        /// Sync changes for a file. This is only used for changes AFTER the initial sync has completed.
        /// </summary>
        /// <param name="sftp">SFTP client</param>
        /// <param name="sourcePath">Path to the source file</param>
        /// <param name="destinationPath">Path to the destination file</param>
        public static void SyncFile(SftpClient sftp, string sourcePath, string destinationPath)
        {
            Logger.LogInfo($"Syncing {sourcePath} -> {destinationPath}");

            int retryCount = 0;
            const int maxRetries = 5;
            const int delayBetweenRetriesMs = 500;

            while (retryCount < maxRetries)
            {
                try
                {
                    // Because the VMS SFTP server uses Posix not RMS, and some older servers do not correctly
                    // handle truncating files when the content gets shorter (resulting in corruption at the end of the file),
                    // we delete the file first if it exists, before writing new content.
                    if (sftp.Exists(destinationPath))
                    {
                        sftp.DeleteFile(destinationPath);
                    }

                    // Read the local file content
                    var localFileContent = File.ReadAllText(sourcePath);

                    // Write the remote file
                    sftp.WriteAllText(destinationPath, localFileContent);

                    return;
                }
                catch (Exception ex)
                {
                    retryCount++;
                    if (retryCount >= maxRetries)
                    {
                        Logger.LogError($"Failed to sync after {maxRetries} retries. Exception: {ex.Message}");
                        return;
                    }

                    Logger.LogInfo($"Retry {retryCount}/{maxRetries} after exception: {ex.Message}");
                    Thread.Sleep(delayBetweenRetriesMs);
                }
            }
        }

        public static Task<IEnumerable<FileInfo>> SyncDirectoryAsync(SftpClient sftp, string sourcePath, string destinationPath, string searchPattern)
        {
            if (new DirectoryInfo(sourcePath).EnumerateFiles(searchPattern, SearchOption.TopDirectoryOnly).Any())
            {
                Logger.LogInfo($"Sync started for {sourcePath}\\{searchPattern} -> {destinationPath}");

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
            return Directory.GetDirectories(localPath).Where(path =>
            {
                var relativePath = path.Substring(_localRootDirectory.Length);

                // Existing exclusions
                bool isExcluded = relativePath.EndsWith(".git")
                                  || relativePath.EndsWith(".vs")
                                  || relativePath.EndsWith("bin")
                                  || relativePath.EndsWith("obj")
                                  || relativePath.Contains(".");

                // Check _excludedFolders
                if (!isExcluded && _excludedFolders != null && _excludedFolders.Count > 0)
                {
                    string fullPath = Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar);
                    isExcluded = _excludedFolders.Any(excluded =>
                    {
                        var excludedFullPath = Path.GetFullPath(excluded).TrimEnd(Path.DirectorySeparatorChar);
                        return string.Equals(fullPath, excludedFullPath, StringComparison.OrdinalIgnoreCase);
                    });
                }

                return !isExcluded;
            }).ToArray();
        }

        public async Task CreateDirectories(string localPath, string remotePath)
        {
            Logger.LogInfo($"Creating directory {localPath} -> {remotePath}");

            try
            {
                if (!EnsureConnectedSafe())
                    return;

                //Got local directories to sync
                var localDirectories = FilteredDirectories(localPath);

                //Get remote directories
                var remoteDirectories = (await ListDirectoryAsync(_sftp, remotePath)).Where(item => item.IsDirectory).ToDictionary(item =>
                {
                    if (item.Name.Contains(".DIR", StringComparison.OrdinalIgnoreCase))
                        return item.Name.Remove(item.Name.IndexOf(".DIR", StringComparison.OrdinalIgnoreCase));
                    else
                        return item.Name;
                });

                //Compare local and remote directories, creating missing ones, and recurse for subdirectories
                foreach (var item in localDirectories)
                {
                    var directoryName = item.Split(Path.DirectorySeparatorChar).Last();
                    if (!remoteDirectories.ContainsKey(directoryName))
                    {
                        //Create new remote directory
                        _sftp.CreateDirectory(remotePath + "/" + directoryName);
                    }
                    //And recurse for any subdirectories it may need
                    await CreateDirectories(localPath + "\\" + directoryName, remotePath + "/" + directoryName);
                }
            }
            catch (Exception)
            {
                Logger.LogError("Failed to create directories. Check the remote root directory exists.");

                Environment.Exit(-1);
            }
        }

        /// <summary>
        /// For the pattern we are responsible for, sync the files in the local directory to the remote directory.
        /// </summary>
        /// <param name="localPath">Local directory path</param>
        /// <param name="remotePath">Remote directory path</param>
        /// <returns></returns>
        public async Task InitialSync(string localPath, string remotePath)
        {
            //Wait for the folders to be created before starting the initial sync
            await DoneMakingFolders;

            if (!EnsureConnectedSafe())
                return;

            //Get the local directories to sync
            var localDirectories = FilteredDirectories(localPath);

            //Get the remote directories to sync, removing the .DIR suffix if it exists
            var remoteDirectories = (await ListDirectoryAsync(_sftp, remotePath)).Where(item => item.IsDirectory).ToDictionary(item =>
            {
                if (item.Name.Contains(".DIR", StringComparison.OrdinalIgnoreCase))
                    return item.Name.Remove(item.Name.IndexOf(".DIR", StringComparison.OrdinalIgnoreCase));
                else
                    return item.Name;
            });

            //
            foreach (var item in localDirectories)
            {
                var directoryName = item.Split(Path.DirectorySeparatorChar).Last();
                await InitialSync(localPath + "\\" + directoryName, remotePath + "/" + directoryName);
            }

            await SyncDirectoryAsync(_sftp, localPath, remotePath, _searchPattern);
        }


        public static Task UploadFileAsync(SftpClient sftp, Stream file, string destination)
        {
            Func<Stream, string, AsyncCallback, object?, IAsyncResult> begin = (stream, path, callback, state) => sftp.BeginUploadFile(stream, path, true, callback, state, null);
            return Task.Factory.FromAsync(begin, sftp.EndUploadFile, file, destination, null);
        }


        public static Task<IEnumerable<ISftpFile>> ListDirectoryAsync(SftpClient sftp, string path)
        {
            Func<string, AsyncCallback, object?, IAsyncResult> begin = (bpath, callback, state) => sftp.BeginListDirectory(bpath, callback, state, null);
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

        private void EnsureConnected()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(RemoteSync));

            if (_sftp.IsConnected)
                return;

            _sftp.Connect();
        }

        private bool EnsureConnectedSafe()
        {
            try
            {
                EnsureConnected();
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogError($"SFTP connection error: {ex.Message}");
                return false;
            }
        }


        private async void Fsw_Changed(object? sender, FileSystemEventArgs arg)
        {
            try
            {
                if (arg.ChangeType == WatcherChangeTypes.Changed || arg.ChangeType == WatcherChangeTypes.Created
                    || arg.ChangeType == WatcherChangeTypes.Renamed)
                {
                    var changedPath = Path.GetDirectoryName(arg.FullPath);
                    var relativePath = _localRootDirectory == changedPath ? "" : changedPath?.Substring(_localRootDirectory.Length).Replace('\\', '/');
                    var fullRemotePath = _remoteRootDirectory + relativePath;
                    await Task.Yield();
                    bool makeDirectory = true;
                    lock (_activeDirSync)
                    {
                        if (changedPath == null)
                            return;
                        if (_activeDirSync.Contains(changedPath))
                            makeDirectory = false;
                        else
                            _activeDirSync.Add(changedPath);
                    }

                    bool connectionOk;
                    await _sftpLock.WaitAsync();
                    try
                    {
                        connectionOk = EnsureConnectedSafe();
                        if (connectionOk)
                        {
                            //check if we're a new directory
                            if (makeDirectory && Directory.Exists(arg.FullPath) && !_sftp.Exists(fullRemotePath))
                            {
                                _sftp.CreateDirectory(fullRemotePath);
                            }
                        }
                    }
                    finally
                    {
                        _sftpLock.Release();
                    }

                    if (!connectionOk)
                    {
                        if (makeDirectory)
                        {
                            lock (_activeDirSync)
                            {
                                _activeDirSync.Remove(changedPath);
                            }
                        }
                        return;
                    }

                    if (makeDirectory)
                    {
                        lock (_activeDirSync)
                        {
                            _activeDirSync.Remove(changedPath);
                        }
                    }

                    if (Directory.Exists(arg.FullPath))
                        return;

                    var waitStart = DateTime.UtcNow;
                    while (!IsFileReady(arg.FullPath))
                    {
                        if (!File.Exists(arg.FullPath))
                            return;
                        if (DateTime.UtcNow - waitStart > TimeSpan.FromSeconds(30))
                        {
                            Logger.LogWarnig($"Timed out waiting for file to be ready: {arg.FullPath}");
                            return;
                        }
                        await Task.Delay(25);
                    }

                    lock (_activeDirSync)
                    {
                        if (_activeDirSync.Contains(arg.FullPath))
                            return;
                        else
                            _activeDirSync.Add(arg.FullPath);
                    }

                    bool fileConnectionOk;
                    await _sftpLock.WaitAsync();
                    try
                    {
                        fileConnectionOk = EnsureConnectedSafe();
                        if (fileConnectionOk)
                        {
                            SyncFile(_sftp, arg.FullPath, fullRemotePath + "/" + Path.GetFileName(arg.FullPath));
                        }
                    }
                    finally
                    {
                        _sftpLock.Release();
                        lock (_activeDirSync)
                        {
                            _activeDirSync.Remove(arg.FullPath);
                        }
                    }

                    if (!fileConnectionOk)
                        return;
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"Unhandled exception in file sync handler: {ex.Message}");
            }
        }

        public void Dispose()
        {
            if (_sftp != null)
            {
                _disposed = true;
                _sftp.Dispose();
            }
        }
    }
}
