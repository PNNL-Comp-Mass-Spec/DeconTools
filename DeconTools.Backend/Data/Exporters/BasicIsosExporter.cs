using System;
using System.Collections.Generic;
using System.IO;

namespace DeconTools.Backend.Data
{
    public class BasicIsosExporter : IsosExporter
    {
        private readonly string fileName;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="fileName"></param>
        public BasicIsosExporter(string fileName)
        {
            delimiter = ',';
            headerLine = "scan_num,charge,abundance,mz,fit,average_mw,monoisotopic_mw,mostabundant_mw,fwhm,signal_noise,mono_abundance,mono_plus2_abundance";

            // Alternate header line if writing out the fit_count_basis
            //this.headerLine = "scan_num,charge,abundance,mz,fit,average_mw,monoisotopic_mw,mostabundant_mw,fwhm,signal_noise,mono_abundance,mono_plus2_abundance,fit_basis_count";
            this.fileName = fileName;
        }
        protected sealed override string headerLine { get; set; }
        protected sealed override char delimiter { get; set; }

        public override void Export(Core.ResultCollection results)
        {
            StreamWriter sw;

            try
            {
                sw = new StreamWriter(fileName);
            }
            catch (Exception)
            {
                throw new IOException("Could not access the output file. Check to see if it is already open.");
            }
            sw.WriteLine(headerLine);

            var data = new List<string>();

            foreach (var isosResult in results.ResultList)
            {
                var result = (StandardIsosResult)isosResult;

                data.Clear();
                data.Add(getScanNumber(result.ScanSet.PrimaryScanNumber).ToString());
                data.Add(result.IsotopicProfile.ChargeState.ToString());
                data.Add(DblToString(result.IsotopicProfile.GetAbundance(), 4, true));
                data.Add(DblToString(result.IsotopicProfile.GetMZ(), 5));
                data.Add(DblToString(result.IsotopicProfile.Score, 4));       // Fit Score
                data.Add(DblToString(result.IsotopicProfile.AverageMass, 5));
                data.Add(DblToString(result.IsotopicProfile.MonoIsotopicMass, 5));
                data.Add(DblToString(result.IsotopicProfile.MostAbundantIsotopeMass, 5));
                data.Add(DblToString(result.IsotopicProfile.GetFWHM(), 4));
                data.Add(DblToString(result.IsotopicProfile.GetSignalToNoise(), 2));
                data.Add(DblToString(result.IsotopicProfile.GetMonoAbundance(), 4, true));
                data.Add(DblToString(result.IsotopicProfile.GetMonoPlusTwoAbundance(), 4, true));

                // Uncomment to write out the fit_count_basis
                // data.Add(result.IsotopicProfile.ScoreCountBasis);				// Number of points used for the fit score

                sw.WriteLine(string.Join(delimiter.ToString(), data));
            }

            sw.Close();
        }

        protected override int getScanNumber(int scan_num)
        {
            return scan_num;
        }

        public override void Export(string binaryResultCollectionFilename, bool deleteBinaryFileAfterUse)
        {
            throw new NotImplementedException();
        }
    }
}
