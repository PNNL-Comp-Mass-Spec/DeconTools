// -------------------------------------------------------------------------------
// Written by Navdeep Jaitly and Anoop Mayampurath for the Department of Energy (PNNL, Richland, WA)
// Copyright 2006, Battelle Memorial Institute
// E-mail: navdeep.jaitly@pnl.gov
// Website: https://omics.pnl.gov/software or http://panomics.pnnl.gov
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
    /// Summary description for ctlDTASettings.
    /// </summary>
    public class ctlDTASettings : System.Windows.Forms.UserControl
    {
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox mcmbOutputType;
        private System.Windows.Forms.CheckBox mchkConsiderCharge;
        private System.Windows.Forms.TextBox mtxtConsiderCharge;
        private System.Windows.Forms.Label mlabelChargeToConsider;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox mtxtMinScan;
        private System.Windows.Forms.TextBox mtxtMaxScan;
        private System.Windows.Forms.TextBox mtxtMinMass;
        private System.Windows.Forms.TextBox mtxtMinIonCount;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox mtxtCCMass;
        private System.Windows.Forms.TextBox mtxtProtonMass;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox mTxtWindowSize;
        private System.Windows.Forms.Label mlblMinScanHelp;
        private System.Windows.Forms.Label mlblOutputTypeHelp;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.Label mlblOutputTypeDTAHelp;
        private System.Windows.Forms.Label mlblOutputTypeLogHelp;
        private System.Windows.Forms.Label mlbOutputTypeMGFHelp;
        private System.Windows.Forms.Label mlblOutputTypeCDTAHelp;
        private System.Windows.Forms.Label mlblMaxScanHelp;
        private System.Windows.Forms.Label mlblMinMassHelp;
        private System.Windows.Forms.Label mlblMaxMassHelp;
        private System.Windows.Forms.Label mlbConsiderChargeHelp;
        private System.Windows.Forms.Label mlblCCMassHelp;
        private System.Windows.Forms.Label mlblProtonMassHelp;
        private System.Windows.Forms.Label mlblWindowSizeHelp;
        private System.Windows.Forms.TextBox mtxtMaxMass;
        private System.Windows.Forms.Label mlblMinIonCountHelp;
        private System.Windows.Forms.Panel panel1;
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        public ctlDTASettings()
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
            this.label1 = new System.Windows.Forms.Label();
            this.mcmbOutputType = new System.Windows.Forms.ComboBox();
            this.mchkConsiderCharge = new System.Windows.Forms.CheckBox();
            this.mtxtConsiderCharge = new System.Windows.Forms.TextBox();
            this.mlabelChargeToConsider = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.mtxtMinIonCount = new System.Windows.Forms.TextBox();
            this.mtxtMaxMass = new System.Windows.Forms.TextBox();
            this.mtxtMinMass = new System.Windows.Forms.TextBox();
            this.mtxtMaxScan = new System.Windows.Forms.TextBox();
            this.mtxtMinScan = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.mTxtWindowSize = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.mtxtProtonMass = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.mtxtCCMass = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.mlblMinScanHelp = new System.Windows.Forms.Label();
            this.mlblOutputTypeHelp = new System.Windows.Forms.Label();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.mlblMinIonCountHelp = new System.Windows.Forms.Label();
            this.mlblWindowSizeHelp = new System.Windows.Forms.Label();
            this.mlblProtonMassHelp = new System.Windows.Forms.Label();
            this.mlblCCMassHelp = new System.Windows.Forms.Label();
            this.mlbConsiderChargeHelp = new System.Windows.Forms.Label();
            this.mlblMaxMassHelp = new System.Windows.Forms.Label();
            this.mlblMinMassHelp = new System.Windows.Forms.Label();
            this.mlblMaxScanHelp = new System.Windows.Forms.Label();
            this.mlblOutputTypeCDTAHelp = new System.Windows.Forms.Label();
            this.mlbOutputTypeMGFHelp = new System.Windows.Forms.Label();
            this.mlblOutputTypeLogHelp = new System.Windows.Forms.Label();
            this.mlblOutputTypeDTAHelp = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(16, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(80, 24);
            this.label1.TabIndex = 0;
            this.label1.Text = "Output Type";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // mcmbOutputType
            // 
            this.mcmbOutputType.Items.AddRange(new object[] {
                                                                "DTA",
                                                                "LOG",
                                                                "MGF",
                                                                "CDTA"});
            this.mcmbOutputType.Location = new System.Drawing.Point(128, 16);
            this.mcmbOutputType.Name = "mcmbOutputType";
            this.mcmbOutputType.Size = new System.Drawing.Size(96, 21);
            this.mcmbOutputType.TabIndex = 1;
            this.mcmbOutputType.Click += new System.EventHandler(this.mcmbOutputType_Click);
            // 
            // mchkConsiderCharge
            // 
            this.mchkConsiderCharge.Location = new System.Drawing.Point(24, 48);
            this.mchkConsiderCharge.Name = "mchkConsiderCharge";
            this.mchkConsiderCharge.Size = new System.Drawing.Size(128, 32);
            this.mchkConsiderCharge.TabIndex = 2;
            this.mchkConsiderCharge.Text = "Consider Charge";
            this.mchkConsiderCharge.Enter += new System.EventHandler(this.mchkConsiderCharge_Enter);
            this.mchkConsiderCharge.CheckedChanged += new System.EventHandler(this.mchkConsiderCharge_CheckedChanged);
            // 
            // mtxtConsiderCharge
            // 
            this.mtxtConsiderCharge.Enabled = false;
            this.mtxtConsiderCharge.Location = new System.Drawing.Point(200, 88);
            this.mtxtConsiderCharge.Name = "mtxtConsiderCharge";
            this.mtxtConsiderCharge.Size = new System.Drawing.Size(24, 20);
            this.mtxtConsiderCharge.TabIndex = 3;
            this.mtxtConsiderCharge.Text = "0";
            // 
            // mlabelChargeToConsider
            // 
            this.mlabelChargeToConsider.Enabled = false;
            this.mlabelChargeToConsider.Location = new System.Drawing.Point(64, 88);
            this.mlabelChargeToConsider.Name = "mlabelChargeToConsider";
            this.mlabelChargeToConsider.Size = new System.Drawing.Size(120, 24);
            this.mlabelChargeToConsider.TabIndex = 4;
            this.mlabelChargeToConsider.Text = "Charge To Consider";
            this.mlabelChargeToConsider.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.mcmbOutputType);
            this.groupBox1.Controls.Add(this.mchkConsiderCharge);
            this.groupBox1.Controls.Add(this.mlabelChargeToConsider);
            this.groupBox1.Controls.Add(this.mtxtConsiderCharge);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Left;
            this.groupBox1.Location = new System.Drawing.Point(0, 0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(248, 120);
            this.groupBox1.TabIndex = 5;
            this.groupBox1.TabStop = false;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.mtxtMinIonCount);
            this.groupBox2.Controls.Add(this.mtxtMaxMass);
            this.groupBox2.Controls.Add(this.mtxtMinMass);
            this.groupBox2.Controls.Add(this.mtxtMaxScan);
            this.groupBox2.Controls.Add(this.mtxtMinScan);
            this.groupBox2.Controls.Add(this.label6);
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBox2.Location = new System.Drawing.Point(0, 0);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(488, 168);
            this.groupBox2.TabIndex = 6;
            this.groupBox2.TabStop = false;
            // 
            // mtxtMinIonCount
            // 
            this.mtxtMinIonCount.Location = new System.Drawing.Point(272, 120);
            this.mtxtMinIonCount.Name = "mtxtMinIonCount";
            this.mtxtMinIonCount.Size = new System.Drawing.Size(48, 20);
            this.mtxtMinIonCount.TabIndex = 9;
            this.mtxtMinIonCount.Text = "35";
            this.mtxtMinIonCount.Enter += new System.EventHandler(this.mtxtMinIonCountEnter);
            // 
            // mtxtMaxMass
            // 
            this.mtxtMaxMass.Location = new System.Drawing.Point(424, 72);
            this.mtxtMaxMass.Name = "mtxtMaxMass";
            this.mtxtMaxMass.Size = new System.Drawing.Size(48, 20);
            this.mtxtMaxMass.TabIndex = 8;
            this.mtxtMaxMass.Text = "5000";
            this.mtxtMaxMass.Enter += new System.EventHandler(this.mtxtMaxMassEnter);
            // 
            // mtxtMinMass
            // 
            this.mtxtMinMass.Location = new System.Drawing.Point(424, 24);
            this.mtxtMinMass.Name = "mtxtMinMass";
            this.mtxtMinMass.Size = new System.Drawing.Size(48, 20);
            this.mtxtMinMass.TabIndex = 7;
            this.mtxtMinMass.Text = "200";
            this.mtxtMinMass.Enter += new System.EventHandler(this.mtxtMinMassEnter);
            // 
            // mtxtMaxScan
            // 
            this.mtxtMaxScan.Location = new System.Drawing.Point(176, 72);
            this.mtxtMaxScan.Name = "mtxtMaxScan";
            this.mtxtMaxScan.Size = new System.Drawing.Size(48, 20);
            this.mtxtMaxScan.TabIndex = 6;
            this.mtxtMaxScan.Text = "1000000";
            this.mtxtMaxScan.Enter += new System.EventHandler(this.mtxtMaxScanEnter);
            // 
            // mtxtMinScan
            // 
            this.mtxtMinScan.Location = new System.Drawing.Point(176, 24);
            this.mtxtMinScan.Name = "mtxtMinScan";
            this.mtxtMinScan.Size = new System.Drawing.Size(48, 20);
            this.mtxtMinScan.TabIndex = 5;
            this.mtxtMinScan.Text = "1";
            this.mtxtMinScan.Enter += new System.EventHandler(this.mtxtMinScanEnter);
            // 
            // label6
            // 
            this.label6.Location = new System.Drawing.Point(16, 120);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(248, 32);
            this.label6.TabIndex = 4;
            this.label6.Text = "Minimum Ion Count for Spectra Consideration";
            this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label5
            // 
            this.label5.Location = new System.Drawing.Point(256, 72);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(152, 24);
            this.label5.TabIndex = 3;
            this.label5.Text = "Maximum Mass To Consider";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label4
            // 
            this.label4.Location = new System.Drawing.Point(256, 24);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(152, 24);
            this.label4.TabIndex = 2;
            this.label4.Text = "Minimum Mass To Consider";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(16, 72);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(152, 24);
            this.label3.TabIndex = 1;
            this.label3.Text = "Maximum Scan To Consider";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(16, 24);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(144, 24);
            this.label2.TabIndex = 0;
            this.label2.Text = "Miminum Scan To Consider";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.mTxtWindowSize);
            this.groupBox3.Controls.Add(this.label9);
            this.groupBox3.Controls.Add(this.mtxtProtonMass);
            this.groupBox3.Controls.Add(this.label8);
            this.groupBox3.Controls.Add(this.mtxtCCMass);
            this.groupBox3.Controls.Add(this.label7);
            this.groupBox3.Dock = System.Windows.Forms.DockStyle.Left;
            this.groupBox3.Location = new System.Drawing.Point(248, 0);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(232, 120);
            this.groupBox3.TabIndex = 7;
            this.groupBox3.TabStop = false;
            // 
            // mTxtWindowSize
            // 
            this.mTxtWindowSize.Location = new System.Drawing.Point(144, 80);
            this.mTxtWindowSize.Name = "mTxtWindowSize";
            this.mTxtWindowSize.Size = new System.Drawing.Size(40, 20);
            this.mTxtWindowSize.TabIndex = 5;
            this.mTxtWindowSize.Text = "5";
            this.mTxtWindowSize.Enter += new System.EventHandler(this.mTxtWindowSize_Enter);
            // 
            // label9
            // 
            this.label9.Location = new System.Drawing.Point(32, 80);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(88, 24);
            this.label9.TabIndex = 4;
            this.label9.Text = "Window Size";
            this.label9.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // mtxtProtonMass
            // 
            this.mtxtProtonMass.Location = new System.Drawing.Point(144, 48);
            this.mtxtProtonMass.Name = "mtxtProtonMass";
            this.mtxtProtonMass.Size = new System.Drawing.Size(72, 20);
            this.mtxtProtonMass.TabIndex = 3;
            this.mtxtProtonMass.Text = "1.00727638";
            this.mtxtProtonMass.Enter += new System.EventHandler(this.mtxtProtonMass_Enter);
            // 
            // label8
            // 
            this.label8.Location = new System.Drawing.Point(32, 48);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(72, 24);
            this.label8.TabIndex = 2;
            this.label8.Text = "Proton Mass";
            this.label8.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // mtxtCCMass
            // 
            this.mtxtCCMass.Location = new System.Drawing.Point(144, 16);
            this.mtxtCCMass.Name = "mtxtCCMass";
            this.mtxtCCMass.Size = new System.Drawing.Size(72, 20);
            this.mtxtCCMass.TabIndex = 1;
            this.mtxtCCMass.Text = "1.00727638";
            this.mtxtCCMass.Enter += new System.EventHandler(this.mtxtCCMass_Enter);
            // 
            // label7
            // 
            this.label7.Location = new System.Drawing.Point(32, 16);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(56, 24);
            this.label7.TabIndex = 0;
            this.label7.Text = "CC Mass";
            this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // mlblMinScanHelp
            // 
            this.mlblMinScanHelp.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
            this.mlblMinScanHelp.Location = new System.Drawing.Point(16, 24);
            this.mlblMinScanHelp.Name = "mlblMinScanHelp";
            this.mlblMinScanHelp.Size = new System.Drawing.Size(480, 32);
            this.mlblMinScanHelp.TabIndex = 8;
            this.mlblMinScanHelp.Text = "- Minimum of scan range for which DTAs are to be generated";
            this.mlblMinScanHelp.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // mlblOutputTypeHelp
            // 
            this.mlblOutputTypeHelp.Location = new System.Drawing.Point(8, 24);
            this.mlblOutputTypeHelp.Name = "mlblOutputTypeHelp";
            this.mlblOutputTypeHelp.Size = new System.Drawing.Size(152, 24);
            this.mlblOutputTypeHelp.TabIndex = 9;
            this.mlblOutputTypeHelp.Text = "- Choose the required output";
            this.mlblOutputTypeHelp.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.mlblOutputTypeHelp.Visible = false;
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.mlblMinIonCountHelp);
            this.groupBox4.Controls.Add(this.mlblWindowSizeHelp);
            this.groupBox4.Controls.Add(this.mlblProtonMassHelp);
            this.groupBox4.Controls.Add(this.mlblCCMassHelp);
            this.groupBox4.Controls.Add(this.mlbConsiderChargeHelp);
            this.groupBox4.Controls.Add(this.mlblMaxMassHelp);
            this.groupBox4.Controls.Add(this.mlblMinMassHelp);
            this.groupBox4.Controls.Add(this.mlblMaxScanHelp);
            this.groupBox4.Controls.Add(this.mlblOutputTypeCDTAHelp);
            this.groupBox4.Controls.Add(this.mlbOutputTypeMGFHelp);
            this.groupBox4.Controls.Add(this.mlblOutputTypeLogHelp);
            this.groupBox4.Controls.Add(this.mlblOutputTypeDTAHelp);
            this.groupBox4.Controls.Add(this.mlblMinScanHelp);
            this.groupBox4.Controls.Add(this.mlblOutputTypeHelp);
            this.groupBox4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
            this.groupBox4.Location = new System.Drawing.Point(0, 288);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(488, 224);
            this.groupBox4.TabIndex = 10;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Helpful Hints";
            // 
            // mlblMinIonCountHelp
            // 
            this.mlblMinIonCountHelp.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
            this.mlblMinIonCountHelp.Location = new System.Drawing.Point(8, 24);
            this.mlblMinIonCountHelp.Name = "mlblMinIonCountHelp";
            this.mlblMinIonCountHelp.Size = new System.Drawing.Size(472, 32);
            this.mlblMinIonCountHelp.TabIndex = 21;
            this.mlblMinIonCountHelp.Text = "- Enter the minimum ion needed in the MSn spectra to be considered";
            this.mlblMinIonCountHelp.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.mlblMinIonCountHelp.Visible = false;
            // 
            // mlblWindowSizeHelp
            // 
            this.mlblWindowSizeHelp.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
            this.mlblWindowSizeHelp.Location = new System.Drawing.Point(16, 16);
            this.mlblWindowSizeHelp.Name = "mlblWindowSizeHelp";
            this.mlblWindowSizeHelp.Size = new System.Drawing.Size(472, 40);
            this.mlblWindowSizeHelp.TabIndex = 20;
            this.mlblWindowSizeHelp.Text = "- This sets the m/z window range on either side of the precursor m/z for deisotop" +
                "ing";
            this.mlblWindowSizeHelp.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.mlblWindowSizeHelp.Visible = false;
            // 
            // mlblProtonMassHelp
            // 
            this.mlblProtonMassHelp.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
            this.mlblProtonMassHelp.Location = new System.Drawing.Point(16, 16);
            this.mlblProtonMassHelp.Name = "mlblProtonMassHelp";
            this.mlblProtonMassHelp.Size = new System.Drawing.Size(480, 40);
            this.mlblProtonMassHelp.TabIndex = 19;
            this.mlblProtonMassHelp.Text = "- Enter the mass of a proton";
            this.mlblProtonMassHelp.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.mlblProtonMassHelp.Visible = false;
            // 
            // mlblCCMassHelp
            // 
            this.mlblCCMassHelp.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
            this.mlblCCMassHelp.Location = new System.Drawing.Point(16, 16);
            this.mlblCCMassHelp.Name = "mlblCCMassHelp";
            this.mlblCCMassHelp.Size = new System.Drawing.Size(480, 40);
            this.mlblCCMassHelp.TabIndex = 18;
            this.mlblCCMassHelp.Text = "- Enter the mass of a charge carrier";
            this.mlblCCMassHelp.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.mlblCCMassHelp.Visible = false;
            // 
            // mlbConsiderChargeHelp
            // 
            this.mlbConsiderChargeHelp.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
            this.mlbConsiderChargeHelp.Location = new System.Drawing.Point(16, 16);
            this.mlbConsiderChargeHelp.Name = "mlbConsiderChargeHelp";
            this.mlbConsiderChargeHelp.Size = new System.Drawing.Size(480, 40);
            this.mlbConsiderChargeHelp.TabIndex = 17;
            this.mlbConsiderChargeHelp.Text = "- If checked, creates dtas only of a specified charge";
            this.mlbConsiderChargeHelp.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.mlbConsiderChargeHelp.Visible = false;
            // 
            // mlblMaxMassHelp
            // 
            this.mlblMaxMassHelp.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
            this.mlblMaxMassHelp.Location = new System.Drawing.Point(16, 16);
            this.mlblMaxMassHelp.Name = "mlblMaxMassHelp";
            this.mlblMaxMassHelp.Size = new System.Drawing.Size(488, 40);
            this.mlblMaxMassHelp.TabIndex = 16;
            this.mlblMaxMassHelp.Text = "- Enter the maximum of mass range to be considered for dta generation";
            this.mlblMaxMassHelp.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.mlblMaxMassHelp.Visible = false;
            // 
            // mlblMinMassHelp
            // 
            this.mlblMinMassHelp.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
            this.mlblMinMassHelp.Location = new System.Drawing.Point(16, 24);
            this.mlblMinMassHelp.Name = "mlblMinMassHelp";
            this.mlblMinMassHelp.Size = new System.Drawing.Size(480, 32);
            this.mlblMinMassHelp.TabIndex = 15;
            this.mlblMinMassHelp.Text = "- Enter the minimum of the mass range  to be considered for dta generation";
            this.mlblMinMassHelp.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.mlblMinMassHelp.Visible = false;
            // 
            // mlblMaxScanHelp
            // 
            this.mlblMaxScanHelp.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
            this.mlblMaxScanHelp.Location = new System.Drawing.Point(16, 24);
            this.mlblMaxScanHelp.Name = "mlblMaxScanHelp";
            this.mlblMaxScanHelp.Size = new System.Drawing.Size(472, 32);
            this.mlblMaxScanHelp.TabIndex = 14;
            this.mlblMaxScanHelp.Text = "- Enter the maximum scan to be considered for dta generation";
            this.mlblMaxScanHelp.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.mlblMaxScanHelp.Visible = false;
            // 
            // mlblOutputTypeCDTAHelp
            // 
            this.mlblOutputTypeCDTAHelp.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
            this.mlblOutputTypeCDTAHelp.Location = new System.Drawing.Point(56, 128);
            this.mlblOutputTypeCDTAHelp.Name = "mlblOutputTypeCDTAHelp";
            this.mlblOutputTypeCDTAHelp.Size = new System.Drawing.Size(400, 24);
            this.mlblOutputTypeCDTAHelp.TabIndex = 13;
            this.mlblOutputTypeCDTAHelp.Text = "-CDTA : Creates .dta files, log file (_log.txt), and a compostie dta file (_dta.t" +
                "xt)";
            this.mlblOutputTypeCDTAHelp.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.mlblOutputTypeCDTAHelp.Visible = false;
            // 
            // mlbOutputTypeMGFHelp
            // 
            this.mlbOutputTypeMGFHelp.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
            this.mlbOutputTypeMGFHelp.Location = new System.Drawing.Point(56, 104);
            this.mlbOutputTypeMGFHelp.Name = "mlbOutputTypeMGFHelp";
            this.mlbOutputTypeMGFHelp.Size = new System.Drawing.Size(400, 24);
            this.mlbOutputTypeMGFHelp.TabIndex = 12;
            this.mlbOutputTypeMGFHelp.Text = "-MGF : Creates .mgf file along with log file (_log.txt)";
            this.mlbOutputTypeMGFHelp.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.mlbOutputTypeMGFHelp.Visible = false;
            // 
            // mlblOutputTypeLogHelp
            // 
            this.mlblOutputTypeLogHelp.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
            this.mlblOutputTypeLogHelp.Location = new System.Drawing.Point(56, 80);
            this.mlblOutputTypeLogHelp.Name = "mlblOutputTypeLogHelp";
            this.mlblOutputTypeLogHelp.Size = new System.Drawing.Size(400, 24);
            this.mlblOutputTypeLogHelp.TabIndex = 11;
            this.mlblOutputTypeLogHelp.Text = "-LOG : create only log file (_log.txt)";
            this.mlblOutputTypeLogHelp.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.mlblOutputTypeLogHelp.Visible = false;
            // 
            // mlblOutputTypeDTAHelp
            // 
            this.mlblOutputTypeDTAHelp.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
            this.mlblOutputTypeDTAHelp.Location = new System.Drawing.Point(56, 56);
            this.mlblOutputTypeDTAHelp.Name = "mlblOutputTypeDTAHelp";
            this.mlblOutputTypeDTAHelp.Size = new System.Drawing.Size(384, 24);
            this.mlblOutputTypeDTAHelp.TabIndex = 10;
            this.mlblOutputTypeDTAHelp.Text = "- DTA : creates .dta files along with a log file (_log.txt) [default]";
            this.mlblOutputTypeDTAHelp.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.mlblOutputTypeDTAHelp.Visible = false;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.groupBox3);
            this.panel1.Controls.Add(this.groupBox1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 168);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(488, 120);
            this.panel1.TabIndex = 11;
            // 
            // ctlDTASettings
            // 
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.groupBox2);
            this.Name = "ctlDTASettings";
            this.Size = new System.Drawing.Size(488, 512);
            this.groupBox1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.groupBox4.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);
        }
        #endregion

        private void mtxtMinScanEnter(object sender, System.EventArgs e)
        {
            mlblMinMassHelp.Visible = false;
            mlblMaxMassHelp.Visible = false;
            mlblMaxScanHelp.Visible = false;
            mlblMinScanHelp.Visible = true;
            mlbConsiderChargeHelp.Visible = false;
            mlblCCMassHelp.Visible = false;
            mlblProtonMassHelp.Visible = false;

            mlblOutputTypeHelp.Visible = false;
            mlblOutputTypeCDTAHelp.Visible = false;
            mlblOutputTypeDTAHelp.Visible = false;
            mlblOutputTypeLogHelp.Visible = false;
            mlbOutputTypeMGFHelp.Visible = false;

            mlblWindowSizeHelp.Visible = false;
            mlblMinIonCountHelp.Visible = false;
        }

        private void mtxtMinMassEnter(object sender, System.EventArgs e)
        {
            mlblMinMassHelp.Visible = true;
            mlblMaxMassHelp.Visible = false;
            mlblMaxScanHelp.Visible = false;
            mlblMinScanHelp.Visible = false;
            mlbConsiderChargeHelp.Visible = false;
            mlblCCMassHelp.Visible = false;
            mlblProtonMassHelp.Visible = false;

            mlblOutputTypeHelp.Visible = false;
            mlblOutputTypeCDTAHelp.Visible = false;
            mlblOutputTypeDTAHelp.Visible = false;
            mlblOutputTypeLogHelp.Visible = false;
            mlbOutputTypeMGFHelp.Visible = false;

            mlblMinIonCountHelp.Visible = false;
            mlblWindowSizeHelp.Visible = false;
        }

        private void mtxtMaxScanEnter(object sender, System.EventArgs e)
        {
            mlblMinMassHelp.Visible = false;
            mlblMaxMassHelp.Visible = false;
            mlblMaxScanHelp.Visible = true;
            mlblMinScanHelp.Visible = false;
            mlbConsiderChargeHelp.Visible = false;
            mlblCCMassHelp.Visible = false;
            mlblProtonMassHelp.Visible = false;
            mlblOutputTypeHelp.Visible = false;
            mlblOutputTypeCDTAHelp.Visible = false;
            mlblOutputTypeDTAHelp.Visible = false;
            mlblOutputTypeLogHelp.Visible = false;
            mlbOutputTypeMGFHelp.Visible = false;
            mlblWindowSizeHelp.Visible = false;
            mlblMinIonCountHelp.Visible = false;
        }

        private void mtxtMaxMassEnter(object sender, System.EventArgs e)
        {
            mlblMinMassHelp.Visible = false;
            mlblMaxMassHelp.Visible = true;
            mlblMaxScanHelp.Visible = false;
            mlblMinScanHelp.Visible = false;
            mlbConsiderChargeHelp.Visible = false;
            mlblCCMassHelp.Visible = false;
            mlblProtonMassHelp.Visible = false;
            mlblOutputTypeHelp.Visible = false;
            mlblOutputTypeCDTAHelp.Visible = false;
            mlblOutputTypeDTAHelp.Visible = false;
            mlblOutputTypeLogHelp.Visible = false;
            mlbOutputTypeMGFHelp.Visible = false;
            mlblMinIonCountHelp.Visible = false;
            mlblWindowSizeHelp.Visible = false;
        }

        private void mtxtMinIonCountEnter(object sender, System.EventArgs e)
        {
            mlblMinMassHelp.Visible = false;
            mlblMaxMassHelp.Visible = false;
            mlblMaxScanHelp.Visible = false;
            mlblMinScanHelp.Visible = true;
            mlbConsiderChargeHelp.Visible = false;
            mlblCCMassHelp.Visible = false;
            mlblProtonMassHelp.Visible = false;
            mlblOutputTypeHelp.Visible = false;
            mlblOutputTypeCDTAHelp.Visible = false;
            mlblOutputTypeDTAHelp.Visible = false;
            mlblOutputTypeLogHelp.Visible = false;
            mlbOutputTypeMGFHelp.Visible = false;
            mlblMinIonCountHelp.Visible = true;
            mlblWindowSizeHelp.Visible = false;
        }

        private void mcmbOutputType_Click(object sender, System.EventArgs e)
        {
            mlblMinMassHelp.Visible = false;
            mlblMaxMassHelp.Visible = false;
            mlblMaxScanHelp.Visible = false;
            mlblMinScanHelp.Visible = false;
            mlbConsiderChargeHelp.Visible = false;
            mlblCCMassHelp.Visible = false;
            mlblProtonMassHelp.Visible = false;
            mlblOutputTypeHelp.Visible = true;
            mlblOutputTypeCDTAHelp.Visible = true;
            mlblOutputTypeDTAHelp.Visible = true;
            mlblOutputTypeLogHelp.Visible = true;
            mlbOutputTypeMGFHelp.Visible = true;
            mlblMinIonCountHelp.Visible = false;
            mlblWindowSizeHelp.Visible = false;
        }

        private void mchkConsiderCharge_Enter(object sender, System.EventArgs e)
        {
            mlblMinMassHelp.Visible = false;
            mlblMaxMassHelp.Visible = false;
            mlblMaxScanHelp.Visible = false;
            mlblMinScanHelp.Visible = false;
            mlbConsiderChargeHelp.Visible = true;
            mlblCCMassHelp.Visible = false;
            mlblProtonMassHelp.Visible = false;
            mlblOutputTypeHelp.Visible = false;
            mlblOutputTypeCDTAHelp.Visible = false;
            mlblOutputTypeDTAHelp.Visible = false;
            mlblOutputTypeLogHelp.Visible = false;
            mlbOutputTypeMGFHelp.Visible = false;
            mlblMinIonCountHelp.Visible = false;
            mlblWindowSizeHelp.Visible = false;
        }

        private void mchkConsiderCharge_CheckedChanged(object sender, System.EventArgs e)
        {
            mlabelChargeToConsider.Enabled = mchkConsiderCharge.Checked;
            mtxtConsiderCharge.Enabled = mchkConsiderCharge.Checked;
        }

        private void mtxtCCMass_Enter(object sender, System.EventArgs e)
        {
            mlblMinMassHelp.Visible = false;
            mlblMaxMassHelp.Visible = false;
            mlblMaxScanHelp.Visible = false;
            mlblMinScanHelp.Visible = false;
            mlbConsiderChargeHelp.Visible = false;
            mlblCCMassHelp.Visible = true;
            mlblProtonMassHelp.Visible = false;
            mlblOutputTypeHelp.Visible = false;
            mlblOutputTypeCDTAHelp.Visible = false;
            mlblOutputTypeDTAHelp.Visible = false;
            mlblOutputTypeLogHelp.Visible = false;
            mlbOutputTypeMGFHelp.Visible = false;
            mlblMinIonCountHelp.Visible = false;
            mlblWindowSizeHelp.Visible = false;
        }

        private void mtxtProtonMass_Enter(object sender, System.EventArgs e)
        {
            mlblMinMassHelp.Visible = false;
            mlblMaxMassHelp.Visible = false;
            mlblMaxScanHelp.Visible = false;
            mlblMinScanHelp.Visible = false;
            mlbConsiderChargeHelp.Visible = false;
            mlblCCMassHelp.Visible = false;
            mlblProtonMassHelp.Visible = true;
            mlblOutputTypeHelp.Visible = false;
            mlblOutputTypeCDTAHelp.Visible = false;
            mlblOutputTypeDTAHelp.Visible = false;
            mlblOutputTypeLogHelp.Visible = false;
            mlbOutputTypeMGFHelp.Visible = false;
            mlblMinIonCountHelp.Visible = false;
            mlblWindowSizeHelp.Visible = false;
        }

        private void mTxtWindowSize_Enter(object sender, System.EventArgs e)
        {
            mlblMinMassHelp.Visible = false;
            mlblMaxMassHelp.Visible = false;
            mlblMaxScanHelp.Visible = false;
            mlblMinScanHelp.Visible = false;
            mlbConsiderChargeHelp.Visible = false;
            mlblCCMassHelp.Visible = false;
            mlblProtonMassHelp.Visible = false;
            mlblOutputTypeHelp.Visible = false;
            mlblOutputTypeCDTAHelp.Visible = false;
            mlblOutputTypeDTAHelp.Visible = false;
            mlblOutputTypeLogHelp.Visible = false;
            mlbOutputTypeMGFHelp.Visible = false;
            mlblMinIonCountHelp.Visible = false;
            mlblWindowSizeHelp.Visible = true;
        }

        public int MaximumScanToConsider
        {
            set
            {
                mtxtMaxScan.Text = Convert.ToString(value);
            }
            get
            {
                return Convert.ToInt32(mtxtMaxScan.Text);
            }
        }

        public int MinimumScanToConsider
        {
            set
            {
                mtxtMinScan.Text = Convert.ToString(value);
            }
            get
            {
                return Convert.ToInt32(mtxtMinScan.Text);
            }
        }

        public double MinimumMassToConsider
        {
            set
            {
                mtxtMinMass.Text = Convert.ToString(value);
            }
            get
            {
                return Convert.ToDouble(mtxtMinMass.Text);
            }
        }

        public double MaximumMassToConsider
        {
            set
            {
                mtxtMaxMass.Text = Convert.ToString(value);
            }
            get
            {
                return Convert.ToDouble(mtxtMaxMass.Text);
            }
        }

        public int MinIonCount
        {
            set
            {
                mtxtMinIonCount.Text = Convert.ToString(value);
            }
            get
            {
                return Convert.ToInt16(mtxtMinIonCount.Text);
            }
        }

        public DeconToolsV2.DTAGeneration.OUTPUT_TYPE OutputType
        {
            get
            {
                if (mcmbOutputType.SelectedIndex == 0)
                {
                    return DeconToolsV2.DTAGeneration.OUTPUT_TYPE.DTA;
                }
                else if (mcmbOutputType.SelectedIndex == 1)
                {
                    return DeconToolsV2.DTAGeneration.OUTPUT_TYPE.LOG;
                }
                else if (mcmbOutputType.SelectedIndex == 2)
                {
                    return DeconToolsV2.DTAGeneration.OUTPUT_TYPE.MGF;
                }
                else if (mcmbOutputType.SelectedIndex == 3)
                {
                    return DeconToolsV2.DTAGeneration.OUTPUT_TYPE.CDTA;
                }

                return DeconToolsV2.DTAGeneration.OUTPUT_TYPE.DTA;
            }
            set
            {
                switch (value)
                {
                    case DeconToolsV2.DTAGeneration.OUTPUT_TYPE.DTA:
                        mcmbOutputType.SelectedIndex = 0;
                        break;
                    case DeconToolsV2.DTAGeneration.OUTPUT_TYPE.LOG:
                        mcmbOutputType.SelectedIndex = 1;
                        break;
                    case DeconToolsV2.DTAGeneration.OUTPUT_TYPE.MGF:
                        mcmbOutputType.SelectedIndex = 2;
                        break;
                    case DeconToolsV2.DTAGeneration.OUTPUT_TYPE.CDTA:
                        mcmbOutputType.SelectedIndex = 3;
                        break;
                    default:
                        mcmbOutputType.SelectedIndex = 0;
                        break;
                }
            }
        }

        public bool ConsiderCharge
        {
            set
            {
                mchkConsiderCharge.Checked = value;
            }

            get
            {
                return mchkConsiderCharge.Checked;
            }
        }

        public int ChargeToConsider
        {
            set
            {
                mtxtConsiderCharge.Text = Convert.ToString(value);
            }
            get
            {
                return Convert.ToInt16(mtxtConsiderCharge.Text);
            }
        }

        public double CCMass
        {
            set
            {
                mtxtCCMass.Text = Convert.ToString(value);
            }
            get
            {
                return Convert.ToDouble(mtxtCCMass.Text);
            }
        }

        public double ProtonMass
        {
            set
            {
                mtxtProtonMass.Text = Convert.ToString(value);
            }

            get
            {
                return Convert.ToDouble(mtxtProtonMass.Text);
            }
        }

        public int WindowSize
        {
            set
            {
                mTxtWindowSize.Text = Convert.ToString(value);
            }
            get
            {
                return Convert.ToInt16(mTxtWindowSize.Text);
            }
        }
    }
}
