using System;
using System.Collections.Generic;
using DeconTools.Backend;
using DeconTools.Backend.Core;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend.ProcessingTasks.ChromatogramProcessing;
using DeconTools.Backend.ProcessingTasks.FitScoreCalculators;
using DeconTools.Backend.ProcessingTasks.PeakDetectors;
using DeconTools.Backend.ProcessingTasks.Quantifiers;
using DeconTools.Backend.ProcessingTasks.ResultValidators;
using DeconTools.Backend.ProcessingTasks.Smoothers;
using DeconTools.Backend.ProcessingTasks.TargetedFeatureFinders;
using DeconTools.Backend.ProcessingTasks.TheorFeatureGenerator;

namespace DeconTools.Workflows.Backend.Core
{
    public sealed class N14N15Workflow2 : TargetedWorkflow
    {
        private JoshTheorFeatureGenerator theorFeatureGen;
        private JoshTheorFeatureGenerator theorN15FeatureGen;

        private PeakChromatogramGenerator chromGenN14;
        private PeakChromatogramGenerator chromGenN15;
        private DeconToolsSavitzkyGolaySmoother chromSmoother;
        private ChromPeakDetector chromPeakDetector;
        private SmartChromPeakSelector chromPeakSelectorN14;
        private BasicChromPeakSelector chromPeakSelectorN15;

        private DeconToolsPeakDetector msPeakDetector;

        private IterativeTFF labelledProfileFinder;
        private BasicTFF unlabelledProfilefinder;

        private N14N15QuantifierTask quantifier;

        private MassTagFitScoreCalculator fitScoreCalc;

        private ResultValidatorTask resultValidatorN14;

        private LabelledIsotopicProfileScorer resultValidatorN15;


        #region Constructors

        public N14N15Workflow2(Run run, TargetedWorkflowParameters parameters)
        {
            this.WorkflowParameters = parameters;
            this.Run = run;

            InitializeWorkflow();
        
        }

        public N14N15Workflow2(TargetedWorkflowParameters parameters)
            : this(null, parameters)
        {

        }


        #endregion

        #region Properties

        N14N15Workflow2Parameters _workflowParameters;
        public override WorkflowParameters WorkflowParameters
        {
            get
            {
                return _workflowParameters;
            }
            set
            {
                _workflowParameters = value as N14N15Workflow2Parameters;
            }
        }


        #endregion

        #region Public Methods

        #endregion


        #region Workflow Members
        public override void Execute()
        {
            if (this.Run.CurrentMassTag==null)
            {
                throw new NullReferenceException("Cannot execute workflow. Current target is null.");
            }

            ResetStoredData();

            try
            {
                this.Result = this.Run.ResultCollection.GetTargetedResult(this.Run.CurrentMassTag);
                this.Result.ResetResult();

                ExecuteTask(theorFeatureGen);
                ExecuteTask(theorN15FeatureGen);
                ExecuteTask(chromGenN14);
                ExecuteTask(chromSmoother);
                updateChromDataXYValues(this.Run.XYData);

                ExecuteTask(chromPeakDetector);
                updateChromDetectedPeaks(this.Run.PeakList);

                ExecuteTask(chromPeakSelectorN14);
                this.ChromPeakSelected = this.Result.ChromPeakSelected;
                chromPeakSelectorN15.ReferenceNETValueForReferenceMode = this.Result.GetNET();    //so that the NET value of the N14 result can be used to help find the N15 chrom peak

                ExecuteTask(MSGenerator);
                updateMassSpectrumXYValues(this.Run.XYData);

                ExecuteTask(msPeakDetector);
                ExecuteTask(unlabelledProfilefinder);

                ExecuteTask(fitScoreCalc);
                ExecuteTask(resultValidatorN14);

                //a bit of a hack... but we need to declare that the Result isn't failed so that the following tasks will be performed
                this.Result.FailedResult = false;

                //now process the N15 profile


                ExecuteTask(chromGenN15);
                ExecuteTask(chromSmoother);

                ExecuteTask(chromPeakDetector);

                ExecuteTask(chromPeakSelectorN15);

                //even if we don't find anything, we want to create a mass spectrum and pull out values of N15 data
                N14N15_TResult n14n15result = (N14N15_TResult)this.Result;
                if (n14n15result.ChromPeakSelectedN15 == null)
                {
                    n14n15result.ScanSetForN15Profile = this.Result.ScanSet;
                    this.Run.CurrentScanSet = n14n15result.ScanSetForN15Profile;

                    if (n14n15result.ScanSetForN15Profile == null)
                    {
                        this.Result.FailedResult = true;
                        this.Result.FailureType = DeconTools.Backend.Globals.TargetedResultFailureType.CHROMPEAK_NOT_FOUND_WITHIN_TOLERANCES;
                    }
                }


                ExecuteTask(MSGenerator);
                updateMassSpectrumXYValues(this.Run.XYData);

                if (Run.XYData!=null)
                {
                    Run.XYData = Run.XYData.TrimData(n14n15result.Target.MZ - 5,
                                     n14n15result.Target.IsotopicProfileLabelled.MonoPeakMZ + 5);
 
                }

                
                ExecuteTask(msPeakDetector);
                ExecuteTask(labelledProfileFinder);

                resultValidatorN15.CurrentResult = this.Result;

                ExecuteTask(resultValidatorN15);
                ExecuteTask(quantifier);



            }
            catch (Exception ex)
            {
                var result = Run.ResultCollection.GetTargetedResult(this.Run.CurrentMassTag);
                result.ErrorDescription = ex.Message + "\n" + ex.StackTrace;

                return;
            }

        }

        public override void InitializeWorkflow()
        {
            //this.Run.ResultCollection.ResultType = DeconTools.Backend.Globals.ResultType.N14N15_TARGETED_RESULT;

            ValidateParameters();

            theorFeatureGen = new JoshTheorFeatureGenerator(DeconTools.Backend.Globals.LabellingType.NONE, 0.005);
            theorN15FeatureGen = new JoshTheorFeatureGenerator(DeconTools.Backend.Globals.LabellingType.N15, 0.005);

            chromGenN14 = new PeakChromatogramGenerator(_workflowParameters.ChromToleranceInPPM, _workflowParameters.ChromGeneratorMode);
            chromGenN14.TopNPeaksLowerCutOff = 0.333;

            chromGenN15 = new PeakChromatogramGenerator(_workflowParameters.ChromToleranceInPPM, ChromatogramGeneratorMode.MOST_ABUNDANT_PEAK, IsotopicProfileType.LABELLED);
            chromGenN15.TopNPeaksLowerCutOff = 0.333;

            int pointsToSmooth = (_workflowParameters.ChromSmootherNumPointsInSmooth + 1) / 2;
            chromSmoother = new DeconToolsSavitzkyGolaySmoother(pointsToSmooth, pointsToSmooth, 2);
            chromPeakDetector = new ChromPeakDetector(_workflowParameters.ChromPeakDetectorPeakBR, _workflowParameters.ChromPeakDetectorSigNoise);

            SmartChromPeakSelectorParameters smartchrompeakSelectorParams = new SmartChromPeakSelectorParameters();
            smartchrompeakSelectorParams.MSFeatureFinderType = DeconTools.Backend.Globals.TargetedFeatureFinderType.ITERATIVE;
            smartchrompeakSelectorParams.MSPeakDetectorPeakBR = _workflowParameters.MSPeakDetectorPeakBR;
            smartchrompeakSelectorParams.MSPeakDetectorSigNoiseThresh = _workflowParameters.MSPeakDetectorSigNoise;
            smartchrompeakSelectorParams.MSToleranceInPPM = _workflowParameters.MSToleranceInPPM;
            smartchrompeakSelectorParams.NETTolerance = (float) _workflowParameters.ChromNETTolerance;
            smartchrompeakSelectorParams.NumScansToSum = _workflowParameters.NumMSScansToSum;
            smartchrompeakSelectorParams.NumChromPeaksAllowed = 10;
            smartchrompeakSelectorParams.IterativeTffMinRelIntensityForPeakInclusion = 0.5;


            chromPeakSelectorN14 = new SmartChromPeakSelector(smartchrompeakSelectorParams);


            ChromPeakSelectorParameters chromPeakSelectorParameters=new ChromPeakSelectorParameters();
            chromPeakSelectorParameters.NumScansToSum = _workflowParameters.NumMSScansToSum;
            chromPeakSelectorParameters.NETTolerance =(float)_workflowParameters.ChromNETTolerance;
            chromPeakSelectorParameters.PeakSelectorMode = DeconTools.Backend.Globals.PeakSelectorMode.N15IntelligentMode;

            chromPeakSelectorN15 = new BasicChromPeakSelector(chromPeakSelectorParameters);

            msPeakDetector = new DeconToolsPeakDetector(_workflowParameters.MSPeakDetectorPeakBR, _workflowParameters.MSPeakDetectorSigNoise, DeconTools.Backend.Globals.PeakFitType.QUADRATIC, false);

            unlabelledProfilefinder = new BasicTFF(_workflowParameters.MSToleranceInPPM);

            var iterativeTFFParameters = new IterativeTFFParameters();
            iterativeTFFParameters.ToleranceInPPM = _workflowParameters.TargetedFeatureFinderToleranceInPPM;
            iterativeTFFParameters.IsotopicProfileType = _workflowParameters.TargetedFeatureFinderIsotopicProfileTargetType;
            iterativeTFFParameters.MinimumRelIntensityForForPeakInclusion = 0.33;
            labelledProfileFinder = new IterativeTFF(iterativeTFFParameters);

            quantifier = new N14N15QuantifierTask(_workflowParameters.NumPeaksUsedInQuant, _workflowParameters.MSToleranceInPPM);

            fitScoreCalc = new MassTagFitScoreCalculator();

            double minRelativeIntensityForScore = 0.2;
            resultValidatorN14 = new ResultValidatorTask(minRelativeIntensityForScore, true);

            resultValidatorN15 = new LabelledIsotopicProfileScorer(minRelativeIntensityForScore);

            ChromatogramXYData = new XYData();
            MassSpectrumXYData = new XYData();
            ChromPeaksDetected = new List<ChromPeak>();



        }

        #endregion


        private void ValidateParameters()
        {
            bool pointsInSmoothIsEvenNumber = (_workflowParameters.ChromSmootherNumPointsInSmooth % 2 == 0);
            if (pointsInSmoothIsEvenNumber)
            {
                throw new ArgumentOutOfRangeException("Points in chrom smoother is an even number, but must be an odd number.");
            }

            //add parameter validation
        }
    }
}
