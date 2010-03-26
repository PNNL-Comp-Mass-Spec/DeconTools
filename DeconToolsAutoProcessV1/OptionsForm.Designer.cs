namespace DeconToolsAutoProcessV1
{
    partial class OptionsForm
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
            this.chkUseResultMergerMode = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btnUpdate = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.chkCreateMSFeatureForEachPeak = new System.Windows.Forms.CheckBox();
            this.label2 = new System.Windows.Forms.Label();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // chkUseResultMergerMode
            // 
            this.chkUseResultMergerMode.AutoSize = true;
            this.chkUseResultMergerMode.Location = new System.Drawing.Point(13, 13);
            this.chkUseResultMergerMode.Name = "chkUseResultMergerMode";
            this.chkUseResultMergerMode.Size = new System.Drawing.Size(155, 17);
            this.chkUseResultMergerMode.TabIndex = 0;
            this.chkUseResultMergerMode.Text = "Use \'Result Merger Mode\' *";
            this.chkUseResultMergerMode.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(34, 38);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(238, 26);
            this.label1.TabIndex = 1;
            this.label1.Text = "* This mode will combine results from multiple files\r\ninto a single _isos and a s" +
                "ingle _scans file\r\n";
            // 
            // btnUpdate
            // 
            this.btnUpdate.Location = new System.Drawing.Point(136, 131);
            this.btnUpdate.Name = "btnUpdate";
            this.btnUpdate.Size = new System.Drawing.Size(75, 23);
            this.btnUpdate.TabIndex = 2;
            this.btnUpdate.Text = "Update";
            this.btnUpdate.UseVisualStyleBackColor = true;
            this.btnUpdate.Click += new System.EventHandler(this.btnUpdate_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(217, 131);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(52, 23);
            this.btnCancel.TabIndex = 2;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // chkCreateMSFeatureForEachPeak
            // 
            this.chkCreateMSFeatureForEachPeak.AutoSize = true;
            this.chkCreateMSFeatureForEachPeak.Location = new System.Drawing.Point(13, 73);
            this.chkCreateMSFeatureForEachPeak.Name = "chkCreateMSFeatureForEachPeak";
            this.chkCreateMSFeatureForEachPeak.Size = new System.Drawing.Size(198, 17);
            this.chkCreateMSFeatureForEachPeak.TabIndex = 0;
            this.chkCreateMSFeatureForEachPeak.Text = "Create MS_Feature for each peak **";
            this.chkCreateMSFeatureForEachPeak.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(34, 89);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(218, 39);
            this.label2.TabIndex = 1;
            this.label2.Text = "** This mode will create an MS Feature for \r\neach MS Peak detected.  No Deconvolu" +
                "tion\r\nis performed. \r\n";
            // 
            // comboBox1
            // 
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Location = new System.Drawing.Point(37, 190);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(232, 21);
            this.comboBox1.TabIndex = 3;
            this.comboBox1.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
            // 
            // OptionsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(291, 171);
            this.Controls.Add(this.comboBox1);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnUpdate);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.chkCreateMSFeatureForEachPeak);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.chkUseResultMergerMode);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "OptionsForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "OptionsForm";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox chkUseResultMergerMode;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnUpdate;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.CheckBox chkCreateMSFeatureForEachPeak;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox comboBox1;
    }
}