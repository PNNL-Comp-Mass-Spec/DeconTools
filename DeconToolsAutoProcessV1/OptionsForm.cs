using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DeconTools.Backend;

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


        public Globals.ProjectControllerType ProjectControllerType { get; set; }



        public bool CreateMSFeatureForEachPeakMode { get; set; }

        /// <summary>
        /// Used in defining which Scans are analyzed.  See ScanSetCollectionCreator.  
        /// i.e. if you are summing five scans and NumScansToAdvance is set to 3
        /// you would get something like:    100,104,107 , with scan representing a summed scan across 5 scans. 
        /// </summary>


        //public OptionsForm(Globals.ProjectControllerType projectType)
        //{
        //    ProjectControllerType = projectType;
        //    InitializeComponent();
        //    this.comboBox1.DataSource = Enum.GetValues(typeof(Globals.ProjectControllerType));

        //    updateData();
        //}






        public OptionsForm()
            : this(false)
        {

        }

        public OptionsForm(bool isRunMergingModeUsed)
            : this(isRunMergingModeUsed, false)
        {

        }

        public OptionsForm(bool isRunMergingModeUsed, bool createMSFeatureForEachPeak)
        {
            InitializeComponent();
            this.CreateMSFeatureForEachPeakMode = createMSFeatureForEachPeak;
            this.isResultMergingModeUsed = isRunMergingModeUsed;
            updateData();
        }

    
        private void updateData()
        {
            this.chkUseResultMergerMode.Checked = isResultMergingModeUsed;
            this.chkCreateMSFeatureForEachPeak.Checked = CreateMSFeatureForEachPeakMode;
            this.comboBox1.SelectedItem = this.ProjectControllerType;


        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {

            this.DialogResult = DialogResult.OK;
            this.isResultMergingModeUsed = chkUseResultMergerMode.Checked;
            this.CreateMSFeatureForEachPeakMode = chkCreateMSFeatureForEachPeak.Checked;
            this.ProjectControllerType = (Globals.ProjectControllerType)this.comboBox1.SelectedItem;
            this.Close();
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
