
namespace DeconTools.Workflows.Backend.Core
{
    public class TargetedAlignerWorkflowParameters:TargetedWorkflowParameters
    {

        #region Constructors
        public TargetedAlignerWorkflowParameters()
        {
            this.ChromGeneratorMode = DeconTools.Backend.Globals.ChromatogramGeneratorMode.MOST_ABUNDANT_PEAK;
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



            this.NumDesiredMassTagsPerNETGrouping = 25;
            this.NumMaxAttemptsPerNETGrouping = 200;

            this.NumChromPeaksAllowedDuringSelection = 1;

            
            this.UpperFitScoreAllowedCriteria = 0.1;
            this.MinimumChromPeakIntensityCriteria = 2.5e5f;
            this.IScoreAllowedCriteria = 0.15;
            this.MultipleHighQualityMatchesAreAllowed = false;

            

        }

        #endregion

        #region Properties
        public double UpperFitScoreAllowedCriteria { get; set; }

        public double IScoreAllowedCriteria { get; set; }

        public float MinimumChromPeakIntensityCriteria { get; set; }

        public int NumDesiredMassTagsPerNETGrouping { get; set; }

        public int NumMaxAttemptsPerNETGrouping { get; set; }

      

        

        public string ImportedFeaturesFilename { get; set; }

        public override Globals.TargetedWorkflowTypes WorkflowType
        {
            get { return  Globals.TargetedWorkflowTypes.TargetedAlignerWorkflow1; }
        }

        #endregion

         

    }
}
