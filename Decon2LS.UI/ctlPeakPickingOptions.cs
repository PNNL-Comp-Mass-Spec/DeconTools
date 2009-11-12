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
	/// Summary description for ctlPeakPickingOptions.
	/// </summary>
	public class ctlPeakPickingOptions : System.Windows.Forms.UserControl
	{
		private System.Windows.Forms.GroupBox groupBox1;		
		private System.Windows.Forms.Splitter splitter1;
		private System.Windows.Forms.PictureBox pictureBox_SNR;
		private System.Windows.Forms.PictureBox pictureBox_Lorentz;
		private System.Windows.Forms.Label label_Lorentz;
		private System.Windows.Forms.PictureBox pictureBox_Quad;
		private System.Windows.Forms.PictureBox pictureBox_Apex;
		private System.Windows.Forms.Label label_quad;
		private System.Windows.Forms.Label label_apex;
		private System.Windows.Forms.Panel panel2;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Label label_snr_tip;
		private System.Windows.Forms.Label label_peak_tip;
		private System.Windows.Forms.Label label_bkg_tip;
		private System.Windows.Forms.Label label_apex_tip;
		private System.Windows.Forms.Label label_lorentzian_tip;
		private System.Windows.Forms.Label label_quadratic_tip;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.PictureBox pictureBox_BkgRatio;
		private System.Windows.Forms.TextBox mtextBgRatio;
		private System.Windows.Forms.TextBox mtextSNR;
		private System.Windows.Forms.ComboBox mcmbPeakFitType;
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public ctlPeakPickingOptions()
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();
			
			// TODO: Add any initialization after the InitializeComponent call
			mcmbPeakFitType.SelectedIndex = 0;
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
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(ctlPeakPickingOptions));
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.mcmbPeakFitType = new System.Windows.Forms.ComboBox();
			this.splitter1 = new System.Windows.Forms.Splitter();
			this.pictureBox_Apex = new System.Windows.Forms.PictureBox();
			this.pictureBox_Lorentz = new System.Windows.Forms.PictureBox();
			this.pictureBox_Quad = new System.Windows.Forms.PictureBox();
			this.label_apex = new System.Windows.Forms.Label();
			this.label_Lorentz = new System.Windows.Forms.Label();
			this.label_quad = new System.Windows.Forms.Label();
			this.label_lorentzian_tip = new System.Windows.Forms.Label();
			this.label_apex_tip = new System.Windows.Forms.Label();
			this.label_quadratic_tip = new System.Windows.Forms.Label();
			this.label_peak_tip = new System.Windows.Forms.Label();
			this.pictureBox_SNR = new System.Windows.Forms.PictureBox();
			this.label_snr_tip = new System.Windows.Forms.Label();
			this.label_bkg_tip = new System.Windows.Forms.Label();
			this.panel2 = new System.Windows.Forms.Panel();
			this.mtextSNR = new System.Windows.Forms.TextBox();
			this.mtextBgRatio = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.panel1 = new System.Windows.Forms.Panel();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.pictureBox_BkgRatio = new System.Windows.Forms.PictureBox();
			this.groupBox1.SuspendLayout();
			this.panel2.SuspendLayout();
			this.panel1.SuspendLayout();
			this.groupBox2.SuspendLayout();
			this.SuspendLayout();
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.mcmbPeakFitType);
			this.groupBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.groupBox1.Location = new System.Drawing.Point(8, 8);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(496, 64);
			this.groupBox1.TabIndex = 0;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Peak Fit Type";
			// 
			// mcmbPeakFitType
			// 
			this.mcmbPeakFitType.Cursor = System.Windows.Forms.Cursors.Arrow;
			this.mcmbPeakFitType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.mcmbPeakFitType.Items.AddRange(new object[] {
																 "Apex",
																 "Lorentzian",
																 "Quadratic"});
			this.mcmbPeakFitType.Location = new System.Drawing.Point(16, 32);
			this.mcmbPeakFitType.Name = "mcmbPeakFitType";
			this.mcmbPeakFitType.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.mcmbPeakFitType.Size = new System.Drawing.Size(328, 21);
			this.mcmbPeakFitType.Sorted = true;
			this.mcmbPeakFitType.TabIndex = 0;
			this.mcmbPeakFitType.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectionIndexChanged);
			// 
			// splitter1
			// 
			this.splitter1.Location = new System.Drawing.Point(0, 0);
			this.splitter1.Name = "splitter1";
			this.splitter1.Size = new System.Drawing.Size(8, 480);
			this.splitter1.TabIndex = 1;
			this.splitter1.TabStop = false;
			// 
			// pictureBox_Apex
			// 
			this.pictureBox_Apex.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox_Apex.Image")));
			this.pictureBox_Apex.Location = new System.Drawing.Point(16, 24);
			this.pictureBox_Apex.Name = "pictureBox_Apex";
			this.pictureBox_Apex.Size = new System.Drawing.Size(104, 144);
			this.pictureBox_Apex.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			this.pictureBox_Apex.TabIndex = 4;
			this.pictureBox_Apex.TabStop = false;
			// 
			// pictureBox_Lorentz
			// 
			this.pictureBox_Lorentz.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox_Lorentz.Image")));
			this.pictureBox_Lorentz.Location = new System.Drawing.Point(184, 24);
			this.pictureBox_Lorentz.Name = "pictureBox_Lorentz";
			this.pictureBox_Lorentz.Size = new System.Drawing.Size(120, 144);
			this.pictureBox_Lorentz.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			this.pictureBox_Lorentz.TabIndex = 1;
			this.pictureBox_Lorentz.TabStop = false;
			// 
			// pictureBox_Quad
			// 
			this.pictureBox_Quad.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox_Quad.Image")));
			this.pictureBox_Quad.Location = new System.Drawing.Point(368, 24);
			this.pictureBox_Quad.Name = "pictureBox_Quad";
			this.pictureBox_Quad.Size = new System.Drawing.Size(112, 144);
			this.pictureBox_Quad.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			this.pictureBox_Quad.TabIndex = 3;
			this.pictureBox_Quad.TabStop = false;
			// 
			// label_apex
			// 
			this.label_apex.Location = new System.Drawing.Point(40, 176);
			this.label_apex.Name = "label_apex";
			this.label_apex.Size = new System.Drawing.Size(48, 16);
			this.label_apex.TabIndex = 6;
			this.label_apex.Text = "Apex";
			// 
			// label_Lorentz
			// 
			this.label_Lorentz.Location = new System.Drawing.Point(216, 176);
			this.label_Lorentz.Name = "label_Lorentz";
			this.label_Lorentz.Size = new System.Drawing.Size(64, 16);
			this.label_Lorentz.TabIndex = 2;
			this.label_Lorentz.Text = "Lorentzian";
			// 
			// label_quad
			// 
			this.label_quad.Location = new System.Drawing.Point(392, 176);
			this.label_quad.Name = "label_quad";
			this.label_quad.Size = new System.Drawing.Size(56, 16);
			this.label_quad.TabIndex = 5;
			this.label_quad.Text = "Quadratic";
			// 
			// label_lorentzian_tip
			// 
			this.label_lorentzian_tip.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.5F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label_lorentzian_tip.Location = new System.Drawing.Point(24, 264);
			this.label_lorentzian_tip.Name = "label_lorentzian_tip";
			this.label_lorentzian_tip.Size = new System.Drawing.Size(320, 24);
			this.label_lorentzian_tip.TabIndex = 10;
			this.label_lorentzian_tip.Text = "- Lorentzian: Does a lorentzian fit to the entire peak profile";
			this.label_lorentzian_tip.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// label_apex_tip
			// 
			this.label_apex_tip.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.5F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label_apex_tip.Location = new System.Drawing.Point(24, 240);
			this.label_apex_tip.Name = "label_apex_tip";
			this.label_apex_tip.Size = new System.Drawing.Size(304, 16);
			this.label_apex_tip.TabIndex = 9;
			this.label_apex_tip.Text = " - Apex: Choose the most intense point in the peak profile";
			this.label_apex_tip.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// label_quadratic_tip
			// 
			this.label_quadratic_tip.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.5F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label_quadratic_tip.Location = new System.Drawing.Point(24, 296);
			this.label_quadratic_tip.Name = "label_quadratic_tip";
			this.label_quadratic_tip.Size = new System.Drawing.Size(456, 24);
			this.label_quadratic_tip.TabIndex = 11;
			this.label_quadratic_tip.Text = "- Quadratic: Chooses three points - the apex,  and one on either side of the prof" +
				"ile,  and performs a quadratic fit";
			this.label_quadratic_tip.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// label_peak_tip
			// 
			this.label_peak_tip.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.5F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label_peak_tip.Location = new System.Drawing.Point(16, 208);
			this.label_peak_tip.Name = "label_peak_tip";
			this.label_peak_tip.Size = new System.Drawing.Size(248, 16);
			this.label_peak_tip.TabIndex = 8;
			this.label_peak_tip.Text = "Sets the type of peak-fitting to be performed";
			this.label_peak_tip.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// pictureBox_SNR
			// 
			this.pictureBox_SNR.BackColor = System.Drawing.SystemColors.Control;
			this.pictureBox_SNR.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox_SNR.Image")));
			this.pictureBox_SNR.Location = new System.Drawing.Point(8, 16);
			this.pictureBox_SNR.Name = "pictureBox_SNR";
			this.pictureBox_SNR.Size = new System.Drawing.Size(480, 264);
			this.pictureBox_SNR.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			this.pictureBox_SNR.TabIndex = 0;
			this.pictureBox_SNR.TabStop = false;
			this.pictureBox_SNR.Visible = false;
			// 
			// label_snr_tip
			// 
			this.label_snr_tip.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.5F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label_snr_tip.Location = new System.Drawing.Point(8, 304);
			this.label_snr_tip.Name = "label_snr_tip";
			this.label_snr_tip.Size = new System.Drawing.Size(352, 24);
			this.label_snr_tip.TabIndex = 1;
			this.label_snr_tip.Text = "Sets the Signal-To-Noise Ratio using the given formula";
			this.label_snr_tip.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.label_snr_tip.Visible = false;
			// 
			// label_bkg_tip
			// 
			this.label_bkg_tip.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.5F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label_bkg_tip.Location = new System.Drawing.Point(8, 304);
			this.label_bkg_tip.Name = "label_bkg_tip";
			this.label_bkg_tip.Size = new System.Drawing.Size(352, 21);
			this.label_bkg_tip.TabIndex = 8;
			this.label_bkg_tip.Text = "Sets the maximum intensity level to be considered as background.";
			this.label_bkg_tip.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.label_bkg_tip.Visible = false;
			// 
			// panel2
			// 
			this.panel2.Controls.Add(this.mtextSNR);
			this.panel2.Controls.Add(this.mtextBgRatio);
			this.panel2.Controls.Add(this.label2);
			this.panel2.Controls.Add(this.label1);
			this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel2.Location = new System.Drawing.Point(0, 0);
			this.panel2.Name = "panel2";
			this.panel2.Size = new System.Drawing.Size(496, 48);
			this.panel2.TabIndex = 4;
			// 
			// mtextSNR
			// 
			this.mtextSNR.Location = new System.Drawing.Point(104, 16);
			this.mtextSNR.Name = "mtextSNR";
			this.mtextSNR.Size = new System.Drawing.Size(48, 20);
			this.mtextSNR.TabIndex = 0;
			this.mtextSNR.Text = "3";
			this.mtextSNR.Leave += new System.EventHandler(this.textBox_SNR_Leave);
			this.mtextSNR.Enter += new System.EventHandler(this.textBox_SNR_Enter);
			// 
			// mtextBgRatio
			// 
			this.mtextBgRatio.Location = new System.Drawing.Point(352, 16);
			this.mtextBgRatio.Name = "mtextBgRatio";
			this.mtextBgRatio.Size = new System.Drawing.Size(48, 20);
			this.mtextBgRatio.TabIndex = 3;
			this.mtextBgRatio.Text = "5";
			this.mtextBgRatio.Leave += new System.EventHandler(this.textBox_BkgRatio_Leave);
			this.mtextBgRatio.Enter += new System.EventHandler(this.textBox_BkgRatio_Enter);
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(168, 16);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(160, 16);
			this.label2.TabIndex = 2;
			this.label2.Text = "Minimum Background Ratio (r)";
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(16, 16);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(72, 16);
			this.label1.TabIndex = 1;
			this.label1.Text = "Minimum S/N";
			// 
			// panel1
			// 
			this.panel1.Controls.Add(this.panel2);
			this.panel1.Location = new System.Drawing.Point(16, 72);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(496, 48);
			this.panel1.TabIndex = 2;
			// 
			// groupBox2
			// 
			this.groupBox2.AccessibleDescription = "p";
			this.groupBox2.Controls.Add(this.pictureBox_Apex);
			this.groupBox2.Controls.Add(this.pictureBox_Quad);
			this.groupBox2.Controls.Add(this.pictureBox_Lorentz);
			this.groupBox2.Controls.Add(this.label_apex);
			this.groupBox2.Controls.Add(this.label_Lorentz);
			this.groupBox2.Controls.Add(this.label_quad);
			this.groupBox2.Controls.Add(this.label_peak_tip);
			this.groupBox2.Controls.Add(this.label_apex_tip);
			this.groupBox2.Controls.Add(this.label_quadratic_tip);
			this.groupBox2.Controls.Add(this.label_lorentzian_tip);
			this.groupBox2.Controls.Add(this.pictureBox_SNR);
			this.groupBox2.Controls.Add(this.label_snr_tip);
			this.groupBox2.Controls.Add(this.pictureBox_BkgRatio);
			this.groupBox2.Controls.Add(this.label_bkg_tip);
			this.groupBox2.Location = new System.Drawing.Point(8, 120);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(496, 344);
			this.groupBox2.TabIndex = 3;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "Helpful Tips";
			// 
			// pictureBox_BkgRatio
			// 
			this.pictureBox_BkgRatio.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox_BkgRatio.Image")));
			this.pictureBox_BkgRatio.Location = new System.Drawing.Point(8, 16);
			this.pictureBox_BkgRatio.Name = "pictureBox_BkgRatio";
			this.pictureBox_BkgRatio.Size = new System.Drawing.Size(472, 256);
			this.pictureBox_BkgRatio.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			this.pictureBox_BkgRatio.TabIndex = 4;
			this.pictureBox_BkgRatio.TabStop = false;
			this.pictureBox_BkgRatio.Visible = false;
			// 
			// ctlPeakPickingOptions
			// 
			this.Controls.Add(this.panel1);
			this.Controls.Add(this.splitter1);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.groupBox2);
			this.Name = "ctlPeakPickingOptions";
			this.Size = new System.Drawing.Size(528, 480);
			this.groupBox1.ResumeLayout(false);
			this.panel2.ResumeLayout(false);
			this.panel1.ResumeLayout(false);
			this.groupBox2.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		private void comboBox1_SelectionIndexChanged(object sender, System.EventArgs e)
		{
			
		}
		
		
		

		private void textBox_BkgRatio_Enter(object sender, System.EventArgs e)
		{
			pictureBox_BkgRatio.Visible = true;
			pictureBox_SNR.Visible = false;
			pictureBox_Apex.Visible = false;
			pictureBox_Lorentz.Visible = false;
			pictureBox_Quad.Visible = false;
			label_apex.Visible = false;
			label_Lorentz.Visible = false;
			label_quad.Visible = false;
			label_peak_tip.Visible = false;
			label_bkg_tip.Visible = true;
            label_snr_tip.Visible = false;
			label_apex_tip.Visible = false;
			label_quadratic_tip.Visible = false;
			label_lorentzian_tip.Visible = false;
			
		}

		private void textBox_SNR_Enter(object sender, System.EventArgs e)
		{
			pictureBox_SNR.Visible = true;
			pictureBox_BkgRatio.Visible = false;
			pictureBox_Apex.Visible = false;
			pictureBox_Lorentz.Visible = false;
			pictureBox_Quad.Visible = false;
			label_apex.Visible = false;
			label_Lorentz.Visible = false;
			label_quad.Visible = false;
			label_peak_tip.Visible = false;
			label_bkg_tip.Visible = false;
			label_snr_tip.Visible = true;
			label_apex_tip.Visible = false;
			label_quadratic_tip.Visible = false;
			label_lorentzian_tip.Visible = false;
			
		}

		private void textBox_SNR_Leave(object sender, System.EventArgs e)
		{
			pictureBox_BkgRatio.Visible = false;
			pictureBox_SNR.Visible = false;
			pictureBox_Apex.Visible = true;
			pictureBox_Lorentz.Visible = true;
			pictureBox_Quad.Visible = true;
			label_apex.Visible = true;
			label_Lorentz.Visible = true;
			label_quad.Visible = true;
			label_peak_tip.Visible = true;
			label_bkg_tip.Visible = false;
			label_snr_tip.Visible = false;
			label_apex_tip.Visible = true;
			label_quadratic_tip.Visible = true;
			label_lorentzian_tip.Visible = true;		
			
		}

		private void textBox_BkgRatio_Leave(object sender, System.EventArgs e)
		{
			pictureBox_BkgRatio.Visible = false;
			pictureBox_SNR.Visible = false;
			pictureBox_Apex.Visible = true;
			pictureBox_Lorentz.Visible = true;
			pictureBox_Quad.Visible = true;
			label_apex.Visible = true;
			label_Lorentz.Visible = true;
			label_quad.Visible = true;
			label_peak_tip.Visible = true;
			label_bkg_tip.Visible = false;
			label_snr_tip.Visible = false;	
			label_apex_tip.Visible = true;
			label_quadratic_tip.Visible = true;
			label_lorentzian_tip.Visible = true;
			
		}
		
		public double PeakBackgroundRatio
		{
			set
			{
				mtextBgRatio.Text = Convert.ToString(value) ; 
			}
			get
			{
				return Convert.ToDouble(mtextBgRatio.Text) ; 
			}
		}
		public double SignalToNoiseThreshold
		{
			set
			{
				mtextSNR.Text = Convert.ToString(value) ; 
			}
			get
			{
				return Convert.ToDouble(mtextSNR.Text) ; 
			}
		}
		public DeconToolsV2.Peaks.PEAK_FIT_TYPE PeakFitType
		{
			get
			{
				if (mcmbPeakFitType.SelectedIndex == 0)
				{
					return DeconToolsV2.Peaks.PEAK_FIT_TYPE.APEX ; 
				}
				else if (mcmbPeakFitType.SelectedIndex == 1)
				{
					return DeconToolsV2.Peaks.PEAK_FIT_TYPE.LORENTZIAN ; 
				}
				return DeconToolsV2.Peaks.PEAK_FIT_TYPE.QUADRATIC ; 
			}
			set
			{
				switch (value)
				{
					case DeconToolsV2.Peaks.PEAK_FIT_TYPE.APEX:
						mcmbPeakFitType.SelectedIndex = 0 ; 
						break ; 
					case DeconToolsV2.Peaks.PEAK_FIT_TYPE.LORENTZIAN:
						mcmbPeakFitType.SelectedIndex = 1 ; 
						break ; 
					case DeconToolsV2.Peaks.PEAK_FIT_TYPE.QUADRATIC:
						mcmbPeakFitType.SelectedIndex = 2 ; 
						break ; 
					default:
						break ; 
				}
			}
		}
	}
}
