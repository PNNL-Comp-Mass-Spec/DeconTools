using System.Collections.Generic;
using DeconTools.Backend;
using DeconTools.Backend.Core;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend.ProcessingTasks.FitScoreCalculators;
using DeconTools.Backend.ProcessingTasks.PeakDetectors;
using DeconTools.Backend.ProcessingTasks.Quantifiers;
using DeconTools.Backend.ProcessingTasks.ResultValidators;
using DeconTools.Backend.ProcessingTasks.Smoothers;
using DeconTools.Backend.ProcessingTasks.TargetedFeatureFinders;
using DeconTools.Backend.ProcessingTasks.TheorFeatureGenerator;
using DeconTools.Workflows.Backend.Core.ChromPeakSelection;

namespace DeconTools.Workflows.Backend.Core
{
    public sealed class N14N15Workflow2 : TargetedWorkflow
    {
        private JoshTheorFeatureGenerator theorFeatureGen;
        private JoshTheorFeatureGenerator theorN15FeatureGen;

        private PeakChromatogramGenerator chromGenN14;
        private PeakChromatogramGenerator chromGenN15;
        private SavitzkyGolaySmoother chromSmoother;
        private ChromPeakDetector chromPeakDetector;
        private SmartChromPeakSelector chromPeakSelectorN14;
        private BasicChromPeakSelector chromPeakSelectorN15;

        private DeconToolsPeakDetectorV2 msPeakDetector;

        private IterativeTFF labeledProfileFinder;
        private IterativeTFF unlabeledProfilefinder;

        private N14N15QuantifierTask quantifier;

        private IsotopicProfileFitScoreCalculator fitScoreCalc;

        private ResultValidatorTask resultValidatorN14;

        private LabeledIsotopicProfileScorer resultValidatorN15;

        private N14N15Workflow2Parameters _n14N15Workflow2Parameters => WorkflowParameters as N14N15Workflow2Parameters;

        #region Constructors

        public N14N15Workflow2(Run run, TargetedWorkflowParameters parameters) : base(run,parameters)
        {
            MsLeftTrimAmount = 5;
            MsRightTrimAmount = 5;
        }

        public N14N15Workflow2(TargetedWorkflowParameters parameters)
            : this(null, parameters)
        {
        }

        #endregion

        #region Workflow Members

        public override void DoWorkflow()
        {
            Result = Run.ResultCollection.GetTargetedResult(Run.CurrentMassTag);
            Result.ResetResult();

            ExecuteTask(theorFeatureGen);
            ExecuteTask(theorN15FeatureGen);
            ExecuteTask(chromGenN14);
            ExecuteTask(chromSmoother);
            updateChromDataXYValues(Run.XYData);

            ExecuteTask(chromPeakDetector);
            UpdateChromDetectedPeaks(Run.PeakList);

            ExecuteTask(chromPeakSelectorN14);
            ChromPeakSelected = Result.ChromPeakSelected;
            chromPeakSelectorN15.ReferenceNETValueForReferenceMode = Result.GetNET();    //so that the NET value of the N14 result can be used to help find the N15 chrom peak

            ExecuteTask(MSGenerator);
            updateMassSpectrumXYValues(Run.XYData);

            ExecuteTask(msPeakDetector);
            ExecuteTask(unlabeledProfilefinder);

            ExecuteTask(fitScoreCalc);
            ExecuteTask(resultValidatorN14);

            //a bit of a hack... but we need to declare that the Result isn't failed so that the following tasks will be performed
            Result.FailedResult = false;

            //now process the N15 profile

            ExecuteTask(chromGenN15);
            ExecuteTask(chromSmoother);

            ExecuteTask(chromPeakDetector);

            ExecuteTask(chromPeakSelectorN15);

            //even if we don't find anything, we want to create a mass spectrum and pull out values of N15 data
            var n14n15result = (N14N15_TResult)Result;
            if (n14n15result.ChromPeakSelectedN15 == null)
            {
                n14n15result.ScanSetForN15Profile = Result.ScanSet;
                Run.CurrentScanSet = n14n15result.ScanSetForN15Profile;

                if (n14n15result.ScanSetForN15Profile == null)
                {
                    Result.FailedResult = true;
                    Result.FailureType = DeconTools.Backend.Globals.TargetedResultFailureType.ChrompeakNotFoundWithinTolerances;
                }
            }

            ExecuteTask(MSGenerator);
            updateMassSpectrumXYValues(Run.XYData);

            //TrimData(Run.XYData, Run.CurrentMassTag.MZ, MsLeftTrimAmount, MsRightTrimAmount);

            ExecuteTask(msPeakDetector);
            ExecuteTask(labeledProfileFinder);

            resultValidatorN15.CurrentResult = Result;

            ExecuteTask(resultValidatorN15);
            ExecuteTask(quantifier);
        }

        protected override void DoPostInitialization()
        {
            base.DoPostInitialization();
            ValidateParameters();

            theorFeatureGen = new JoshTheorFeatureGenerator(DeconTools.Backend.Globals.LabelingType.NONE, 0.005);
            theorN15FeatureGen = new JoshTheorFeatureGenerator(DeconTools.Backend.Globals.LabelingType.N15, 0.005);

            chromGenN14 = new PeakChromatogramGenerator(
                _workflowParameters.ChromGenTolerance, _workflowParameters.ChromGeneratorMode)
            {
                TopNPeaksLowerCutOff = 0.333
            };

            chromGenN15 = new PeakChromatogramGenerator(
                _workflowParameters.ChromGenTolerance,
                DeconTools.Backend.Globals.ChromatogramGeneratorMode.MOST_ABUNDANT_PEAK, DeconTools.Backend.Globals.IsotopicProfileType.LABELED)
            {
                TopNPeaksLowerCutOff = 0.333
            };

            var pointsToSmooth = (_workflowParameters.ChromSmootherNumPointsInSmooth + 1) / 2;
            chromSmoother = new SavitzkyGolaySmoother(_workflowParameters.ChromSmootherNumPointsInSmooth, 2);
            chromPeakDetector = new ChromPeakDetector(_workflowParameters.ChromPeakDetectorPeakBR, _workflowParameters.ChromPeakDetectorSigNoise);

            var smartChromPeakSelectorParams = new SmartChromPeakSelectorParameters
            {
                MSFeatureFinderType = DeconTools.Backend.Globals.TargetedFeatureFinderType.ITERATIVE,
                MSPeakDetectorPeakBR = _workflowParameters.MSPeakDetectorPeakBR,
                MSPeakDetectorSigNoiseThresh = _workflowParameters.MSPeakDetectorSigNoise,
                MSToleranceInPPM = _workflowParameters.MSToleranceInPPM,
                NETTolerance = (float)_workflowParameters.ChromNETTolerance,
                NumScansToSum = _workflowParameters.NumMSScansToSum,
                NumChromPeaksAllowed = 10,
                IterativeTffMinRelIntensityForPeakInclusion = 0.5
            };

            chromPeakSelectorN14 = new SmartChromPeakSelector(smartChromPeakSelectorParams);

            var chromPeakSelectorParameters = new ChromPeakSelectorParameters
            {
                NumScansToSum = _workflowParameters.NumMSScansToSum,
                NETTolerance = (float)_workflowParameters.ChromNETTolerance,
                PeakSelectorMode = DeconTools.Backend.Globals.PeakSelectorMode.N15IntelligentMode
            };

            chromPeakSelectorN15 = new BasicChromPeakSelector(chromPeakSelectorParameters)
            {
                IsotopicProfileType = DeconTools.Backend.Globals.IsotopicProfileType.LABELED
            };

            msPeakDetector = new DeconToolsPeakDetectorV2(_workflowParameters.MSPeakDetectorPeakBR,
                _workflowParameters.MSPeakDetectorSigNoise, DeconTools.Backend.Globals.PeakFitType.QUADRATIC, false);

            var iterativeTFFParameters = new IterativeTFFParameters
            {
                ToleranceInPPM = _n14N15Workflow2Parameters.TargetedFeatureFinderToleranceInPPM,
                MinimumRelIntensityForForPeakInclusion = 0.33,
                IsotopicProfileType = DeconTools.Backend.Globals.IsotopicProfileType.UNLABELED
            };
            unlabeledProfilefinder = new IterativeTFF(iterativeTFFParameters);

            iterativeTFFParameters = new IterativeTFFParameters
            {
                ToleranceInPPM = _n14N15Workflow2Parameters.TargetedFeatureFinderToleranceInPPM,
                MinimumRelIntensityForForPeakInclusion = 0.33,
                IsotopicProfileType = DeconTools.Backend.Globals.IsotopicProfileType.LABELED
            };
            labeledProfileFinder = new IterativeTFF(iterativeTFFParameters);

            quantifier = new N14N15QuantifierTask(_n14N15Workflow2Parameters.NumPeaksUsedInQuant, _workflowParameters.MSToleranceInPPM);

            fitScoreCalc = new IsotopicProfileFitScoreCalculator();

            var minRelativeIntensityForScore = 0.2;
            resultValidatorN14 = new ResultValidatorTask(minRelativeIntensityForScore, true);

            resultValidatorN15 = new LabeledIsotopicProfileScorer(minRelativeIntensityForScore);

            ChromatogramXYData = new XYData();
            MassSpectrumXYData = new XYData();
            ChromPeaksDetected = new List<ChromPeak>();
        }

        #endregion

        protected override DeconTools.Backend.Globals.ResultType GetResultType()
        {
            return DeconTools.Backend.Globals.ResultType.N14N15_TARGETED_RESULT;
        }
    }
}
