
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
        private bool _invalidRemotePathOnLoad = false;
        private string _committedLocalPath = String.Empty;
        private bool _committingLocalPath = false;

        private const string InvalidRemotePathMessage =
            "The remote path must be specified in Linux format (for example /dga1303/1tallent/win), " +
            "not as an OpenVMS directory specification. Characters [ and ] are not allowed.";

        private static bool ContainsVmsBrackets(string? path) =>
            path != null && path.IndexOfAny(['[', ']']) >= 0;

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
            _committedLocalPath = _settings.LocalPath;
            UpdateExclusionControls();

            //Check the local path (against the excluded directories) when leaving the field and when closing the form
            textBoxLocalPath.Validating += (s, e) => CommitLocalPath();
            FormClosing += (s, e) => CommitLocalPath();

            //Remote system settings
            textBoxRemoteHost.Text = _settings.RemoteHost;
            textBoxRemoteUser.Text = _settings.RemoteUsername;

            textBoxRemotePassword.Text = _settings.RemotePassword.Length > 0 ? DPAPIEncryption.Decrypt(_settings.RemotePassword) : String.Empty;

            //A previously saved path may contain an OpenVMS directory spec. If so, blank it
            //out and tell the user once the form is shown.
            if (ContainsVmsBrackets(_settings.RemotePath))
            {
                _settings.RemotePath = String.Empty;
                _invalidRemotePathOnLoad = true;
            }

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
            if (_invalidRemotePathOnLoad)
            {
                MessageBox.Show(
                    "The saved remote path was in OpenVMS directory format and has been cleared.\n\n" + InvalidRemotePathMessage,
                    "Invalid Remote Path",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                textBoxRemotePath.Focus();
            }

            if (_syncRunning)
            {
                MessageBox.Show("Changes will  not take effect until sync is restarted!", "Sync Running", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        // ----- LOCAL SYSTEM CODE -----

        private void textBoxLocalPath_TextChanged(object sender, EventArgs e)
        {
            //The new path is not saved here. It is committed (and checked against the excluded
            //directories) when the user leaves the field, picks a directory, or closes the form,
            //so that we don't react to partially typed paths.
            UpdateExclusionControls();
        }

        /// <summary>
        /// Save the local path from the text box, first checking that the excluded directories still exist under it.
        /// If any do not, the user can either remove those exclusions or reject the new local path (the text box
        /// is then put back to the previous path).
        /// </summary>
        /// <returns>False if the user rejected the new local path, otherwise true</returns>
        private bool CommitLocalPath()
        {
            if (_settings == null || initialUiLoad || _committingLocalPath)
                return true;

            _committingLocalPath = true;
            try
            {
                var newPath = textBoxLocalPath.Text;
                var blank = String.IsNullOrWhiteSpace(newPath);

                //A path that doesn't exist is not saved (as before), and the Add button stays disabled
                if (!blank && !Directory.Exists(newPath))
                    return true;

                if (string.Equals(newPath, _committedLocalPath, StringComparison.OrdinalIgnoreCase))
                    return true;

                if (!blank)
                {
                    var missing = listBox.Items.Cast<string>()
                        .Where(relative => !Directory.Exists(Path.Combine(newPath, relative)))
                        .ToList();

                    if (missing.Count > 0)
                    {
                        var shown = string.Join(Environment.NewLine, missing.Take(10).Select(m => "    " + m));
                        if (missing.Count > 10)
                            shown += Environment.NewLine + $"    ...and {missing.Count - 10} more";

                        var removeButton = new TaskDialogButton("Remove these exclusions");
                        var chooseButton = new TaskDialogButton("Choose a different Local Path");

                        var page = new TaskDialogPage
                        {
                            Caption = "Excluded Directories",
                            Heading = "Some excluded directories do not exist under the new Local Path",
                            Text = $"The new Local Path is:{Environment.NewLine}    {newPath}{Environment.NewLine}{Environment.NewLine}"
                                 + $"These excluded directories are not subdirectories of it:{Environment.NewLine}{shown}",
                            Icon = TaskDialogIcon.Warning,
                            AllowCancel = true,
                            DefaultButton = chooseButton,
                            Buttons = { removeButton, chooseButton }
                        };

                        if (TaskDialog.ShowDialog(this, page) != removeButton)
                        {
                            //Reject the new local path
                            textBoxLocalPath.Text = _committedLocalPath;
                            return false;
                        }

                        foreach (var item in missing)
                            listBox.Items.Remove(item);

                        _settings.ExcludedDirectories = listBox.Items.Cast<string>().ToList();
                    }
                }

                _settings.LocalPath = newPath;
                _committedLocalPath = newPath;
                return true;
            }
            finally
            {
                _committingLocalPath = false;
            }
        }

        private void buttonLocalPath_Click(object sender, EventArgs e)
        {
            //Keep asking until the user picks a local path that is acceptable, or cancels
            while (true)
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

                if (folderDialog.ShowDialog() != DialogResult.OK
                    || string.IsNullOrWhiteSpace(folderDialog.SelectedPath))
                {
                    return;
                }

                textBoxLocalPath.Text = folderDialog.SelectedPath;

                if (CommitLocalPath())
                    return;
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
            //Exclusions are stored relative to the local path, so we need a valid, committed one
            if (!CommitLocalPath())
                return;

            var localPath = textBoxLocalPath.Text;
            if (!IsLocalPathValid())
            {
                UpdateExclusionControls();
                return;
            }

            //Create a directory picker dialog, starting in the local path
            using var directoryPicker = new FolderBrowserDialog
            {
                Description = "Select a directory to exclude",
                InitialDirectory = localPath,
                SelectedPath = localPath
            };

            //Show it and check if the user picked a directory
            if (directoryPicker.ShowDialog() != DialogResult.OK)
                return;

            //The directory must be below the local path. Store it relative to the local path.
            if (!AppSettings.TryGetRelativeSubdirectory(localPath, directoryPicker.SelectedPath, out var newItem))
            {
                MessageBox.Show(
                    $"The excluded directory must be a subdirectory of the Local Path:{Environment.NewLine}{Environment.NewLine}{localPath}",
                    "Invalid Excluded Directory",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            //Already excluded?
            var existing = listBox.Items.Cast<string>()
                .FirstOrDefault(item => string.Equals(item, newItem, StringComparison.OrdinalIgnoreCase));
            if (existing != null)
            {
                listBox.SelectedItem = existing;
                return;
            }

            //Add it and sort the list items alphabetically
            listBox.Items.Add(newItem);
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

        private bool IsLocalPathValid() =>
            !String.IsNullOrWhiteSpace(textBoxLocalPath.Text) && Directory.Exists(textBoxLocalPath.Text);

        /// <summary>
        /// Directories can only be excluded once a valid local path has been specified
        /// </summary>
        private void UpdateExclusionControls()
        {
            btnAdd.Enabled = IsLocalPathValid();
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
            if (initialUiLoad)
                return;

            //The remote path must be a Linux-style path. An OpenVMS directory spec
            //(e.g. $1$DGA1303:[1tallent.WIN]) contains square brackets, and will cause
            //problems if it is allowed to be used.
            if (ContainsVmsBrackets(textBoxRemotePath.Text))
            {
                //Clearing the field re-enters this handler, which stores the empty value.
                textBoxRemotePath.Clear();

                MessageBox.Show(
                    InvalidRemotePathMessage,
                    "Invalid Remote Path",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                textBoxRemotePath.Focus();
                return;
            }

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
