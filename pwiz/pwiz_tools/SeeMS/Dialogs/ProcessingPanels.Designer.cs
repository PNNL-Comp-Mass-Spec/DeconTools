﻿//
// $Id: ProcessingPanels.Designer.cs 1593 2009-12-03 17:21:14Z chambm $
//
//
// Original author: Matt Chambers <matt.chambers .@. vanderbilt.edu>
//
// Copyright 2009 Vanderbilt University - Nashville, TN 37232
//
// Licensed under the Apache License, Version 2.0 (the "License"); 
// you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at 
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software 
// distributed under the License is distributed on an "AS IS" BASIS, 
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
// See the License for the specific language governing permissions and 
// limitations under the License.
//

namespace seems
{
    partial class ProcessingPanels
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose( bool disposing )
        {
            if( disposing && ( components != null ) )
            {
                components.Dispose();
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
            this.processingPanelsTabControl = new System.Windows.Forms.TabControl();
            this.thresholderTabPage = new System.Windows.Forms.TabPage();
            this.thresholderPanel = new System.Windows.Forms.Panel();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.thresholderValueTextBox = new System.Windows.Forms.TextBox();
            this.thresholderOrientationComboBox = new System.Windows.Forms.ComboBox();
            this.thresholderTypeComboBox = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.smootherTabPage = new System.Windows.Forms.TabPage();
            this.smootherPanel = new System.Windows.Forms.Panel();
            this.smootherWhittakerParameters = new System.Windows.Forms.GroupBox();
            this.label8 = new System.Windows.Forms.Label();
            this.smootherWhittakerLambdaTextBox = new System.Windows.Forms.TextBox();
            this.smootherSavitzkyGolayParameters = new System.Windows.Forms.GroupBox();
            this.smootherSavitzkyGolayPolynomialOrderTrackBar = new System.Windows.Forms.TrackBar();
            this.smootherSavitzkyGolayWindowSizeTrackBar = new System.Windows.Forms.TrackBar();
            this.smootherSavitzkyGolayPolynomialOrderLabel = new System.Windows.Forms.Label();
            this.smootherSavitzkyGolayWindowSizeLabel = new System.Windows.Forms.Label();
            this.smootherParametersGroupBox = new System.Windows.Forms.GroupBox();
            this.label5 = new System.Windows.Forms.Label();
            this.smootherAlgorithmComboBox = new System.Windows.Forms.ComboBox();
            this.peakPickerTabPage = new System.Windows.Forms.TabPage();
            this.peakPickerPanel = new System.Windows.Forms.Panel();
            this.peakPickerLocalMaximumParameters = new System.Windows.Forms.GroupBox();
            this.peakPickerLocalMaximumWindowSizeTrackBar = new System.Windows.Forms.TrackBar();
            this.peakPickerLocalMaximumWindowSizeLabel = new System.Windows.Forms.Label();
            this.peakPickerParametersGroupBox = new System.Windows.Forms.GroupBox();
            this.peakPickerPreferVendorCentroidingCheckbox = new System.Windows.Forms.CheckBox();
            this.label4 = new System.Windows.Forms.Label();
            this.peakPickerAlgorithmComboBox = new System.Windows.Forms.ComboBox();
            this.trackBar1 = new System.Windows.Forms.TrackBar();
            this.trackBar2 = new System.Windows.Forms.TrackBar();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.chargeStateCalculatorTabPage = new System.Windows.Forms.TabPage();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.trackBar3 = new System.Windows.Forms.TrackBar();
            this.label9 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.label10 = new System.Windows.Forms.Label();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.chargeStateCalculatorPanel = new System.Windows.Forms.Panel();
            this.chargeStateCalculatorOverrideExistingCheckBox = new System.Windows.Forms.CheckBox();
            this.chargeStateCalculatorMaxChargeUpDown = new System.Windows.Forms.NumericUpDown();
            this.chargeStateCalculatorMinChargeUpDown = new System.Windows.Forms.NumericUpDown();
            this.label11 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.chargeStateCalculatorIntensityFraction = new System.Windows.Forms.TextBox();
            this.processingPanelsTabControl.SuspendLayout();
            this.thresholderTabPage.SuspendLayout();
            this.thresholderPanel.SuspendLayout();
            this.smootherTabPage.SuspendLayout();
            this.smootherPanel.SuspendLayout();
            this.smootherWhittakerParameters.SuspendLayout();
            this.smootherSavitzkyGolayParameters.SuspendLayout();
            ( (System.ComponentModel.ISupportInitialize) ( this.smootherSavitzkyGolayPolynomialOrderTrackBar ) ).BeginInit();
            ( (System.ComponentModel.ISupportInitialize) ( this.smootherSavitzkyGolayWindowSizeTrackBar ) ).BeginInit();
            this.peakPickerTabPage.SuspendLayout();
            this.peakPickerPanel.SuspendLayout();
            this.peakPickerLocalMaximumParameters.SuspendLayout();
            ( (System.ComponentModel.ISupportInitialize) ( this.peakPickerLocalMaximumWindowSizeTrackBar ) ).BeginInit();
            ( (System.ComponentModel.ISupportInitialize) ( this.trackBar1 ) ).BeginInit();
            ( (System.ComponentModel.ISupportInitialize) ( this.trackBar2 ) ).BeginInit();
            this.chargeStateCalculatorTabPage.SuspendLayout();
            this.groupBox1.SuspendLayout();
            ( (System.ComponentModel.ISupportInitialize) ( this.trackBar3 ) ).BeginInit();
            this.chargeStateCalculatorPanel.SuspendLayout();
            ( (System.ComponentModel.ISupportInitialize) ( this.chargeStateCalculatorMaxChargeUpDown ) ).BeginInit();
            ( (System.ComponentModel.ISupportInitialize) ( this.chargeStateCalculatorMinChargeUpDown ) ).BeginInit();
            this.SuspendLayout();
            // 
            // processingPanelsTabControl
            // 
            this.processingPanelsTabControl.Controls.Add( this.thresholderTabPage );
            this.processingPanelsTabControl.Controls.Add( this.smootherTabPage );
            this.processingPanelsTabControl.Controls.Add( this.peakPickerTabPage );
            this.processingPanelsTabControl.Controls.Add( this.chargeStateCalculatorTabPage );
            this.processingPanelsTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.processingPanelsTabControl.Location = new System.Drawing.Point( 0, 0 );
            this.processingPanelsTabControl.Name = "processingPanelsTabControl";
            this.processingPanelsTabControl.SelectedIndex = 0;
            this.processingPanelsTabControl.Size = new System.Drawing.Size( 716, 752 );
            this.processingPanelsTabControl.TabIndex = 0;
            // 
            // thresholderTabPage
            // 
            this.thresholderTabPage.BackColor = System.Drawing.Color.DimGray;
            this.thresholderTabPage.Controls.Add( this.thresholderPanel );
            this.thresholderTabPage.Location = new System.Drawing.Point( 4, 22 );
            this.thresholderTabPage.Name = "thresholderTabPage";
            this.thresholderTabPage.Padding = new System.Windows.Forms.Padding( 3 );
            this.thresholderTabPage.Size = new System.Drawing.Size( 708, 726 );
            this.thresholderTabPage.TabIndex = 0;
            this.thresholderTabPage.Text = "Thresholder";
            // 
            // thresholderPanel
            // 
            this.thresholderPanel.BackColor = System.Drawing.SystemColors.Control;
            this.thresholderPanel.Controls.Add( this.label3 );
            this.thresholderPanel.Controls.Add( this.label2 );
            this.thresholderPanel.Controls.Add( this.thresholderValueTextBox );
            this.thresholderPanel.Controls.Add( this.thresholderOrientationComboBox );
            this.thresholderPanel.Controls.Add( this.thresholderTypeComboBox );
            this.thresholderPanel.Controls.Add( this.label1 );
            this.thresholderPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.thresholderPanel.Location = new System.Drawing.Point( 3, 3 );
            this.thresholderPanel.Name = "thresholderPanel";
            this.thresholderPanel.Size = new System.Drawing.Size( 702, 720 );
            this.thresholderPanel.TabIndex = 0;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point( 4, 62 );
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size( 57, 13 );
            this.label3.TabIndex = 13;
            this.label3.Text = "Threshold:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point( 4, 35 );
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size( 61, 13 );
            this.label2.TabIndex = 12;
            this.label2.Text = "Orientation:";
            // 
            // thresholderValueTextBox
            // 
            this.thresholderValueTextBox.Location = new System.Drawing.Point( 70, 59 );
            this.thresholderValueTextBox.Name = "thresholderValueTextBox";
            this.thresholderValueTextBox.Size = new System.Drawing.Size( 166, 20 );
            this.thresholderValueTextBox.TabIndex = 11;
            // 
            // thresholderOrientationComboBox
            // 
            this.thresholderOrientationComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.thresholderOrientationComboBox.FormattingEnabled = true;
            this.thresholderOrientationComboBox.Location = new System.Drawing.Point( 70, 32 );
            this.thresholderOrientationComboBox.Name = "thresholderOrientationComboBox";
            this.thresholderOrientationComboBox.Size = new System.Drawing.Size( 166, 21 );
            this.thresholderOrientationComboBox.TabIndex = 10;
            // 
            // thresholderTypeComboBox
            // 
            this.thresholderTypeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.thresholderTypeComboBox.FormattingEnabled = true;
            this.thresholderTypeComboBox.Location = new System.Drawing.Point( 70, 5 );
            this.thresholderTypeComboBox.Name = "thresholderTypeComboBox";
            this.thresholderTypeComboBox.Size = new System.Drawing.Size( 166, 21 );
            this.thresholderTypeComboBox.TabIndex = 9;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point( 4, 8 );
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size( 34, 13 );
            this.label1.TabIndex = 2;
            this.label1.Text = "Type:";
            // 
            // smootherTabPage
            // 
            this.smootherTabPage.Controls.Add( this.smootherPanel );
            this.smootherTabPage.Location = new System.Drawing.Point( 4, 22 );
            this.smootherTabPage.Name = "smootherTabPage";
            this.smootherTabPage.Padding = new System.Windows.Forms.Padding( 3 );
            this.smootherTabPage.Size = new System.Drawing.Size( 708, 726 );
            this.smootherTabPage.TabIndex = 1;
            this.smootherTabPage.Text = "Smoother";
            this.smootherTabPage.UseVisualStyleBackColor = true;
            // 
            // smootherPanel
            // 
            this.smootherPanel.BackColor = System.Drawing.SystemColors.Control;
            this.smootherPanel.Controls.Add( this.smootherWhittakerParameters );
            this.smootherPanel.Controls.Add( this.smootherSavitzkyGolayParameters );
            this.smootherPanel.Controls.Add( this.smootherParametersGroupBox );
            this.smootherPanel.Controls.Add( this.label5 );
            this.smootherPanel.Controls.Add( this.smootherAlgorithmComboBox );
            this.smootherPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.smootherPanel.Location = new System.Drawing.Point( 3, 3 );
            this.smootherPanel.Name = "smootherPanel";
            this.smootherPanel.Size = new System.Drawing.Size( 702, 720 );
            this.smootherPanel.TabIndex = 0;
            // 
            // smootherWhittakerParameters
            // 
            this.smootherWhittakerParameters.Controls.Add( this.label8 );
            this.smootherWhittakerParameters.Controls.Add( this.smootherWhittakerLambdaTextBox );
            this.smootherWhittakerParameters.Location = new System.Drawing.Point( 62, 203 );
            this.smootherWhittakerParameters.Name = "smootherWhittakerParameters";
            this.smootherWhittakerParameters.Size = new System.Drawing.Size( 219, 53 );
            this.smootherWhittakerParameters.TabIndex = 6;
            this.smootherWhittakerParameters.TabStop = false;
            this.smootherWhittakerParameters.Text = "Whittaker Parameters";
            this.smootherWhittakerParameters.Visible = false;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point( 6, 22 );
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size( 48, 13 );
            this.label8.TabIndex = 0;
            this.label8.Text = "Lambda:";
            // 
            // smootherWhittakerLambdaTextBox
            // 
            this.smootherWhittakerLambdaTextBox.Location = new System.Drawing.Point( 60, 19 );
            this.smootherWhittakerLambdaTextBox.Name = "smootherWhittakerLambdaTextBox";
            this.smootherWhittakerLambdaTextBox.Size = new System.Drawing.Size( 149, 20 );
            this.smootherWhittakerLambdaTextBox.TabIndex = 1;
            // 
            // smootherSavitzkyGolayParameters
            // 
            this.smootherSavitzkyGolayParameters.Controls.Add( this.smootherSavitzkyGolayPolynomialOrderTrackBar );
            this.smootherSavitzkyGolayParameters.Controls.Add( this.smootherSavitzkyGolayWindowSizeTrackBar );
            this.smootherSavitzkyGolayParameters.Controls.Add( this.smootherSavitzkyGolayPolynomialOrderLabel );
            this.smootherSavitzkyGolayParameters.Controls.Add( this.smootherSavitzkyGolayWindowSizeLabel );
            this.smootherSavitzkyGolayParameters.Location = new System.Drawing.Point( 62, 119 );
            this.smootherSavitzkyGolayParameters.Name = "smootherSavitzkyGolayParameters";
            this.smootherSavitzkyGolayParameters.Size = new System.Drawing.Size( 219, 78 );
            this.smootherSavitzkyGolayParameters.TabIndex = 5;
            this.smootherSavitzkyGolayParameters.TabStop = false;
            this.smootherSavitzkyGolayParameters.Text = "Savitzky-Golay Parameters";
            this.smootherSavitzkyGolayParameters.Visible = false;
            // 
            // smootherSavitzkyGolayPolynomialOrderTrackBar
            // 
            this.smootherSavitzkyGolayPolynomialOrderTrackBar.AutoSize = false;
            this.smootherSavitzkyGolayPolynomialOrderTrackBar.LargeChange = 2;
            this.smootherSavitzkyGolayPolynomialOrderTrackBar.Location = new System.Drawing.Point( 110, 18 );
            this.smootherSavitzkyGolayPolynomialOrderTrackBar.Maximum = 20;
            this.smootherSavitzkyGolayPolynomialOrderTrackBar.Minimum = 2;
            this.smootherSavitzkyGolayPolynomialOrderTrackBar.Name = "smootherSavitzkyGolayPolynomialOrderTrackBar";
            this.smootherSavitzkyGolayPolynomialOrderTrackBar.Size = new System.Drawing.Size( 108, 26 );
            this.smootherSavitzkyGolayPolynomialOrderTrackBar.TabIndex = 1;
            this.smootherSavitzkyGolayPolynomialOrderTrackBar.TickStyle = System.Windows.Forms.TickStyle.None;
            this.smootherSavitzkyGolayPolynomialOrderTrackBar.Value = 2;
            this.smootherSavitzkyGolayPolynomialOrderTrackBar.ValueChanged += new System.EventHandler( this.smootherSavitzkyGolayTrackBar_ValueChanged );
            // 
            // smootherSavitzkyGolayWindowSizeTrackBar
            // 
            this.smootherSavitzkyGolayWindowSizeTrackBar.AutoSize = false;
            this.smootherSavitzkyGolayWindowSizeTrackBar.LargeChange = 20;
            this.smootherSavitzkyGolayWindowSizeTrackBar.Location = new System.Drawing.Point( 110, 46 );
            this.smootherSavitzkyGolayWindowSizeTrackBar.Maximum = 99;
            this.smootherSavitzkyGolayWindowSizeTrackBar.Minimum = 5;
            this.smootherSavitzkyGolayWindowSizeTrackBar.Name = "smootherSavitzkyGolayWindowSizeTrackBar";
            this.smootherSavitzkyGolayWindowSizeTrackBar.Size = new System.Drawing.Size( 108, 26 );
            this.smootherSavitzkyGolayWindowSizeTrackBar.SmallChange = 2;
            this.smootherSavitzkyGolayWindowSizeTrackBar.TabIndex = 2;
            this.smootherSavitzkyGolayWindowSizeTrackBar.TickFrequency = 2;
            this.smootherSavitzkyGolayWindowSizeTrackBar.TickStyle = System.Windows.Forms.TickStyle.None;
            this.smootherSavitzkyGolayWindowSizeTrackBar.Value = 5;
            this.smootherSavitzkyGolayWindowSizeTrackBar.ValueChanged += new System.EventHandler( this.smootherSavitzkyGolayTrackBar_ValueChanged );
            // 
            // smootherSavitzkyGolayPolynomialOrderLabel
            // 
            this.smootherSavitzkyGolayPolynomialOrderLabel.AutoSize = true;
            this.smootherSavitzkyGolayPolynomialOrderLabel.Location = new System.Drawing.Point( 6, 22 );
            this.smootherSavitzkyGolayPolynomialOrderLabel.Name = "smootherSavitzkyGolayPolynomialOrderLabel";
            this.smootherSavitzkyGolayPolynomialOrderLabel.Size = new System.Drawing.Size( 87, 13 );
            this.smootherSavitzkyGolayPolynomialOrderLabel.TabIndex = 0;
            this.smootherSavitzkyGolayPolynomialOrderLabel.Text = "Polynomial order:";
            // 
            // smootherSavitzkyGolayWindowSizeLabel
            // 
            this.smootherSavitzkyGolayWindowSizeLabel.AutoSize = true;
            this.smootherSavitzkyGolayWindowSizeLabel.Location = new System.Drawing.Point( 6, 48 );
            this.smootherSavitzkyGolayWindowSizeLabel.Name = "smootherSavitzkyGolayWindowSizeLabel";
            this.smootherSavitzkyGolayWindowSizeLabel.Size = new System.Drawing.Size( 70, 13 );
            this.smootherSavitzkyGolayWindowSizeLabel.TabIndex = 0;
            this.smootherSavitzkyGolayWindowSizeLabel.Text = "Window size:";
            // 
            // smootherParametersGroupBox
            // 
            this.smootherParametersGroupBox.Location = new System.Drawing.Point( 6, 35 );
            this.smootherParametersGroupBox.Name = "smootherParametersGroupBox";
            this.smootherParametersGroupBox.Size = new System.Drawing.Size( 274, 78 );
            this.smootherParametersGroupBox.TabIndex = 4;
            this.smootherParametersGroupBox.TabStop = false;
            this.smootherParametersGroupBox.Text = "Smoother Parameters Placeholder";
            this.smootherParametersGroupBox.Visible = false;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point( 3, 11 );
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size( 53, 13 );
            this.label5.TabIndex = 0;
            this.label5.Text = "Algorithm:";
            // 
            // smootherAlgorithmComboBox
            // 
            this.smootherAlgorithmComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.smootherAlgorithmComboBox.FormattingEnabled = true;
            this.smootherAlgorithmComboBox.Location = new System.Drawing.Point( 62, 8 );
            this.smootherAlgorithmComboBox.Name = "smootherAlgorithmComboBox";
            this.smootherAlgorithmComboBox.Size = new System.Drawing.Size( 218, 21 );
            this.smootherAlgorithmComboBox.TabIndex = 0;
            // 
            // peakPickerTabPage
            // 
            this.peakPickerTabPage.Controls.Add( this.peakPickerPanel );
            this.peakPickerTabPage.Location = new System.Drawing.Point( 4, 22 );
            this.peakPickerTabPage.Name = "peakPickerTabPage";
            this.peakPickerTabPage.Padding = new System.Windows.Forms.Padding( 3 );
            this.peakPickerTabPage.Size = new System.Drawing.Size( 708, 726 );
            this.peakPickerTabPage.TabIndex = 2;
            this.peakPickerTabPage.Text = "Peak Picker";
            this.peakPickerTabPage.UseVisualStyleBackColor = true;
            // 
            // peakPickerPanel
            // 
            this.peakPickerPanel.BackColor = System.Drawing.SystemColors.Control;
            this.peakPickerPanel.Controls.Add( this.peakPickerLocalMaximumParameters );
            this.peakPickerPanel.Controls.Add( this.peakPickerParametersGroupBox );
            this.peakPickerPanel.Controls.Add( this.peakPickerPreferVendorCentroidingCheckbox );
            this.peakPickerPanel.Controls.Add( this.label4 );
            this.peakPickerPanel.Controls.Add( this.peakPickerAlgorithmComboBox );
            this.peakPickerPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.peakPickerPanel.Location = new System.Drawing.Point( 3, 3 );
            this.peakPickerPanel.Name = "peakPickerPanel";
            this.peakPickerPanel.Size = new System.Drawing.Size( 702, 720 );
            this.peakPickerPanel.TabIndex = 1;
            // 
            // peakPickerLocalMaximumParameters
            // 
            this.peakPickerLocalMaximumParameters.Controls.Add( this.peakPickerLocalMaximumWindowSizeTrackBar );
            this.peakPickerLocalMaximumParameters.Controls.Add( this.peakPickerLocalMaximumWindowSizeLabel );
            this.peakPickerLocalMaximumParameters.Location = new System.Drawing.Point( 11, 145 );
            this.peakPickerLocalMaximumParameters.Name = "peakPickerLocalMaximumParameters";
            this.peakPickerLocalMaximumParameters.Size = new System.Drawing.Size( 218, 54 );
            this.peakPickerLocalMaximumParameters.TabIndex = 7;
            this.peakPickerLocalMaximumParameters.TabStop = false;
            this.peakPickerLocalMaximumParameters.Text = "Local Maximum Parameters";
            this.peakPickerLocalMaximumParameters.Visible = false;
            // 
            // peakPickerLocalMaximumWindowSizeTrackBar
            // 
            this.peakPickerLocalMaximumWindowSizeTrackBar.AutoSize = false;
            this.peakPickerLocalMaximumWindowSizeTrackBar.LargeChange = 20;
            this.peakPickerLocalMaximumWindowSizeTrackBar.Location = new System.Drawing.Point( 102, 19 );
            this.peakPickerLocalMaximumWindowSizeTrackBar.Maximum = 99;
            this.peakPickerLocalMaximumWindowSizeTrackBar.Minimum = 3;
            this.peakPickerLocalMaximumWindowSizeTrackBar.Name = "peakPickerLocalMaximumWindowSizeTrackBar";
            this.peakPickerLocalMaximumWindowSizeTrackBar.Size = new System.Drawing.Size( 108, 26 );
            this.peakPickerLocalMaximumWindowSizeTrackBar.SmallChange = 2;
            this.peakPickerLocalMaximumWindowSizeTrackBar.TabIndex = 2;
            this.peakPickerLocalMaximumWindowSizeTrackBar.TickFrequency = 2;
            this.peakPickerLocalMaximumWindowSizeTrackBar.TickStyle = System.Windows.Forms.TickStyle.None;
            this.peakPickerLocalMaximumWindowSizeTrackBar.Value = 3;
            this.peakPickerLocalMaximumWindowSizeTrackBar.ValueChanged += new System.EventHandler( this.peakPickerLocalMaximumWindowSizeTrackBar_ValueChanged );
            // 
            // peakPickerLocalMaximumWindowSizeLabel
            // 
            this.peakPickerLocalMaximumWindowSizeLabel.AutoSize = true;
            this.peakPickerLocalMaximumWindowSizeLabel.Location = new System.Drawing.Point( 6, 26 );
            this.peakPickerLocalMaximumWindowSizeLabel.Name = "peakPickerLocalMaximumWindowSizeLabel";
            this.peakPickerLocalMaximumWindowSizeLabel.Size = new System.Drawing.Size( 70, 13 );
            this.peakPickerLocalMaximumWindowSizeLabel.TabIndex = 0;
            this.peakPickerLocalMaximumWindowSizeLabel.Text = "Window size:";
            // 
            // peakPickerParametersGroupBox
            // 
            this.peakPickerParametersGroupBox.Location = new System.Drawing.Point( 10, 61 );
            this.peakPickerParametersGroupBox.Name = "peakPickerParametersGroupBox";
            this.peakPickerParametersGroupBox.Size = new System.Drawing.Size( 246, 78 );
            this.peakPickerParametersGroupBox.TabIndex = 6;
            this.peakPickerParametersGroupBox.TabStop = false;
            this.peakPickerParametersGroupBox.Text = "Peak Detector Parameters Placeholder";
            this.peakPickerParametersGroupBox.Visible = false;
            // 
            // peakPickerPreferVendorCentroidingCheckbox
            // 
            this.peakPickerPreferVendorCentroidingCheckbox.AutoSize = true;
            this.peakPickerPreferVendorCentroidingCheckbox.Checked = true;
            this.peakPickerPreferVendorCentroidingCheckbox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.peakPickerPreferVendorCentroidingCheckbox.Location = new System.Drawing.Point( 10, 10 );
            this.peakPickerPreferVendorCentroidingCheckbox.Name = "peakPickerPreferVendorCentroidingCheckbox";
            this.peakPickerPreferVendorCentroidingCheckbox.Size = new System.Drawing.Size( 154, 17 );
            this.peakPickerPreferVendorCentroidingCheckbox.TabIndex = 0;
            this.peakPickerPreferVendorCentroidingCheckbox.Text = "Prefer vendor peak picking";
            this.peakPickerPreferVendorCentroidingCheckbox.UseVisualStyleBackColor = true;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point( 7, 36 );
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size( 53, 13 );
            this.label4.TabIndex = 1;
            this.label4.Text = "Algorithm:";
            // 
            // peakPickerAlgorithmComboBox
            // 
            this.peakPickerAlgorithmComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.peakPickerAlgorithmComboBox.FormattingEnabled = true;
            this.peakPickerAlgorithmComboBox.Location = new System.Drawing.Point( 66, 33 );
            this.peakPickerAlgorithmComboBox.Name = "peakPickerAlgorithmComboBox";
            this.peakPickerAlgorithmComboBox.Size = new System.Drawing.Size( 190, 21 );
            this.peakPickerAlgorithmComboBox.TabIndex = 1;
            // 
            // trackBar1
            // 
            this.trackBar1.AutoSize = false;
            this.trackBar1.LargeChange = 2;
            this.trackBar1.Location = new System.Drawing.Point( 110, 18 );
            this.trackBar1.Maximum = 20;
            this.trackBar1.Minimum = 2;
            this.trackBar1.Name = "trackBar1";
            this.trackBar1.Size = new System.Drawing.Size( 108, 26 );
            this.trackBar1.TabIndex = 1;
            this.trackBar1.TickStyle = System.Windows.Forms.TickStyle.None;
            this.trackBar1.Value = 2;
            // 
            // trackBar2
            // 
            this.trackBar2.AutoSize = false;
            this.trackBar2.LargeChange = 20;
            this.trackBar2.Location = new System.Drawing.Point( 110, 46 );
            this.trackBar2.Maximum = 99;
            this.trackBar2.Minimum = 5;
            this.trackBar2.Name = "trackBar2";
            this.trackBar2.Size = new System.Drawing.Size( 108, 26 );
            this.trackBar2.SmallChange = 2;
            this.trackBar2.TabIndex = 2;
            this.trackBar2.TickFrequency = 2;
            this.trackBar2.TickStyle = System.Windows.Forms.TickStyle.None;
            this.trackBar2.Value = 5;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point( 6, 22 );
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size( 87, 13 );
            this.label6.TabIndex = 0;
            this.label6.Text = "Polynomial order:";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point( 6, 48 );
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size( 70, 13 );
            this.label7.TabIndex = 0;
            this.label7.Text = "Window size:";
            // 
            // chargeStateCalculatorTabPage
            // 
            this.chargeStateCalculatorTabPage.Controls.Add( this.chargeStateCalculatorPanel );
            this.chargeStateCalculatorTabPage.Location = new System.Drawing.Point( 4, 22 );
            this.chargeStateCalculatorTabPage.Name = "chargeStateCalculatorTabPage";
            this.chargeStateCalculatorTabPage.Padding = new System.Windows.Forms.Padding( 3 );
            this.chargeStateCalculatorTabPage.Size = new System.Drawing.Size( 708, 726 );
            this.chargeStateCalculatorTabPage.TabIndex = 3;
            this.chargeStateCalculatorTabPage.Text = "Charge State Calculator";
            this.chargeStateCalculatorTabPage.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add( this.trackBar3 );
            this.groupBox1.Controls.Add( this.label9 );
            this.groupBox1.Location = new System.Drawing.Point( 11, 145 );
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size( 218, 54 );
            this.groupBox1.TabIndex = 7;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Local Maximum Parameters";
            this.groupBox1.Visible = false;
            // 
            // trackBar3
            // 
            this.trackBar3.AutoSize = false;
            this.trackBar3.LargeChange = 20;
            this.trackBar3.Location = new System.Drawing.Point( 102, 19 );
            this.trackBar3.Maximum = 99;
            this.trackBar3.Minimum = 3;
            this.trackBar3.Name = "trackBar3";
            this.trackBar3.Size = new System.Drawing.Size( 108, 26 );
            this.trackBar3.SmallChange = 2;
            this.trackBar3.TabIndex = 2;
            this.trackBar3.TickFrequency = 2;
            this.trackBar3.TickStyle = System.Windows.Forms.TickStyle.None;
            this.trackBar3.Value = 3;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point( 6, 26 );
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size( 70, 13 );
            this.label9.TabIndex = 0;
            this.label9.Text = "Window size:";
            // 
            // groupBox2
            // 
            this.groupBox2.Location = new System.Drawing.Point( 10, 61 );
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size( 246, 78 );
            this.groupBox2.TabIndex = 6;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Peak Detector Parameters Placeholder";
            this.groupBox2.Visible = false;
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Checked = true;
            this.checkBox1.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox1.Location = new System.Drawing.Point( 10, 10 );
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size( 154, 17 );
            this.checkBox1.TabIndex = 0;
            this.checkBox1.Text = "Prefer vendor peak picking";
            this.checkBox1.UseVisualStyleBackColor = true;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point( 7, 36 );
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size( 53, 13 );
            this.label10.TabIndex = 1;
            this.label10.Text = "Algorithm:";
            // 
            // comboBox1
            // 
            this.comboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Location = new System.Drawing.Point( 66, 33 );
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size( 190, 21 );
            this.comboBox1.TabIndex = 1;
            // 
            // chargeStateCalculatorPanel
            // 
            this.chargeStateCalculatorPanel.BackColor = System.Drawing.SystemColors.Control;
            this.chargeStateCalculatorPanel.Controls.Add( this.label13 );
            this.chargeStateCalculatorPanel.Controls.Add( this.chargeStateCalculatorIntensityFraction );
            this.chargeStateCalculatorPanel.Controls.Add( this.chargeStateCalculatorMaxChargeUpDown );
            this.chargeStateCalculatorPanel.Controls.Add( this.chargeStateCalculatorMinChargeUpDown );
            this.chargeStateCalculatorPanel.Controls.Add( this.label11 );
            this.chargeStateCalculatorPanel.Controls.Add( this.label12 );
            this.chargeStateCalculatorPanel.Controls.Add( this.chargeStateCalculatorOverrideExistingCheckBox );
            this.chargeStateCalculatorPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chargeStateCalculatorPanel.Location = new System.Drawing.Point( 3, 3 );
            this.chargeStateCalculatorPanel.Name = "chargeStateCalculatorPanel";
            this.chargeStateCalculatorPanel.Size = new System.Drawing.Size( 702, 720 );
            this.chargeStateCalculatorPanel.TabIndex = 2;
            // 
            // chargeStateCalculatorOverrideExistingCheckBox
            // 
            this.chargeStateCalculatorOverrideExistingCheckBox.AutoSize = true;
            this.chargeStateCalculatorOverrideExistingCheckBox.Checked = true;
            this.chargeStateCalculatorOverrideExistingCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chargeStateCalculatorOverrideExistingCheckBox.Location = new System.Drawing.Point( 10, 10 );
            this.chargeStateCalculatorOverrideExistingCheckBox.Name = "chargeStateCalculatorOverrideExistingCheckBox";
            this.chargeStateCalculatorOverrideExistingCheckBox.Size = new System.Drawing.Size( 166, 17 );
            this.chargeStateCalculatorOverrideExistingCheckBox.TabIndex = 0;
            this.chargeStateCalculatorOverrideExistingCheckBox.Text = "Override existing charge state";
            this.chargeStateCalculatorOverrideExistingCheckBox.UseVisualStyleBackColor = true;
            // 
            // chargeStateCalculatorMaxChargeUpDown
            // 
            this.chargeStateCalculatorMaxChargeUpDown.Location = new System.Drawing.Point( 123, 58 );
            this.chargeStateCalculatorMaxChargeUpDown.Minimum = new decimal( new int[] {
            1,
            0,
            0,
            0} );
            this.chargeStateCalculatorMaxChargeUpDown.Name = "chargeStateCalculatorMaxChargeUpDown";
            this.chargeStateCalculatorMaxChargeUpDown.Size = new System.Drawing.Size( 41, 20 );
            this.chargeStateCalculatorMaxChargeUpDown.TabIndex = 12;
            this.chargeStateCalculatorMaxChargeUpDown.Value = new decimal( new int[] {
            1,
            0,
            0,
            0} );
            // 
            // chargeStateCalculatorMinChargeUpDown
            // 
            this.chargeStateCalculatorMinChargeUpDown.Location = new System.Drawing.Point( 123, 32 );
            this.chargeStateCalculatorMinChargeUpDown.Minimum = new decimal( new int[] {
            1,
            0,
            0,
            0} );
            this.chargeStateCalculatorMinChargeUpDown.Name = "chargeStateCalculatorMinChargeUpDown";
            this.chargeStateCalculatorMinChargeUpDown.Size = new System.Drawing.Size( 41, 20 );
            this.chargeStateCalculatorMinChargeUpDown.TabIndex = 11;
            this.chargeStateCalculatorMinChargeUpDown.Value = new decimal( new int[] {
            1,
            0,
            0,
            0} );
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point( 7, 59 );
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size( 116, 13 );
            this.label11.TabIndex = 10;
            this.label11.Text = "Max. precursor charge:";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point( 7, 34 );
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size( 113, 13 );
            this.label12.TabIndex = 9;
            this.label12.Text = "Min. precursor charge:";
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point( 7, 81 );
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size( 274, 13 );
            this.label13.TabIndex = 15;
            this.label13.Text = "Fraction of intensity below precursor m/z for +1 products:";
            // 
            // chargeStateCalculatorIntensityFraction
            // 
            this.chargeStateCalculatorIntensityFraction.Location = new System.Drawing.Point( 287, 78 );
            this.chargeStateCalculatorIntensityFraction.Name = "chargeStateCalculatorIntensityFraction";
            this.chargeStateCalculatorIntensityFraction.Size = new System.Drawing.Size( 62, 20 );
            this.chargeStateCalculatorIntensityFraction.TabIndex = 14;
            // 
            // ProcessingPanels
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF( 6F, 13F );
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add( this.processingPanelsTabControl );
            this.Name = "ProcessingPanels";
            this.Size = new System.Drawing.Size( 716, 752 );
            this.processingPanelsTabControl.ResumeLayout( false );
            this.thresholderTabPage.ResumeLayout( false );
            this.thresholderPanel.ResumeLayout( false );
            this.thresholderPanel.PerformLayout();
            this.smootherTabPage.ResumeLayout( false );
            this.smootherPanel.ResumeLayout( false );
            this.smootherPanel.PerformLayout();
            this.smootherWhittakerParameters.ResumeLayout( false );
            this.smootherWhittakerParameters.PerformLayout();
            this.smootherSavitzkyGolayParameters.ResumeLayout( false );
            this.smootherSavitzkyGolayParameters.PerformLayout();
            ( (System.ComponentModel.ISupportInitialize) ( this.smootherSavitzkyGolayPolynomialOrderTrackBar ) ).EndInit();
            ( (System.ComponentModel.ISupportInitialize) ( this.smootherSavitzkyGolayWindowSizeTrackBar ) ).EndInit();
            this.peakPickerTabPage.ResumeLayout( false );
            this.peakPickerPanel.ResumeLayout( false );
            this.peakPickerPanel.PerformLayout();
            this.peakPickerLocalMaximumParameters.ResumeLayout( false );
            this.peakPickerLocalMaximumParameters.PerformLayout();
            ( (System.ComponentModel.ISupportInitialize) ( this.peakPickerLocalMaximumWindowSizeTrackBar ) ).EndInit();
            ( (System.ComponentModel.ISupportInitialize) ( this.trackBar1 ) ).EndInit();
            ( (System.ComponentModel.ISupportInitialize) ( this.trackBar2 ) ).EndInit();
            this.chargeStateCalculatorTabPage.ResumeLayout( false );
            this.groupBox1.ResumeLayout( false );
            this.groupBox1.PerformLayout();
            ( (System.ComponentModel.ISupportInitialize) ( this.trackBar3 ) ).EndInit();
            this.chargeStateCalculatorPanel.ResumeLayout( false );
            this.chargeStateCalculatorPanel.PerformLayout();
            ( (System.ComponentModel.ISupportInitialize) ( this.chargeStateCalculatorMaxChargeUpDown ) ).EndInit();
            ( (System.ComponentModel.ISupportInitialize) ( this.chargeStateCalculatorMinChargeUpDown ) ).EndInit();
            this.ResumeLayout( false );

        }

        #endregion

        private System.Windows.Forms.TabControl processingPanelsTabControl;
        private System.Windows.Forms.TabPage thresholderTabPage;
        private System.Windows.Forms.TabPage smootherTabPage;
        private System.Windows.Forms.Label label1;
        public System.Windows.Forms.Panel thresholderPanel;
        public System.Windows.Forms.ComboBox thresholderTypeComboBox;
        public System.Windows.Forms.ComboBox thresholderOrientationComboBox;
        public System.Windows.Forms.TextBox thresholderValueTextBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        public System.Windows.Forms.Panel smootherPanel;
        private System.Windows.Forms.TabPage peakPickerTabPage;
        public System.Windows.Forms.Panel peakPickerPanel;
        public System.Windows.Forms.CheckBox peakPickerPreferVendorCentroidingCheckbox;
        private System.Windows.Forms.Label label4;
        public System.Windows.Forms.ComboBox peakPickerAlgorithmComboBox;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label smootherSavitzkyGolayWindowSizeLabel;
        private System.Windows.Forms.Label smootherSavitzkyGolayPolynomialOrderLabel;
        private System.Windows.Forms.Label label8;
        public System.Windows.Forms.GroupBox smootherSavitzkyGolayParameters;
        public System.Windows.Forms.GroupBox smootherParametersGroupBox;
        public System.Windows.Forms.ComboBox smootherAlgorithmComboBox;
        public System.Windows.Forms.GroupBox smootherWhittakerParameters;
        public System.Windows.Forms.TextBox smootherWhittakerLambdaTextBox;
        public System.Windows.Forms.TrackBar smootherSavitzkyGolayWindowSizeTrackBar;
        public System.Windows.Forms.TrackBar smootherSavitzkyGolayPolynomialOrderTrackBar;
        public System.Windows.Forms.GroupBox peakPickerLocalMaximumParameters;
        public System.Windows.Forms.TrackBar peakPickerLocalMaximumWindowSizeTrackBar;
        private System.Windows.Forms.Label peakPickerLocalMaximumWindowSizeLabel;
        public System.Windows.Forms.GroupBox peakPickerParametersGroupBox;
        public System.Windows.Forms.TrackBar trackBar1;
        public System.Windows.Forms.TrackBar trackBar2;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TabPage chargeStateCalculatorTabPage;
        public System.Windows.Forms.Panel chargeStateCalculatorPanel;
        public System.Windows.Forms.CheckBox chargeStateCalculatorOverrideExistingCheckBox;
        public System.Windows.Forms.GroupBox groupBox1;
        public System.Windows.Forms.TrackBar trackBar3;
        private System.Windows.Forms.Label label9;
        public System.Windows.Forms.GroupBox groupBox2;
        public System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.Label label10;
        public System.Windows.Forms.ComboBox comboBox1;
        public System.Windows.Forms.NumericUpDown chargeStateCalculatorMaxChargeUpDown;
        public System.Windows.Forms.NumericUpDown chargeStateCalculatorMinChargeUpDown;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label13;
        public System.Windows.Forms.TextBox chargeStateCalculatorIntensityFraction;
    }
}
