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


        protected override string buildIsosResultOutput(Core.IsosResult result)
        {
            var data = new List<string>
            {
                result.ScanSet.PrimaryScanNumber.ToString(),
                result.IsotopicProfile.ChargeState.ToString(),
                DblToString(result.IsotopicProfile.GetAbundance(), 4, true),        // Fit score
                DblToString(result.IsotopicProfile.GetMZ(), 5),
                DblToString(result.IsotopicProfile.Score, 4),
                DblToString(result.IsotopicProfile.AverageMass, 5),
                DblToString(result.IsotopicProfile.MonoIsotopicMass, 5),
                DblToString(result.IsotopicProfile.MostAbundantIsotopeMass, 5),
                DblToString(result.IsotopicProfile.GetFWHM(), 4),
                DblToString(result.IsotopicProfile.GetSignalToNoise(), 2),
                DblToString(result.IsotopicProfile.GetMonoAbundance(), 4, true),
                DblToString(result.IsotopicProfile.GetMonoPlusTwoAbundance(), 4, true),
                DblToString(result.IsotopicProfile.OriginalIntensity, 4, true),
                result.IsotopicProfile.IsSaturatedAsNumericText,
                ResultValidators.ResultValidationUtils.GetStringFlagCode(result.Flags)
            };

            // Uncomment to write out the fit_count_basis
            // data.Add(result.IsotopicProfile.ScoreCountBasis);				// Number of points used for the fit score

            return string.Join(Delimiter.ToString(), data);

        }

        protected override string buildHeaderLine()
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
                "mono_plus2_abundance",
                "orig_intensity",
                "TIA_orig_intensity",
                "flag"
            };

            // Uncomment to write out the fit_count_basis
            //            //data.Add("fit_basis_count");

            return string.Join(Delimiter.ToString(), data);



        }


    }
}
