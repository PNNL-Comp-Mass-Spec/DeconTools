using System.Collections.Generic;
using DeconTools.Backend.Core;
using DeconTools.Utilities;

namespace DeconTools.Backend.ProcessingTasks.ResultExporters.IsosResultExporters
{
    public sealed class O16O18IsosResultTextFileExporter : IsosResultTextFileExporter
    {
        #region Constructors
        public O16O18IsosResultTextFileExporter(string fileName)
            : this(fileName, 10000)
        {
        }

        public O16O18IsosResultTextFileExporter(string fileName, int triggerValueToExport)
        {
            TriggerToExport = triggerValueToExport;
            Delimiter = ',';
            Name = "O16O18 IsosResult TextFile Exporter";
            FileName = fileName;

            initializeAndWriteHeader();
        }

        #endregion

        protected override string buildIsosResultOutput(IsosResult result)
        {
            Check.Require(result is O16O18IsosResult, "Cannot use this O16O18ResultExporter with this type of result: " + result);

            var o16o18result = (O16O18IsosResult)result;

            var data = new List<string>
            {
                o16o18result.ScanSet.PrimaryScanNumber.ToString(),
                o16o18result.IsotopicProfile.ChargeState.ToString(),
                DblToString(o16o18result.IsotopicProfile.GetAbundance(), 4, true),
                DblToString(o16o18result.IsotopicProfile.GetMZofMostAbundantPeak(), 5),
                DblToString(o16o18result.IsotopicProfile.Score, 4),
                DblToString(o16o18result.IsotopicProfile.AverageMass, 5),
                DblToString(o16o18result.IsotopicProfile.MonoIsotopicMass, 5),
                DblToString(o16o18result.IsotopicProfile.MostAbundantIsotopeMass, 5),
                DblToString(o16o18result.IsotopicProfile.GetFWHM(), 4),
                DblToString(o16o18result.IsotopicProfile.GetSignalToNoise(), 2),
                DblToString(o16o18result.IsotopicProfile.GetMonoAbundance(), 4, true),
                DblToString(o16o18result.MonoPlus2Abundance, 4, true),
                DblToString(o16o18result.MonoPlus4Abundance, 4, true),
                DblToString(o16o18result.MonoMinus4Abundance, 4, true),
                ResultValidators.ResultValidationUtils.GetStringFlagCode(o16o18result.Flags),
                DblToString(o16o18result.InterferenceScore, 5)
            };

            //traditionally, the m/z of the most abundant peak is reported. If you want the m/z of the mono peak, get the monoIsotopic mass

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
                "mono_plus4_abundance",
                "mono_minus4_abundance",
                "flag",
                "interference_score"
            };

            return string.Join(Delimiter.ToString(), data);
        }
    }
}
