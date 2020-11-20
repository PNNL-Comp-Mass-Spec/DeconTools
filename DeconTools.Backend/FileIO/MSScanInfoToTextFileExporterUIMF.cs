using System.Collections.Generic;
using DeconTools.Backend.Core;

namespace DeconTools.Backend.FileIO
{
    public class MSScanInfoToTextFileExporterUIMF : TextFileExporter<ScanResult>
    {
        #region Constructors

        public MSScanInfoToTextFileExporterUIMF(string fileName) : base(fileName, ',') { }

        public MSScanInfoToTextFileExporterUIMF(string fileName, char delimiter) : base(fileName, delimiter) { }

        #endregion

        #region Private Methods
        protected override string buildResultOutput(ScanResult result)
        {
            var uimfScanResult = (UimfScanResult)result;

            var data = new List<string>
            {
                uimfScanResult.ScanSet.PrimaryScanNumber.ToString(),
                DblToString(uimfScanResult.ScanTime, 3),
                result.SpectrumType.ToString(),
                DblToString(uimfScanResult.BasePeak.Height, 4, true),
                DblToString(uimfScanResult.BasePeak.XValue, 5),
                DblToString(uimfScanResult.TICValue, 4, true),
                uimfScanResult.NumPeaks.ToString(),
                uimfScanResult.NumIsotopicProfiles.ToString(),
                DblToString(uimfScanResult.FramePressureUnsmoothed, 4),
                DblToString(uimfScanResult.FramePressureSmoothed, 4)
            };

            return string.Join(Delimiter.ToString(), data);
        }

        protected override string buildHeaderLine()
        {
            var data = new List<string>
            {
                "frame_num",
                "frame_time",
                "type",
                "bpi",
                "bpi_mz",
                "tic",
                "num_peaks",
                "num_deisotoped",
                "frame_pressure_unsmoothed",
                "frame_pressure_smoothed"
            };

            return string.Join(Delimiter.ToString(), data);
        }
        #endregion

    }
}
