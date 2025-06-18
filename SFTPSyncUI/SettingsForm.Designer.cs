
namespace SFTPSyncUI
{
    partial class SettingsForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SettingsForm));
            checkStartAtLogin = new CheckBox();
            checkStartInTray = new CheckBox();
            buttonVerifyAccess = new Button();
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
            listBox = new ListBox();
            labelExclusions = new Label();
            btnAdd = new Button();
            btnRemove = new Button();
            groupLocal = new GroupBox();
            groupRemote = new GroupBox();
            groupStartup = new GroupBox();
            btnClose = new Button();
            groupLocal.SuspendLayout();
            groupRemote.SuspendLayout();
            groupStartup.SuspendLayout();
            SuspendLayout();
            // 
            // checkStartAtLogin
            // 
            checkStartAtLogin.AutoSize = true;
            checkStartAtLogin.Location = new Point(46, 36);
            checkStartAtLogin.Name = "checkStartAtLogin";
            checkStartAtLogin.Size = new Size(117, 24);
            checkStartAtLogin.TabIndex = 12;
            checkStartAtLogin.Text = "Start at &login";
            checkStartAtLogin.UseVisualStyleBackColor = true;
            checkStartAtLogin.CheckedChanged += checkStartAtLogin_CheckedChanged;
            // 
            // checkStartInTray
            // 
            checkStartInTray.AutoSize = true;
            checkStartInTray.Location = new Point(180, 36);
            checkStartInTray.Name = "checkStartInTray";
            checkStartInTray.Size = new Size(156, 24);
            checkStartInTray.TabIndex = 13;
            checkStartInTray.Text = "Start in system &tray";
            checkStartInTray.UseVisualStyleBackColor = true;
            checkStartInTray.CheckedChanged += checkStartInTray_CheckedChanged;
            // 
            // buttonVerifyAccess
            // 
            buttonVerifyAccess.Enabled = false;
            buttonVerifyAccess.Location = new Point(725, 110);
            buttonVerifyAccess.Name = "buttonVerifyAccess";
            buttonVerifyAccess.Size = new Size(104, 29);
            buttonVerifyAccess.TabIndex = 11;
            buttonVerifyAccess.Text = "&Verify Access";
            buttonVerifyAccess.UseVisualStyleBackColor = true;
            buttonVerifyAccess.Click += buttonVerifyAccess_Click;
            // 
            // checkBoxShowPassword
            // 
            checkBoxShowPassword.AutoSize = true;
            checkBoxShowPassword.Location = new Point(695, 70);
            checkBoxShowPassword.Name = "checkBoxShowPassword";
            checkBoxShowPassword.Size = new Size(134, 24);
            checkBoxShowPassword.TabIndex = 10;
            checkBoxShowPassword.Text = "Show &password";
            checkBoxShowPassword.UseVisualStyleBackColor = true;
            checkBoxShowPassword.CheckedChanged += checkBoxShowPassword_CheckedChanged;
            // 
            // checkBoxAutoStartSync
            // 
            checkBoxAutoStartSync.AutoSize = true;
            checkBoxAutoStartSync.Location = new Point(356, 36);
            checkBoxAutoStartSync.Name = "checkBoxAutoStartSync";
            checkBoxAutoStartSync.Size = new Size(128, 24);
            checkBoxAutoStartSync.TabIndex = 14;
            checkBoxAutoStartSync.Text = "&Auto start sync";
            checkBoxAutoStartSync.UseVisualStyleBackColor = true;
            checkBoxAutoStartSync.CheckedChanged += checkBoxAutoStartSync_CheckedChanged;
            // 
            // textBoxRemotePath
            // 
            textBoxRemotePath.Location = new Point(122, 70);
            textBoxRemotePath.Name = "textBoxRemotePath";
            textBoxRemotePath.Size = new Size(454, 27);
            textBoxRemotePath.TabIndex = 9;
            textBoxRemotePath.TextChanged += textBoxRemotePath_TextChanged;
            // 
            // labelRemotePath
            // 
            labelRemotePath.AutoSize = true;
            labelRemotePath.Location = new Point(21, 73);
            labelRemotePath.Name = "labelRemotePath";
            labelRemotePath.Size = new Size(95, 20);
            labelRemotePath.TabIndex = 15;
            labelRemotePath.Text = "Remote path";
            // 
            // textBoxRemotePassword
            // 
            textBoxRemotePassword.Location = new Point(674, 37);
            textBoxRemotePassword.Name = "textBoxRemotePassword";
            textBoxRemotePassword.Size = new Size(155, 27);
            textBoxRemotePassword.TabIndex = 8;
            textBoxRemotePassword.UseSystemPasswordChar = true;
            textBoxRemotePassword.TextChanged += textBoxRemotePassword_TextChanged;
            // 
            // labelRemotePassword
            // 
            labelRemotePassword.AutoSize = true;
            labelRemotePassword.Location = new Point(598, 40);
            labelRemotePassword.Name = "labelRemotePassword";
            labelRemotePassword.Size = new Size(70, 20);
            labelRemotePassword.TabIndex = 13;
            labelRemotePassword.Text = "Password";
            // 
            // textBoxRemoteUser
            // 
            textBoxRemoteUser.Location = new Point(437, 37);
            textBoxRemoteUser.Name = "textBoxRemoteUser";
            textBoxRemoteUser.Size = new Size(139, 27);
            textBoxRemoteUser.TabIndex = 7;
            textBoxRemoteUser.TextChanged += textBoxRemoteUser_TextChanged;
            // 
            // labelRemoteUser
            // 
            labelRemoteUser.AutoSize = true;
            labelRemoteUser.Location = new Point(356, 40);
            labelRemoteUser.Name = "labelRemoteUser";
            labelRemoteUser.Size = new Size(75, 20);
            labelRemoteUser.TabIndex = 11;
            labelRemoteUser.Text = "Username";
            // 
            // textBoxRemoteHost
            // 
            textBoxRemoteHost.Location = new Point(122, 37);
            textBoxRemoteHost.Name = "textBoxRemoteHost";
            textBoxRemoteHost.Size = new Size(214, 27);
            textBoxRemoteHost.TabIndex = 6;
            textBoxRemoteHost.TextChanged += textBoxRemoteHost_TextChanged;
            // 
            // labelRemoteHost
            // 
            labelRemoteHost.AutoSize = true;
            labelRemoteHost.Location = new Point(21, 40);
            labelRemoteHost.Name = "labelRemoteHost";
            labelRemoteHost.Size = new Size(93, 20);
            labelRemoteHost.TabIndex = 9;
            labelRemoteHost.Text = "Remote host";
            // 
            // textBoxSearchSpec
            // 
            textBoxSearchSpec.Location = new Point(122, 65);
            textBoxSearchSpec.Name = "textBoxSearchSpec";
            textBoxSearchSpec.Size = new Size(707, 27);
            textBoxSearchSpec.TabIndex = 2;
            textBoxSearchSpec.TextChanged += textBoxSearchSpec_TextChanged;
            // 
            // labelSearchSpec
            // 
            labelSearchSpec.AutoSize = true;
            labelSearchSpec.Location = new Point(21, 68);
            labelSearchSpec.Name = "labelSearchSpec";
            labelSearchSpec.Size = new Size(87, 20);
            labelSearchSpec.TabIndex = 7;
            labelSearchSpec.Text = "Search spec";
            // 
            // buttonLocalPath
            // 
            buttonLocalPath.Location = new Point(795, 32);
            buttonLocalPath.Name = "buttonLocalPath";
            buttonLocalPath.Size = new Size(34, 29);
            buttonLocalPath.TabIndex = 1;
            buttonLocalPath.Text = "...";
            buttonLocalPath.UseVisualStyleBackColor = true;
            buttonLocalPath.Click += buttonLocalPath_Click;
            // 
            // textBoxLocalPath
            // 
            textBoxLocalPath.Location = new Point(122, 32);
            textBoxLocalPath.Name = "textBoxLocalPath";
            textBoxLocalPath.Size = new Size(667, 27);
            textBoxLocalPath.TabIndex = 0;
            textBoxLocalPath.TextChanged += textBoxLocalPath_TextChanged;
            // 
            // labelLocalPath
            // 
            labelLocalPath.AutoSize = true;
            labelLocalPath.Location = new Point(21, 36);
            labelLocalPath.Name = "labelLocalPath";
            labelLocalPath.Size = new Size(78, 20);
            labelLocalPath.TabIndex = 4;
            labelLocalPath.Text = "Local path";
            // 
            // listBox
            // 
            listBox.FormattingEnabled = true;
            listBox.Location = new Point(122, 98);
            listBox.Name = "listBox";
            listBox.Size = new Size(707, 204);
            listBox.TabIndex = 3;
            listBox.SelectedIndexChanged += listBox_SelectedIndexChanged;
            // 
            // labelExclusions
            // 
            labelExclusions.AutoSize = true;
            labelExclusions.Location = new Point(21, 100);
            labelExclusions.Name = "labelExclusions";
            labelExclusions.Size = new Size(76, 20);
            labelExclusions.TabIndex = 17;
            labelExclusions.Text = "Exclusions";
            // 
            // btnAdd
            // 
            btnAdd.Location = new Point(635, 308);
            btnAdd.Name = "btnAdd";
            btnAdd.Size = new Size(94, 29);
            btnAdd.TabIndex = 4;
            btnAdd.Text = "&Add";
            btnAdd.UseVisualStyleBackColor = true;
            btnAdd.Click += btnAdd_Click;
            // 
            // btnRemove
            // 
            btnRemove.Location = new Point(735, 308);
            btnRemove.Name = "btnRemove";
            btnRemove.Size = new Size(94, 29);
            btnRemove.TabIndex = 5;
            btnRemove.Text = "&Remove";
            btnRemove.UseVisualStyleBackColor = true;
            btnRemove.Click += btnRemove_Click;
            // 
            // groupLocal
            // 
            groupLocal.Controls.Add(listBox);
            groupLocal.Controls.Add(btnRemove);
            groupLocal.Controls.Add(textBoxSearchSpec);
            groupLocal.Controls.Add(btnAdd);
            groupLocal.Controls.Add(labelSearchSpec);
            groupLocal.Controls.Add(labelExclusions);
            groupLocal.Controls.Add(buttonLocalPath);
            groupLocal.Controls.Add(textBoxLocalPath);
            groupLocal.Controls.Add(labelLocalPath);
            groupLocal.Location = new Point(12, 12);
            groupLocal.Name = "groupLocal";
            groupLocal.Size = new Size(857, 353);
            groupLocal.TabIndex = 20;
            groupLocal.TabStop = false;
            groupLocal.Text = "Local Windows System Settings";
            // 
            // groupRemote
            // 
            groupRemote.Controls.Add(textBoxRemotePath);
            groupRemote.Controls.Add(textBoxRemoteHost);
            groupRemote.Controls.Add(buttonVerifyAccess);
            groupRemote.Controls.Add(labelRemoteUser);
            groupRemote.Controls.Add(labelRemoteHost);
            groupRemote.Controls.Add(textBoxRemoteUser);
            groupRemote.Controls.Add(checkBoxShowPassword);
            groupRemote.Controls.Add(labelRemotePassword);
            groupRemote.Controls.Add(textBoxRemotePassword);
            groupRemote.Controls.Add(labelRemotePath);
            groupRemote.Location = new Point(12, 371);
            groupRemote.Name = "groupRemote";
            groupRemote.Size = new Size(857, 155);
            groupRemote.TabIndex = 21;
            groupRemote.TabStop = false;
            groupRemote.Text = "Remote OpenVMS System Settings";
            // 
            // groupStartup
            // 
            groupStartup.Controls.Add(btnClose);
            groupStartup.Controls.Add(checkBoxAutoStartSync);
            groupStartup.Controls.Add(checkStartAtLogin);
            groupStartup.Controls.Add(checkStartInTray);
            groupStartup.Location = new Point(12, 532);
            groupStartup.Name = "groupStartup";
            groupStartup.Size = new Size(857, 84);
            groupStartup.TabIndex = 22;
            groupStartup.TabStop = false;
            groupStartup.Text = "Application Startup Settings";
            // 
            // btnClose
            // 
            btnClose.Location = new Point(725, 33);
            btnClose.Name = "btnClose";
            btnClose.Size = new Size(104, 29);
            btnClose.TabIndex = 15;
            btnClose.Text = "&Close";
            btnClose.UseVisualStyleBackColor = true;
            btnClose.Click += btnClose_Click;
            // 
            // SettingsForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(878, 628);
            Controls.Add(groupStartup);
            Controls.Add(groupRemote);
            Controls.Add(groupLocal);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "SettingsForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "SFTP Sync Settings";
            Shown += SettingsForm_Shown;
            groupLocal.ResumeLayout(false);
            groupLocal.PerformLayout();
            groupRemote.ResumeLayout(false);
            groupRemote.PerformLayout();
            groupStartup.ResumeLayout(false);
            groupStartup.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private CheckBox checkStartAtLogin;
        private CheckBox checkStartInTray;
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
        private CheckBox checkBoxAutoStartSync;
        private CheckBox checkBoxShowPassword;
        private Button buttonVerifyAccess;
        private ListBox listBox;
        private Label labelExclusions;
        private Button btnAdd;
        private Button btnRemove;
        private GroupBox groupLocal;
        private GroupBox groupRemote;
        private GroupBox groupStartup;
        private Button btnClose;
    }
}
