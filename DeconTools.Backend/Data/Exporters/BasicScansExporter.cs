using System;
using System.Collections.Generic;
using System.IO;
using DeconTools.Backend.Core;

namespace DeconTools.Backend.Data
{
    public class BasicScansExporter : ScansExporter
    {
        private readonly string fileName;

        public BasicScansExporter(string fileName)
        {
            delimiter = ',';
            headerLine = "scan_num,scan_time,type,bpi,bpi_mz,tic,num_peaks,num_deisotoped";
            this.fileName = fileName;
        }

        protected sealed override string headerLine { get; set; }
        protected sealed override char delimiter { get; set; }

        public override void Export(ResultCollection results)
        {
            StreamWriter sw;
            try
            {
                sw = new StreamWriter(fileName);
            }
            catch (Exception ex)
            {
                throw new Exception("Error creating file " + fileName + ": " + ex.Message);
            }

            if (!string.Equals(PNNLOmics.Utilities.StringUtilities.DblToString(3.14159, 4, false, 0.001, false), DblToString(3.14159, 4)))
            {
                Console.WriteLine("Note: using a period for the decimal point because the result files are CSV files");
            }

            sw.WriteLine(headerLine);
            var data = new List<string>();

            foreach (var result in results.ScanResultList)
            {
                data.Clear();
                data.Add(result.ScanSet.PrimaryScanNumber.ToString());
                data.Add(DblToString(result.ScanTime, 4));
                data.Add(result.SpectrumType.ToString());
                data.Add(DblToString(result.BasePeak.Height, 4, true));
                data.Add(DblToString(result.BasePeak.XValue, 5));
                data.Add(DblToString(result.ScanSet.TICValue, 4, true));
                data.Add(result.NumPeaks.ToString());
                data.Add(result.NumIsotopicProfiles.ToString());

                sw.WriteLine(string.Join(delimiter.ToString(), data));
            }
            sw.Close();
        }
    }
}
