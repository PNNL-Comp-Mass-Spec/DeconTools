using System;
using System.Collections.Generic;
using DeconTools.Backend.Core;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend.Utilities.IqLogger;
using DeconTools.Workflows.Backend.Core.ChromPeakSelection;
using DeconTools.Workflows.Backend.FileIO;

namespace DeconTools.Workflows.Backend.Core
{
    public class O16O18IqWorkflow : ChargeStateChildIqWorkflow
    {

        #region Constructors
        public O16O18IqWorkflow(Run run, TargetedWorkflowParameters parameters)
            : base(run, parameters)
        {
            this.ChromatogramCorrelator = new O16O18ChromCorrelator(parameters.ChromSmootherNumPointsInSmooth, 0.01,
                                                                    parameters.ChromGenTolerance,
                                                                    parameters.ChromGenToleranceUnit);



        }

        public O16O18IqWorkflow(TargetedWorkflowParameters parameters)
            : this(null,parameters)
        {
        }
        #endregion

        public override TargetedWorkflowParameters WorkflowParameters { get; set; }
        public override IqResultExporter CreateExporter()
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

            result.NumChromPeaksWithinTolerance = result.ChromPeakList.Count;

          
            //Creates a ChromPeakIqTarget for each peak found
            foreach (ChromPeak peak in result.ChromPeakList)
            {
                var target = new ChromPeakIqTarget(ChromPeakAnalyzerIqWorkflow);
                TargetUtilities.CopyTargetProperties(result.Target, target, false);
                target.ChromPeak = peak;
                result.Target.AddTarget(target);
            }

            //Executes each grandchild ChromPeakAnalyzerIqWorkflow
            var children = result.Target.ChildTargets();
            var targetRemovalList = new List<IqTarget>();
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
        }

        protected internal override IqResult CreateIQResult(IqTarget target)
        {
            return new O16O18IqResult(target);
        }


        protected override void InitializeChromPeakAnalyzerWorkflow()
        {
            ChromPeakAnalyzerIqWorkflow = new O16O18ChromPeakAnalyzerIqWorkflow(Run, WorkflowParameters);
        }

    }
}
