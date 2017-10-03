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
        private readonly StreamWriter sw;
        private const char DELIMITER = ',';

        public string OutputFilename
        {
            get => outputFilename;
            set => outputFilename = value;
        }

        public BasicScansMergerExporter(string outputFileName)
        {
            outputFilename = outputFileName;
            try
            {
                sw = new StreamWriter(outputFilename);
            }
            catch (Exception ex)
            {
                throw new Exception("ScansMergerAndExporter can't export data. Check if file is open. Details: " + ex.Message);
            }

            runCounter = 1;

            sw.WriteLine(buildHeader());
        }

        private string buildHeader()
        {
            var data = new List<string> {
                "scan_num",
                "scan_time",
                "type",
                "bpi",
                "bpi_mz",
                "tic",
                "num_peaks",
                "num_deisotoped" };

            return string.Join(DELIMITER.ToString(), data);
        }

        public override void MergeAndExport(ResultCollection resultList)
        {
            Check.Require(resultList != null, "Scans merger failed. ResultCollection is null");
            Check.Require(resultList.ScanResultList != null && resultList.ScanResultList.Count > 0, "Scans merger failed... there's a problem in the ScanResult List");

            var scanresult = resultList.ScanResultList[0];   // we only take the first one since we delete each scanResult after writing it out to file

            var data = new List<string>
            {
                getScanNumber(scanresult.ScanSet.PrimaryScanNumber).ToString(),
                DblToString(scanresult.ScanTime, 4),
                scanresult.SpectrumType.ToString(),
                DblToString(scanresult.BasePeak.Height, 4, true),
                DblToString(scanresult.BasePeak.XValue, 5),
                DblToString(scanresult.ScanSet.TICValue, 4, true),
                scanresult.NumPeaks.ToString(),
                scanresult.NumIsotopicProfiles.ToString()
            };

            sw.WriteLine(string.Join(DELIMITER.ToString(), data));

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
