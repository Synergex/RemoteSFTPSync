using Renci.SshNet;

namespace SFTPSyncLib
{
    /// <summary>
    /// A small elastic pool of SFTP connections. A connection is only opened when work needs one and none is idle (up to
    /// the maximum), and idle connections are closed after a while. One connection is always kept warm so that the first
    /// change after a quiet period is not delayed by a reconnect.
    /// </summary>
    internal sealed class SftpConnectionPool : IDisposable
    {
        private const int MaxConnectAttempts = 3;
        private const int ConnectRetryDelayMs = 2000;
        private static readonly TimeSpan KeepAliveInterval = TimeSpan.FromSeconds(30);

        private readonly string _host;
        private readonly string _username;
        private readonly string _password;
        private readonly TimeSpan _idleTimeout;
        private readonly SemaphoreSlim _slots;
        private readonly List<IdleConnection> _idle = new List<IdleConnection>();
        private readonly object _lock = new object();
        private readonly Timer _reaper;
        private bool _disposed;

        public SftpConnectionPool(string host, string username, string password, int maxConnections, TimeSpan idleTimeout)
        {
            _host = host;
            _username = username;
            _password = password;
            _idleTimeout = idleTimeout;
            _slots = new SemaphoreSlim(Math.Max(1, maxConnections));
            _reaper = new Timer(_ => CloseIdleConnections(), null, TimeSpan.FromSeconds(15), TimeSpan.FromSeconds(15));
        }

        /// <summary>
        /// Wait for a free slot, then return a connected client (an idle one if there is one, otherwise a new one).
        /// Dispose the lease to give the connection back.
        /// </summary>
        public async Task<Lease> AcquireAsync(CancellationToken token)
        {
            await _slots.WaitAsync(token).ConfigureAwait(false);

            SftpClient? client = null;
            try
            {
                for (int attempt = 1; ; attempt++)
                {
                    client = TakeIdle() ?? CreateClient();
                    try
                    {
                        if (!client.IsConnected)
                        {
                            var toConnect = client;
                            await Task.Run(() => toConnect.Connect(), token).ConfigureAwait(false);
                        }
                        return new Lease(this, client);
                    }
                    catch (Exception ex) when (attempt < MaxConnectAttempts && !token.IsCancellationRequested)
                    {
                        Logger.LogWarnig($"SFTP connection failed (attempt {attempt}/{MaxConnectAttempts}), retrying. Exception: {ex.Message}");
                        DisposeQuietly(client);
                        client = null;
                        await Task.Delay(ConnectRetryDelayMs * attempt, token).ConfigureAwait(false);
                    }
                }
            }
            catch
            {
                DisposeQuietly(client);
                _slots.Release();
                throw;
            }
        }

        private SftpClient CreateClient()
        {
            return new SftpClient(_host, _username, _password) { KeepAliveInterval = KeepAliveInterval };
        }

        private SftpClient? TakeIdle()
        {
            lock (_lock)
            {
                if (_idle.Count == 0)
                    return null;

                // Most recently used first, so busy connections stay warm and the others age out
                var entry = _idle[_idle.Count - 1];
                _idle.RemoveAt(_idle.Count - 1);
                return entry.Client;
            }
        }

        private void Release(SftpClient client)
        {
            bool keep;
            lock (_lock)
            {
                keep = !_disposed && client.IsConnected;
                if (keep)
                {
                    _idle.Add(new IdleConnection(client, DateTime.UtcNow));
                }
            }

            if (!keep)
            {
                DisposeQuietly(client);
            }

            try
            {
                _slots.Release();
            }
            catch (ObjectDisposedException)
            {
            }
        }

        private void CloseIdleConnections()
        {
            List<SftpClient>? expired = null;

            lock (_lock)
            {
                if (_disposed)
                    return;

                var now = DateTime.UtcNow;

                // The oldest are at the start. Always leave one warm connection.
                while (_idle.Count > 1 && now - _idle[0].IdleSince > _idleTimeout)
                {
                    expired ??= new List<SftpClient>();
                    expired.Add(_idle[0].Client);
                    _idle.RemoveAt(0);
                }
            }

            if (expired != null)
            {
                Logger.LogInfo($"Closing {expired.Count} idle SFTP connection(s)");
                foreach (var client in expired)
                {
                    DisposeQuietly(client);
                }
            }
        }

        private static void DisposeQuietly(SftpClient? client)
        {
            try
            {
                client?.Dispose();
            }
            catch
            {
                // Closing a dead connection can throw; nothing useful to do about it
            }
        }

        public void Dispose()
        {
            List<IdleConnection> idle;

            lock (_lock)
            {
                if (_disposed)
                    return;

                _disposed = true;
                idle = new List<IdleConnection>(_idle);
                _idle.Clear();
            }

            _reaper.Dispose();

            // Closing a connection can block for a long time if the server is slow, so close them in parallel.
            // Connections that are in use are closed when their lease is returned.
            Parallel.ForEach(idle, entry => DisposeQuietly(entry.Client));
        }

        private readonly record struct IdleConnection(SftpClient Client, DateTime IdleSince);

        public sealed class Lease : IDisposable
        {
            private readonly SftpConnectionPool _pool;
            private SftpClient? _client;

            internal Lease(SftpConnectionPool pool, SftpClient client)
            {
                _pool = pool;
                _client = client;
            }

            public SftpClient Client => _client ?? throw new ObjectDisposedException(nameof(Lease));

            public void Dispose()
            {
                var client = Interlocked.Exchange(ref _client, null);
                if (client != null)
                {
                    _pool.Release(client);
                }
            }
        }
    }
}
