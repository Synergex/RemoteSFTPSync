using Renci.SshNet;
using Renci.SshNet.Sftp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RemoteSFTPSync
{
    class RemoteSync : IDisposable
    {
        string _host;
        string _username;
        string _password;
        string _searchPattern;
        string _localRootDirectory;
        string _remoteRootDirectory;
        SftpClient _sftp;
        FileSystemWatcher _fsw;
        HashSet<string> _activeDirSync = new HashSet<string>();

        public RemoteSync(string host, string username, string password, string localRootDirectory, string remoteRootDirectory, string searchPattern)
        {
            _host = host;
            _username = username;
            _password = password;
            _searchPattern = searchPattern;
            _localRootDirectory = localRootDirectory;
            _remoteRootDirectory = remoteRootDirectory;

            _sftp = new SftpClient(host, username, password);
            _sftp.Connect();
            var tsk = InitialSync(_localRootDirectory, _remoteRootDirectory);

            tsk.ContinueWith((tmp) =>
            {
                _fsw = new FileSystemWatcher(localRootDirectory, searchPattern);
                _fsw.IncludeSubdirectories = true;
                _fsw.NotifyFilter = NotifyFilters.LastWrite;
                _fsw.Changed += Fsw_Changed;
                _fsw.EnableRaisingEvents = true;
            });
        }

        
        public static Task<IEnumerable<FileInfo>> SyncDirectoryAsync(SftpClient sftp, string sourcePath, string destinationPath, string searchPattern)
        {
            return Task<IEnumerable<FileInfo>>.Factory.FromAsync(sftp.BeginSynchronizeDirectories,
                                               sftp.EndSynchronizeDirectories, sourcePath,
                                               destinationPath, searchPattern, null);
        }

        public async Task InitialSync(string localPath, string remotePath)
        {
            var localDirectories = Directory.GetDirectories(localPath);
            var remoteDirectories = (await ListDirectoryAsync(_sftp, remotePath)).Where(item => item.IsDirectory).ToDictionary(item => item.Name.Remove(item.Name.IndexOf(".DIR")));
            foreach (var item in localDirectories)
            {
                var directoryName = item.Split(Path.DirectorySeparatorChar).Last();
                if (!remoteDirectories.ContainsKey(directoryName))
                {
                    _sftp.CreateDirectory(remotePath + "/" + directoryName);
                }
                await InitialSync(localPath + "\\" + directoryName, remotePath + "/" + directoryName);
            }
            await SyncDirectoryAsync(_sftp, localPath, remotePath, _searchPattern);
        }

        public static Task UploadFileAsync(SftpClient sftp, Stream file, string destination)
        {
            Func<Stream, string, AsyncCallback, object, IAsyncResult> begin = (stream, path, callback, state) => sftp.BeginUploadFile(stream, path, true, callback, state, null);
            return Task.Factory.FromAsync(begin, sftp.EndUploadFile, file, destination, null);
        }

        public static Task<IEnumerable<SftpFile>> ListDirectoryAsync(SftpClient sftp, string path)
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
            if (arg.ChangeType == WatcherChangeTypes.Changed || arg.ChangeType == WatcherChangeTypes.Created)
            {
                var changedPath = Path.GetDirectoryName(arg.FullPath);
                lock (_activeDirSync)
                {
                    if (_activeDirSync.Contains(changedPath))
                        return;
                    else
                        _activeDirSync.Add(changedPath);
                }
                while (!IsFileReady(arg.FullPath))
                    Thread.Sleep(50);

                var relativePath = _localRootDirectory == changedPath ? "" : changedPath.Substring(_localRootDirectory.Length).Replace('\\', '/');
                var fullRemotePath = _remoteRootDirectory + relativePath;

                //check if we're a new directory
                if (Directory.Exists(arg.FullPath))
                {
                    _sftp.CreateDirectory(fullRemotePath);
                }

                await SyncDirectoryAsync(_sftp, changedPath, fullRemotePath, _searchPattern);
                
                lock (_activeDirSync)
                {
                    _activeDirSync.Remove(changedPath);
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

            if (_fsw != null)
            {
                _fsw.Dispose();
            }
            _fsw = null;
        }
    }
}
