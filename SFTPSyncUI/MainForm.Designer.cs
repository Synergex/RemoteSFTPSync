﻿
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
            listBoxMessages.FormattingEnabled = true;
            listBoxMessages.HorizontalScrollbar = true;
            listBoxMessages.Location = new Point(0, 28);
            listBoxMessages.Margin = new Padding(10);
            listBoxMessages.Name = "listBoxMessages";
            listBoxMessages.ScrollAlwaysVisible = true;
            listBoxMessages.SelectionMode = SelectionMode.None;
            listBoxMessages.Size = new Size(1182, 725);
            listBoxMessages.TabIndex = 14;
            // 
            // menuStrip1
            // 
            menuStrip1.ImageScalingSize = new Size(20, 20);
            menuStrip1.Items.AddRange(new ToolStripItem[] { mnuFile, mnuTools, mnuHelp });
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new Size(1182, 28);
            menuStrip1.TabIndex = 5;
            menuStrip1.Text = "menuStrip1";
            // 
            // mnuFile
            // 
            mnuFile.DropDownItems.AddRange(new ToolStripItem[] { mnuFileStartSync, mnuFileStopSync, toolStripMenuItem1, mnuFileExit });
            mnuFile.Name = "mnuFile";
            mnuFile.Size = new Size(46, 24);
            mnuFile.Text = "&File";
            // 
            // mnuFileStartSync
            // 
            mnuFileStartSync.Enabled = false;
            mnuFileStartSync.Name = "mnuFileStartSync";
            mnuFileStartSync.Size = new Size(224, 26);
            mnuFileStartSync.Text = "&Start sync";
            mnuFileStartSync.Click += mnuFileStartSync_Click;
            // 
            // mnuFileStopSync
            // 
            mnuFileStopSync.Enabled = false;
            mnuFileStopSync.Name = "mnuFileStopSync";
            mnuFileStopSync.Size = new Size(224, 26);
            mnuFileStopSync.Text = "S&top sync";
            mnuFileStopSync.Click += mnuFileStopSync_Click;
            // 
            // toolStripMenuItem1
            // 
            toolStripMenuItem1.Name = "toolStripMenuItem1";
            toolStripMenuItem1.Size = new Size(221, 6);
            // 
            // mnuFileExit
            // 
            mnuFileExit.Name = "mnuFileExit";
            mnuFileExit.ShortcutKeys = Keys.Alt | Keys.F4;
            mnuFileExit.Size = new Size(224, 26);
            mnuFileExit.Text = "E&xit";
            mnuFileExit.Click += mnuFileExit_Click;
            // 
            // mnuTools
            // 
            mnuTools.DropDownItems.AddRange(new ToolStripItem[] { mnuToolsOptions });
            mnuTools.Name = "mnuTools";
            mnuTools.Size = new Size(58, 24);
            mnuTools.Text = "&Tools";
            // 
            // mnuToolsOptions
            // 
            mnuToolsOptions.Name = "mnuToolsOptions";
            mnuToolsOptions.Size = new Size(224, 26);
            mnuToolsOptions.Text = "&Options";
            mnuToolsOptions.Click += mnuToolsOptions_Click;
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
            mnuHelpView.Size = new Size(184, 26);
            mnuHelpView.Text = "&View Help";
            mnuHelpView.Click += mnuHelpView_Click;
            // 
            // mnuHelpAbout
            // 
            mnuHelpAbout.Name = "mnuHelpAbout";
            mnuHelpAbout.Size = new Size(184, 26);
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
            Controls.Add(menuStrip1);
            Icon = (Icon)resources.GetObject("$this.Icon");
            MainMenuStrip = menuStrip1;
            MinimumSize = new Size(1200, 800);
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
        private ToolStripStatusLabel Panel1;
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
    }
}
