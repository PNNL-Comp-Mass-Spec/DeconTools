// Written by Navdeep Jaitly
// for the Department of Energy (PNNL, Richland, WA)
// Copyright 2006, Battelle Memorial Institute
// E-mail: navdeep.jaitly@pnl.gov
// Website: https://omics.pnl.gov/software or http://panomics.pnnl.gov
// -------------------------------------------------------------------------------
// 
// Licensed under the Apache License, Version 2.0; you may not use this file except
// in compliance with the License.  You may obtain a copy of the License at 
// http://www.apache.org/licenses/LICENSE-2.0

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace Decon2LS
{
    /// <summary>
    /// Summary description for frmFocusParameters.
    /// </summary>
    public class frmFocusParameters : System.Windows.Forms.Form
    {
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox mtxtFocusMZ;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button mbtnOK;
        private System.Windows.Forms.Button mbtnCancel;
        private System.Windows.Forms.TextBox mtxtFocusScan;
        private System.Windows.Forms.TextBox mtxtMZTolerance;
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        public frmFocusParameters()
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            //
            // TODO: Add any constructor code after InitializeComponent call
            //
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

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.mtxtFocusMZ = new System.Windows.Forms.TextBox();
            this.mtxtFocusScan = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.mtxtMZTolerance = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.mbtnOK = new System.Windows.Forms.Button();
            this.mbtnCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
            this.label1.Location = new System.Drawing.Point(96, 8);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(64, 24);
            this.label1.TabIndex = 0;
            this.label1.Text = "MZ:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // mtxtFocusMZ
            // 
            this.mtxtFocusMZ.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
            this.mtxtFocusMZ.Location = new System.Drawing.Point(176, 7);
            this.mtxtFocusMZ.Name = "mtxtFocusMZ";
            this.mtxtFocusMZ.Size = new System.Drawing.Size(104, 26);
            this.mtxtFocusMZ.TabIndex = 1;
            this.mtxtFocusMZ.Text = "450.1";
            // 
            // mtxtFocusScan
            // 
            this.mtxtFocusScan.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
            this.mtxtFocusScan.Location = new System.Drawing.Point(176, 43);
            this.mtxtFocusScan.Name = "mtxtFocusScan";
            this.mtxtFocusScan.Size = new System.Drawing.Size(104, 26);
            this.mtxtFocusScan.TabIndex = 3;
            this.mtxtFocusScan.Text = "100";
            // 
            // label2
            // 
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
            this.label2.Location = new System.Drawing.Point(96, 44);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(64, 24);
            this.label2.TabIndex = 2;
            this.label2.Text = "Scan #:";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // mtxtMZTolerance
            // 
            this.mtxtMZTolerance.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
            this.mtxtMZTolerance.Location = new System.Drawing.Point(176, 81);
            this.mtxtMZTolerance.Name = "mtxtMZTolerance";
            this.mtxtMZTolerance.Size = new System.Drawing.Size(104, 26);
            this.mtxtMZTolerance.TabIndex = 5;
            this.mtxtMZTolerance.Text = "0.1";
            // 
            // label3
            // 
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
            this.label3.Location = new System.Drawing.Point(10, 81);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(158, 24);
            this.label3.TabIndex = 4;
            this.label3.Text = "m/z Tolerance (Da):";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // mbtnOK
            // 
            this.mbtnOK.Location = new System.Drawing.Point(29, 121);
            this.mbtnOK.Name = "mbtnOK";
            this.mbtnOK.Size = new System.Drawing.Size(104, 29);
            this.mbtnOK.TabIndex = 6;
            this.mbtnOK.Text = "OK";
            this.mbtnOK.Click += new System.EventHandler(this.mbtnOK_Click);
            // 
            // mbtnCancel
            // 
            this.mbtnCancel.Location = new System.Drawing.Point(161, 120);
            this.mbtnCancel.Name = "mbtnCancel";
            this.mbtnCancel.Size = new System.Drawing.Size(104, 29);
            this.mbtnCancel.TabIndex = 7;
            this.mbtnCancel.Text = "Cancel";
            this.mbtnCancel.Click += new System.EventHandler(this.mbtnCancel_Click);
            // 
            // frmFocusParameters
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(288, 158);
            this.Controls.Add(this.mbtnCancel);
            this.Controls.Add(this.mbtnOK);
            this.Controls.Add(this.mtxtMZTolerance);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.mtxtFocusScan);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.mtxtFocusMZ);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmFocusParameters";
            this.ShowInTaskbar = false;
            this.Text = "frmFocusParameters";
            this.ResumeLayout(false);

        }
        #endregion

        private void mbtnOK_Click(object sender, System.EventArgs e)
        {
            DialogResult = DialogResult.OK ; 
            this.Hide() ; 
        }

        private void mbtnCancel_Click(object sender, System.EventArgs e)
        {
            DialogResult = DialogResult.Cancel ; 
            this.Hide() ; 
        }

        public float FocusMZ
        {
            get
            {
                return Convert.ToSingle(mtxtFocusMZ.Text) ;
            }
            set
            {
                mtxtFocusMZ.Text = Convert.ToString(value) ; 
            }
        }

        public float FocusMZTolerance
        {
            get
            {
                return Convert.ToSingle(mtxtMZTolerance.Text) ;
            }
            set
            {
                mtxtMZTolerance.Text = Convert.ToString(value) ; 
            }
        }

        public int FocusScan
        {
            get
            {
                return Convert.ToInt32(mtxtFocusScan.Text) ;
            }
            set
            {
                mtxtFocusScan.Text = Convert.ToString(value) ; 
            }
        }
    }
}
