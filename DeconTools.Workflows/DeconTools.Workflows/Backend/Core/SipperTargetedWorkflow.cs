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
    public class SipperTargetedWorkflow : TargetedWorkflow
    {

        private JoshTheorFeatureGenerator _theorFeatureGen;
        private PeakChromatogramGenerator _chromGen;
        private DeconToolsSavitzkyGolaySmoother _chromSmoother;
        private ChromPeakDetector _chromPeakDetector;
        private ResultValidatorTask _resultValidator;

        private ChromPeakSelectorBase _chromPeakSelector;

        //private SmartChromPeakSelector chromPeakSelector;

        private TFFBase _iterativeMSFeatureFinder;
        private DeconToolsPeakDetector _msPeakDetector;
        private MassTagFitScoreCalculator _fitScoreCalc;
        private SipperQuantifier _quantifier;

        #region Constructors

        public SipperTargetedWorkflow(Run run, TargetedWorkflowParameters parameters)
        {
            WorkflowParameters = parameters;

            Run = run;
            InitializeWorkflow();

        }

        public SipperTargetedWorkflow(TargetedWorkflowParameters parameters)
            : this(null, parameters)
        {
            
            
        }



        public override sealed void InitializeWorkflow()
        {
            ValidateParameters();

            _theorFeatureGen = new JoshTheorFeatureGenerator(DeconTools.Backend.Globals.LabellingType.NONE,0.00000001);

            _chromGen = new PeakChromatogramGenerator(_workflowParameters.ChromToleranceInPPM, _workflowParameters.ChromGeneratorMode);
            _chromGen.TopNPeaksLowerCutOff = 0.333;

            int pointsToSmooth = (_workflowParameters.ChromSmootherNumPointsInSmooth + 1) / 2;   // adding 0.5 prevents rounding problems
            _chromSmoother = new DeconToolsSavitzkyGolaySmoother(pointsToSmooth, pointsToSmooth, 2);
            _chromPeakDetector = new ChromPeakDetector(_workflowParameters.ChromPeakDetectorPeakBR, _workflowParameters.ChromPeakDetectorSigNoise);


            _chromPeakSelector = CreateChromPeakSelector(_workflowParameters);


            _msPeakDetector = new DeconToolsPeakDetector(_workflowParameters.MSPeakDetectorPeakBR, _workflowParameters.MSPeakDetectorSigNoise, DeconTools.Backend.Globals.PeakFitType.QUADRATIC, false);

            IterativeTFFParameters iterativeTFFParameters = new IterativeTFFParameters();
            iterativeTFFParameters.ToleranceInPPM = _workflowParameters.MSToleranceInPPM;

            _iterativeMSFeatureFinder = new SipperIterativeMSFeatureFinder(iterativeTFFParameters);
            // _iterativeMSFeatureFinder = new IterativeTFF(iterativeTFFParameters);

            _quantifier = new SipperQuantifier();
            _fitScoreCalc = new MassTagFitScoreCalculator();
            _resultValidator = new ResultValidatorTask();

            ChromatogramXYData = new XYData();
            MassSpectrumXYData = new XYData();
            RatioVals = new XYData();
            RatioLogVals = new XYData();
            ChromPeaksDetected = new List<ChromPeak>();
            ChromCorrelationRSquaredVals = new XYData();
        }


        public override void Execute()
        {
            ResetStoredData();

            Run.ResultCollection.ResultType = DeconTools.Backend.Globals.ResultType.SIPPER_TARGETED_RESULT;

            try
            {

                Result = Run.ResultCollection.GetTargetedResult(Run.CurrentMassTag);
                Result.ResetResult();

                ExecuteTask(_theorFeatureGen);
                ExecuteTask(_chromGen);
                ExecuteTask(_chromSmoother);
                updateChromDataXYValues(Run.XYData);

                ExecuteTask(_chromPeakDetector);
                updateChromDetectedPeaks(Run.PeakList);

                ExecuteTask(_chromPeakSelector);
                ChromPeakSelected = Result.ChromPeakSelected;

                //Console.WriteLine("ChromPeak width = \t" + ChromPeakSelected.Width);

                Result.ResetMassSpectrumRelatedInfo();


                ExecuteTask(MSGenerator);


                double minMZ = Run.CurrentMassTag.MZ - 3;
                double maxMz = Run.CurrentMassTag.MZ + 20;

                if (Run.XYData != null)
                {
                    Run.XYData = Run.XYData.TrimData(minMZ, maxMz);
                }


                updateMassSpectrumXYValues(Run.XYData);

                ExecuteTask(_iterativeMSFeatureFinder);
                ExecuteTask(_fitScoreCalc);
                ExecuteTask(_resultValidator);

                ExecuteTask(_quantifier);

                GetDataFromQuantifier();

            }
            catch (Exception ex)
            {
                TargetedResultBase result = Run.ResultCollection.CurrentTargetedResult;
                result.FailedResult = true;
                result.ErrorDescription = ex.Message;
                Console.WriteLine(((LcmsFeatureTarget)result.Target).FeatureToMassTagID + "; " + result.ErrorDescription);

                return;
            }
        }


        private void GetDataFromQuantifier()
        {
            RatioVals.Xvalues = _quantifier.RatioVals == null ? new double[] { 1, 2, 3, 4, 5, 6 } : _quantifier.RatioVals.Xvalues;
            RatioVals.Yvalues = _quantifier.RatioVals == null ? new double[] { 0, 0, 0, 0, 0, 0 } : _quantifier.RatioVals.Yvalues;

            RatioLogVals.Xvalues = _quantifier.RatioLogVals == null ? new double[] { 1, 2, 3, 4, 5, 6 } : _quantifier.RatioLogVals.Xvalues;
            RatioLogVals.Yvalues = _quantifier.RatioLogVals == null ? new double[] { 0, 0, 0, 0, 0, 0 } : _quantifier.RatioLogVals.Yvalues;

            var peakNumList = new List<double>();
            var rsquaredvalList = new List<double>();

            
            int counter = 0;
            foreach (var val in _quantifier.ChromatogramRSquaredVals)
            {
                peakNumList.Add(counter);
                rsquaredvalList.Add(val);

                counter++;
            }

            ChromCorrelationRSquaredVals.Xvalues = peakNumList.ToArray();
            ChromCorrelationRSquaredVals.Yvalues = rsquaredvalList.ToArray();


            NormalizedIso = _quantifier.NormalizedIso;
            NormalizedAdjustedIso = _quantifier.NormalizedAdjustedIso;

            SubtractedIso = _quantifier.HighQualitySubtractedProfile;

            
            
        }

        public IsotopicProfile SubtractedIso { get; set; }

        public IsotopicProfile NormalizedIso { get; set; }

        public IsotopicProfile NormalizedAdjustedIso { get; set; }


        #endregion

        #region Properties
        TargetedWorkflowParameters _workflowParameters;
        public override WorkflowParameters WorkflowParameters
        {
            get
            {
                return _workflowParameters;
            }
            set
            {
                _workflowParameters = value as TargetedWorkflowParameters;
            }
        }


        public XYData ChromCorrelationRSquaredVals { get; set; }

        public XYData RatioVals { get; set; }

        public XYData RatioLogVals { get; set; }



        #endregion

        #region Public Methods

        #endregion

        #region Private Methods

        private void ValidateParameters()
        {
            bool pointsInSmoothIsEvenNumber = (_workflowParameters.ChromSmootherNumPointsInSmooth % 2 == 0);
            if (pointsInSmoothIsEvenNumber)
            {
                throw new ArgumentOutOfRangeException("Points in chrom smoother is an even number, but must be an odd number.");
            }

            //add parameter validation

        }

        #endregion

    }
}
