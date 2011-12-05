using System;
using System.Text;
using DeconTools.Backend.Core;
using DeconTools.Utilities;

namespace DeconTools.Backend.ProcessingTasks.ResultExporters.IsosResultExporters
{
    public class UIMFIsosResultTextFileExporter:IsosResultTextFileExporter
    {
        private int triggerVal;
        private char delimiter;

 



        #region Constructors
        public UIMFIsosResultTextFileExporter(string fileName)
            : this(fileName, 1000000)
        {

        }

        public UIMFIsosResultTextFileExporter(string fileName, int triggerValueToExport)
        {
            this.TriggerToExport = triggerValueToExport;
            this.delimiter = ',';
            this.Name = "UIMF IsosResult TextFile Exporter";
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





        protected override string buildIsosResultOutput(DeconTools.Backend.Core.IsosResult result)
        {
            Check.Require(result is UIMFIsosResult, "UIMF Isos Exporter is only used with UIMF results");
            UIMFIsosResult uimfResult = (UIMFIsosResult)result;

            if (MSFeatureIDsWritten.Contains(result.MSFeatureID))   //this prevents duplicate IDs from being written
            {
                return string.Empty;
            }

            MSFeatureIDsWritten.Add(result.MSFeatureID);

            StringBuilder sb = new StringBuilder();
          
            //We wish to report the FrameNum Not the FrameIndex.   FrameNum is unique
            sb.Append(uimfResult.MSFeatureID);
            sb.Append(delimiter);
            sb.Append(UIMFLibraryAdapter.getInstance(result.Run.Filename).Datareader.GetFrameNumByIndex(uimfResult.FrameSet.PrimaryFrame));
            sb.Append(delimiter);
            sb.Append(uimfResult.ScanSet.PrimaryScanNumber + 1);    //adds 1 to PrimaryScanNumber (which is 0-based)
            sb.Append(delimiter);
            sb.Append(uimfResult.IsotopicProfile.ChargeState);
            sb.Append(delimiter);
            sb.Append(uimfResult.IsotopicProfile.GetAbundance());
            sb.Append(delimiter);
            sb.Append(uimfResult.IsotopicProfile.GetMZ().ToString("0.#####"));
            sb.Append(delimiter);
            sb.Append(uimfResult.IsotopicProfile.GetScore().ToString("0.####"));
            sb.Append(delimiter);
            sb.Append(uimfResult.IsotopicProfile.AverageMass.ToString("0.#####"));
            sb.Append(delimiter);
            sb.Append(uimfResult.IsotopicProfile.MonoIsotopicMass.ToString("0.#####"));
            sb.Append(delimiter);
            sb.Append(uimfResult.IsotopicProfile.MostAbundantIsotopeMass.ToString("0.#####"));
            sb.Append(delimiter);
            sb.Append(uimfResult.IsotopicProfile.GetFWHM().ToString("0.####"));
            sb.Append(delimiter);
            sb.Append(uimfResult.IsotopicProfile.GetSignalToNoise().ToString("0.##"));
            sb.Append(delimiter);
            sb.Append(uimfResult.IsotopicProfile.GetMonoAbundance());
            sb.Append(delimiter);
            sb.Append(uimfResult.IsotopicProfile.GetMonoPlusTwoAbundance());
            sb.Append(delimiter);
            sb.Append(uimfResult.IsotopicProfile.OriginalIntensity);
            sb.Append(delimiter);
            sb.Append(uimfResult.IsotopicProfile.IsSaturated ? 1 : 0);
            sb.Append(delimiter);
            sb.Append(uimfResult.DriftTime.ToString("0.###"));
            sb.Append(delimiter);
            sb.Append(ResultValidators.ResultValidationUtils.GetStringFlagCode(result.Flags));
            sb.Append(delimiter);
            sb.Append(uimfResult.InterferenceScore.ToString("0.#####"));


            return sb.ToString();
        }

        protected override string buildHeaderLine()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("msfeature_id");
            sb.Append(Delimiter);
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
            sb.Append("unsummed_intensity");
            sb.Append(Delimiter);
            sb.Append("saturation_flag");
            sb.Append(Delimiter);
            sb.Append("drift_time");
            sb.Append(delimiter);
            sb.Append("flag");
            sb.Append(delimiter);
            sb.Append("interference_score");
            sb.Append(Environment.NewLine);

            return sb.ToString();
        }
    }
}
