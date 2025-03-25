
using System.Text.RegularExpressions;

namespace SFTPSync
{
    public partial class MainForm : Form
    {
        private NotifyIcon notifyIcon;
        private ContextMenuStrip contextMenu;
        private bool initialUiLoad = false;

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
            if (SFTPSync.Settings == null)
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

            contextMenu.Items.Add(new ToolStripSeparator());

            contextMenu.Items.Add("Exit", null, (s, e) =>
            {
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
            textBoxLocalPath.Text = SFTPSync.Settings.LocalPath;
            textBoxSearchSpec.Text = SFTPSync.Settings.LocalSearchPattern;
            textBoxRemoteHost.Text = SFTPSync.Settings.RemoteHost;
            textBoxRemotePath.Text = SFTPSync.Settings.RemotePath;
            textBoxRemoteUser.Text = SFTPSync.Settings.RemoteUsername;
            textBoxRemotePassword.Text = SFTPSync.Settings.RemotePassword;
            checkStartAtLogin.Checked = SFTPSync.Settings.StartAtLogin;
            checkStartInTray.Checked = SFTPSync.Settings.StartInTray;
            checkBoxAutoStartSync.Checked = SFTPSync.Settings.AutoStartSync;
            initialUiLoad = false;

            enableDisableAutoStart();

            //Set the initial window visibility
            ShowInTaskbar = !checkStartInTray.Checked;
            WindowState = checkStartInTray.Checked ? FormWindowState.Minimized : FormWindowState.Normal;
        }

        public void AddMessage(string message)
        {
            if (listBoxMessages.Items.Count > 1000)
                listBoxMessages.Items.RemoveAt(0);
            listBoxMessages.Items.Add(message);
            listBoxMessages.SelectedIndex = listBoxMessages.Items.Count - 1;
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
                if (SFTPSync.Settings != null)
                {
                    SFTPSync.Settings.LocalPath = textBoxLocalPath.Text;
                }
            }
            enableDisableAutoStart();
        }

        private void textBoxSearchSpec_TextChanged(object sender, EventArgs e)
        {
            if (initialUiLoad)
                return;

            if (String.IsNullOrWhiteSpace(textBoxLocalPath.Text) || searchSpecOK())
            {
                if (SFTPSync.Settings != null)
                {
                    SFTPSync.Settings.LocalSearchPattern = textBoxSearchSpec.Text;
                }
            }
            enableDisableAutoStart();
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

            if (SFTPSync.Settings != null)
            {
                SFTPSync.Settings.RemotePath = textBoxRemotePath.Text;
            }
            enableDisableAutoStart();
        }

        private void textBoxRemoteHost_TextChanged(object sender, EventArgs e)
        {
            if (initialUiLoad)
                return;

            if (SFTPSync.Settings != null)
            {
                SFTPSync.Settings.RemoteHost = textBoxRemoteHost.Text;
            }
            enableDisableAutoStart();
        }

        private void textBoxRemoteUser_TextChanged(object sender, EventArgs e)
        {
            if (initialUiLoad)
                return;

            if (SFTPSync.Settings != null)
            {
                SFTPSync.Settings.RemoteUsername = textBoxRemoteUser.Text;
            }
            enableDisableAutoStart();
        }

        private void textBoxRemotePassword_TextChanged(object sender, EventArgs e)
        {
            if (initialUiLoad)
                return;

            if (SFTPSync.Settings != null)
            {
                SFTPSync.Settings.RemotePassword = textBoxRemotePassword.Text;
            }
            enableDisableAutoStart();
        }

        private void checkStartAtLogin_CheckedChanged(object sender, EventArgs e)
        {
            if (initialUiLoad)
                return;

            if (SFTPSync.Settings != null)
            {
                SFTPSync.Settings.StartAtLogin = checkStartAtLogin.Checked;
            }
        }

        private void checkStartInTray_CheckedChanged(object sender, EventArgs e)
        {
            if (initialUiLoad)
                return;

            if (SFTPSync.Settings != null)
            {
                SFTPSync.Settings.StartInTray = checkStartInTray.Checked;
            }
        }

        private void checkBoxAutoStartSync_CheckedChanged(object sender, EventArgs e)
        {
            if (initialUiLoad)
                return;

            if (SFTPSync.Settings != null)
            {
                SFTPSync.Settings.AutoStartSync = checkBoxAutoStartSync.Checked;
            }
        }

        private void enableDisableAutoStart()
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
            if (allOk && String.IsNullOrWhiteSpace(textBoxRemotePassword.Text))
                allOk = false;

            checkBoxAutoStartSync.Enabled = allOk;
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
    }
}
