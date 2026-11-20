
using Renci.SshNet;
using SFTPSyncLib;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows.Forms;

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
            WindowState = _settings.StartInTray ? FormWindowState.Minimized : FormWindowState.Normal;
            ShowInTaskbar = WindowState != FormWindowState.Minimized;

            //Log list: a selection is one row or a range of adjacent rows, and can be copied
            SetupLogListSelection();
        }

        // ----- LOG LIST SELECTION AND COPY -----

        private bool _adjustingLogSelection = false;

        private void SetupLogListSelection()
        {
            var logContextMenu = new ContextMenuStrip();
            var copyItem = new ToolStripMenuItem("&Copy")
            {
                ShortcutKeyDisplayString = "Ctrl+C"
            };
            copyItem.Click += (s, e) => CopySelectedLogRows();
            logContextMenu.Items.Add(copyItem);

            //Only offer the menu when there is something selected
            logContextMenu.Opening += (s, e) => e.Cancel = listBoxMessages.SelectedItems.Count == 0;
            listBoxMessages.ContextMenuStrip = logContextMenu;

            listBoxMessages.SelectedIndexChanged += (s, e) => EnsureContiguousLogSelection();

            //Right-clicking a row that is not selected selects just that row, like most list controls
            listBoxMessages.MouseDown += (s, e) =>
            {
                if (e.Button != MouseButtons.Right)
                    return;

                int index = listBoxMessages.IndexFromPoint(e.Location);
                if (index >= 0 && !listBoxMessages.GetSelected(index))
                {
                    listBoxMessages.ClearSelected();
                    listBoxMessages.SetSelected(index, true);
                }
            };

            listBoxMessages.KeyDown += (s, e) =>
            {
                if (e.Control && e.KeyCode == Keys.C)
                {
                    CopySelectedLogRows();
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                }
            };
        }

        /// <summary>
        /// Extended selection allows Ctrl+click to build a scattered selection. Only a single row or a range of
        /// adjacent rows is allowed, so fill any gap between the first and last selected rows.
        /// </summary>
        private void EnsureContiguousLogSelection()
        {
            if (_adjustingLogSelection || listBoxMessages.SelectedIndices.Count < 2)
                return;

            int first = listBoxMessages.SelectedIndices[0];
            int last = listBoxMessages.SelectedIndices[listBoxMessages.SelectedIndices.Count - 1];

            if (last - first + 1 == listBoxMessages.SelectedIndices.Count)
                return;

            _adjustingLogSelection = true;
            try
            {
                listBoxMessages.BeginUpdate();
                for (int i = first; i <= last; i++)
                    listBoxMessages.SetSelected(i, true);
            }
            finally
            {
                listBoxMessages.EndUpdate();
                _adjustingLogSelection = false;
            }
        }

        /// <summary>
        /// Copy the selected log rows to the clipboard, one per line, in the order they appear in the list
        /// </summary>
        private void CopySelectedLogRows()
        {
            if (listBoxMessages.SelectedItems.Count == 0)
                return;

            var text = string.Join(Environment.NewLine, listBoxMessages.SelectedItems.Cast<object>().Select(item => item.ToString()));

            try
            {
                Clipboard.SetText(text);
            }
            catch (Exception ex)
            {
                //The clipboard can be locked by another application
                MessageBox.Show($"Failed to copy to the clipboard: {ex.Message}", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
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
            // If the user is looking at a selection, keep the view where it is (the new row pushes everything
            // down by one), otherwise keep the newest message at the top
            int topIndex = listBoxMessages.TopIndex;
            bool keepView = listBoxMessages.SelectedItems.Count > 0 && topIndex > 0;

            listBoxMessages.Items.Insert(0, message);

            listBoxMessages.TopIndex = keepView ? topIndex + 1 : 0;

            // Optional: trim excess from UI if _log did a dequeue
            if (listBoxMessages.Items.Count > 1000)
            {
                listBoxMessages.Items.RemoveAt(listBoxMessages.Items.Count - 1);
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
                mnuFileStopSync.Enabled = true;
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

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            ShowInTaskbar = WindowState != FormWindowState.Minimized && Visible;
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);

            // Auto-start after handle exists so BeginInvoke works even when minimized.
            if (checkCanStartSync() && _settings.AutoStartSync)
                startSync();
        }

        public void SetStatusBarText(string text)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => Panel1.Text = text));
            }
            else
            {
                Panel1.Text = text;
            }
        }

        private void mnuToolsOptions_Click(object sender, EventArgs e)
        {
            var dialog = new SettingsForm(_settings, syncRunning);
            dialog.ShowDialog(this);

            if (!syncRunning)
                checkCanStartSync();

        }

        private void ListBoxMessages_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0 || e.Font == null)
                return;

            string? itemText = listBoxMessages.Items[e.Index].ToString();

            if (itemText == null)
                return;

            Color textColor = Color.Black;

            if (itemText.Contains(" ERR: "))
                textColor = Color.Red;
            else if (itemText.Contains(" WRN: "))
                textColor = Color.Orange;

            e.DrawBackground();

            TextRenderer.DrawText(
                e.Graphics,
                itemText,
                e.Font,
                e.Bounds,
                textColor,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter
            );

            e.DrawFocusRectangle();
        }
    }
}
