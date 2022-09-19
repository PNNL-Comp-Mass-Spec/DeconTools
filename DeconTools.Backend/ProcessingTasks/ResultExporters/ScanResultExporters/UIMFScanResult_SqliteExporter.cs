using System;
using System.Data.Common;
using System.Data.SQLite;
using System.IO;
using DeconTools.Backend.Core;
using DeconTools.Backend.Utilities;
using DeconTools.Utilities.SqliteUtils;

namespace DeconTools.Backend.ProcessingTasks.ResultExporters.ScanResultExporters
{
    public sealed class UIMFScanResult_SqliteExporter : ScanResult_SqliteExporter
    {
        #region Constructors
        public UIMFScanResult_SqliteExporter(string fileName)
        {
            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }

            var fact = DbProviderFactories.GetFactory("System.Data.SQLite");
            cnn = fact.CreateConnection();

            if (cnn == null)
            {
                throw new Exception("Factory.CreateConnection returned a null DbConnection instance in UIMFScanResult_SqliteExporter constructor");
            }

            cnn.ConnectionString = "Data Source=" + fileName;

            try
            {
                cnn.Open();
            }
            catch (Exception ex)
            {
                Logger.Instance.AddEntry("SqlitePeakListExporter failed. Details: " + ex.Message, true);
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

        protected override void addScanResults(ResultCollection rc)
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

                    foreach (var scanResult in rc.ScanResultList)
                    {
                        var uimfResult = (UimfScanResult)scanResult;

                        frameNumParam.Value = uimfResult.LCScanNum;
                        frameTimeParam.Value = uimfResult.ScanTime;
                        typeParam.Value = uimfResult.SpectrumType;
                        bpiParam.Value = uimfResult.BasePeak.Height;
                        bpiMZParam.Value = uimfResult.BasePeak.XValue;
                        ticParam.Value = uimfResult.TICValue;
                        num_peaksParam.Value = uimfResult.NumPeaks;
                        num_deisotopedParam.Value = uimfResult.NumIsotopicProfiles;
                        framePressureUnsmoothed.Value = uimfResult.FramePressureUnsmoothed;
                        framePressureSmoothed.Value = uimfResult.FramePressureSmoothed;

                        mycommand.ExecuteNonQuery();
                    }
                }
                mytransaction.Commit();
            }
        }
    }
}
