namespace SFTPSyncUI
{
    partial class ExclusionsForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            listBox = new ListBox();
            btnAdd = new Button();
            btnRemove = new Button();
            btnClose = new Button();
            SuspendLayout();
            // 
            // listBox
            // 
            listBox.FormattingEnabled = true;
            listBox.Location = new Point(12, 12);
            listBox.Name = "listBox";
            listBox.SelectionMode = SelectionMode.MultiExtended;
            listBox.Size = new Size(631, 424);
            listBox.TabIndex = 0;
            listBox.SelectedIndexChanged += listBox_SelectedIndexChanged;
            // 
            // btnAdd
            // 
            btnAdd.Location = new Point(649, 12);
            btnAdd.Name = "btnAdd";
            btnAdd.Size = new Size(94, 29);
            btnAdd.TabIndex = 1;
            btnAdd.Text = "&Add";
            btnAdd.UseVisualStyleBackColor = true;
            btnAdd.Click += btnAdd_Click;
            // 
            // btnRemove
            // 
            btnRemove.Location = new Point(649, 47);
            btnRemove.Name = "btnRemove";
            btnRemove.Size = new Size(94, 29);
            btnRemove.TabIndex = 2;
            btnRemove.Text = "&Remove";
            btnRemove.UseVisualStyleBackColor = true;
            btnRemove.Click += btnRemove_Click;
            // 
            // btnClose
            // 
            btnClose.Location = new Point(649, 407);
            btnClose.Name = "btnClose";
            btnClose.Size = new Size(94, 29);
            btnClose.TabIndex = 3;
            btnClose.Text = "&Close";
            btnClose.UseVisualStyleBackColor = true;
            btnClose.Click += btnClose_Click;
            // 
            // ExclusionsForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = btnClose;
            ClientSize = new Size(752, 450);
            Controls.Add(btnClose);
            Controls.Add(btnRemove);
            Controls.Add(btnAdd);
            Controls.Add(listBox);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "ExclusionsForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Excluded Folders";
            ResumeLayout(false);
        }

        #endregion

        private ListBox listBox;
        private Button btnAdd;
        private Button btnRemove;
        private Button btnClose;
    }
}