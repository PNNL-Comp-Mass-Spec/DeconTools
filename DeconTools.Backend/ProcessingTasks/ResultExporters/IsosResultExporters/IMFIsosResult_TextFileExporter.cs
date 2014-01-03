using System;
using System.Text;

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
            this.TriggerToExport = triggerValueToExport;
            this.delimiter = ',';
            this.Name = "IMF IsosResult TextFile Exporter";
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
            sb.Append(result.IsotopicProfile.Score.ToString("0.####"));						// Fit score
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
            sb.Append(result.IsotopicProfile.IsSaturated ? 1 : 0);
            sb.Append(delimiter);
            sb.Append(ResultValidators.ResultValidationUtils.GetStringFlagCode(result.Flags));
			// Uncomment to write out the fit_count_basis
			//sb.Append(Delimiter);
			//sb.Append(result.IsotopicProfile.ScoreCountBasis);				// Number of points used for the fit score
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
            sb.Append(delimiter);
            sb.Append("flag");
			// Uncomment to write out the fit_count_basis
			//sb.Append(Delimiter);
			//sb.Append("fit_basis_count");
            sb.Append(Environment.NewLine);
            return sb.ToString();


        }


    }
}
