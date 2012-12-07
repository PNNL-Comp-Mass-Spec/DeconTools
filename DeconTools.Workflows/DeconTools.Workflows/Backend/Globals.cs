
namespace DeconTools.Workflows.Backend
{

    public enum ValidationCode
    {
        None,
        Yes,
        No,
        Maybe
    }
    
    public class Globals
    {


        public enum TargetedWorkflowTypes
        {
            Undefined,
            UnlabelledTargeted1,
            O16O18Targeted1,
            N14N15Targeted1,
            TargetedAlignerWorkflow1, 
            PeakDetectAndExportWorkflow1,
            SipperTargeted1,
            BasicTargetedWorkflowExecutor1,
            LcmsFeatureTargetedWorkflowExecutor1,
            SipperWorkflowExecutor1,
			TopDownTargeted1,
			TopDownTargetedWorkflowExecutor1,
			UIMFTargetedMSMSWorkflowCollapseIMS
        }


        public enum TargetType
        {
            LcmsFeature,
            DatabaseTarget
        }


       



    }
}
