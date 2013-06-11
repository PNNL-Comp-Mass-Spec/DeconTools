using System;
using System.Collections.Generic;
using DeconTools.Backend.Core;
using DeconTools.Backend.Utilities.IqLogger;
using DeconTools.Workflows.Backend.FileIO;

namespace DeconTools.Workflows.Backend.Core
{
    public class O16O18IqWorkflow : ChargeStateChildIqWorkflow
    {

        #region Constructors
        public O16O18IqWorkflow(Run run, TargetedWorkflowParameters parameters)
            : base(run, parameters)
        {
        }

        public O16O18IqWorkflow(TargetedWorkflowParameters parameters)
            : base(parameters)
        {
        }
        #endregion

        public override TargetedWorkflowParameters WorkflowParameters { get; set; }
        public override ResultExporter CreateExporter()
        {
            throw new System.NotImplementedException();
        }


        protected override void ExecuteWorkflow(IqResult result)
        {
            if (ChromPeakAnalyzerIqWorkflow == null)
            {
                InitializeChromPeakAnalyzerWorkflow();
            }

            result.Target.TheorIsotopicProfile = TheorFeatureGen.GenerateTheorProfile(result.Target.EmpiricalFormula, result.Target.ChargeState);

            result.IqResultDetail.Chromatogram = ChromGen.GenerateChromatogram(Run, result.Target.TheorIsotopicProfile, result.Target.ElutionTimeTheor);

            result.IqResultDetail.Chromatogram = ChromSmoother.Smooth(result.IqResultDetail.Chromatogram);

            result.ChromPeakList = ChromPeakDetector.FindPeaks(result.IqResultDetail.Chromatogram);

            ChromPeakDetector.CalculateElutionTimes(Run, result.ChromPeakList);

            ChromPeakDetector.FilterPeaksOnNET(WorkflowParameters.ChromNETTolerance, result.Target.ElutionTimeTheor, result.ChromPeakList);

            int tempMinScanWithinTol = (int) Run.NetAlignmentInfo.GetScanForNet(result.Target.ElutionTimeTheor - WorkflowParameters.ChromNETTolerance);
            int tempMaxScanWithinTol = (int) Run.NetAlignmentInfo.GetScanForNet(result.Target.ElutionTimeTheor + WorkflowParameters.ChromNETTolerance);
            int tempCenterTol = (int) Run.NetAlignmentInfo.GetScanForNet(result.Target.ElutionTimeTheor);

            result.NumChromPeaksWithinTolerance = result.ChromPeakList.Count;

            //General peak information output written to console.
            IqLogger.Log.Debug("\tSmartPeakSelector --> NETTolerance= " + WorkflowParameters.ChromNETTolerance + ";  chromMinCenterMax= " +
                              tempMinScanWithinTol + "\t" + tempCenterTol + "" + "\t" + tempMaxScanWithinTol + Environment.NewLine);
            IqLogger.Log.Debug("\tMT= " + result.Target.ID + ";z= " + result.Target.ChargeState + "; mz= " + result.Target.MZTheor.ToString("0.000") +
                              ";  ------------------------- PeaksWithinTol = " + result.ChromPeakList.Count + Environment.NewLine);

            //Creates a ChromPeakIqTarget for each peak found
            foreach (ChromPeak peak in result.ChromPeakList)
            {
                ChromPeakIqTarget target = new ChromPeakIqTarget(ChromPeakAnalyzerIqWorkflow);
                TargetUtilities.CopyTargetProperties(result.Target, target, false);
                target.ChromPeak = peak;
                result.Target.AddTarget(target);
            }

            //Executes each grandchild ChromPeakAnalyzerIqWorkflow
            var children = result.Target.ChildTargets();
            List<IqTarget> targetRemovalList = new List<IqTarget>();
            foreach (var child in children)
            {
                child.DoWorkflow();

                /*
                //Selects grandchildren with extremely poor metric scores for removal
                IqResult childResult = child.GetResult();
                if ((childResult.FitScore >= .8) || (childResult.CorrelationData.RSquaredValsMedian <= .15))
                {
                    targetRemovalList.Add(child);
                }
                */
            }

            /*
            //Removes the poorly scoring grandchild ChromPeakIqTargets
            foreach (IqTarget iqTarget in targetRemovalList)
            {
                result.RemoveResult(iqTarget.GetResult());
                result.Target.RemoveTarget(iqTarget);
            }
            */

            if (Utilities.SipperDataDump.OutputResults)
            {
                //Data Dump for use with Sipper
                children = result.Target.ChildTargets();
                foreach (var child in children)
                {
                    Utilities.SipperDataDump.DataDump(child, Run);
                }
            }
        }


        public override void InitializeWorkflow()
        {
            base.InitializeWorkflow();

            

        }


    }
}
