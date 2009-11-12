using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend.Core;

namespace DeconTools.DataProcessingConfiguration
{
    public partial class HornDeconvolutorConfig : Form
    {
        private HornDeconvolutor data;

        public HornDeconvolutor Data
        {
            get { return data; }
            set { data = value; }
        }

        public HornDeconvolutorConfig(Task task)
        {
            if (task is DeconTools.Backend.ProcessingTasks.HornDeconvolutor)
            {

            }
            else
            {
                throw new Exception("The task type is not correct.");
            }

            InitializeComponent();

            this.data = (HornDeconvolutor)task;
            updateFormData();




        }

        private void updateFormData()
        {
            this.absolutePeptideIntensityUpDown.Value = (decimal)this.data.AbsoluteThresholdPeptideIntensity;
            this.averagineFormulaTextbox.Text = this.data.AveragineFormula;
            this.chargeCarrierTextBox.Text = this.data.ChargeCarrierMass.ToString();
            this.isCheckAllPatternsCheckbox.Checked = this.data.CheckPatternsAgainstChargeOne;
            this.deleteIntensityThreshUpDown.Value = (decimal)this.data.DeleteIntensityThreshold;
            this.IsAbsoluteIntensitiesUsedCheckbox.Checked = this.data.IsAbsolutePepIntensityUsed;
            this.actualMonoMZComboBox.SelectedItem = convertActualMZParameterToComboBox(this.data.IsActualMonoMZUsed);
            this.isCompleteFitCheckbox.Checked = this.data.IsCompleteFit;
            this.isMercuryCachingCheckbox.Checked = this.data.IsMercuryCashingUsed;
            this.isMSMSProcessedCheckbox.Checked = this.data.IsMSMSProcessed;
            // this.data.IsMZRangeUsed;
            this.IsO16O18DataCheckbox.Checked = this.data.IsO16O18Data;
            this.isotopicProfileFitComboBox.SelectedItem = this.data.IsotopicProfileFitType;
            this.isThrashUsedCheckbox.Checked = this.data.IsThrashed;
            this.leftFitFactorUpDown.Value = (decimal)this.data.LeftFitStringencyFactor;
            this.maxAllowedChargeUpdown.Value = (int)this.data.MaxChargeAllowed;
            this.maxAllowedFitUpDown.Value = (decimal)this.data.MaxFitAllowed;
            this.maxAllowedMWUpDown.Value = (decimal)this.data.MaxMWAllowed;
            //  this.data.MaxMZ;
            this.minIntensityForScoreUpDown.Value = (decimal)this.data.MinIntensityForScore;
            //  this.data.MinMZ;
            this.minPepBackgroundRatioUpDown.Value = (decimal)this.data.MinPeptideBackgroundRatio;
            this.numAllowedShouldersUpDown.Value = (int)this.data.NumAllowedShoulderPeaks;
            this.RightFitFactorUpDown.Value = (decimal)this.data.RightFitStringencyFactor;
            this.tagFormulaTextbox.Text = this.data.TagFormula;
        }

        private object convertActualMZParameterToComboBox(bool p)
        {
            throw new NotImplementedException();
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            updateTaskData();
            this.Close();
        }

        private void updateTaskData()
        {
            throw new NotImplementedException();
        }




    }
}
