using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using DeconTools.Backend.Utilities;

namespace DeconTools.Backend.ProcessingTasks.ResultExporters.IsosResultExporters
{
    public class IMFIsosResult_TextFileExporter : IsosResultExporters.IsosResultTextFileExporter
    {
        private int triggerVal;
        private char delimiter;
        #region Constructors
        public IMFIsosResult_TextFileExporter(string fileName)
            : this(fileName, 1000000)
        {

        }

        public IMFIsosResult_TextFileExporter(string fileName, int triggerValueToExport)
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

            this.TriggerToExport = triggerValueToExport;
            this.delimiter = ',';

            sw.Write(buildHeaderLine());

        }

        #endregion

        #region Properties
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
        public override int TriggerToExport
        {
            get
            {
                return triggerVal;
            }
            set
            {
                triggerVal = value;
            }
        }
        #endregion

        #region Public Methods
        #endregion

        #region Private Methods
        #endregion



        protected override string buildIsosResultOutput(DeconTools.Backend.Core.IsosResult result)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(result.ScanSet.PrimaryScanNumber);
            sb.Append(delimiter);
            sb.Append(result.IsotopicProfile.ChargeState);
            sb.Append(delimiter);
            sb.Append(result.IsotopicProfile.GetAbundance());
            sb.Append(delimiter);
            sb.Append(result.IsotopicProfile.GetMZ().ToString("0.#####"));
            sb.Append(delimiter);
            sb.Append(result.IsotopicProfile.Score.ToString("0.####"));
            sb.Append(delimiter);
            sb.Append(result.IsotopicProfile.AverageMass.ToString("0.#####"));
            sb.Append(delimiter);
            sb.Append(result.IsotopicProfile.MonoIsotopicMass.ToString("0.#####"));
            sb.Append(delimiter);
            sb.Append(result.IsotopicProfile.MostAbundantIsotopeMass.ToString("0.#####"));
            sb.Append(delimiter);
            sb.Append(result.IsotopicProfile.GetFWHM().ToString("0.####"));
            sb.Append(delimiter);
            sb.Append(result.IsotopicProfile.GetSignalToNoise().ToString("0.##"));
            sb.Append(delimiter);
            sb.Append(result.IsotopicProfile.GetMonoAbundance());
            sb.Append(delimiter);
            sb.Append(result.IsotopicProfile.GetMonoPlusTwoAbundance());
            sb.Append(delimiter);
            sb.Append(result.IsotopicProfile.OriginalIntensity);
            sb.Append(delimiter);
            sb.Append(result.IsotopicProfile.Original_Total_isotopic_abundance);
            return sb.ToString();
        }

        protected override string buildHeaderLine()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("scan_num");
            sb.Append(Delimiter);
            sb.Append("charge");
            sb.Append(Delimiter);
            sb.Append("abundance");
            sb.Append(Delimiter);
            sb.Append("mz");
            sb.Append(Delimiter);
            sb.Append("fit");
            sb.Append(Delimiter);
            sb.Append("average_mw");
            sb.Append(Delimiter);
            sb.Append("monoisotopic_mw");
            sb.Append(Delimiter);
            sb.Append("mostabundant_mw");
            sb.Append(Delimiter);
            sb.Append("fwhm");
            sb.Append(Delimiter);
            sb.Append("signal_noise");
            sb.Append(Delimiter);
            sb.Append("mono_abundance");
            sb.Append(Delimiter);
            sb.Append("mono_plus2_abundance");
            sb.Append(Delimiter);
            sb.Append("orig_intensity");
            sb.Append(Delimiter);
            sb.Append("TIA_orig_intensity");
            sb.Append(Environment.NewLine);
            return sb.ToString();


        }


    }
}
