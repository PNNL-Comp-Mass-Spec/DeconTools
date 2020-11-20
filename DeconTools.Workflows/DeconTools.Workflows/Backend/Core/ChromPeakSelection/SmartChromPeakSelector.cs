using System;
using DeconTools.Backend.ProcessingTasks.FitScoreCalculators;
using DeconTools.Backend.ProcessingTasks.PeakDetectors;
using DeconTools.Backend.ProcessingTasks.ResultValidators;
using DeconTools.Backend.ProcessingTasks.TargetedFeatureFinders;

namespace DeconTools.Workflows.Backend.Core.ChromPeakSelection
{
    public class SmartChromPeakSelector : SmartChromPeakSelectorBase
    {
        #region Constructors
        public SmartChromPeakSelector(SmartChromPeakSelectorParameters parameters)
        {
            Parameters = parameters;

            MSPeakDetector = new DeconToolsPeakDetectorV2(parameters.MSPeakDetectorPeakBR, parameters.MSPeakDetectorSigNoiseThresh, DeconTools.Backend.Globals.PeakFitType.QUADRATIC, true);

            var iterativeTFFParams = new IterativeTFFParameters();
            iterativeTFFParams.ToleranceInPPM = parameters.MSToleranceInPPM;
            iterativeTFFParams.MinimumRelIntensityForForPeakInclusion = parameters.IterativeTffMinRelIntensityForPeakInclusion;

            if (parameters.MSFeatureFinderType == DeconTools.Backend.Globals.TargetedFeatureFinderType.BASIC)
            {
                throw new NotSupportedException("Currently the Basic TFF is not supported in the SmartChromPeakSelector");

                //TargetedMSFeatureFinder = new TargetedFeatureFinders.BasicTFF(parameters.MSToleranceInPPM);
            }
            else
            {
                TargetedMSFeatureFinder = new IterativeTFF(iterativeTFFParams);
            }

            resultValidator = new ResultValidatorTask();
            fitScoreCalc = new IsotopicProfileFitScoreCalculator();

            InterferenceScorer = new InterferenceScorer();
        }

        #endregion

    }
}
