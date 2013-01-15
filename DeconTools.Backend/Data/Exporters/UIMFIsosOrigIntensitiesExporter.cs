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
    public class UIMFIsosOrigIntensitiesExporter : IsosExporter
    {
        private string fileName;

        public UIMFIsosOrigIntensitiesExporter(string fileName)
        {
            this.fileName = fileName;
            this.delimiter = ',';
            this.headerLine = "frame_num,ims_scan_num,charge,abundance,mz,fit,";
            this.headerLine += "average_mw,monoisotopic_mw,mostabundant_mw,fwhm,";
            this.headerLine += "signal_noise,mono_abundance,mono_plus2_abundance,total_isotopic_abundance,";
            this.headerLine += "orig_intensity,TIA_orig_intensity,";
            this.headerLine += "drift_time,cumulative_drift_time";
        }

        public override void Export(string binaryResultCollectionFilename, bool deleteBinaryFileAfterUse)
        {
			Check.Require(File.Exists(binaryResultCollectionFilename), "Can't write _isos; binaryResultFile doesn't exist: " + Utilities.DiagnosticUtilities.GetFullPathSafe(binaryResultCollectionFilename));

            StreamWriter sw;
            try
            {
                sw = new StreamWriter(this.fileName);

            }
            catch (Exception ex)
            {
                throw new System.IO.IOException("Isos Exporter cannot export; Check if file is already opened.  Details: " + ex.Message);
            }

            sw.WriteLine(headerLine);

            ResultCollection results;
            IsosResultDeSerializer deserializer = new IsosResultDeSerializer(binaryResultCollectionFilename);

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
                    throw new System.IO.IOException("Exporter could not delete binary file. Details: " + ex.Message);

                }
            }


        }

        private void writeUIMFIsosResults(StreamWriter sw, ResultCollection results, List<OriginalIntensitiesDTO> origIntensitiesCollection)
        {
            Check.Require(results.ResultList.Count == origIntensitiesCollection.Count, "OriginalIntensities Data Transfer Object should have the same number of results as in the IsosResult object");
 
            
            if (results == null) return;
            StringBuilder sb;


            int counter = 0;
            foreach (IsosResult result in results.ResultList)
            {
                Check.Require(result is UIMFIsosResult, "UIMF Isos Exporter is only used with UIMF results");
                
                
                UIMFIsosResult uimfResult = (UIMFIsosResult)result;

                OriginalIntensitiesDTO originalIntensitiesDataObject = origIntensitiesCollection[counter];

                sb = new StringBuilder();
                sb.Append(uimfResult.ScanSet.PrimaryScanNumber);
                sb.Append(delimiter);
                sb.Append(uimfResult.IMSScanSet.PrimaryScanNumber);    
                sb.Append(delimiter);
                sb.Append(uimfResult.IsotopicProfile.ChargeState);
                sb.Append(delimiter);
                sb.Append(uimfResult.IsotopicProfile.GetAbundance());
                sb.Append(delimiter);
                sb.Append(uimfResult.IsotopicProfile.GetMZ().ToString("0.#####"));
                sb.Append(delimiter);
                sb.Append(uimfResult.IsotopicProfile.GetScore().ToString("0.####"));
                sb.Append(delimiter);
                sb.Append(uimfResult.IsotopicProfile.AverageMass.ToString("0.#####"));
                sb.Append(delimiter);
                sb.Append(uimfResult.IsotopicProfile.MonoIsotopicMass.ToString("0.#####"));
                sb.Append(delimiter);
                sb.Append(uimfResult.IsotopicProfile.MostAbundantIsotopeMass.ToString("0.#####"));
                sb.Append(delimiter);
                sb.Append(uimfResult.IsotopicProfile.GetFWHM().ToString("0.####"));
                sb.Append(delimiter);
                sb.Append(uimfResult.IsotopicProfile.GetSignalToNoise().ToString("0.##"));
                sb.Append(delimiter);
                sb.Append(uimfResult.IsotopicProfile.GetMonoAbundance());
                sb.Append(delimiter);
                sb.Append(uimfResult.IsotopicProfile.GetMonoPlusTwoAbundance());
                sb.Append(delimiter);
                sb.Append(uimfResult.IsotopicProfile.GetSummedIntensity());
                sb.Append(delimiter);
                sb.Append(origIntensitiesCollection[counter].originalIntensity);
                sb.Append(delimiter);
                sb.Append(origIntensitiesCollection[counter].totIsotopicOrginalIntens);
                sb.Append(delimiter);
                sb.Append(uimfResult.IMSScanSet.DriftTime.ToString("0.###"));
                sb.Append(delimiter);
                sb.Append((uimfResult.IMSScanSet.DriftTime * (uimfResult.ScanSet.PrimaryScanNumber + 1)).ToString("0.##"));     //PrimaryFrame is 0-based; so need to add one for calculation to be correct.

                sw.WriteLine(sb.ToString());

                counter++;
            }
        }

        public override void Export(DeconTools.Backend.Core.ResultCollection results)
        {
            StreamWriter sw;
            try
            {
                sw = new StreamWriter(this.fileName);

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
