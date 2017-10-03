using System.Collections.Generic;
using DeconTools.Backend.Core;
using DeconTools.Utilities;

namespace DeconTools.Backend.FileIO
{
    public class MSFeatureToTextFileExporterUIMF : TextFileExporter<IsosResult>
    {
        #region Constructors
        public MSFeatureToTextFileExporterUIMF(string fileName) : base(fileName, ',') { }

        public MSFeatureToTextFileExporterUIMF(string fileName, char delimiter) : base(fileName, delimiter) { }

        #endregion

        #region Private Methods
        protected override string buildResultOutput(IsosResult result)
        {
            Check.Require(result is UIMFIsosResult, "UIMF Isos Exporter is only used with UIMF results");
            var uimfResult = (UIMFIsosResult)result;

            var data = new List<string>
            {
                uimfResult.ScanSet.PrimaryScanNumber.ToString(),
                (uimfResult.IMSScanSet.PrimaryScanNumber + 1).ToString(),                // Add 1 to PrimaryScanNumber (which is 0-based)
                uimfResult.IsotopicProfile.ChargeState.ToString(),
                DblToString(uimfResult.IsotopicProfile.GetAbundance(), 4, true),
                DblToString(uimfResult.IsotopicProfile.GetMZ(), 5),
                DblToString(uimfResult.IsotopicProfile.GetScore(), 4),
                DblToString(uimfResult.IsotopicProfile.AverageMass, 5),
                DblToString(uimfResult.IsotopicProfile.MonoIsotopicMass, 5),
                DblToString(uimfResult.IsotopicProfile.MostAbundantIsotopeMass, 5),
                DblToString(uimfResult.IsotopicProfile.GetFWHM(), 4),
                DblToString(uimfResult.IsotopicProfile.GetSignalToNoise(), 2),
                DblToString(uimfResult.IsotopicProfile.GetMonoAbundance(), 4, true),
                DblToString(uimfResult.IsotopicProfile.GetMonoPlusTwoAbundance(), 4, true),
                DblToString(uimfResult.IsotopicProfile.OriginalIntensity, 4, true),
                uimfResult.IsotopicProfile.IsSaturatedAsNumericText,
                DblToString(uimfResult.DriftTime, 3),
                ProcessingTasks.ResultValidators.ResultValidationUtils.GetStringFlagCode(result.Flags),
                DblToString(uimfResult.InterferenceScore, 4)
            };

            return string.Join(Delimiter.ToString(), data);
        }

        protected override string buildHeaderLine()
        {
            var data = new List<string>
            {
                "frame_num",
                "ims_scan_num",
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
                "drift_time",
                "flag",
                "interference_score"
            };

            return string.Join(Delimiter.ToString(), data);
        }
        #endregion

    }
}
