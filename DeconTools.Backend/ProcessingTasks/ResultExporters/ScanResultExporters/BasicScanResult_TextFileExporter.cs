using System.Text;
using DeconTools.Backend.Core;

namespace DeconTools.Backend.ProcessingTasks.ResultExporters.ScanResultExporters
{
    public class BasicScanResult_TextFileExporter : ScanResult_TextFileExporter
    {
  
        #region Constructors
        public BasicScanResult_TextFileExporter(string fileName):base(fileName) {}
        
        #endregion

   
        protected override string buildScansResultOutput(ScanResult result)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(result.ScanSet.PrimaryScanNumber);
            sb.Append(Delimiter);
            sb.Append(result.ScanTime.ToString("0.####"));
            sb.Append(Delimiter);
            sb.Append(result.SpectrumType);
            sb.Append(Delimiter);
            sb.Append(result.BasePeak.Height);
            sb.Append(Delimiter);
            sb.Append(result.BasePeak.XValue.ToString("0.#####"));
            sb.Append(Delimiter);
            sb.Append(result.ScanSet.TICValue);
            sb.Append(Delimiter);
            sb.Append(result.NumPeaks);
            sb.Append(Delimiter);
            sb.Append(result.NumIsotopicProfiles);
            sb.Append(Delimiter);
            sb.Append(result.Description);

            return sb.ToString();


        }

        protected override string buildHeaderLine()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("scan_num");
            sb.Append(Delimiter);
            sb.Append("scan_time");
            sb.Append(Delimiter); 
            sb.Append("type");
            sb.Append(Delimiter);
            sb.Append("bpi");
            sb.Append(Delimiter);
            sb.Append("bpi_mz");
            sb.Append(Delimiter);
            sb.Append("tic");
            sb.Append(Delimiter);
            sb.Append("num_peaks");
            sb.Append(Delimiter);
            sb.Append("num_deisotoped");
            sb.Append(Delimiter);
            sb.Append("info");
           

            return sb.ToString();
        }

      
    }
}
