namespace SFTPSync
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
            groupBoxActivity = new GroupBox();
            listBoxMessages = new ListBox();
            groupBoxSettings.SuspendLayout();
            groupBoxActivity.SuspendLayout();
            SuspendLayout();
            // 
            // checkStartAtLogin
            // 
            checkStartAtLogin.AutoSize = true;
            checkStartAtLogin.Location = new Point(982, 29);
            checkStartAtLogin.Name = "checkStartAtLogin";
            checkStartAtLogin.Size = new Size(117, 24);
            checkStartAtLogin.TabIndex = 7;
            checkStartAtLogin.Text = "&Start at login";
            checkStartAtLogin.UseVisualStyleBackColor = true;
            checkStartAtLogin.CheckedChanged += checkStartAtLogin_CheckedChanged;
            // 
            // checkStartInTray
            // 
            checkStartInTray.AutoSize = true;
            checkStartInTray.Location = new Point(982, 62);
            checkStartInTray.Name = "checkStartInTray";
            checkStartInTray.Size = new Size(156, 24);
            checkStartInTray.TabIndex = 8;
            checkStartInTray.Text = "Start in system &tray";
            checkStartInTray.UseVisualStyleBackColor = true;
            checkStartInTray.CheckedChanged += checkStartInTray_CheckedChanged;
            // 
            // groupBoxSettings
            // 
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
            groupBoxSettings.Location = new Point(12, 12);
            groupBoxSettings.Name = "groupBoxSettings";
            groupBoxSettings.Size = new Size(1158, 137);
            groupBoxSettings.TabIndex = 4;
            groupBoxSettings.TabStop = false;
            groupBoxSettings.Text = "Settings";
            // 
            // checkBoxAutoStartSync
            // 
            checkBoxAutoStartSync.AutoSize = true;
            checkBoxAutoStartSync.Location = new Point(982, 95);
            checkBoxAutoStartSync.Name = "checkBoxAutoStartSync";
            checkBoxAutoStartSync.Size = new Size(128, 24);
            checkBoxAutoStartSync.TabIndex = 16;
            checkBoxAutoStartSync.Text = "&Auto start sync";
            checkBoxAutoStartSync.UseVisualStyleBackColor = true;
            checkBoxAutoStartSync.CheckedChanged += checkBoxAutoStartSync_CheckedChanged;
            // 
            // textBoxRemotePath
            // 
            textBoxRemotePath.Location = new Point(119, 93);
            textBoxRemotePath.Name = "textBoxRemotePath";
            textBoxRemotePath.Size = new Size(487, 27);
            textBoxRemotePath.TabIndex = 3;
            textBoxRemotePath.TextChanged += textBoxRemotePath_TextChanged;
            // 
            // labelRemotePath
            // 
            labelRemotePath.AutoSize = true;
            labelRemotePath.Location = new Point(18, 96);
            labelRemotePath.Name = "labelRemotePath";
            labelRemotePath.Size = new Size(95, 20);
            labelRemotePath.TabIndex = 15;
            labelRemotePath.Text = "Remote path";
            // 
            // textBoxRemotePassword
            // 
            textBoxRemotePassword.Location = new Point(796, 93);
            textBoxRemotePassword.Name = "textBoxRemotePassword";
            textBoxRemotePassword.PasswordChar = '*';
            textBoxRemotePassword.Size = new Size(125, 27);
            textBoxRemotePassword.TabIndex = 6;
            textBoxRemotePassword.TextChanged += textBoxRemotePassword_TextChanged;
            // 
            // labelRemotePassword
            // 
            labelRemotePassword.AutoSize = true;
            labelRemotePassword.Location = new Point(661, 96);
            labelRemotePassword.Name = "labelRemotePassword";
            labelRemotePassword.Size = new Size(128, 20);
            labelRemotePassword.TabIndex = 13;
            labelRemotePassword.Text = "Remote password";
            // 
            // textBoxRemoteUser
            // 
            textBoxRemoteUser.Location = new Point(796, 60);
            textBoxRemoteUser.Name = "textBoxRemoteUser";
            textBoxRemoteUser.Size = new Size(125, 27);
            textBoxRemoteUser.TabIndex = 5;
            textBoxRemoteUser.TextChanged += textBoxRemoteUser_TextChanged;
            // 
            // labelRemoteUser
            // 
            labelRemoteUser.AutoSize = true;
            labelRemoteUser.Location = new Point(661, 63);
            labelRemoteUser.Name = "labelRemoteUser";
            labelRemoteUser.Size = new Size(92, 20);
            labelRemoteUser.TabIndex = 11;
            labelRemoteUser.Text = "Remote user";
            // 
            // textBoxRemoteHost
            // 
            textBoxRemoteHost.Location = new Point(796, 27);
            textBoxRemoteHost.Name = "textBoxRemoteHost";
            textBoxRemoteHost.Size = new Size(125, 27);
            textBoxRemoteHost.TabIndex = 4;
            textBoxRemoteHost.TextChanged += textBoxRemoteHost_TextChanged;
            // 
            // labelRemoteHost
            // 
            labelRemoteHost.AutoSize = true;
            labelRemoteHost.Location = new Point(661, 31);
            labelRemoteHost.Name = "labelRemoteHost";
            labelRemoteHost.Size = new Size(93, 20);
            labelRemoteHost.TabIndex = 9;
            labelRemoteHost.Text = "Remote host";
            // 
            // textBoxSearchSpec
            // 
            textBoxSearchSpec.Location = new Point(119, 60);
            textBoxSearchSpec.Name = "textBoxSearchSpec";
            textBoxSearchSpec.Size = new Size(487, 27);
            textBoxSearchSpec.TabIndex = 2;
            textBoxSearchSpec.TextChanged += textBoxSearchSpec_TextChanged;
            // 
            // labelSearchSpec
            // 
            labelSearchSpec.AutoSize = true;
            labelSearchSpec.Location = new Point(18, 63);
            labelSearchSpec.Name = "labelSearchSpec";
            labelSearchSpec.Size = new Size(87, 20);
            labelSearchSpec.TabIndex = 7;
            labelSearchSpec.Text = "Search spec";
            // 
            // buttonLocalPath
            // 
            buttonLocalPath.Location = new Point(612, 25);
            buttonLocalPath.Name = "buttonLocalPath";
            buttonLocalPath.Size = new Size(34, 29);
            buttonLocalPath.TabIndex = 1;
            buttonLocalPath.Text = "...";
            buttonLocalPath.UseVisualStyleBackColor = true;
            buttonLocalPath.Click += buttonLocalPath_Click;
            // 
            // textBoxLocalPath
            // 
            textBoxLocalPath.Location = new Point(119, 27);
            textBoxLocalPath.Name = "textBoxLocalPath";
            textBoxLocalPath.Size = new Size(487, 27);
            textBoxLocalPath.TabIndex = 0;
            textBoxLocalPath.TextChanged += textBoxLocalPath_TextChanged;
            // 
            // labelLocalPath
            // 
            labelLocalPath.AutoSize = true;
            labelLocalPath.Location = new Point(18, 31);
            labelLocalPath.Name = "labelLocalPath";
            labelLocalPath.Size = new Size(78, 20);
            labelLocalPath.TabIndex = 4;
            labelLocalPath.Text = "Local path";
            // 
            // groupBoxActivity
            // 
            groupBoxActivity.Controls.Add(listBoxMessages);
            groupBoxActivity.Location = new Point(12, 155);
            groupBoxActivity.Name = "groupBoxActivity";
            groupBoxActivity.Size = new Size(1158, 586);
            groupBoxActivity.TabIndex = 5;
            groupBoxActivity.TabStop = false;
            groupBoxActivity.Text = "Activity";
            // 
            // listBoxMessages
            // 
            listBoxMessages.FormattingEnabled = true;
            listBoxMessages.HorizontalScrollbar = true;
            listBoxMessages.Location = new Point(18, 26);
            listBoxMessages.Name = "listBoxMessages";
            listBoxMessages.ScrollAlwaysVisible = true;
            listBoxMessages.SelectionMode = SelectionMode.MultiSimple;
            listBoxMessages.Size = new Size(1120, 544);
            listBoxMessages.TabIndex = 0;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1182, 753);
            Controls.Add(groupBoxActivity);
            Controls.Add(groupBoxSettings);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "MainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "SFTP Sync";
            FormClosing += MainForm_FormClosing;
            groupBoxSettings.ResumeLayout(false);
            groupBoxSettings.PerformLayout();
            groupBoxActivity.ResumeLayout(false);
            ResumeLayout(false);
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
        private GroupBox groupBoxActivity;
        private ListBox listBoxMessages;
        private CheckBox checkBoxAutoStartSync;
    }
}
