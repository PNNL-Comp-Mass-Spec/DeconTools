using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core;
using DeconTools.Workflows.Backend.Utilities.IqCodeParser;

namespace DeconTools.Workflows.Backend.Core
{
	/// <summary>
	///Checks the sequences integrity when dealing with top down data that could contain extremely large PTMS 
	/// </summary>
	public class ChargeStateChildTopDownIqWorkflow : ChargeStateChildIqWorkflow
	{
		#region Constructors

		public ChargeStateChildTopDownIqWorkflow(Run run, TargetedWorkflowParameters parameters) : base(run, parameters)
		{
			Parser = new IqCodeParser();
		}

		public ChargeStateChildTopDownIqWorkflow(TargetedWorkflowParameters parameters) : base(parameters)
		{
			Parser = new IqCodeParser();
		}

		#endregion

		#region Properties

		protected IqCodeParser Parser;

		#endregion


		protected override void ExecuteWorkflow(IqResult result)
		{
			result.Target.TheorIsotopicProfile = TheorFeatureGen.GenerateTheorProfile(result.Target.EmpiricalFormula, result.Target.ChargeState);

			if (!Parser.CheckSequenceIntegrity(result.Target.Code))
			{
				ShiftIsotopicProfile(result.Target.TheorIsotopicProfile, result.Target.MonoMassTheor, result.Target.ChargeState);
			}

			result.IqResultDetail.Chromatogram = ChromGen.GenerateChromatogram(Run, result.Target.TheorIsotopicProfile, result.Target.ElutionTimeTheor);

			result.IqResultDetail.Chromatogram = ChromSmoother.Smooth(result.IqResultDetail.Chromatogram);

			result.ChromPeakList = ChromPeakDetector.FindPeaks(result.IqResultDetail.Chromatogram);

			ChromPeakDetector.CalculateElutionTimes(Run, result.ChromPeakList);

			ChromPeakDetector.FilterPeaksOnNET(WorkflowParameters.ChromNETTolerance, result.Target.ElutionTimeTheor, result.ChromPeakList);

		    int tempMinScanWithinTol = (int) Run.NetAlignmentInfo.GetScanForNet(result.Target.ElutionTimeTheor - WorkflowParameters.ChromNETTolerance);
            int tempMaxScanWithinTol = (int)Run.NetAlignmentInfo.GetScanForNet(result.Target.ElutionTimeTheor + WorkflowParameters.ChromNETTolerance);
            int tempCenterTol = (int)Run.NetAlignmentInfo.GetScanForNet(result.Target.ElutionTimeTheor);

			result.NumChromPeaksWithinTolerance = result.ChromPeakList.Count;

			//General peak information output written to console.
			Console.WriteLine("SmartPeakSelector --> NETTolerance= " + WorkflowParameters.ChromNETTolerance + ";  chromMinCenterMax= " +
							  tempMinScanWithinTol + "\t" + tempCenterTol + "" +
							  "\t" + tempMaxScanWithinTol);
			Console.WriteLine("MT= " + result.Target.ID + ";z= " + result.Target.ChargeState + "; mz= " + result.Target.MZTheor.ToString("0.000") +
							  ";  ------------------------- PeaksWithinTol = " + result.ChromPeakList.Count);

			//Creates a ChromPeakIqTarget for each peak found
			foreach (ChromPeak peak in result.ChromPeakList)
			{
				ChromPeakIqTarget target = new ChromPeakIqTarget(new ChromPeakAnalyzerIqWorkflow(Run, WorkflowParameters));
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

				
				//Selects grandchildren with extremely poor metric scores for removal
				IqResult childResult = child.GetResult();
				if ((childResult.FitScore >= .8) || (childResult.CorrelationData.RSquaredValsMedian <= .15))
				{
					targetRemovalList.Add(child);
				}
				
			}

			
			//Removes the poorly scoring grandchild ChromPeakIqTargets
			foreach (IqTarget iqTarget in targetRemovalList)
			{
				result.RemoveResult(iqTarget.GetResult());
				result.Target.RemoveTarget(iqTarget);
			}
			

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

		protected void ShiftIsotopicProfile (IsotopicProfile profile, double monoMass, int chargeState)
		{
			double monoMZ = (monoMass/chargeState) + DeconTools.Backend.Globals.PROTON_MASS;

			double mzDifference = profile.MonoPeakMZ - monoMZ;

			profile.MonoIsotopicMass = monoMass;
			profile.MonoPeakMZ = monoMZ;
			
			foreach (MSPeak peak in profile.Peaklist)
			{
				peak.XValue = peak.XValue - mzDifference;
			}
		}
	}
}
