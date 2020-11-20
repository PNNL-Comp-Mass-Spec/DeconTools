using System;
using DeconTools.Backend.Core;
using DeconTools.Backend.ProcessingTasks.PeakDetectors;
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

        protected override void DoPostInitialization()
        {
            base.DoPostInitialization();

            ChromPeakDetector = new ChromPeakDetectorMedianBased(WorkflowParameters.ChromPeakDetectorPeakBR, WorkflowParameters.ChromPeakDetectorSigNoise);
        }

        /// <summary>
        /// ChargeStateChildTopDownIqWorkflow
        /// Generates theoretical isotopic profile, XIC, and creates ChromPeakIqTargets based on peaks found
        /// </summary>
        /// <param name="result"></param>
        protected override void ExecuteWorkflow(IqResult result)
        {
            //Generate theoretical isotopic profile
            result.Target.TheorIsotopicProfile = TheorFeatureGen.GenerateTheorProfile(result.Target.EmpiricalFormula, result.Target.ChargeState);

            if (!Parser.CheckSequenceIntegrity(result.Target.Code))
            {
                ShiftIsotopicProfile(result.Target.TheorIsotopicProfile, result.Target.MonoMassTheor, result.Target.ChargeState);
            }

            //Generate XIC and smooth
            result.IqResultDetail.Chromatogram = ChromGen.GenerateChromatogram(Run, result.Target.TheorIsotopicProfile, result.Target.ElutionTimeTheor);
            result.IqResultDetail.Chromatogram = ChromSmoother.Smooth(result.IqResultDetail.Chromatogram);

            //Look for peaks in XIC
            result.ChromPeakList = ChromPeakDetector.FindPeaks(result.IqResultDetail.Chromatogram);
            ChromPeakDetector.CalculateElutionTimes(Run, result.ChromPeakList);
            ChromPeakDetector.FilterPeaksOnNET(WorkflowParameters.ChromNETTolerance, result.Target.ElutionTimeTheor, result.ChromPeakList);

            var tempMinScanWithinTol = (int) Run.NetAlignmentInfo.GetScanForNet(result.Target.ElutionTimeTheor - WorkflowParameters.ChromNETTolerance);
            var tempMaxScanWithinTol = (int)Run.NetAlignmentInfo.GetScanForNet(result.Target.ElutionTimeTheor + WorkflowParameters.ChromNETTolerance);
            var tempCenterTol = (int)Run.NetAlignmentInfo.GetScanForNet(result.Target.ElutionTimeTheor);

            result.NumChromPeaksWithinTolerance = result.ChromPeakList.Count;

            //General peak information output written to console.
            Console.WriteLine("SmartPeakSelector --> NETTolerance= " + WorkflowParameters.ChromNETTolerance + ";  chromMinCenterMax= " +
                              tempMinScanWithinTol + "\t" + tempCenterTol + "" +
                              "\t" + tempMaxScanWithinTol);
            Console.WriteLine("MT= " + result.Target.ID + ";z= " + result.Target.ChargeState + "; mz= " + result.Target.MZTheor.ToString("0.000") +
                              ";  ------------------------- PeaksWithinTol = " + result.ChromPeakList.Count);

            //Creates a ChromPeakIqTarget for each peak found
            foreach (var peak in result.ChromPeakList)
            {
                var chromPeak = (ChromPeak)peak;
                var target = new ChromPeakIqTarget(new ChromPeakAnalyzerIqWorkflow(Run, WorkflowParameters));
                TargetUtilities.CopyTargetProperties(result.Target, target, false);
                target.ChromPeak = chromPeak;
                result.Target.AddTarget(target);
            }

            //Executes each grandchild ChromPeakAnalyzerIqWorkflow
            var children = result.Target.ChildTargets();

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

        protected void ShiftIsotopicProfile (IsotopicProfile profile, double monoMass, int chargeState)
        {
            var monoMZ = (monoMass/chargeState) + DeconTools.Backend.Globals.PROTON_MASS;

            var mzDifference = profile.MonoPeakMZ - monoMZ;

            profile.MonoIsotopicMass = monoMass;
            profile.MonoPeakMZ = monoMZ;

            foreach (var peak in profile.Peaklist)
            {
                peak.XValue = peak.XValue - mzDifference;
            }
        }
    }
}
