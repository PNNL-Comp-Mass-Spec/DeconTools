﻿using System;
using System.Data;
using System.Runtime.InteropServices;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using DeconTools.Backend.Core;
using DeconTools.Utilities;
using MSDBLibrary;

namespace DeconTools.Backend.Data
{
    public class UIMFSQLiteIsosExporter : IsosExporter
    {
        private string fileName;
        private DBWriter sqliteWriter = new DBWriter();
        public UIMFSQLiteIsosExporter(string fileName)
        {
            this.fileName = fileName;
            this.delimiter = ',';
        }

        protected override string headerLine { get; set; }
        protected override char delimiter { get; set; }

        public override void Export(ResultCollection results)
        {
            try
            {
                sqliteWriter.CreateNewDB(fileName);
                
            }
            catch (Exception)
            {
                throw;
            }

            exportSQLiteUIMFIsosResults(results);

            sqliteWriter.CloseDB(fileName);
        }

        public override void Export(string binaryResultCollectionFilename, bool deleteBinaryFileAfterUse)
        {
            Check.Require(File.Exists(binaryResultCollectionFilename), "Can't write _isos; binaryResultFile doesn't exist");

            try
            {
                sqliteWriter.CreateNewDB(fileName);

            }
            catch (Exception ex)
            {
                throw new System.IO.IOException("Isos Exporter cannot export; Check if file is already opened.  Details: " + ex.Message);
            }

            ResultCollection results;
            IsosResultDeSerializer deserializer = new IsosResultDeSerializer(binaryResultCollectionFilename);

            do
            {
                results = deserializer.GetNextSetOfResults();
                exportSQLiteUIMFIsosResults(results);

            } while (results != null);

            sqliteWriter.CloseDB(fileName);
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



        private void exportSQLiteUIMFIsosResults(ResultCollection results)
        {
            if (results == null) return;

            //Insert records to MS_Features table
            //Insert records in bulk mood, 500 records each time
            //this is significantly faster than inserting one record at a time
            //500 records are the maximum number sqlite3 can handle
            ArrayList records = new ArrayList();
            int count = 0;
            foreach (IsosResult result in results.ResultList)
            {
                Check.Require(result is UIMFIsosResult, "UIMF Isos Exporter is only used with UIMF results");
                UIMFIsosResult uimfResult = (UIMFIsosResult)result;
                MS_Features fp = new MS_Features();
                fp.frame_num = (ushort)uimfResult.FrameSet.PrimaryFrame;
                fp.ims_scan_num = (ushort)getScanNumber(uimfResult.ScanSet.PrimaryScanNumber);
                fp.charge = (byte)uimfResult.IsotopicProfile.ChargeState;
                fp.abundance = (uint)uimfResult.IsotopicProfile.GetAbundance();
                fp.mz = uimfResult.IsotopicProfile.GetMZ();
                fp.fit = (float)uimfResult.IsotopicProfile.GetScore();
                fp.average_mw = uimfResult.IsotopicProfile.AverageMass;
                fp.monoisotopic_mw = uimfResult.IsotopicProfile.MonoIsotopicMass;
                fp.mostabundance_mw = uimfResult.IsotopicProfile.MostAbundantIsotopeMass;
                fp.fwhm = (float)uimfResult.IsotopicProfile.GetFWHM();
                fp.signal_noise = uimfResult.IsotopicProfile.GetSignalToNoise();
                fp.mono_abundance = (uint)uimfResult.IsotopicProfile.GetMonoAbundance();
                fp.mono_plus2_abundance = (uint)uimfResult.IsotopicProfile.GetMonoPlusTwoAbundance();
                fp.orig_intensity = (float)uimfResult.IsotopicProfile.OriginalIntensity;
                fp.TIA_orig_intensity = (float)uimfResult.IsotopicProfile.Original_Total_isotopic_abundance;
                fp.ims_drift_time = (float)uimfResult.ScanSet.DriftTime;
                records.Add(fp);
                count++;
                if (count == 500)
                {
                    sqliteWriter.InsertMSFeatures(records);
                    count = 0;
                    records = new ArrayList();
                }
            }
            //Insert the rest of the records to MS_Features table
            sqliteWriter.InsertMSFeatures(records);
        }

        protected override int getScanNumber(int scan_num)
        {
            return (scan_num + 1); //  in DeconTools.backend, we are 0-based;  but output has to be 1-based? (needs verification)
        }
    }
}