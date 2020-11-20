using System.Collections.Generic;
using DeconTools.Backend.Core;

namespace DeconTools.Backend.FileIO
{
    public class MSScanInfoToTextFileExporterBasic : TextFileExporter<ScanResult>
    {
        #region Constructors
        public MSScanInfoToTextFileExporterBasic(string fileName) : base(fileName, ',') { }

        public MSScanInfoToTextFileExporterBasic(string fileName, char delimiter)
            : base(fileName, delimiter)
        {
            Name = "MSScanInfoToTextFileExporterBasic";
        }

        #endregion

        #region Private Methods
        protected override string buildResultOutput(ScanResult result)
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
                result.NumIsotopicProfiles.ToString()
            };

            return string.Join(Delimiter.ToString(), data);
        }

        protected override string buildHeaderLine()
        {
            var data = new List<string> {
                "scan_num",
                "scan_time",
                "type", "bpi",
                "bpi_mz",
                "tic",
                "num_peaks",
                "num_deisotoped" };

            return string.Join(Delimiter.ToString(), data);
        }
        #endregion

    }
}
