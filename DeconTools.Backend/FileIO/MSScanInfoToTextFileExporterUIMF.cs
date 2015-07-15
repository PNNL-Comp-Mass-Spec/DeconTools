using System.Text;
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
            StringBuilder sb = new StringBuilder();

            UimfScanResult uimfScanResult = (UimfScanResult)result;
            sb.Append(uimfScanResult.ScanSet.PrimaryScanNumber);
            sb.Append(Delimiter);
            sb.Append(DblToString(uimfScanResult.ScanTime, 3));
            sb.Append(Delimiter);
            sb.Append(result.SpectrumType);
            sb.Append(Delimiter);
            sb.Append(DblToString(uimfScanResult.BasePeak.Height, 4, true));
            sb.Append(Delimiter);
            sb.Append(DblToString(uimfScanResult.BasePeak.XValue, 5));
            sb.Append(Delimiter);
            sb.Append(DblToString(uimfScanResult.TICValue, 4, true));
            sb.Append(Delimiter);
            sb.Append(uimfScanResult.NumPeaks);
            sb.Append(Delimiter);
            sb.Append(uimfScanResult.NumIsotopicProfiles);
            sb.Append(Delimiter);
            sb.Append(DblToString(uimfScanResult.FramePressureUnsmoothed, 4));
            sb.Append(Delimiter);
            sb.Append(DblToString(uimfScanResult.FramePressureSmoothed, 4));

            return sb.ToString();

        }

        protected override string buildHeaderLine()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("frame_num");
            sb.Append(Delimiter);
            sb.Append("frame_time");
            sb.Append(Delimiter);
            sb.Append("type");
            sb.Append(Delimiter);
            sb.Append("bpi");
            sb.Append(Delimiter);
            sb.Append("bpi_mz");
            sb.Append(Delimiter);
            sb.Append("tic");
            sb.Append(Delimiter);
            sb.Append("num_peaks");
            sb.Append(Delimiter);
            sb.Append("num_deisotoped");
            sb.Append(Delimiter);
            sb.Append("frame_pressure_unsmoothed");
            sb.Append(Delimiter);
            sb.Append("frame_pressure_smoothed");

            return sb.ToString();
        }
        #endregion

    }
}
