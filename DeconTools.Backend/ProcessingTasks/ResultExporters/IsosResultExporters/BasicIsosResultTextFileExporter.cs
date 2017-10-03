using System.Collections.Generic;
using DeconTools.Backend.Core;

namespace DeconTools.Backend.ProcessingTasks.ResultExporters.IsosResultExporters
{
    public class BasicIsosResultTextFileExporter : IsosResultTextFileExporter
    {

        #region Constructors
        public BasicIsosResultTextFileExporter(string fileName)
            : this(fileName, 10000)
        {

        }


        public BasicIsosResultTextFileExporter(string fileName, int triggerValueToExport)
        {
            TriggerToExport = triggerValueToExport;
            Delimiter = ',';
            Name = "Basic IsosResult TextFile Exporter";
            FileName = fileName;

            initializeAndWriteHeader();

        }

        #endregion

        #region Properties

        #endregion

        protected override string buildIsosResultOutput(IsosResult result)
        {

            var data = new List<string>
            {
                result.ScanSet.PrimaryScanNumber.ToString(),
                result.IsotopicProfile.ChargeState.ToString(),
                DblToString(result.IntensityAggregate, 4, true),                    // Abundance
                DblToString(result.IsotopicProfile.GetMZofMostAbundantPeak(), 5),   // Traditionally, the m/z of the most abundant peak is reported. If you want the m/z of the mono peak, get the monoIsotopic mass
                DblToString(result.IsotopicProfile.Score, 4),                       // Fit score
                DblToString(result.IsotopicProfile.AverageMass, 5),
                DblToString(result.IsotopicProfile.MonoIsotopicMass, 5),
                DblToString(result.IsotopicProfile.MostAbundantIsotopeMass, 5),
                DblToString(result.IsotopicProfile.GetFWHM(), 4),
                DblToString(result.IsotopicProfile.GetSignalToNoise(), 2),
                DblToString(result.IsotopicProfile.GetMonoAbundance(), 4, true),
                DblToString(result.IsotopicProfile.GetMonoPlusTwoAbundance(), 4, true),
                ResultValidators.ResultValidationUtils.GetStringFlagCode(result.Flags),
                DblToString(result.InterferenceScore, 5)
            };

            // Abundance
            //traditionally, the m/z of the most abundant peak is reported. If you want the m/z of the mono peak, get the monoIsotopic mass
            // Fit score
            // Uncomment to write out the fit_count_basis
            //            //data.Add(result.IsotopicProfile.ScoreCountBasis);				// Number of points used for the fit score

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

            // Uncomment to write out the fit_count_basis
            //            //data.Add("fit_basis_count");

            return string.Join(Delimiter.ToString(), data);
        }
    }





}
