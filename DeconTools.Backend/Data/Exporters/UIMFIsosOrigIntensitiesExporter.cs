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
                sb.Append(DblToString(uimfResult.IsotopicProfile.GetAbundance(), 4, true));
                sb.Append(delimiter);
                sb.Append(DblToString(uimfResult.IsotopicProfile.GetMZ(), 5));
                sb.Append(delimiter);
                sb.Append(DblToString(uimfResult.IsotopicProfile.GetScore(), 4));
                sb.Append(delimiter);
                sb.Append(DblToString(uimfResult.IsotopicProfile.AverageMass, 5));
                sb.Append(delimiter);
                sb.Append(DblToString(uimfResult.IsotopicProfile.MonoIsotopicMass, 5));
                sb.Append(delimiter);
                sb.Append(DblToString(uimfResult.IsotopicProfile.MostAbundantIsotopeMass, 5));
                sb.Append(delimiter);
                sb.Append(DblToString(uimfResult.IsotopicProfile.GetFWHM(), 4));
                sb.Append(delimiter);
                sb.Append(DblToString(uimfResult.IsotopicProfile.GetSignalToNoise(), 2));
                sb.Append(delimiter);
                sb.Append(DblToString(uimfResult.IsotopicProfile.GetMonoAbundance(), 4, true));
                sb.Append(delimiter);
                sb.Append(DblToString(uimfResult.IsotopicProfile.GetMonoPlusTwoAbundance(), 4, true));
                sb.Append(delimiter);
                sb.Append(DblToString(uimfResult.IsotopicProfile.GetSummedIntensity(), 4, true));
                sb.Append(delimiter);
                sb.Append(DblToString(origIntensitiesCollection[counter].originalIntensity, 4, true));
                sb.Append(delimiter);
                sb.Append(DblToString(origIntensitiesCollection[counter].totIsotopicOrginalIntens, 4, true));
                sb.Append(delimiter);
                sb.Append(DblToString(uimfResult.IMSScanSet.DriftTime, 3));
                sb.Append(delimiter);
                sb.Append(DblToString(uimfResult.IMSScanSet.DriftTime * (uimfResult.ScanSet.PrimaryScanNumber + 1), 2));     //PrimaryFrame is 0-based; so need to add one for calculation to be correct.

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
