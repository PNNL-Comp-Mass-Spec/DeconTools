using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using DeconTools.Backend.Core;
using DeconTools.Utilities;

namespace DeconTools.Backend.Data
{
    public class UIMFScansExporter : ScansExporter
    {
        private string fileName;

        public UIMFScansExporter(string fileName)
        {
            this.headerLine = "frame_num,frame_time,type,bpi,bpi_mz,tic,num_peaks,num_deisotoped,frame_pressure_front,frame_pressure_back";
            this.delimiter = ',';
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
                Check.Require(result is UIMFScanResult, "UIMF_Scans_Exporter only works on UIMF Scan Results");
                UIMFScanResult uimfResult = (UIMFScanResult)result;
                sb = new StringBuilder();
                sb.Append(uimfResult.Frameset.PrimaryFrame);
                sb.Append(delimiter);
                sb.Append(uimfResult.ScanTime.ToString("0.###"));
                sb.Append(delimiter);
                sb.Append(result.SpectrumType);
                sb.Append(delimiter);
                sb.Append(uimfResult.BasePeak.Intensity);
                sb.Append(delimiter);
                sb.Append(uimfResult.BasePeak.MZ.ToString("0.#####"));
                sb.Append(delimiter);
                sb.Append(uimfResult.TICValue);
                sb.Append(delimiter);
                sb.Append(uimfResult.NumPeaks);
                sb.Append(delimiter);
                sb.Append(uimfResult.NumIsotopicProfiles);
                sb.Append(delimiter);
                sb.Append(uimfResult.FramePressureFront.ToString("0.####"));
                sb.Append(delimiter);
                sb.Append(uimfResult.FramePressureBack.ToString("0.####"));
                
                sw.WriteLine(sb.ToString());
            }
            sw.Close();
        }
    }
}
