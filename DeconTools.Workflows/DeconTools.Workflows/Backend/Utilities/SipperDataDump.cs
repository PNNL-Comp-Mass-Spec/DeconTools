using System.IO;
using DeconTools.Backend.Core;
using DeconTools.Workflows.Backend.Core;

namespace DeconTools.Workflows.Backend.Utilities
{
    /// <summary>
    /// Used to dump data in a format where sipper can read it.
    /// </summary>
    public static class SipperDataDump
    {
        public static bool OutputResults { get; set; }

        public static string Outfile { get; set; }

        public static void DataDumpSetup(string filename)
        {
            OutputResults = true;
            Outfile = filename;
            if (File.Exists(Outfile))
            {
                File.Delete(Outfile);
            }

            using (var sipper = File.AppendText(Outfile))
            {
                sipper.WriteLine("TargetID" + "\t" + "PRSM_ID" + "\t" + "ChargeState" + "\t" + "Code" + "\t" + "EmpiricalFormula" + "\t" + "MonoMZ" +
                                 "\t" + "MonoisotopicMass" + "\t" + "TargetScan" + "\t" + "ObservedScan" +
                                 "\t" + "Scan" + "\t" + "NETError" + "\t" + "MassError" + "\t" + "FitScore" + "\t" +
                                 "ChromCorrMedian" + "\t" + "ChromCorrAverage" + "\t" + "ChromCorrStdev" + "\t" +
                                 "CorrelationData" + "\t" + "Abundance" + "\t" + "Flagged" + "\t" + "Status");
            }
        }

        /// <summary>
        /// Temporary Data Dumping Point
        /// Will be removed when GUI is developed
        /// </summary>
        /// <param name="input"></param>
        /// <param name="run"></param>
        public static void DataDump(IqTarget input, Run run)
        {
            //Temporary Data Dumping Point
            //Data Dump also in TopDownIqTesting and ChromCorrelationData
            if (!(input is ChromPeakIqTarget target))
            {
                return;
            }

            var status = "UNK";
            var result = target.GetResult();

            if (!(result.Target.ParentTarget is IqChargeStateTarget parent))
            {
                return;
            }

            if (parent.ObservedScan != -1)
            {
                if (target.ChromPeak.XValue - target.ChromPeak.Width / 2 <= parent.ObservedScan &&
                    target.ChromPeak.XValue + target.ChromPeak.Width / 2 >= parent.ObservedScan)
                {
                    status = "T-POS";
                }
                else
                {
                    status = "T-NEG";
                }
            }

            using (var sipper = File.AppendText(Outfile))
            {
                sipper.WriteLine(target.ID + "\t" + parent.AlternateID + "\t" + target.ChargeState + "\t" + target.Code + "\t" + target.EmpiricalFormula + "\t" +
                                 target.MZTheor.ToString("0.0000") + "\t" + target.MonoMassTheor + "\t" +
                                 run.NetAlignmentInfo.GetScanForNet(target.ElutionTimeTheor) +
                                 "\t" + parent.ObservedScan + "\t" + target.ChromPeak.XValue.ToString("0.00") + "\t" +
                                 result.NETError.ToString("0.0000") + "\t" + result.MassErrorBefore.ToString("0.0000") + "\t" +
                                 result.FitScore.ToString("0.0000") + "\t" + result.CorrelationData.RSquaredValsMedian + "\t" +
                                 result.CorrelationData.RSquaredValsAverage + "\t" + result.CorrelationData.RSquaredValsStDev + "\t" +
                                 result.CorrelationData.ToStringWithDetails() + "\t" + target.GetResult().Abundance + "\t" + result.IsIsotopicProfileFlagged + "\t" +
                                 status);
            }
        }
    }
}
