// Written by Navdeep Jaitly for the Department of Energy (PNNL, Richland, WA)
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
    /// Summary description for frmFitFormulaInput.
    /// </summary>
    public class frmFitFormulaInput : System.Windows.Forms.Form
    {
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox mtxtFormula;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox mtxtCharge;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Button mbtnOK;
        private System.Windows.Forms.Button mbtnCancel;
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        public frmFitFormulaInput()
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
            this.panel1 = new System.Windows.Forms.Panel();
            this.label1 = new System.Windows.Forms.Label();
            this.mtxtFormula = new System.Windows.Forms.TextBox();
            this.panel2 = new System.Windows.Forms.Panel();
            this.label2 = new System.Windows.Forms.Label();
            this.mtxtCharge = new System.Windows.Forms.TextBox();
            this.panel3 = new System.Windows.Forms.Panel();
            this.mbtnOK = new System.Windows.Forms.Button();
            this.mbtnCancel = new System.Windows.Forms.Button();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel3.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.mtxtFormula);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(296, 24);
            this.panel1.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.Dock = System.Windows.Forms.DockStyle.Left;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
            this.label1.Location = new System.Drawing.Point(0, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(128, 24);
            this.label1.TabIndex = 0;
            this.label1.Text = "Chemical Formula:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // mtxtFormula
            // 
            this.mtxtFormula.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mtxtFormula.Location = new System.Drawing.Point(128, 0);
            this.mtxtFormula.Name = "mtxtFormula";
            this.mtxtFormula.Size = new System.Drawing.Size(168, 20);
            this.mtxtFormula.TabIndex = 1;
            this.mtxtFormula.Text = "";
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.mtxtCharge);
            this.panel2.Controls.Add(this.label2);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel2.Location = new System.Drawing.Point(0, 24);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(296, 24);
            this.panel2.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.Dock = System.Windows.Forms.DockStyle.Left;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
            this.label2.Location = new System.Drawing.Point(0, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(128, 24);
            this.label2.TabIndex = 1;
            this.label2.Text = "Charge:";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // mtxtCharge
            // 
            this.mtxtCharge.Dock = System.Windows.Forms.DockStyle.Left;
            this.mtxtCharge.Location = new System.Drawing.Point(128, 0);
            this.mtxtCharge.Name = "mtxtCharge";
            this.mtxtCharge.Size = new System.Drawing.Size(32, 20);
            this.mtxtCharge.TabIndex = 2;
            this.mtxtCharge.Text = "";
            // 
            // panel3
            // 
            this.panel3.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel3.Controls.Add(this.mbtnCancel);
            this.panel3.Controls.Add(this.mbtnOK);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel3.Location = new System.Drawing.Point(0, 70);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(296, 40);
            this.panel3.TabIndex = 2;
            // 
            // mbtnOK
            // 
            this.mbtnOK.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
            this.mbtnOK.Location = new System.Drawing.Point(61, 10);
            this.mbtnOK.Name = "mbtnOK";
            this.mbtnOK.Size = new System.Drawing.Size(64, 24);
            this.mbtnOK.TabIndex = 0;
            this.mbtnOK.Text = "OK";
            this.mbtnOK.Click += new System.EventHandler(this.mbtnOK_Click);
            // 
            // mbtnCancel
            // 
            this.mbtnCancel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
            this.mbtnCancel.Location = new System.Drawing.Point(149, 10);
            this.mbtnCancel.Name = "mbtnCancel";
            this.mbtnCancel.Size = new System.Drawing.Size(64, 24);
            this.mbtnCancel.TabIndex = 1;
            this.mbtnCancel.Text = "Cancel";
            this.mbtnCancel.Click += new System.EventHandler(this.mbtnCancel_Click);
            // 
            // frmFitFormulaInput
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(296, 110);
            this.Controls.Add(this.panel3);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.Name = "frmFitFormulaInput";
            this.Text = "frmFitFormulaInput";
            this.panel1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        #endregion

        public string Formula
        {
            get
            {
                return mtxtFormula.Text ; 
            }
        }
        public short Charge
        {
            get
            {
                try
                {
                    return Convert.ToInt16(mtxtCharge.Text) ; 
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message + ex.StackTrace) ; 
                    return 1 ; 
                }
            }
        }

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
    }
}
