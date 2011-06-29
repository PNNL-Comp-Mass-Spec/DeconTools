using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.ProcessingTasks.TheorFeatureGenerator;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend.ProcessingTasks.Smoothers;
using DeconTools.Backend.ProcessingTasks.PeakDetectors;
using DeconTools.Backend.ProcessingTasks.ChromatogramProcessing;
using DeconTools.Backend.ProcessingTasks.TargetedFeatureFinders;
using DeconTools.Backend.Core;
using DeconTools.Backend.ProcessingTasks.FitScoreCalculators;
using DeconTools.Backend.ProcessingTasks.ResultValidators;

namespace DeconTools.Backend.Workflows
{
    public class BasicTargetedWorkflow:WorkflowBase
    {
        private DeconToolsTargetedWorkflowParameters _workflowParameters;
        private TomTheorFeatureGenerator theorFeatureGen;
        private PeakChromatogramGenerator chromGen;
        private DeconToolsSavitzkyGolaySmoother chromSmoother;
        private ChromPeakDetector chromPeakDetector;
        private SmartChromPeakSelector chromPeakSelector;
        private DeconToolsPeakDetector msPeakDetector;
        private IterativeTFF msfeatureFinder;
        private MassTagFitScoreCalculator fitScoreCalc;
        private ResultValidatorTask resultValidator;


        #region Constructors

        public BasicTargetedWorkflow(Run run, DeconToolsTargetedWorkflowParameters parameters)
        {
            this.WorkflowParameters = parameters;
            this.Run = run;
            this.Run.ResultCollection.MassTagResultType = Globals.MassTagResultType.BASIC_MASSTAG_RESULT;

            InitializeWorkflow();
        }

        #endregion

        #region Properties
        string _name;
        public string Name
        {
            get
            { return this.ToString(); }
            set
            {
                _name = value;
            }

        }
        #endregion

        #region Public Methods

        #endregion

        #region Private Methods

        #endregion


        public override WorkflowParameters WorkflowParameters
        {
            get
            {
                return _workflowParameters;
            }
            set
            {
                _workflowParameters = value as DeconToolsTargetedWorkflowParameters;
            }
        }

        public override void InitializeWorkflow()
        {
            ValidateParameters();

            theorFeatureGen = new TomTheorFeatureGenerator(DeconTools.Backend.Globals.LabellingType.NONE, 0.005);

            chromGen = new PeakChromatogramGenerator(_workflowParameters.ChromToleranceInPPM, _workflowParameters.ChromGeneratorMode);
            chromGen.TopNPeaksLowerCutOff = 0.333;

            int pointsToSmooth = (_workflowParameters.ChromSmootherNumPointsInSmooth + 1) / 2;   // adding 0.5 prevents rounding problems
            chromSmoother = new DeconToolsSavitzkyGolaySmoother(pointsToSmooth, pointsToSmooth, 2);
            chromPeakDetector = new ChromPeakDetector(_workflowParameters.ChromPeakDetectorPeakBR, _workflowParameters.ChromPeakDetectorSigNoise);

            chromPeakSelector = new SmartChromPeakSelector((float)_workflowParameters.ChromNETTolerance, _workflowParameters.NumMSScansToSum);

            //chromPeakSelector.SetDefaultMSPeakDetectorSettings(_workflowParameters.SmartChromSelectorPeakBR, _workflowParameters.SmartChromSelectorPeakSigNoiseRatio, DeconTools.Backend.Globals.PeakFitType.QUADRATIC, false);


            msPeakDetector = new DeconToolsPeakDetector(_workflowParameters.MSPeakDetectorPeakBR, _workflowParameters.MSPeakDetectorSigNoise, DeconTools.Backend.Globals.PeakFitType.QUADRATIC, false);

            IterativeTFFParameters iterativeTFFParameters = new IterativeTFFParameters();
            iterativeTFFParameters.ToleranceInPPM = _workflowParameters.MSToleranceInPPM;

            msfeatureFinder = new  IterativeTFF(iterativeTFFParameters);

            fitScoreCalc = new MassTagFitScoreCalculator();

            resultValidator = new ResultValidatorTask();

            
            ChromatogramXYData = new XYData();
            MassSpectrumXYData = new XYData();
            ChromPeaksDetected = new List<ChromPeak>();
        }

        private void ValidateParameters()
        {
            bool pointsInSmoothIsEvenNumber = (_workflowParameters.ChromSmootherNumPointsInSmooth % 2 == 0);
            if (pointsInSmoothIsEvenNumber)
            {
                throw new ArgumentOutOfRangeException("Points in chrom smoother is an even number, but must be an odd number.");
            }
        }

        public override void Execute()
        {
            ResetStoredData();

            try
            {

                this.Result = this.Run.ResultCollection.GetMassTagResult(this.Run.CurrentMassTag);

                theorFeatureGen.Execute(this.Run.ResultCollection);

                chromGen.Execute(this.Run.ResultCollection);

                chromSmoother.Execute(this.Run.ResultCollection);
                updateChromDataXYValues(this.Run.XYData);

                chromPeakDetector.Execute(this.Run.ResultCollection);
                updateChromDetectedPeaks(this.Run.PeakList);

                chromPeakSelector.Execute(this.Run.ResultCollection);
                this.ChromPeakSelected = this.Result.ChromPeakSelected;

                MSGenerator.Execute(this.Run.ResultCollection);

                updateMassSpectrumXYValues(this.Run.XYData);

                msPeakDetector.Execute(this.Run.ResultCollection);

                msfeatureFinder.Execute(this.Run.ResultCollection);

                fitScoreCalc.Execute(this.Run.ResultCollection);

                resultValidator.Execute(this.Run.ResultCollection);

             

            }
            catch (Exception ex)
            {
                MassTagResultBase result = (MassTagResultBase)this.Run.ResultCollection.GetMassTagResult(this.Run.CurrentMassTag);
                result.ErrorDescription = ex.Message + "\n" + ex.StackTrace;

                return;
            }
        }
    }
}
