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

            foreach (var isosResult in results.ResultList)
            {
                var result = (StandardIsosResult)isosResult;

                sb = new StringBuilder();
                sb.Append(getScanNumber(result.ScanSet.PrimaryScanNumber));       
                sb.Append(delimiter);
                sb.Append(result.IsotopicProfile.ChargeState);
                sb.Append(delimiter);
                sb.Append(DblToString(result.IsotopicProfile.GetAbundance(), 1));
                sb.Append(delimiter);
                sb.Append(DblToString(result.IsotopicProfile.GetMZ(), 5));
                sb.Append(delimiter);
                sb.Append(DblToString(result.IsotopicProfile.Score, 4));		// Fit Score
                sb.Append(delimiter);
                sb.Append(DblToString(result.IsotopicProfile.AverageMass, 5));
                sb.Append(delimiter);
                sb.Append(DblToString(result.IsotopicProfile.MonoIsotopicMass, 5));
                sb.Append(delimiter);
                //     this.headerLine = "scan_num,charge,abundance,mz,fit,average_mw,monoisotopic_mw,mostabundant_mw,fwhm,signal_noise,mono_abundance,mono_plus2_abundance";

                sb.Append(DblToString(result.IsotopicProfile.MostAbundantIsotopeMass, 5));
                sb.Append(delimiter);
                sb.Append(DblToString(result.IsotopicProfile.GetFWHM(), 4));
                sb.Append(delimiter);
                sb.Append(DblToString(result.IsotopicProfile.GetSignalToNoise(), 2));
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
