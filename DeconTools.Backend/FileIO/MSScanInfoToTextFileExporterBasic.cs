using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            this.Name = "MSScanInfoToTextFileExporterBasic";
        }

        #endregion

        #region Private Methods
        protected override string buildResultOutput(ScanResult result)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(result.ScanSet.PrimaryScanNumber);
            sb.Append(Delimiter);
            sb.Append(result.ScanTime.ToString("0.####"));
            sb.Append(Delimiter);
            sb.Append(result.SpectrumType);
            sb.Append(Delimiter);
            sb.Append(result.BasePeak.Height);
            sb.Append(Delimiter);
            sb.Append(result.BasePeak.XValue.ToString("0.#####"));
            sb.Append(Delimiter);
            sb.Append(result.ScanSet.TICValue);
            sb.Append(Delimiter);
            sb.Append(result.NumPeaks);
            sb.Append(Delimiter);
            sb.Append(result.NumIsotopicProfiles);

            return sb.ToString();

        }

        protected override string buildHeaderLine()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("scan_num");
            sb.Append(Delimiter);
            sb.Append("scan_time");
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

            return sb.ToString();
        }
        #endregion

    }
}
