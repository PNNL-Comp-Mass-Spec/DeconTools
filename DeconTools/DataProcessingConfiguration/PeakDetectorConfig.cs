using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using DeconTools.Backend.Core;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend;

namespace DeconTools.DataProcessingConfiguration
{
    public partial class PeakDetectorConfig : Form
    {

        private DeconToolsPeakDetector data;

        public DeconToolsPeakDetector Data
        {
            get { return data; }
            set { data = value; }
        }

        public PeakDetectorConfig(Task peakDetectorTask)
        {
            if (peakDetectorTask is DeconTools.Backend.ProcessingTasks.DeconToolsPeakDetector)
            {
            }
            else
            {
                throw new Exception("The task type is not correct.");
            }
            
            InitializeComponent();
            loadPeakFitTypes();
            this.data = (DeconToolsPeakDetector)peakDetectorTask;
            updateFormData();
        }

        private void loadPeakFitTypes()
        {
            peakFitComboBox.DataSource = Enum.GetValues(typeof(Globals.PeakFitType));
        }

        private void updateFormData()
        {
            this.peakFitComboBox.SelectedItem = this.data.PeakFitType;
            this.sigNoiseUpDown.Value = (Decimal)(this.data.SigNoiseThreshold);
            this.peakBackgroundUpDown.Value = (Decimal)(this.data.PeakBackgroundRatio);
            this.isDataThresholdedCheckBox.Checked = this.data.IsDataThresholded;
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            updatePeakDetectorTaskData();
            this.Close();
        }

        private void updatePeakDetectorTaskData()
        {
            this.data.PeakFitType =(Globals.PeakFitType)this.peakFitComboBox.SelectedItem;
            this.data.SigNoiseThreshold = (double)this.sigNoiseUpDown.Value;
            this.data.PeakBackgroundRatio = (double)this.peakBackgroundUpDown.Value;
            this.data.IsDataThresholded = this.isDataThresholdedCheckBox.Checked;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
