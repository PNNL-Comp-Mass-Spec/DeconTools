using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using DeconTools.Utilities;
using DeconTools.Backend.Core;

namespace DeconTools.Backend.ProcessingTasks.IsosMergerExporters
{
    public class BasicScansMergerExporter:IIsosMergerExporter
    {

        private int runCounter;
        private string outputFilename;
        private StreamWriter sw;
        private char delimiter = ',';



        public string OutputFilename
        {
            get { return outputFilename; }
            set { outputFilename = value; }
        }

        public BasicScansMergerExporter(string outputFileName)
        {
            this.outputFilename = outputFileName;
            try
            {
                sw = new StreamWriter(this.outputFilename);
            }
            catch (Exception ex)
            {
                throw new Exception("ScansMergerAndExporter can't export data. Check if file is open. Details: " + ex.Message);
            }

            runCounter = 1;

            sw.Write(buildHeader());
        }

        private string buildHeader()
        {
            var sb = new StringBuilder();

            sb.Append("scan_num");
            sb.Append(delimiter);
            sb.Append("scan_time");
            sb.Append(delimiter);
            sb.Append("type");
            sb.Append(delimiter);
            sb.Append("bpi");
            sb.Append(delimiter);
            sb.Append("bpi_mz");
            sb.Append(delimiter);
            sb.Append("tic");
            sb.Append(delimiter);
            sb.Append("num_peaks");
            sb.Append(delimiter);
            sb.Append("num_deisotoped");
            sb.Append(Environment.NewLine);

            return sb.ToString();
        }

        public override void MergeAndExport(DeconTools.Backend.Core.ResultCollection resultList)
        {
            Check.Require(resultList != null, "Scans merger failed. ResultCollection is null");
            Check.Require(resultList.ScanResultList != null && resultList.ScanResultList.Count > 0, "Scans merger failed... there's a problem in the ScanResult List");

            var sb = new StringBuilder();
            var scanresult = resultList.ScanResultList[0];   // we only take the first one since we delete each scanResult after writing it out to file

            sb.Append(getScanNumber(scanresult.ScanSet.PrimaryScanNumber));
            sb.Append(delimiter);
            sb.Append(DblToString(scanresult.ScanTime, 4));
            sb.Append(delimiter);
            sb.Append(scanresult.SpectrumType);
            sb.Append(delimiter);
            sb.Append(DblToString(scanresult.BasePeak.Height, 4, true));
            sb.Append(delimiter);
            sb.Append(DblToString(scanresult.BasePeak.XValue, 5));
            sb.Append(delimiter);
            sb.Append(DblToString(scanresult.ScanSet.TICValue, 4, true));
            sb.Append(delimiter);
            sb.Append(scanresult.NumPeaks);
            sb.Append(delimiter);
            sb.Append(scanresult.NumIsotopicProfiles);

            sw.WriteLine(sb.ToString());

            resultList.ScanResultList.Clear();          // scanResult List is cleared every time. 
            runCounter++;
        }

        private int getScanNumber(int scan_num)
        {
            return scan_num * runCounter;
        }


        public override void Cleanup()
        {
            if (sw != null)
            {
                try
                {
                    sw.Close();
                }
                catch (Exception)
                {

                }
            }
        }
    }
}
