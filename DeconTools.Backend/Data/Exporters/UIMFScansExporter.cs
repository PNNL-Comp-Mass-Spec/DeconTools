using System;
using System.Collections.Generic;
using System.IO;
using DeconTools.Backend.Core;
using DeconTools.Utilities;

namespace DeconTools.Backend.Data
{
    public class UIMFScansExporter : ScansExporter
    {
        private readonly string fileName;

        public UIMFScansExporter(string fileName)
        {
            headerLine = "frame_num,frame_time,type,bpi,bpi_mz,tic,num_peaks,num_deisotoped,frame_pressure_unsmoothed,frame_pressure_unsmoothed";
            delimiter = ',';
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
                Check.Require(result is UimfScanResult, "UIMF_Scans_Exporter only works on UIMF Scan Results");
                var uimfResult = (UimfScanResult)result;
                data.Clear();

                data.Add(uimfResult.ScanSet.PrimaryScanNumber.ToString());
                data.Add(DblToString(uimfResult.ScanTime, 3));
                data.Add(result.SpectrumType.ToString());
                data.Add(DblToString(uimfResult.BasePeak.Height, 4, true));
                data.Add(DblToString(uimfResult.BasePeak.XValue, 5));
                data.Add(DblToString(uimfResult.TICValue, 4, true));
                data.Add(uimfResult.NumPeaks.ToString());
                data.Add(uimfResult.NumIsotopicProfiles.ToString());
                data.Add(DblToString(uimfResult.FramePressureUnsmoothed, 4));
                data.Add(DblToString(uimfResult.FramePressureSmoothed, 4));

                sw.WriteLine(string.Join(delimiter.ToString(), data));
            }
            sw.Close();
        }
    }
}
