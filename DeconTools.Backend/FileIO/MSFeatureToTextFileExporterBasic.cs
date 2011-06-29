using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core;

namespace DeconTools.Backend.FileIO
{
    public class MSFeatureToTextFileExporterBasic : TextFileExporter<IsosResult>
    {
        #region Constructors
        public MSFeatureToTextFileExporterBasic(string fileName)
            : this(fileName, ',')
        {

        }

        public MSFeatureToTextFileExporterBasic(string fileName, char delimiter)
        {
            this.Name = "MSFeatureToTextFileExporterBasic";
            this.Delimiter = delimiter;
            this.FileName = fileName;

            initializeAndWriteHeader();

        }

        #endregion

        #region Private Methods
        protected override string buildResultOutput(IsosResult result)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(result.ScanSet.PrimaryScanNumber);
            sb.Append(Delimiter);
            sb.Append(result.IsotopicProfile.ChargeState);
            sb.Append(Delimiter);
            sb.Append(result.IsotopicProfile.GetAbundance());
            sb.Append(Delimiter);
            sb.Append(result.IsotopicProfile.GetMZ().ToString("0.#####"));
            sb.Append(Delimiter);
            sb.Append(result.IsotopicProfile.Score.ToString("0.####"));
            sb.Append(Delimiter);
            sb.Append(result.IsotopicProfile.AverageMass.ToString("0.#####"));
            sb.Append(Delimiter);
            sb.Append(result.IsotopicProfile.MonoIsotopicMass.ToString("0.#####"));
            sb.Append(Delimiter);
            sb.Append(result.IsotopicProfile.MostAbundantIsotopeMass.ToString("0.#####"));
            sb.Append(Delimiter);
            sb.Append(result.IsotopicProfile.GetFWHM().ToString("0.####"));
            sb.Append(Delimiter);
            sb.Append(result.IsotopicProfile.GetSignalToNoise().ToString("0.##"));
            sb.Append(Delimiter);
            sb.Append(result.IsotopicProfile.GetMonoAbundance());
            sb.Append(Delimiter);
            sb.Append(result.IsotopicProfile.GetMonoPlusTwoAbundance());
            sb.Append(Delimiter);
            sb.Append(DeconTools.Backend.ProcessingTasks.ResultValidators.ResultValidationUtils.GetStringFlagCode(result.Flags));
            sb.Append(Delimiter);
            sb.Append(result.InterferenceScore.ToString("0.#####"));
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
            sb.Append("flag");
            sb.Append(Delimiter);
            sb.Append("interference_score");
            return sb.ToString();
        }
        #endregion

  
    }
}
