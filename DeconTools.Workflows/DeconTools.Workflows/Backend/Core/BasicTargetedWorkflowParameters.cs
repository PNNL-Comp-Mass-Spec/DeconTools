using DeconTools.Backend.ProcessingTasks;

namespace DeconTools.Workflows.Backend.Core
{
    public class BasicTargetedWorkflowParameters:DeconToolsTargetedWorkflowParameters
    {
        public BasicTargetedWorkflowParameters()
        {
            this.ChromGeneratorMode = ChromatogramGeneratorMode.MOST_ABUNDANT_PEAK;
            this.ChromNETTolerance = 0.025;
            this.ChromPeakDetectorPeakBR = 1;
            this.ChromPeakDetectorSigNoise = 1;
            this.ChromSmootherNumPointsInSmooth = 9;
            this.ChromToleranceInPPM = 10;
            this.MSPeakDetectorPeakBR = 2;
            this.MSPeakDetectorSigNoise = 2;
            this.MSToleranceInPPM = 10;
            this.NumMSScansToSum = 1;


            this.ChromGenSourceDataPeakBR = 2;
            this.ChromGenSourceDataSigNoise = 3;

            this.ResultType = DeconTools.Backend.Globals.MassTagResultType.BASIC_MASSTAG_RESULT;
        }

        public override string WorkflowType
        {
            get
            {
                return "UnlabelledTargeted1";
            }
        }

    }
}
