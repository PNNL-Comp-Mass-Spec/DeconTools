using System.Text;
using DeconTools.Backend.Core;

namespace DeconTools.Backend.FileIO
{
    public class MSFeatureToTextFileExporterIMF : TextFileExporter<IsosResult>
    {
        #region Constructors
        public MSFeatureToTextFileExporterIMF(string fileName) : base(fileName, ',') { }

        public MSFeatureToTextFileExporterIMF(string fileName, char delimiter) : base(fileName, delimiter) { }
        #endregion



        #region Private Methods
        protected override string buildResultOutput(IsosResult result)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(result.ScanSet.PrimaryScanNumber);
            sb.Append(Delimiter);
            sb.Append(result.IsotopicProfile.ChargeState);
            sb.Append(Delimiter);
            sb.Append(DblToString(result.IsotopicProfile.GetAbundance(), 4, true));
            sb.Append(Delimiter);
            sb.Append(DblToString(result.IsotopicProfile.GetMZ(), 5));
            sb.Append(Delimiter);
            sb.Append(DblToString(result.IsotopicProfile.Score, 4));
            sb.Append(Delimiter);
            sb.Append(DblToString(result.IsotopicProfile.AverageMass, 5));
            sb.Append(Delimiter);
            sb.Append(DblToString(result.IsotopicProfile.MonoIsotopicMass, 5));
            sb.Append(Delimiter);
            sb.Append(DblToString(result.IsotopicProfile.MostAbundantIsotopeMass, 5));
            sb.Append(Delimiter);
            sb.Append(DblToString(result.IsotopicProfile.GetFWHM(), 4));
            sb.Append(Delimiter);
            sb.Append(DblToString(result.IsotopicProfile.GetSignalToNoise(), 2));
            sb.Append(Delimiter);
            sb.Append(DblToString(result.IsotopicProfile.GetMonoAbundance(), 4, true));
            sb.Append(Delimiter);
            sb.Append(DblToString(result.IsotopicProfile.GetMonoPlusTwoAbundance(), 4, true));
            sb.Append(Delimiter);
            sb.Append(DblToString(result.IsotopicProfile.OriginalIntensity, 4, true));
            sb.Append(Delimiter);
            sb.Append(result.IsotopicProfile.IsSaturated ? 1 : 0);   // 1 if true
            sb.Append(Delimiter);
            sb.Append(DeconTools.Backend.ProcessingTasks.ResultValidators.ResultValidationUtils.GetStringFlagCode(result.Flags));
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
            sb.Append(Delimiter);
            sb.Append("flag");
            return sb.ToString();
        }
        #endregion


    }
}
