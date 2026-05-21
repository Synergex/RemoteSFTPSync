
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
            listBoxMessages = new ListBox();
            menuStrip1 = new MenuStrip();
            mnuFile = new ToolStripMenuItem();
            mnuFileStartSync = new ToolStripMenuItem();
            mnuFileStopSync = new ToolStripMenuItem();
            toolStripMenuItem1 = new ToolStripSeparator();
            mnuFileExit = new ToolStripMenuItem();
            mnuTools = new ToolStripMenuItem();
            mnuToolsOptions = new ToolStripMenuItem();
            mnuHelp = new ToolStripMenuItem();
            mnuHelpView = new ToolStripMenuItem();
            mnuHelpAbout = new ToolStripMenuItem();
            StatusBar = new StatusStrip();
            Panel1 = new ToolStripStatusLabel();
            menuStrip1.SuspendLayout();
            StatusBar.SuspendLayout();
            SuspendLayout();
            // 
            // listBoxMessages
            // 
            listBoxMessages.Dock = DockStyle.Fill;
            listBoxMessages.DrawMode = DrawMode.OwnerDrawFixed;
            listBoxMessages.Font = new Font("Cascadia Mono", 8F, FontStyle.Regular, GraphicsUnit.Point, 0);
            listBoxMessages.FormattingEnabled = true;
            listBoxMessages.HorizontalScrollbar = true;
            listBoxMessages.ItemHeight = 17;
            listBoxMessages.Location = new Point(0, 24);
            listBoxMessages.Margin = new Padding(9, 8, 9, 8);
            listBoxMessages.Name = "listBoxMessages";
            listBoxMessages.ScrollAlwaysVisible = true;
            listBoxMessages.SelectionMode = SelectionMode.None;
            listBoxMessages.Size = new Size(1036, 525);
            listBoxMessages.TabIndex = 14;
            listBoxMessages.DrawItem += ListBoxMessages_DrawItem;
            // 
            // menuStrip1
            // 
            menuStrip1.ImageScalingSize = new Size(20, 20);
            menuStrip1.Items.AddRange(new ToolStripItem[] { mnuFile, mnuTools, mnuHelp });
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Padding = new Padding(5, 2, 0, 2);
            menuStrip1.Size = new Size(1036, 24);
            menuStrip1.TabIndex = 5;
            menuStrip1.Text = "menuStrip1";
            // 
            // mnuFile
            // 
            mnuFile.DropDownItems.AddRange(new ToolStripItem[] { mnuFileStartSync, mnuFileStopSync, toolStripMenuItem1, mnuFileExit });
            mnuFile.Name = "mnuFile";
            mnuFile.Size = new Size(37, 20);
            mnuFile.Text = "&File";
            // 
            // mnuFileStartSync
            // 
            mnuFileStartSync.Enabled = false;
            mnuFileStartSync.Name = "mnuFileStartSync";
            mnuFileStartSync.Size = new Size(134, 22);
            mnuFileStartSync.Text = "&Start sync";
            mnuFileStartSync.Click += mnuFileStartSync_Click;
            // 
            // mnuFileStopSync
            // 
            mnuFileStopSync.Enabled = false;
            mnuFileStopSync.Name = "mnuFileStopSync";
            mnuFileStopSync.Size = new Size(134, 22);
            mnuFileStopSync.Text = "S&top sync";
            mnuFileStopSync.Click += mnuFileStopSync_Click;
            // 
            // toolStripMenuItem1
            // 
            toolStripMenuItem1.Name = "toolStripMenuItem1";
            toolStripMenuItem1.Size = new Size(131, 6);
            // 
            // mnuFileExit
            // 
            mnuFileExit.Name = "mnuFileExit";
            mnuFileExit.ShortcutKeys = Keys.Alt | Keys.F4;
            mnuFileExit.Size = new Size(134, 22);
            mnuFileExit.Text = "E&xit";
            mnuFileExit.Click += mnuFileExit_Click;
            // 
            // mnuTools
            // 
            mnuTools.DropDownItems.AddRange(new ToolStripItem[] { mnuToolsOptions });
            mnuTools.Name = "mnuTools";
            mnuTools.Size = new Size(47, 20);
            mnuTools.Text = "&Tools";
            // 
            // mnuToolsOptions
            // 
            mnuToolsOptions.Name = "mnuToolsOptions";
            mnuToolsOptions.Size = new Size(116, 22);
            mnuToolsOptions.Text = "&Options";
            mnuToolsOptions.Click += mnuToolsOptions_Click;
            // 
            // mnuHelp
            // 
            mnuHelp.DropDownItems.AddRange(new ToolStripItem[] { mnuHelpView, mnuHelpAbout });
            mnuHelp.Name = "mnuHelp";
            mnuHelp.Size = new Size(44, 20);
            mnuHelp.Text = "&Help";
            // 
            // mnuHelpView
            // 
            mnuHelpView.Enabled = false;
            mnuHelpView.Name = "mnuHelpView";
            mnuHelpView.ShortcutKeys = Keys.F1;
            mnuHelpView.Size = new Size(146, 22);
            mnuHelpView.Text = "&View Help";
            mnuHelpView.Click += mnuHelpView_Click;
            // 
            // mnuHelpAbout
            // 
            mnuHelpAbout.Name = "mnuHelpAbout";
            mnuHelpAbout.Size = new Size(146, 22);
            mnuHelpAbout.Text = "&About";
            mnuHelpAbout.Click += mnuHelpAbout_Click;
            // 
            // StatusBar
            // 
            StatusBar.ImageScalingSize = new Size(20, 20);
            StatusBar.Items.AddRange(new ToolStripItem[] { Panel1 });
            StatusBar.Location = new Point(0, 549);
            StatusBar.Name = "StatusBar";
            StatusBar.Padding = new Padding(1, 0, 12, 0);
            StatusBar.Size = new Size(1036, 22);
            StatusBar.TabIndex = 6;
            StatusBar.Text = "StatusBar.Text";
            // 
            // Panel1
            // 
            Panel1.Name = "Panel1";
            Panel1.Size = new Size(76, 17);
            Panel1.Text = "Sync inactive";
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1036, 571);
            Controls.Add(listBoxMessages);
            Controls.Add(menuStrip1);
            Controls.Add(StatusBar);
            Icon = (Icon)resources.GetObject("$this.Icon");
            MainMenuStrip = menuStrip1;
            Margin = new Padding(3, 2, 3, 2);
            MinimumSize = new Size(1052, 608);
            Name = "MainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "SFTP Sync";
            FormClosing += MainForm_FormClosing;
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            StatusBar.ResumeLayout(false);
            StatusBar.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private StatusStrip StatusBar;
        private ListBox listBoxMessages;
        private MenuStrip menuStrip1;
        private ToolStripMenuItem mnuFile;
        private ToolStripMenuItem mnuHelp;
        private ToolStripMenuItem mnuHelpView;
        private ToolStripMenuItem mnuFileExit;
        private ToolStripMenuItem mnuHelpAbout;
        private ToolStripMenuItem mnuFileStartSync;
        private ToolStripMenuItem mnuFileStopSync;
        private ToolStripSeparator toolStripMenuItem1;
        private ToolStripMenuItem mnuTools;
        private ToolStripMenuItem mnuToolsOptions;
        private ToolStripStatusLabel Panel1;
    }
}
