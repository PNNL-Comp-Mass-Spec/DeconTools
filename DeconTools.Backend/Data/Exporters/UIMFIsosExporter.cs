using System;
using System.IO;
using System.Text;
using DeconTools.Backend.Core;
using DeconTools.Utilities;

namespace DeconTools.Backend.Data
{
    public sealed class UIMFIsosExporter : IsosExporter
    {
        private readonly string fileName;

        public UIMFIsosExporter(string fileName)
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
                "orig_intensity",
                "TIA_orig_intensity",
                "drift_time"
            };

            headerLine = string.Join(delimiter.ToString(), data);

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

            writeUIMFIsosResults(sw, results);

            sw.Close();
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
                writeUIMFIsosResults(sw, results);

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


        private void writeUIMFIsosResults(StreamWriter sw, ResultCollection results)
        {
            if (results == null) return;

            var data = new List<string>();

            foreach (var result in results.ResultList)
            {
                Check.Require(result is UIMFIsosResult, "UIMF Isos Exporter is only used with UIMF results");
                var uimfResult = (UIMFIsosResult)result;

                data.Clear();
                data.Add(uimfResult.ScanSet.PrimaryScanNumber.ToString());
                data.Add(getScanNumber(uimfResult.IMSScanSet.PrimaryScanNumber).ToString());    //calls a method that adds 1 to PrimaryScanNumber (which is 0-based)
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
                data.Add(DblToString(uimfResult.IsotopicProfile.OriginalIntensity, 4, true));
                data.Add(uimfResult.IsotopicProfile.IsSaturatedAsNumericText);
                data.Add(DblToString(uimfResult.IMSScanSet.DriftTime, 3));

                sw.WriteLine(string.Join(delimiter.ToString(), data));
            }
        }



        protected override int getScanNumber(int scan_num)
        {
            return (scan_num + 1); //  in DeconTools.backend, we are 0-based;  but output has to be 1-based? (needs verification)
        }
    }
}
