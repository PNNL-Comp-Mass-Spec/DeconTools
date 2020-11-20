using System.Collections.Generic;
using DeconTools.Backend.Core;

namespace DeconTools.Backend.ProcessingTasks.ResultExporters.ScanResultExporters
{
    public class UIMFScanResult_TextFileExporter : ScanResult_TextFileExporter
    {
        #region Constructors
        public UIMFScanResult_TextFileExporter(string fileName) : base(fileName) { }
        #endregion

        #region Public Methods
        #endregion

        #region Private Methods
        #endregion

        protected override string buildScansResultOutput(ScanResult result)
        {
            var uimfScanResult = (UimfScanResult)result;

            var data = new List<string>
            {
                //we want to report the unique 'FrameNum', not the non-unique 'Frame_index');
                uimfScanResult.LCScanNum.ToString(),
                DblToString(uimfScanResult.ScanTime, 3),
                result.SpectrumType.ToString(),
                DblToString(uimfScanResult.BasePeak.Height, 4, true),
                DblToString(uimfScanResult.BasePeak.XValue, 5),
                DblToString(uimfScanResult.TICValue, 4, true),
                uimfScanResult.NumPeaks.ToString(),
                uimfScanResult.NumIsotopicProfiles.ToString(),
                DblToString(uimfScanResult.FramePressureUnsmoothed, 5),
                DblToString(uimfScanResult.FramePressureSmoothed, 5)
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
                "FramePressureUnsmoothed",
                "FramePressureSmoothed"
            };

            return string.Join(Delimiter.ToString(), data);
        }
    }
}
