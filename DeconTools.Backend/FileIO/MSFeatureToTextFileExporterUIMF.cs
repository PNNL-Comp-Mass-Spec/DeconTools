using System.Text;
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

            var sb = new StringBuilder();
            sb.Append(uimfResult.ScanSet.PrimaryScanNumber);
            sb.Append(Delimiter);
            sb.Append(uimfResult.IMSScanSet.PrimaryScanNumber + 1);    //adds 1 to PrimaryScanNumber (which is 0-based)
            sb.Append(Delimiter);
            sb.Append(uimfResult.IsotopicProfile.ChargeState);
            sb.Append(Delimiter);
            sb.Append(DblToString(uimfResult.IsotopicProfile.GetAbundance(), 4, true));
            sb.Append(Delimiter);
            sb.Append(DblToString(uimfResult.IsotopicProfile.GetMZ(), 5));
            sb.Append(Delimiter);
            sb.Append(DblToString(uimfResult.IsotopicProfile.GetScore(), 4));
            sb.Append(Delimiter);
            sb.Append(DblToString(uimfResult.IsotopicProfile.AverageMass, 5));
            sb.Append(Delimiter);
            sb.Append(DblToString(uimfResult.IsotopicProfile.MonoIsotopicMass, 5));
            sb.Append(Delimiter);
            sb.Append(DblToString(uimfResult.IsotopicProfile.MostAbundantIsotopeMass, 5));
            sb.Append(Delimiter);
            sb.Append(DblToString(uimfResult.IsotopicProfile.GetFWHM(), 4));
            sb.Append(Delimiter);
            sb.Append(DblToString(uimfResult.IsotopicProfile.GetSignalToNoise(), 2));
            sb.Append(Delimiter);
            sb.Append(DblToString(uimfResult.IsotopicProfile.GetMonoAbundance(), 4, true));
            sb.Append(Delimiter);
            sb.Append(DblToString(uimfResult.IsotopicProfile.GetMonoPlusTwoAbundance(), 4, true));
            sb.Append(Delimiter);
            sb.Append(DblToString(uimfResult.IsotopicProfile.OriginalIntensity, 4, true));
            sb.Append(Delimiter);
            sb.Append(uimfResult.IsotopicProfile.IsSaturated ? 1 : 0);
            sb.Append(Delimiter);
            sb.Append(DblToString(uimfResult.DriftTime, 3));
            sb.Append(Delimiter);
            sb.Append(DeconTools.Backend.ProcessingTasks.ResultValidators.ResultValidationUtils.GetStringFlagCode(result.Flags));
            sb.Append(Delimiter);
            sb.Append(DblToString(uimfResult.InterferenceScore, 4));


            return sb.ToString();
        }

        protected override string buildHeaderLine()
        {
            var sb = new StringBuilder();

            sb.Append("frame_num");
            sb.Append(Delimiter);
            sb.Append("ims_scan_num");
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
            sb.Append(Delimiter);
            sb.Append("drift_time");
            sb.Append(Delimiter);
            sb.Append("flag");
            sb.Append(Delimiter);
            sb.Append("interference_score");

            return sb.ToString();
        }
        #endregion

    }
}
