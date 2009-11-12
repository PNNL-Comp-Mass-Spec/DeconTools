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
            StringBuilder sb;
            StreamWriter sw;
            try
            {
                sw = new StreamWriter(this.fileName);

            }
            catch (Exception)
            {
                throw;
            }
            sw.WriteLine(headerLine);
            foreach (ScanResult result in results.ScanResultList)
            {
                sb = new StringBuilder();
                sb.Append(result.ScanSet.PrimaryScanNumber);   
                sb.Append(delimiter);
                sb.Append(result.ScanTime.ToString("0.####"));
                sb.Append(delimiter);
                sb.Append(result.SpectrumType);
                sb.Append(delimiter);
                sb.Append(result.BasePeak.Intensity);
                sb.Append(delimiter);
                sb.Append(result.BasePeak.MZ.ToString("0.#####"));
                sb.Append(delimiter);
                sb.Append(result.ScanSet.TICValue);
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
