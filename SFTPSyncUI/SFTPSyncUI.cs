
using Microsoft.Win32;
using SFTPSyncLib;
using System.Reflection;

namespace SFTPSyncUI
{
    internal static class SFTPSyncUI
    {
        static Mutex? mutex = null;

        public static string ExecutableFile = Assembly.GetExecutingAssembly().Location.Replace(".dll", ".exe");
        public static AppSettings? Settings { get; private set; }

        private const string autoRunRegistryKey = @"Software\Microsoft\Windows\CurrentVersion\Run";

        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Check if another instance is already running and if so, exit
            const string mutexName = $"Global\\SFTPSync";
            bool createdNew;
            mutex = new Mutex(true, mutexName, out createdNew);
            if (!createdNew)
            {
                MessageBox.Show($"Another instance of {Application.ProductName} is already running.", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
                Application.Exit();
                return;
            }

            // Load settings
            try
            {
                Settings = AppSettings.LoadFromFile();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
                return;
            }

            // Do we have settings?
            if (Settings == null)
            {
                MessageBox.Show("Failed to load user settings!", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
                return;
            }

            // Check the run at startup status is correct
            if (Settings.StartAtLogin && !IsProgramInStartup())
                AddProgramToStartup();
            else if (!Settings.StartAtLogin && IsProgramInStartup())
                RemoveProgramFromStartup();

            // Run the main form (it will start hidden, but put an icon in the system tray)
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }

        /// <summary>
        /// Check whether the program is set to run at startup
        /// </summary>
        /// <returns></returns>
        static bool IsProgramInStartup()
        {
            using (RegistryKey? key = Registry.CurrentUser.OpenSubKey(autoRunRegistryKey, false))
            {
                return key?.GetValue(Application.ProductName) != null;
            }
        }

        /// <summary>
        /// Add the program to run at startup
        /// </summary>
        static void AddProgramToStartup()
        {
            using (RegistryKey? key = Registry.CurrentUser.OpenSubKey(autoRunRegistryKey, true))
            {
                if (key != null)
                {
                    key.SetValue(Application.ProductName, $"\"{Assembly.GetExecutingAssembly().Location.Replace(".dll", ".exe")}\"");
                }
            }
        }

        /// <summary>
        /// Remove the program from running at startup
        /// </summary>
        static void RemoveProgramFromStartup()
        {
            if (Application.ProductName != null)
            {
                using (RegistryKey? key = Registry.CurrentUser.OpenSubKey(autoRunRegistryKey, true))
                {
                    if (key?.GetValue(Application.ProductName) != null)
                    {
                        key.DeleteValue(Application.ProductName);
                    }
                }
            }
        }

        public static List<RemoteSync> remoteSyncs = new List<RemoteSync>();

        public static async void StartSync(Action<string> loggerAction)
        {
            if (Settings == null)
                return;

            Logger.LogUpdated += loggerAction;
            Logger.Log("Starting sync...");

            var director = new SyncDirector(Settings.LocalPath);

            foreach (var split in Settings.LocalSearchPattern.Split(';', StringSplitOptions.RemoveEmptyEntries))
            {
                if (remoteSyncs.Count > 0)
                {
                    await remoteSyncs[0].DoneMakingFolders;
                }
                remoteSyncs.Add(new RemoteSync(Settings.RemoteHost, Settings.RemoteUsername, Settings.RemotePassword, Settings.LocalPath, Settings.RemotePath, split, remoteSyncs.Count == 0, director));
            }
        }

        public static void StopSync(Action<string> loggerAction)
        {
            foreach (var remoteSync in remoteSyncs)
            {
                try
                {
                    remoteSync.Dispose();
                }
                catch { /* Swallow any exceptions */ }
            }

            remoteSyncs.Clear();

            Logger.Log("Stopping sync...");
            Logger.LogUpdated -= loggerAction;
        }
    }
}