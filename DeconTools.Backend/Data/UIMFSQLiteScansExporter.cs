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

            ArrayList records = new ArrayList();
            foreach (ScanResult result in results.ScanResultList)
            {
                Check.Require(result is UIMFScanResult, "UIMF_Scans_Exporter only works on UIMF Scan Results");
                UIMFScanResult uimfResult = (UIMFScanResult)result;

                IMS_Frames fp = new IMS_Frames();
                fp.frame_num = (ushort)uimfResult.Frameset.PrimaryFrame;
                fp.frame_time = (float)uimfResult.ScanTime;
                fp.type = (ushort)uimfResult.SpectrumType;
                fp.bpi = uimfResult.BasePeak.Intensity;
                fp.bpi_mz = (float)uimfResult.BasePeak.MZ;
                fp.tic = uimfResult.TICValue;
                fp.num_peaks = (uint)uimfResult.NumPeaks;
                fp.num_deisotoped = (uint)uimfResult.NumIsotopicProfiles;
                fp.frame_pressure_front = (float)uimfResult.FramePressureFront;
                fp.frame_pressure_back = (float)uimfResult.FramePressureBack;
                records.Add(fp);
            }
            sqliteWriter.InsertIMSFrames(records);
            sqliteWriter.CloseDB(fileName);
        }
    }
}
