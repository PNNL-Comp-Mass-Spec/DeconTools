namespace DeconTools.DataProcessingConfiguration
{
    partial class PeakDetectorConfig
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
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.sigNoiseUpDown = new System.Windows.Forms.NumericUpDown();
            this.peakBackgroundUpDown = new System.Windows.Forms.NumericUpDown();
            this.peakFitComboBox = new System.Windows.Forms.ComboBox();
            this.isDataThresholdedCheckBox = new System.Windows.Forms.CheckBox();
            this.btnUpdate = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.sigNoiseUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.peakBackgroundUpDown)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(122, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Signal-to-noise threshold";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 48);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(127, 13);
            this.label2.TabIndex = 0;
            this.label2.Text = "Peak-to-background ratio";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)));
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 71);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(103, 13);
            this.label3.TabIndex = 0;
            this.label3.Text = "Is data thresholded?";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label4
            // 
            this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)));
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 95);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(66, 13);
            this.label4.TabIndex = 0;
            this.label4.Text = "Peak fit type";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // sigNoiseUpDown
            // 
            this.sigNoiseUpDown.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)));
            this.sigNoiseUpDown.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.sigNoiseUpDown.DecimalPlaces = 1;
            this.sigNoiseUpDown.Increment = new decimal(new int[] {
            5,
            0,
            0,
            65536});
            this.sigNoiseUpDown.Location = new System.Drawing.Point(152, 20);
            this.sigNoiseUpDown.Name = "sigNoiseUpDown";
            this.sigNoiseUpDown.Size = new System.Drawing.Size(133, 20);
            this.sigNoiseUpDown.TabIndex = 1;
            // 
            // peakBackgroundUpDown
            // 
            this.peakBackgroundUpDown.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)));
            this.peakBackgroundUpDown.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.peakBackgroundUpDown.DecimalPlaces = 1;
            this.peakBackgroundUpDown.Increment = new decimal(new int[] {
            5,
            0,
            0,
            65536});
            this.peakBackgroundUpDown.Location = new System.Drawing.Point(152, 46);
            this.peakBackgroundUpDown.Name = "peakBackgroundUpDown";
            this.peakBackgroundUpDown.Size = new System.Drawing.Size(133, 20);
            this.peakBackgroundUpDown.TabIndex = 1;
            // 
            // peakFitComboBox
            // 
            this.peakFitComboBox.FormattingEnabled = true;
            this.peakFitComboBox.Location = new System.Drawing.Point(152, 92);
            this.peakFitComboBox.Name = "peakFitComboBox";
            this.peakFitComboBox.Size = new System.Drawing.Size(133, 21);
            this.peakFitComboBox.TabIndex = 2;
            // 
            // isDataThresholdedCheckBox
            // 
            this.isDataThresholdedCheckBox.AutoSize = true;
            this.isDataThresholdedCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.isDataThresholdedCheckBox.Location = new System.Drawing.Point(152, 73);
            this.isDataThresholdedCheckBox.Name = "isDataThresholdedCheckBox";
            this.isDataThresholdedCheckBox.Size = new System.Drawing.Size(12, 11);
            this.isDataThresholdedCheckBox.TabIndex = 3;
            this.isDataThresholdedCheckBox.UseVisualStyleBackColor = true;
            // 
            // btnUpdate
            // 
            this.btnUpdate.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnUpdate.Location = new System.Drawing.Point(152, 127);
            this.btnUpdate.Name = "btnUpdate";
            this.btnUpdate.Size = new System.Drawing.Size(75, 23);
            this.btnUpdate.TabIndex = 4;
            this.btnUpdate.Text = "Update";
            this.btnUpdate.UseVisualStyleBackColor = true;
            this.btnUpdate.Click += new System.EventHandler(this.btnUpdate_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCancel.Location = new System.Drawing.Point(233, 127);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(52, 23);
            this.btnCancel.TabIndex = 4;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // PeakDetectorConfig
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(294, 158);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnUpdate);
            this.Controls.Add(this.isDataThresholdedCheckBox);
            this.Controls.Add(this.peakFitComboBox);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.peakBackgroundUpDown);
            this.Controls.Add(this.sigNoiseUpDown);
            this.Name = "PeakDetectorConfig";
            this.Text = "PeakDetectorConfig";
            ((System.ComponentModel.ISupportInitialize)(this.sigNoiseUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.peakBackgroundUpDown)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.NumericUpDown sigNoiseUpDown;
        private System.Windows.Forms.NumericUpDown peakBackgroundUpDown;
        private System.Windows.Forms.ComboBox peakFitComboBox;
        private System.Windows.Forms.CheckBox isDataThresholdedCheckBox;
        private System.Windows.Forms.Button btnUpdate;
        private System.Windows.Forms.Button btnCancel;
    }
}