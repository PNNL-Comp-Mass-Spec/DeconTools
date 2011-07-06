
namespace DeconTools.Workflows.Backend.Core
{
    public class TargetedAlignerWorkflowParameters:DeconToolsTargetedWorkflowParameters
    {

        #region Constructors
        public TargetedAlignerWorkflowParameters()
        {
            this.ChromGeneratorMode = DeconTools.Backend.ProcessingTasks.ChromatogramGeneratorMode.MOST_ABUNDANT_PEAK;
            this.ChromNETTolerance = 0.2;    //wide NET tolerance
            this.ChromPeakDetectorPeakBR = 2;
            this.ChromPeakDetectorSigNoise = 2;
            this.ChromSmootherNumPointsInSmooth = 9;
            this.ChromToleranceInPPM = 25;
            this.MSPeakDetectorPeakBR = 2;
            this.MSPeakDetectorSigNoise = 2;
            this.MSToleranceInPPM = 25;
            this.NumMSScansToSum = 1;


            this.ChromGenSourceDataPeakBR = 2;
            this.ChromGenSourceDataSigNoise = 3;
        }

        #endregion

        #region Properties

        #endregion

        #region Public Methods

        #endregion

        #region Private Methods

        #endregion

        public override string WorkflowType
        {
            get { return "TargetedAlignerWorkflow1"; }
        }

        public override void LoadParameters(string xmlFilename)
        {
            throw new System.NotImplementedException();
        }
    }
}
