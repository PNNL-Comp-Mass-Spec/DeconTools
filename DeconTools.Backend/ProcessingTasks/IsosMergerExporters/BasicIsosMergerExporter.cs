using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using DeconTools.Backend.Core;

namespace DeconTools.Backend.ProcessingTasks
{
    public class BasicIsosMergerExporter : IIsosMergerExporter
    {

        private char delimiter = ',';
        private int runCounter;

        private string outputFilename;

        public string OutputFilename
        {
            get { return outputFilename; }
            set { outputFilename = value; }
        }

        private StreamWriter sw;

        public BasicIsosMergerExporter(string outputFileName)
        {
            this.outputFilename = outputFileName;
            try
            {
                sw = new StreamWriter(this.outputFilename);
            }
            catch (Exception ex)
            {
                throw new Exception("IsosMergerAndExporter can't export data. Check if file is open. Details: " + ex.Message);
            }

            runCounter = 1;

            sw.Write(buildHeader());
        }

        public override void MergeAndExport(ResultCollection results)
        {

            StringBuilder sb;

            foreach (StandardIsosResult result in results.ResultList)
            {
                sb = new StringBuilder();
                sb.Append(getScanNumber(result.ScanSet.PrimaryScanNumber));    //this prevents duplicate scan_nums
                sb.Append(delimiter);
                sb.Append(result.IsotopicProfile.ChargeState);
                sb.Append(delimiter);
                sb.Append(DblToString(result.IsotopicProfile.GetAbundance(), 4, true));
                sb.Append(delimiter);
                sb.Append(DblToString(result.IsotopicProfile.GetMZ(), 5));
                sb.Append(delimiter);
                sb.Append(DblToString(result.IsotopicProfile.Score, 4));
                sb.Append(delimiter);
                sb.Append(DblToString(result.IsotopicProfile.AverageMass, 5));
                sb.Append(delimiter);
                sb.Append(DblToString(result.IsotopicProfile.MonoIsotopicMass, 5));
                sb.Append(delimiter);
                sb.Append(DblToString(result.IsotopicProfile.MostAbundantIsotopeMass, 5));
                sb.Append(delimiter);
                sb.Append(DblToString(result.IsotopicProfile.GetFWHM(), 4));
                sb.Append(delimiter);
                sb.Append(DblToString(result.IsotopicProfile.GetSignalToNoise(), 2));
                sb.Append(delimiter);
                sb.Append(DblToString(result.IsotopicProfile.GetMonoAbundance(), 4, true));
                sb.Append(delimiter);
                sb.Append(DblToString(result.IsotopicProfile.GetMonoPlusTwoAbundance(), 4, true));

                sw.WriteLine(sb.ToString());
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

        private string buildHeader()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("scan_num");
            sb.Append(delimiter);
            sb.Append("charge");
            sb.Append(delimiter);
            sb.Append("abundance");
            sb.Append(delimiter);
            sb.Append("mz");
            sb.Append(delimiter);
            sb.Append("fit");
            sb.Append(delimiter);
            sb.Append("average_mw");
            sb.Append(delimiter);
            sb.Append("monoisotopic_mw");
            sb.Append(delimiter);
            sb.Append("mostabundant_mw");
            sb.Append(delimiter);
            sb.Append("fwhm");
            sb.Append(delimiter);
            sb.Append("signal_noise");
            sb.Append(delimiter);
            sb.Append("mono_abundance");
            sb.Append(delimiter);
            sb.Append("mono_plus2_abundance");
            sb.Append(Environment.NewLine);
            return sb.ToString();
        }
    }
}
