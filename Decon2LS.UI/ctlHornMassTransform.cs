// Written by Navdeep Jaitly and Anoop Mayampurath for the Department of Energy (PNNL, Richland, WA)
// Copyright 2006, Battelle Memorial Institute
// E-mail: navdeep.jaitly@pnl.gov
// Website: http://ncrr.pnl.gov/software
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
		/// Summary description for ctlHornMassTransform.
		/// </summary>
		public class ctlHornMassTransform : System.Windows.Forms.UserControl
		{
			private System.Windows.Forms.Label label1;
			private System.Windows.Forms.Label label2;
			private System.Windows.Forms.Label label_bkgr;
			private System.Windows.Forms.GroupBox groupBox1;
			private System.Windows.Forms.Label label4;
			private System.Windows.Forms.PictureBox pictureBox_BkgRatio;
			private System.Windows.Forms.PictureBox pictureBox_NumShoulders;
			private System.Windows.Forms.GroupBox groupBox2;
			private System.Windows.Forms.GroupBox groupBox3;
			private System.Windows.Forms.GroupBox groupBox4;
			private System.Windows.Forms.Label label_bkg_tip;
			private System.Windows.Forms.Label label_ccmass_tip;
			private System.Windows.Forms.Label label_mass_tip;
			private System.Windows.Forms.Label label_oxy_tip;
			private System.Windows.Forms.Label label_thrash_tip;
			private System.Windows.Forms.Label label_fit_tip;
			private System.Windows.Forms.GroupBox groupBox5;
			private System.Windows.Forms.Panel panel1;
			private System.Windows.Forms.Label label_shoulders_tip;
			private System.Windows.Forms.CheckBox mCheckO16O18;
			private System.Windows.Forms.TextBox mTextChargeMass;
			private System.Windows.Forms.TextBox mTextMaxMass;
			private System.Windows.Forms.CheckBox mCheckThrash;
			private System.Windows.Forms.CheckBox mCheckCompleteFit;
			private System.Windows.Forms.TextBox mTextPeptideBgRatio;
			private System.Windows.Forms.TextBox mTextNumShoulders;
			private System.Windows.Forms.TextBox mTextMaxCharge;
			private System.Windows.Forms.Label label5;
			private System.Windows.Forms.CheckBox mCheckCacheMercury;
			private System.Windows.Forms.ComboBox mcmbIsotopeFitType;
			private System.Windows.Forms.CheckBox mchkBoxUseAbsolutePeptideIntensity;
			private System.Windows.Forms.TextBox mtxtAbsolutePeptideIntensityThreshold;
			private System.Windows.Forms.CheckBox mCheckFitAllAgainstCharge1;
			private System.Windows.Forms.TextBox mTextNumScansToSum;
			private System.Windows.Forms.CheckBox mchkSumSpectra;
			private System.Windows.Forms.CheckBox mchkSumAcrossScanRange;
			private System.Windows.Forms.CheckBox mchkUseActualMonoMZ;
			private System.Windows.Forms.ToolTip toolTip1;
			private System.Windows.Forms.Label label3;
			private System.Windows.Forms.TextBox txtLeftStringencyFactor;
			private System.Windows.Forms.Label label6;
			private System.Windows.Forms.TextBox txtRightStringencyFactor;
			private System.ComponentModel.IContainer components;

			public ctlHornMassTransform()
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
				this.components = new System.ComponentModel.Container();
				System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(ctlHornMassTransform));
				this.label1 = new System.Windows.Forms.Label();
				this.label2 = new System.Windows.Forms.Label();
				this.mCheckO16O18 = new System.Windows.Forms.CheckBox();
				this.mTextChargeMass = new System.Windows.Forms.TextBox();
				this.mTextMaxMass = new System.Windows.Forms.TextBox();
				this.mCheckThrash = new System.Windows.Forms.CheckBox();
				this.mCheckCompleteFit = new System.Windows.Forms.CheckBox();
				this.label_bkgr = new System.Windows.Forms.Label();
				this.mTextPeptideBgRatio = new System.Windows.Forms.TextBox();
				this.mTextNumShoulders = new System.Windows.Forms.TextBox();
				this.groupBox1 = new System.Windows.Forms.GroupBox();
				this.mchkSumAcrossScanRange = new System.Windows.Forms.CheckBox();
				this.mchkSumSpectra = new System.Windows.Forms.CheckBox();
				this.mTextNumScansToSum = new System.Windows.Forms.TextBox();
				this.mCheckFitAllAgainstCharge1 = new System.Windows.Forms.CheckBox();
				this.mCheckCacheMercury = new System.Windows.Forms.CheckBox();
				this.mchkUseActualMonoMZ = new System.Windows.Forms.CheckBox();
				this.mcmbIsotopeFitType = new System.Windows.Forms.ComboBox();
				this.label4 = new System.Windows.Forms.Label();
				this.pictureBox_BkgRatio = new System.Windows.Forms.PictureBox();
				this.pictureBox_NumShoulders = new System.Windows.Forms.PictureBox();
				this.groupBox2 = new System.Windows.Forms.GroupBox();
				this.panel1 = new System.Windows.Forms.Panel();
				this.groupBox3 = new System.Windows.Forms.GroupBox();
				this.mTextMaxCharge = new System.Windows.Forms.TextBox();
				this.label5 = new System.Windows.Forms.Label();
				this.groupBox5 = new System.Windows.Forms.GroupBox();
				this.mtxtAbsolutePeptideIntensityThreshold = new System.Windows.Forms.TextBox();
				this.mchkBoxUseAbsolutePeptideIntensity = new System.Windows.Forms.CheckBox();
				this.groupBox4 = new System.Windows.Forms.GroupBox();
				this.label_shoulders_tip = new System.Windows.Forms.Label();
				this.label_fit_tip = new System.Windows.Forms.Label();
				this.label_thrash_tip = new System.Windows.Forms.Label();
				this.label_oxy_tip = new System.Windows.Forms.Label();
				this.label_mass_tip = new System.Windows.Forms.Label();
				this.label_ccmass_tip = new System.Windows.Forms.Label();
				this.label_bkg_tip = new System.Windows.Forms.Label();
				this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
				this.txtLeftStringencyFactor = new System.Windows.Forms.TextBox();
				this.label3 = new System.Windows.Forms.Label();
				this.label6 = new System.Windows.Forms.Label();
				this.txtRightStringencyFactor = new System.Windows.Forms.TextBox();
				this.groupBox1.SuspendLayout();
				this.groupBox2.SuspendLayout();
				this.panel1.SuspendLayout();
				this.groupBox3.SuspendLayout();
				this.groupBox5.SuspendLayout();
				this.groupBox4.SuspendLayout();
				this.SuspendLayout();
				// 
				// label1
				// 
				this.label1.Location = new System.Drawing.Point(8, 16);
				this.label1.Name = "label1";
				this.label1.Size = new System.Drawing.Size(96, 16);
				this.label1.TabIndex = 0;
				this.label1.Text = "Charge Mass";
				// 
				// label2
				// 
				this.label2.Location = new System.Drawing.Point(8, 40);
				this.label2.Name = "label2";
				this.label2.Size = new System.Drawing.Size(104, 16);
				this.label2.TabIndex = 1;
				this.label2.Text = "Maximum Mass";
				// 
				// mCheckO16O18
				// 
				this.mCheckO16O18.Checked = true;
				this.mCheckO16O18.CheckState = System.Windows.Forms.CheckState.Checked;
				this.mCheckO16O18.Location = new System.Drawing.Point(16, 14);
				this.mCheckO16O18.Name = "mCheckO16O18";
				this.mCheckO16O18.Size = new System.Drawing.Size(160, 24);
				this.mCheckO16O18.TabIndex = 3;
				this.mCheckO16O18.Text = "016/ 018 labelled mixture";
				// 
				// mTextChargeMass
				// 
				this.mTextChargeMass.Location = new System.Drawing.Point(144, 16);
				this.mTextChargeMass.Name = "mTextChargeMass";
				this.mTextChargeMass.Size = new System.Drawing.Size(56, 20);
				this.mTextChargeMass.TabIndex = 4;
				this.mTextChargeMass.Text = "1.00727638";
				// 
				// mTextMaxMass
				// 
				this.mTextMaxMass.Location = new System.Drawing.Point(144, 40);
				this.mTextMaxMass.Name = "mTextMaxMass";
				this.mTextMaxMass.Size = new System.Drawing.Size(56, 20);
				this.mTextMaxMass.TabIndex = 5;
				this.mTextMaxMass.Text = "10000";
				// 
				// mCheckThrash
				// 
				this.mCheckThrash.Checked = true;
				this.mCheckThrash.CheckState = System.Windows.Forms.CheckState.Checked;
				this.mCheckThrash.Location = new System.Drawing.Point(16, 37);
				this.mCheckThrash.Name = "mCheckThrash";
				this.mCheckThrash.Size = new System.Drawing.Size(72, 24);
				this.mCheckThrash.TabIndex = 7;
				this.mCheckThrash.Text = "THRASH";
				this.mCheckThrash.CheckedChanged += new System.EventHandler(this.checkBox_Thrash_CheckedChanged);
				// 
				// mCheckCompleteFit
				// 
				this.mCheckCompleteFit.Location = new System.Drawing.Point(112, 40);
				this.mCheckCompleteFit.Name = "mCheckCompleteFit";
				this.mCheckCompleteFit.Size = new System.Drawing.Size(96, 24);
				this.mCheckCompleteFit.TabIndex = 8;
				this.mCheckCompleteFit.Text = "Complete Fit";
				// 
				// label_bkgr
				// 
				this.label_bkgr.Dock = System.Windows.Forms.DockStyle.Left;
				this.label_bkgr.Location = new System.Drawing.Point(3, 16);
				this.label_bkgr.Name = "label_bkgr";
				this.label_bkgr.Size = new System.Drawing.Size(181, 29);
				this.label_bkgr.TabIndex = 9;
				this.label_bkgr.Text = "Peptide Background Ratio (r)";
				// 
				// mTextPeptideBgRatio
				// 
				this.mTextPeptideBgRatio.Dock = System.Windows.Forms.DockStyle.Left;
				this.mTextPeptideBgRatio.Location = new System.Drawing.Point(184, 16);
				this.mTextPeptideBgRatio.Name = "mTextPeptideBgRatio";
				this.mTextPeptideBgRatio.Size = new System.Drawing.Size(32, 20);
				this.mTextPeptideBgRatio.TabIndex = 10;
				this.mTextPeptideBgRatio.Text = "5";
				this.mTextPeptideBgRatio.Leave += new System.EventHandler(this.textBox_BkgRatio_Leave);
				this.mTextPeptideBgRatio.Enter += new System.EventHandler(this.textBox_BkgRatio_Enter);
				// 
				// mTextNumShoulders
				// 
				this.mTextNumShoulders.Location = new System.Drawing.Point(143, 89);
				this.mTextNumShoulders.Name = "mTextNumShoulders";
				this.mTextNumShoulders.Size = new System.Drawing.Size(58, 20);
				this.mTextNumShoulders.TabIndex = 11;
				this.mTextNumShoulders.Text = "1";
				this.mTextNumShoulders.Leave += new System.EventHandler(this.textBox_NumShoulders_Leave);
				this.mTextNumShoulders.Enter += new System.EventHandler(this.textBox_NumShoulders_Enter);
				// 
				// groupBox1
				// 
				this.groupBox1.Controls.Add(this.txtLeftStringencyFactor);
				this.groupBox1.Controls.Add(this.mchkSumAcrossScanRange);
				this.groupBox1.Controls.Add(this.mchkSumSpectra);
				this.groupBox1.Controls.Add(this.mTextNumScansToSum);
				this.groupBox1.Controls.Add(this.mCheckFitAllAgainstCharge1);
				this.groupBox1.Controls.Add(this.mCheckCacheMercury);
				this.groupBox1.Controls.Add(this.mCheckO16O18);
				this.groupBox1.Controls.Add(this.mCheckThrash);
				this.groupBox1.Controls.Add(this.mCheckCompleteFit);
				this.groupBox1.Controls.Add(this.mchkUseActualMonoMZ);
				this.groupBox1.Controls.Add(this.label3);
				this.groupBox1.Controls.Add(this.label6);
				this.groupBox1.Controls.Add(this.txtRightStringencyFactor);
				this.groupBox1.Dock = System.Windows.Forms.DockStyle.Left;
				this.groupBox1.Location = new System.Drawing.Point(216, 5);
				this.groupBox1.Name = "groupBox1";
				this.groupBox1.Size = new System.Drawing.Size(240, 267);
				this.groupBox1.TabIndex = 14;
				this.groupBox1.TabStop = false;
				// 
				// mchkSumAcrossScanRange
				// 
				this.mchkSumAcrossScanRange.Location = new System.Drawing.Point(16, 136);
				this.mchkSumAcrossScanRange.Name = "mchkSumAcrossScanRange";
				this.mchkSumAcrossScanRange.Size = new System.Drawing.Size(152, 24);
				this.mchkSumAcrossScanRange.TabIndex = 14;
				this.mchkSumAcrossScanRange.Text = "Sum Across Scan Range";
				this.mchkSumAcrossScanRange.CheckStateChanged += new System.EventHandler(this.mchkSumAcrossScanRange_CheckStateChanged);
				// 
				// mchkSumSpectra
				// 
				this.mchkSumSpectra.Location = new System.Drawing.Point(16, 112);
				this.mchkSumSpectra.Name = "mchkSumSpectra";
				this.mchkSumSpectra.Size = new System.Drawing.Size(160, 24);
				this.mchkSumSpectra.TabIndex = 13;
				this.mchkSumSpectra.Text = "Sum All Spectra";
				this.mchkSumSpectra.CheckedChanged += new System.EventHandler(this.mchkSumSpectra_CheckedChanged);
				// 
				// mTextNumScansToSum
				// 
				this.mTextNumScansToSum.Enabled = false;
				this.mTextNumScansToSum.Location = new System.Drawing.Point(192, 136);
				this.mTextNumScansToSum.Name = "mTextNumScansToSum";
				this.mTextNumScansToSum.Size = new System.Drawing.Size(32, 20);
				this.mTextNumScansToSum.TabIndex = 12;
				this.mTextNumScansToSum.Text = "5";
				// 
				// mCheckFitAllAgainstCharge1
				// 
				this.mCheckFitAllAgainstCharge1.Checked = true;
				this.mCheckFitAllAgainstCharge1.CheckState = System.Windows.Forms.CheckState.Checked;
				this.mCheckFitAllAgainstCharge1.Location = new System.Drawing.Point(16, 80);
				this.mCheckFitAllAgainstCharge1.Name = "mCheckFitAllAgainstCharge1";
				this.mCheckFitAllAgainstCharge1.Size = new System.Drawing.Size(216, 32);
				this.mCheckFitAllAgainstCharge1.TabIndex = 10;
				this.mCheckFitAllAgainstCharge1.Text = "Test All Peaks Against Charge 1";
				// 
				// mCheckCacheMercury
				// 
				this.mCheckCacheMercury.Checked = true;
				this.mCheckCacheMercury.CheckState = System.Windows.Forms.CheckState.Checked;
				this.mCheckCacheMercury.Location = new System.Drawing.Point(16, 60);
				this.mCheckCacheMercury.Name = "mCheckCacheMercury";
				this.mCheckCacheMercury.Size = new System.Drawing.Size(168, 24);
				this.mCheckCacheMercury.TabIndex = 9;
				this.mCheckCacheMercury.Text = "Cache Isotope Distributions";
				// 
				// mchkUseActualMonoMZ
				// 
				this.mchkUseActualMonoMZ.Location = new System.Drawing.Point(16, 160);
				this.mchkUseActualMonoMZ.Name = "mchkUseActualMonoMZ";
				this.mchkUseActualMonoMZ.Size = new System.Drawing.Size(200, 32);
				this.mchkUseActualMonoMZ.TabIndex = 14;
				this.mchkUseActualMonoMZ.Text = "Prefer actual vs. calculated for monoisotopic peak m/z";
				this.toolTip1.SetToolTip(this.mchkUseActualMonoMZ, @"In cases when the most abundant peak is NOT the monoisotopic peak, the m/z for the monoisotopic peak is calculated based on alignment of the most abundant peak to the theoretical (mercury) isotopic profile. Checking this box will override this and force the use of the experimentally derived monoisotopic m/z value");
				// 
				// mcmbIsotopeFitType
				// 
				this.mcmbIsotopeFitType.Items.AddRange(new object[] {
																		"PEAK",
																		"AREA",
																		"CHISQ"});
				this.mcmbIsotopeFitType.Location = new System.Drawing.Point(16, 128);
				this.mcmbIsotopeFitType.Name = "mcmbIsotopeFitType";
				this.mcmbIsotopeFitType.Size = new System.Drawing.Size(176, 21);
				this.mcmbIsotopeFitType.TabIndex = 11;
				this.mcmbIsotopeFitType.Text = "Isotope Fit Type";
				// 
				// label4
				// 
				this.label4.Location = new System.Drawing.Point(8, 96);
				this.label4.Name = "label4";
				this.label4.Size = new System.Drawing.Size(128, 16);
				this.label4.TabIndex = 15;
				this.label4.Text = "Allowable shoulders (n)";
				// 
				// pictureBox_BkgRatio
				// 
				this.pictureBox_BkgRatio.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox_BkgRatio.Image")));
				this.pictureBox_BkgRatio.Location = new System.Drawing.Point(8, 16);
				this.pictureBox_BkgRatio.Name = "pictureBox_BkgRatio";
				this.pictureBox_BkgRatio.Size = new System.Drawing.Size(440, 192);
				this.pictureBox_BkgRatio.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
				this.pictureBox_BkgRatio.TabIndex = 18;
				this.pictureBox_BkgRatio.TabStop = false;
				this.pictureBox_BkgRatio.Visible = false;
				// 
				// pictureBox_NumShoulders
				// 
				this.pictureBox_NumShoulders.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox_NumShoulders.Image")));
				this.pictureBox_NumShoulders.Location = new System.Drawing.Point(88, 24);
				this.pictureBox_NumShoulders.Name = "pictureBox_NumShoulders";
				this.pictureBox_NumShoulders.Size = new System.Drawing.Size(272, 184);
				this.pictureBox_NumShoulders.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
				this.pictureBox_NumShoulders.TabIndex = 19;
				this.pictureBox_NumShoulders.TabStop = false;
				this.pictureBox_NumShoulders.Visible = false;
				this.pictureBox_NumShoulders.Click += new System.EventHandler(this.pictureBox_NumShoulders_Click);
				// 
				// groupBox2
				// 
				this.groupBox2.Controls.Add(this.panel1);
				this.groupBox2.Controls.Add(this.groupBox5);
				this.groupBox2.Controls.Add(this.groupBox4);
				this.groupBox2.Dock = System.Windows.Forms.DockStyle.Fill;
				this.groupBox2.Location = new System.Drawing.Point(0, 0);
				this.groupBox2.Name = "groupBox2";
				this.groupBox2.Size = new System.Drawing.Size(480, 608);
				this.groupBox2.TabIndex = 22;
				this.groupBox2.TabStop = false;
				this.groupBox2.Enter += new System.EventHandler(this.groupBox2_Enter);
				// 
				// panel1
				// 
				this.panel1.Controls.Add(this.groupBox1);
				this.panel1.Controls.Add(this.groupBox3);
				this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
				this.panel1.DockPadding.All = 5;
				this.panel1.Location = new System.Drawing.Point(3, 16);
				this.panel1.Name = "panel1";
				this.panel1.Size = new System.Drawing.Size(474, 277);
				this.panel1.TabIndex = 25;
				// 
				// groupBox3
				// 
				this.groupBox3.Controls.Add(this.mTextMaxCharge);
				this.groupBox3.Controls.Add(this.label5);
				this.groupBox3.Controls.Add(this.mTextChargeMass);
				this.groupBox3.Controls.Add(this.mTextMaxMass);
				this.groupBox3.Controls.Add(this.label1);
				this.groupBox3.Controls.Add(this.label2);
				this.groupBox3.Controls.Add(this.label4);
				this.groupBox3.Controls.Add(this.mTextNumShoulders);
				this.groupBox3.Controls.Add(this.mcmbIsotopeFitType);
				this.groupBox3.Dock = System.Windows.Forms.DockStyle.Left;
				this.groupBox3.Location = new System.Drawing.Point(5, 5);
				this.groupBox3.Name = "groupBox3";
				this.groupBox3.Size = new System.Drawing.Size(211, 267);
				this.groupBox3.TabIndex = 22;
				this.groupBox3.TabStop = false;
				// 
				// mTextMaxCharge
				// 
				this.mTextMaxCharge.Location = new System.Drawing.Point(144, 64);
				this.mTextMaxCharge.Name = "mTextMaxCharge";
				this.mTextMaxCharge.Size = new System.Drawing.Size(56, 20);
				this.mTextMaxCharge.TabIndex = 23;
				this.mTextMaxCharge.Text = "10";
				// 
				// label5
				// 
				this.label5.Location = new System.Drawing.Point(8, 64);
				this.label5.Name = "label5";
				this.label5.Size = new System.Drawing.Size(104, 16);
				this.label5.TabIndex = 22;
				this.label5.Text = "Maximum Charge";
				// 
				// groupBox5
				// 
				this.groupBox5.Controls.Add(this.mtxtAbsolutePeptideIntensityThreshold);
				this.groupBox5.Controls.Add(this.mchkBoxUseAbsolutePeptideIntensity);
				this.groupBox5.Controls.Add(this.mTextPeptideBgRatio);
				this.groupBox5.Controls.Add(this.label_bkgr);
				this.groupBox5.Dock = System.Windows.Forms.DockStyle.Bottom;
				this.groupBox5.Location = new System.Drawing.Point(3, 293);
				this.groupBox5.Name = "groupBox5";
				this.groupBox5.Size = new System.Drawing.Size(474, 48);
				this.groupBox5.TabIndex = 24;
				this.groupBox5.TabStop = false;
				// 
				// mtxtAbsolutePeptideIntensityThreshold
				// 
				this.mtxtAbsolutePeptideIntensityThreshold.Dock = System.Windows.Forms.DockStyle.Left;
				this.mtxtAbsolutePeptideIntensityThreshold.Enabled = false;
				this.mtxtAbsolutePeptideIntensityThreshold.Location = new System.Drawing.Point(352, 16);
				this.mtxtAbsolutePeptideIntensityThreshold.Name = "mtxtAbsolutePeptideIntensityThreshold";
				this.mtxtAbsolutePeptideIntensityThreshold.Size = new System.Drawing.Size(32, 20);
				this.mtxtAbsolutePeptideIntensityThreshold.TabIndex = 12;
				this.mtxtAbsolutePeptideIntensityThreshold.Text = "10";
				// 
				// mchkBoxUseAbsolutePeptideIntensity
				// 
				this.mchkBoxUseAbsolutePeptideIntensity.Dock = System.Windows.Forms.DockStyle.Left;
				this.mchkBoxUseAbsolutePeptideIntensity.Location = new System.Drawing.Point(216, 16);
				this.mchkBoxUseAbsolutePeptideIntensity.Name = "mchkBoxUseAbsolutePeptideIntensity";
				this.mchkBoxUseAbsolutePeptideIntensity.Size = new System.Drawing.Size(136, 29);
				this.mchkBoxUseAbsolutePeptideIntensity.TabIndex = 11;
				this.mchkBoxUseAbsolutePeptideIntensity.Text = "Use Absolute Intensity";
				this.mchkBoxUseAbsolutePeptideIntensity.CheckedChanged += new System.EventHandler(this.mchkBoxUseAbsolutePeptideIntensity_CheckedChanged);
				// 
				// groupBox4
				// 
				this.groupBox4.Controls.Add(this.label_shoulders_tip);
				this.groupBox4.Controls.Add(this.label_fit_tip);
				this.groupBox4.Controls.Add(this.label_thrash_tip);
				this.groupBox4.Controls.Add(this.label_oxy_tip);
				this.groupBox4.Controls.Add(this.label_mass_tip);
				this.groupBox4.Controls.Add(this.label_ccmass_tip);
				this.groupBox4.Controls.Add(this.label_bkg_tip);
				this.groupBox4.Controls.Add(this.pictureBox_NumShoulders);
				this.groupBox4.Controls.Add(this.pictureBox_BkgRatio);
				this.groupBox4.Dock = System.Windows.Forms.DockStyle.Bottom;
				this.groupBox4.Location = new System.Drawing.Point(3, 341);
				this.groupBox4.Name = "groupBox4";
				this.groupBox4.Size = new System.Drawing.Size(474, 264);
				this.groupBox4.TabIndex = 23;
				this.groupBox4.TabStop = false;
				this.groupBox4.Text = "Helpful Tips";
				// 
				// label_shoulders_tip
				// 
				this.label_shoulders_tip.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.5F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
				this.label_shoulders_tip.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
				this.label_shoulders_tip.Location = new System.Drawing.Point(8, 224);
				this.label_shoulders_tip.Name = "label_shoulders_tip";
				this.label_shoulders_tip.Size = new System.Drawing.Size(432, 32);
				this.label_shoulders_tip.TabIndex = 26;
				this.label_shoulders_tip.Text = "Sets the number of allowable shoulders as the number of non-decreasing peaks prec" +
					"edinga  minima for it to bo considered a shoulder";
				this.label_shoulders_tip.Visible = false;
				// 
				// label_fit_tip
				// 
				this.label_fit_tip.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.5F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
				this.label_fit_tip.Location = new System.Drawing.Point(8, 216);
				this.label_fit_tip.Name = "label_fit_tip";
				this.label_fit_tip.Size = new System.Drawing.Size(448, 32);
				this.label_fit_tip.TabIndex = 25;
				this.label_fit_tip.Text = "Complete fit: same as THRASH except the best fit from a series of fits is returne" +
					"d";
				this.label_fit_tip.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
				// 
				// label_thrash_tip
				// 
				this.label_thrash_tip.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.5F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
				this.label_thrash_tip.Location = new System.Drawing.Point(8, 168);
				this.label_thrash_tip.Name = "label_thrash_tip";
				this.label_thrash_tip.Size = new System.Drawing.Size(448, 32);
				this.label_thrash_tip.TabIndex = 24;
				this.label_thrash_tip.Text = "THRASH: If selected, scores each isotopic profile in steps of +/- 1 Da for fit to" +
					" data, exits and returns if new_score>current_score";
				this.label_thrash_tip.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
				// 
				// label_oxy_tip
				// 
				this.label_oxy_tip.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.5F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
				this.label_oxy_tip.Location = new System.Drawing.Point(8, 120);
				this.label_oxy_tip.Name = "label_oxy_tip";
				this.label_oxy_tip.Size = new System.Drawing.Size(448, 32);
				this.label_oxy_tip.TabIndex = 23;
				this.label_oxy_tip.Text = "016/ 018 labelled mixture : Select if the growth mixture was tagged 016/018";
				this.label_oxy_tip.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
				// 
				// label_mass_tip
				// 
				this.label_mass_tip.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.5F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
				this.label_mass_tip.Location = new System.Drawing.Point(8, 72);
				this.label_mass_tip.Name = "label_mass_tip";
				this.label_mass_tip.Size = new System.Drawing.Size(448, 32);
				this.label_mass_tip.TabIndex = 22;
				this.label_mass_tip.Text = "Min/ Max Mass : Mass Range to be considered";
				this.label_mass_tip.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
				// 
				// label_ccmass_tip
				// 
				this.label_ccmass_tip.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.5F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
				this.label_ccmass_tip.ForeColor = System.Drawing.SystemColors.ControlText;
				this.label_ccmass_tip.Location = new System.Drawing.Point(8, 32);
				this.label_ccmass_tip.Name = "label_ccmass_tip";
				this.label_ccmass_tip.Size = new System.Drawing.Size(448, 24);
				this.label_ccmass_tip.TabIndex = 21;
				this.label_ccmass_tip.Text = " Charge Mass: Mass of one unit of charge (in Da)";
				this.label_ccmass_tip.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
				// 
				// label_bkg_tip
				// 
				this.label_bkg_tip.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.5F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
				this.label_bkg_tip.Location = new System.Drawing.Point(8, 224);
				this.label_bkg_tip.Name = "label_bkg_tip";
				this.label_bkg_tip.Size = new System.Drawing.Size(448, 24);
				this.label_bkg_tip.TabIndex = 20;
				this.label_bkg_tip.Text = "Sets the maximum intensity level for a peak to be considered as background";
				this.label_bkg_tip.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
				this.label_bkg_tip.Visible = false;
				// 
				// txtLeftStringencyFactor
				// 
				this.txtLeftStringencyFactor.Location = new System.Drawing.Point(8, 192);
				this.txtLeftStringencyFactor.Name = "txtLeftStringencyFactor";
				this.txtLeftStringencyFactor.Size = new System.Drawing.Size(32, 20);
				this.txtLeftStringencyFactor.TabIndex = 15;
				this.txtLeftStringencyFactor.Text = "1";
				// 
				// label3
				// 
				this.label3.Location = new System.Drawing.Point(48, 197);
				this.label3.Name = "label3";
				this.label3.Size = new System.Drawing.Size(186, 16);
				this.label3.TabIndex = 15;
				this.label3.Text = "Left fit stringency factor (default = 1)";
				// 
				// label6
				// 
				this.label6.Location = new System.Drawing.Point(48, 220);
				this.label6.Name = "label6";
				this.label6.Size = new System.Drawing.Size(200, 16);
				this.label6.TabIndex = 15;
				this.label6.Text = "Right fit stringency factor (default = 1)";
				// 
				// txtRightStringencyFactor
				// 
				this.txtRightStringencyFactor.Location = new System.Drawing.Point(8, 216);
				this.txtRightStringencyFactor.Name = "txtRightStringencyFactor";
				this.txtRightStringencyFactor.Size = new System.Drawing.Size(32, 20);
				this.txtRightStringencyFactor.TabIndex = 15;
				this.txtRightStringencyFactor.Text = "1";
				// 
				// ctlHornMassTransform
				// 
				this.Controls.Add(this.groupBox2);
				this.Name = "ctlHornMassTransform";
				this.Size = new System.Drawing.Size(480, 608);
				this.groupBox1.ResumeLayout(false);
				this.groupBox2.ResumeLayout(false);
				this.panel1.ResumeLayout(false);
				this.groupBox3.ResumeLayout(false);
				this.groupBox5.ResumeLayout(false);
				this.groupBox4.ResumeLayout(false);
				this.ResumeLayout(false);

			}
			#endregion

			private void textBox_BkgRatio_Enter(object sender, System.EventArgs e)
			{
				pictureBox_BkgRatio.Visible = true;
				label_bkg_tip.Visible = true;
				label_ccmass_tip.Visible = false;
				label_fit_tip.Visible = false;
				label_mass_tip.Visible = false;
				label_oxy_tip.Visible = false;
				label_thrash_tip.Visible = false;				
				pictureBox_NumShoulders.Visible = false;	
				label_shoulders_tip.Visible = false;
			}

			private void textBox_BkgRatio_Leave(object sender, System.EventArgs e)
			{
				pictureBox_BkgRatio.Visible = false;
				pictureBox_NumShoulders.Visible = false;						
				label_bkg_tip.Visible = false;
				label_shoulders_tip.Visible = false;
				label_ccmass_tip.Visible = true;
				label_fit_tip.Visible = true;
				label_mass_tip.Visible = true;
				label_oxy_tip.Visible = true;
				label_thrash_tip.Visible = true;		
			}

			private void textBox_NumShoulders_Enter(object sender, System.EventArgs e)
			{
				pictureBox_NumShoulders.Visible = true;
				label_shoulders_tip.Visible = true;
				pictureBox_BkgRatio.Visible = false;
				label_bkg_tip.Visible = false;
				label_ccmass_tip.Visible = false;
				label_fit_tip.Visible = false;
				label_mass_tip.Visible = false;
				label_oxy_tip.Visible = false;
				label_thrash_tip.Visible = false;	
				
			}

			
			private void pictureBox_NumShoulders_Click(object sender, System.EventArgs e)
			{
			
			}

			private void checkBox_Thrash_CheckedChanged(object sender, System.EventArgs e)
			{
				mCheckCompleteFit.Enabled = mCheckThrash.Checked;
				
			}

			private void textBox_NumShoulders_Leave(object sender, System.EventArgs e)
			{

				pictureBox_BkgRatio.Visible = false;
				pictureBox_NumShoulders.Visible = false;
				label_shoulders_tip.Visible = false;		
				label_bkg_tip.Visible = false;
				label_ccmass_tip.Visible = true;
				label_fit_tip.Visible = true;
				label_mass_tip.Visible = true;
				label_oxy_tip.Visible = true;
				label_thrash_tip.Visible = true;		
			
			}

			private void mchkBoxUseAbsolutePeptideIntensity_CheckedChanged(object sender, System.EventArgs e)
			{
				mtxtAbsolutePeptideIntensityThreshold.Enabled = mchkBoxUseAbsolutePeptideIntensity.Checked ; 
			}

			private void mchkSumSpectra_CheckedChanged(object sender, System.EventArgs e)
			{
				if (mchkSumSpectra.Checked)
					mchkSumAcrossScanRange.Checked = false ; 								
			}

			private void groupBox2_Enter(object sender, System.EventArgs e)
			{
			
			}

			private void mchkSumAcrossScanRange_CheckStateChanged(object sender, System.EventArgs e)
			{
				mTextNumScansToSum.Enabled = mchkSumAcrossScanRange.Checked ;	
				if (mchkSumAcrossScanRange.Checked)
					mchkSumSpectra.Checked = false ; 
			}

			public int NumScansToSumOver
			{
				get
				{
					return Convert.ToInt16(mTextNumScansToSum.Text) ; 
				}
				set
				{
					mTextNumScansToSum.Text = Convert.ToString(value) ; 
				}
			}

			public bool SumAcrossScanRange				
			{
				get
				{
					return mchkSumAcrossScanRange.Checked ; 
				}
				set
				{
					mchkSumAcrossScanRange.Checked  = value ; 
				}
			}

			public bool SumSpectra
			{
				get
				{
					return mchkSumSpectra.Checked ; 
				}
				set
				{
					mchkSumSpectra.Checked = value ; 
				}
			}

			public short MaxCharge
			{
				get
				{
					return Convert.ToInt16(mTextMaxCharge.Text) ; 
				}
				set
				{
					mTextMaxCharge.Text = Convert.ToString(value) ; 
				}
			}

			public double MaxMW
			{
				get
				{
					return Convert.ToDouble(mTextMaxMass.Text) ; 
				}
				set
				{
					mTextMaxMass.Text = Convert.ToString(value) ; 
				}
			}

			public short NumPeaksForShoulder
			{
				get
				{
					return Convert.ToInt16(mTextNumShoulders.Text) ; 
				}
				set
				{
					mTextNumShoulders.Text = Convert.ToString(value) ; 
				}
			}

			public bool O16O18Media
			{
				get
				{
					return mCheckO16O18.Checked ; 
				}
				set
				{
					mCheckO16O18.Checked = value ; 
				}
			}

			public double PeptideMinBackgroundRatio
			{
				get
				{
					return Convert.ToDouble(mTextPeptideBgRatio.Text) ; 
				}
				set
				{
					mTextPeptideBgRatio.Text = Convert.ToString(value) ; 
				}
			}

			public bool ThrashOrNot
			{
				get
				{
					return mCheckThrash.Checked ; 
				}
				set
				{
					mCheckThrash.Checked = value ; 
				}
			}

			public bool CompleteFit
			{
				get
				{
					return mCheckCompleteFit.Checked ; 
				}
				set
				{
					mCheckCompleteFit.Checked = value ; 
				}
			}

			public bool UseMercuryCaching 
			{
				get
				{
					return mCheckCacheMercury.Checked ; 
				}
				set
				{
					mCheckCacheMercury.Checked = value ; 
				}
			}

			public double CCMass
			{
				get
				{
					return Convert.ToDouble(mTextChargeMass.Text) ; 
				}
				set
				{
					mTextChargeMass.Text = Convert.ToString(value) ; 
				}
			}

			public bool CheckAllPatternsAgainstCharge1
			{
				get
				{
					return mCheckFitAllAgainstCharge1.Checked ; 
				}
				set
				{
					mCheckFitAllAgainstCharge1.Checked = value ; 
				}
			}

			public DeconToolsV2.enmIsotopeFitType IsotopeFitType
			{
				get
				{
					switch (mcmbIsotopeFitType.SelectedIndex)
					{
						case 0:
							return DeconToolsV2.enmIsotopeFitType.PEAK ; 
						case 1:
							return DeconToolsV2.enmIsotopeFitType.AREA ; 
						case 2:
							return DeconToolsV2.enmIsotopeFitType.CHISQ ; 
						default:
							return DeconToolsV2.enmIsotopeFitType.AREA ; 
					}
				}
				set
				{
					switch(value)
					{
						case DeconToolsV2.enmIsotopeFitType.PEAK : 
							mcmbIsotopeFitType.SelectedIndex = 0 ;
							break ;
						case DeconToolsV2.enmIsotopeFitType.AREA : 
							mcmbIsotopeFitType.SelectedIndex = 1 ;
							break ;
						case DeconToolsV2.enmIsotopeFitType.CHISQ : 
							mcmbIsotopeFitType.SelectedIndex = 2 ;
							break ;
						default:
							mcmbIsotopeFitType.SelectedIndex = 0 ;
							break ;
					}
				}
			}

			public bool UseAbsolutePeptideIntensity
			{
				get
				{
					return mchkBoxUseAbsolutePeptideIntensity.Checked ; 
				}
				set
				{
					mchkBoxUseAbsolutePeptideIntensity.Checked = value ; 
				}
			}
			public double AbsolutePeptideIntensity
			{
				get
				{
					return Convert.ToDouble(mtxtAbsolutePeptideIntensityThreshold.Text) ; 
				}
				set
				{
					mtxtAbsolutePeptideIntensityThreshold.Text = Convert.ToString(value); 
				}
			}

			public bool IsActualMonoMZUsed
			{
				get
				{
					return this.mchkUseActualMonoMZ.Checked;
				}
				set
				{
					this.mchkUseActualMonoMZ.Checked = value;
				}
			}

			public double LeftFitStringencyFactor
			{
				get
				{
					return Convert.ToDouble(this.txtLeftStringencyFactor.Text);
				}
				set
				{
					this.txtLeftStringencyFactor.Text = Convert.ToString(value);
				}
			}

			public double RightFitStringencyFactor
			{
				get
				{
					return Convert.ToDouble(this.txtRightStringencyFactor.Text);
				}
				set
				{
					this.txtRightStringencyFactor.Text = Convert.ToString(value);
				}
			}


			//TODO:  Add new Frame-related parameters


		}
	}



