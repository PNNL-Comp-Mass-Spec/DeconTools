// Written by Kyle Littlefield 
// for the Department of Energy (PNNL, Richland, WA)
// Copyright 2006, Battelle Memorial Institute
// E-mail: navdeep.jaitly@pnl.gov
// Website: http://ncrr.pnl.gov/software
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
	/// Summary description for Form1.
	/// </summary>
	public class frmFloatDialog : PNNL.Controls.frmDialogBase
	{
		private float mEditingValue;
		private System.Windows.Forms.Label mPromptLabel;
		private System.Windows.Forms.TextBox mFloatInputTextBox;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public frmFloatDialog()
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
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(frmFloatDialog));
			this.mPromptLabel = new System.Windows.Forms.Label();
			this.mFloatInputTextBox = new System.Windows.Forms.TextBox();
			this.SuspendLayout();
			// 
			// mPromptLabel
			// 
			this.mPromptLabel.AccessibleDescription = resources.GetString("mPromptLabel.AccessibleDescription");
			this.mPromptLabel.AccessibleName = resources.GetString("mPromptLabel.AccessibleName");
			this.mPromptLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("mPromptLabel.Anchor")));
			this.mPromptLabel.AutoSize = ((bool)(resources.GetObject("mPromptLabel.AutoSize")));
			this.mPromptLabel.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("mPromptLabel.Dock")));
			this.mPromptLabel.Enabled = ((bool)(resources.GetObject("mPromptLabel.Enabled")));
			this.mPromptLabel.Font = ((System.Drawing.Font)(resources.GetObject("mPromptLabel.Font")));
			this.mPromptLabel.Image = ((System.Drawing.Image)(resources.GetObject("mPromptLabel.Image")));
			this.mPromptLabel.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("mPromptLabel.ImageAlign")));
			this.mPromptLabel.ImageIndex = ((int)(resources.GetObject("mPromptLabel.ImageIndex")));
			this.mPromptLabel.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("mPromptLabel.ImeMode")));
			this.mPromptLabel.Location = ((System.Drawing.Point)(resources.GetObject("mPromptLabel.Location")));
			this.mPromptLabel.Name = "mPromptLabel";
			this.mPromptLabel.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("mPromptLabel.RightToLeft")));
			this.mPromptLabel.Size = ((System.Drawing.Size)(resources.GetObject("mPromptLabel.Size")));
			this.mPromptLabel.TabIndex = ((int)(resources.GetObject("mPromptLabel.TabIndex")));
			this.mPromptLabel.Text = resources.GetString("mPromptLabel.Text");
			this.mPromptLabel.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("mPromptLabel.TextAlign")));
			this.mPromptLabel.Visible = ((bool)(resources.GetObject("mPromptLabel.Visible")));
			// 
			// mFloatInputTextBox
			// 
			this.mFloatInputTextBox.AccessibleDescription = resources.GetString("mFloatInputTextBox.AccessibleDescription");
			this.mFloatInputTextBox.AccessibleName = resources.GetString("mFloatInputTextBox.AccessibleName");
			this.mFloatInputTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("mFloatInputTextBox.Anchor")));
			this.mFloatInputTextBox.AutoSize = ((bool)(resources.GetObject("mFloatInputTextBox.AutoSize")));
			this.mFloatInputTextBox.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("mFloatInputTextBox.BackgroundImage")));
			this.mFloatInputTextBox.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("mFloatInputTextBox.Dock")));
			this.mFloatInputTextBox.Enabled = ((bool)(resources.GetObject("mFloatInputTextBox.Enabled")));
			this.mFloatInputTextBox.Font = ((System.Drawing.Font)(resources.GetObject("mFloatInputTextBox.Font")));
			this.mFloatInputTextBox.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("mFloatInputTextBox.ImeMode")));
			this.mFloatInputTextBox.Location = ((System.Drawing.Point)(resources.GetObject("mFloatInputTextBox.Location")));
			this.mFloatInputTextBox.MaxLength = ((int)(resources.GetObject("mFloatInputTextBox.MaxLength")));
			this.mFloatInputTextBox.Multiline = ((bool)(resources.GetObject("mFloatInputTextBox.Multiline")));
			this.mFloatInputTextBox.Name = "mFloatInputTextBox";
			this.mFloatInputTextBox.PasswordChar = ((char)(resources.GetObject("mFloatInputTextBox.PasswordChar")));
			this.mFloatInputTextBox.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("mFloatInputTextBox.RightToLeft")));
			this.mFloatInputTextBox.ScrollBars = ((System.Windows.Forms.ScrollBars)(resources.GetObject("mFloatInputTextBox.ScrollBars")));
			this.mFloatInputTextBox.Size = ((System.Drawing.Size)(resources.GetObject("mFloatInputTextBox.Size")));
			this.mFloatInputTextBox.TabIndex = ((int)(resources.GetObject("mFloatInputTextBox.TabIndex")));
			this.mFloatInputTextBox.Text = resources.GetString("mFloatInputTextBox.Text");
			this.mFloatInputTextBox.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("mFloatInputTextBox.TextAlign")));
			this.mFloatInputTextBox.Visible = ((bool)(resources.GetObject("mFloatInputTextBox.Visible")));
			this.mFloatInputTextBox.WordWrap = ((bool)(resources.GetObject("mFloatInputTextBox.WordWrap")));
			this.mFloatInputTextBox.Validating += new System.ComponentModel.CancelEventHandler(this.mFloatInputTextBox_Validating);
			// 
			// frmFloatDialog
			// 
			this.AccessibleDescription = resources.GetString("$this.AccessibleDescription");
			this.AccessibleName = resources.GetString("$this.AccessibleName");
			this.AutoScaleBaseSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScaleBaseSize")));
			this.AutoScroll = ((bool)(resources.GetObject("$this.AutoScroll")));
			this.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMargin")));
			this.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMinSize")));
			this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
			this.ClientSize = ((System.Drawing.Size)(resources.GetObject("$this.ClientSize")));
			this.Controls.Add(this.mFloatInputTextBox);
			this.Controls.Add(this.mPromptLabel);
			this.Enabled = ((bool)(resources.GetObject("$this.Enabled")));
			this.Font = ((System.Drawing.Font)(resources.GetObject("$this.Font")));
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("$this.ImeMode")));
			this.Location = ((System.Drawing.Point)(resources.GetObject("$this.Location")));
			this.MaximumSize = ((System.Drawing.Size)(resources.GetObject("$this.MaximumSize")));
			this.MinimumSize = ((System.Drawing.Size)(resources.GetObject("$this.MinimumSize")));
			this.Name = "frmFloatDialog";
			this.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("$this.RightToLeft")));
			this.StartPosition = ((System.Windows.Forms.FormStartPosition)(resources.GetObject("$this.StartPosition")));
			this.Text = resources.GetString("$this.Text");
			this.Controls.SetChildIndex(this.mPromptLabel, 0);
			this.Controls.SetChildIndex(this.mFloatInputTextBox, 0);
			this.ResumeLayout(false);

		}
		#endregion

		private void mFloatInputTextBox_Validating(object sender, System.ComponentModel.CancelEventArgs e)
		{
			try 
			{
				this.mEditingValue = float.Parse(this.mFloatInputTextBox.Text);
			} 
			catch (Exception ex) 
			{
				Console.WriteLine(ex.Message + ex.StackTrace) ; 
			}
		}

		public String Prompt 
		{
			get 
			{
				return mPromptLabel.Text;
			}
			set 
			{
				mPromptLabel.Text = value;
			}
		}

		public float EditingValue 
		{
			get 
			{
				return mEditingValue;
			}
			set 
			{
				mEditingValue = value;
				this.mFloatInputTextBox.Text = value.ToString();
			}
		}
	}
}
