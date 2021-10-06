// Written by Kyle Littlefield for the Department of Energy (PNNL, Richland, WA)
// Copyright 2006, Battelle Memorial Institute
// E-mail: navdeep.jaitly@pnl.gov or proteomics@pnnl.gov
// Website: https://github.com/PNNL-Comp-Mass-Spec/ or https://panomics.pnnl.gov/ or https://www.pnnl.gov/integrative-omics
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
    /// Summary description for frmSimpleDialogBox.
    /// </summary>
    public class frmSimpleDialogBox : System.Windows.Forms.Form
    {
        private System.Windows.Forms.Label mlabel;
        private System.Windows.Forms.Button mbtn_OK;
        private System.Windows.Forms.Button mbtn_cancel;
        private System.Windows.Forms.TextBox mtxt_value;
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        public frmSimpleDialogBox(string label, string default_value)
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            //
            // TODO: Add any constructor code after InitializeComponent call
            //
            this.mtxt_value.Text = default_value;
            this.mlabel.Text = label;
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

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.mlabel = new System.Windows.Forms.Label();
            this.mtxt_value = new System.Windows.Forms.TextBox();
            this.mbtn_OK = new System.Windows.Forms.Button();
            this.mbtn_cancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            //
            // mlabel
            //
            this.mlabel.Location = new System.Drawing.Point(8, 16);
            this.mlabel.Name = "mlabel";
            this.mlabel.Size = new System.Drawing.Size(152, 16);
            this.mlabel.TabIndex = 0;
            //
            // mtxt_value
            //
            this.mtxt_value.Location = new System.Drawing.Point(183, 10);
            this.mtxt_value.Name = "mtxt_value";
            this.mtxt_value.Size = new System.Drawing.Size(80, 20);
            this.mtxt_value.TabIndex = 1;
            this.mtxt_value.Text = "";
            //
            // mbtn_OK
            //
            this.mbtn_OK.Location = new System.Drawing.Point(56, 48);
            this.mbtn_OK.Name = "mbtn_OK";
            this.mbtn_OK.Size = new System.Drawing.Size(80, 24);
            this.mbtn_OK.TabIndex = 2;
            this.mbtn_OK.Text = "OK";
            this.mbtn_OK.Click += new System.EventHandler(this.mbtn_OK_Click);
            //
            // mbtn_cancel
            //
            this.mbtn_cancel.Location = new System.Drawing.Point(168, 48);
            this.mbtn_cancel.Name = "mbtn_cancel";
            this.mbtn_cancel.Size = new System.Drawing.Size(80, 24);
            this.mbtn_cancel.TabIndex = 3;
            this.mbtn_cancel.Text = "Cancel";
            this.mbtn_cancel.Click += new System.EventHandler(this.mbtn_cancel_Click);
            //
            // frmSimpleDialogBox
            //
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(296, 78);
            this.Controls.Add(this.mbtn_cancel);
            this.Controls.Add(this.mbtn_OK);
            this.Controls.Add(this.mtxt_value);
            this.Controls.Add(this.mlabel);
            this.Name = "frmSimpleDialogBox";
            this.Text = "frmSimpleDialogBox";
            this.ResumeLayout(false);
        }
        #endregion

        private void mbtn_OK_Click(object sender, System.EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Hide();
        }

        private void mbtn_cancel_Click(object sender, System.EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Hide();
        }

        public string Value
        {
            get
            {
                return this.mtxt_value.Text;
            }
            set
            {
                this.mtxt_value.Text = value;
            }
        }
        public string Label
        {
            get
            {
                return this.mlabel.Text;
            }
            set
            {
                this.mlabel.Text = value;
            }
        }
    }
}
