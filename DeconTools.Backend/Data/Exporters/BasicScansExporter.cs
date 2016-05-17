using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using DeconTools.Backend.Core;

namespace DeconTools.Backend.Data
{
    public class BasicScansExporter : ScansExporter
    {
        private string fileName;

        public BasicScansExporter(string fileName)
        {
            this.delimiter = ',';
            this.headerLine = "scan_num,scan_time,type,bpi,bpi_mz,tic,num_peaks,num_deisotoped";
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

            foreach (ScanResult result in results.ScanResultList)
            {
                var sb = new StringBuilder();
                sb.Append(result.ScanSet.PrimaryScanNumber);   
                sb.Append(delimiter);
                sb.Append(DblToString(result.ScanTime, 4));
                sb.Append(delimiter);
                sb.Append(result.SpectrumType);
                sb.Append(delimiter);
                sb.Append(DblToString(result.BasePeak.Height, 4, true));
                sb.Append(delimiter);
                sb.Append(DblToString(result.BasePeak.XValue, 5));
                sb.Append(delimiter);
                sb.Append(DblToString(result.ScanSet.TICValue, 4, true));
                sb.Append(delimiter);
                sb.Append(result.NumPeaks);
                sb.Append(delimiter);
                sb.Append(result.NumIsotopicProfiles);

                sw.WriteLine(sb.ToString());
            }
            sw.Close();
        }
    }
}
