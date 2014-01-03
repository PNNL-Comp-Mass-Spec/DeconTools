using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace DeconTools.Backend.Data
{
    public class BasicIsosExporter : IsosExporter
    {
        private string fileName;
        public BasicIsosExporter(string fileName)
        {
            this.delimiter = ',';
			this.headerLine = "scan_num,charge,abundance,mz,fit,average_mw,monoisotopic_mw,mostabundant_mw,fwhm,signal_noise,mono_abundance,mono_plus2_abundance";
			// Alternate header line if writing out the fit_count_basis
			//this.headerLine = "scan_num,charge,abundance,mz,fit,average_mw,monoisotopic_mw,mostabundant_mw,fwhm,signal_noise,mono_abundance,mono_plus2_abundance,fit_basis_count";
			this.fileName = fileName;
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
                sb.Append(result.IsotopicProfile.Score.ToString("0.####"));		// Fit Score
                sb.Append(delimiter);
                sb.Append(result.IsotopicProfile.AverageMass.ToString("0.#####"));
                sb.Append(delimiter);
                sb.Append(result.IsotopicProfile.MonoIsotopicMass.ToString("0.#####"));
                sb.Append(delimiter);
                //     this.headerLine = "scan_num,charge,abundance,mz,fit,average_mw,monoisotopic_mw,mostabundant_mw,fwhm,signal_noise,mono_abundance,mono_plus2_abundance";

                sb.Append(result.IsotopicProfile.MostAbundantIsotopeMass.ToString("0.#####"));
                sb.Append(delimiter);
                sb.Append(result.IsotopicProfile.GetFWHM().ToString("0.####"));
                sb.Append(delimiter);
                sb.Append(result.IsotopicProfile.GetSignalToNoise().ToString("0.##"));
                sb.Append(delimiter);
                sb.Append(result.IsotopicProfile.GetMonoAbundance());
                sb.Append(delimiter);
                sb.Append(result.IsotopicProfile.GetMonoPlusTwoAbundance());
				// Uncomment to write out the fit_count_basis
				//sb.Append(delimiter);
				//sb.Append(result.IsotopicProfile.ScoreCountBasis);				// Number of points used for the fit score

                sw.WriteLine(sb.ToString());
            }

            sw.Close();

        }

        protected override int getScanNumber(int scan_num)
        {
            return scan_num;         //
        }

        public override void Export(string binaryResultCollectionFilename, bool deleteBinaryFileAfterUse)
        {
            throw new NotImplementedException();
        }
    }
}
