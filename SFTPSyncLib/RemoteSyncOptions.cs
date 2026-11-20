namespace SFTPSyncLib
{
    /// <summary>
    /// Everything a <see cref="RemoteSync"/> needs to know about what to sync and how many connections it may use.
    /// </summary>
    public sealed class RemoteSyncOptions
    {
        public const int DefaultInitialSyncWorkers = 20;
        public const int DefaultMaxConnections = 6;
        public const int DefaultInitialSyncChunkSize = 50;

        public string Host { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string LocalRootDirectory { get; set; } = string.Empty;
        public string RemoteRootDirectory { get; set; } = string.Empty;

        /// <summary>The file name patterns (for example *.DBL) that are synced.</summary>
        public string[] SearchPatterns { get; set; } = Array.Empty<string>();

        public List<string>? ExcludedFolders { get; set; }
        public bool DeleteEnabled { get; set; }

        /// <summary>
        /// The number of connections used (temporarily) to run the initial sync. Work is shared between them, so this is
        /// independent of the number of search patterns.
        /// </summary>
        public int InitialSyncWorkers { get; set; } = DefaultInitialSyncWorkers;

        /// <summary>
        /// The most connections that will be open at once for real-time syncing. Connections are only opened when there is
        /// work for them (for example after switching Git branch) and are closed again when idle.
        /// </summary>
        public int MaxConnections { get; set; } = DefaultMaxConnections;

        /// <summary>Directories with more files than this are split so that several connections can share them.</summary>
        public int InitialSyncChunkSize { get; set; } = DefaultInitialSyncChunkSize;
    }
}
