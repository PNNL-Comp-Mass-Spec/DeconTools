
namespace DeconTools.Workflows.Backend.Core
{
    public class TargetedAlignerWorkflowParameters:TargetedWorkflowParameters
    {
        #region Constructors
        public TargetedAlignerWorkflowParameters()
        {
            ChromGeneratorMode = DeconTools.Backend.Globals.ChromatogramGeneratorMode.MOST_ABUNDANT_PEAK;
            ChromNETTolerance = 0.2;    //wide NET tolerance
            ChromPeakDetectorPeakBR = 2;
            ChromPeakDetectorSigNoise = 2;
            ChromSmootherNumPointsInSmooth = 9;
            ChromGenTolerance = 25;
            MSPeakDetectorPeakBR = 2;
            MSPeakDetectorSigNoise = 2;
            MSToleranceInPPM = 25;
            NumMSScansToSum = 1;

            ChromGenSourceDataPeakBR = 2;
            ChromGenSourceDataSigNoise = 3;

            NumDesiredMassTagsPerNETGrouping = 25;
            NumMaxAttemptsPerNETGrouping = 200;
            NumMaxAttemptsDuringFirstPassMassAnalysis = 2000;

            NumChromPeaksAllowedDuringSelection = 1;

            UpperFitScoreAllowedCriteria = 0.1;
            MinimumChromPeakIntensityCriteria = 2.5e5f;
            IScoreAllowedCriteria = 0.15;
            MultipleHighQualityMatchesAreAllowed = false;
        }

        #endregion

        #region Properties
        public double UpperFitScoreAllowedCriteria { get; set; }

        public double IScoreAllowedCriteria { get; set; }

        public float MinimumChromPeakIntensityCriteria { get; set; }

        public int NumDesiredMassTagsPerNETGrouping { get; set; }

        public int NumMaxAttemptsPerNETGrouping { get; set; }

        public string ImportedFeaturesFilename { get; set; }

        public override Globals.TargetedWorkflowTypes WorkflowType => Globals.TargetedWorkflowTypes.TargetedAlignerWorkflow1;

        /// <summary>
        /// In first pass of Targeted Aligner we use a very wide mass tolerance and
        /// wide NET. This parameter will limit the attempts to find suitable targets for the initial
        /// narrowing of mass alignment
        /// </summary>
        public int NumMaxAttemptsDuringFirstPassMassAnalysis { get; set; }

        #endregion

    }
}
