using System;
using System.Collections.Generic;
using System.Linq;
using DeconTools.Backend.Core;
using DeconTools.Workflows.Backend.Core.ChromPeakSelection;
using DeconTools.Workflows.Backend.FileIO;

namespace DeconTools.Workflows.Backend.Core
{
    public class ChromPeakDeciderTopDownIqWorkflow : ChromPeakDeciderIqWorkflow
    {
        #region Constructors

        public ChromPeakDeciderTopDownIqWorkflow(Run run, TargetedWorkflowParameters parameters) : base(run, parameters)
        {
        }

        public ChromPeakDeciderTopDownIqWorkflow(TargetedWorkflowParameters parameters) : base(parameters)
        {
        }

        #endregion

        #region Protected Methods

        protected override void DoMainInitialization()
        {
            base.DoMainInitialization();
            ChromatogramCorrelator = new IqChargeCorrelator(WorkflowParameters.ChromSmootherNumPointsInSmooth);
        }

        protected internal override IqResult CreateIQResult(IqTarget target)
        {
            return new TopDownIqResult(target);
        }

        public override IqResultExporter CreateExporter()
        {
            return new TopDownIqResultExporter();
        }

        /// <summary>
        /// Parent level workflow for Top-Down IQ analysis
        /// </summary>
        /// <param name="result"></param>
        protected override void ExecuteWorkflow(IqResult result)
        {
            //Executes the ChargeState level children workflows
            var children = result.Target.ChildTargets();

            foreach (var child in children)
            {
                child.DoWorkflow();
            }

            PerformChargeCorrelation(result);
            var referenceTarget = TargetSelector((TopDownIqResult) result);

            ExpandChargeRange((TopDownIqTarget) result.Target, referenceTarget);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Returns a list of all ChromPeakIqTarget objects under a parent target
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        private List<ChromPeakIqTarget> GetAllChromPeakIqTargets(IqResult result)
        {
            var chromPeakTargets = new List<ChromPeakIqTarget>();
            var children = result.Target.ChildTargets();
            foreach (var child in children)
            {
                var grandchildTargets = child.ChildTargets();
                foreach (ChromPeakIqTarget target in grandchildTargets)
                {
                    chromPeakTargets.Add(target);
                }
            }
            return chromPeakTargets;
        }

        /// <summary>
        /// Expands charge range to cover all instances of a given sequence
        /// </summary>
        /// <param name="parentTarget"></param>
        /// <param name="referenceTarget"></param>
        private void ExpandChargeRange(TopDownIqTarget parentTarget, ChromPeakIqTarget referenceTarget)
        {
            var childWorkflow = new ChargeStateChildTopDownIqWorkflow(Run, WorkflowParameters);

            var childTargets = parentTarget.ChildTargets().ToArray();

            var fitTolerance = 0.5;
            var correlationTolerance = 0.95;

            var minCharge = (IqChargeStateTarget) childTargets.First();
            var maxCharge = (IqChargeStateTarget) childTargets.Last();

            if (minCharge.GetResult().IqResultDetail.Chromatogram != null)
            {
                var charge = minCharge.ChargeState;
                var extendDown = true;
                while (extendDown)
                {
                    var extend = new IqChargeStateTarget(childWorkflow);
                    charge = charge - 1;
                    extend.ChargeState = charge;
                    parentTarget.AddTarget(extend);
                    extend.RefineTarget();
                    extend.DoWorkflow();
                    extendDown = ChargeExpansionPeakSelection(parentTarget, referenceTarget, extend, correlationTolerance, fitTolerance);
                }
            }

            if (maxCharge.GetResult().IqResultDetail.Chromatogram != null)
            {
                var charge = maxCharge.ChargeState;
                var extendUp = true;
                while (extendUp)
                {
                    var extend = new IqChargeStateTarget(childWorkflow);
                    charge = charge + 1;
                    extend.ChargeState = charge;
                    parentTarget.AddTarget(extend);
                    extend.RefineTarget();
                    extend.DoWorkflow();
                    extendUp = ChargeExpansionPeakSelection(parentTarget, referenceTarget, extend, correlationTolerance, fitTolerance);
                }
            }

            parentTarget.SortChildTargetsByCharge();
        }

        /// <summary>
        /// Selects a peak that correlates with the reference target and returns a bool to continue extending the charge range
        /// </summary>
        /// <param name="parentTarget"></param>
        /// <param name="referenceTarget"></param>
        /// <param name="chargeTarget"></param>
        /// <param name="correlationCutoff"></param>
        /// <param name="fitCutoff"></param>
        /// <returns></returns>
        private bool ChargeExpansionPeakSelection(TopDownIqTarget parentTarget, ChromPeakIqTarget referenceTarget, IqChargeStateTarget chargeTarget, double correlationCutoff, double fitCutoff)
        {
            var iqChargeCorrelator = ChromatogramCorrelator as IqChargeCorrelator;

            var peakTargets = chargeTarget.ChildTargets();
            var sortedPeakTargets = peakTargets.OrderBy(x => x.GetResult().FitScore);

            foreach (ChromPeakIqTarget target in sortedPeakTargets)
            {
                if (referenceTarget.ChromPeak != null)
                {
                    var minScan = referenceTarget.ChromPeak.XValue - (0.5*referenceTarget.ChromPeak.Width);
                    var maxScan = referenceTarget.ChromPeak.XValue + (0.5*+referenceTarget.ChromPeak.Width);
                    if ((target.ChromPeak.XValue > minScan) && (target.ChromPeak.XValue < maxScan))
                    {
                        var correlation = iqChargeCorrelator.PairWiseChargeCorrelation(referenceTarget, target, Run, 3);
                        if (correlation > correlationCutoff && target.GetResult().FitScore < fitCutoff)
                        {
                            UpdateSelection(target);
                            var parentResult = parentTarget.GetResult() as TopDownIqResult;
                            foreach (var item in parentResult.ChargeCorrelationData.CorrelationData)
                            {
                                if (referenceTarget == item.ReferenceTarget)
                                {
                                    var corrItem = new ChromCorrelationDataItem(0, 0, correlation);
                                    var corr = new ChromCorrelationData();
                                    corr.AddCorrelationData(corrItem);
                                    item.PeakCorrelationData.Add(target, corr);
                                }
                            }

                            return true;
                        }
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Performs charge correlation on parent level target
        /// </summary>
        /// <param name="result"></param>
        private void PerformChargeCorrelation(IqResult result)
        {
            var iqChargeCorrelator = ChromatogramCorrelator as IqChargeCorrelator;
            var topDownIqResult = result as TopDownIqResult;
            if (iqChargeCorrelator != null && topDownIqResult != null)
            {
                topDownIqResult.ChargeCorrelationData = iqChargeCorrelator.CorrelateData(GetAllChromPeakIqTargets(result), Run);
            }
            else
            {
                throw new Exception("IqChargeCorrelator and/or TopDownIqResult is not compatible");
            }
        }

        /// <summary>
        /// Selects charge correlating group based on composite score
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        private ChromPeakIqTarget TargetSelector(TopDownIqResult result)
        {
            double bestScore = 0;
            var bestScoringGroup = new ChargeCorrelationItem();
            var corrData = result.ChargeCorrelationData.CorrelationData;

            foreach (var group in corrData)
            {
                double groupScore = 0;
                var entries = group.PeakCorrelationData;
                foreach (var entry in entries)
                {
                    groupScore += (entry.Value.RSquaredValsMedian.HasValue) ? entry.Value.RSquaredValsMedian.Value : 0;
                    groupScore += (1 - entry.Key.GetResult().FitScore);
                }

                if (groupScore > bestScore)
                {
                    bestScore = groupScore;
                    bestScoringGroup = group;
                }
            }

            bestScoringGroup.SelectedTargetGrouping = true;
            result.SelectedCorrelationGroup = bestScoringGroup;
            foreach (var entry in bestScoringGroup.PeakCorrelationData)
            {
                UpdateSelection(entry.Key);
            }

            return bestScoringGroup.ReferenceTarget;
        }

        /// <summary>
        /// Updates parent charge state result based on the selected chrom peak target
        /// </summary>
        /// <param name="chromPeakTarget"></param>
        private void UpdateSelection(ChromPeakIqTarget chromPeakTarget)
        {
            var childResult = chromPeakTarget.ParentTarget.GetResult();

            var chromPeakResult = chromPeakTarget.GetResult();

            childResult.ChromPeakSelected = chromPeakTarget.ChromPeak;

            childResult.LcScanObs = chromPeakResult.LcScanObs;

            childResult.LCScanSetSelected = chromPeakResult.LCScanSetSelected;

            childResult.IqResultDetail.MassSpectrum = chromPeakResult.IqResultDetail.MassSpectrum;

            TrimData(childResult.IqResultDetail.MassSpectrum, childResult.Target.MZTheor, MsLeftTrimAmount, MsRightTrimAmount);

            childResult.ObservedIsotopicProfile = chromPeakResult.ObservedIsotopicProfile;

            childResult.FitScore = chromPeakResult.FitScore;

            childResult.NETError = chromPeakResult.NETError;

            childResult.InterferenceScore = chromPeakResult.InterferenceScore;

            childResult.CorrelationData = chromPeakResult.CorrelationData;

            childResult.MonoMassObs = chromPeakResult.ObservedIsotopicProfile == null
                                          ? 0
                                          : chromPeakResult.ObservedIsotopicProfile.MonoIsotopicMass;

            childResult.MZObs = chromPeakResult.ObservedIsotopicProfile == null
                                    ? 0
                                    : chromPeakResult.ObservedIsotopicProfile.MonoPeakMZ;

            childResult.MZObsCalibrated = chromPeakResult.ObservedIsotopicProfile == null
                                              ? 0
                                              : Run.GetAlignedMZ(childResult.MZObs, chromPeakResult.LcScanObs);

            childResult.MonoMassObsCalibrated = (childResult.MZObsCalibrated - DeconTools.Backend.Globals.PROTON_MASS)*
                                                childResult.MZObsCalibrated/childResult.Target.ChargeState;

            childResult.MassErrorBefore = chromPeakResult.ObservedIsotopicProfile == null
                                              ? 0
                                              : chromPeakResult.MassErrorBefore;

            childResult.MassErrorAfter = (childResult.MZObsCalibrated - childResult.Target.MZTheor)/childResult.Target.MZTheor*
                                         1e6;

            var elutionTime = childResult.ChromPeakSelected == null ? 0d : ((ChromPeak) childResult.ChromPeakSelected).NETValue;
            childResult.ElutionTimeObs = elutionTime;

            childResult.Abundance = GetAbundance(childResult);
        }

        #endregion
    }
}
