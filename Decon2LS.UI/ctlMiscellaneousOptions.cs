// Written by Navdeep Jaitly and Anoop Mayampurath for the Department of Energy (PNNL, Richland, WA)
// Copyright 2006, Battelle Memorial Institute
// E-mail: navdeep.jaitly@pnl.gov
// Website: http://omics.pnl.gov/software or http://panomics.pnnl.gov
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
    /// Summary description for ctlMiscellaneousOptions.
    /// </summary>
    public class ctlMiscellaneousOptions : System.Windows.Forms.UserControl
    {
        private System.Windows.Forms.Panel mpanelScanOptions;
        private System.Windows.Forms.TextBox mtxtMinScan;
        private System.Windows.Forms.TextBox mtxtMaxScan;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.CheckBox mCheckUseScanRange;
        private System.Windows.Forms.Label mlabelMinScan;
        private System.Windows.Forms.Label mlabelMaxScan;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.CheckBox mChkSmoothing;
        private System.Windows.Forms.GroupBox mgroupBoxSavGol;
        private System.Windows.Forms.TextBox mtxtRight;
        private System.Windows.Forms.Label mlblRight;
        private System.Windows.Forms.TextBox mtxtLeft;
        private System.Windows.Forms.Label mlblLeft;
        private System.Windows.Forms.TextBox mtxtOrder;
        private System.Windows.Forms.Label mlblOrder;
        private short SGMaxPts = 50 ; 
        private short SGMaxOrder = 5 ;
        private short MaxNumZerosToFill = 150 ; 
        private System.Windows.Forms.TextBox mtxtMaxMZ;
        private System.Windows.Forms.Label mlabelMaxMZ;
        private System.Windows.Forms.TextBox mtxtMinMZ;
        private System.Windows.Forms.Label mlabelMinMZ;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.CheckBox mCheckUseMZRange;
        private System.Windows.Forms.Panel mpanelMZOptions;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.CheckBox mChkZeroFill;
        private System.Windows.Forms.GroupBox mgroupZeroFill;
        private System.Windows.Forms.Label mlblZeroFill;
        private System.Windows.Forms.TextBox mtxtZeros;
        private System.Windows.Forms.Panel panelMSMS;
        private System.Windows.Forms.CheckBox mchkProcessMSMS;
        private System.Windows.Forms.GroupBox groupBoxMSMS; 
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        public ctlMiscellaneousOptions()
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();

            // TODO: Add any initialization after the InitializeComponent call

        }

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose( bool disposing )
        {
            if( disposing )
            {
                if(components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose( disposing );
        }

        #region Component Designer generated code
        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.mpanelScanOptions = new System.Windows.Forms.Panel();
            this.mtxtMaxScan = new System.Windows.Forms.TextBox();
            this.mlabelMaxScan = new System.Windows.Forms.Label();
            this.mtxtMinScan = new System.Windows.Forms.TextBox();
            this.mlabelMinScan = new System.Windows.Forms.Label();
            this.panel2 = new System.Windows.Forms.Panel();
            this.mCheckUseScanRange = new System.Windows.Forms.CheckBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.mgroupBoxSavGol = new System.Windows.Forms.GroupBox();
            this.mtxtOrder = new System.Windows.Forms.TextBox();
            this.mlblOrder = new System.Windows.Forms.Label();
            this.mtxtRight = new System.Windows.Forms.TextBox();
            this.mlblRight = new System.Windows.Forms.Label();
            this.mtxtLeft = new System.Windows.Forms.TextBox();
            this.mlblLeft = new System.Windows.Forms.Label();
            this.mChkSmoothing = new System.Windows.Forms.CheckBox();
            this.mpanelMZOptions = new System.Windows.Forms.Panel();
            this.mtxtMaxMZ = new System.Windows.Forms.TextBox();
            this.mlabelMaxMZ = new System.Windows.Forms.Label();
            this.mtxtMinMZ = new System.Windows.Forms.TextBox();
            this.mlabelMinMZ = new System.Windows.Forms.Label();
            this.panel4 = new System.Windows.Forms.Panel();
            this.mCheckUseMZRange = new System.Windows.Forms.CheckBox();
            this.panel3 = new System.Windows.Forms.Panel();
            this.mgroupZeroFill = new System.Windows.Forms.GroupBox();
            this.mtxtZeros = new System.Windows.Forms.TextBox();
            this.mlblZeroFill = new System.Windows.Forms.Label();
            this.mChkZeroFill = new System.Windows.Forms.CheckBox();
            this.panelMSMS = new System.Windows.Forms.Panel();
            this.groupBoxMSMS = new System.Windows.Forms.GroupBox();
            this.mchkProcessMSMS = new System.Windows.Forms.CheckBox();
            this.mpanelScanOptions.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel1.SuspendLayout();
            this.mgroupBoxSavGol.SuspendLayout();
            this.mpanelMZOptions.SuspendLayout();
            this.panel4.SuspendLayout();
            this.panel3.SuspendLayout();
            this.mgroupZeroFill.SuspendLayout();
            this.panelMSMS.SuspendLayout();
            this.groupBoxMSMS.SuspendLayout();
            this.SuspendLayout();
            // 
            // mpanelScanOptions
            // 
            this.mpanelScanOptions.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.mpanelScanOptions.Controls.Add(this.mtxtMaxScan);
            this.mpanelScanOptions.Controls.Add(this.mlabelMaxScan);
            this.mpanelScanOptions.Controls.Add(this.mtxtMinScan);
            this.mpanelScanOptions.Controls.Add(this.mlabelMinScan);
            this.mpanelScanOptions.Dock = System.Windows.Forms.DockStyle.Top;
            this.mpanelScanOptions.DockPadding.All = 5;
            this.mpanelScanOptions.Enabled = false;
            this.mpanelScanOptions.Location = new System.Drawing.Point(0, 40);
            this.mpanelScanOptions.Name = "mpanelScanOptions";
            this.mpanelScanOptions.Size = new System.Drawing.Size(528, 32);
            this.mpanelScanOptions.TabIndex = 0;
            // 
            // mtxtMaxScan
            // 
            this.mtxtMaxScan.Dock = System.Windows.Forms.DockStyle.Left;
            this.mtxtMaxScan.Enabled = false;
            this.mtxtMaxScan.Location = new System.Drawing.Point(198, 5);
            this.mtxtMaxScan.Name = "mtxtMaxScan";
            this.mtxtMaxScan.Size = new System.Drawing.Size(59, 20);
            this.mtxtMaxScan.TabIndex = 3;
            this.mtxtMaxScan.Text = "1000";
            // 
            // mlabelMaxScan
            // 
            this.mlabelMaxScan.Dock = System.Windows.Forms.DockStyle.Left;
            this.mlabelMaxScan.Enabled = false;
            this.mlabelMaxScan.Location = new System.Drawing.Point(131, 5);
            this.mlabelMaxScan.Name = "mlabelMaxScan";
            this.mlabelMaxScan.Size = new System.Drawing.Size(67, 20);
            this.mlabelMaxScan.TabIndex = 2;
            this.mlabelMaxScan.Text = "Max Scan:";
            this.mlabelMaxScan.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // mtxtMinScan
            // 
            this.mtxtMinScan.Dock = System.Windows.Forms.DockStyle.Left;
            this.mtxtMinScan.Enabled = false;
            this.mtxtMinScan.Location = new System.Drawing.Point(72, 5);
            this.mtxtMinScan.Name = "mtxtMinScan";
            this.mtxtMinScan.Size = new System.Drawing.Size(59, 20);
            this.mtxtMinScan.TabIndex = 1;
            this.mtxtMinScan.Text = "1";
            // 
            // mlabelMinScan
            // 
            this.mlabelMinScan.Dock = System.Windows.Forms.DockStyle.Left;
            this.mlabelMinScan.Enabled = false;
            this.mlabelMinScan.Location = new System.Drawing.Point(5, 5);
            this.mlabelMinScan.Name = "mlabelMinScan";
            this.mlabelMinScan.Size = new System.Drawing.Size(67, 20);
            this.mlabelMinScan.TabIndex = 0;
            this.mlabelMinScan.Text = "Min Scan:";
            this.mlabelMinScan.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // panel2
            // 
            this.panel2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel2.Controls.Add(this.mCheckUseScanRange);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel2.Location = new System.Drawing.Point(0, 0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(528, 40);
            this.panel2.TabIndex = 1;
            // 
            // mCheckUseScanRange
            // 
            this.mCheckUseScanRange.Location = new System.Drawing.Point(8, 8);
            this.mCheckUseScanRange.Name = "mCheckUseScanRange";
            this.mCheckUseScanRange.Size = new System.Drawing.Size(200, 24);
            this.mCheckUseScanRange.TabIndex = 0;
            this.mCheckUseScanRange.Text = "Specify Scan Range to Deisotope";
            this.mCheckUseScanRange.CheckedChanged += new System.EventHandler(this.mCheckUseScanRange_CheckedChanged);
            // 
            // panel1
            // 
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Controls.Add(this.mgroupBoxSavGol);
            this.panel1.Controls.Add(this.mChkSmoothing);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.DockPadding.All = 5;
            this.panel1.Location = new System.Drawing.Point(0, 144);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(528, 96);
            this.panel1.TabIndex = 2;
            // 
            // mgroupBoxSavGol
            // 
            this.mgroupBoxSavGol.Controls.Add(this.mtxtOrder);
            this.mgroupBoxSavGol.Controls.Add(this.mlblOrder);
            this.mgroupBoxSavGol.Controls.Add(this.mtxtRight);
            this.mgroupBoxSavGol.Controls.Add(this.mlblRight);
            this.mgroupBoxSavGol.Controls.Add(this.mtxtLeft);
            this.mgroupBoxSavGol.Controls.Add(this.mlblLeft);
            this.mgroupBoxSavGol.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mgroupBoxSavGol.Location = new System.Drawing.Point(5, 29);
            this.mgroupBoxSavGol.Name = "mgroupBoxSavGol";
            this.mgroupBoxSavGol.Size = new System.Drawing.Size(516, 60);
            this.mgroupBoxSavGol.TabIndex = 1;
            this.mgroupBoxSavGol.TabStop = false;
            this.mgroupBoxSavGol.Text = "Savitzky Golay Options";
            // 
            // mtxtOrder
            // 
            this.mtxtOrder.Dock = System.Windows.Forms.DockStyle.Left;
            this.mtxtOrder.Enabled = false;
            this.mtxtOrder.Location = new System.Drawing.Point(275, 16);
            this.mtxtOrder.Name = "mtxtOrder";
            this.mtxtOrder.Size = new System.Drawing.Size(37, 20);
            this.mtxtOrder.TabIndex = 9;
            this.mtxtOrder.Text = "2";
            // 
            // mlblOrder
            // 
            this.mlblOrder.Dock = System.Windows.Forms.DockStyle.Left;
            this.mlblOrder.Enabled = false;
            this.mlblOrder.Location = new System.Drawing.Point(208, 16);
            this.mlblOrder.Name = "mlblOrder";
            this.mlblOrder.Size = new System.Drawing.Size(67, 41);
            this.mlblOrder.TabIndex = 8;
            this.mlblOrder.Text = "Order:";
            this.mlblOrder.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // mtxtRight
            // 
            this.mtxtRight.Dock = System.Windows.Forms.DockStyle.Left;
            this.mtxtRight.Enabled = false;
            this.mtxtRight.Location = new System.Drawing.Point(179, 16);
            this.mtxtRight.Name = "mtxtRight";
            this.mtxtRight.Size = new System.Drawing.Size(29, 20);
            this.mtxtRight.TabIndex = 7;
            this.mtxtRight.Text = "2";
            // 
            // mlblRight
            // 
            this.mlblRight.Dock = System.Windows.Forms.DockStyle.Left;
            this.mlblRight.Enabled = false;
            this.mlblRight.Location = new System.Drawing.Point(112, 16);
            this.mlblRight.Name = "mlblRight";
            this.mlblRight.Size = new System.Drawing.Size(67, 41);
            this.mlblRight.TabIndex = 6;
            this.mlblRight.Text = "Num Right:";
            this.mlblRight.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // mtxtLeft
            // 
            this.mtxtLeft.Dock = System.Windows.Forms.DockStyle.Left;
            this.mtxtLeft.Enabled = false;
            this.mtxtLeft.Location = new System.Drawing.Point(70, 16);
            this.mtxtLeft.Name = "mtxtLeft";
            this.mtxtLeft.Size = new System.Drawing.Size(42, 20);
            this.mtxtLeft.TabIndex = 5;
            this.mtxtLeft.Text = "2";
            // 
            // mlblLeft
            // 
            this.mlblLeft.Dock = System.Windows.Forms.DockStyle.Left;
            this.mlblLeft.Enabled = false;
            this.mlblLeft.Location = new System.Drawing.Point(3, 16);
            this.mlblLeft.Name = "mlblLeft";
            this.mlblLeft.Size = new System.Drawing.Size(67, 41);
            this.mlblLeft.TabIndex = 4;
            this.mlblLeft.Text = "Num Left:";
            this.mlblLeft.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // mChkSmoothing
            // 
            this.mChkSmoothing.Dock = System.Windows.Forms.DockStyle.Top;
            this.mChkSmoothing.Location = new System.Drawing.Point(5, 5);
            this.mChkSmoothing.Name = "mChkSmoothing";
            this.mChkSmoothing.Size = new System.Drawing.Size(516, 24);
            this.mChkSmoothing.TabIndex = 0;
            this.mChkSmoothing.Text = "Use Savitzky Golay to Smooth Data:";
            this.mChkSmoothing.CheckedChanged += new System.EventHandler(this.mChkSmoothing_CheckedChanged);
            // 
            // mpanelMZOptions
            // 
            this.mpanelMZOptions.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.mpanelMZOptions.Controls.Add(this.mtxtMaxMZ);
            this.mpanelMZOptions.Controls.Add(this.mlabelMaxMZ);
            this.mpanelMZOptions.Controls.Add(this.mtxtMinMZ);
            this.mpanelMZOptions.Controls.Add(this.mlabelMinMZ);
            this.mpanelMZOptions.Dock = System.Windows.Forms.DockStyle.Top;
            this.mpanelMZOptions.DockPadding.All = 5;
            this.mpanelMZOptions.Enabled = false;
            this.mpanelMZOptions.Location = new System.Drawing.Point(0, 112);
            this.mpanelMZOptions.Name = "mpanelMZOptions";
            this.mpanelMZOptions.Size = new System.Drawing.Size(528, 32);
            this.mpanelMZOptions.TabIndex = 3;
            // 
            // mtxtMaxMZ
            // 
            this.mtxtMaxMZ.Dock = System.Windows.Forms.DockStyle.Left;
            this.mtxtMaxMZ.Enabled = false;
            this.mtxtMaxMZ.Location = new System.Drawing.Point(198, 5);
            this.mtxtMaxMZ.Name = "mtxtMaxMZ";
            this.mtxtMaxMZ.Size = new System.Drawing.Size(59, 20);
            this.mtxtMaxMZ.TabIndex = 3;
            this.mtxtMaxMZ.Text = "2000";
            // 
            // mlabelMaxMZ
            // 
            this.mlabelMaxMZ.Dock = System.Windows.Forms.DockStyle.Left;
            this.mlabelMaxMZ.Enabled = false;
            this.mlabelMaxMZ.Location = new System.Drawing.Point(131, 5);
            this.mlabelMaxMZ.Name = "mlabelMaxMZ";
            this.mlabelMaxMZ.Size = new System.Drawing.Size(67, 20);
            this.mlabelMaxMZ.TabIndex = 2;
            this.mlabelMaxMZ.Text = "Max m/z:";
            this.mlabelMaxMZ.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // mtxtMinMZ
            // 
            this.mtxtMinMZ.Dock = System.Windows.Forms.DockStyle.Left;
            this.mtxtMinMZ.Enabled = false;
            this.mtxtMinMZ.Location = new System.Drawing.Point(72, 5);
            this.mtxtMinMZ.Name = "mtxtMinMZ";
            this.mtxtMinMZ.Size = new System.Drawing.Size(59, 20);
            this.mtxtMinMZ.TabIndex = 1;
            this.mtxtMinMZ.Text = "400";
            // 
            // mlabelMinMZ
            // 
            this.mlabelMinMZ.Dock = System.Windows.Forms.DockStyle.Left;
            this.mlabelMinMZ.Enabled = false;
            this.mlabelMinMZ.Location = new System.Drawing.Point(5, 5);
            this.mlabelMinMZ.Name = "mlabelMinMZ";
            this.mlabelMinMZ.Size = new System.Drawing.Size(67, 20);
            this.mlabelMinMZ.TabIndex = 0;
            this.mlabelMinMZ.Text = "Min m/z:";
            this.mlabelMinMZ.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // panel4
            // 
            this.panel4.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel4.Controls.Add(this.mCheckUseMZRange);
            this.panel4.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel4.Location = new System.Drawing.Point(0, 72);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(528, 40);
            this.panel4.TabIndex = 4;
            // 
            // mCheckUseMZRange
            // 
            this.mCheckUseMZRange.Location = new System.Drawing.Point(8, 8);
            this.mCheckUseMZRange.Name = "mCheckUseMZRange";
            this.mCheckUseMZRange.Size = new System.Drawing.Size(200, 24);
            this.mCheckUseMZRange.TabIndex = 0;
            this.mCheckUseMZRange.Text = "Specify MZ Range to Deisotope";
            this.mCheckUseMZRange.CheckedChanged += new System.EventHandler(this.mCheckUseMZRange_CheckedChanged);
            // 
            // panel3
            // 
            this.panel3.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel3.Controls.Add(this.mgroupZeroFill);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel3.Location = new System.Drawing.Point(0, 240);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(528, 48);
            this.panel3.TabIndex = 5;
            // 
            // mgroupZeroFill
            // 
            this.mgroupZeroFill.Controls.Add(this.mtxtZeros);
            this.mgroupZeroFill.Controls.Add(this.mlblZeroFill);
            this.mgroupZeroFill.Controls.Add(this.mChkZeroFill);
            this.mgroupZeroFill.Dock = System.Windows.Forms.DockStyle.Top;
            this.mgroupZeroFill.Location = new System.Drawing.Point(0, 0);
            this.mgroupZeroFill.Name = "mgroupZeroFill";
            this.mgroupZeroFill.Size = new System.Drawing.Size(526, 40);
            this.mgroupZeroFill.TabIndex = 1;
            this.mgroupZeroFill.TabStop = false;
            this.mgroupZeroFill.Text = "Zero Fill Options";
            // 
            // mtxtZeros
            // 
            this.mtxtZeros.Dock = System.Windows.Forms.DockStyle.Left;
            this.mtxtZeros.Enabled = false;
            this.mtxtZeros.Location = new System.Drawing.Point(256, 16);
            this.mtxtZeros.Name = "mtxtZeros";
            this.mtxtZeros.Size = new System.Drawing.Size(40, 20);
            this.mtxtZeros.TabIndex = 4;
            this.mtxtZeros.Text = "3";
            // 
            // mlblZeroFill
            // 
            this.mlblZeroFill.Dock = System.Windows.Forms.DockStyle.Left;
            this.mlblZeroFill.Enabled = false;
            this.mlblZeroFill.Location = new System.Drawing.Point(120, 16);
            this.mlblZeroFill.Name = "mlblZeroFill";
            this.mlblZeroFill.Size = new System.Drawing.Size(136, 21);
            this.mlblZeroFill.TabIndex = 3;
            this.mlblZeroFill.Text = "Number of Zeros To Fill";
            this.mlblZeroFill.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // mChkZeroFill
            // 
            this.mChkZeroFill.Dock = System.Windows.Forms.DockStyle.Left;
            this.mChkZeroFill.Location = new System.Drawing.Point(3, 16);
            this.mChkZeroFill.Name = "mChkZeroFill";
            this.mChkZeroFill.Size = new System.Drawing.Size(117, 21);
            this.mChkZeroFill.TabIndex = 0;
            this.mChkZeroFill.Text = "Zero Fill Spectra";
            this.mChkZeroFill.CheckedChanged += new System.EventHandler(this.mCheckZeroFile_CheckedChanged);
            // 
            // panelMSMS
            // 
            this.panelMSMS.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelMSMS.Controls.Add(this.groupBoxMSMS);
            this.panelMSMS.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelMSMS.Location = new System.Drawing.Point(0, 288);
            this.panelMSMS.Name = "panelMSMS";
            this.panelMSMS.Size = new System.Drawing.Size(528, 48);
            this.panelMSMS.TabIndex = 6;
            // 
            // groupBoxMSMS
            // 
            this.groupBoxMSMS.Controls.Add(this.mchkProcessMSMS);
            this.groupBoxMSMS.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBoxMSMS.Location = new System.Drawing.Point(0, 0);
            this.groupBoxMSMS.Name = "groupBoxMSMS";
            this.groupBoxMSMS.Size = new System.Drawing.Size(526, 40);
            this.groupBoxMSMS.TabIndex = 1;
            this.groupBoxMSMS.TabStop = false;
            this.groupBoxMSMS.Text = "MS/MS processing options";
            // 
            // mchkProcessMSMS
            // 
            this.mchkProcessMSMS.Dock = System.Windows.Forms.DockStyle.Left;
            this.mchkProcessMSMS.Location = new System.Drawing.Point(3, 16);
            this.mchkProcessMSMS.Name = "mchkProcessMSMS";
            this.mchkProcessMSMS.Size = new System.Drawing.Size(237, 21);
            this.mchkProcessMSMS.TabIndex = 0;
            this.mchkProcessMSMS.Text = "Deisotope High Resolution MS/MS";
            // 
            // ctlMiscellaneousOptions
            // 
            this.Controls.Add(this.panelMSMS);
            this.Controls.Add(this.panel3);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.mpanelMZOptions);
            this.Controls.Add(this.panel4);
            this.Controls.Add(this.mpanelScanOptions);
            this.Controls.Add(this.panel2);
            this.Name = "ctlMiscellaneousOptions";
            this.Size = new System.Drawing.Size(528, 352);
            this.mpanelScanOptions.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.mgroupBoxSavGol.ResumeLayout(false);
            this.mpanelMZOptions.ResumeLayout(false);
            this.panel4.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            this.mgroupZeroFill.ResumeLayout(false);
            this.panelMSMS.ResumeLayout(false);
            this.groupBoxMSMS.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        #endregion

        private void mCheckUseScanRange_CheckedChanged(object sender, System.EventArgs e)
        {
            mpanelScanOptions.Enabled = mCheckUseScanRange.Checked ; 
            mtxtMaxScan.Enabled = mCheckUseScanRange.Checked ; 
            mtxtMinScan.Enabled = mCheckUseScanRange.Checked ; 
            mlabelMinScan.Enabled = mCheckUseScanRange.Checked ; 
            mlabelMaxScan.Enabled = mCheckUseScanRange.Checked ; 
        }

        private void mCheckUseMZRange_CheckedChanged(object sender, System.EventArgs e)
        {
            mpanelMZOptions.Enabled = mCheckUseMZRange.Checked ; 
            mtxtMinMZ.Enabled = mCheckUseMZRange.Checked ; 
            mtxtMaxMZ.Enabled = mCheckUseMZRange.Checked ; 
            mlabelMinMZ.Enabled = mCheckUseMZRange.Checked ; 
            mlabelMaxMZ.Enabled = mCheckUseMZRange.Checked ; 
        }

        private void mChkSmoothing_CheckedChanged(object sender, System.EventArgs e)
        {
            mgroupBoxSavGol.Enabled = mChkSmoothing.Checked ; 
            mtxtLeft.Enabled = mChkSmoothing.Checked ; 
            mtxtRight.Enabled = mChkSmoothing.Checked ; 
            mtxtOrder.Enabled = mChkSmoothing.Checked ; 
            mlblLeft.Enabled = mChkSmoothing.Checked ; 
            mlblRight.Enabled = mChkSmoothing.Checked ; 
            mlblOrder.Enabled = mChkSmoothing.Checked ; 
        }

        private void mCheckZeroFile_CheckedChanged(object sender, System.EventArgs e)
        {
            //mgroupZeroFill.Enabled = mChkZeroFill.Checked ; //[gord] this disables the checkbox itself
            mtxtZeros.Enabled = mChkZeroFill.Checked ; 
            mlblZeroFill.Enabled = mChkZeroFill.Checked ; 
            mtxtZeros.Enabled = mChkZeroFill.Checked ; 
        }

        public bool ZeroFill
        {
            get
            {
                return mChkZeroFill.Checked ; 
            }
            set
            {
                mChkZeroFill.Checked = value ; 
            }
        }
        public short NumZerosToFill
        {
            get
            {
                return Convert.ToInt16(mtxtZeros.Text) ; 
            }
            set
            {
                mtxtZeros.Text = Convert.ToString(value) ; 
            }
        }
    
        public bool UseScanRange
        {
            get
            {
                return mCheckUseScanRange.Checked ; 
            }
            set
            {
                mCheckUseScanRange.Checked = value ; 
            }
        }
        public int MinScan
        {
            get
            {
                return Convert.ToInt32(mtxtMinScan.Text) ; 
            }
            set
            {
                mtxtMinScan.Text = Convert.ToString(value) ; 
            }
        }
        public int MaxScan
        {
            get
            {
                return Convert.ToInt32(mtxtMaxScan.Text) ; 
            }
            set
            {
                mtxtMaxScan.Text = Convert.ToString(value) ; 
            }
        }

        public bool UseMZRange
        {
            get
            {
                return mCheckUseMZRange.Checked ; 
            }
            set
            {
                mCheckUseMZRange.Checked = value ; 
            }
        }
        public double MinMZ
        {
            get
            {
                return Convert.ToDouble(mtxtMinMZ.Text) ; 
            }
            set
            {
                mtxtMinMZ.Text = Convert.ToString(value) ; 
            }
        }
        public double MaxMZ
        {
            get
            {
                return Convert.ToDouble(mtxtMaxMZ.Text) ; 
            }
            set
            {
                mtxtMaxMZ.Text = Convert.ToString(value) ; 
            }
        }

        
        public bool UseSavitzkyGolaySmooth
        {
            get
            {
                return mChkSmoothing.Checked ; 
            }
            set
            {
                mChkSmoothing.Checked = value ; 
            }
        }
        public short SGNumLeft
        {
            get
            {
                return Convert.ToInt16(mtxtLeft.Text) ; 
            }
            set
            {
                mtxtLeft.Text = Convert.ToString(value) ; 
            }
        }
        public short SGNumRight
        {
            get
            {
                return Convert.ToInt16(mtxtRight.Text) ; 
            }
            set
            {
                mtxtRight.Text = Convert.ToString(value) ; 
            }
        }
        public short SGOrder
        {
            get
            {
                return Convert.ToInt16(mtxtOrder.Text) ; 
            }
            set
            {
                mtxtOrder.Text = Convert.ToString(value) ; 
            }
        }
        

        public void SanityCheck()
        {
            if (SGNumLeft > SGMaxPts)
            {
                MessageBox.Show(this, "Can have at most " + Convert.ToString(SGMaxPts) + " points for smoothing") ; 
                SGNumLeft = SGMaxPts ; 
            }
            if (SGNumRight > SGMaxPts)
            {
                MessageBox.Show(this, "Can have at most " + Convert.ToString(SGMaxPts) + " points for smoothing") ; 
                SGNumRight = SGMaxPts ; 
            }
            if (SGOrder > SGMaxOrder)
            {
                MessageBox.Show(this, "Maximum value of order = " + Convert.ToString(SGMaxOrder)) ; 
                SGOrder = SGMaxOrder ; 
            }
            if (SGNumLeft + SGNumRight < SGOrder)
            {
                MessageBox.Show(this, "Savitzky Golay Order can be at most equal to # left plus # right. Using updated value.") ; 
                var val = SGNumLeft ;
                val += SGNumRight ; 
                SGOrder = val ; 
            }
            if (NumZerosToFill > MaxNumZerosToFill)
            {
                MessageBox.Show(this, "Maximum value of number of zeros to fill = " + Convert.ToString(MaxNumZerosToFill)) ; 
                SGOrder = MaxNumZerosToFill ; 
            }

        }

        public bool ProcessMSMS
        {
            get
            {
                return mchkProcessMSMS.Checked ; 
            }
            set
            {
                mchkProcessMSMS.Checked = value ; 
            }
        }


    }
}
