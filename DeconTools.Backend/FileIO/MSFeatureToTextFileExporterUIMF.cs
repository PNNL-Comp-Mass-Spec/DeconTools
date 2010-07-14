using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core;
using DeconTools.Utilities;

namespace DeconTools.Backend.FileIO
{
    public class MSFeatureToTextFileExporterUIMF:TextFileExporter<IsosResult>
    {
        #region Constructors
        public MSFeatureToTextFileExporterUIMF(string fileName)
            : this(fileName, ',')
        {

        }

        public MSFeatureToTextFileExporterUIMF(string fileName, char delimiter)
        {
            this.Name = "MSFeatureToTextFileExporterUIMF";
            this.Delimiter = delimiter;
            this.FileName = fileName;

            initializeAndWriteHeader();
        }
        #endregion
  
        #region Private Methods
        protected override string buildResultOutput(IsosResult result)
        {
            Check.Require(result is UIMFIsosResult, "UIMF Isos Exporter is only used with UIMF results");
            UIMFIsosResult uimfResult = (UIMFIsosResult)result;

            StringBuilder sb = new StringBuilder();
            sb.Append(uimfResult.FrameSet.PrimaryFrame);
            sb.Append(Delimiter);
            sb.Append(uimfResult.ScanSet.PrimaryScanNumber + 1);    //adds 1 to PrimaryScanNumber (which is 0-based)
            sb.Append(Delimiter);
            sb.Append(uimfResult.IsotopicProfile.ChargeState);
            sb.Append(Delimiter);
            sb.Append(uimfResult.IsotopicProfile.GetAbundance());
            sb.Append(Delimiter);
            sb.Append(uimfResult.IsotopicProfile.GetMZ().ToString("0.#####"));
            sb.Append(Delimiter);
            sb.Append(uimfResult.IsotopicProfile.GetScore().ToString("0.####"));
            sb.Append(Delimiter);
            sb.Append(uimfResult.IsotopicProfile.AverageMass.ToString("0.#####"));
            sb.Append(Delimiter);
            sb.Append(uimfResult.IsotopicProfile.MonoIsotopicMass.ToString("0.#####"));
            sb.Append(Delimiter);
            sb.Append(uimfResult.IsotopicProfile.MostAbundantIsotopeMass.ToString("0.#####"));
            sb.Append(Delimiter);
            sb.Append(uimfResult.IsotopicProfile.GetFWHM().ToString("0.####"));
            sb.Append(Delimiter);
            sb.Append(uimfResult.IsotopicProfile.GetSignalToNoise().ToString("0.##"));
            sb.Append(Delimiter);
            sb.Append(uimfResult.IsotopicProfile.GetMonoAbundance());
            sb.Append(Delimiter);
            sb.Append(uimfResult.IsotopicProfile.GetMonoPlusTwoAbundance());
            sb.Append(Delimiter);
            sb.Append(uimfResult.IsotopicProfile.OriginalIntensity);
            sb.Append(Delimiter);
            sb.Append(uimfResult.IsotopicProfile.Original_Total_isotopic_abundance);
            sb.Append(Delimiter);
            sb.Append(uimfResult.DriftTime.ToString("0.###"));
            sb.Append(Delimiter);
            sb.Append(DeconTools.Backend.ProcessingTasks.ResultValidators.ResultValidationUtils.GetStringFlagCode(result.Flags));


            return sb.ToString();
        }

        protected override string buildHeaderLine()
        {
            StringBuilder sb = new StringBuilder();

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
            sb.Append(Environment.NewLine);

            return sb.ToString();
        }
        #endregion
    
    }
}
