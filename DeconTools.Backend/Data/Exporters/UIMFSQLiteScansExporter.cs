using System;
using System.Collections;
using DeconTools.Backend.Core;
using DeconTools.Utilities;
using MSDBLibrary;

namespace DeconTools.Backend.Data
{
    public class UIMFSQLiteScansExporter : ScansExporter
    {
        private readonly string fileName;
        private readonly DBWriter sqLiteWriter = new DBWriter();

        public UIMFSQLiteScansExporter(string fileName)
        {
            this.fileName = fileName;
        }

        protected override string headerLine { get; set; }
        protected override char delimiter { get; set; }

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

            //Insert records to IMS_Frames table
            //Insert records in bulk mood, 500 records each time
            //this is significantly faster than inserting one record at a time
            //500 records are the maximum number SQLite3 can handle
            var records = new ArrayList();
            var count = 0;
            foreach (var result in results.ScanResultList)
            {
                Check.Require(result is UimfScanResult, "UIMF_Scans_Exporter only works on UIMF Scan Results");
                var uimfResult = (UimfScanResult)result;

                var fp = new IMS_Frames
                {
                    frame_num = (ushort)uimfResult.ScanSet.PrimaryScanNumber,
                    frame_time = (float)uimfResult.ScanTime,
                    type = (ushort)uimfResult.SpectrumType,
                    bpi = uimfResult.BasePeak.Height,
                    bpi_mz = (float)uimfResult.BasePeak.XValue,
                    tic = uimfResult.TICValue,
                    num_peaks = (uint)uimfResult.NumPeaks,
                    num_deisotoped = (uint)uimfResult.NumIsotopicProfiles,
                    frame_pressure_front = (float)uimfResult.FramePressureUnsmoothed,
                    frame_pressure_back = (float)uimfResult.FramePressureSmoothed
                };
                records.Add(fp);
                count++;
                if (count == 500)
                {
                    sqLiteWriter.InsertIMSFrames(records);
                    count = 0;
                    records = new ArrayList();
                }
            }

            //Insert the rest of the records to IMS_Frames table
            sqLiteWriter.InsertIMSFrames(records);
            sqLiteWriter.CloseDB(fileName);
        }
    }
}
