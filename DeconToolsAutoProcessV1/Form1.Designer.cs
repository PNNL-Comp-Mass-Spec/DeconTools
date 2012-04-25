namespace DeconToolsAutoProcessV1
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
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
            this.btnSetupWizard = new System.Windows.Forms.Button();
            this.btnAutoProcess = new System.Windows.Forms.Button();
            this.Cancel = new System.Windows.Forms.Button();
            this.btnAbort = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.progressBar2 = new System.Windows.Forms.ProgressBar();
            this.txtTotalFeatures = new System.Windows.Forms.TextBox();
            this.txtProcessingStatus = new System.Windows.Forms.TextBox();
            this.txtFrame = new System.Windows.Forms.TextBox();
            this.txtScanCompleted = new System.Windows.Forms.TextBox();
            this.txtFile = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.lblNumFeaturesInCurrentFrame = new System.Windows.Forms.Label();
            this.lblNumFeaturesInCurrentScan = new System.Windows.Forms.Label();
            this.lblNumIsotopicProfiles = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.txtOutputPath = new System.Windows.Forms.TextBox();
            this.btnSetOutputPath = new System.Windows.Forms.Button();
            this.label8 = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnSetupWizard
            // 
            this.btnSetupWizard.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSetupWizard.BackColor = System.Drawing.Color.GhostWhite;
            this.btnSetupWizard.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSetupWizard.Location = new System.Drawing.Point(8, 8);
            this.btnSetupWizard.Name = "btnSetupWizard";
            this.btnSetupWizard.Size = new System.Drawing.Size(373, 34);
            this.btnSetupWizard.TabIndex = 0;
            this.btnSetupWizard.Text = "Setup Wizard";
            this.btnSetupWizard.UseVisualStyleBackColor = false;
            this.btnSetupWizard.Click += new System.EventHandler(this.button1_Click);
            // 
            // btnAutoProcess
            // 
            this.btnAutoProcess.BackColor = System.Drawing.Color.GhostWhite;
            this.btnAutoProcess.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAutoProcess.Location = new System.Drawing.Point(8, 48);
            this.btnAutoProcess.Name = "btnAutoProcess";
            this.btnAutoProcess.Size = new System.Drawing.Size(227, 54);
            this.btnAutoProcess.TabIndex = 1;
            this.btnAutoProcess.Text = "AutoProcess";
            this.btnAutoProcess.UseVisualStyleBackColor = false;
            this.btnAutoProcess.Click += new System.EventHandler(this.btnAutoProcess_Click);
            // 
            // Cancel
            // 
            this.Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.Cancel.BackColor = System.Drawing.Color.GhostWhite;
            this.Cancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Cancel.Location = new System.Drawing.Point(232, 277);
            this.Cancel.Name = "Cancel";
            this.Cancel.Size = new System.Drawing.Size(149, 27);
            this.Cancel.TabIndex = 4;
            this.Cancel.Text = "Quit";
            this.Cancel.UseVisualStyleBackColor = false;
            this.Cancel.Click += new System.EventHandler(this.Cancel_Click);
            // 
            // btnAbort
            // 
            this.btnAbort.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAbort.BackColor = System.Drawing.Color.GhostWhite;
            this.btnAbort.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAbort.Location = new System.Drawing.Point(241, 48);
            this.btnAbort.Name = "btnAbort";
            this.btnAbort.Size = new System.Drawing.Size(140, 54);
            this.btnAbort.TabIndex = 2;
            this.btnAbort.Text = "Abort";
            this.btnAbort.UseVisualStyleBackColor = false;
            this.btnAbort.Click += new System.EventHandler(this.btnAbort_Click);
            // 
            // button2
            // 
            this.button2.BackColor = System.Drawing.Color.GhostWhite;
            this.button2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button2.Location = new System.Drawing.Point(8, 277);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(215, 27);
            this.button2.TabIndex = 3;
            this.button2.Text = "Show log";
            this.button2.UseVisualStyleBackColor = false;
            this.button2.Visible = false;
            this.button2.Click += new System.EventHandler(this.btnAutoProcess_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.progressBar2);
            this.groupBox1.Controls.Add(this.txtTotalFeatures);
            this.groupBox1.Controls.Add(this.txtProcessingStatus);
            this.groupBox1.Controls.Add(this.txtFrame);
            this.groupBox1.Controls.Add(this.txtScanCompleted);
            this.groupBox1.Controls.Add(this.txtFile);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.lblNumFeaturesInCurrentFrame);
            this.groupBox1.Controls.Add(this.lblNumFeaturesInCurrentScan);
            this.groupBox1.Controls.Add(this.lblNumIsotopicProfiles);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.label7);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.progressBar1);
            this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.groupBox1.Location = new System.Drawing.Point(8, 124);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(373, 147);
            this.groupBox1.TabIndex = 5;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Status";
            // 
            // progressBar2
            // 
            this.progressBar2.Location = new System.Drawing.Point(98, 93);
            this.progressBar2.Name = "progressBar2";
            this.progressBar2.Size = new System.Drawing.Size(220, 13);
            this.progressBar2.TabIndex = 3;
            // 
            // txtTotalFeatures
            // 
            this.txtTotalFeatures.BackColor = System.Drawing.Color.GhostWhite;
            this.txtTotalFeatures.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtTotalFeatures.Location = new System.Drawing.Point(98, 121);
            this.txtTotalFeatures.Name = "txtTotalFeatures";
            this.txtTotalFeatures.Size = new System.Drawing.Size(102, 13);
            this.txtTotalFeatures.TabIndex = 2;
            this.txtTotalFeatures.TabStop = false;
            // 
            // txtProcessingStatus
            // 
            this.txtProcessingStatus.BackColor = System.Drawing.Color.GhostWhite;
            this.txtProcessingStatus.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtProcessingStatus.Location = new System.Drawing.Point(209, 121);
            this.txtProcessingStatus.Name = "txtProcessingStatus";
            this.txtProcessingStatus.Size = new System.Drawing.Size(109, 13);
            this.txtProcessingStatus.TabIndex = 2;
            this.txtProcessingStatus.TabStop = false;
            // 
            // txtFrame
            // 
            this.txtFrame.BackColor = System.Drawing.Color.GhostWhite;
            this.txtFrame.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtFrame.Location = new System.Drawing.Point(9, 94);
            this.txtFrame.Name = "txtFrame";
            this.txtFrame.Size = new System.Drawing.Size(69, 13);
            this.txtFrame.TabIndex = 2;
            this.txtFrame.TabStop = false;
            // 
            // txtScanCompleted
            // 
            this.txtScanCompleted.BackColor = System.Drawing.Color.GhostWhite;
            this.txtScanCompleted.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtScanCompleted.Location = new System.Drawing.Point(9, 121);
            this.txtScanCompleted.Name = "txtScanCompleted";
            this.txtScanCompleted.Size = new System.Drawing.Size(69, 13);
            this.txtScanCompleted.TabIndex = 1;
            this.txtScanCompleted.TabStop = false;
            // 
            // txtFile
            // 
            this.txtFile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtFile.BackColor = System.Drawing.Color.GhostWhite;
            this.txtFile.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtFile.Location = new System.Drawing.Point(37, 16);
            this.txtFile.Name = "txtFile";
            this.txtFile.Size = new System.Drawing.Size(329, 13);
            this.txtFile.TabIndex = 2;
            this.txtFile.TabStop = false;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(206, 109);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(95, 13);
            this.label4.TabIndex = 1;
            this.label4.Text = "Processing Status:";
            // 
            // lblNumFeaturesInCurrentFrame
            // 
            this.lblNumFeaturesInCurrentFrame.AutoSize = true;
            this.lblNumFeaturesInCurrentFrame.Location = new System.Drawing.Point(95, 93);
            this.lblNumFeaturesInCurrentFrame.Name = "lblNumFeaturesInCurrentFrame";
            this.lblNumFeaturesInCurrentFrame.Size = new System.Drawing.Size(10, 13);
            this.lblNumFeaturesInCurrentFrame.TabIndex = 1;
            this.lblNumFeaturesInCurrentFrame.Text = " ";
            // 
            // lblNumFeaturesInCurrentScan
            // 
            this.lblNumFeaturesInCurrentScan.AutoSize = true;
            this.lblNumFeaturesInCurrentScan.Location = new System.Drawing.Point(95, 121);
            this.lblNumFeaturesInCurrentScan.Name = "lblNumFeaturesInCurrentScan";
            this.lblNumFeaturesInCurrentScan.Size = new System.Drawing.Size(10, 13);
            this.lblNumFeaturesInCurrentScan.TabIndex = 1;
            this.lblNumFeaturesInCurrentScan.Text = " ";
            // 
            // lblNumIsotopicProfiles
            // 
            this.lblNumIsotopicProfiles.AutoSize = true;
            this.lblNumIsotopicProfiles.Location = new System.Drawing.Point(291, 80);
            this.lblNumIsotopicProfiles.Name = "lblNumIsotopicProfiles";
            this.lblNumIsotopicProfiles.Size = new System.Drawing.Size(10, 13);
            this.lblNumIsotopicProfiles.TabIndex = 1;
            this.lblNumIsotopicProfiles.Text = " ";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(6, 83);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(39, 13);
            this.label6.TabIndex = 1;
            this.label6.Text = "Frame:";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(95, 109);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(75, 13);
            this.label7.TabIndex = 1;
            this.label7.Text = "Total features:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(95, 80);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(120, 13);
            this.label5.TabIndex = 1;
            this.label5.Text = "#features (current scan)";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 110);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(35, 13);
            this.label3.TabIndex = 1;
            this.label3.Text = "Scan:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 16);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(26, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "File:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 39);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(156, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Overall file processing progress:";
            // 
            // progressBar1
            // 
            this.progressBar1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar1.Location = new System.Drawing.Point(6, 55);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(363, 22);
            this.progressBar1.TabIndex = 0;
            // 
            // txtOutputPath
            // 
            this.txtOutputPath.AllowDrop = true;
            this.txtOutputPath.Location = new System.Drawing.Point(8, 323);
            this.txtOutputPath.Name = "txtOutputPath";
            this.txtOutputPath.Size = new System.Drawing.Size(284, 20);
            this.txtOutputPath.TabIndex = 6;
            this.txtOutputPath.TextChanged += new System.EventHandler(this.txtOutputPath_TextChanged);
            this.txtOutputPath.DragDrop += new System.Windows.Forms.DragEventHandler(this.txtOutputPath_DragDrop);
            this.txtOutputPath.DragEnter += new System.Windows.Forms.DragEventHandler(this.basic_dragEnter);
            // 
            // btnSetOutputPath
            // 
            this.btnSetOutputPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSetOutputPath.BackColor = System.Drawing.Color.GhostWhite;
            this.btnSetOutputPath.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSetOutputPath.Location = new System.Drawing.Point(298, 322);
            this.btnSetOutputPath.Name = "btnSetOutputPath";
            this.btnSetOutputPath.Size = new System.Drawing.Size(83, 23);
            this.btnSetOutputPath.TabIndex = 4;
            this.btnSetOutputPath.Text = "set";
            this.btnSetOutputPath.UseVisualStyleBackColor = false;
            this.btnSetOutputPath.Click += new System.EventHandler(this.btnSetOutputPath_Click);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(7, 311);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(219, 13);
            this.label8.TabIndex = 1;
            this.label8.Text = "Optional output path (leave empty for default)";
            // 
            // Form1
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.LightSteelBlue;
            this.ClientSize = new System.Drawing.Size(388, 362);
            this.Controls.Add(this.txtOutputPath);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.btnSetOutputPath);
            this.Controls.Add(this.Cancel);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.btnAbort);
            this.Controls.Add(this.btnAutoProcess);
            this.Controls.Add(this.btnSetupWizard);
            this.Controls.Add(this.label8);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Name = "Form1";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "DeconTools AutoProcessor V1.0";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnSetupWizard;
        private System.Windows.Forms.Button btnAutoProcess;
        private System.Windows.Forms.Button Cancel;
        private System.Windows.Forms.Button btnAbort;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.TextBox txtFile;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtScanCompleted;
        private System.Windows.Forms.TextBox txtProcessingStatus;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ProgressBar progressBar2;
        private System.Windows.Forms.Label lblNumIsotopicProfiles;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtFrame;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label lblNumFeaturesInCurrentFrame;
        private System.Windows.Forms.Label lblNumFeaturesInCurrentScan;
        private System.Windows.Forms.TextBox txtTotalFeatures;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox txtOutputPath;
        private System.Windows.Forms.Button btnSetOutputPath;
        private System.Windows.Forms.Label label8;
    }
}

