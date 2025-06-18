
using System.Data;

namespace SFTPSyncUI
{
    public partial class ExclusionsForm : Form
    {
        /// <summary>
        /// Public property allowing the excluded folders to be set or retrieved.
        /// </summary>
        public List<string> ExcludedDirectories
        {
            get
            {
                return listBox.Items.Cast<string>().ToList();
            }
            set
            {
                listBox.Items.Clear();
                foreach (var dir in value)
                {
                    listBox.Items.Add(dir);
                }
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public ExclusionsForm()
        {
            InitializeComponent();
        }


        /// <summary>
        /// The user clicked the close button, so close the form
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }


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
            }
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
        }
    }
}