// -------------------------------------------------------------------------------
// Written by Navdeep Jaitly for the Department of Energy (PNNL, Richland, WA)
// Copyright 2006, Battelle Memorial Institute
// E-mail: navdeep.jaitly@pnl.gov
// Website: http://ncrr.pnl.gov/software
// -------------------------------------------------------------------------------
// 
// Licensed under the Apache License, Version 2.0; you may not use this file except
// in compliance with the License.  You may obtain a copy of the License at 
// http://www.apache.org/licenses/LICENSE-2.0

using System;
using System.Data; 
using System.Windows.Forms;
using PNNL.Controls ;
using System.Collections;

namespace Decon2LS
{

    public interface IMediatedForm 
    {
        clsMediator Mediator 
        {
            get;
        }
    }

    /// <summary>
    /// Summary description for clsMediator.
    /// </summary>
    public class clsTransformResultsEventArgs : EventArgs
    {
        public DeconToolsV2.HornTransform.clsHornTransformResults []marr_records ; 
        public clsTransformResultsEventArgs(ref DeconToolsV2.HornTransform.clsHornTransformResults [] transform_results)
        {
            marr_records = transform_results ; 
        }
    }

    public class clsZoomEventArgs : EventArgs
    {
        public DeconToolsV2.HornTransform.clsHornTransformResults mobj_record ; 
        public clsZoomEventArgs(DeconToolsV2.HornTransform.clsHornTransformResults record)
        {
            mobj_record = record ; 
        }
    }

    public class clsNewScanEventArgs : EventArgs
    {
        public int mint_scan_num ; 
        public clsNewScanEventArgs(int scan_num) 
        {
            mint_scan_num = scan_num ; 
        }
    }
    public class clsMediator : ICategorizedItemNotifier
    {
        public delegate void dlgMediatorEventHandler(object sender, object event_args) ; 
        public delegate void dlgMediatorFileOpenHandler(object sender, object event_args) ; 
        public delegate void dlgMediatorProgressHandler(object sender, object event_args) ; 
        public delegate void dlgMediatorStatusHandler(object sender, object event_args) ;
        public delegate void dlgMediatorTransformResultsHandler(object sender, clsTransformResultsEventArgs event_args) ;
        public delegate void dlgMediatorZoomHandler(object sender, clsZoomEventArgs event_args) ;
        public delegate void dlgMediatorNewScanHandler(object sender, clsNewScanEventArgs event_args) ;
        
        private clsMediator parent = null;

        public event dlgMediatorFileOpenHandler mevnt_FileOpen ;
        public event dlgMediatorProgressHandler mevnt_Progress ; 
        public event dlgMediatorStatusHandler mevnt_Status ;
        public event dlgMediatorTransformResultsHandler mevnt_MassTransform_Complete ;
        public event dlgMediatorZoomHandler mevnt_Zoom ;
        

        private System.Windows.Forms.Form mfrm_main = new frmStatus(); 
        private Decon2LS.frmStatus mfrm_status ;

        /// <summary>
        /// ICategorizedItemNotifier members
        /// </summary>
        public event ItemChangedHandler ItemOpen;
        public event ItemChangedHandler ItemClose;

        public frmStatus StatusForm
        {
            get
            {
                return mfrm_status ; 
            }
        }

        public System.Windows.Forms.Form MainForm
        {
            get
            {
                return mfrm_main ; 
            }
        }

        public clsMediator(System.Windows.Forms.Form main_form)
        {
            //
            // TODO: Add constructor logic here
            //
            mfrm_main = main_form ; 
            mfrm_status = new frmStatus(this);
        }

        public clsMediator(System.Windows.Forms.Form main_form, clsMediator parent) : this(main_form)
        {
            this.Parent = parent;
        }
        
        public clsMediator Parent 
        {
            get 
            {
                return parent;
            }
            set 
            {
                this.parent = value;
            }
        }

        public void RaiseFileOpen(object sender, object event_args, ref bool Cancel)
        {
            try
            {
                if (mevnt_FileOpen != null)
                {
                    mevnt_FileOpen(sender, event_args) ; 
                }
                if (parent != null) 
                {
                    parent.RaiseFileOpen(sender, event_args, ref Cancel);
                }
            }
            catch (Exception e )
            {
                Console.WriteLine(e.Message + e.StackTrace) ; 
            }
        }

        public void RaiseStatusMessage(object sender, object event_args, ref bool Cancel)
        {
            try
            {
                if (mevnt_Status != null)
                {
                    mevnt_Status(sender, event_args) ; 
                }
                if (parent != null) 
                {
                    parent.RaiseStatusMessage(sender, event_args, ref Cancel);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + e.StackTrace) ; 
            }
        }

        public void RaiseProgressMessage(object sender, object event_args, ref bool Cancel)
        {
            try
            {
                if (mevnt_Status != null)
                {
                    mevnt_Progress(sender, event_args) ; 
                }
                if (parent != null) 
                {
                    parent.RaiseProgressMessage(sender, event_args, ref Cancel);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + e.StackTrace) ; 
            }
        }

        public void RaiseItemOpen(object sender, ICategorizedItem item) 
        {
            PNNL.Controls.ItemChangedEventArgs event_args = new PNNL.Controls.ItemChangedEventArgs(item);
            if (ItemOpen != null) 
            {
                ItemOpen(sender, event_args);
            }
            if (parent != null) 
            {
                parent.RaiseItemOpen(sender, item);
            }
        }

        public void RaiseItemClose(object sender, ICategorizedItem item) 
        {
            PNNL.Controls.ItemChangedEventArgs event_args = new PNNL.Controls.ItemChangedEventArgs(item);
            if (ItemClose != null) 
            {
                ItemClose(sender, event_args);
            }
            if (parent != null) 
            {
                parent.RaiseItemClose(sender, item);
            }
        }

        public void RaiseMassTransformComplete(object sender, ref DeconToolsV2.HornTransform.clsHornTransformResults []transform_results)
        {
            if (mevnt_MassTransform_Complete != null)
            {
                mevnt_MassTransform_Complete(sender, new clsTransformResultsEventArgs(ref transform_results)) ; 
            }
            if (parent != null) 
            {
                parent.RaiseMassTransformComplete(sender, ref transform_results);
            }
        }

        public void RaiseZoom(object sender, clsZoomEventArgs event_args)
        {
            if (mevnt_Zoom != null)
            {
                mevnt_Zoom(sender, event_args) ; 
            }
            if (parent != null) 
            {
                parent.RaiseZoom(sender, event_args);
            }
        }

        /// <summary>
        /// Opens a form in the application.
        /// </summary>
        /// <param name="form"></param>
        public virtual void RequestFormOpen(Form form) 
        {
            if (parent != null) 
            {
                parent.RequestFormOpen(form);
            }
        }


        /// <summary>
        /// Requests the last active form of a given type.
        /// </summary>
        /// <param name="type"></param>
        public virtual Form RequestLastActiveFormForType(Type type) 
        {
            if (parent != null) 
            {
                return parent.RequestLastActiveFormForType(type);
            }
            return null;
        }
    }

    public class Decon2LSMediator : clsMediator 
    {
        private EventHandler mFormClosedHandler;
        private Hashtable mLastActiveFormByType = new Hashtable(); 

        public Decon2LSMediator(frmDecon2LS form) : base(form) 
        {
            mFormClosedHandler = new EventHandler(this.FormClosed);
            ArrayList list = new ArrayList();
            list.Add(form);
            mLastActiveFormByType[form.GetType()] = list;
            form.MdiChildActivate += new EventHandler(MainForm_MdiChildActivate);
        }

        public override void RequestFormOpen(Form form)
        {
            if (form is IMediatedForm) 
            {
                ((IMediatedForm) form).Mediator.Parent = this;
            }
            form.MdiParent = this.MainForm;
            // attach handler to close event of form
            form.Closed += mFormClosedHandler;
            form.Show();
            // Raise item event if its a categorized item.
            if (form is ICategorizedItem) 
            {
                this.RaiseItemOpen(this, (ICategorizedItem) form);
            }
        }

        public override Form RequestLastActiveFormForType(Type type)
        {
            ArrayList formsForType = (ArrayList) mLastActiveFormByType[type];
            if (formsForType == null || formsForType.Count <= 0) 
            {
                return null;
            }
            return (Form) formsForType[0];
        }


        /// <summary>
        /// Notified when a form opened via the RequestFormOpen method is closed.  
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FormClosed(object sender, EventArgs e)
        {
            Form form = (Form) sender;
            if (form is ICategorizedItem) 
            {
                this.RaiseItemClose(this, (ICategorizedItem) form);
            }
            ArrayList formList = (ArrayList) mLastActiveFormByType[sender.GetType()];
            if (formList != null) 
            {
                formList.Remove(sender);
            }
            // Remove closed event handler from form
            form.Closed -= mFormClosedHandler ;
        }

        private void MainForm_MdiChildActivate(object sender, EventArgs e)
        {
            Form activeMdiChild = this.MainForm.ActiveMdiChild;
            if (activeMdiChild != null) 
            {
                ArrayList formList = (ArrayList) mLastActiveFormByType[activeMdiChild.GetType()];
                if (formList == null) 
                {
                    formList = new ArrayList();
                    mLastActiveFormByType[activeMdiChild.GetType()] = formList;
                } 
                else 
                {
                    formList.Remove(activeMdiChild);
                }
                formList.Insert(0, activeMdiChild);
            }
        }
    }
}
