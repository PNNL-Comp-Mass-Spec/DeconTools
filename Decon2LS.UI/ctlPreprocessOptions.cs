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
    /// Summary description for ctlMiscellaneousOptions.
    /// </summary>
    public class ctlPreprocessOptions : System.Windows.Forms.UserControl
    {
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.TextBox mtxtMinX;
        private System.Windows.Forms.TextBox mtxtMaxX;
        private System.Windows.Forms.ComboBox mcmbApodizationType;
        private System.Windows.Forms.Label mlabelMaxX;
        private System.Windows.Forms.Label mlabelMinX;
        private System.Windows.Forms.Panel mpanelApodizationType;
        private System.Windows.Forms.Panel panel5;
        private System.Windows.Forms.ComboBox mcmbNumZeroFills;
        private System.Windows.Forms.Panel panelZeroFill;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox mtxtTrianglePercent;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Panel panelCalibration;
        private System.Windows.Forms.Label labelCalibrationType;
        private System.Windows.Forms.ComboBox mcmbCalibrationType;
        private System.Windows.Forms.Panel mpanelCalibrationType;
        private System.Windows.Forms.Panel mpanelCalibrationConstants;
        private System.Windows.Forms.TextBox mtxtC;
        private System.Windows.Forms.TextBox mtxtB;
        private System.Windows.Forms.TextBox mtxtA;
        private System.Windows.Forms.Label mlabelC;
        private System.Windows.Forms.Label mlabelB;
        private System.Windows.Forms.Label mlabelA; 
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        public ctlPreprocessOptions()
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();

            // TODO: Add any initialization after the InitializeComponent call
            foreach (DeconToolsV2.Readers.ApodizationType type in Enum.GetValues(typeof(DeconToolsV2.Readers.ApodizationType)))
            {
                mcmbApodizationType.Items.Add(type.ToString()) ; 
            }
            foreach (DeconToolsV2.Readers.CalibrationType type in Enum.GetValues(typeof(DeconToolsV2.Readers.CalibrationType)))
            {
                mcmbCalibrationType.Items.Add(type.ToString()) ; 
            }
            CalibrationType = DeconToolsV2.Readers.CalibrationType.UNDEFINED ; 
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
            this.panel2 = new System.Windows.Forms.Panel();
            this.mpanelApodizationType = new System.Windows.Forms.Panel();
            this.mcmbApodizationType = new System.Windows.Forms.ComboBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.mtxtTrianglePercent = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.mtxtMaxX = new System.Windows.Forms.TextBox();
            this.mlabelMaxX = new System.Windows.Forms.Label();
            this.mtxtMinX = new System.Windows.Forms.TextBox();
            this.mlabelMinX = new System.Windows.Forms.Label();
            this.panelZeroFill = new System.Windows.Forms.Panel();
            this.panel5 = new System.Windows.Forms.Panel();
            this.mcmbNumZeroFills = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.panelCalibration = new System.Windows.Forms.Panel();
            this.mpanelCalibrationConstants = new System.Windows.Forms.Panel();
            this.mtxtC = new System.Windows.Forms.TextBox();
            this.mlabelC = new System.Windows.Forms.Label();
            this.mtxtB = new System.Windows.Forms.TextBox();
            this.mlabelB = new System.Windows.Forms.Label();
            this.mtxtA = new System.Windows.Forms.TextBox();
            this.mlabelA = new System.Windows.Forms.Label();
            this.mpanelCalibrationType = new System.Windows.Forms.Panel();
            this.mcmbCalibrationType = new System.Windows.Forms.ComboBox();
            this.labelCalibrationType = new System.Windows.Forms.Label();
            this.panel2.SuspendLayout();
            this.mpanelApodizationType.SuspendLayout();
            this.panel1.SuspendLayout();
            this.panelZeroFill.SuspendLayout();
            this.panel5.SuspendLayout();
            this.panelCalibration.SuspendLayout();
            this.mpanelCalibrationConstants.SuspendLayout();
            this.mpanelCalibrationType.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel2
            // 
            this.panel2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel2.Controls.Add(this.mpanelApodizationType);
            this.panel2.Controls.Add(this.panel1);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel2.Location = new System.Drawing.Point(0, 0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(464, 64);
            this.panel2.TabIndex = 1;
            // 
            // mpanelApodizationType
            // 
            this.mpanelApodizationType.Controls.Add(this.mcmbApodizationType);
            this.mpanelApodizationType.Dock = System.Windows.Forms.DockStyle.Top;
            this.mpanelApodizationType.Location = new System.Drawing.Point(0, 32);
            this.mpanelApodizationType.Name = "mpanelApodizationType";
            this.mpanelApodizationType.Padding = new System.Windows.Forms.Padding(5);
            this.mpanelApodizationType.Size = new System.Drawing.Size(462, 40);
            this.mpanelApodizationType.TabIndex = 2;
            // 
            // mcmbApodizationType
            // 
            this.mcmbApodizationType.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mcmbApodizationType.Location = new System.Drawing.Point(5, 5);
            this.mcmbApodizationType.Name = "mcmbApodizationType";
            this.mcmbApodizationType.Size = new System.Drawing.Size(452, 21);
            this.mcmbApodizationType.TabIndex = 0;
            this.mcmbApodizationType.Text = "Apodization Type";
            this.mcmbApodizationType.SelectedIndexChanged += new System.EventHandler(this.mcmbApodizationType_SelectedIndexChanged);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.mtxtTrianglePercent);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.mtxtMaxX);
            this.panel1.Controls.Add(this.mlabelMaxX);
            this.panel1.Controls.Add(this.mtxtMinX);
            this.panel1.Controls.Add(this.mlabelMinX);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Padding = new System.Windows.Forms.Padding(5);
            this.panel1.Size = new System.Drawing.Size(462, 32);
            this.panel1.TabIndex = 1;
            // 
            // mtxtTrianglePercent
            // 
            this.mtxtTrianglePercent.Dock = System.Windows.Forms.DockStyle.Left;
            this.mtxtTrianglePercent.Location = new System.Drawing.Point(376, 5);
            this.mtxtTrianglePercent.Name = "mtxtTrianglePercent";
            this.mtxtTrianglePercent.Size = new System.Drawing.Size(64, 20);
            this.mtxtTrianglePercent.TabIndex = 5;
            this.mtxtTrianglePercent.Text = "0";
            // 
            // label2
            // 
            this.label2.Dock = System.Windows.Forms.DockStyle.Left;
            this.label2.Location = new System.Drawing.Point(213, 5);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(163, 22);
            this.label2.TabIndex = 6;
            this.label2.Text = "Apodization Apex Position:";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // mtxtMaxX
            // 
            this.mtxtMaxX.Dock = System.Windows.Forms.DockStyle.Left;
            this.mtxtMaxX.Location = new System.Drawing.Point(149, 5);
            this.mtxtMaxX.Name = "mtxtMaxX";
            this.mtxtMaxX.Size = new System.Drawing.Size(64, 20);
            this.mtxtMaxX.TabIndex = 4;
            this.mtxtMaxX.Text = "0.9437166";
            // 
            // mlabelMaxX
            // 
            this.mlabelMaxX.Dock = System.Windows.Forms.DockStyle.Left;
            this.mlabelMaxX.Location = new System.Drawing.Point(109, 5);
            this.mlabelMaxX.Name = "mlabelMaxX";
            this.mlabelMaxX.Size = new System.Drawing.Size(40, 22);
            this.mlabelMaxX.TabIndex = 3;
            this.mlabelMaxX.Text = "Max X:";
            this.mlabelMaxX.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // mtxtMinX
            // 
            this.mtxtMinX.Dock = System.Windows.Forms.DockStyle.Left;
            this.mtxtMinX.Location = new System.Drawing.Point(45, 5);
            this.mtxtMinX.Name = "mtxtMinX";
            this.mtxtMinX.Size = new System.Drawing.Size(64, 20);
            this.mtxtMinX.TabIndex = 2;
            this.mtxtMinX.Text = "0";
            // 
            // mlabelMinX
            // 
            this.mlabelMinX.Dock = System.Windows.Forms.DockStyle.Left;
            this.mlabelMinX.Location = new System.Drawing.Point(5, 5);
            this.mlabelMinX.Name = "mlabelMinX";
            this.mlabelMinX.Size = new System.Drawing.Size(40, 22);
            this.mlabelMinX.TabIndex = 1;
            this.mlabelMinX.Text = "Min X:";
            this.mlabelMinX.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // panelZeroFill
            // 
            this.panelZeroFill.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelZeroFill.Controls.Add(this.panel5);
            this.panelZeroFill.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelZeroFill.Location = new System.Drawing.Point(0, 64);
            this.panelZeroFill.Name = "panelZeroFill";
            this.panelZeroFill.Size = new System.Drawing.Size(464, 32);
            this.panelZeroFill.TabIndex = 2;
            // 
            // panel5
            // 
            this.panel5.Controls.Add(this.mcmbNumZeroFills);
            this.panel5.Controls.Add(this.label1);
            this.panel5.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel5.Location = new System.Drawing.Point(0, 0);
            this.panel5.Name = "panel5";
            this.panel5.Padding = new System.Windows.Forms.Padding(5);
            this.panel5.Size = new System.Drawing.Size(462, 30);
            this.panel5.TabIndex = 1;
            // 
            // mcmbNumZeroFills
            // 
            this.mcmbNumZeroFills.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mcmbNumZeroFills.Items.AddRange(new object[] {
            "0",
            "1",
            "2",
            "3"});
            this.mcmbNumZeroFills.Location = new System.Drawing.Point(96, 5);
            this.mcmbNumZeroFills.Name = "mcmbNumZeroFills";
            this.mcmbNumZeroFills.Size = new System.Drawing.Size(361, 21);
            this.mcmbNumZeroFills.TabIndex = 1;
            this.mcmbNumZeroFills.Text = "Num Zero Fills";
            // 
            // label1
            // 
            this.label1.Dock = System.Windows.Forms.DockStyle.Left;
            this.label1.Location = new System.Drawing.Point(5, 5);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(91, 20);
            this.label1.TabIndex = 2;
            this.label1.Text = "Num Zero Fills:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // panelCalibration
            // 
            this.panelCalibration.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelCalibration.Controls.Add(this.mpanelCalibrationConstants);
            this.panelCalibration.Controls.Add(this.mpanelCalibrationType);
            this.panelCalibration.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelCalibration.Location = new System.Drawing.Point(0, 96);
            this.panelCalibration.Name = "panelCalibration";
            this.panelCalibration.Size = new System.Drawing.Size(464, 64);
            this.panelCalibration.TabIndex = 3;
            // 
            // mpanelCalibrationConstants
            // 
            this.mpanelCalibrationConstants.Controls.Add(this.mtxtC);
            this.mpanelCalibrationConstants.Controls.Add(this.mlabelC);
            this.mpanelCalibrationConstants.Controls.Add(this.mtxtB);
            this.mpanelCalibrationConstants.Controls.Add(this.mlabelB);
            this.mpanelCalibrationConstants.Controls.Add(this.mtxtA);
            this.mpanelCalibrationConstants.Controls.Add(this.mlabelA);
            this.mpanelCalibrationConstants.Dock = System.Windows.Forms.DockStyle.Top;
            this.mpanelCalibrationConstants.Location = new System.Drawing.Point(0, 30);
            this.mpanelCalibrationConstants.Name = "mpanelCalibrationConstants";
            this.mpanelCalibrationConstants.Padding = new System.Windows.Forms.Padding(5);
            this.mpanelCalibrationConstants.Size = new System.Drawing.Size(462, 32);
            this.mpanelCalibrationConstants.TabIndex = 2;
            // 
            // mtxtC
            // 
            this.mtxtC.Dock = System.Windows.Forms.DockStyle.Left;
            this.mtxtC.Location = new System.Drawing.Point(280, 5);
            this.mtxtC.Name = "mtxtC";
            this.mtxtC.Size = new System.Drawing.Size(100, 20);
            this.mtxtC.TabIndex = 5;
            this.mtxtC.Text = "0";
            // 
            // mlabelC
            // 
            this.mlabelC.Dock = System.Windows.Forms.DockStyle.Left;
            this.mlabelC.Location = new System.Drawing.Point(255, 5);
            this.mlabelC.Name = "mlabelC";
            this.mlabelC.Size = new System.Drawing.Size(25, 22);
            this.mlabelC.TabIndex = 6;
            this.mlabelC.Text = "C:";
            this.mlabelC.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // mtxtB
            // 
            this.mtxtB.Dock = System.Windows.Forms.DockStyle.Left;
            this.mtxtB.Location = new System.Drawing.Point(155, 5);
            this.mtxtB.Name = "mtxtB";
            this.mtxtB.Size = new System.Drawing.Size(100, 20);
            this.mtxtB.TabIndex = 4;
            this.mtxtB.Text = "0";
            // 
            // mlabelB
            // 
            this.mlabelB.Dock = System.Windows.Forms.DockStyle.Left;
            this.mlabelB.Location = new System.Drawing.Point(130, 5);
            this.mlabelB.Name = "mlabelB";
            this.mlabelB.Size = new System.Drawing.Size(25, 22);
            this.mlabelB.TabIndex = 3;
            this.mlabelB.Text = "B:";
            this.mlabelB.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // mtxtA
            // 
            this.mtxtA.Dock = System.Windows.Forms.DockStyle.Left;
            this.mtxtA.Location = new System.Drawing.Point(30, 5);
            this.mtxtA.Name = "mtxtA";
            this.mtxtA.Size = new System.Drawing.Size(100, 20);
            this.mtxtA.TabIndex = 2;
            this.mtxtA.Text = "0";
            // 
            // mlabelA
            // 
            this.mlabelA.Dock = System.Windows.Forms.DockStyle.Left;
            this.mlabelA.Location = new System.Drawing.Point(5, 5);
            this.mlabelA.Name = "mlabelA";
            this.mlabelA.Size = new System.Drawing.Size(25, 22);
            this.mlabelA.TabIndex = 1;
            this.mlabelA.Text = "A:";
            this.mlabelA.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // mpanelCalibrationType
            // 
            this.mpanelCalibrationType.Controls.Add(this.mcmbCalibrationType);
            this.mpanelCalibrationType.Controls.Add(this.labelCalibrationType);
            this.mpanelCalibrationType.Dock = System.Windows.Forms.DockStyle.Top;
            this.mpanelCalibrationType.Location = new System.Drawing.Point(0, 0);
            this.mpanelCalibrationType.Name = "mpanelCalibrationType";
            this.mpanelCalibrationType.Padding = new System.Windows.Forms.Padding(5);
            this.mpanelCalibrationType.Size = new System.Drawing.Size(462, 30);
            this.mpanelCalibrationType.TabIndex = 1;
            // 
            // mcmbCalibrationType
            // 
            this.mcmbCalibrationType.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mcmbCalibrationType.Location = new System.Drawing.Point(96, 5);
            this.mcmbCalibrationType.Name = "mcmbCalibrationType";
            this.mcmbCalibrationType.Size = new System.Drawing.Size(361, 21);
            this.mcmbCalibrationType.TabIndex = 1;
            this.mcmbCalibrationType.Text = "Calibration Type";
            this.mcmbCalibrationType.SelectedIndexChanged += new System.EventHandler(this.mcmbCalibrationType_SelectedIndexChanged);
            // 
            // labelCalibrationType
            // 
            this.labelCalibrationType.Dock = System.Windows.Forms.DockStyle.Left;
            this.labelCalibrationType.Location = new System.Drawing.Point(5, 5);
            this.labelCalibrationType.Name = "labelCalibrationType";
            this.labelCalibrationType.Size = new System.Drawing.Size(91, 20);
            this.labelCalibrationType.TabIndex = 2;
            this.labelCalibrationType.Text = "Calibration Type:";
            this.labelCalibrationType.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // ctlPreprocessOptions
            // 
            this.Controls.Add(this.panelCalibration);
            this.Controls.Add(this.panelZeroFill);
            this.Controls.Add(this.panel2);
            this.Name = "ctlPreprocessOptions";
            this.Size = new System.Drawing.Size(464, 240);
            this.panel2.ResumeLayout(false);
            this.mpanelApodizationType.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panelZeroFill.ResumeLayout(false);
            this.panel5.ResumeLayout(false);
            this.panelCalibration.ResumeLayout(false);
            this.mpanelCalibrationConstants.ResumeLayout(false);
            this.mpanelCalibrationConstants.PerformLayout();
            this.mpanelCalibrationType.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        #endregion

        private void SetApodizationControlsEnabled(bool enabled) 
        {
            mtxtMaxX.Enabled = enabled ; 
            mtxtMinX.Enabled = enabled ; 
            mtxtTrianglePercent.Enabled = enabled ; 
        }

        private void SetCalibrationControlsEnabled(bool enabled) 
        {
            mpanelCalibrationConstants.Enabled = enabled ; 
            mlabelA.Enabled = enabled ; 
            mtxtA.Enabled = enabled ; 
            mlabelB.Enabled = enabled ; 
            mtxtB.Enabled = enabled ; 
            mlabelC.Enabled = enabled ; 
            mtxtC.Enabled = enabled ; 
        }

        private void mcmbApodizationType_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            if (mcmbApodizationType.SelectedIndex == -1 || mcmbApodizationType.Items[mcmbApodizationType.SelectedIndex].ToString() == DeconToolsV2.Readers.ApodizationType.NOAPODIZATION.ToString())
            {
                SetApodizationControlsEnabled(false) ; 
            }
            else
            {
                SetApodizationControlsEnabled(true) ; 
            }		
        }

        private void mcmbCalibrationType_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            if (mcmbCalibrationType.SelectedIndex == -1 || mcmbCalibrationType.Items[mcmbCalibrationType.SelectedIndex].ToString() == DeconToolsV2.Readers.CalibrationType.UNDEFINED.ToString())
            {
                SetCalibrationControlsEnabled(false) ; 
            }
            else
            {
                SetCalibrationControlsEnabled(true) ; 
            }				
        }


        public DeconToolsV2.Readers.ApodizationType ApodizationType
        {
            get
            {
                if (mcmbApodizationType.SelectedIndex == -1)
                    return DeconToolsV2.Readers.ApodizationType.NOAPODIZATION ; 

                var selectedType = mcmbApodizationType.Items[mcmbApodizationType.SelectedIndex].ToString() ; 
                foreach (DeconToolsV2.Readers.ApodizationType type in Enum.GetValues(typeof(DeconToolsV2.Readers.ApodizationType)))
                {
                    if (type.ToString() == selectedType)
                        return type ;
                }
                return DeconToolsV2.Readers.ApodizationType.NOAPODIZATION ; 
            }
            set
            {
                mcmbApodizationType.SelectedIndex = -1 ;
                var selectedType = value.ToString() ; 
                for (var i = 0 ; i < mcmbApodizationType.Items.Count ; i++)
                {
                    if (mcmbApodizationType.Items[i].ToString() == selectedType)
                    {
                        mcmbApodizationType.SelectedIndex = i ; 
                        return ; 
                    }
                }
                if (mcmbApodizationType.SelectedIndex == -1 || mcmbApodizationType.Items[mcmbApodizationType.SelectedIndex].ToString() == DeconToolsV2.Readers.ApodizationType.NOAPODIZATION.ToString())
                {
                    SetApodizationControlsEnabled(false) ; 
                }
                else
                {
                    SetApodizationControlsEnabled(true) ; 
                }
            }
        }

        public int ApodizationPercent
        {
            set
            {
                mtxtTrianglePercent.Text = Convert.ToString(value) ; 
            }
            get
            {
                return Convert.ToInt32(mtxtTrianglePercent.Text) ; 
            }
        }

        public double ApodizationMinX
        {
            set
            {
                mtxtMinX.Text = Convert.ToString(value) ; 
            }
            get
            {
                return Convert.ToDouble(mtxtMinX.Text) ; 
            }
        }
        public double ApodizationMaxX
        {
            set
            {
                mtxtMaxX.Text = Convert.ToString(value) ; 
            }
            get
            {
                return Convert.ToDouble(mtxtMaxX.Text) ; 
            }
        }

        public short NumZeroFills
        {
            set
            {
                mcmbNumZeroFills.SelectedIndex = Convert.ToInt32(value) ; 
            }
            get
            {
                return Convert.ToInt16(mcmbNumZeroFills.SelectedIndex) ; 
            }
        }

        public DeconToolsV2.Readers.CalibrationType CalibrationType
        {
            get
            {
                if (mcmbCalibrationType.SelectedIndex == -1)
                    return DeconToolsV2.Readers.CalibrationType.UNDEFINED ; 

                var selectedType = mcmbCalibrationType.Items[mcmbCalibrationType.SelectedIndex].ToString() ; 
                return (DeconToolsV2.Readers.CalibrationType) Enum.Parse(typeof(DeconToolsV2.Readers.CalibrationType), selectedType)  ;
            }
            set
            {
                var selectedType = value.ToString() ; 
                for (var i = 0 ; i < mcmbCalibrationType.Items.Count ; i++)
                {
                    if (mcmbCalibrationType.Items[i].ToString() == selectedType)
                    {
                        mcmbCalibrationType.SelectedIndex = i ; 
                        return ; 
                    }
                }
            }
        }

        public double CalibrationConstA
        {
            get
            {
                return Convert.ToDouble(mtxtA.Text) ; 
            }
            set
            {
                mtxtA.Text = Convert.ToString(value) ;
            }
        }
        public double CalibrationConstB
        {
            get
            {
                return Convert.ToDouble(mtxtB.Text) ; 
            }
            set
            {
                mtxtB.Text = Convert.ToString(value) ;
            }
        }
        public double CalibrationConstC
        {
            get
            {
                return Convert.ToDouble(mtxtC.Text) ; 
            }
            set
            {
                mtxtC.Text = Convert.ToString(value) ;
            }
        }

    }
}
