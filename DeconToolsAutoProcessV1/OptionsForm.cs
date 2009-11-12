using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
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
        {
            InitializeComponent();
        }

        public OptionsForm(bool isRunMergingModeUsed)
            : this()
        {
            this.isResultMergingModeUsed = isRunMergingModeUsed;
            updateData();
        }

        private void updateData()
        {
            this.chkUseResultMergerMode.Checked = isResultMergingModeUsed;
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.isResultMergingModeUsed = chkUseResultMergerMode.Checked;
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
