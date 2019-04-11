using System;
using System.Collections.Generic;
using DeconTools.Backend.Core;
using DeconTools.Utilities;

namespace DeconTools.Backend.ProcessingTasks.ResultExporters.IsosResultExporters
{
    public sealed class UIMFIsosResultTextFileExporter : IsosResultTextFileExporter
    {
        #region Constructors
        public UIMFIsosResultTextFileExporter(string fileName)
            : this(fileName, 1000000)
        {

        }

        public UIMFIsosResultTextFileExporter(string fileName, int triggerValueToExport)
        {
            TriggerToExport = triggerValueToExport;
            Delimiter = ',';
            Name = "UIMF IsosResult TextFile Exporter";
            FileName = fileName;

            initializeAndWriteHeader();

        }
        #endregion

        #region Properties

        public override char Delimiter { get; set; }

        public override int TriggerToExport { get; set; }

        #endregion

        protected override string buildIsosResultOutput(IsosResult result)
        {
            Check.Require(result is UIMFIsosResult, "UIMF Isos Exporter is only used with UIMF results");
            var uimfResult = (UIMFIsosResult)result;

            if (MSFeatureIDsWritten.Contains(result.MSFeatureID))   //this prevents duplicate IDs from being written
            {
                return string.Empty;
            }

            MSFeatureIDsWritten.Add(result.MSFeatureID);

            var data = new List<string>
            {
                uimfResult.MSFeatureID.ToString(),
                uimfResult.ScanSet.PrimaryScanNumber.ToString(),           //We wish to report the FrameNum Not the FrameIndex.   FrameNum is unique
                uimfResult.IMSScanSet.PrimaryScanNumber.ToString(),
                uimfResult.IsotopicProfile.ChargeState.ToString(),
                DblToString(uimfResult.IntensityAggregate, 4, true),        // Abundance
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
                ResultValidators.ResultValidationUtils.GetStringFlagCode(result.Flags),
                DblToString(uimfResult.InterferenceScore, 5)
            };

            return string.Join(Delimiter.ToString(), data);
        }

        protected override string buildHeaderLine()
        {
            var data = new List<string>
            {
                "msfeature_id",
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
                "unsummed_intensity",
                "saturation_flag",
                "drift_time",
                "flag",
                "interference_score"
            };

            return string.Join(Delimiter.ToString(), data);
        }
    }
}
