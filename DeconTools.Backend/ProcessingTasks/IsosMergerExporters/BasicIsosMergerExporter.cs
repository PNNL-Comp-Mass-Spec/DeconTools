using System;
using System.Collections.Generic;
using System.IO;
using DeconTools.Backend.Core;

namespace DeconTools.Backend.ProcessingTasks
{
    public class BasicIsosMergerExporter : IIsosMergerExporter
    {

        const char DELIMITER = ',';

        private int runCounter;

        public string OutputFilename { get; set; }

        private readonly StreamWriter sw;

        public BasicIsosMergerExporter(string outputFileName)
        {
            OutputFilename = outputFileName;
            try
            {
                sw = new StreamWriter(OutputFilename);
            }
            catch (Exception ex)
            {
                throw new Exception("IsosMergerAndExporter can't export data. Check if file is open. Details: " + ex.Message);
            }

            runCounter = 1;

            sw.WriteLine(buildHeader());
        }

        public override void MergeAndExport(ResultCollection results)
        {

            var data = new List<string>();

            foreach (var isosResult in results.ResultList)
            {
                var result = (StandardIsosResult)isosResult;
                data.Clear();

                data.Add(getScanNumber(result.ScanSet.PrimaryScanNumber).ToString());    //this prevents duplicate scan_nums
                data.Add(result.IsotopicProfile.ChargeState.ToString());
                data.Add(DblToString(result.IsotopicProfile.GetAbundance(), 4, true));
                data.Add(DblToString(result.IsotopicProfile.GetMZ(), 5));
                data.Add(DblToString(result.IsotopicProfile.Score, 4));
                data.Add(DblToString(result.IsotopicProfile.AverageMass, 5));
                data.Add(DblToString(result.IsotopicProfile.MonoIsotopicMass, 5));
                data.Add(DblToString(result.IsotopicProfile.MostAbundantIsotopeMass, 5));
                data.Add(DblToString(result.IsotopicProfile.GetFWHM(), 4));
                data.Add(DblToString(result.IsotopicProfile.GetSignalToNoise(), 2));
                data.Add(DblToString(result.IsotopicProfile.GetMonoAbundance(), 4, true));
                data.Add(DblToString(result.IsotopicProfile.GetMonoPlusTwoAbundance(), 4, true));

                sw.WriteLine(string.Join(DELIMITER.ToString(), data));
            }

            results.ResultList.Clear();        //since we are writing the results to a stream as we go, it's important to clear the list each time
            runCounter++;
        }

        protected int getScanNumber(int scan_num)
        {
            return scan_num * runCounter;         //this is used as a means of preventing duplicate scan_nums
        }

        public override void Cleanup()
        {
            if (sw == null) return;
            try
            {
                sw.Close();
            }
            catch (Exception)
            {
                // Ignore errors
            }
        }

        private string buildHeader()
        {
            var data = new List<string>
            {
                "scan_num",
                "charge",
                "abundance",
                "mz",
                "fit",
                "average_mw",
                "monoisotopic_mw",
                "mostabundant_mw",
                "fwhm",
                "signal_noise",
                "mono_abundance",
                "mono_plus2_abundance"
            };

            return string.Join(DELIMITER.ToString(), data);
        }
    }
}
