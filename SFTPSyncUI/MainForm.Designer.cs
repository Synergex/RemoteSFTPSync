
namespace SFTPSyncUI
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            checkStartAtLogin = new CheckBox();
            checkStartInTray = new CheckBox();
            groupBoxSettings = new GroupBox();
            btnExclusions = new Button();
            buttonVerifyAccess = new Button();
            buttonStartStopSync = new Button();
            checkBoxShowPassword = new CheckBox();
            checkBoxAutoStartSync = new CheckBox();
            textBoxRemotePath = new TextBox();
            labelRemotePath = new Label();
            textBoxRemotePassword = new TextBox();
            labelRemotePassword = new Label();
            textBoxRemoteUser = new TextBox();
            labelRemoteUser = new Label();
            textBoxRemoteHost = new TextBox();
            labelRemoteHost = new Label();
            textBoxSearchSpec = new TextBox();
            labelSearchSpec = new Label();
            buttonLocalPath = new Button();
            textBoxLocalPath = new TextBox();
            labelLocalPath = new Label();
            listBoxMessages = new ListBox();
            menuStrip1 = new MenuStrip();
            mnuFile = new ToolStripMenuItem();
            mnuFileExit = new ToolStripMenuItem();
            mnuHelp = new ToolStripMenuItem();
            mnuHelpView = new ToolStripMenuItem();
            mnuHelpAbout = new ToolStripMenuItem();
            StatusBar = new StatusStrip();
            Panel1 = new ToolStripStatusLabel();
            groupBoxSettings.SuspendLayout();
            menuStrip1.SuspendLayout();
            StatusBar.SuspendLayout();
            SuspendLayout();
            // 
            // checkStartAtLogin
            // 
            checkStartAtLogin.AutoSize = true;
            checkStartAtLogin.Location = new Point(109, 130);
            checkStartAtLogin.Name = "checkStartAtLogin";
            checkStartAtLogin.Size = new Size(117, 24);
            checkStartAtLogin.TabIndex = 4;
            checkStartAtLogin.Text = "Start at &login";
            checkStartAtLogin.UseVisualStyleBackColor = true;
            checkStartAtLogin.CheckedChanged += checkStartAtLogin_CheckedChanged;
            // 
            // checkStartInTray
            // 
            checkStartInTray.AutoSize = true;
            checkStartInTray.Location = new Point(232, 130);
            checkStartInTray.Name = "checkStartInTray";
            checkStartInTray.Size = new Size(156, 24);
            checkStartInTray.TabIndex = 5;
            checkStartInTray.Text = "Start in system &tray";
            checkStartInTray.UseVisualStyleBackColor = true;
            checkStartInTray.CheckedChanged += checkStartInTray_CheckedChanged;
            // 
            // groupBoxSettings
            // 
            groupBoxSettings.Controls.Add(btnExclusions);
            groupBoxSettings.Controls.Add(buttonVerifyAccess);
            groupBoxSettings.Controls.Add(buttonStartStopSync);
            groupBoxSettings.Controls.Add(checkBoxShowPassword);
            groupBoxSettings.Controls.Add(checkBoxAutoStartSync);
            groupBoxSettings.Controls.Add(textBoxRemotePath);
            groupBoxSettings.Controls.Add(labelRemotePath);
            groupBoxSettings.Controls.Add(textBoxRemotePassword);
            groupBoxSettings.Controls.Add(labelRemotePassword);
            groupBoxSettings.Controls.Add(textBoxRemoteUser);
            groupBoxSettings.Controls.Add(labelRemoteUser);
            groupBoxSettings.Controls.Add(textBoxRemoteHost);
            groupBoxSettings.Controls.Add(labelRemoteHost);
            groupBoxSettings.Controls.Add(textBoxSearchSpec);
            groupBoxSettings.Controls.Add(labelSearchSpec);
            groupBoxSettings.Controls.Add(buttonLocalPath);
            groupBoxSettings.Controls.Add(textBoxLocalPath);
            groupBoxSettings.Controls.Add(labelLocalPath);
            groupBoxSettings.Controls.Add(checkStartAtLogin);
            groupBoxSettings.Controls.Add(checkStartInTray);
            groupBoxSettings.Dock = DockStyle.Top;
            groupBoxSettings.Location = new Point(0, 28);
            groupBoxSettings.Name = "groupBoxSettings";
            groupBoxSettings.Size = new Size(1182, 165);
            groupBoxSettings.TabIndex = 4;
            groupBoxSettings.TabStop = false;
            groupBoxSettings.Text = "Settings";
            // 
            // btnExclusions
            // 
            btnExclusions.Location = new Point(532, 128);
            btnExclusions.Name = "btnExclusions";
            btnExclusions.Size = new Size(94, 29);
            btnExclusions.TabIndex = 7;
            btnExclusions.Text = "&Exclusions";
            btnExclusions.UseVisualStyleBackColor = true;
            btnExclusions.Click += btnExclusions_Click;
            // 
            // buttonVerifyAccess
            // 
            buttonVerifyAccess.Enabled = false;
            buttonVerifyAccess.Location = new Point(962, 128);
            buttonVerifyAccess.Name = "buttonVerifyAccess";
            buttonVerifyAccess.Size = new Size(104, 29);
            buttonVerifyAccess.TabIndex = 12;
            buttonVerifyAccess.Text = "&Verify Access";
            buttonVerifyAccess.UseVisualStyleBackColor = true;
            buttonVerifyAccess.Click += buttonVerifyAccess_Click;
            // 
            // buttonStartStopSync
            // 
            buttonStartStopSync.Location = new Point(1072, 128);
            buttonStartStopSync.Name = "buttonStartStopSync";
            buttonStartStopSync.Size = new Size(94, 29);
            buttonStartStopSync.TabIndex = 13;
            buttonStartStopSync.Text = "&Start Sync";
            buttonStartStopSync.UseVisualStyleBackColor = true;
            buttonStartStopSync.Click += buttonStartStopSync_Click;
            // 
            // checkBoxShowPassword
            // 
            checkBoxShowPassword.AutoSize = true;
            checkBoxShowPassword.Location = new Point(822, 130);
            checkBoxShowPassword.Name = "checkBoxShowPassword";
            checkBoxShowPassword.Size = new Size(134, 24);
            checkBoxShowPassword.TabIndex = 11;
            checkBoxShowPassword.Text = "Show &password";
            checkBoxShowPassword.UseVisualStyleBackColor = true;
            checkBoxShowPassword.CheckedChanged += checkBoxShowPassword_CheckedChanged;
            // 
            // checkBoxAutoStartSync
            // 
            checkBoxAutoStartSync.AutoSize = true;
            checkBoxAutoStartSync.Location = new Point(394, 130);
            checkBoxAutoStartSync.Name = "checkBoxAutoStartSync";
            checkBoxAutoStartSync.Size = new Size(128, 24);
            checkBoxAutoStartSync.TabIndex = 6;
            checkBoxAutoStartSync.Text = "&Auto start sync";
            checkBoxAutoStartSync.UseVisualStyleBackColor = true;
            checkBoxAutoStartSync.CheckedChanged += checkBoxAutoStartSync_CheckedChanged;
            // 
            // textBoxRemotePath
            // 
            textBoxRemotePath.Location = new Point(112, 93);
            textBoxRemotePath.Name = "textBoxRemotePath";
            textBoxRemotePath.Size = new Size(514, 27);
            textBoxRemotePath.TabIndex = 3;
            textBoxRemotePath.TextChanged += textBoxRemotePath_TextChanged;
            // 
            // labelRemotePath
            // 
            labelRemotePath.AutoSize = true;
            labelRemotePath.Location = new Point(11, 96);
            labelRemotePath.Name = "labelRemotePath";
            labelRemotePath.Size = new Size(95, 20);
            labelRemotePath.TabIndex = 15;
            labelRemotePath.Text = "Remote path";
            // 
            // textBoxRemotePassword
            // 
            textBoxRemotePassword.Location = new Point(801, 93);
            textBoxRemotePassword.Name = "textBoxRemotePassword";
            textBoxRemotePassword.Size = new Size(365, 27);
            textBoxRemotePassword.TabIndex = 10;
            textBoxRemotePassword.UseSystemPasswordChar = true;
            textBoxRemotePassword.TextChanged += textBoxRemotePassword_TextChanged;
            // 
            // labelRemotePassword
            // 
            labelRemotePassword.AutoSize = true;
            labelRemotePassword.Location = new Point(670, 96);
            labelRemotePassword.Name = "labelRemotePassword";
            labelRemotePassword.Size = new Size(128, 20);
            labelRemotePassword.TabIndex = 13;
            labelRemotePassword.Text = "Remote password";
            // 
            // textBoxRemoteUser
            // 
            textBoxRemoteUser.Location = new Point(801, 60);
            textBoxRemoteUser.Name = "textBoxRemoteUser";
            textBoxRemoteUser.Size = new Size(365, 27);
            textBoxRemoteUser.TabIndex = 9;
            textBoxRemoteUser.TextChanged += textBoxRemoteUser_TextChanged;
            // 
            // labelRemoteUser
            // 
            labelRemoteUser.AutoSize = true;
            labelRemoteUser.Location = new Point(670, 63);
            labelRemoteUser.Name = "labelRemoteUser";
            labelRemoteUser.Size = new Size(92, 20);
            labelRemoteUser.TabIndex = 11;
            labelRemoteUser.Text = "Remote user";
            // 
            // textBoxRemoteHost
            // 
            textBoxRemoteHost.Location = new Point(801, 27);
            textBoxRemoteHost.Name = "textBoxRemoteHost";
            textBoxRemoteHost.Size = new Size(365, 27);
            textBoxRemoteHost.TabIndex = 8;
            textBoxRemoteHost.TextChanged += textBoxRemoteHost_TextChanged;
            // 
            // labelRemoteHost
            // 
            labelRemoteHost.AutoSize = true;
            labelRemoteHost.Location = new Point(670, 31);
            labelRemoteHost.Name = "labelRemoteHost";
            labelRemoteHost.Size = new Size(93, 20);
            labelRemoteHost.TabIndex = 9;
            labelRemoteHost.Text = "Remote host";
            // 
            // textBoxSearchSpec
            // 
            textBoxSearchSpec.Location = new Point(109, 60);
            textBoxSearchSpec.Name = "textBoxSearchSpec";
            textBoxSearchSpec.Size = new Size(517, 27);
            textBoxSearchSpec.TabIndex = 2;
            textBoxSearchSpec.TextChanged += textBoxSearchSpec_TextChanged;
            // 
            // labelSearchSpec
            // 
            labelSearchSpec.AutoSize = true;
            labelSearchSpec.Location = new Point(11, 63);
            labelSearchSpec.Name = "labelSearchSpec";
            labelSearchSpec.Size = new Size(87, 20);
            labelSearchSpec.TabIndex = 7;
            labelSearchSpec.Text = "Search spec";
            // 
            // buttonLocalPath
            // 
            buttonLocalPath.Location = new Point(627, 26);
            buttonLocalPath.Name = "buttonLocalPath";
            buttonLocalPath.Size = new Size(34, 29);
            buttonLocalPath.TabIndex = 1;
            buttonLocalPath.Text = "...";
            buttonLocalPath.UseVisualStyleBackColor = true;
            buttonLocalPath.Click += buttonLocalPath_Click;
            // 
            // textBoxLocalPath
            // 
            textBoxLocalPath.Location = new Point(109, 27);
            textBoxLocalPath.Name = "textBoxLocalPath";
            textBoxLocalPath.Size = new Size(517, 27);
            textBoxLocalPath.TabIndex = 0;
            textBoxLocalPath.TextChanged += textBoxLocalPath_TextChanged;
            // 
            // labelLocalPath
            // 
            labelLocalPath.AutoSize = true;
            labelLocalPath.Location = new Point(11, 31);
            labelLocalPath.Name = "labelLocalPath";
            labelLocalPath.Size = new Size(78, 20);
            labelLocalPath.TabIndex = 4;
            labelLocalPath.Text = "Local path";
            // 
            // listBoxMessages
            // 
            listBoxMessages.Dock = DockStyle.Fill;
            listBoxMessages.FormattingEnabled = true;
            listBoxMessages.HorizontalScrollbar = true;
            listBoxMessages.Location = new Point(0, 193);
            listBoxMessages.Margin = new Padding(10);
            listBoxMessages.Name = "listBoxMessages";
            listBoxMessages.ScrollAlwaysVisible = true;
            listBoxMessages.SelectionMode = SelectionMode.None;
            listBoxMessages.Size = new Size(1182, 560);
            listBoxMessages.TabIndex = 14;
            // 
            // menuStrip1
            // 
            menuStrip1.ImageScalingSize = new Size(20, 20);
            menuStrip1.Items.AddRange(new ToolStripItem[] { mnuFile, mnuHelp });
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new Size(1182, 28);
            menuStrip1.TabIndex = 5;
            menuStrip1.Text = "menuStrip1";
            // 
            // mnuFile
            // 
            mnuFile.DropDownItems.AddRange(new ToolStripItem[] { mnuFileExit });
            mnuFile.Name = "mnuFile";
            mnuFile.Size = new Size(46, 24);
            mnuFile.Text = "&File";
            // 
            // mnuFileExit
            // 
            mnuFileExit.Name = "mnuFileExit";
            mnuFileExit.ShortcutKeys = Keys.Alt | Keys.F4;
            mnuFileExit.Size = new Size(224, 26);
            mnuFileExit.Text = "E&xit";
            mnuFileExit.Click += mnuFileExit_Click;
            // 
            // mnuHelp
            // 
            mnuHelp.DropDownItems.AddRange(new ToolStripItem[] { mnuHelpView, mnuHelpAbout });
            mnuHelp.Name = "mnuHelp";
            mnuHelp.Size = new Size(55, 24);
            mnuHelp.Text = "&Help";
            // 
            // mnuHelpView
            // 
            mnuHelpView.Enabled = false;
            mnuHelpView.Name = "mnuHelpView";
            mnuHelpView.ShortcutKeys = Keys.F1;
            mnuHelpView.Size = new Size(224, 26);
            mnuHelpView.Text = "&View Help";
            mnuHelpView.Click += mnuHelpView_Click;
            // 
            // mnuHelpAbout
            // 
            mnuHelpAbout.Name = "mnuHelpAbout";
            mnuHelpAbout.Size = new Size(224, 26);
            mnuHelpAbout.Text = "&About";
            mnuHelpAbout.Click += mnuHelpAbout_Click;
            // 
            // StatusBar
            // 
            StatusBar.ImageScalingSize = new Size(20, 20);
            StatusBar.Items.AddRange(new ToolStripItem[] { Panel1 });
            StatusBar.Location = new Point(0, 729);
            StatusBar.Name = "StatusBar";
            StatusBar.Size = new Size(1182, 24);
            StatusBar.TabIndex = 6;
            // 
            // Panel1
            // 
            Panel1.AutoSize = false;
            Panel1.ImageAlign = ContentAlignment.MiddleLeft;
            Panel1.Name = "Panel1";
            Panel1.Size = new Size(1200, 18);
            Panel1.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1182, 753);
            Controls.Add(StatusBar);
            Controls.Add(listBoxMessages);
            Controls.Add(groupBoxSettings);
            Controls.Add(menuStrip1);
            Icon = (Icon)resources.GetObject("$this.Icon");
            MainMenuStrip = menuStrip1;
            MinimumSize = new Size(1200, 800);
            Name = "MainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "SFTP Sync";
            FormClosing += MainForm_FormClosing;
            groupBoxSettings.ResumeLayout(false);
            groupBoxSettings.PerformLayout();
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            StatusBar.ResumeLayout(false);
            StatusBar.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private CheckBox checkStartAtLogin;
        private CheckBox checkStartInTray;
        private GroupBox groupBoxSettings;
        private TextBox textBoxLocalPath;
        private Label labelLocalPath;
        private Label labelRemotePath;
        private TextBox textBoxRemotePassword;
        private Label labelRemotePassword;
        private TextBox textBoxRemoteUser;
        private Label labelRemoteUser;
        private TextBox textBoxRemoteHost;
        private Label labelRemoteHost;
        private TextBox textBoxSearchSpec;
        private Label labelSearchSpec;
        private Button buttonLocalPath;
        private TextBox textBoxRemotePath;
        private ToolStripStatusLabel Panel1;
        private StatusStrip StatusBar;
        private ListBox listBoxMessages;
        private CheckBox checkBoxAutoStartSync;
        private CheckBox checkBoxShowPassword;
        private Button buttonStartStopSync;
        private Button buttonVerifyAccess;
        private MenuStrip menuStrip1;
        private ToolStripMenuItem mnuFile;
        private ToolStripMenuItem mnuHelp;
        private ToolStripMenuItem mnuHelpView;
        private ToolStripMenuItem mnuFileExit;
        private ToolStripMenuItem mnuHelpAbout;
        private Button btnExclusions;
    }
}
