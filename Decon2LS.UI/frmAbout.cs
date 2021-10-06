// Written by Navdeep Jaitly and Anoop Mayampurath for the Department of Energy (PNNL, Richland, WA)
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
    /// Summary description for frmAbout.
    /// </summary>
    public class frmAbout : System.Windows.Forms.Form
    {
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.LinkLabel mlinkLabelDownload;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.Button mbuttonOK;
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        public frmAbout()
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
            this.label1 = new System.Windows.Forms.Label();
            this.mlinkLabelDownload = new System.Windows.Forms.LinkLabel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.label2 = new System.Windows.Forms.Label();
            this.panel2 = new System.Windows.Forms.Panel();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.panel4 = new System.Windows.Forms.Panel();
            this.mbuttonOK = new System.Windows.Forms.Button();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel4.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label1.Dock = System.Windows.Forms.DockStyle.Top;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
            this.label1.Location = new System.Drawing.Point(0, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(674, 40);
            this.label1.TabIndex = 0;
            this.label1.Text = "Decon2LS";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // mlinkLabelDownload
            // 
            this.mlinkLabelDownload.Dock = System.Windows.Forms.DockStyle.Right;
            this.mlinkLabelDownload.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
            this.mlinkLabelDownload.Location = new System.Drawing.Point(450, 0);
            this.mlinkLabelDownload.Name = "mlinkLabelDownload";
            this.mlinkLabelDownload.Size = new System.Drawing.Size(224, 32);
            this.mlinkLabelDownload.TabIndex = 1;
            this.mlinkLabelDownload.TabStop = true;
            this.mlinkLabelDownload.Text = "https://omics.pnl.gov/software or http://panomics.pnnl.gov";
            this.mlinkLabelDownload.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.mlinkLabelDownload);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 40);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(674, 32);
            this.panel1.TabIndex = 2;
            // 
            // label2
            // 
            this.label2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
            this.label2.Location = new System.Drawing.Point(0, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(450, 32);
            this.label2.TabIndex = 2;
            this.label2.Text = "Version. Download Latest version from: ";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.richTextBox1);
            this.panel2.Controls.Add(this.panel4);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(0, 72);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(674, 264);
            this.panel2.TabIndex = 3;
            // 
            // richTextBox1
            // 
            this.richTextBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.richTextBox1.Location = new System.Drawing.Point(0, 0);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.ReadOnly = true;
            this.richTextBox1.Size = new System.Drawing.Size(674, 216);
            this.richTextBox1.TabIndex = 0;
            this.richTextBox1.TabStop = false;
            this.richTextBox1.Text = "Program written by Navdeep Jaitly, Anoop Mayampurath, Kyle Littlefield, Michael B" +
                "uschbach  and Gordon Anderson for the National Centre of Research Resources and " +
                "the Department of Energy (PNNL, Richland, WA) in 2005-2006. \n\nE-mail: navdeep.ja" +
                "itly@pnl.gov.\nWebsite: http://ncrr.pnl.gov/or http://www.sysbio.org/resources/st" +
                "aff/\n\n\nPublic Distribution Version\n\n For information on Decon2LS algorithms and " +
                "on the AMT Tag approach, please download source code (COMING SOON at http://ncrr" +
                ".pnl.gov/software/) or see: \"Advances in Proteomics Data Analysis and Display Us" +
                "ing an Accurate Mass and Time Tag Approach,\"J.D. Zimmer, M.E. Monroe, W.J. Qian," +
                " and R.D. Smith. Mass Spectrometry Reviews, 25, 450-482 (2006).\n\nLicensed under " +
                "the Apache License, Version 2.0; you may not use this file except in compliance " +
                "\nwith the License.  You may obtain a copy of the License at http://www.apache.or" +
                "g/licenses/LICENSE-2.0\n\nAll publications that result from the use of this softwa" +
                "re should include the following acknowledgment statement:\nPortions of this resea" +
                "rch were supported by the W.R. Wiley Environmental Molecular Science Laboratory," +
                " a national scientific user facility sponsored by the U.S. Department of Energy\'" +
                "s Office of Biological and Environmental Research and located at PNNL.  PNNL is " +
                "operated by Battelle Memorial Institute for the U.S. Department of Energy under " +
                "contract DE-AC05-76RL0 1830.\n\nNotice: This computer software was prepared by Bat" +
                "telle Memorial Institute, hereinafter the Contractor, under Contract No. DE-AC05" +
                "-76RL0 1830 with the Department of Energy (DOE).  All rights in the computer sof" +
                "tware are reserved by DOE on behalf of the United States Government and the Cont" +
                "ractor as provided in the Contract.NEITHER THE GOVERNMENT NOR THE CONTRACTOR MAK" +
                "ES ANY WARRANTY, EXPRESS OR IMPLIED, OR ASSUMES ANY LIABILITY FOR THE USE OF THI" +
                "S SOFTWARE.  This notice including this sentence must appear on any copies of th" +
                "is computer software.";
            // 
            // panel4
            // 
            this.panel4.Controls.Add(this.mbuttonOK);
            this.panel4.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel4.Location = new System.Drawing.Point(0, 216);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(674, 48);
            this.panel4.TabIndex = 2;
            // 
            // mbuttonOK
            // 
            this.mbuttonOK.Location = new System.Drawing.Point(256, 10);
            this.mbuttonOK.Name = "mbuttonOK";
            this.mbuttonOK.Size = new System.Drawing.Size(104, 24);
            this.mbuttonOK.TabIndex = 0;
            this.mbuttonOK.Text = "OK";
            this.mbuttonOK.Click += new System.EventHandler(this.mbuttonOK_Click);
            // 
            // frmAbout
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(674, 336);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "frmAbout";
            this.Text = "frmAbout";
            this.Load += new System.EventHandler(this.frmAbout_Load);
            this.panel1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panel4.ResumeLayout(false);
            this.ResumeLayout(false);
        }
        #endregion

        private void mrichTextBoxName_TextChanged(object sender, System.EventArgs e)
        {
        }

        private void mbuttonOK_Click(object sender, System.EventArgs e)
        {
            DialogResult = DialogResult.OK;
            this.Hide();
        }

        private void frmAbout_Load(object sender, System.EventArgs e)
        {
            var assem = System.Reflection.Assembly.GetExecutingAssembly();
            var assemName = assem.GetName();
            label2.Text = "Version " + assemName.Version.Major + "." + assemName.Version.Minor + " Download Latest version from:";
        }
    }
}
