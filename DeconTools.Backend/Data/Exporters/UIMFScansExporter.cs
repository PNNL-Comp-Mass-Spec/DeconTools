using System;
using System.IO;
using System.Text;
using DeconTools.Backend.Core;
using DeconTools.Utilities;

namespace DeconTools.Backend.Data
{
    public class UIMFScansExporter : ScansExporter
    {
        private string fileName;

        public UIMFScansExporter(string fileName)
        {
            this.headerLine = "frame_num,frame_time,type,bpi,bpi_mz,tic,num_peaks,num_deisotoped,frame_pressure_unsmoothed,frame_pressure_unsmoothed";
            this.delimiter = ',';
            this.fileName = fileName;
        }

        protected override string headerLine { get; set; }
        protected override char delimiter { get; set; }

        public override void Export(DeconTools.Backend.Core.ResultCollection results)
        {
            StreamWriter sw;
            try
            {
                sw = new StreamWriter(this.fileName);
            }
            catch (Exception ex)
            {
                throw new Exception("Error creating file " + this.fileName + ": " + ex.Message);
            }

            if (!string.Equals(PNNLOmics.Utilities.StringUtilities.DblToString(3.14159, 4, false, 0.001, false), DblToString(3.14159, 4)))
            {
                Console.WriteLine("Note: using a period for the decimal point for because the result files are CSV files");
            }

            sw.WriteLine(headerLine);

            foreach (var result in results.ScanResultList)
            {
                Check.Require(result is UimfScanResult, "UIMF_Scans_Exporter only works on UIMF Scan Results");
                var uimfResult = (UimfScanResult)result;
                var sb = new StringBuilder();
                sb.Append(uimfResult.ScanSet.PrimaryScanNumber);
                sb.Append(delimiter);
                sb.Append(DblToString(uimfResult.ScanTime, 3));
                sb.Append(delimiter);
                sb.Append(result.SpectrumType);
                sb.Append(delimiter);
                sb.Append(DblToString(uimfResult.BasePeak.Height, 4, true));
                sb.Append(delimiter);
                sb.Append(DblToString(uimfResult.BasePeak.XValue, 5));
                sb.Append(delimiter);
                sb.Append(DblToString(uimfResult.TICValue, 4, true));
                sb.Append(delimiter);
                sb.Append(uimfResult.NumPeaks);
                sb.Append(delimiter);
                sb.Append(uimfResult.NumIsotopicProfiles);
                sb.Append(delimiter);
                sb.Append(DblToString(uimfResult.FramePressureUnsmoothed, 4));
                sb.Append(delimiter);
                sb.Append(DblToString(uimfResult.FramePressureSmoothed, 4));

                sw.WriteLine(sb.ToString());
            }
            sw.Close();
        }
    }
}
