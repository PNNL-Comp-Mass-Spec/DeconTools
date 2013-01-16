using System;
using System.Collections.Generic;
using DeconTools.Backend;
using DeconTools.Backend.Core;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend.ProcessingTasks.ChromatogramProcessing;
using DeconTools.Backend.ProcessingTasks.FitScoreCalculators;
using DeconTools.Backend.ProcessingTasks.PeakDetectors;
using DeconTools.Backend.ProcessingTasks.ResultValidators;
using DeconTools.Backend.ProcessingTasks.Smoothers;
using DeconTools.Backend.ProcessingTasks.TargetedFeatureFinders;
using DeconTools.Backend.ProcessingTasks.TheorFeatureGenerator;
using DeconTools.Utilities;

namespace DeconTools.Workflows.Backend.Core
{
    public abstract class TargetedWorkflow : WorkflowBase
    {
        protected TargetedWorkflowParameters _workflowParameters;
        protected JoshTheorFeatureGenerator _theorFeatureGen;
        protected PeakChromatogramGenerator _chromGen;
        protected SavitzkyGolaySmoother _chromSmoother;
        protected ChromPeakDetector _chromPeakDetector;
        protected ChromPeakSelectorBase _chromPeakSelector;
        protected IterativeTFF _msfeatureFinder;
        //protected MassTagFitScoreCalculator _fitScoreCalc;
        protected Task _fitScoreCalc;
        protected ResultValidatorTask _resultValidator;
        protected ChromatogramCorrelatorTask _chromatogramCorrelatorTask;

        protected IterativeTFFParameters _iterativeTFFParameters = new IterativeTFFParameters();


        #region Constructors
        #endregion

       
        #region Properties
        public virtual IList<ChromPeak> ChromPeaksDetected { get; set; }

        public virtual ChromPeak ChromPeakSelected { get; set; }

        public virtual XYData MassSpectrumXYData { get; set; }

        public virtual XYData ChromatogramXYData { get; set; }

        public bool Success { get; set; }

        public bool IsWorkflowInitialized { get; set; }

        public string WorkflowStatusMessage { get; set; }

        #endregion


        protected void UpdateChromDetectedPeaks(List<Peak> list)
        {
            foreach (ChromPeak chrompeak in list)
            {
                this.ChromPeaksDetected.Add(chrompeak);

            }


        }

        protected  virtual void ValidateParameters()
        {
            Check.Require(_workflowParameters != null, "Cannot validate workflow parameters. Parameters are null");

            bool pointsInSmoothIsEvenNumber = (_workflowParameters.ChromSmootherNumPointsInSmooth % 2 == 0);
            if (pointsInSmoothIsEvenNumber)
            {
                throw new ArgumentOutOfRangeException("Points in chrom smoother is an even number, but must be an odd number.");
            }
        }

        protected void updateChromDataXYValues(XYData xydata)
        {
            if (xydata == null)
            {
                //ResetStoredXYData(ChromatogramXYData);
                return;
            }
            else
            {
                this.ChromatogramXYData.Xvalues = xydata.Xvalues;
                this.ChromatogramXYData.Yvalues = xydata.Yvalues;
            }

        }


        protected virtual void updateMassAndNETValuesAfterAlignment()
        {


        }


        protected void updateMassSpectrumXYValues(XYData xydata)
        {
            if (xydata == null)
            {
                //ResetStoredXYData(ChromatogramXYData);
                return;
            }
            else
            {
                this.MassSpectrumXYData.Xvalues = xydata.Xvalues;
                this.MassSpectrumXYData.Yvalues = xydata.Yvalues;
            }
        }

        public override void InitializeWorkflow()
        {
            DoPreInitialization();

            DoMainInitialization();

            DoPostInitialization();

            IsWorkflowInitialized = true;

        }

        protected virtual void DoPreInitialization(){}
        
        protected  virtual  void DoPostInitialization(){}

        protected  virtual void DoMainInitialization()
        {
            ValidateParameters();

            _theorFeatureGen = new JoshTheorFeatureGenerator(DeconTools.Backend.Globals.LabellingType.NONE, 0.005);
            _chromGen = new PeakChromatogramGenerator(_workflowParameters.ChromToleranceInPPM, _workflowParameters.ChromGeneratorMode);
            _chromGen.TopNPeaksLowerCutOff = 0.333;
            _chromGen.NETWindowWidthForAlignedData = (float)_workflowParameters.ChromNETTolerance * 2;   //only
            _chromGen.NETWindowWidthForNonAlignedData = (float) _workflowParameters.ChromNETTolerance*2;

            bool allowNegativeValues=false;
            _chromSmoother = new SavitzkyGolaySmoother(_workflowParameters.ChromSmootherNumPointsInSmooth, 2, allowNegativeValues);
            _chromPeakDetector = new ChromPeakDetector(_workflowParameters.ChromPeakDetectorPeakBR, _workflowParameters.ChromPeakDetectorSigNoise);
            _chromPeakSelector = CreateChromPeakSelector(_workflowParameters);

            _iterativeTFFParameters = new IterativeTFFParameters();
            _iterativeTFFParameters.ToleranceInPPM = _workflowParameters.MSToleranceInPPM;
            
            _msfeatureFinder = new IterativeTFF(_iterativeTFFParameters);
            _fitScoreCalc = new MassTagFitScoreCalculator();
            _resultValidator = new ResultValidatorTask();
            _chromatogramCorrelatorTask = new ChromatogramCorrelatorTask();
            _chromatogramCorrelatorTask.ChromToleranceInPPM = _workflowParameters.ChromToleranceInPPM;

            ChromatogramXYData = new XYData();
            MassSpectrumXYData = new XYData();
            ChromPeaksDetected = new List<ChromPeak>();

        }


        protected virtual void ExecutePostWorkflowHook()
        {
            if (Result!=null && Result.Target!=null && Success)
            {
                WorkflowStatusMessage = "Result " + Result.Target.ID + "; m/z= " + Result.Target.MZ.ToString("0.0000") +
                                        "; z=" + Result.Target.ChargeState;

                if (Result.FailedResult==false)
                {
                    if (Result.IsotopicProfile!=null)
                    {
                        WorkflowStatusMessage = WorkflowStatusMessage + "; Target FOUND!";

                    }
                    
                }
                else
                {
                    WorkflowStatusMessage = WorkflowStatusMessage + "; Target NOT found. Reason: " + Result.FailureType;
                }

                
            }
            
        }


        protected virtual void HandleWorkflowError(Exception ex)
        {
            Success = false;
            WorkflowStatusMessage = "Unexpected IQ workflow error. Error info: " + ex.Message;

            if (ex.Message.Contains("COM") || ex.Message.ToLower().Contains(".dll"))
            {
                throw new ApplicationException("There was a critical failure! Error info: " + ex.Message);
            }

            TargetedResultBase result =Run.ResultCollection.GetTargetedResult(Run.CurrentMassTag);
            result.ErrorDescription = ex.Message + "\n" + ex.StackTrace;
            result.FailedResult = true;
        }


        public virtual void ResetStoredData()
        {
            this.ResetStoredXYData(this.ChromatogramXYData);
            this.ResetStoredXYData(this.MassSpectrumXYData);

            this.Run.XYData = null;
            this.Run.PeakList = new List<Peak>();

            this.ChromPeaksDetected.Clear();
            this.ChromPeakSelected = null;
        }

        public void ResetStoredXYData(XYData xydata)
        {
            xydata.Xvalues = new double[] { 0, 1, 2, 3 };
            xydata.Yvalues = new double[] { 0, 0, 0, 0 };
        }


        public override void InitializeRunRelatedTasks()
        {
            if (Run == null) return;

            base.InitializeRunRelatedTasks();

            if (this.WorkflowParameters is TargetedWorkflowParameters)
            {
                this.Run.ResultCollection.ResultType = ((TargetedWorkflowParameters)this.WorkflowParameters).ResultType;
            }
        }

   
        /// <summary>
        /// Factory method for creating the Workflow object using the WorkflowType information in the parameter object
        /// </summary>
        /// <param name="workflowParameters"></param>
        /// <returns></returns>
        public static TargetedWorkflow CreateWorkflow(WorkflowParameters workflowParameters)
        {
            TargetedWorkflow wf;

            switch (workflowParameters.WorkflowType)
            {
                case Globals.TargetedWorkflowTypes.Undefined:
                    wf = new BasicTargetedWorkflow(workflowParameters as TargetedWorkflowParameters);
                    break;
                case Globals.TargetedWorkflowTypes.UnlabelledTargeted1:
                    wf = new BasicTargetedWorkflow(workflowParameters as TargetedWorkflowParameters);
                    break;
                case Globals.TargetedWorkflowTypes.O16O18Targeted1:
                    wf = new O16O18Workflow(workflowParameters as TargetedWorkflowParameters);
                    break;
                case Globals.TargetedWorkflowTypes.N14N15Targeted1:
                    wf = new N14N15Workflow2(workflowParameters as TargetedWorkflowParameters);
                    break;
                case Globals.TargetedWorkflowTypes.SipperTargeted1:
                    wf = new SipperTargetedWorkflow(workflowParameters as TargetedWorkflowParameters);
                    break;
                case Globals.TargetedWorkflowTypes.TargetedAlignerWorkflow1:
                    wf = new TargetedAlignerWorkflow(workflowParameters);
                    break;
				case Globals.TargetedWorkflowTypes.TopDownTargeted1:
					wf = new TopDownTargetedWorkflow(workflowParameters as TargetedWorkflowParameters);
            		break;
                case Globals.TargetedWorkflowTypes.PeakDetectAndExportWorkflow1:
                    throw new System.NotImplementedException("Cannot create this workflow type here.");
                case Globals.TargetedWorkflowTypes.BasicTargetedWorkflowExecutor1:
                    throw new System.NotImplementedException("Cannot create this workflow type here.");
				case Globals.TargetedWorkflowTypes.UIMFTargetedMSMSWorkflowCollapseIMS:
					wf = new UIMFTargetedMSMSWorkflowCollapseIMS(workflowParameters as TargetedWorkflowParameters);
            		break;
                default:
                    wf = new BasicTargetedWorkflow(workflowParameters as TargetedWorkflowParameters);
                    break;
            }

            return wf;

        }


        /// <summary>
        /// Factory method for creating the key ChromPeakSelector algorithm
        /// </summary>
        /// <param name="workflowParameters"></param>
        /// <returns></returns>
        public static ChromPeakSelectorBase CreateChromPeakSelector(TargetedWorkflowParameters workflowParameters)
        {
            ChromPeakSelectorBase chromPeakSelector;
            ChromPeakSelectorParameters chromPeakSelectorParameters = new ChromPeakSelectorParameters();
            chromPeakSelectorParameters.NETTolerance = (float)workflowParameters.ChromNETTolerance;
            chromPeakSelectorParameters.NumScansToSum = workflowParameters.NumMSScansToSum;
            chromPeakSelectorParameters.PeakSelectorMode = workflowParameters.ChromPeakSelectorMode;
            chromPeakSelectorParameters.SummingMode = workflowParameters.SummingMode;
            chromPeakSelectorParameters.AreaOfPeakToSumInDynamicSumming = workflowParameters.AreaOfPeakToSumInDynamicSumming;
            chromPeakSelectorParameters.MaxScansSummedInDynamicSumming = workflowParameters.MaxScansSummedInDynamicSumming;



            switch (workflowParameters.ChromPeakSelectorMode)
            {
                case DeconTools.Backend.Globals.PeakSelectorMode.ClosestToTarget:
                case DeconTools.Backend.Globals.PeakSelectorMode.MostIntense:
                case DeconTools.Backend.Globals.PeakSelectorMode.N15IntelligentMode:
                case DeconTools.Backend.Globals.PeakSelectorMode.RelativeToOtherChromPeak:
                    chromPeakSelector = new BasicChromPeakSelector(chromPeakSelectorParameters);
                    break;

                case DeconTools.Backend.Globals.PeakSelectorMode.Smart:

                    var smartchrompeakSelectorParameters = new SmartChromPeakSelectorParameters(chromPeakSelectorParameters);
                    smartchrompeakSelectorParameters.MSFeatureFinderType = DeconTools.Backend.Globals.TargetedFeatureFinderType.ITERATIVE;
                    smartchrompeakSelectorParameters.MSPeakDetectorPeakBR = workflowParameters.MSPeakDetectorPeakBR;
                    smartchrompeakSelectorParameters.MSPeakDetectorSigNoiseThresh = workflowParameters.MSPeakDetectorSigNoise;
                    smartchrompeakSelectorParameters.MSToleranceInPPM = workflowParameters.MSToleranceInPPM;
                    smartchrompeakSelectorParameters.NumChromPeaksAllowed = workflowParameters.NumChromPeaksAllowedDuringSelection;
                    smartchrompeakSelectorParameters.MultipleHighQualityMatchesAreAllowed = workflowParameters.MultipleHighQualityMatchesAreAllowed;
                    smartchrompeakSelectorParameters.IterativeTffMinRelIntensityForPeakInclusion = 0.66;

                    chromPeakSelector = new SmartChromPeakSelector(smartchrompeakSelectorParameters);

                    break;
				case DeconTools.Backend.Globals.PeakSelectorMode.SmartUIMF:
					var smartUIMFchrompeakSelectorParameters = new SmartChromPeakSelectorParameters(chromPeakSelectorParameters);
					smartUIMFchrompeakSelectorParameters.MSFeatureFinderType = DeconTools.Backend.Globals.TargetedFeatureFinderType.ITERATIVE;
					smartUIMFchrompeakSelectorParameters.MSPeakDetectorPeakBR = workflowParameters.MSPeakDetectorPeakBR;
					smartUIMFchrompeakSelectorParameters.MSPeakDetectorSigNoiseThresh = workflowParameters.MSPeakDetectorSigNoise;
					smartUIMFchrompeakSelectorParameters.MSToleranceInPPM = workflowParameters.MSToleranceInPPM;
					smartUIMFchrompeakSelectorParameters.NumChromPeaksAllowed = workflowParameters.NumChromPeaksAllowedDuringSelection;
					smartUIMFchrompeakSelectorParameters.MultipleHighQualityMatchesAreAllowed = workflowParameters.MultipleHighQualityMatchesAreAllowed;
					smartUIMFchrompeakSelectorParameters.IterativeTffMinRelIntensityForPeakInclusion = 0.66;

					chromPeakSelector = new SmartChromPeakSelectorUIMF(smartUIMFchrompeakSelectorParameters);

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return chromPeakSelector;


        }

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
    }
}
