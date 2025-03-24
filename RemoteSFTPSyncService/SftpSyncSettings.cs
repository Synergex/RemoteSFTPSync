
namespace RemoteSFTPSyncService
{
    public class SftpSyncSettings
    {
        public string LocalRoot { get; set; } = String.Empty;
        public string SearchPatterns { get; set; } = String.Empty;

        public string RemoteHost { get; set; } = String.Empty;

        public string RemoteUsername { get; set; } = String.Empty;

        public string RemotePassword { get; set; } = String.Empty;

        public string RemoteRoot { get; set; } = String.Empty;

    }
}
