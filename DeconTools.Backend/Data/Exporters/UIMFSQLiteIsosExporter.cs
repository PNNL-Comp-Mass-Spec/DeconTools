using System;
using System.Collections;
using System.IO;
using DeconTools.Backend.Core;
using DeconTools.Utilities;
using MSDBLibrary;

namespace DeconTools.Backend.Data
{
    public class UIMFSQLiteIsosExporter : IsosExporter
    {
        private readonly string fileName;
        private readonly DBWriter sqLiteWriter = new DBWriter();

        public UIMFSQLiteIsosExporter(string fileName)
        {
            this.fileName = fileName;
            delimiter = ',';
        }

        protected override string headerLine { get; set; }
        protected sealed override char delimiter { get; set; }

        public override void Export(ResultCollection results)
        {
            try
            {
                sqLiteWriter.CreateNewDB(fileName);
            }
            catch (Exception)
            {
                throw;
            }

            exportSQLiteUIMFIsosResults(results);

            sqLiteWriter.CloseDB(fileName);
        }

        public override void Export(string binaryResultCollectionFilename, bool deleteBinaryFileAfterUse)
        {
            Check.Require(File.Exists(binaryResultCollectionFilename), "Can't write _isos; binaryResultFile doesn't exist: " + Utilities.DiagnosticUtilities.GetFullPathSafe(binaryResultCollectionFilename));

            try
            {
                sqLiteWriter.CreateNewDB(fileName);
            }
            catch (Exception ex)
            {
                throw new IOException("Isos Exporter cannot export; Check if file is already opened.  Details: " + ex.Message);
            }

            ResultCollection results;
            var deserializer = new IsosResultDeSerializer(binaryResultCollectionFilename);

            do
            {
                results = deserializer.GetNextSetOfResults();
                exportSQLiteUIMFIsosResults(results);

            } while (results != null);

            sqLiteWriter.CloseDB(fileName);
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

        private void exportSQLiteUIMFIsosResults(ResultCollection results)
        {
            if (results == null) return;

            //Insert records to MS_Features table
            //Insert records in bulk mood, 500 records each time
            //this is significantly faster than inserting one record at a time
            //500 records are the maximum number SQLite3 can handle
            var records = new ArrayList();
            var count = 0;
            foreach (var result in results.ResultList)
            {
                Check.Require(result is UIMFIsosResult, "UIMF Isos Exporter is only used with UIMF results");
                var uimfResult = (UIMFIsosResult)result;

                var fp = new MS_Features
                {
                    frame_num = (ushort)uimfResult.ScanSet.PrimaryScanNumber,
                    ims_scan_num = (ushort)uimfResult.IMSScanSet.PrimaryScanNumber,
                    charge = (byte)uimfResult.IsotopicProfile.ChargeState,
                    abundance = (uint)uimfResult.IsotopicProfile.GetAbundance(),
                    mz = uimfResult.IsotopicProfile.GetMZ(),
                    fit = (float)uimfResult.IsotopicProfile.GetScore(),
                    average_mw = uimfResult.IsotopicProfile.AverageMass,
                    monoisotopic_mw = uimfResult.IsotopicProfile.MonoIsotopicMass,
                    mostabundance_mw = uimfResult.IsotopicProfile.MostAbundantIsotopeMass,
                    fwhm = (float)uimfResult.IsotopicProfile.GetFWHM(),
                    signal_noise = uimfResult.IsotopicProfile.GetSignalToNoise(),
                    mono_abundance = (uint)uimfResult.IsotopicProfile.GetMonoAbundance(),
                    mono_plus2_abundance = (uint)uimfResult.IsotopicProfile.GetMonoPlusTwoAbundance(),
                    orig_intensity = (float)uimfResult.IsotopicProfile.OriginalIntensity,
                    ims_drift_time = (float)uimfResult.IMSScanSet.DriftTime
                };

                //fp.TIA_orig_intensity = (float)uimfResult.IsotopicProfile.OriginalTotalIsotopicAbundance;
                records.Add(fp);
                count++;
                if (count == 500)
                {
                    sqLiteWriter.InsertMSFeatures(records);
                    count = 0;
                    records = new ArrayList();
                }
            }

            //Insert the rest of the records to MS_Features table
            sqLiteWriter.InsertMSFeatures(records);
        }

        protected override int getScanNumber(int scan_num)
        {
            return (scan_num + 1); //  in DeconTools.backend, we are 0-based;  but output has to be 1-based? (needs verification)
        }
    }
}
