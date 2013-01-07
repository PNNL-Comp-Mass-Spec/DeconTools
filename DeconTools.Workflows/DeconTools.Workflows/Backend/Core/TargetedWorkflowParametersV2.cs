using System;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend.ProcessingTasks.ChromatogramProcessing;

namespace DeconTools.Workflows.Backend.Core
{
    public class TargetedWorkflowParametersV2 : TargetedWorkflowParameters
    {

        #region Constructors

        public TargetedWorkflowParametersV2()
        {
            IsotopeProfileType = DeconTools.Backend.Globals.LabellingType.NONE;
            IsotopeLabelingEfficiency = 1;
        }

        #endregion

        #region Properties
        
        /// <summary>
        /// sets which type of isotope profile to use
        /// </summary>
        public DeconTools.Backend.Globals.LabellingType IsotopeProfileType { get; set; }

        /// <summary>
        /// LabelingEfficiency Ranges between 0 and 1.0
        /// </summary>
        public double IsotopeLabelingEfficiency { get; set; }

        #endregion

        public override Globals.TargetedWorkflowTypes WorkflowType
        {
            get { throw new NotImplementedException(); }
        }
    }
}
