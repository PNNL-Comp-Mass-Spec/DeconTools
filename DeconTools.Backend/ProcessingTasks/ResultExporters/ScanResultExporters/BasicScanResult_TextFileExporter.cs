using System.Collections.Generic;
using DeconTools.Backend.Core;

namespace DeconTools.Backend.ProcessingTasks.ResultExporters.ScanResultExporters
{
    public class BasicScanResult_TextFileExporter : ScanResult_TextFileExporter
    {

        #region Constructors
        public BasicScanResult_TextFileExporter(string fileName) : base(fileName) {}

        #endregion

        protected override string buildScansResultOutput(ScanResult result)
        {
            var data = new List<string>
            {
                result.ScanSet.PrimaryScanNumber.ToString(),
                DblToString(result.ScanTime, 4),
                result.SpectrumType.ToString(),
                DblToString(result.BasePeak.Height, 4, true),
                DblToString(result.BasePeak.XValue, 5),
                DblToString(result.ScanSet.TICValue, 4, true),
                result.NumPeaks.ToString(),
                result.NumIsotopicProfiles.ToString(),
                result.Description
            };


            return string.Join(Delimiter.ToString(), data);

        }

        protected override string buildHeaderLine()
        {
            var data = new List<string> {
                "scan_num",
                "scan_time",
                "type",
                "bpi",
                "bpi_mz",
                "tic",
                "num_peaks",
                "num_deisotoped",
                "info" };

            return string.Join(Delimiter.ToString(), data);
        }


    }
}
