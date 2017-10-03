using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using DeconTools.Backend.Core;
using DeconTools.Backend.DTO;
using DeconTools.Utilities;

namespace DeconTools.Backend.Data
{
    public sealed class UIMFIsosOrigIntensitiesExporter : IsosExporter
    {
        private string fileName;

        public UIMFIsosOrigIntensitiesExporter(string fileName)
        {
            this.fileName = fileName;
            delimiter = ',';

            var data = new List<string>
            {
                "frame_num",
                "ims_scan_num",
                "charge",
                "abundance",
                "mz",
                "fit",
                "average_mw",
                "monoisotopic_mw",
                "mostabundant_mw",
                "fwhm",
                "signal_noise",
                "mono_abundance",
                "mono_plus2_abundance",
                "total_isotopic_abundance",
                "orig_intensity",
                "TIA_orig_intensity",
                "drift_time",
                "cumulative_drift_time"
            };

            headerLine = string.Join(delimiter.ToString(), data);
        }

        public override void Export(string binaryResultCollectionFilename, bool deleteBinaryFileAfterUse)
        {
            Check.Require(File.Exists(binaryResultCollectionFilename), "Can't write _isos; binaryResultFile doesn't exist: " + Utilities.DiagnosticUtilities.GetFullPathSafe(binaryResultCollectionFilename));

            StreamWriter sw;
            try
            {
                sw = new StreamWriter(fileName);

            }
            catch (Exception ex)
            {
                throw new IOException("Isos Exporter cannot export; Check if file is already opened.  Details: " + ex.Message);
            }

            sw.WriteLine(headerLine);

            ResultCollection results;
            var deserializer = new IsosResultDeSerializer(binaryResultCollectionFilename);

            do
            {
                results = deserializer.GetNextSetOfResults();

                
                //OriginalIntensitiesExtractor origIntensExtractor = new OriginalIntensitiesExtractor(results);
                //List<OriginalIntensitiesDTO> origIntensitiesCollection = origIntensExtractor.ExtractOriginalIntensities();
                //writeUIMFIsosResults(sw, results, origIntensitiesCollection);

            } while (results != null);

            sw.Close();
            deserializer.Close();


            if (deleteBinaryFileAfterUse)
            {
                try
                {
                    if (File.Exists(binaryResultCollectionFilename))
                    {
                        File.Delete(binaryResultCollectionFilename);
                    }

                }
                catch (Exception ex)
                {
                    throw new IOException("Exporter could not delete binary file. Details: " + ex.Message);

                }
            }


        }

        private void writeUIMFIsosResults(StreamWriter sw, ResultCollection results, List<OriginalIntensitiesDTO> origIntensitiesCollection)
        {
            Check.Require(results.ResultList.Count == origIntensitiesCollection.Count, "OriginalIntensities Data Transfer Object should have the same number of results as in the IsosResult object");
 
            
            if (results == null) return;

            var data = new List<string>();

            var counter = 0;
            foreach (var result in results.ResultList)
            {
                Check.Require(result is UIMFIsosResult, "UIMF Isos Exporter is only used with UIMF results");

                var uimfResult = (UIMFIsosResult)result;

                var originalIntensityInfo = origIntensitiesCollection[counter];

                data.Clear();
                data.Add(uimfResult.ScanSet.PrimaryScanNumber.ToString());
                data.Add(uimfResult.IMSScanSet.PrimaryScanNumber.ToString());
                data.Add(uimfResult.IsotopicProfile.ChargeState.ToString());
                data.Add(DblToString(uimfResult.IsotopicProfile.GetAbundance(), 4, true));
                data.Add(DblToString(uimfResult.IsotopicProfile.GetMZ(), 5));
                data.Add(DblToString(uimfResult.IsotopicProfile.GetScore(), 4));
                data.Add(DblToString(uimfResult.IsotopicProfile.AverageMass, 5));
                data.Add(DblToString(uimfResult.IsotopicProfile.MonoIsotopicMass, 5));
                data.Add(DblToString(uimfResult.IsotopicProfile.MostAbundantIsotopeMass, 5));
                data.Add(DblToString(uimfResult.IsotopicProfile.GetFWHM(), 4));
                data.Add(DblToString(uimfResult.IsotopicProfile.GetSignalToNoise(), 2));
                data.Add(DblToString(uimfResult.IsotopicProfile.GetMonoAbundance(), 4, true));
                data.Add(DblToString(uimfResult.IsotopicProfile.GetMonoPlusTwoAbundance(), 4, true));
                data.Add(DblToString(uimfResult.IsotopicProfile.GetSummedIntensity(), 4, true));
                data.Add(DblToString(originalIntensityInfo.originalIntensity, 4, true));
                data.Add(DblToString(originalIntensityInfo.totIsotopicOrginalIntens, 4, true));
                data.Add(DblToString(uimfResult.IMSScanSet.DriftTime, 3));
                data.Add(DblToString(uimfResult.IMSScanSet.DriftTime * (uimfResult.ScanSet.PrimaryScanNumber + 1), 2));     //PrimaryFrame is 0-based; so need to add one for calculation to be correct.

                sw.WriteLine(string.Join(delimiter.ToString(), data));

                counter++;
            }
        }

        public override void Export(DeconTools.Backend.Core.ResultCollection results)
        {
            StreamWriter sw;
            try
            {
                sw = new StreamWriter(fileName);

            }
            catch (Exception)
            {
                throw;
            }
            sw.WriteLine(headerLine);

            //OriginalIntensitiesExtractor origIntensExtractor = new OriginalIntensitiesExtractor(results);
            //List<OriginalIntensitiesDTO> origIntensitiesCollection = origIntensExtractor.ExtractOriginalIntensities();
            //writeUIMFIsosResults(sw, results, origIntensitiesCollection);

            sw.Close();
        }


       
        protected override string headerLine { get; set; }
        protected override char delimiter { get; set; }


    }
}
