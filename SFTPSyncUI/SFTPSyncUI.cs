
using Microsoft.Win32;
using SFTPSyncLib;
using System.IO.Pipes;
using System.Reflection;

namespace SFTPSyncUI
{
    internal static class SFTPSyncUI
    {
        public static string ExecutableFile = String.Empty;

        public static AppSettings? Settings { get; private set; }

        private const string autoRunRegistryKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
        private const string pipeName = "SFTPSyncUIPipe";

        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            string? processPath = Environment.ProcessPath;
            if (processPath != null)
                ExecutableFile = Path.ChangeExtension(processPath, ".exe");

            // Check if another instance is already running and if so, tell it to show its window, then exit
            bool createdNew;
            Mutex mutex = new Mutex(true, $"Global\\SFTPSyncUI", out createdNew);

            if (!createdNew)
            {
                try
                {
                    using (var client = new NamedPipeClientStream(".", pipeName, PipeDirection.Out))
                    {
                        client.Connect(1000); // 1 second timeout
                        using (var writer = new StreamWriter(client) { AutoFlush = true })
                        {
                            writer.WriteLine("SHOW");
                        }
                    }
                }
                catch
                {
                    // If the pipe isn't available, ignore error
                }

                // Then exit this instance
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

            // Create an OpenPipe server to listen for messages from other instances
            StartPipeServer();

            // Run the main form (it will start hidden, but put an icon in the system tray)
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }

        /// <summary>
        /// Start the named pipe server to listen for messages from other instances
        /// </summary>
        private static void StartPipeServer()
        {
            Task.Run(() =>
            {
                while (true)
                {
                    using (var server = new NamedPipeServerStream(pipeName, PipeDirection.In))
                    {
                        server.WaitForConnection();
                        using (var reader = new StreamReader(server))
                        {
                            var command = reader.ReadLine();
                            if (command == "SHOW")
                            {
                                ShowMainWindow();
                            }
                        }
                    }
                }
            });
        }

        /// <summary>
        /// Show the main window
        /// </summary>
        private static void ShowMainWindow()
        {
            // Needs to be run on UI thread
            Application.OpenForms[0]?.BeginInvoke((Action)(() =>
            {
                var form = Application.OpenForms[0];

                if (form?.WindowState == FormWindowState.Minimized)
                    form.WindowState = FormWindowState.Normal;

                form?.Show();
                form?.BringToFront();
                form?.Activate();
            }));
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
                    key.SetValue(Application.ProductName, $"\"{ExecutableFile}\"");
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
            Logger.LogInfo("Starting sync workers...");

            var director = new SyncDirector(Settings.LocalPath);

            foreach (var pattern in Settings.LocalSearchPattern.Split(';', StringSplitOptions.RemoveEmptyEntries))
            {
                if (remoteSyncs.Count > 0)
                {
                    await remoteSyncs[0].DoneMakingFolders;
                }
                try
                {
                    remoteSyncs.Add(new RemoteSync(
                        Settings.RemoteHost, 
                        Settings.RemoteUsername, 
                        DPAPIEncryption.Decrypt(Settings.RemotePassword), 
                        Settings.LocalPath, 
                        Settings.RemotePath, 
                        pattern, 
                        remoteSyncs.Count == 0, 
                        director));

                    Logger.LogInfo($"Started sync worker {remoteSyncs.Count} for pattern {pattern}");
                }
                catch (Exception)
                {
                    Logger.LogError($"Failed to start sync worder for pattern {pattern}");
                }
            }
        }

        public static void StopSync(Action<string> loggerAction)
        {
            Logger.LogInfo($"Stopping {remoteSyncs.Count} sync workers...");

            foreach (var remoteSync in remoteSyncs)
            {
                try
                {
                    remoteSync.Dispose();
                }
                catch { /* Swallow any exceptions */ }
            }

            remoteSyncs.Clear();

            Logger.LogUpdated -= loggerAction;
        }
    }
}