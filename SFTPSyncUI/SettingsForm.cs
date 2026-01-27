
using Renci.SshNet;
using SFTPSyncLib;
using System.Diagnostics;
using System.Security.Policy;
using System.Text.RegularExpressions;

namespace SFTPSyncUI
{
    public partial class SettingsForm : Form
    {
        private AppSettings? _settings = null;
        private bool initialUiLoad = false;
        private bool _syncRunning = false;

        public SettingsForm(AppSettings settings, bool syncRunning)
        {
            InitializeComponent();
            _settings = settings;
            _syncRunning = syncRunning;

            //Load current settings.
            initialUiLoad = true;

            //Local system settings
            textBoxLocalPath.Text = _settings.LocalPath;
            textBoxSearchSpec.Text = _settings.LocalSearchPattern;
            listBox.Items.AddRange(_settings.ExcludedDirectories.ToArray());

            //Remote system settings
            textBoxRemoteHost.Text = _settings.RemoteHost;
            textBoxRemoteUser.Text = _settings.RemoteUsername;

            textBoxRemotePassword.Text = _settings.RemotePassword.Length > 0 ? DPAPIEncryption.Decrypt(_settings.RemotePassword) : String.Empty;

            textBoxRemotePath.Text = _settings.RemotePath;
            buttonVerifyAccess.Enabled = !_settings.AccessVerified;

            chkSupportDelete.Checked = _settings.DeleteEnabled;

            //Application settings
            checkStartAtLogin.Checked = _settings.StartAtLogin;
            checkStartInTray.Checked = _settings.StartInTray;
            checkBoxAutoStartSync.Checked = _settings.AutoStartSync;

            //Done loading current settings
            initialUiLoad = false;

        }

        private void SettingsForm_Shown(object sender, EventArgs e)
        {
            if (_syncRunning)
            {
                MessageBox.Show("Changes will  not take effect until sync is restarted!", "Sync Running", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        // ----- LOCAL SYSTEM CODE -----

        private void textBoxLocalPath_TextChanged(object sender, EventArgs e)
        {
            if (!initialUiLoad
                && (String.IsNullOrWhiteSpace(textBoxLocalPath.Text)
                || Directory.Exists(textBoxLocalPath.Text)))
            {
                _settings?.LocalPath = textBoxLocalPath.Text;
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

            if (folderDialog.ShowDialog() == DialogResult.OK
                && !string.IsNullOrWhiteSpace(folderDialog.SelectedPath))
            {
                textBoxLocalPath.Text = folderDialog.SelectedPath;
            }
        }

        private void textBoxSearchSpec_TextChanged(object sender, EventArgs e)
        {
            if (!initialUiLoad
                && (String.IsNullOrWhiteSpace(textBoxLocalPath.Text)
                || searchSpecOK()))
            {
                _settings?.LocalSearchPattern = textBoxSearchSpec.Text;
            }
        }

        private bool searchSpecOK()
        {
            //TODO: Need real validation here!
            //Not sure if isValidSearchSpec works, it wasnt being used before

            return !String.IsNullOrWhiteSpace(textBoxSearchSpec.Text)
                && isValidSearchSpec(textBoxSearchSpec.Text);
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


        // ----- LOCAL SYSTEM EXCLUDED DIRECTORIES CODE -----

        /// <summary>
        /// The user clicked the add button, so show a directory picker dialog
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnAdd_Click(object sender, EventArgs e)
        {
            //Create a directory picker dialog
            var directoryPicker = new FolderBrowserDialog
            {
                Description = "Select a directory to exclude"
            };

            //Show it and check if the user picked a directory
            if (directoryPicker.ShowDialog() == DialogResult.OK)
            {
                //Get the selected directory and add it to the list
                string newItem = directoryPicker.SelectedPath;
                listBox.Items.Add(newItem);

                //Sort the list items alphabetically
                var items = listBox.Items.Cast<string>().OrderBy(item => item).ToList();
                listBox.Items.Clear();
                foreach (var item in items)
                {
                    listBox.Items.Add(item);
                }

                // Select the newly added item in the list
                int index = listBox.Items.IndexOf(newItem);
                if (index >= 0)
                {
                    listBox.SelectedIndex = index;
                }

                //And save the new excluded directories to the settings
                _settings?.ExcludedDirectories = listBox.Items.Cast<string>().ToList();
            }
        }

        /// <summary>
        /// The user clicked the remove button, so remove the selected item(s) from the list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnRemove_Click(object sender, EventArgs e)
        {
            //The user clicked the remove button, so remove the selected item(s)
            foreach (var item in listBox.SelectedItems.Cast<string>().ToList())
            {
                listBox.Items.Remove(item);
            }

            //And save the new excluded directories to the settings
            _settings?.ExcludedDirectories = listBox.Items.Cast<string>().ToList();
        }

        /// <summary>
        /// The user changed the selection in the list box, so enable or disable the remove button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            //Enable the remove button only if there are selected items
            btnRemove.Enabled = listBox.SelectedItems.Count > 0;
        }

        // ----- REMOTE SYSTEM CODE -----

        private void textBoxRemoteHost_TextChanged(object sender, EventArgs e)
        {
            if (!initialUiLoad
                && _settings != null
                && !_settings.RemoteHost.Equals(textBoxRemoteHost.Text))
            {
                _settings.RemoteHost = textBoxRemoteHost.Text;
                _settings.AccessVerified = false;
                buttonVerifyAccess.Enabled = true;
            }
        }

        private void textBoxRemoteUser_TextChanged(object sender, EventArgs e)
        {
            if (!initialUiLoad
                && _settings != null
                && !_settings.RemoteUsername.Equals(textBoxRemoteUser.Text))
            {
                _settings.RemoteUsername = textBoxRemoteUser.Text;
                _settings.AccessVerified = false;
                buttonVerifyAccess.Enabled = true;
            }
        }

        private void textBoxRemotePassword_TextChanged(object sender, EventArgs e)
        {
            if (!initialUiLoad
                && _settings != null
                && !_settings.RemotePassword.Equals(DPAPIEncryption.Encrypt(textBoxRemotePassword.Text)))
            {
                _settings.RemotePassword = DPAPIEncryption.Encrypt(textBoxRemotePassword.Text);
                _settings.AccessVerified = false;
                buttonVerifyAccess.Enabled = true;
            }
        }
        private void textBoxRemotePath_TextChanged(object sender, EventArgs e)
        {
            if (!initialUiLoad)
                _settings?.RemotePath = textBoxRemotePath.Text;
        }

        // ----- APPLICATION SETTINGS CODE -----

        private void checkStartAtLogin_CheckedChanged(object sender, EventArgs e)
        {
            if (!initialUiLoad)
                _settings?.StartAtLogin = checkStartAtLogin.Checked;
        }

        private void checkStartInTray_CheckedChanged(object sender, EventArgs e)
        {
            if (!initialUiLoad)
                _settings?.StartInTray = checkStartInTray.Checked;
        }

        private void checkBoxAutoStartSync_CheckedChanged(object sender, EventArgs e)
        {
            if (!initialUiLoad)
                _settings?.AutoStartSync = checkBoxAutoStartSync.Checked;
        }
        private void chkSupportDelete_CheckedChanged(object sender, EventArgs e)
        {
            if (!initialUiLoad)
                _settings?.DeleteEnabled = chkSupportDelete.Checked;
        }

        private void checkBoxShowPassword_CheckedChanged(object sender, EventArgs e)
        {
            textBoxRemotePassword.UseSystemPasswordChar = !checkBoxShowPassword.Checked;
        }

        private void buttonVerifyAccess_Click(object sender, EventArgs e)
        {
            //We get here if any of remote host, user and password all have values, and
            //one or more of them have changed, and the user clicked the "Verify Access"
            //button. We'll verify by attempting to make an SFTP connection to the remote
            //system using the credentials provided.

            using (SftpClient sftp = new SftpClient(textBoxRemoteHost.Text, textBoxRemoteUser.Text, textBoxRemotePassword.Text))
            {
                try
                {
                    Cursor.Current = Cursors.WaitCursor;
                    sftp.Connect();
                    Cursor.Current = Cursors.Default;
                    MessageBox.Show("Access verified.", "Access Check", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    _settings?.AccessVerified = true;
                    buttonVerifyAccess.Enabled = false;
                }
                catch (Exception)
                {
                    Cursor.Current = Cursors.Default;
                    MessageBox.Show("Failed to verify access.", "Access Check", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    if (sftp != null && sftp.IsConnected)
                    {
                        sftp.Disconnect();
                    }
                }
            }

        }

        // ----- END OF FORM CODE -----

        /// <summary>
        /// The user clicked the close button, so close the form
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
