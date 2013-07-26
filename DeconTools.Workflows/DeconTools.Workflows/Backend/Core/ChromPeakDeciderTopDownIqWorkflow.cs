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
		public ChromPeakDeciderTopDownIqWorkflow(Run run, TargetedWorkflowParameters parameters) : base(run, parameters)
		{
		}

		public ChromPeakDeciderTopDownIqWorkflow(TargetedWorkflowParameters parameters) : base(parameters)
		{
		}

		protected override void DoMainInitialization()
		{
			base.DoMainInitialization();
			ChromatogramCorrelator = new IqChargeCorrelator(WorkflowParameters.ChromSmootherNumPointsInSmooth);
		}

		protected internal override IqResult CreateIQResult(IqTarget target)
		{
			return new TopDownIqResult(target);
		}

		//Captured what the original IqWorkflow does
		protected override void ExecuteWorkflow(IqResult result)
		{
			var peakSelector = ChromPeakSelector as IqSmartChromPeakSelector;

			//Executes the ChargeState level children workflows
			var children = result.Target.ChildTargets();
			foreach (IqTarget child in children)
			{
				child.DoWorkflow();
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
			}
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
			//foreach (var corr in topDownIqResult.ChargeCorrelationData.CorrelationData)
			//{
			//    Console.WriteLine("Reference: Charge: " + corr.ReferenceTarget.ChargeState + "-----------------------------------------------------");
			//    foreach (KeyValuePair<ChromPeakIqTarget, ChromCorrelationData> item in corr.PeakCorrelationData)
			//    {
			//        Console.WriteLine("Target: Charge: " + item.Key.ChargeState + " R2: " + item.Value.RSquaredValsMedian);
			//    }
			//}
		}

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
	}
}
