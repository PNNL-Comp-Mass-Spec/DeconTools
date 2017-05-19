using System;
using System.Text;
using DeconTools.Backend.Core;
using DeconTools.Utilities;

namespace DeconTools.Backend.ProcessingTasks.ResultExporters.IsosResultExporters
{
    public class O16O18IsosResultTextFileExporter : IsosResultTextFileExporter
    {


        #region Constructors
        public O16O18IsosResultTextFileExporter(string fileName)
            : this(fileName, 10000)
        {

        }

        public O16O18IsosResultTextFileExporter(string fileName, int triggerValueToExport)
        {
            this.TriggerToExport = triggerValueToExport;
            this.Delimiter = ',';
            this.Name = "O16O18 IsosResult TextFile Exporter";
            this.FileName = fileName;

            initializeAndWriteHeader();

        }

        #endregion

   

        protected override string buildIsosResultOutput(Core.IsosResult result)
        {
            Check.Require(result is O16O18IsosResult, "Cannot use this O16O18ResultExporter with this type of result: " + result);

            var o16o18result = (O16O18IsosResult)result;


            var sb = new StringBuilder();



            sb.Append(o16o18result.ScanSet.PrimaryScanNumber);
            sb.Append(Delimiter);
            sb.Append(o16o18result.IsotopicProfile.ChargeState);
            sb.Append(Delimiter);
            sb.Append(DblToString(o16o18result.IsotopicProfile.GetAbundance(), 4, true));
            sb.Append(Delimiter);
            sb.Append(DblToString(o16o18result.IsotopicProfile.GetMZofMostAbundantPeak(), 5));   //traditionally, the m/z of the most abundant peak is reported. If you want the m/z of the mono peak, get the monoIsotopic mass
            sb.Append(Delimiter);
            sb.Append(DblToString(o16o18result.IsotopicProfile.Score, 4));
            sb.Append(Delimiter);
            sb.Append(DblToString(o16o18result.IsotopicProfile.AverageMass, 5));
            sb.Append(Delimiter);
            sb.Append(DblToString(o16o18result.IsotopicProfile.MonoIsotopicMass, 5));
            sb.Append(Delimiter);
            sb.Append(DblToString(o16o18result.IsotopicProfile.MostAbundantIsotopeMass, 5));
            sb.Append(Delimiter);
            sb.Append(DblToString(o16o18result.IsotopicProfile.GetFWHM(), 4));
            sb.Append(Delimiter);
            sb.Append(DblToString(o16o18result.IsotopicProfile.GetSignalToNoise(), 2));
            sb.Append(Delimiter);
            sb.Append(DblToString(o16o18result.IsotopicProfile.GetMonoAbundance(), 4, true));
            sb.Append(Delimiter);
            sb.Append(DblToString(o16o18result.MonoPlus2Abundance, 4, true));
            sb.Append(Delimiter);
            sb.Append(DblToString(o16o18result.MonoPlus4Abundance, 4, true));
            sb.Append(Delimiter);
            sb.Append(DblToString(o16o18result.MonoMinus4Abundance, 4, true));
            sb.Append(Delimiter);

            sb.Append(ResultValidators.ResultValidationUtils.GetStringFlagCode(o16o18result.Flags));
            sb.Append(Delimiter);
            sb.Append(DblToString(o16o18result.InterferenceScore, 5));
            return sb.ToString();
        }

        protected override string buildHeaderLine()
        {
            var sb = new StringBuilder();
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
            sb.Append("mono_plus4_abundance");
            sb.Append(Delimiter);
            sb.Append("mono_minus4_abundance");
            sb.Append(Delimiter);
            sb.Append("flag");
            sb.Append(Delimiter);
            sb.Append("interference_score");
            sb.Append(Environment.NewLine);
            return sb.ToString();
        }


    }
}
