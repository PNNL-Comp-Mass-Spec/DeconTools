using System;
using System.Collections.Generic;
using DeconTools.Backend.Core;
using DeconTools.Backend.Utilities.IqLogger;

namespace DeconTools.Workflows.Backend.Core
{
	/// <summary>
	/// Generates Theoretical Isotopic Profile, Chromatogram, and finds peaks
	/// Grand Child ChromPeakIqTargets are created for all peaks found.
	/// </summary>
	public class ChargeStateChildIqWorkflow: BasicIqWorkflow
	{
		#region Constructors

		public ChargeStateChildIqWorkflow(Run run, TargetedWorkflowParameters parameters) : base(run, parameters)
		{
			TargetUtilities = new IqTargetUtilities();
		}

		public ChargeStateChildIqWorkflow(TargetedWorkflowParameters parameters) : base(parameters)
		{
			TargetUtilities = new IqTargetUtilities();
		}

		#endregion

		#region Properties

		protected IqTargetUtilities TargetUtilities;


        protected ChromPeakAnalyzerIqWorkflow ChromPeakAnalyzerIqWorkflow { get; set; }


		#endregion

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

            int tempMinScanWithinTol = (int)Run.NetAlignmentInfo.GetScanForNet(result.Target.ElutionTimeTheor - WorkflowParameters.ChromNETTolerance);
            int tempMaxScanWithinTol = (int)Run.NetAlignmentInfo.GetScanForNet(result.Target.ElutionTimeTheor + WorkflowParameters.ChromNETTolerance);
            int tempCenterTol = (int)Run.NetAlignmentInfo.GetScanForNet(result.Target.ElutionTimeTheor);

			result.NumChromPeaksWithinTolerance = result.ChromPeakList.Count;

			//General peak information output written to console.
			IqLogger.Log.Debug("SmartPeakSelector --> NETTolerance= " + WorkflowParameters.ChromNETTolerance + ";  chromMinCenterMax= " +
							  tempMinScanWithinTol + "\t" + tempCenterTol + "" + "\t" + tempMaxScanWithinTol + Environment.NewLine);
            IqLogger.Log.Debug("MT= " + result.Target.ID + ";z= " + result.Target.ChargeState + "; mz= " + result.Target.MZTheor.ToString("0.000") +
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

	    protected virtual void InitializeChromPeakAnalyzerWorkflow()
	    {
	        ChromPeakAnalyzerIqWorkflow = new ChromPeakAnalyzerIqWorkflow(Run, WorkflowParameters);
	    }
	}
}
