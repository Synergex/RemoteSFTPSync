
using SFTPSyncLib;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace SFTPSyncUI
{
    public partial class MainForm : Form
    {
        private NotifyIcon notifyIcon;
        private ContextMenuStrip contextMenu;
        private bool initialUiLoad = false;
        private bool syncRunning = false;

        public MainForm()
        {
            InitializeComponent();

            // Create a system tray icon
            contextMenu = new ContextMenuStrip();
            notifyIcon = new NotifyIcon
            {
                Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath),
                Text = Application.ProductName,
                Visible = true
            };

            // Can't happen, but suppresses "might be null" warnings
            if (SFTPSyncUI.Settings == null)
            {
                MessageBox.Show("Settings not loaded", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
                return;
            }

            // Add context menu items

            contextMenu.Items.Add("Show window", null, (s, e) =>
            {
                Show();
                WindowState = FormWindowState.Normal;
                ShowInTaskbar = true;
            });

            // Conditionally add the Help item

            // Path when installed
            string helpFilePath = Path.Combine(Application.StartupPath, "SFTPSync.chm");

            if (!File.Exists(helpFilePath))
            {
                // Path when in development environment
                helpFilePath = Path.Combine(Application.StartupPath, "docs", "SFTPSync.chm");
            }

            if (File.Exists(helpFilePath))
            {
                contextMenu.Items.Add("Help", null, (s, e) =>
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
                });
            }

            contextMenu.Items.Add(new ToolStripSeparator());

            contextMenu.Items.Add("Exit", null, (s, e) =>
            {
                //The user asked us to close the application

                // Stop the sync process if it is running
                if (syncRunning)
                {
                    stopSync();
                }

                //And close the application
                notifyIcon.Visible = false;
                Application.Exit();
            });

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

            //Load the current settings
            initialUiLoad = true;
            textBoxLocalPath.Text = SFTPSyncUI.Settings.LocalPath;
            textBoxSearchSpec.Text = SFTPSyncUI.Settings.LocalSearchPattern;
            textBoxRemoteHost.Text = SFTPSyncUI.Settings.RemoteHost;
            textBoxRemotePath.Text = SFTPSyncUI.Settings.RemotePath;
            textBoxRemoteUser.Text = SFTPSyncUI.Settings.RemoteUsername;
            textBoxRemotePassword.Text = DPAPIEncryption.Decrypt(SFTPSyncUI.Settings.RemotePassword);
            checkStartAtLogin.Checked = SFTPSyncUI.Settings.StartAtLogin;
            checkStartInTray.Checked = SFTPSyncUI.Settings.StartInTray;
            checkBoxAutoStartSync.Checked = SFTPSyncUI.Settings.AutoStartSync;
            initialUiLoad = false;

            //Set the initial window visibility
            ShowInTaskbar = !checkStartInTray.Checked;
            WindowState = checkStartInTray.Checked ? FormWindowState.Minimized : FormWindowState.Normal;

            // Can we and should we start the sync process now?
            if (enableDisableStartSync() && checkBoxAutoStartSync.Checked)
            {
                startSync();
            }
        }

        private void buttonLocalPath_Click(object sender, EventArgs e)
        {
            using var folderDialog = new FolderBrowserDialog
            {
                Description = "Local path",
                UseDescriptionForTitle = true,
                ShowNewFolderButton = true,
                InitialDirectory = !String.IsNullOrWhiteSpace(textBoxLocalPath.Text) && Directory.Exists(textBoxLocalPath.Text)
                ? textBoxLocalPath.Text
                : Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            };

            if (folderDialog.ShowDialog() == DialogResult.OK && !string.IsNullOrWhiteSpace(folderDialog.SelectedPath))
            {
                textBoxLocalPath.Text = folderDialog.SelectedPath;
            }
        }

        private void textBoxLocalPath_TextChanged(object sender, EventArgs e)
        {
            if (initialUiLoad)
                return;

            if (String.IsNullOrWhiteSpace(textBoxLocalPath.Text) || Directory.Exists(textBoxLocalPath.Text))
            {
                if (SFTPSyncUI.Settings != null)
                {
                    SFTPSyncUI.Settings.LocalPath = textBoxLocalPath.Text;
                }
            }
            enableDisableStartSync();
        }

        private void textBoxSearchSpec_TextChanged(object sender, EventArgs e)
        {
            if (initialUiLoad)
                return;

            if (String.IsNullOrWhiteSpace(textBoxLocalPath.Text) || searchSpecOK())
            {
                if (SFTPSyncUI.Settings != null)
                {
                    SFTPSyncUI.Settings.LocalSearchPattern = textBoxSearchSpec.Text;
                }
            }
            enableDisableStartSync();
        }

        private bool searchSpecOK()
        {
            //TODO: Need real validation here!


            return !String.IsNullOrWhiteSpace(textBoxSearchSpec.Text);
        }

        private void textBoxRemotePath_TextChanged(object sender, EventArgs e)
        {
            if (initialUiLoad)
                return;

            if (SFTPSyncUI.Settings != null)
            {
                SFTPSyncUI.Settings.RemotePath = textBoxRemotePath.Text;
            }
            enableDisableStartSync();
        }

        private void textBoxRemoteHost_TextChanged(object sender, EventArgs e)
        {
            if (initialUiLoad)
                return;

            if (SFTPSyncUI.Settings != null)
            {
                SFTPSyncUI.Settings.RemoteHost = textBoxRemoteHost.Text;
            }
            enableDisableStartSync();
        }

        private void textBoxRemoteUser_TextChanged(object sender, EventArgs e)
        {
            if (initialUiLoad)
                return;

            if (SFTPSyncUI.Settings != null)
            {
                SFTPSyncUI.Settings.RemoteUsername = textBoxRemoteUser.Text;
            }
            enableDisableStartSync();
        }

        private void textBoxRemotePassword_TextChanged(object sender, EventArgs e)
        {
            if (initialUiLoad)
                return;

            if (SFTPSyncUI.Settings != null)
            {
                SFTPSyncUI.Settings.RemotePassword = DPAPIEncryption.Encrypt(textBoxRemotePassword.Text);
            }
            enableDisableStartSync();
        }

        private void checkStartAtLogin_CheckedChanged(object sender, EventArgs e)
        {
            if (initialUiLoad)
                return;

            if (SFTPSyncUI.Settings != null)
            {
                SFTPSyncUI.Settings.StartAtLogin = checkStartAtLogin.Checked;
            }
        }

        private void checkStartInTray_CheckedChanged(object sender, EventArgs e)
        {
            if (initialUiLoad)
                return;

            if (SFTPSyncUI.Settings != null)
            {
                SFTPSyncUI.Settings.StartInTray = checkStartInTray.Checked;
            }
        }

        private void checkBoxAutoStartSync_CheckedChanged(object sender, EventArgs e)
        {
            if (initialUiLoad)
                return;

            if (SFTPSyncUI.Settings != null)
            {
                SFTPSyncUI.Settings.AutoStartSync = checkBoxAutoStartSync.Checked;
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

        private bool enableDisableStartSync()
        {
            bool allOk = true;

            //Local path validation (must be non-blank and a valid directory)

            if (String.IsNullOrWhiteSpace(textBoxLocalPath.Text) || !Directory.Exists(textBoxLocalPath.Text))
                allOk = false;

            //Search spec validation (has to be a semi-colon delimited list of file extensions)
            if (allOk && !isValidSearchSpec(textBoxSearchSpec.Text))
                allOk = false;

            //Remote path validation (just has to be non-null)
            if (allOk && String.IsNullOrWhiteSpace(textBoxRemotePath.Text))
                allOk = false;

            //Remote host validation (just has to be non-null)
            if (allOk && String.IsNullOrWhiteSpace(textBoxRemoteHost.Text))
                allOk = false;

            //Remote username validation (just has to be non-null)
            if (allOk && String.IsNullOrWhiteSpace(textBoxRemoteUser.Text))
                allOk = false;

            //Remote password validation (just has to be non-null)
            if (allOk && String.IsNullOrWhiteSpace(DPAPIEncryption.Decrypt(textBoxRemotePassword.Text)))
                allOk = false;

            buttonStartStopSync.Enabled = allOk;

            return allOk;
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
        private void buttonStartStopSync_Click(object sender, EventArgs e)
        {
            if (syncRunning)
            {
                stopSync();
            }
            else
            {
                startSync();
            }
        }

        private void startSync()
        {
            syncRunning = true;
            configureUI();
            SFTPSyncUI.StartSync(AppendLog);
        }

        private void stopSync()
        {
            syncRunning = false;
            configureUI();
            SFTPSyncUI.StopSync(AppendLog);
        }

        private void configureUI()
        {
            textBoxLocalPath.Enabled = !syncRunning;
            buttonLocalPath.Enabled = !syncRunning;
            textBoxSearchSpec.Enabled = !syncRunning;
            textBoxRemotePath.Enabled = !syncRunning;
            textBoxRemoteHost.Enabled = !syncRunning;
            textBoxRemoteUser.Enabled = !syncRunning;
            textBoxRemotePassword.Enabled = !syncRunning;

            buttonStartStopSync.Text = syncRunning ? "&Stop Sync" : "&Start Sync";
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

        private void checkBoxShowPassword_CheckedChanged(object sender, EventArgs e)
        {
            textBoxRemotePassword.UseSystemPasswordChar = !checkBoxShowPassword.Checked;
        }
    }
}
