using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend.ProcessingTasks.FitScoreCalculators;
using DeconTools.Backend.ProcessingTasks.PeakDetectors;
using DeconTools.Backend.ProcessingTasks.ResultValidators;
using DeconTools.Backend.ProcessingTasks.Smoothers;
using DeconTools.Backend.ProcessingTasks.TargetedFeatureFinders;
using DeconTools.Backend.ProcessingTasks.TheorFeatureGenerator;
using DeconTools.Workflows.Backend.Core.ChromPeakSelection;
using DeconTools.Workflows.Backend.FileIO;

namespace DeconTools.Workflows.Backend.Core
{
	/// <summary>
	/// Parent Level Workflow
	/// Decides which peak is best and sets IqResult for exporting necessary data
	/// </summary>
	public class ChromPeakDeciderIqWorkflow : BasicIqWorkflow
	{

		#region Constructors

		public ChromPeakDeciderIqWorkflow(Run run, TargetedWorkflowParameters parameters) : base(run, parameters)
		{
		}

		public ChromPeakDeciderIqWorkflow(TargetedWorkflowParameters parameters) : base(parameters)
		{
		}

		#endregion

		#region Properties

		public override TargetedWorkflowParameters WorkflowParameters { get; set; }

		#endregion

		public override ChromPeakSelectorBase CreateChromPeakSelector(TargetedWorkflowParameters workflowParameters)
		{
			ChromPeakSelectorParameters chromPeakSelectorParameters = new ChromPeakSelectorParameters();
			chromPeakSelectorParameters.NETTolerance = (float)workflowParameters.ChromNETTolerance;
			chromPeakSelectorParameters.NumScansToSum = workflowParameters.NumMSScansToSum;
			chromPeakSelectorParameters.PeakSelectorMode = workflowParameters.ChromPeakSelectorMode;
			chromPeakSelectorParameters.SummingMode = workflowParameters.SummingMode;
			chromPeakSelectorParameters.AreaOfPeakToSumInDynamicSumming = workflowParameters.AreaOfPeakToSumInDynamicSumming;
			chromPeakSelectorParameters.MaxScansSummedInDynamicSumming = workflowParameters.MaxScansSummedInDynamicSumming;

			var smartchrompeakSelectorParameters = new SmartChromPeakSelectorParameters(chromPeakSelectorParameters);
			smartchrompeakSelectorParameters.MSFeatureFinderType = DeconTools.Backend.Globals.TargetedFeatureFinderType.ITERATIVE;
			smartchrompeakSelectorParameters.MSPeakDetectorPeakBR = workflowParameters.MSPeakDetectorPeakBR;
			smartchrompeakSelectorParameters.MSPeakDetectorSigNoiseThresh = workflowParameters.MSPeakDetectorSigNoise;
			smartchrompeakSelectorParameters.MSToleranceInPPM = workflowParameters.MSToleranceInPPM;
			smartchrompeakSelectorParameters.NumChromPeaksAllowed = workflowParameters.NumChromPeaksAllowedDuringSelection;
			smartchrompeakSelectorParameters.MultipleHighQualityMatchesAreAllowed = workflowParameters.MultipleHighQualityMatchesAreAllowed;
			smartchrompeakSelectorParameters.IterativeTffMinRelIntensityForPeakInclusion = 0.66;

			return new IqSmartChromPeakSelector(smartchrompeakSelectorParameters);
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

					childResult.LCScanSetSelected = chromPeakResult.LCScanSetSelected;

					childResult.IqResultDetail.MassSpectrum = chromPeakResult.IqResultDetail.MassSpectrum;

					TrimData(childResult.IqResultDetail.MassSpectrum, childResult.Target.MZTheor, MsLeftTrimAmount, MsRightTrimAmount);

					childResult.ObservedIsotopicProfile = chromPeakResult.ObservedIsotopicProfile;

					childResult.FitScore = chromPeakResult.FitScore;

					childResult.InterferenceScore = chromPeakResult.InterferenceScore;

					childResult.MonoMassObs = chromPeakResult.ObservedIsotopicProfile == null
						                          ? 0
						                          : chromPeakResult.ObservedIsotopicProfile.MonoIsotopicMass;

					childResult.MZObs = chromPeakResult.ObservedIsotopicProfile == null
						                    ? 0
						                    : chromPeakResult.ObservedIsotopicProfile.MonoPeakMZ;

					var elutionTime = childResult.ChromPeakSelected == null ? 0d : ((ChromPeak) childResult.ChromPeakSelected).NETValue;
					childResult.ElutionTimeObs = elutionTime;

					childResult.Abundance = GetAbundance(childResult);
				}
			}

		}

	}
}
