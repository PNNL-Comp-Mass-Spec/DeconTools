using DeconTools.Backend.ProcessingTasks;
using DeconTools.Workflows.Backend.Core;

namespace DeconTools.Workflows.Backend.Core
{
    public class O16O18WorkflowParameters : TargetedWorkflowParameters
    {

        #region Constructors
        public O16O18WorkflowParameters()
        {
            this.ChromGeneratorMode = ChromatogramGeneratorMode.O16O18_THREE_MONOPEAKS;
            this.ChromNETTolerance = 0.025;
            this.ChromPeakDetectorPeakBR = 2;
            this.ChromPeakDetectorSigNoise = 2;
            this.ChromSmootherNumPointsInSmooth = 15;
            this.ChromToleranceInPPM = 10;
            this.MSPeakDetectorPeakBR = 2;
            this.MSPeakDetectorSigNoise = 2;
            this.MSToleranceInPPM = 10;
            this.NumMSScansToSum = 1;
            

            this.ChromGenSourceDataPeakBR = 2;
            this.ChromGenSourceDataSigNoise = 3;

            this.ResultType = DeconTools.Backend.Globals.ResultType.O16O18_TARGETED_RESULT;

        }
        #endregion


        public override Globals.TargetedWorkflowTypes WorkflowType
        {
            get { return Globals.TargetedWorkflowTypes.O16O18Targeted1; }
        }
    }
}
