namespace DeconTools
{
    partial class DataProcessingConfigurationForm
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.btnPeakDetectorConfig = new System.Windows.Forms.Button();
            this.radioButton1 = new System.Windows.Forms.RadioButton();
            this.radioButton2 = new System.Windows.Forms.RadioButton();
            this.btnDeconvolutorConfig = new System.Windows.Forms.Button();
            this.btnMSGeneratorConfig = new System.Windows.Forms.Button();
            this.btnSaveTaskList = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.groupBox5.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.btnMSGeneratorConfig);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(385, 62);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Task 1 - \'MS Generator\'";
            // 
            // groupBox2
            // 
            this.groupBox2.Location = new System.Drawing.Point(12, 80);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(385, 62);
            this.groupBox2.TabIndex = 0;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Task 2 - \'Data Smoother\'   -- CURRENTLY UNAVAILABLE";
            // 
            // groupBox3
            // 
            this.groupBox3.Location = new System.Drawing.Point(12, 148);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(385, 63);
            this.groupBox3.TabIndex = 0;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Task 3 - \'Zero Filler\'  -- CURRENTLY UNAVAILABLE";
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.btnPeakDetectorConfig);
            this.groupBox4.Location = new System.Drawing.Point(12, 217);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(385, 63);
            this.groupBox4.TabIndex = 0;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Task 4 - \'Peak Detector\'";
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.btnDeconvolutorConfig);
            this.groupBox5.Controls.Add(this.radioButton2);
            this.groupBox5.Controls.Add(this.radioButton1);
            this.groupBox5.Location = new System.Drawing.Point(12, 286);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(385, 63);
            this.groupBox5.TabIndex = 0;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "Task 5 - \'Deconvolutor\'";
            // 
            // btnPeakDetectorConfig
            // 
            this.btnPeakDetectorConfig.FlatAppearance.BorderColor = System.Drawing.Color.Gray;
            this.btnPeakDetectorConfig.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnPeakDetectorConfig.Location = new System.Drawing.Point(304, 19);
            this.btnPeakDetectorConfig.Name = "btnPeakDetectorConfig";
            this.btnPeakDetectorConfig.Size = new System.Drawing.Size(75, 23);
            this.btnPeakDetectorConfig.TabIndex = 0;
            this.btnPeakDetectorConfig.Text = "Config";
            this.btnPeakDetectorConfig.UseVisualStyleBackColor = true;
            // 
            // radioButton1
            // 
            this.radioButton1.AutoSize = true;
            this.radioButton1.Location = new System.Drawing.Point(31, 20);
            this.radioButton1.Name = "radioButton1";
            this.radioButton1.Size = new System.Drawing.Size(142, 17);
            this.radioButton1.TabIndex = 0;
            this.radioButton1.TabStop = true;
            this.radioButton1.Text = "Use Horn Deconvolution";
            this.radioButton1.UseVisualStyleBackColor = true;
            // 
            // radioButton2
            // 
            this.radioButton2.AutoSize = true;
            this.radioButton2.Location = new System.Drawing.Point(31, 40);
            this.radioButton2.Name = "radioButton2";
            this.radioButton2.Size = new System.Drawing.Size(152, 17);
            this.radioButton2.TabIndex = 0;
            this.radioButton2.TabStop = true;
            this.radioButton2.Text = "Use RAPID Deconvolution";
            this.radioButton2.UseVisualStyleBackColor = true;
            // 
            // btnDeconvolutorConfig
            // 
            this.btnDeconvolutorConfig.FlatAppearance.BorderColor = System.Drawing.Color.Gray;
            this.btnDeconvolutorConfig.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDeconvolutorConfig.Location = new System.Drawing.Point(304, 20);
            this.btnDeconvolutorConfig.Name = "btnDeconvolutorConfig";
            this.btnDeconvolutorConfig.Size = new System.Drawing.Size(75, 23);
            this.btnDeconvolutorConfig.TabIndex = 0;
            this.btnDeconvolutorConfig.Text = "Config";
            this.btnDeconvolutorConfig.UseVisualStyleBackColor = true;
            // 
            // btnMSGeneratorConfig
            // 
            this.btnMSGeneratorConfig.FlatAppearance.BorderColor = System.Drawing.Color.Gray;
            this.btnMSGeneratorConfig.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnMSGeneratorConfig.Location = new System.Drawing.Point(304, 19);
            this.btnMSGeneratorConfig.Name = "btnMSGeneratorConfig";
            this.btnMSGeneratorConfig.Size = new System.Drawing.Size(75, 23);
            this.btnMSGeneratorConfig.TabIndex = 0;
            this.btnMSGeneratorConfig.Text = "Config";
            this.btnMSGeneratorConfig.UseVisualStyleBackColor = true;
            // 
            // btnSaveTaskList
            // 
            this.btnSaveTaskList.FlatAppearance.BorderColor = System.Drawing.Color.Gray;
            this.btnSaveTaskList.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSaveTaskList.Location = new System.Drawing.Point(12, 384);
            this.btnSaveTaskList.Name = "btnSaveTaskList";
            this.btnSaveTaskList.Size = new System.Drawing.Size(128, 23);
            this.btnSaveTaskList.TabIndex = 0;
            this.btnSaveTaskList.Text = "Save configurations";
            this.btnSaveTaskList.UseVisualStyleBackColor = true;
            // 
            // button1
            // 
            this.button1.FlatAppearance.BorderColor = System.Drawing.Color.Gray;
            this.button1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button1.Location = new System.Drawing.Point(12, 355);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(128, 23);
            this.button1.TabIndex = 0;
            this.button1.Text = "Load configurations";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // button2
            // 
            this.button2.FlatAppearance.BorderColor = System.Drawing.Color.Gray;
            this.button2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button2.Location = new System.Drawing.Point(316, 370);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 37);
            this.button2.TabIndex = 0;
            this.button2.Text = "Done!";
            this.button2.UseVisualStyleBackColor = true;
            // 
            // DataProcessingConfigurationForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.WhiteSmoke;
            this.ClientSize = new System.Drawing.Size(407, 422);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.btnSaveTaskList);
            this.Controls.Add(this.groupBox5);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Name = "DataProcessingConfigurationForm";
            this.Text = "DataProcessingConfigurationForm";
            this.groupBox1.ResumeLayout(false);
            this.groupBox4.ResumeLayout(false);
            this.groupBox5.ResumeLayout(false);
            this.groupBox5.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.Button btnPeakDetectorConfig;
        private System.Windows.Forms.Button btnDeconvolutorConfig;
        private System.Windows.Forms.RadioButton radioButton2;
        private System.Windows.Forms.RadioButton radioButton1;
        private System.Windows.Forms.Button btnMSGeneratorConfig;
        private System.Windows.Forms.Button btnSaveTaskList;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
    }
}