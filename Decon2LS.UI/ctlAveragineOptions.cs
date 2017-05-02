// Written by Navdeep Jaitly and Anoop Mayampurath for the Department of Energy (PNNL, Richland, WA)
// Copyright 2006, Battelle Memorial Institute
// E-mail: navdeep.jaitly@pnl.gov
// Website: http://omics.pnl.gov/software or http://panomics.pnnl.gov
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
    /// Summary description for ctlAveragineOptions.
    /// </summary>
    public class ctlAveragineOptions : System.Windows.Forms.UserControl
    {
        private const string mstrProteinAveragine = "C4.9384 H7.7583 N1.3577 O1.4773 S0.0417" ;
        private const string mstrDNAAveragine = "C3.9 H4.9 N1.5 O2.4 P0.4" ;
        private const string mstrRNAAveragine = "C3.8 H4.7 N1.5 O2.8 P0.4" ;

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Label label_AvgFormula;
        private System.Windows.Forms.Label label_TagFormula_tip;
        private System.Windows.Forms.RadioButton mradioButtonCustom;
        private System.Windows.Forms.RadioButton mradioButtonRNA;
        private System.Windows.Forms.RadioButton mradioButtonDNA;
        private System.Windows.Forms.RadioButton mradioButtonProtein;
        private System.Windows.Forms.TextBox mTextCustomAveragine;
        private System.Windows.Forms.TextBox mTextTagFormula;
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        public ctlAveragineOptions()
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.mTextCustomAveragine = new System.Windows.Forms.TextBox();
            this.mradioButtonCustom = new System.Windows.Forms.RadioButton();
            this.mradioButtonRNA = new System.Windows.Forms.RadioButton();
            this.mradioButtonDNA = new System.Windows.Forms.RadioButton();
            this.mradioButtonProtein = new System.Windows.Forms.RadioButton();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.mTextTagFormula = new System.Windows.Forms.TextBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.label_TagFormula_tip = new System.Windows.Forms.Label();
            this.label_AvgFormula = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.mTextCustomAveragine);
            this.groupBox1.Controls.Add(this.mradioButtonCustom);
            this.groupBox1.Controls.Add(this.mradioButtonRNA);
            this.groupBox1.Controls.Add(this.mradioButtonDNA);
            this.groupBox1.Controls.Add(this.mradioButtonProtein);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.groupBox1.Location = new System.Drawing.Point(0, 0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(504, 208);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Averagine Formula";
            // 
            // mTextCustomAveragine
            // 
            this.mTextCustomAveragine.Location = new System.Drawing.Point(160, 152);
            this.mTextCustomAveragine.Name = "mTextCustomAveragine";
            this.mTextCustomAveragine.Size = new System.Drawing.Size(256, 20);
            this.mTextCustomAveragine.TabIndex = 7;
            this.mTextCustomAveragine.Text = "";
            // 
            // mradioButtonCustom
            // 
            this.mradioButtonCustom.Location = new System.Drawing.Point(48, 152);
            this.mradioButtonCustom.Name = "mradioButtonCustom";
            this.mradioButtonCustom.Size = new System.Drawing.Size(96, 24);
            this.mradioButtonCustom.TabIndex = 6;
            this.mradioButtonCustom.Text = "Custom";
            // 
            // mradioButtonRNA
            // 
            this.mradioButtonRNA.Location = new System.Drawing.Point(48, 104);
            this.mradioButtonRNA.Name = "mradioButtonRNA";
            this.mradioButtonRNA.Size = new System.Drawing.Size(352, 24);
            this.mradioButtonRNA.TabIndex = 4;
            this.mradioButtonRNA.Text = "RNA: C3.8 H4.7 N1.5 O2.8 P0.4 = 128.5775 Da";
            // 
            // mradioButtonDNA
            // 
            this.mradioButtonDNA.Location = new System.Drawing.Point(48, 64);
            this.mradioButtonDNA.Name = "mradioButtonDNA";
            this.mradioButtonDNA.Size = new System.Drawing.Size(352, 24);
            this.mradioButtonDNA.TabIndex = 3;
            this.mradioButtonDNA.Text = "DNA: C3.9 H4.9 N1.5 O2.4 P0.4 = 123.5805 Da";
            // 
            // mradioButtonProtein
            // 
            this.mradioButtonProtein.Checked = true;
            this.mradioButtonProtein.Location = new System.Drawing.Point(48, 24);
            this.mradioButtonProtein.Name = "mradioButtonProtein";
            this.mradioButtonProtein.Size = new System.Drawing.Size(368, 24);
            this.mradioButtonProtein.TabIndex = 2;
            this.mradioButtonProtein.TabStop = true;
            this.mradioButtonProtein.Text = "Protein: C4.9384 H7.7583 N1.3577 O1.4773 S0.0417 = 111.1237 Da";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.mTextTagFormula);
            this.groupBox2.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.groupBox2.Location = new System.Drawing.Point(0, 208);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(504, 128);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Tag Formula";
            // 
            // mTextTagFormula
            // 
            this.mTextTagFormula.Location = new System.Drawing.Point(168, 56);
            this.mTextTagFormula.Name = "mTextTagFormula";
            this.mTextTagFormula.Size = new System.Drawing.Size(256, 20);
            this.mTextTagFormula.TabIndex = 9;
            this.mTextTagFormula.Text = "";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.label_TagFormula_tip);
            this.groupBox3.Controls.Add(this.label_AvgFormula);
            this.groupBox3.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.groupBox3.Location = new System.Drawing.Point(0, 336);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(504, 136);
            this.groupBox3.TabIndex = 2;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Helpful Tips";
            // 
            // label_TagFormula_tip
            // 
            this.label_TagFormula_tip.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.5F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
            this.label_TagFormula_tip.Location = new System.Drawing.Point(8, 72);
            this.label_TagFormula_tip.Name = "label_TagFormula_tip";
            this.label_TagFormula_tip.Size = new System.Drawing.Size(376, 16);
            this.label_TagFormula_tip.TabIndex = 1;
            this.label_TagFormula_tip.Text = "Tag Formula:  Enter any chemical tag attached to the compound";
            // 
            // label_AvgFormula
            // 
            this.label_AvgFormula.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.5F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
            this.label_AvgFormula.Location = new System.Drawing.Point(8, 32);
            this.label_AvgFormula.Name = "label_AvgFormula";
            this.label_AvgFormula.Size = new System.Drawing.Size(368, 16);
            this.label_AvgFormula.TabIndex = 0;
            this.label_AvgFormula.Text = "Averagine Formula : Select the emperical formula to be used";
            // 
            // ctlAveragineOptions
            // 
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox3);
            this.Name = "ctlAveragineOptions";
            this.Size = new System.Drawing.Size(504, 472);
            this.groupBox1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        #endregion

        public string AveragineFormula
        {
            get
            {
                if (mradioButtonProtein.Checked)
                {
                    return mstrProteinAveragine ;
                }
                else if (mradioButtonDNA.Checked)
                {
                    return mstrDNAAveragine ;
                }
                else if (mradioButtonRNA.Checked)
                {
                    return mstrRNAAveragine ;
                }
                else
                {
                    return mTextCustomAveragine.Text ; 
                }
            }
            set
            {				
                if (value == mstrProteinAveragine)
                {
                    mradioButtonProtein.Checked = true ; 
                }
                else if (value == mstrDNAAveragine)
                {
                    mradioButtonDNA.Checked = true ;
                }
                else if (value == mstrRNAAveragine)
                {
                    mradioButtonRNA.Checked = true ; 
                }
                else
                {
                    mTextCustomAveragine.Text = value ; 
                    mradioButtonCustom.Checked = true ; 
                }
            }
        }
        public string TagFormula
        {
            get
            {
                return mTextTagFormula.Text ; 
            }
            set
            {
                mTextTagFormula.Text = value ; 
            }
        }
    }
}

