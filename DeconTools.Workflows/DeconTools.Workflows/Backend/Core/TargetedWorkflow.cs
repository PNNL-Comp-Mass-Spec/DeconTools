using System;
using System.Collections.Generic;
using DeconTools.Backend;
using DeconTools.Backend.Core;
using DeconTools.Backend.ProcessingTasks.ChromatogramProcessing;

namespace DeconTools.Workflows.Backend.Core
{
    public abstract class TargetedWorkflow : WorkflowBase
    {

        #region Constructors
        #endregion

        private TargetedWorkflowParameters _workflowParameters;


        #region Properties
        public virtual IList<ChromPeak> ChromPeaksDetected { get; set; }

        public virtual ChromPeak ChromPeakSelected { get; set; }

        public virtual XYData MassSpectrumXYData { get; set; }

        public virtual XYData ChromatogramXYData { get; set; }
        #endregion


        protected void updateChromDetectedPeaks(List<Peak> list)
        {
            foreach (ChromPeak chrompeak in list)
            {
                this.ChromPeaksDetected.Add(chrompeak);

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
            base.InitializeRunRelatedTasks();

            if (this.WorkflowParameters is TargetedWorkflowParameters)
            {
                this.Run.ResultCollection.ResultType = ((TargetedWorkflowParameters)this.WorkflowParameters).ResultType;
            }
        }

        #region Public Methods

        #endregion

        #region Private Methods

        #endregion


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
        protected static ChromPeakSelectorBase CreateChromPeakSelector(TargetedWorkflowParameters workflowParameters)
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

                    chromPeakSelector = new SmartChromPeakSelector(smartchrompeakSelectorParameters);

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return chromPeakSelector;


        }
    }
}
