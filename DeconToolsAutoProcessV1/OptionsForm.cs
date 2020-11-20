using System;
using System.Windows.Forms;

namespace DeconToolsAutoProcessV1
{
    public partial class OptionsForm : Form
    {
        private bool isResultMergingModeUsed;

        public bool IsResultMergingModeUsed
        {
            get { return isResultMergingModeUsed; }
            set { isResultMergingModeUsed = value; }
        }

        public OptionsForm()
            : this(false)
        {
        }

        public OptionsForm(bool isRunMergingModeUsed)
        {
            InitializeComponent();
            this.isResultMergingModeUsed = isRunMergingModeUsed;
            updateData();
        }

        private void updateData()
        {
            chkUseResultMergerMode.Checked = isResultMergingModeUsed;
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            isResultMergingModeUsed = chkUseResultMergerMode.Checked;

            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            //this.ProjectControllerType = (Globals.ProjectControllerType)comboBox1.SelectedItem;
        }
    }
}
