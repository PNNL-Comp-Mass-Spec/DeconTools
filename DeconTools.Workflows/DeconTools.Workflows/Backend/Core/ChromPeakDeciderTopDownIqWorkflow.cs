using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core;
using DeconTools.Workflows.Backend.Core.ChromPeakSelection;

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


		/// <summary>
		/// Parent level workflow for Top-Down IQ analysis
		/// </summary>
		/// <param name="result"></param>
		protected override void ExecuteWorkflow(IqResult result)
		{
			//Executes the ChargeState level children workflows
			IEnumerable<IqTarget> children = result.Target.ChildTargets();

			foreach (IqTarget child in children)
			{
				child.DoWorkflow();
			}

			PerformChargeCorrelation(result);
			TargetSelector((TopDownIqResult) result);

			//ExpandChargeRange((TopDownIqTarget) result.Target);

			
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
			List<ChromPeakIqTarget> chromPeakTargets = new List<ChromPeakIqTarget>();
			var children = result.Target.ChildTargets();
			foreach (IqTarget child in children)
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
		/// Selects best peak for each chargestate and updates child IqResult
		/// </summary>
		/// <param name="child"></param>
		private void SelectChildPeak(IqTarget child)
		{
			var peakSelector = ChromPeakSelector as IqSmartChromPeakSelector;

			IqResult childResult = child.GetResult();

			List<ChromPeakIqTarget> peakTargetList = new List<ChromPeakIqTarget>();
			var peakTargets = child.ChildTargets();
			foreach (ChromPeakIqTarget target in peakTargets)
			{
				peakTargetList.Add(target);
			}

			bool filterOutFlagged = childResult.Target.TheorIsotopicProfile.GetIndexOfMostIntensePeak() == 0;

			ChromPeakIqTarget chromPeakTarget = peakSelector.SelectBestPeak(peakTargetList, filterOutFlagged);

			if (chromPeakTarget != null)
			{

				IqResult chromPeakResult = chromPeakTarget.GetResult();

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


				childResult.MonoMassObsCalibrated = (childResult.MZObsCalibrated - DeconTools.Backend.Globals.PROTON_MASS) *
													childResult.MZObsCalibrated / childResult.Target.ChargeState;

				childResult.MassErrorBefore = chromPeakResult.ObservedIsotopicProfile == null
												  ? 0
												  : chromPeakResult.MassErrorBefore;


				childResult.MassErrorAfter = (childResult.MZObsCalibrated - childResult.Target.MZTheor) / childResult.Target.MZTheor *
											 1e6;


				var elutionTime = childResult.ChromPeakSelected == null ? 0d : ((ChromPeak)childResult.ChromPeakSelected).NETValue;
				childResult.ElutionTimeObs = elutionTime;

				childResult.Abundance = GetAbundance(childResult);
			}
		}


		/// <summary>
		/// Expands charge range to cover all instances of a given sequence
		/// </summary>
		/// <param name="parent"></param>
		private void ExpandChargeRange(TopDownIqTarget parent)
		{
			double fitTolerance = .25;

			ChargeStateChildTopDownIqWorkflow childWorkflow = new ChargeStateChildTopDownIqWorkflow(Run, WorkflowParameters);

			parent.SortChildTargetsByCharge();
			var children = parent.ChildTargets();

			int childCount = children.Count();
			double[] fitScoreList = new double[childCount];
			int i = 0;

			foreach (IqTarget child in children)
			{
				fitScoreList[i] = child.GetResult().FitScore;
				i++;
			}

			double[] sortedList = (double[]) fitScoreList.Clone();
			fitScoreList.CopyTo(sortedList, 0);
			Array.Sort(sortedList);
			double medianFit = sortedList[childCount/2];

			if (medianFit < fitTolerance)
			{
				if (fitScoreList[0] < fitTolerance && fitScoreList[1] < fitTolerance)
				{
					
				}
				if (fitScoreList[childCount - 1] < fitTolerance && fitScoreList[childCount - 2] < fitTolerance)
				{

				}
			}

			parent.SortChildTargetsByCharge();
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


		private void TargetSelector(TopDownIqResult result)
		{
			double bestScore = 0;
			ChargeCorrelationItem bestScoringGroup = new ChargeCorrelationItem();
			List<ChargeCorrelationItem> corrData = result.ChargeCorrelationData.CorrelationData;

			foreach (ChargeCorrelationItem group in corrData)
			{
				double groupScore = 0;
				var entries = group.PeakCorrelationData;
				foreach (KeyValuePair<ChromPeakIqTarget, ChromCorrelationData> entry in entries)
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

			foreach (KeyValuePair<ChromPeakIqTarget, ChromCorrelationData> entry in bestScoringGroup.PeakCorrelationData)
			{
				UpdateSelection(entry.Key);
			}

		}


		private void UpdateSelection(ChromPeakIqTarget chromPeakTarget)
		{

			IqResult childResult = chromPeakTarget.ParentTarget.GetResult();

			IqResult chromPeakResult = chromPeakTarget.GetResult();

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
