
using Renci.SshNet;
using SFTPSyncLib;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace SFTPSyncUI
{
    public partial class MainForm : Form
    {
        private AppSettings _settings;
        private NotifyIcon notifyIcon;
        private ContextMenuStrip contextMenu;
        private bool syncRunning = false;
        private string helpFilePath = string.Empty;

        public MainForm(AppSettings settings)
        {
            InitializeComponent();
            _settings = settings;

            // Create a system tray icon

            contextMenu = new ContextMenuStrip();
            notifyIcon = new NotifyIcon
            {
                Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath),
                Text = Application.ProductName,
                Visible = true
            };

            // Add context menu items

            contextMenu.Items.Add("Show window", null, (s, e) =>
            {
                Show();
                WindowState = FormWindowState.Normal;
                ShowInTaskbar = true;
            });

            // Conditionally add the Help item

            // Path when installed
            helpFilePath = Path.Combine(Application.StartupPath, "SFTPSync.chm");

            if (!File.Exists(helpFilePath))
            {
                // Path when in development environment
                helpFilePath = Path.Combine(Application.StartupPath, "docs", "SFTPSync.chm");
            }

            if (File.Exists(helpFilePath))
            {
                contextMenu.Items.Add("Help", null, (s, e) => { ShowHelp(); });
                mnuHelpView.Enabled = true;
            }

            contextMenu.Items.Add(new ToolStripSeparator());

            contextMenu.Items.Add("Exit", null, (s, e) => { StopApplication(); });

            // Assign the context menu to the notify icon
            notifyIcon.ContextMenuStrip = contextMenu;

            // Handle double-click
            notifyIcon.DoubleClick += (s, e) =>
            {
                // Show the main window
                Show();
                WindowState = FormWindowState.Normal;
                ShowInTaskbar = true;
            };

            //Set the initial window visibility
            ShowInTaskbar = _settings.StartInTray;
            WindowState = _settings.StartInTray ? FormWindowState.Minimized : FormWindowState.Normal;

            // Can we and should we start the sync process now?
            if (checkCanStartSync() && _settings.AutoStartSync)
                startSync();
        }
        private void mnuFileExit_Click(object sender, EventArgs e)
        {
            //The user asked us to close the application
            StopApplication();
        }

        private void mnuHelpView_Click(object sender, EventArgs e)
        {
            ShowHelp();
        }

        private void mnuHelpAbout_Click(object sender, EventArgs e)
        {
            ShowAbout();
        }

        private void StopApplication()
        {
            //The user asked us to close the application

            if (syncRunning)
            {
                if (MessageBox.Show($"Stop sync and close?",
                    Application.ProductName,
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button2) == DialogResult.No)
                {
                    return;
                }

                //Stop the sync process
                stopSync();
            }

            //Close the application
            notifyIcon.Visible = false;
            Application.Exit();
        }

        private void ShowAbout()
        {
            var dlg = new AboutForm();
            dlg.ShowDialog();
        }

        private void ShowHelp()
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "hh.exe",
                    Arguments = $"\"{helpFilePath}\"",
                    UseShellExecute = false
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unable to open help file. {ex.Message}",
                    Application.ProductName,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void AppendLog(string message)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => AddMessage(message)));
            }
            else
            {
                AddMessage(message);
            }
        }

        private void AddMessage(string message)
        {
            listBoxMessages.Items.Add(message);

            // Optional: auto-scroll to bottom
            listBoxMessages.TopIndex = listBoxMessages.Items.Count - 1;

            // Optional: trim excess from UI if _log did a dequeue
            if (listBoxMessages.Items.Count > 1000)
            {
                listBoxMessages.Items.RemoveAt(0);
            }
        }

        private bool checkCanStartSync()
        {
            if (_settings == null)
                return false;

            var canStart =
                //Local settings OK
                !String.IsNullOrWhiteSpace(_settings.LocalPath) && Directory.Exists(_settings.LocalPath)
                && isValidSearchSpec(_settings.LocalSearchPattern)
                //Remote settings OK
                && !String.IsNullOrWhiteSpace(_settings.RemotePath)
                && !String.IsNullOrWhiteSpace(_settings.RemoteHost)
                && !String.IsNullOrWhiteSpace(_settings.RemoteUsername)
                && !String.IsNullOrWhiteSpace(_settings.RemotePassword)
                && _settings.AccessVerified;

            mnuFileStartSync.Enabled = canStart;

            return canStart;
        }

        public static bool isValidSearchSpec(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return false;

            // Split the input by semicolon
            var patterns = input.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            if (patterns.Length == 0)
                return false;

            // Define a regex for validating wildcard file specs (e.g., *.txt, data_??.csv)
            var wildcardRegex = new Regex(@"^[^<>:""/\\|?*]*[\*\?][^<>:""/\\|]*\.[a-z0-9\*\?]+$", RegexOptions.IgnoreCase);

            // Check each pattern
            return patterns.All(p => wildcardRegex.IsMatch(p));
        }

        private void mnuFileStartSync_Click(object sender, EventArgs e)
        {
            startSync();
        }

        private void mnuFileStopSync_Click(object sender, EventArgs e)
        {
            stopSync();
        }

        private void startSync()
        {
            if (!syncRunning)
            {
                mnuFileStartSync.Enabled = false;
                //TODO: Can't currently enable stop sync because it crashes the app
                //mnuFileStopSync.Enabled = true; 
                syncRunning = true;
                SFTPSyncUI.StartSync(AppendLog);
            }
        }

        private void stopSync()
        {
            if (syncRunning)
            {
                SFTPSyncUI.StopSync(AppendLog);
                syncRunning = false;
                mnuFileStartSync.Enabled = true;
                mnuFileStopSync.Enabled = false;
            }
        }

        /// <summary>
        /// The user clicked the X button. Hide the window and minimize to the system tray
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Check if the user clicked the X button
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                Hide();
                ShowInTaskbar = false;
            }
        }

        public void SetStatusBarText(string text)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => StatusBar.Items[0].Text = text));
            }
            else
            {
                StatusBar.Items[0].Text = text;
            }
        }

        private void mnuToolsOptions_Click(object sender, EventArgs e)
        {
            var dialog = new SettingsForm(_settings,syncRunning);
            dialog.ShowDialog(this);

            if (!syncRunning)
                checkCanStartSync();

        }
    }
}
