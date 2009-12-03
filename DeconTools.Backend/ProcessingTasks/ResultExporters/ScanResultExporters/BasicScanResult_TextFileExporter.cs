using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using DeconTools.Backend.Core;
using DeconTools.Backend.Utilities;

namespace DeconTools.Backend.ProcessingTasks.ResultExporters.ScanResultExporters
{
    public class BasicScanResult_TextFileExporter : ScanResult_TextFileExporter
    {
        private char delimiter;
       

        #region Constructors
        public BasicScanResult_TextFileExporter(string fileName)
        {
            try
            {
                sw = new StreamWriter(fileName);
            }
            catch (Exception ex)
            {
                Logger.Instance.AddEntry("IsosResultExporter failed. Details: " + ex.Message, Logger.Instance.OutputFilename);
                throw new Exception("Result exporter failed.  Check to see if it is already open or not.");
            }

            this.delimiter = ',';

            sw.Write(buildHeaderLine());

        }
        #endregion

   
        protected override string buildScansResultOutput(ScanResult result)
        {
            StringBuilder sb = new StringBuilder();
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
            sb.Append(Environment.NewLine);

            return sb.ToString();
        }

        public override char Delimiter
        {
            get
            {
                return delimiter;
            }
            set
            {
                delimiter = value;
            }
        }
    }
}
