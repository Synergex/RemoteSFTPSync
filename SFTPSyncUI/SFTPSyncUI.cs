
using Microsoft.Win32;
using SFTPSyncLib;
using System.IO.Pipes;
using System.Reflection;
using System.Linq;

namespace SFTPSyncUI
{
    internal static class SFTPSyncUI
    {
        public static string ExecutableFile = String.Empty;

        private static AppSettings? settings;

        private static MainForm? mainForm;

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

            Application.ThreadException += (sender, args) =>
            {
                Logger.LogError($"Unhandled UI exception: {args.Exception.Message}");
            };
            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                if (args.ExceptionObject is Exception ex)
                    Logger.LogError($"Unhandled exception: {ex.Message}");
                else
                    Logger.LogError("Unhandled exception: unknown error");
            };
            TaskScheduler.UnobservedTaskException += (sender, args) =>
            {
                Logger.LogError($"Unobserved task exception: {args.Exception.Message}");
                args.SetObserved();
            };

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
                settings = AppSettings.LoadFromFile();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
                return;
            }

            // Do we have settings?
            if (settings == null)
            {
                MessageBox.Show("Failed to load user settings!", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
                return;
            }

            // Check the run at startup status is correct
            if (settings.StartAtLogin && !IsProgramInStartup())
                AddProgramToStartup();
            else if (!settings.StartAtLogin && IsProgramInStartup())
                RemoveProgramFromStartup();

            // Create an OpenPipe server to listen for messages from other instances
            StartPipeServer();

            // Run the main form (it will start hidden, but put an icon in the system tray)
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(mainForm = new MainForm(settings));
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
            if (mainForm == null)
                return;

            // Needs to be run on UI thread
            mainForm.BeginInvoke((Action)(() =>
            {
                if (mainForm.WindowState == FormWindowState.Minimized)
                    mainForm.WindowState = FormWindowState.Normal;

                mainForm.Show();
                mainForm.BringToFront();
                mainForm.Activate();
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

        public static List<RemoteSync> RemoteSyncWorkers = new List<RemoteSync>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="loggerAction"></param>
        public static async void StartSync(Action<string> loggerAction)
        {
            if (settings == null)
                return;

            Logger.LogUpdated += loggerAction;
            Logger.LogInfo("Starting sync workers...");
            mainForm?.SetStatusBarText("Performing initial sync...");

            Logger.LogInfo("Sample info message");
            Logger.LogWarnig("Sample warning message");
            Logger.LogError("Sample error message");

            var capturedSettings = settings;
            var capturedMainForm = mainForm;
            var setStatus = (string text) =>
            {
                if (capturedMainForm == null)
                    return;
                capturedMainForm.BeginInvoke((Action)(() => capturedMainForm.SetStatusBarText(text)));
            };

            try
            {
                await Task.Run(async () =>
                {
                    var director = new SyncDirector(capturedSettings.LocalPath, capturedSettings.DeleteEnabled);

                    var patterns = capturedSettings.LocalSearchPattern
                        .Split(';', StringSplitOptions.RemoveEmptyEntries)
                        .Select(pattern => pattern.Trim())
                        .Where(pattern => pattern.Length > 0)
                        .ToArray();

                    if (patterns.Length == 0)
                    {
                        Logger.LogError("No valid search patterns were configured.");
                        setStatus("Invalid search patterns");
                        return;
                    }

                    var initialSyncTcs = new TaskCompletionSource<object?>();
                    var initialSyncTask = initialSyncTcs.Task;

                    foreach (var pattern in patterns)
                    {
                        try
                        {
                            RemoteSyncWorkers.Add(new RemoteSync(
                                capturedSettings.RemoteHost,
                                capturedSettings.RemoteUsername,
                                DPAPIEncryption.Decrypt(capturedSettings.RemotePassword),
                                capturedSettings.LocalPath,
                                capturedSettings.RemotePath,
                                pattern,
                                director,
                                capturedSettings.ExcludedDirectories,
                                initialSyncTask,
                                capturedSettings.DeleteEnabled,
                                RemoteSyncWorkers.Count == 0));

                            Logger.LogInfo($"Started sync worker {RemoteSyncWorkers.Count} for pattern {pattern}");
                        }
                        catch (Exception)
                        {
                            Logger.LogError($"Failed to start sync worker for pattern {pattern}");
                        }
                    }

                    var connectTasks = new List<Task>();

                    Logger.LogInfo($"Establishing SFTP connections for {RemoteSyncWorkers.Count} workers");

                    for (int i = 0; i < RemoteSyncWorkers.Count; i++)
                    {
                        connectTasks.Add(RemoteSyncWorkers[i].ConnectAsync());
                    }
                    await Task.WhenAll(connectTasks);

                    Task runInitialSyncTask;

                    try
                    {
                        runInitialSyncTask = RemoteSync.RunSharedInitialSyncAsync(
                            capturedSettings.RemoteHost,
                            capturedSettings.RemoteUsername,
                            DPAPIEncryption.Decrypt(capturedSettings.RemotePassword),
                            capturedSettings.LocalPath,
                            capturedSettings.RemotePath,
                            patterns,
                            capturedSettings.ExcludedDirectories,
                            patterns.Length);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError($"Failed to start initial sync. Exception: {ex.Message}");
                        setStatus("Initial sync failed");
                        return;
                    }

                    await runInitialSyncTask.ContinueWith(t =>
                    {
                        if (t.IsFaulted && t.Exception != null)
                        {
                            initialSyncTcs.TrySetException(t.Exception.InnerExceptions);
                        }
                        else if (t.IsCanceled)
                        {
                            initialSyncTcs.TrySetCanceled();
                        }
                        else
                        {
                            initialSyncTcs.TrySetResult(null);
                        }
                    }, TaskScheduler.Default);

                    //Wait for all sync workers to finish initial sync then tell the user

                    try
                    {
                        await runInitialSyncTask;
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError($"Initial sync failed. Exception: {ex.Message}");
                        setStatus("Initial sync failed");
                        return;
                    }

                    Logger.LogInfo("Initial sync complete, real-time sync active");
                    setStatus("Real time sync active");
                });
            }
            catch (Exception ex)
            {
                Logger.LogError($"Failed to start sync. Exception: {ex.Message}");
                mainForm?.SetStatusBarText("Sync failed");
                return;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="loggerAction"></param>
        public static void StopSync(Action<string> loggerAction)
        {
            Logger.LogInfo("Stopping sync...");
            mainForm?.SetStatusBarText("Stopping sync...");

            foreach (var remoteSync in RemoteSyncWorkers)
            {
                try
                {
                    remoteSync.Dispose();
                }
                catch { /* Swallow any exceptions */ }
            }

            RemoteSyncWorkers.Clear();

            Logger.LogUpdated -= loggerAction;

            Logger.LogInfo("Sync stopped");
            mainForm?.SetStatusBarText("Sync stopped");
        }
    }
}
