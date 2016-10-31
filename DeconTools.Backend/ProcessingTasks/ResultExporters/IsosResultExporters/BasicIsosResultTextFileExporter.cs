using System;
using System.Text;
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
            this.TriggerToExport = triggerValueToExport;
            this.Delimiter = ',';
            this.Name = "Basic IsosResult TextFile Exporter";
            this.FileName = fileName;

            initializeAndWriteHeader();

        }

        #endregion

        #region Properties
      
        #endregion

        protected override string buildIsosResultOutput(IsosResult result)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(result.ScanSet.PrimaryScanNumber);
            sb.Append(Delimiter);
            sb.Append(result.IsotopicProfile.ChargeState);
            sb.Append(Delimiter);
            sb.Append(DblToString(result.IntensityAggregate, 4, true));       // Abundance
            sb.Append(Delimiter);
            sb.Append(DblToString(result.IsotopicProfile.GetMZofMostAbundantPeak(), 5));   //traditionally, the m/z of the most abundant peak is reported. If you want the m/z of the mono peak, get the monoIsotopic mass
            sb.Append(Delimiter);
            sb.Append(DblToString(result.IsotopicProfile.Score, 4));				// Fit score
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
            sb.Append(ResultValidators.ResultValidationUtils.GetStringFlagCode(result.Flags));
            sb.Append(Delimiter);
            sb.Append(DblToString(result.InterferenceScore, 5));
            // Uncomment to write out the fit_count_basis
            //sb.Append(Delimiter);
            //sb.Append(result.IsotopicProfile.ScoreCountBasis);				// Number of points used for the fit score
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
            // Uncomment to write out the fit_count_basis
            //sb.Append(Delimiter);
            //sb.Append("fit_basis_count");
            sb.Append(Environment.NewLine);
            return sb.ToString();
        }
    }
        

  


}
