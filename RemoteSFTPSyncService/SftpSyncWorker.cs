
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RemoteSFTPSyncService;

namespace RemoteSFTPSync
{
    public class SftpSyncWorker : BackgroundService
    {
        private readonly ILogger<SftpSyncWorker> _logger;
        private readonly SftpSyncSettings _settings;

        private List<RemoteSync>? _remoteSyncs;
        private SyncDirector? _director;

        public SftpSyncWorker(
            ILogger<SftpSyncWorker> logger,
            IOptions<SftpSyncSettings> options)
        {
            _logger = logger;
            _settings = options.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("SFTP Sync Worker starting.");

            _director = new SyncDirector(_settings.LocalRoot);
            _remoteSyncs = new List<RemoteSync>();

            foreach (var pattern in _settings.SearchPatterns.Split(';', StringSplitOptions.RemoveEmptyEntries))
            {
                if (_remoteSyncs.Count > 0)
                    await _remoteSyncs[0].DoneMakingFolders;

                _remoteSyncs.Add(new RemoteSync(
                    _settings.RemoteHost,
                    _settings.RemoteUsername,
                    _settings.RemotePassword,
                    _settings.LocalRoot,
                    _settings.RemoteRoot,
                    pattern,
                    _remoteSyncs.Count == 0,
                    _director));
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);
            }

            foreach (var sync in _remoteSyncs)
            {
                sync.Dispose();
            }

            _logger.LogInformation("SFTP Sync Worker stopping.");
        }
    }
}
