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
    /// Summary description for frmStatus.
    /// </summary>
    public class frmStatus : System.Windows.Forms.Form
    {
        private System.Windows.Forms.Label lblProgress;
        private System.Windows.Forms.ProgressBar mbar_progress;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button mbtn_cancel;

        private clsMediator mobj_mediator ; 

        private System.Threading.ManualResetEvent mevnt_init = new System.Threading.ManualResetEvent(false);
        private System.Threading.ManualResetEvent mevnt_abort = new System.Threading.ManualResetEvent(false);
        private System.Windows.Forms.Label mlbl_status;

        private delegate void SetControlString(string status) ; 
        private delegate void SetProgressValue() ; 
        private delegate void NonArgFunc() ; 
        private int mint_percent_done = 0 ; 
        private int mint_step_size = 2 ; 

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        public frmStatus()
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            //
            // TODO: Add any constructor code after InitializeComponent call
            //
            mbar_progress.Minimum = 0 ; 
            mbar_progress.Maximum = 100 ; 
            mbar_progress.Step = mint_step_size ; 
        }

        public frmStatus(clsMediator mediator)
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            //
            // TODO: Add any constructor code after InitializeComponent call
            //
            HookMediator(mediator) ; 
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
            this.lblProgress = new System.Windows.Forms.Label();
            this.mbar_progress = new System.Windows.Forms.ProgressBar();
            this.label1 = new System.Windows.Forms.Label();
            this.mbtn_cancel = new System.Windows.Forms.Button();
            this.mlbl_status = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // lblProgress
            // 
            this.lblProgress.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
            this.lblProgress.Location = new System.Drawing.Point(8, 16);
            this.lblProgress.Name = "lblProgress";
            this.lblProgress.Size = new System.Drawing.Size(72, 16);
            this.lblProgress.TabIndex = 0;
            this.lblProgress.Text = "Progress:";
            // 
            // mbar_progress
            // 
            this.mbar_progress.Location = new System.Drawing.Point(72, 16);
            this.mbar_progress.Name = "mbar_progress";
            this.mbar_progress.Size = new System.Drawing.Size(176, 16);
            this.mbar_progress.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
            this.label1.Location = new System.Drawing.Point(8, 40);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(48, 16);
            this.label1.TabIndex = 2;
            this.label1.Text = "Status:";
            // 
            // mbtn_cancel
            // 
            this.mbtn_cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.mbtn_cancel.Location = new System.Drawing.Point(96, 80);
            this.mbtn_cancel.Name = "mbtn_cancel";
            this.mbtn_cancel.Size = new System.Drawing.Size(80, 24);
            this.mbtn_cancel.TabIndex = 3;
            this.mbtn_cancel.Text = "Cancel";
            this.mbtn_cancel.Click += new System.EventHandler(this.mbtn_cancel_Click);
            // 
            // mlbl_status
            // 
            this.mlbl_status.Location = new System.Drawing.Point(72, 40);
            this.mlbl_status.Name = "mlbl_status";
            this.mlbl_status.Size = new System.Drawing.Size(176, 24);
            this.mlbl_status.TabIndex = 4;
            // 
            // frmStatus
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.BackColor = System.Drawing.SystemColors.Control;
            this.CancelButton = this.mbtn_cancel;
            this.ClientSize = new System.Drawing.Size(274, 113);
            this.ControlBox = false;
            this.Controls.Add(this.mlbl_status);
            this.Controls.Add(this.mbtn_cancel);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.mbar_progress);
            this.Controls.Add(this.lblProgress);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmStatus";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Status";
            this.ResumeLayout(false);

        }
        #endregion

        private void HookMediator(clsMediator mediator)
        {
            mobj_mediator = mediator ; 
            mobj_mediator.mevnt_Progress +=new Decon2LS.clsMediator.dlgMediatorProgressHandler(OnProgressMessage);
            mobj_mediator.mevnt_Status += new Decon2LS.clsMediator.dlgMediatorStatusHandler(OnStatusMessage);
        }

        private void OnProgressMessage(object sender, object event_args)
        {
            try
            {
                if (!IsHandleCreated)
                    return ; 
                var percent_done = (int) event_args ; 
                Console.WriteLine(Convert.ToString(percent_done)) ; 
                // if the new status is greater, or if its been reset to a newer value then update
                if (mint_percent_done + mint_step_size < percent_done || mint_percent_done > percent_done)
                {
                    mint_percent_done = percent_done ; 
                    Invoke(new SetProgressValue(this.SetProgressVal), null) ; 
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + e.StackTrace) ; 
            }
        }

        /// <summary>
        /// Prevent this form from ever being closed.  It can be hidden/shown, but not closed.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnClosing(CancelEventArgs e)
        {
            try
            {
                base.OnClosing(e);
                e.Cancel = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + ex.StackTrace) ; 
            }
        }


        private void SetProgressVal()
        {
            try
            {
                mbar_progress.Value = mint_percent_done ; 
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + ex.StackTrace) ; 
            }
        }

        public void Reset()
        {
            try
            {
                mint_percent_done = 0 ; 
                SetProgressVal() ;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + ex.StackTrace) ; 
            }
        }
        private void OnStatusMessage(object sender, object event_args)
        {
            try
            {
                if (!IsHandleCreated)
                    return ; 
                var status_str = (string) event_args ; 
                Invoke(new SetControlString(this.SetStatusString), new object [] {status_str}) ; 
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString()) ; 
            }
        }

        private void SetStatusString(string txt)
        {
            try
            {
                this.mlbl_status.Text = txt ; 
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + ex.StackTrace) ; 
            }
        }

        private void ShowStatusBox(object sender, object event_args)
        {
            try
            {
                this.Text = "Loading file " + (string) event_args ;
                this.mint_percent_done = 0 ;
                mbar_progress.Value = 0 ;
                ShowDialog() ; 
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + ex.StackTrace) ; 
            }
        }

        private void mbtn_cancel_Click(object sender, System.EventArgs e)
        {
            try
            {
                this.DialogResult = DialogResult.Cancel;
                this.Hide();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + ex.StackTrace) ; 
            }
        }
    }
}
