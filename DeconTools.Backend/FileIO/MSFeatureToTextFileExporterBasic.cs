using System.Collections.Generic;
using DeconTools.Backend.Core;

namespace DeconTools.Backend.FileIO
{
    public class MSFeatureToTextFileExporterBasic : TextFileExporter<IsosResult>
    {
        #region Constructors
        public MSFeatureToTextFileExporterBasic(string fileName) : base(fileName, ',') { }

        public MSFeatureToTextFileExporterBasic(string fileName, char delimiter) : base(fileName, delimiter) { }

        #endregion

        #region Private Methods
        protected override string buildResultOutput(IsosResult result)
        {
            var data = new List<string>
            {
                result.ScanSet.PrimaryScanNumber.ToString(),
                result.IsotopicProfile.ChargeState.ToString(),
                DblToString(result.IsotopicProfile.GetAbundance(), 4, true),
                DblToString(result.IsotopicProfile.GetMZ(), 5),
                DblToString(result.IsotopicProfile.Score, 4),
                DblToString(result.IsotopicProfile.AverageMass, 5),
                DblToString(result.IsotopicProfile.MonoIsotopicMass, 5),
                DblToString(result.IsotopicProfile.MostAbundantIsotopeMass, 5),
                DblToString(result.IsotopicProfile.GetFWHM(), 4),
                DblToString(result.IsotopicProfile.GetSignalToNoise(), 2),
                DblToString(result.IsotopicProfile.GetMonoAbundance(), 4, true),
                DblToString(result.IsotopicProfile.GetMonoPlusTwoAbundance(), 4, true),
                ProcessingTasks.ResultValidators.ResultValidationUtils.GetStringFlagCode(result.Flags),
                DblToString(result.InterferenceScore, 5)
            };

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
                "flag",
                "interference_score"
            };

            return string.Join(Delimiter.ToString(), data);
        }
        #endregion

    }
}
