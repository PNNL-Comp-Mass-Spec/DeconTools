using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core;

namespace DeconTools.Backend.Workflows
{
    public abstract class WorkflowBase
    {

        string Name { get; set; }

        #region Public Methods

        public abstract void InitializeWorkflow();

        public abstract void Execute();

        
        public virtual IList<ChromPeak> ChromPeaksDetected { get; set; }

        public virtual ChromPeak ChromPeakSelected { get; set; }

        public virtual XYData MassSpectrumXYData { get; set; }

        public virtual XYData ChromatogramXYData { get; set; }

        public Run Run { get; set; }

        public MassTagResultBase Result { get; set; }


        //public static WorkflowBase CreateWorkflow(string workflowParameterFilename)
        //{



        //}


        #endregion
    }
}
