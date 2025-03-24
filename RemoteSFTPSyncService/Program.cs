using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RemoteSFTPSyncService;

namespace RemoteSFTPSync
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var isService = !(System.Diagnostics.Debugger.IsAttached || args.Contains("--console"));

            var builder = Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    string configFilePath = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                        "RemoteSFTPSync.config");
                    config.AddJsonFile(configFilePath, optional: false, reloadOnChange: true);
                })
                .ConfigureServices((context, services) =>
                {
                    services.Configure<SftpSyncSettings>(context.Configuration.GetSection("SftpSyncSettings"));
                    services.AddHostedService<SftpSyncWorker>();
                });

            if (OperatingSystem.IsWindows() && isService)
            {
                builder.UseWindowsService();
#if WINDOWS
                builder.ConfigureLogging(logging =>
                {
                    logging.AddEventLog();
                });
#endif      
            }
            else
            {
                builder.ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddConsole();
                });
            }

            builder.Build().Run();
        }
    }
}
