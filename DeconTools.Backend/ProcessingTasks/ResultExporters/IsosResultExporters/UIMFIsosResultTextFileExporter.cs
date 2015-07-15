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
            sb.Append(uimfResult.ScanSet.PrimaryScanNumber);
            sb.Append(delimiter);
            sb.Append(uimfResult.IMSScanSet.PrimaryScanNumber);   
            sb.Append(delimiter);
            sb.Append(uimfResult.IsotopicProfile.ChargeState);
            sb.Append(delimiter);
            sb.Append(uimfResult.IntensityAggregate);
            sb.Append(delimiter);
            sb.Append(DblToString(uimfResult.IsotopicProfile.GetMZ(), 5));
            sb.Append(delimiter);
            sb.Append(DblToString(uimfResult.IsotopicProfile.GetScore(), 4));
            sb.Append(delimiter);
            sb.Append(DblToString(uimfResult.IsotopicProfile.AverageMass, 5));
            sb.Append(delimiter);
            sb.Append(DblToString(uimfResult.IsotopicProfile.MonoIsotopicMass, 5));
            sb.Append(delimiter);
            sb.Append(DblToString(uimfResult.IsotopicProfile.MostAbundantIsotopeMass, 5));
            sb.Append(delimiter);
            sb.Append(DblToString(uimfResult.IsotopicProfile.GetFWHM(), 4));
            sb.Append(delimiter);
            sb.Append(DblToString(uimfResult.IsotopicProfile.GetSignalToNoise(), 2));
            sb.Append(delimiter);
            sb.Append(uimfResult.IsotopicProfile.GetMonoAbundance());
            sb.Append(delimiter);
            sb.Append(uimfResult.IsotopicProfile.GetMonoPlusTwoAbundance());
            sb.Append(delimiter);
            sb.Append(uimfResult.IsotopicProfile.OriginalIntensity);
            sb.Append(delimiter);
            sb.Append(uimfResult.IsotopicProfile.IsSaturated ? 1 : 0);
            sb.Append(delimiter);
            sb.Append(DblToString(uimfResult.DriftTime, 3));
            sb.Append(delimiter);
            sb.Append(ResultValidators.ResultValidationUtils.GetStringFlagCode(result.Flags));
            sb.Append(delimiter);
            sb.Append(DblToString(uimfResult.InterferenceScore, 5));


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
