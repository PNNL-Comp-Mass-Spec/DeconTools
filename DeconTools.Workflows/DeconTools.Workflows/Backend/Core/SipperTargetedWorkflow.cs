using System;
using System.Collections.Generic;
using DeconTools.Backend;
using DeconTools.Backend.Core;
using DeconTools.Backend.ProcessingTasks.Quantifiers;
using DeconTools.Backend.ProcessingTasks.TargetedFeatureFinders;
using DeconTools.Backend.ProcessingTasks.TheorFeatureGenerator;

namespace DeconTools.Workflows.Backend.Core
{
    public class SipperTargetedWorkflow : TargetedWorkflow
    {

        
        private TFFBase _iterativeMSFeatureFinder;
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

        protected override void DoPreInitialization()
        {
            base.DoPreInitialization();
            RatioVals = new XYData();
            RatioLogVals = new XYData();
            ChromCorrelationRSquaredVals = new XYData();
        }

        protected override void DoPostInitialization()
        {
            base.DoPostInitialization();

            //we want the theorFeature generator to generate even very low peaks
            _theorFeatureGen = new JoshTheorFeatureGenerator(DeconTools.Backend.Globals.LabellingType.NONE, 0.00000001);

            //we want the MSFeature finder to dig way down low for any peaks
            _iterativeTFFParameters = new IterativeTFFParameters();
            _iterativeTFFParameters.ToleranceInPPM = _workflowParameters.MSToleranceInPPM;
            _iterativeTFFParameters.PeakDetectorPeakBR = 2.1;
            _iterativeTFFParameters.PeakBRStep = 0.2;
            _iterativeTFFParameters.PeakDetectorMinimumPeakBR = 0.1;

            _iterativeMSFeatureFinder = new SipperIterativeMSFeatureFinder(_iterativeTFFParameters);

            _quantifier = new SipperQuantifier();


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
                UpdateChromDetectedPeaks(Run.PeakList);

                ExecuteTask(_chromPeakSelector);
                ChromPeakSelected = Result.ChromPeakSelected;

                //Console.WriteLine("ChromPeak width = \t" + ChromPeakSelected.Width);

                Result.ResetMassSpectrumRelatedInfo();


                ExecuteTask(MSGenerator);


                double minMZ = Run.CurrentMassTag.MZ - 3;
                double maxMz = Run.CurrentMassTag.MZ + 100;

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

                Success = true;

                ExecutePostWorkflowHook();

            }
            catch (Exception ex)
            {
                HandleWorkflowError(ex);

                string targetInfoString = "Uncaptured failure on the following target: " + ((LcmsFeatureTarget)Result.Target).FeatureToMassTagID + "; " + Result.ErrorDescription;
                Console.WriteLine(targetInfoString);
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
    
        public XYData ChromCorrelationRSquaredVals { get; set; }

        public XYData RatioVals { get; set; }

        public XYData RatioLogVals { get; set; }



        #endregion

        #region Public Methods

        #endregion

    
    }
}
