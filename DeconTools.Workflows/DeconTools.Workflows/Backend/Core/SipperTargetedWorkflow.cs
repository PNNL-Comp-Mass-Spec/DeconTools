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
        private SipperQuantifier _quantifier;

        #region Constructors

        public SipperTargetedWorkflow(Run run, TargetedWorkflowParameters parameters)
            : base(run, parameters)
        {
            MsRightTrimAmount = 100;
            MsLeftTrimAmount = 3;
        }

        public SipperTargetedWorkflow(TargetedWorkflowParameters parameters)
            : this(null, parameters)
        {
        }

        protected override DeconTools.Backend.Globals.ResultType GetResultType()
        {
            return DeconTools.Backend.Globals.ResultType.SIPPER_TARGETED_RESULT;
        }

        protected override void DoPreInitialization()
        {
            base.DoPreInitialization();
            RatioVals = new XYData();
            ChromCorrelationRSquaredVals = new XYData();
        }

        protected override void DoPostInitialization()
        {
            base.DoPostInitialization();

            //we want the theorFeature generator to generate even very low peaks
            _theorFeatureGen = new JoshTheorFeatureGenerator(DeconTools.Backend.Globals.LabelingType.NONE, 0.00000001);

            //we want the MSFeature finder to dig way down low for any peaks
            _iterativeTFFParameters = new IterativeTFFParameters();
            _iterativeTFFParameters.ToleranceInPPM = _workflowParameters.MSToleranceInPPM;
            _iterativeTFFParameters.PeakDetectorPeakBR = 2.1;
            _iterativeTFFParameters.PeakBRStep = 0.2;
            _iterativeTFFParameters.PeakDetectorMinimumPeakBR = 0.1;

            _msfeatureFinder = new SipperIterativeMSFeatureFinder(_iterativeTFFParameters);

            _quantifier = new SipperQuantifier();

            //always do ChromCorrelation whether you want it or not!
            _workflowParameters.ChromatogramCorrelationIsPerformed = true;
        }

        protected override void ExecutePostWorkflowHook()
        {
            base.ExecutePostWorkflowHook();

            ExecuteTask(_quantifier);
            GetDataFromQuantifier();
        }

        private void GetDataFromQuantifier()
        {
            RatioVals.Xvalues = _quantifier.RatioVals == null ? new double[] { 1, 2, 3, 4, 5, 6 } : _quantifier.RatioVals.Xvalues;
            RatioVals.Yvalues = _quantifier.RatioVals == null ? new double[] { 0, 0, 0, 0, 0, 0 } : _quantifier.RatioVals.Yvalues;

            var peakNumList = new List<double>();
            var rsquaredvalList = new List<double>();

            var counter = 0;
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

            FitScoreData = _quantifier.FitScoreData;
        }

        #endregion

        #region Properties

        public XYData ChromCorrelationRSquaredVals { get; set; }

        public XYData RatioVals { get; set; }

       public IsotopicProfile SubtractedIso { get; set; }

        public IsotopicProfile NormalizedIso { get; set; }

        public IsotopicProfile NormalizedAdjustedIso { get; set; }

        public Dictionary<decimal, double> FitScoreData { get; set; }

        #endregion

    }
}
