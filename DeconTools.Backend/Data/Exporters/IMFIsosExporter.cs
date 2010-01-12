using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace DeconTools.Backend.Data
{
    public class IMFIsosExporter : IsosExporter
    {
        private string fileName;

        public IMFIsosExporter(string fileName)
        {
            this.delimiter = ',';
            this.headerLine = "scan_num,charge,abundance,mz,fit,average_mw,monoisotopic_mw,mostabundant_mw,fwhm,signal_noise,mono_abundance,mono_plus2_abundance,orig_intensity,TIA_orig_intensity";
            this.fileName = fileName;
        }


        public override void Export(string binaryResultCollectionFilename, bool deleteBinaryFileAfterUse)
        {
            throw new NotImplementedException();
        }

        protected override string headerLine { get; set; }
        protected override char delimiter { get; set; }


        public override void Export(DeconTools.Backend.Core.ResultCollection results)
        {
            StringBuilder sb;
            StreamWriter sw;
            try
            {
                sw = new StreamWriter(this.fileName);

            }
            catch (Exception)
            {
                throw new System.IO.IOException("Could not access the output file. Check to see if it is already open.");
            }
            sw.WriteLine(headerLine);

            foreach (StandardIsosResult result in results.ResultList)
            {
                sb = new StringBuilder();
                sb.Append(getScanNumber(result.ScanSet.PrimaryScanNumber));
                sb.Append(delimiter);
                sb.Append(result.IsotopicProfile.ChargeState);
                sb.Append(delimiter);
                sb.Append(result.IsotopicProfile.GetAbundance());
                sb.Append(delimiter);
                sb.Append(result.IsotopicProfile.GetMZ().ToString("0.#####"));
                sb.Append(delimiter);
                sb.Append(result.IsotopicProfile.Score.ToString("0.####"));
                sb.Append(delimiter);
                sb.Append(result.IsotopicProfile.AverageMass.ToString("0.#####"));
                sb.Append(delimiter);
                sb.Append(result.IsotopicProfile.MonoIsotopicMass.ToString("0.#####"));
                sb.Append(delimiter);
                sb.Append(result.IsotopicProfile.MostAbundantIsotopeMass.ToString("0.#####"));
                sb.Append(delimiter);
                sb.Append(result.IsotopicProfile.GetFWHM().ToString("0.####"));
                sb.Append(delimiter);
                sb.Append(result.IsotopicProfile.GetSignalToNoise().ToString("0.##"));
                sb.Append(delimiter);
                sb.Append(result.IsotopicProfile.GetMonoAbundance());
                sb.Append(delimiter);
                sb.Append(result.IsotopicProfile.GetMonoPlusTwoAbundance());
                sb.Append(delimiter);
                sb.Append(result.IsotopicProfile.OriginalIntensity);
                sb.Append(delimiter);
                sb.Append(result.IsotopicProfile.Original_Total_isotopic_abundance);
                sw.WriteLine(sb.ToString());
            }

            sw.Close();
        }
    }
}
