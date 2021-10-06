// Written by Navdeep Jaitly and Anoop Mayampurath for the Department of Energy (PNNL, Richland, WA)
// Copyright 2006, Battelle Memorial Institute
// E-mail: navdeep.jaitly@pnl.gov or proteomics@pnnl.gov
// Website: https://github.com/PNNL-Comp-Mass-Spec/ or https://panomics.pnnl.gov/ or https://www.pnnl.gov/integrative-omics
// -------------------------------------------------------------------------------
// 
// Licensed under the Apache License, Version 2.0; you may not use this file except
// in compliance with the License.  You may obtain a copy of the License at 
// http://www.apache.org/licenses/LICENSE-2.0

using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;

namespace Decon2LS
{
    /// <summary>
    /// Summary description for ctlThresholdSettings.
    /// </summary>
    public class ctlThresholdSettings : System.Windows.Forms.UserControl
    {
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.PictureBox pictureBox_ThScore;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label_max_fit_tip;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label_ThDeletion_tip;
        private System.Windows.Forms.Label label_thscore_tip;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.TextBox mTextMaxFit;
        private System.Windows.Forms.TextBox mTextDeletionIntensityThreshold;
        private System.Windows.Forms.TextBox mTextScoreIntensityThreshold;
        private System.Windows.Forms.PictureBox pictureBox_ThDeletion;
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        public ctlThresholdSettings()
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();

            // TODO: Add any initialization after the InitializeComponent call

        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            var resources = new System.Resources.ResourceManager(typeof(ctlThresholdSettings));
            this.label3 = new System.Windows.Forms.Label();
            this.mTextMaxFit = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.mTextDeletionIntensityThreshold = new System.Windows.Forms.TextBox();
            this.mTextScoreIntensityThreshold = new System.Windows.Forms.TextBox();
            this.pictureBox_ThScore = new System.Windows.Forms.PictureBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label_ThDeletion_tip = new System.Windows.Forms.Label();
            this.label_max_fit_tip = new System.Windows.Forms.Label();
            this.pictureBox_ThDeletion = new System.Windows.Forms.PictureBox();
            this.label_thscore_tip = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(8, 16);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(96, 24);
            this.label3.TabIndex = 3;
            this.label3.Text = "Maximum Fit";
            // 
            // mTextMaxFit
            // 
            this.mTextMaxFit.Location = new System.Drawing.Point(128, 16);
            this.mTextMaxFit.Name = "mTextMaxFit";
            this.mTextMaxFit.Size = new System.Drawing.Size(40, 20);
            this.mTextMaxFit.TabIndex = 7;
            this.mTextMaxFit.Text = "0.1";
            this.mTextMaxFit.Enter += new System.EventHandler(this.textBox_max_fit_Enter);
            // 
            // label5
            // 
            this.label5.Location = new System.Drawing.Point(8, 48);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(168, 16);
            this.label5.TabIndex = 17;
            this.label5.Text = "Threshold Intensity for Deletion";
            // 
            // label6
            // 
            this.label6.Location = new System.Drawing.Point(256, 48);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(152, 16);
            this.label6.TabIndex = 20;
            this.label6.Text = "Threshold intensity for score";
            // 
            // mTextDeletionIntensityThreshold
            // 
            this.mTextDeletionIntensityThreshold.Location = new System.Drawing.Point(192, 48);
            this.mTextDeletionIntensityThreshold.Name = "mTextDeletionIntensityThreshold";
            this.mTextDeletionIntensityThreshold.Size = new System.Drawing.Size(40, 20);
            this.mTextDeletionIntensityThreshold.TabIndex = 18;
            this.mTextDeletionIntensityThreshold.Text = "10";
            this.mTextDeletionIntensityThreshold.Leave += new System.EventHandler(this.textBox_IntDeletion_Leave);
            this.mTextDeletionIntensityThreshold.Enter += new System.EventHandler(this.textBox_IntDeletion_Enter);
            // 
            // mTextScoreIntensityThreshold
            // 
            this.mTextScoreIntensityThreshold.Location = new System.Drawing.Point(424, 48);
            this.mTextScoreIntensityThreshold.Name = "mTextScoreIntensityThreshold";
            this.mTextScoreIntensityThreshold.Size = new System.Drawing.Size(40, 20);
            this.mTextScoreIntensityThreshold.TabIndex = 19;
            this.mTextScoreIntensityThreshold.Text = "10";
            this.mTextScoreIntensityThreshold.Leave += new System.EventHandler(this.textBox_IntScore_Leave);
            this.mTextScoreIntensityThreshold.Enter += new System.EventHandler(this.textBox_IntScore_Enter);
            // 
            // pictureBox_ThScore
            // 
            this.pictureBox_ThScore.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox_ThScore.Image")));
            this.pictureBox_ThScore.Location = new System.Drawing.Point(16, 24);
            this.pictureBox_ThScore.Name = "pictureBox_ThScore";
            this.pictureBox_ThScore.Size = new System.Drawing.Size(472, 264);
            this.pictureBox_ThScore.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox_ThScore.TabIndex = 22;
            this.pictureBox_ThScore.TabStop = false;
            this.pictureBox_ThScore.Visible = false;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label_ThDeletion_tip);
            this.groupBox1.Controls.Add(this.label_max_fit_tip);
            this.groupBox1.Controls.Add(this.pictureBox_ThDeletion);
            this.groupBox1.Controls.Add(this.pictureBox_ThScore);
            this.groupBox1.Controls.Add(this.label_thscore_tip);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(3, 88);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(514, 365);
            this.groupBox1.TabIndex = 24;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Helpful Tips";
            // 
            // label_ThDeletion_tip
            // 
            this.label_ThDeletion_tip.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.5F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
            this.label_ThDeletion_tip.Location = new System.Drawing.Point(8, 296);
            this.label_ThDeletion_tip.Name = "label_ThDeletion_tip";
            this.label_ThDeletion_tip.Size = new System.Drawing.Size(472, 64);
            this.label_ThDeletion_tip.TabIndex = 24;
            this.label_ThDeletion_tip.Text = @"Sets the intensity threshold (normalized i.e. 0-100) that determines which areas of a peak are to be deleted. For example, in the figure, if the threshold was set to 10, then the colored regions of the peak (that correspond to 10-100) are kept while the others are deleted";
            this.label_ThDeletion_tip.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.label_ThDeletion_tip.Visible = false;
            // 
            // label_max_fit_tip
            // 
            this.label_max_fit_tip.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.5F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
            this.label_max_fit_tip.Location = new System.Drawing.Point(16, 304);
            this.label_max_fit_tip.Name = "label_max_fit_tip";
            this.label_max_fit_tip.Size = new System.Drawing.Size(432, 40);
            this.label_max_fit_tip.TabIndex = 0;
            this.label_max_fit_tip.Text = "Maximum Fit: Sets the maximum fit between isotopic peaks (0-1)";
            this.label_max_fit_tip.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // pictureBox_ThDeletion
            // 
            this.pictureBox_ThDeletion.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox_ThDeletion.Image")));
            this.pictureBox_ThDeletion.Location = new System.Drawing.Point(16, 24);
            this.pictureBox_ThDeletion.Name = "pictureBox_ThDeletion";
            this.pictureBox_ThDeletion.Size = new System.Drawing.Size(464, 256);
            this.pictureBox_ThDeletion.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox_ThDeletion.TabIndex = 23;
            this.pictureBox_ThDeletion.TabStop = false;
            this.pictureBox_ThDeletion.Visible = false;
            // 
            // label_thscore_tip
            // 
            this.label_thscore_tip.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.5F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
            this.label_thscore_tip.Location = new System.Drawing.Point(8, 296);
            this.label_thscore_tip.Name = "label_thscore_tip";
            this.label_thscore_tip.Size = new System.Drawing.Size(480, 64);
            this.label_thscore_tip.TabIndex = 25;
            this.label_thscore_tip.Text = "Sets the minimum normalized intensity (0-100) for selection of the area of the pe" +
                "ak that will be used for fit calculation.  For example, in the figure, for a thr" +
                "eshold value of 10, the colored regions will be considered during fit calculatio" +
                "ns.";
            this.label_thscore_tip.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.label_thscore_tip.Visible = false;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.groupBox1);
            this.groupBox2.Controls.Add(this.groupBox3);
            this.groupBox2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox2.Location = new System.Drawing.Point(0, 0);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(520, 456);
            this.groupBox2.TabIndex = 27;
            this.groupBox2.TabStop = false;
            this.groupBox2.Enter += new System.EventHandler(this.groupBox2_Enter);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.label3);
            this.groupBox3.Controls.Add(this.mTextMaxFit);
            this.groupBox3.Controls.Add(this.label5);
            this.groupBox3.Controls.Add(this.label6);
            this.groupBox3.Controls.Add(this.mTextDeletionIntensityThreshold);
            this.groupBox3.Controls.Add(this.mTextScoreIntensityThreshold);
            this.groupBox3.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBox3.Location = new System.Drawing.Point(3, 16);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(514, 72);
            this.groupBox3.TabIndex = 25;
            this.groupBox3.TabStop = false;
            // 
            // ctlThresholdSettings
            // 
            this.Controls.Add(this.groupBox2);
            this.Name = "ctlThresholdSettings";
            this.Size = new System.Drawing.Size(520, 456);
            this.groupBox1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.ResumeLayout(false);
        }
        #endregion

        private void textBox_max_fit_Enter(object sender, System.EventArgs e)
        {
        }

        private void textBox_IntScore_Leave(object sender, System.EventArgs e)
        {
            label_max_fit_tip.Visible = true;
            label_max_fit_tip.Dock = DockStyle.Bottom;
            pictureBox_ThDeletion.Visible = false;
            pictureBox_ThScore.Visible = false;
            label_ThDeletion_tip.Visible = false;
            label_thscore_tip.Visible = false;
        }

        private void textBox_IntScore_Enter(object sender, System.EventArgs e)
        {
            label_max_fit_tip.Visible = false;
            pictureBox_ThDeletion.Visible = false;
            pictureBox_ThScore.Visible = true;
            label_ThDeletion_tip.Visible = false;
            label_thscore_tip.Visible = true;
            label_thscore_tip.Dock = DockStyle.Bottom;
        }

        private void textBox_IntDeletion_Enter(object sender, System.EventArgs e)
        {
            label_max_fit_tip.Visible = false;
            pictureBox_ThDeletion.Visible = true;
            label_ThDeletion_tip.Visible = true;
            pictureBox_ThScore.Visible = false;
            label_thscore_tip.Visible = false;
        }

        private void textBox_IntDeletion_Leave(object sender, System.EventArgs e)
        {
            label_max_fit_tip.Visible = true;
            pictureBox_ThDeletion.Visible = false;
            label_ThDeletion_tip.Visible = false;
            pictureBox_ThScore.Visible = false;
            label_thscore_tip.Visible = false;
        }

        private void groupBox2_Enter(object sender, System.EventArgs e)
        {
        }

        public double DeleteIntensityThreshold
        {
            get
            {
                return Convert.ToDouble(mTextDeletionIntensityThreshold.Text);
            }
            set
            {
                mTextDeletionIntensityThreshold.Text = Convert.ToString(value);
            }
        }
        public double MaxFit
        {
            get
            {
                return Convert.ToDouble(mTextMaxFit.Text);
            }
            set
            {
                mTextMaxFit.Text = Convert.ToString(value);
            }
        }
        public double MinIntensityForScore
        {
            get
            {
                return Convert.ToDouble(mTextScoreIntensityThreshold.Text);
            }
            set
            {
                mTextScoreIntensityThreshold.Text = Convert.ToString(value);
            }
        }
    }
}
