using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using DeconTools.Backend.Utilities;
using DeconTools.Backend.Core;

namespace DeconTools.Backend.ProcessingTasks.ResultExporters.IsosResultExporters
{
    public class BasicIsosResultTextFileExporter :IsosResultTextFileExporter
    {
        private int triggerVal;
        private char delimiter;

        #region Constructors
        public BasicIsosResultTextFileExporter(string fileName)
            : this(fileName, 1000000)
        {

        }

        
        public BasicIsosResultTextFileExporter(string fileName, int triggerValueToExport)
        {
            this.TriggerToExport = triggerValueToExport;
            this.delimiter = ',';
            this.Name = "Basic IsosResult TextFile Exporter";
            this.FileName = fileName;

            initializeAndWriteHeader();

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

        protected override string buildIsosResultOutput(IsosResult result)
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
            sb.Append(ResultValidators.ResultValidationUtils.GetStringFlagCode(result.Flags));
            return sb.ToString();
        }
        protected override string buildHeaderLine()
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
            sb.Append(delimiter);
            sb.Append("flag");
            sb.Append(Environment.NewLine);
            return sb.ToString();
        }
    }
        

  


}
