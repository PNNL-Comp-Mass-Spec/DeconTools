using System;
using System.IO;
using System.Text;
using DeconTools.Backend.Core;
using DeconTools.Utilities;

namespace DeconTools.Backend.Data
{
    public class UIMFIsosExporter : IsosExporter
    {
        private string fileName;
        public UIMFIsosExporter(string fileName)
        {
            this.fileName = fileName;
            this.delimiter = ',';
            this.headerLine = "frame_num,ims_scan_num,charge,abundance,mz,fit,";
            this.headerLine += "average_mw,monoisotopic_mw,mostabundant_mw,fwhm,";
            this.headerLine += "signal_noise,mono_abundance,mono_plus2_abundance,";
            this.headerLine += "orig_intensity,TIA_orig_intensity,drift_time";
        }

        protected override string headerLine { get; set; }
        protected override char delimiter { get; set; }

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

            writeUIMFIsosResults(sw, results);

            sw.Close();
        }

        public override void Export(string binaryResultCollectionFilename, bool deleteBinaryFileAfterUse)
        {
            Check.Require(File.Exists(binaryResultCollectionFilename), "Can't write _isos; binaryResultFile doesn't exist");

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
                    throw new System.IO.IOException("Exporter could not delete binary file. Details: " + ex.Message);

                }
            }



        }


        private void writeUIMFIsosResults(StreamWriter sw, ResultCollection results)
        {
            if (results == null) return;
            StringBuilder sb;

            foreach (IsosResult result in results.ResultList)
            {
                Check.Require(result is UIMFIsosResult, "UIMF Isos Exporter is only used with UIMF results");
                UIMFIsosResult uimfResult = (UIMFIsosResult)result;

                sb = new StringBuilder();
                sb.Append(uimfResult.FrameSet.PrimaryFrame);
                sb.Append(delimiter);
                sb.Append(getScanNumber(uimfResult.ScanSet.PrimaryScanNumber));    //calls a method that adds 1 to PrimaryScanNumber (which is 0-based)
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
                sb.Append(uimfResult.IsotopicProfile.OriginalIntensity);
                sb.Append(delimiter);
                sb.Append(uimfResult.IsotopicProfile.IsSaturated ? 1 : 0);
                sb.Append(delimiter);
                sb.Append(uimfResult.ScanSet.DriftTime.ToString("0.###"));
                sw.WriteLine(sb.ToString());
            }
        }




        protected override int getScanNumber(int scan_num)
        {
            return (scan_num + 1); //  in DeconTools.backend, we are 0-based;  but output has to be 1-based? (needs verification)
        }
    }
}
