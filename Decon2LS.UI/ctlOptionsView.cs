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
    /// Summary description for ctlOptionsView.
    /// </summary>
    public class ctlOptionsView : System.Windows.Forms.UserControl
    {
        private System.Windows.Forms.TreeView treeView1;
        private System.Windows.Forms.TreeNode nodePeakPicking;
        private System.Windows.Forms.TreeNode nodePeakPickingGeneral;
        private System.Windows.Forms.TreeNode nodeHornTransform;
        private System.Windows.Forms.TreeNode nodeHornTransformGeneral;
        private System.Windows.Forms.TreeNode nodeIsotopeDistribution;
        private System.Windows.Forms.TreeNode nodeIsotopeDistributionGeneral;
        private System.Windows.Forms.TreeNode nodeIsotopeDistributionAveragine;
        private System.Windows.Forms.TreeNode nodeIsotopeDistributionIsotopicDistribution;
        private System.Windows.Forms.TreeNode nodeIsotopeDistributionType;
        private System.Windows.Forms.TreeNode nodeIsotopeDistributionComposition;

        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        public ctlOptionsView()
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();

            // TODO: Add any initialization after the InitializeComponent call

            nodePeakPicking = new TreeNode();
            nodePeakPickingGeneral = new TreeNode();
            nodeHornTransform = new TreeNode();
            nodeHornTransformGeneral = new TreeNode();
            nodeIsotopeDistribution = new TreeNode();
            nodeIsotopeDistributionGeneral = new TreeNode();
            nodeIsotopeDistributionAveragine = new TreeNode();
            nodeIsotopeDistributionComposition = new TreeNode();
            nodeIsotopeDistributionIsotopicDistribution = new TreeNode();
            nodeIsotopeDistributionType = new TreeNode();
            
            
            nodePeakPicking.Text = "Peak Picking";
            nodeHornTransform.Text = "Horn Transform";
            nodeIsotopeDistribution.Text = "Isotope Distribution";
            nodePeakPickingGeneral.Text = "General";
            nodeHornTransformGeneral.Text = "General";
            nodeIsotopeDistributionGeneral.Text = "General";
            nodeIsotopeDistributionAveragine.Text = "Averagine";
            

            treeView1.Nodes.Add(nodePeakPicking);
            nodePeakPicking.Nodes.Add(nodePeakPickingGeneral);
            treeView1.Nodes.Add(nodeHornTransform);
            nodeHornTransform.Nodes.Add(nodeHornTransformGeneral);
            treeView1.Nodes.Add(nodeIsotopeDistribution);
            nodeIsotopeDistribution.Nodes.Add(nodeIsotopeDistributionGeneral);
            nodeIsotopeDistribution.Nodes.Add(nodeIsotopeDistributionAveragine);


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

        public bool Choose()
        {
            if (string.Compare(treeView1.SelectedNode.Text, nodePeakPicking.Text)== 0)
            {
                return true;
            }
            else
            {
                return false;
            }
            
        }
    

        #region Component Designer generated code
        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.treeView1 = new System.Windows.Forms.TreeView();
            this.SuspendLayout();
            // 
            // treeView1
            // 
            this.treeView1.ImageIndex = -1;
            this.treeView1.Location = new System.Drawing.Point(8, 8);
            this.treeView1.Name = "treeView1";
            this.treeView1.SelectedImageIndex = -1;
            this.treeView1.Size = new System.Drawing.Size(264, 424);
            this.treeView1.TabIndex = 0;
            this.treeView1.Click += new System.EventHandler(this.treeView1_Click);
            // 
            // ctlOptionsView
            // 
            this.Controls.Add(this.treeView1);
            this.Name = "ctlOptionsView";
            this.Size = new System.Drawing.Size(520, 448);
            this.ResumeLayout(false);

        }
        #endregion

        private void treeView1_Click(object sender, System.EventArgs e)
        {

            var set_visibility = Choose();
            //ctlPeakPickingOptions1.Visible = set_visibility;
        
        }
    }
}
