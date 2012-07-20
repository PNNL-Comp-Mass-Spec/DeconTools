using System;
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
    public class UIMFSQLiteScansExporter : ScansExporter
    {
        private string fileName;
        private DBWriter sqliteWriter = new DBWriter();

        public UIMFSQLiteScansExporter(string fileName)
        {
            this.fileName = fileName;
        }

        protected override string headerLine { get; set; }
        protected override char delimiter { get; set; }

        public override void Export(DeconTools.Backend.Core.ResultCollection results)
        {
            try
            {
                sqliteWriter.CreateNewDB(fileName);
                
            }
            catch (Exception)
            {
                throw;
            }

            //Insert records to IMS_Frames table
            //Insert records in bulk mood, 500 records each time
            //this is significantly faster than inserting one record at a time
            //500 records are the maximum number sqlite3 can handle
            ArrayList records = new ArrayList();
            int count = 0;
            foreach (ScanResult result in results.ScanResultList)
            {
                Check.Require(result is UimfScanResult, "UIMF_Scans_Exporter only works on UIMF Scan Results");
                UimfScanResult uimfResult = (UimfScanResult)result;

                IMS_Frames fp = new IMS_Frames();
                fp.frame_num = (ushort)uimfResult.Frameset.PrimaryFrame;
                fp.frame_time = (float)uimfResult.ScanTime;
                fp.type = (ushort)uimfResult.SpectrumType;
                fp.bpi = uimfResult.BasePeak.Height;
                fp.bpi_mz = (float)uimfResult.BasePeak.XValue;
                fp.tic = uimfResult.TICValue;
                fp.num_peaks = (uint)uimfResult.NumPeaks;
                fp.num_deisotoped = (uint)uimfResult.NumIsotopicProfiles;
                fp.frame_pressure_front = (float)uimfResult.FramePressureFront;
                fp.frame_pressure_back = (float)uimfResult.FramePressureBack;
                records.Add(fp);
                count++;
                if (count == 500)
                {
                    sqliteWriter.InsertIMSFrames(records);
                    count = 0;
                    records = new ArrayList();
                }
            }
            //Insert the rest of the records to IMS_Frames table
            sqliteWriter.InsertIMSFrames(records);
            sqliteWriter.CloseDB(fileName);
        }
    }
}
