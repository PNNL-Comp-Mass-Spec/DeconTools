// Written by Navdeep Jaitly for the Department of Energy (PNNL, Richland, WA)
// Copyright 2006, Battelle Memorial Institute
// E-mail: navdeep.jaitly@pnl.gov
// Website: http://omics.pnl.gov/software or http://panomics.pnnl.gov
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
    /// Summary description for frmSavGolOptions.
    /// </summary>
    public class frmSavGolOptions : System.Windows.Forms.Form
    {
        private short MaxOrder = 10 ; 
        private short MaxPts = 50 ; 
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button mbtnCancel;
        private System.Windows.Forms.Button mbtnOK;
        private System.Windows.Forms.TextBox mtxtLeft;
        private System.Windows.Forms.TextBox mtxtRight;
        private System.Windows.Forms.TextBox mtxtOrder;
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        public frmSavGolOptions()
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
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.mbtnCancel = new System.Windows.Forms.Button();
            this.mbtnOK = new System.Windows.Forms.Button();
            this.mtxtLeft = new System.Windows.Forms.TextBox();
            this.mtxtRight = new System.Windows.Forms.TextBox();
            this.mtxtOrder = new System.Windows.Forms.TextBox();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(16, 28);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(104, 16);
            this.label1.TabIndex = 0;
            this.label1.Text = "# of points before:";
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(16, 56);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(104, 16);
            this.label2.TabIndex = 1;
            this.label2.Text = "# of points after:";
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(16, 84);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(104, 16);
            this.label3.TabIndex = 2;
            this.label3.Text = "Smoothing Order:";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.mtxtOrder);
            this.groupBox1.Controls.Add(this.mtxtRight);
            this.groupBox1.Controls.Add(this.mtxtLeft);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(8, 8);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(232, 112);
            this.groupBox1.TabIndex = 3;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Options:";
            // 
            // mbtnCancel
            // 
            this.mbtnCancel.Location = new System.Drawing.Point(123, 128);
            this.mbtnCancel.Name = "mbtnCancel";
            this.mbtnCancel.Size = new System.Drawing.Size(69, 25);
            this.mbtnCancel.TabIndex = 4;
            this.mbtnCancel.Text = "Cancel";
            this.mbtnCancel.Click += new System.EventHandler(this.mbtnCancel_Click);
            // 
            // mbtnOK
            // 
            this.mbtnOK.Location = new System.Drawing.Point(32, 128);
            this.mbtnOK.Name = "mbtnOK";
            this.mbtnOK.Size = new System.Drawing.Size(69, 25);
            this.mbtnOK.TabIndex = 5;
            this.mbtnOK.Text = "OK";
            this.mbtnOK.Click += new System.EventHandler(this.mbtnOK_Click);
            // 
            // mtxtLeft
            // 
            this.mtxtLeft.Location = new System.Drawing.Point(137, 21);
            this.mtxtLeft.Name = "mtxtLeft";
            this.mtxtLeft.Size = new System.Drawing.Size(48, 20);
            this.mtxtLeft.TabIndex = 3;
            this.mtxtLeft.Text = "2";
            // 
            // mtxtRight
            // 
            this.mtxtRight.Location = new System.Drawing.Point(136, 51);
            this.mtxtRight.Name = "mtxtRight";
            this.mtxtRight.Size = new System.Drawing.Size(48, 20);
            this.mtxtRight.TabIndex = 4;
            this.mtxtRight.Text = "2";
            // 
            // mtxtOrder
            // 
            this.mtxtOrder.Location = new System.Drawing.Point(136, 80);
            this.mtxtOrder.Name = "mtxtOrder";
            this.mtxtOrder.Size = new System.Drawing.Size(48, 20);
            this.mtxtOrder.TabIndex = 5;
            this.mtxtOrder.Text = "2";
            // 
            // frmSavGolOptions
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(248, 158);
            this.Controls.Add(this.mbtnOK);
            this.Controls.Add(this.mbtnCancel);
            this.Controls.Add(this.groupBox1);
            this.Name = "frmSavGolOptions";
            this.Text = "Savitzky Golay Options";
            this.groupBox1.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        #endregion

        private void mbtnOK_Click(object sender, System.EventArgs e)
        {
            try
            {
                var val = Convert.ToInt16(mtxtLeft.Text) ; 
            }
            catch(Exception ex)
            {
                MessageBox.Show(this, "Format problem for Left Points. Please enter a number") ; 
                Console.WriteLine(ex.Message) ; 
                return ; 
            }
            try
            {
                var val = Convert.ToInt16(mtxtRight.Text) ; 
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message + ex.StackTrace) ; 
                MessageBox.Show(this, "Format problem for Right Points. Please enter a number") ; 
                return ; 
            }
            try
            {
                var val = Convert.ToInt16(mtxtOrder.Text) ; 
            }
            catch(FormatException ex)
            {
                Console.WriteLine(ex.Message + ex.StackTrace) ; 
                MessageBox.Show(this, "Format problem for Order. Please enter a number") ; 
                return ; 
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message + ex.StackTrace) ; 
                MessageBox.Show(this, "Please check entered number") ; 
                return ; 
            }

            if (NumLeft > MaxPts)
            {
                MessageBox.Show(this, "Can have at most " + Convert.ToString(MaxPts) + " points for smoothing") ; 
                NumLeft = MaxPts ; 
                return ;
            }
            if (NumRight > MaxPts)
            {
                MessageBox.Show(this, "Can have at most " + Convert.ToString(MaxPts) + " points for smoothing") ; 
                NumRight = MaxPts ; 
            }
            if (Order > MaxOrder)
            {
                MessageBox.Show(this, "Maximum value of order = " + Convert.ToString(MaxOrder)) ; 
                Order = MaxOrder ; 
            }

            if (NumLeft + NumRight < Order)
            {
                MessageBox.Show(this, "Order can be at most equal to # left plus # right") ; 
                var val = NumLeft ;
                val += NumRight ; 
                Order = val ; 
                return ; 
            }

            DialogResult = DialogResult.OK ; 
            this.Hide() ; 
        }

        private void mbtnCancel_Click(object sender, System.EventArgs e)
        {
            DialogResult = DialogResult.Cancel ; 
            this.Hide() ; 
        }

        #region "Properties"
        public short NumLeft
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
        public short NumRight
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
        public short Order
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

        #endregion
    }
}
