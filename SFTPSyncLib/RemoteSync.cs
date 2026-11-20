using Renci.SshNet;
using Renci.SshNet.Common;
using Renci.SshNet.Sftp;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO.Enumeration;
using System.Text.RegularExpressions;
using System.Threading.Channels;

namespace SFTPSyncLib
{
    /// <summary>
    /// Keeps a local directory tree in sync with a remote one. The initial sync shares its work between a number of
    /// temporary connections, and afterwards changes are uploaded through a small elastic pool of connections, whatever
    /// the number of search patterns.
    /// </summary>
    public sealed class RemoteSync : IDisposable
    {
        private static readonly TimeSpan IdleConnectionTimeout = TimeSpan.FromSeconds(60);

        // How long a newly created empty file is given to be written to before it is synced as an empty file
        private static readonly TimeSpan EmptyFileGracePeriod = TimeSpan.FromSeconds(2);

        // Bounds the number of changed files being prepared (waiting for a connection or for the file to be writable)
        private const int MaxPendingFileTasks = 64;

        private readonly RemoteSyncOptions _options;
        private readonly string[] _searchPatterns;
        private readonly string _localRootDirectory;
        private readonly string _remoteRootDirectory;
        private readonly List<string> _excludedFolders;
        private readonly bool _deleteEnabled;
        private readonly SyncDirector _director;
        private readonly CancellationTokenSource _shutdownCts = new CancellationTokenSource();
        private readonly CancellationToken _token;
        private readonly SftpConnectionPool _pool;
        private readonly object _disposeLock = new object();
        private bool _disposed;

        // Set once the initial sync has completed. File system events before then are ignored.
        private volatile bool _realtimeActive;

        // Changes waiting to be uploaded. A file that changes again before its upload has started is only uploaded once.
        private readonly object _queueLock = new object();
        private readonly Dictionary<string, PendingChange> _pending = new Dictionary<string, PendingChange>(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> _inFlight = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> _blocked = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private readonly Channel<string> _queue = Channel.CreateUnbounded<string>(new UnboundedChannelOptions { SingleReader = true });
        private readonly SemaphoreSlim _pendingFileTasks = new SemaphoreSlim(MaxPendingFileTasks);
        private long _directoryOperationSequence;

        // Remote directories that are known to exist, to avoid checking for them before every upload
        private readonly ConcurrentDictionary<string, byte> _knownRemoteDirectories = new ConcurrentDictionary<string, byte>(StringComparer.OrdinalIgnoreCase);
        private readonly object _directoryCreateLock = new object();

        // VS writes to a temp file (8 alphanum chars + "." + 1-4 alphanum chars + "~") then renames it to the real file.
        // The temp was never synced remotely, so suppress the "not found" cleanup warning for these names.
        private static readonly Regex _vsTempFilePattern = new Regex(@"^[a-zA-Z0-9]{8}\.[a-zA-Z0-9]{1,4}~$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static bool IsVsTempFile(string path) => _vsTempFilePattern.IsMatch(Path.GetFileName(path));

        public RemoteSync(RemoteSyncOptions options, SyncDirector director)
        {
            _options = options;
            _searchPatterns = options.SearchPatterns;
            _localRootDirectory = Path.TrimEndingDirectorySeparator(Path.GetFullPath(options.LocalRootDirectory));
            _remoteRootDirectory = options.RemoteRootDirectory.TrimEnd('/', '\\');
            _excludedFolders = options.ExcludedFolders ?? new List<string>();
            _deleteEnabled = options.DeleteEnabled;
            _director = director;
            _token = _shutdownCts.Token;
            _pool = new SftpConnectionPool(options.Host, options.Username, options.Password,
                options.MaxConnections, IdleConnectionTimeout);

            // Register callbacks immediately; events are ignored until the initial sync has completed.
            foreach (var pattern in _searchPatterns)
            {
                _director.AddCallback(pattern, OnFileEvent);
            }

            if (_deleteEnabled)
            {
                _director.AddDirectoryDeleteCallback(args => OnDirectoryEvent(DirectoryOperation.Delete, args.FullPath, null));
            }
            _director.AddDirectoryCreateCallback(args => OnDirectoryEvent(DirectoryOperation.Create, args.FullPath, null));
            _director.AddDirectoryRenameCallback(args => OnDirectoryEvent(DirectoryOperation.Rename, args.FullPath, args.OldFullPath));
        }

        // ------------------------------------------------------------------------------------------------------------
        // Real-time sync: file system events are queued, then uploaded by whichever pooled connection is free
        // ------------------------------------------------------------------------------------------------------------

        private enum ChangeKind
        {
            Upload,
            Delete,
            DirectoryCreate,
            DirectoryDelete,
            DirectoryRename
        }

        private enum DirectoryOperation
        {
            Create,
            Delete,
            Rename
        }

        private sealed class PendingChange
        {
            public PendingChange(ChangeKind kind, string path, string? oldPath)
            {
                Kind = kind;
                Path = path;
                OldPath = oldPath;
            }

            public ChangeKind Kind { get; private set; }
            public string Path { get; }

            /// <summary>For a rename, the previous local path of the file or directory.</summary>
            public string? OldPath { get; private set; }

            public bool IsDirectoryOperation => Kind >= ChangeKind.DirectoryCreate;

            /// <summary>Fold a newer event for the same file into this one that has not been started yet.</summary>
            public void MergeFrom(PendingChange newer)
            {
                Kind = newer.Kind;
                OldPath = newer.OldPath ?? OldPath;
            }
        }

        private void OnFileEvent(FileSystemEventArgs arg)
        {
            if (!_realtimeActive || _token.IsCancellationRequested)
                return;

            try
            {
                // The whole tree is watched, so events arrive for excluded directories too. Files there are never synced.
                var excluded = IsInExcludedDirectory(arg.FullPath);

                if (arg.ChangeType == WatcherChangeTypes.Deleted)
                {
                    if (_deleteEnabled && !excluded)
                    {
                        Enqueue(arg.FullPath, new PendingChange(ChangeKind.Delete, arg.FullPath, null));
                    }
                }
                else if (arg.ChangeType == WatcherChangeTypes.Changed || arg.ChangeType == WatcherChangeTypes.Created
                    || arg.ChangeType == WatcherChangeTypes.Renamed)
                {
                    var oldPath = (arg as RenamedEventArgs)?.OldFullPath;
                    var oldExcluded = oldPath != null && IsInExcludedDirectory(oldPath);

                    if (excluded)
                    {
                        // Moved into an excluded directory: it is no longer synced, so remove the remote copy
                        if (oldPath != null && !oldExcluded && _deleteEnabled)
                        {
                            Enqueue(oldPath, new PendingChange(ChangeKind.Delete, oldPath, null));
                        }
                    }
                    else
                    {
                        // Moved out of an excluded directory: there is no remote copy of the old name to clean up
                        Enqueue(arg.FullPath, new PendingChange(ChangeKind.Upload, arg.FullPath, oldExcluded ? null : oldPath));
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"Unhandled file event exception: {ex.Message}");
            }
        }

        private void OnDirectoryEvent(DirectoryOperation operation, string path, string? oldPath)
        {
            if (!_realtimeActive || _token.IsCancellationRequested)
                return;

            try
            {
                if (IsExcludedDirectoryPath(path) || (oldPath != null && IsExcludedDirectoryPath(oldPath)))
                    return;

                var kind = operation switch
                {
                    DirectoryOperation.Create => ChangeKind.DirectoryCreate,
                    DirectoryOperation.Delete => ChangeKind.DirectoryDelete,
                    _ => ChangeKind.DirectoryRename
                };

                // Directory operations are never merged, so each gets its own key
                var key = $"\0directory:{Interlocked.Increment(ref _directoryOperationSequence)}";
                Enqueue(key, new PendingChange(kind, path, oldPath));
            }
            catch (Exception ex)
            {
                Logger.LogError($"Unhandled directory event exception: {ex.Message}");
            }
        }

        private void Enqueue(string key, PendingChange change)
        {
            lock (_queueLock)
            {
                if (_pending.TryGetValue(key, out var existing))
                {
                    existing.MergeFrom(change);
                    return;
                }

                _pending[key] = change;
            }

            _queue.Writer.TryWrite(key);
        }

        /// <summary>
        /// Takes changes from the queue in order. Files are uploaded in parallel (up to the connection limit), but a
        /// directory operation waits for everything started before it and then runs on its own, so a directory is never
        /// deleted or renamed underneath an upload.
        /// </summary>
        private async Task DispatchLoopAsync()
        {
            var running = new List<Task>();

            try
            {
                while (await _queue.Reader.WaitToReadAsync(_token).ConfigureAwait(false))
                {
                    while (_queue.Reader.TryRead(out var key))
                    {
                        PendingChange? change;

                        lock (_queueLock)
                        {
                            if (!_pending.TryGetValue(key, out change))
                                continue;

                            // Never upload the same file on two connections at once. It is queued again when the first finishes.
                            if (!change.IsDirectoryOperation && _inFlight.Contains(key))
                            {
                                _blocked.Add(key);
                                continue;
                            }

                            _pending.Remove(key);

                            if (!change.IsDirectoryOperation)
                            {
                                _inFlight.Add(key);
                            }
                        }

                        if (change.IsDirectoryOperation)
                        {
                            await Task.WhenAll(running).ConfigureAwait(false);
                            running.Clear();

                            await Task.Run(() => ProcessDirectoryChange(change), _token).ConfigureAwait(false);
                        }
                        else
                        {
                            await _pendingFileTasks.WaitAsync(_token).ConfigureAwait(false);

                            var fileKey = key;
                            var fileChange = change;
                            running.Add(Task.Run(() => ProcessFileChangeAsync(fileKey, fileChange)));

                            if (running.Count >= MaxPendingFileTasks * 2)
                            {
                                running.RemoveAll(t => t.IsCompleted);
                            }
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Shutting down
            }
            catch (Exception ex)
            {
                Logger.LogError($"Sync queue stopped unexpectedly. Exception: {ex.Message}");
            }
        }

        private async Task ProcessFileChangeAsync(string key, PendingChange change)
        {
            try
            {
                if (_token.IsCancellationRequested)
                    return;

                if (change.Kind == ChangeKind.Upload)
                {
                    if (Directory.Exists(change.Path))
                        return;

                    // Wait for whatever is writing the file to finish, without holding on to a connection
                    var waitStart = DateTime.UtcNow;
                    DateTime? emptySince = null;

                    while (true)
                    {
                        if (TryGetUnlockedFileLength(change.Path, out var length))
                        {
                            if (length > 0)
                                break;

                            // A new file is often created empty and then written to, so give a writer a moment. If it is
                            // still empty after that, it really is an empty file and is synced as one.
                            emptySince ??= DateTime.UtcNow;
                            if (DateTime.UtcNow - emptySince.Value > EmptyFileGracePeriod)
                                break;
                        }
                        else
                        {
                            emptySince = null;

                            if (!File.Exists(change.Path))
                                return;
                        }

                        if (DateTime.UtcNow - waitStart > TimeSpan.FromSeconds(30))
                        {
                            Logger.LogWarnig($"Timeout waiting for file ready: {change.Path}");
                            return;
                        }

                        await Task.Delay(25, _token).ConfigureAwait(false);
                    }
                }

                using (var lease = await _pool.AcquireAsync(_token).ConfigureAwait(false))
                {
                    if (change.Kind == ChangeKind.Upload)
                    {
                        SyncChangedFile(lease.Client, change);
                    }
                    else
                    {
                        SyncFileDelete(lease.Client, change);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Shutting down
            }
            catch (Exception ex)
            {
                Logger.LogError($"Unhandled file sync exception: {ex.Message}");
            }
            finally
            {
                _pendingFileTasks.Release();

                lock (_queueLock)
                {
                    _inFlight.Remove(key);

                    if (_blocked.Remove(key))
                    {
                        _queue.Writer.TryWrite(key);
                    }
                }
            }
        }

        /// <summary>
        /// Sync changes for a file. This is only used for changes AFTER the initial sync has completed.
        /// </summary>
        private void SyncChangedFile(SftpClient sftp, PendingChange change)
        {
            var localPath = change.Path;
            var remoteFilePath = GetRemotePathForLocal(localPath);
            var remoteDirectoryPath = GetRemotePathForLocal(Path.GetDirectoryName(localPath) ?? _localRootDirectory);

            Logger.LogInfo($"Syncing {localPath}");

            const int maxRetries = 5;
            const int delayBetweenRetriesMs = 500;

            for (int attempt = 1; ; attempt++)
            {
                try
                {
                    if (!File.Exists(localPath))
                        return;     // Gone again already; a delete event will follow if it matters

                    if (!sftp.IsConnected)
                    {
                        sftp.Connect();
                    }

                    EnsureRemoteDirectory(sftp, remoteDirectoryPath);

                    // Because the VMS SFTP server uses Posix not RMS, and some older servers do not correctly
                    // handle truncating files when the content gets shorter (resulting in corruption at the end of the file),
                    // we delete the file first if it exists, before writing new content.
                    DeleteRemoteFileIfPresent(sftp, remoteFilePath);

                    sftp.WriteAllText(remoteFilePath, File.ReadAllText(localPath));
                    break;
                }
                catch (Exception ex)
                {
                    // Make sure the directory is checked for again
                    _knownRemoteDirectories.TryRemove(remoteDirectoryPath, out _);

                    if (attempt >= maxRetries)
                    {
                        Logger.LogError($"Sync failed after {maxRetries} retries. Exception: {ex.Message}");
                        return;
                    }

                    Logger.LogInfo($"Retry {attempt}/{maxRetries} after exception: {ex.Message}");
                    Thread.Sleep(delayBetweenRetriesMs);
                }
            }

            DeleteRenamedSource(sftp, change, remoteFilePath);
        }

        /// <summary>
        /// After a file has been renamed, delete the remote copy under its old name.
        /// </summary>
        private void DeleteRenamedSource(SftpClient sftp, PendingChange change, string remoteFilePath)
        {
            if (!_deleteEnabled || change.OldPath == null || IsVsTempFile(change.OldPath))
                return;

            var oldRemoteFilePath = GetRemotePathForLocal(change.OldPath);

            if (string.Equals(oldRemoteFilePath, remoteFilePath, StringComparison.OrdinalIgnoreCase))
                return;

            if (sftp.Exists(oldRemoteFilePath))
            {
                var attributes = sftp.GetAttributes(oldRemoteFilePath);
                if (!attributes.IsDirectory)
                {
                    sftp.DeleteFile(oldRemoteFilePath);
                }
                else
                {
                    Logger.LogWarnig($"Rename cleanup skipped (directory): {oldRemoteFilePath}");
                }
            }
            else
            {
                Logger.LogWarnig($"Rename cleanup skipped (not found): {oldRemoteFilePath}");
            }
        }

        private void SyncFileDelete(SftpClient sftp, PendingChange change)
        {
            var remotePath = GetRemotePathForLocal(change.Path);

            if (!sftp.IsConnected)
            {
                sftp.Connect();
            }

            if (sftp.Exists(remotePath))
            {
                var attributes = sftp.GetAttributes(remotePath);

                if (!attributes.IsDirectory)
                {
                    Logger.LogInfo($"Syncing delete {change.Path}");
                    sftp.DeleteFile(remotePath);
                }
                else
                {
                    Logger.LogWarnig($"Syncing delete skipped (directory): {remotePath}");
                }
            }
            else
            {
                Logger.LogWarnig($"Syncing delete {remotePath} not found");
            }

            DeleteRenamedSource(sftp, change, remotePath);
        }

        /// <summary>
        /// Make sure a remote directory (and any missing parents below the root) exists.
        /// </summary>
        private void EnsureRemoteDirectory(SftpClient sftp, string remotePath)
        {
            if (_knownRemoteDirectories.ContainsKey(remotePath))
                return;

            // Uploads run in parallel, so make sure only one of them creates a directory
            lock (_directoryCreateLock)
            {
                EnsureRemoteDirectoryLocked(sftp, remotePath);
            }
        }

        private void EnsureRemoteDirectoryLocked(SftpClient sftp, string remotePath)
        {
            if (_knownRemoteDirectories.ContainsKey(remotePath))
                return;

            // The remote root must already exist (it is checked when the initial sync starts)
            if (remotePath.Length > _remoteRootDirectory.Length && !sftp.Exists(remotePath))
            {
                var separator = remotePath.LastIndexOf('/');
                if (separator > 0 && separator >= _remoteRootDirectory.Length)
                {
                    EnsureRemoteDirectoryLocked(sftp, remotePath.Substring(0, separator));
                }

                Logger.LogInfo($"Creating directory {remotePath}");
                sftp.CreateDirectory(remotePath);
            }

            _knownRemoteDirectories[remotePath] = 0;
        }

        private void ForgetRemoteDirectories(string remotePath)
        {
            foreach (var known in _knownRemoteDirectories.Keys)
            {
                if (string.Equals(known, remotePath, StringComparison.OrdinalIgnoreCase)
                    || known.StartsWith(remotePath + "/", StringComparison.OrdinalIgnoreCase))
                {
                    _knownRemoteDirectories.TryRemove(known, out _);
                }
            }
        }

        private void ProcessDirectoryChange(PendingChange change)
        {
            try
            {
                if (_token.IsCancellationRequested)
                    return;

                using (var lease = _pool.AcquireAsync(_token).GetAwaiter().GetResult())
                {
                    var sftp = lease.Client;

                    switch (change.Kind)
                    {
                        case ChangeKind.DirectoryCreate:
                            EnsureRemoteDirectory(sftp, GetRemotePathForLocal(change.Path));
                            break;

                        case ChangeKind.DirectoryDelete:
                            DeleteRemoteDirectory(sftp, change);
                            break;

                        case ChangeKind.DirectoryRename:
                            RenameRemoteDirectory(sftp, change);
                            break;
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Shutting down
            }
            catch (Exception ex)
            {
                Logger.LogError($"Unhandled directory sync exception: {ex.Message}");
            }
        }

        private void DeleteRemoteDirectory(SftpClient sftp, PendingChange change)
        {
            var remotePath = GetRemotePathForLocal(change.Path);
            ForgetRemoteDirectories(remotePath);

            if (!sftp.Exists(remotePath) && !sftp.Exists(remotePath + ".DIR"))
            {
                Logger.LogWarnig($"Deleting directory skipped (not found): {remotePath}");
            }
            else
            {
                Logger.LogInfo($"Deleting directory {remotePath}");
                DeleteRemotePathRecursive(sftp, remotePath);
            }
        }

        private void RenameRemoteDirectory(SftpClient sftp, PendingChange change)
        {
            var oldRemotePath = GetRemotePathForLocal(change.OldPath ?? change.Path);
            var newRemotePath = GetRemotePathForLocal(change.Path);

            // OpenVMS directory names preserve case but are case-insensitive. A case-only
            // local rename therefore refers to the same remote directory and needs no action.
            if (string.Equals(oldRemotePath, newRemotePath, StringComparison.OrdinalIgnoreCase))
            {
                Logger.LogInfo($"Ignoring case-only directory rename: {oldRemotePath} -> {newRemotePath}");
                return;
            }

            ForgetRemoteDirectories(oldRemotePath);

            if (TryRenameRemoteDirectory(sftp, oldRemotePath, newRemotePath))
            {
                Logger.LogInfo($"Renamed directory {oldRemotePath} -> {newRemotePath}");
                return;
            }

            Logger.LogWarnig($"Directory rename failed. Attempting fallback: {oldRemotePath} -> {newRemotePath}");

            MoveRemoteDirectoryRecursive(sftp, oldRemotePath, newRemotePath);

            Logger.LogInfo($"Fallback directory rename completed: {oldRemotePath} -> {newRemotePath}");
        }

        // ------------------------------------------------------------------------------------------------------------
        // Initial sync
        // ------------------------------------------------------------------------------------------------------------

        private const int MaxAttempts = 3;
        private const int RetryDelayMs = 2000;

        private static async Task ConnectWithRetryAsync(SftpClient sftp, string description)
        {
            for (int attempt = 1; ; attempt++)
            {
                try
                {
                    sftp.Connect();
                    return;
                }
                catch (Exception ex) when (attempt < MaxAttempts)
                {
                    Logger.LogWarnig($"Connection for {description} failed (attempt {attempt}/{MaxAttempts}), retrying. Exception: {ex.Message}");
                    await Task.Delay(RetryDelayMs * attempt);
                }
            }
        }


        private sealed class SyncChunk
        {
            public SyncChunk(string remotePath, string[] files)
            {
                RemotePath = remotePath;
                Files = files;
            }

            public string RemotePath { get; }
            public string[] Files { get; }
        }

        private int _initialSyncFilesDone;

        private static readonly TimeSpan ProgressInterval = TimeSpan.FromSeconds(5);

        /// <summary>
        /// Raised during the initial sync with the number of files uploaded so far and the total to upload. This can be
        /// raised on any thread.
        /// </summary>
        public event Action<int, int>? InitialSyncProgress;

        private async Task ReportProgressAsync(int totalFiles, CancellationToken token)
        {
            try
            {
                using (var timer = new PeriodicTimer(ProgressInterval))
                {
                    while (await timer.WaitForNextTickAsync(token).ConfigureAwait(false))
                    {
                        PublishProgress(totalFiles, logIt: true);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // The initial sync has finished
            }
        }

        private void PublishProgress(int totalFiles, bool logIt)
        {
            int done = Volatile.Read(ref _initialSyncFilesDone);

            if (logIt)
            {
                Logger.LogInfo($"Initial sync: {done} of {totalFiles} files uploaded ({(totalFiles == 0 ? 100 : done * 100L / totalFiles)}%)");
            }

            try
            {
                InitialSyncProgress?.Invoke(done, totalFiles);
            }
            catch (Exception ex)
            {
                Logger.LogError($"Progress notification failed. Exception: {ex.Message}");
            }
        }

        /// <summary>
        /// Create any missing remote directories, upload every matching file, then switch to real-time syncing.
        /// </summary>
        public async Task InitialSyncAsync()
        {
            var stopwatch = Stopwatch.StartNew();
            var remoteRoot = NormalizeRemoteRootDirectory(_options.RemoteRootDirectory);

            try
            {
                using (var sftp = new SftpClient(_options.Host, _options.Username, _options.Password))
                {
                    await ConnectWithRetryAsync(sftp, "directory creation");
                    await CreateDirectoriesInternal(sftp, _localRootDirectory, _localRootDirectory, remoteRoot, _excludedFolders);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"Create directories failed. Exception: {ex.Message}");
                throw;
            }

            _token.ThrowIfCancellationRequested();

            // Look at each directory once, rather than once per pattern, and split large directories into chunks so that
            // several connections can share them (one directory can hold thousands of files). The biggest chunks go first,
            // so no connection is left uploading a large one on its own at the end.
            var directories = EnumerateLocalDirectories(_localRootDirectory, remoteRoot, _excludedFolders).ToList();
            var chunkSize = Math.Max(1, _options.InitialSyncChunkSize);
            var chunks = new List<SyncChunk>();
            int totalFiles = 0;

            foreach (var directory in directories)
            {
                _knownRemoteDirectories[directory.RemotePath] = 0;

                var files = new DirectoryInfo(directory.LocalPath)
                    .EnumerateFiles("*", SearchOption.TopDirectoryOnly)
                    .Select(file => file.FullName)
                    .Where(path => MatchesSearchPattern(Path.GetFileName(path)))
                    .ToArray();

                totalFiles += files.Length;

                for (int start = 0; start < files.Length; start += chunkSize)
                {
                    chunks.Add(new SyncChunk(directory.RemotePath, files.Skip(start).Take(chunkSize).ToArray()));
                }
            }

            var workQueue = new ConcurrentQueue<SyncChunk>(chunks.OrderByDescending(chunk => chunk.Files.Length));
            int workerCount = Math.Max(1, Math.Min(_options.InitialSyncWorkers, chunks.Count));

            if (chunks.Count > 0)
            {
                Logger.LogInfo($"Initial sync: {totalFiles} files in {directories.Count} directories, using {workerCount} connections");

                PublishProgress(totalFiles, logIt: false);

                using var progressCts = CancellationTokenSource.CreateLinkedTokenSource(_token);
                var progressReporter = Task.Run(() => ReportProgressAsync(totalFiles, progressCts.Token));

                var workers = new List<Task>();

                for (int i = 0; i < workerCount; i++)
                {
                    int workerNumber = i + 1;
                    workers.Add(Task.Run(async () =>
                    {
                        using (var sftp = new SftpClient(_options.Host, _options.Username, _options.Password))
                        {
                            try
                            {
                                await ConnectWithRetryAsync(sftp, $"sync worker {workerNumber}");
                            }
                            catch (Exception ex)
                            {
                                // Other workers drain the shared queue. If none can connect, this is reported after WhenAll.
                                Logger.LogWarnig($"Sync worker {workerNumber} could not connect and will not take part in the initial sync. Exception: {ex.Message}");
                                return;
                            }

                            try
                            {
                                while (!_token.IsCancellationRequested && workQueue.TryDequeue(out var chunk))
                                {
                                    await SyncChunkWithRetryAsync(sftp, chunk);
                                }
                            }
                            catch (OperationCanceledException)
                            {
                                // Shutting down
                            }
                        }
                    }));
                }

                try
                {
                    await Task.WhenAll(workers);
                }
                finally
                {
                    progressCts.Cancel();
                }

                await progressReporter;
                PublishProgress(totalFiles, logIt: false);

                _token.ThrowIfCancellationRequested();

                if (!workQueue.IsEmpty)
                {
                    throw new InvalidOperationException("Initial sync could not complete because no sync worker was able to connect.");
                }
            }

            Logger.LogInfo($"Initial sync finished: {_initialSyncFilesDone} of {totalFiles} files in {stopwatch.Elapsed.TotalSeconds:0.0} seconds");

            // From now on changes are synced as they happen
            _realtimeActive = true;
            _ = Task.Run(DispatchLoopAsync);

            // Open the one connection that is kept warm, so the first change is not delayed by connecting
            _ = Task.Run(async () =>
            {
                try
                {
                    using (await _pool.AcquireAsync(_token))
                    {
                    }
                }
                catch (Exception)
                {
                    // Reported when a change needs a connection
                }
            });
        }

        private bool MatchesSearchPattern(string fileName)
        {
            foreach (var pattern in _searchPatterns)
            {
                if (FileSystemName.MatchesSimpleExpression(pattern, fileName, ignoreCase: true))
                    return true;
            }

            return false;
        }

        private async Task SyncChunkWithRetryAsync(SftpClient sftp, SyncChunk chunk)
        {
            int index = 0;

            for (int attempt = 1; ; attempt++)
            {
                try
                {
                    // Re-establish the connection if a previous timeout dropped it.
                    if (!sftp.IsConnected)
                    {
                        sftp.Connect();
                    }

                    // A retry carries on from the file that failed, not from the start of the chunk
                    for (; index < chunk.Files.Length; index++)
                    {
                        _token.ThrowIfCancellationRequested();
                        UploadInitialFile(sftp, chunk.Files[index], chunk.RemotePath);
                        Interlocked.Increment(ref _initialSyncFilesDone);
                    }

                    return;
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    var failedFile = index < chunk.Files.Length ? chunk.Files[index] : chunk.RemotePath;

                    if (attempt >= MaxAttempts)
                    {
                        Logger.LogError($"Sync failed for {failedFile} after {MaxAttempts} attempts. Exception: {ex.Message}");
                        return;
                    }

                    Logger.LogWarnig($"Sync failed for {failedFile} (attempt {attempt}/{MaxAttempts}), retrying. Exception: {ex.Message}");
                    await Task.Delay(RetryDelayMs * attempt, _token);
                }
            }
        }

        /// <summary>
        /// Delete a remote file before it is replaced, if it is there. This tries the delete straight away, rather than
        /// checking for the file first, which saves a round trip for every file that exists.
        ///
        /// Only "file not found" may be ignored. Any other failure must stop the upload: if the old file were left in
        /// place, the server could write the new content over it without truncating, leaving stale data at the end.
        /// </summary>
        private static void DeleteRemoteFileIfPresent(SftpClient sftp, string remoteFilePath)
        {
            try
            {
                sftp.DeleteFile(remoteFilePath);
            }
            catch (SftpPathNotFoundException)
            {
                // Not there, so there is nothing to delete
            }
            catch (SshException)
            {
                // A server that reports a missing file as some other failure: only carry on if the file really is absent
                if (sftp.Exists(remoteFilePath))
                    throw;
            }
        }

        /// <summary>
        /// Remote files are deleted first, as the VMS server may not truncate correctly.
        /// </summary>
        private static void UploadInitialFile(SftpClient sftp, string localFile, string remoteDirectory)
        {
            var remoteFileName = $"{remoteDirectory}/{Path.GetFileName(localFile)}";

            try
            {
                DeleteRemoteFileIfPresent(sftp, remoteFileName);

                using (var file = File.OpenRead(localFile))
                {
                    sftp.UploadFile(file, remoteFileName, canOverride: true);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to upload {localFile} to {remoteFileName}", ex);
            }
        }

        public void Dispose()
        {
            lock (_disposeLock)
            {
                if (_disposed)
                    return;

                _disposed = true;
            }

            _realtimeActive = false;
            _shutdownCts.Cancel();
            _queue.Writer.TryComplete();
            _pool.Dispose();
            _shutdownCts.Dispose();
        }

        private static string[] FilteredDirectories(string localRootDirectory, string localPath, List<string>? excludedFolders)
        {
            return Directory.GetDirectories(localPath).Where(path =>
            {
                var relativePath = path.Substring(localRootDirectory.Length);

                bool isExcluded = relativePath.EndsWith(".git")
                                  || relativePath.EndsWith(".vs")
                                  || relativePath.EndsWith("bin")
                                  || relativePath.EndsWith("obj")
                                  || relativePath.Contains(".");

                if (!isExcluded && excludedFolders != null && excludedFolders.Count > 0)
                {
                    string fullPath = Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar);
                    isExcluded = excludedFolders.Any(excluded =>
                    {
                        var excludedFullPath = Path.GetFullPath(excluded).TrimEnd(Path.DirectorySeparatorChar);
                        return string.Equals(fullPath, excludedFullPath, StringComparison.OrdinalIgnoreCase);
                    });
                }

                return !isExcluded;
            }).ToArray();
        }

        private static IEnumerable<(string LocalPath, string RemotePath)> EnumerateLocalDirectories(
            string localRootDirectory,
            string remoteRootDirectory,
            List<string>? excludedFolders)
        {
            var queue = new Queue<(string LocalPath, string RemotePath)>();
            queue.Enqueue((localRootDirectory, remoteRootDirectory));

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                yield return current;

                foreach (var directory in FilteredDirectories(localRootDirectory, current.LocalPath, excludedFolders)
                    .OrderBy(path => path, StringComparer.OrdinalIgnoreCase))
                {
                    var directoryName = directory.Split(Path.DirectorySeparatorChar).Last();
                    var remotePath = AppendRemotePath(current.RemotePath, directoryName);
                    queue.Enqueue((directory, remotePath));
                }
            }
        }

        private static async Task CreateDirectoriesInternal(
            SftpClient sftp,
            string localRootDirectory,
            string localPath,
            string remotePath,
            List<string>? excludedFolders)
        {
            var localDirectories = FilteredDirectories(localRootDirectory, localPath, excludedFolders)
                .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            var remoteDirectories = (await ListDirectoryAsync(sftp, remotePath)).Where(item => item.IsDirectory).ToDictionary(item =>
            {
                if (item.Name.Contains(".DIR", StringComparison.OrdinalIgnoreCase))
                    return item.Name.Remove(item.Name.IndexOf(".DIR", StringComparison.OrdinalIgnoreCase));
                else
                    return item.Name;
            }, StringComparer.OrdinalIgnoreCase);

            foreach (var item in localDirectories)
            {
                var directoryName = item.Split(Path.DirectorySeparatorChar).Last();
                var targetRemotePath = AppendRemotePath(remotePath, directoryName);
                if (!remoteDirectories.ContainsKey(directoryName))
                {
                    Logger.LogInfo($"Creating remote directory {targetRemotePath}");
                    sftp.CreateDirectory(targetRemotePath);
                }
                await CreateDirectoriesInternal(sftp, localRootDirectory, localPath + "\\" + directoryName, targetRemotePath, excludedFolders);
            }
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


        /// <summary>
        /// If the file can be opened for exclusive access it means that the file is no longer locked by another
        /// process. Returns false if it is locked (or missing), otherwise true with the file's length.
        /// </summary>
        public static bool TryGetUnlockedFileLength(string fileName, out long length)
        {
            try
            {
                using (var inputStream = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    length = inputStream.Length;
                    return true;
                }
            }
            catch (Exception)
            {
                length = 0;
                return false;
            }
        }

        private string GetRemotePathForLocal(string localPath)
        {
            var relativePath = Path.GetRelativePath(_localRootDirectory, localPath);
            if (relativePath == "." || string.IsNullOrEmpty(relativePath))
                return _remoteRootDirectory;

            relativePath = relativePath.Replace('\\', '/').TrimStart('/');
            if (relativePath.Length == 0)
                return _remoteRootDirectory;

            return _remoteRootDirectory + "/" + relativePath;
        }

        private static string NormalizeRemoteRootDirectory(string remoteRootDirectory)
        {
            if (remoteRootDirectory == "/")
                return "/";

            return remoteRootDirectory.TrimEnd('/', '\\');
        }

        private static string AppendRemotePath(string remotePath, string segment)
        {
            if (string.IsNullOrEmpty(segment))
                return remotePath;

            var normalizedSegment = segment.Replace('\\', '/').Trim('/');
            if (normalizedSegment.Length == 0)
                return remotePath;

            if (remotePath == "/")
                return "/" + normalizedSegment;

            if (string.IsNullOrEmpty(remotePath))
                return normalizedSegment;

            return remotePath + "/" + normalizedSegment;
        }

        /// <summary>
        /// True if a file is in an excluded directory, or in a subdirectory of one. The directories are checked from the
        /// local root down, the same way the initial sync decides which directories to visit.
        /// </summary>
        private bool IsInExcludedDirectory(string localFilePath)
        {
            var directory = Path.GetDirectoryName(Path.GetFullPath(localFilePath));

            while (!string.IsNullOrEmpty(directory)
                   && directory.Length > _localRootDirectory.Length
                   && directory.StartsWith(_localRootDirectory, StringComparison.OrdinalIgnoreCase))
            {
                if (IsExcludedDirectoryPath(directory))
                    return true;

                directory = Path.GetDirectoryName(directory);
            }

            return false;
        }

        private bool IsExcludedDirectoryPath(string localPath)
        {
            var fullPath = Path.GetFullPath(localPath).TrimEnd(Path.DirectorySeparatorChar);
            if (!fullPath.StartsWith(_localRootDirectory, StringComparison.OrdinalIgnoreCase))
                return false;

            var relativePath = fullPath.Substring(_localRootDirectory.Length);
            if (string.IsNullOrWhiteSpace(relativePath))
                return false;

            bool isExcluded = relativePath.EndsWith(".git", StringComparison.OrdinalIgnoreCase)
                              || relativePath.EndsWith(".vs", StringComparison.OrdinalIgnoreCase)
                              || relativePath.EndsWith("bin", StringComparison.OrdinalIgnoreCase)
                              || relativePath.EndsWith("obj", StringComparison.OrdinalIgnoreCase)
                              || relativePath.Contains(".");

            if (!isExcluded && _excludedFolders != null && _excludedFolders.Count > 0)
            {
                isExcluded = _excludedFolders.Any(excluded =>
                {
                    var excludedFullPath = Path.GetFullPath(excluded).TrimEnd(Path.DirectorySeparatorChar);
                    return string.Equals(fullPath, excludedFullPath, StringComparison.OrdinalIgnoreCase)
                           || fullPath.StartsWith(excludedFullPath + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);
                });
            }

            return isExcluded;
        }

        private static void DeleteRemotePathRecursive(SftpClient sftp, string remotePath)
        {
            if (!sftp.Exists(remotePath))
            {
                var vmsDirectoryPath = remotePath + ".DIR";
                if (!sftp.Exists(vmsDirectoryPath))
                    return;
                remotePath = vmsDirectoryPath;
            }

            var attributes = sftp.GetAttributes(remotePath);
            if (!attributes.IsDirectory)
            {
                sftp.DeleteFile(remotePath);
                return;
            }

            foreach (var entry in sftp.ListDirectory(remotePath))
            {
                if (entry.Name == "." || entry.Name == "..")
                    continue;

                if (entry.IsDirectory)
                {
                    DeleteRemotePathRecursive(sftp, entry.FullName);
                }
                else
                {
                    sftp.DeleteFile(entry.FullName);
                }
            }

            sftp.DeleteDirectory(remotePath);
        }

        private static bool TryRenameRemoteDirectory(SftpClient sftp, string oldRemotePath, string newRemotePath)
        {
            string? actualOldPath = null;
            string? actualNewPath = null;

            if (sftp.Exists(oldRemotePath))
            {
                actualOldPath = oldRemotePath;
                actualNewPath = newRemotePath;
            }
            else if (sftp.Exists(oldRemotePath + ".DIR"))
            {
                actualOldPath = oldRemotePath + ".DIR";
                actualNewPath = newRemotePath + ".DIR";
            }

            if (actualOldPath == null || actualNewPath == null)
                return false;

            try
            {
                sftp.RenameFile(actualOldPath, actualNewPath);
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogError($"Directory rename failed: {ex.Message}");
                return false;
            }
        }

        private static void MoveRemoteDirectoryRecursive(SftpClient sftp, string oldRemotePath, string newRemotePath)
        {
            string actualOldPath;
            string actualNewPath = newRemotePath;

            if (sftp.Exists(oldRemotePath))
            {
                actualOldPath = oldRemotePath;
            }
            else if (sftp.Exists(oldRemotePath + ".DIR"))
            {
                actualOldPath = oldRemotePath + ".DIR";
                actualNewPath = newRemotePath + ".DIR";
            }
            else
            {
                Logger.LogError($"Directory not found for rename: {oldRemotePath}");
                return;
            }

            if (!sftp.Exists(actualNewPath))
            {
                sftp.CreateDirectory(actualNewPath);
            }

            foreach (var entry in sftp.ListDirectory(actualOldPath))
            {
                if (entry.Name == "." || entry.Name == "..")
                    continue;

                var destinationPath = actualNewPath + "/" + entry.Name;
                try
                {
                    sftp.RenameFile(entry.FullName, destinationPath);
                }
                catch (Exception ex)
                {
                    Logger.LogError($"Remote file move failed for {entry.FullName}: {ex.Message}");
                    if (entry.IsDirectory)
                    {
                        MoveRemoteDirectoryRecursive(sftp, entry.FullName, destinationPath);
                    }
                }
            }

            DeleteRemotePathRecursive(sftp, actualOldPath);
        }
    }
}
