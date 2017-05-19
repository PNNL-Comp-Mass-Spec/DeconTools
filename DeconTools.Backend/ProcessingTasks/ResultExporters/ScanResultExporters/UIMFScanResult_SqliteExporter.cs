using System;
using System.Data.Common;
using System.Data.SQLite;
using System.IO;
using DeconTools.Backend.Core;
using DeconTools.Backend.Utilities;
using DeconTools.Utilities.SqliteUtils;

namespace DeconTools.Backend.ProcessingTasks.ResultExporters.ScanResultExporters
{
    public class UIMFScanResult_SqliteExporter:ScanResult_SqliteExporter
    {
        #region Constructors
        public UIMFScanResult_SqliteExporter(string fileName)
        {
            if (File.Exists(fileName)) File.Delete(fileName);

            var fact = DbProviderFactories.GetFactory("System.Data.SQLite");
            this.cnn = fact.CreateConnection();
            cnn.ConnectionString = "Data Source=" + fileName;

            try
            {
                cnn.Open();
            }
            catch (Exception ex)
            {
                Logger.Instance.AddEntry("SqlitePeakListExporter failed. Details: " + ex.Message, Logger.Instance.OutputFilename);
                throw;
            }

            buildTables();
        }
        #endregion

        #region Properties
        #endregion

        #region Public Methods
        #endregion

        #region Private Methods
        #endregion
        protected override void buildTables()
        {
            Table scanResultTable = new UIMFScanResult_SqliteTable("T_IMS_Frames");
            var command = cnn.CreateCommand();

            command.CommandText = scanResultTable.BuildCreateTableString();
            command.ExecuteNonQuery();
        }

        protected override void addScanResults(DeconTools.Backend.Core.ResultCollection rc)
        {
            var myconnection = (SQLiteConnection)cnn;

            using (var mytransaction = myconnection.BeginTransaction())
            {
                using (var mycommand = new SQLiteCommand(myconnection))
                {
                    var frameNumParam = new SQLiteParameter();
                    var frameTimeParam = new SQLiteParameter();
                    var typeParam = new SQLiteParameter();
                    var bpiParam = new SQLiteParameter();
                    var bpiMZParam = new SQLiteParameter();
                    var ticParam = new SQLiteParameter();
                    var num_peaksParam = new SQLiteParameter();
                    var num_deisotopedParam = new SQLiteParameter();
                    var framePressureUnsmoothed = new SQLiteParameter();
                    var framePressureSmoothed = new SQLiteParameter();


                    mycommand.CommandText = "INSERT INTO T_IMS_Frames ([frame_num],[frame_time],[type],[bpi],[bpi_mz],[tic],[num_peaks],[num_deisotoped],[frame_pressure_unsmoothed],[frame_pressure_smoothed]) VALUES(?,?,?,?,?,?,?,?,?,?)";
                    mycommand.Parameters.Add(frameNumParam);
                    mycommand.Parameters.Add(frameTimeParam);
                    mycommand.Parameters.Add(typeParam);
                    mycommand.Parameters.Add(bpiParam);
                    mycommand.Parameters.Add(bpiMZParam);
                    mycommand.Parameters.Add(ticParam);
                    mycommand.Parameters.Add(num_peaksParam);
                    mycommand.Parameters.Add(num_deisotopedParam);
                    mycommand.Parameters.Add(framePressureUnsmoothed);
                    mycommand.Parameters.Add(framePressureSmoothed);


                    for (var n = 0; n < rc.ScanResultList.Count; n++)
                    {
                        var r = (UimfScanResult)rc.ScanResultList[n];

                        frameNumParam.Value = r.LCScanNum;
                        frameTimeParam.Value = r.ScanTime;
                        typeParam.Value = r.SpectrumType;
                        bpiParam.Value = r.BasePeak.Height;
                        bpiMZParam.Value = r.BasePeak.XValue;
                        ticParam.Value = r.TICValue;
                        num_peaksParam.Value = r.NumPeaks;
                        num_deisotopedParam.Value = r.NumIsotopicProfiles;
                        framePressureUnsmoothed.Value = r.FramePressureUnsmoothed;
                        framePressureSmoothed.Value = r.FramePressureSmoothed;

                        mycommand.ExecuteNonQuery();
                    }
                }
                mytransaction.Commit();

            }
        }
    }
}
