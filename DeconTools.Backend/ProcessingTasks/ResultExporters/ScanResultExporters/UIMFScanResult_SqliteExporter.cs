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

            DbProviderFactory fact = DbProviderFactories.GetFactory("System.Data.SQLite");
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
            DbCommand command = cnn.CreateCommand();

            command.CommandText = scanResultTable.BuildCreateTableString();
            command.ExecuteNonQuery();
        }

        protected override void addScanResults(DeconTools.Backend.Core.ResultCollection rc)
        {
            SQLiteConnection myconnection = (SQLiteConnection)cnn;

            using (SQLiteTransaction mytransaction = myconnection.BeginTransaction())
            {
                using (SQLiteCommand mycommand = new SQLiteCommand(myconnection))
                {
                    SQLiteParameter frameNumParam = new SQLiteParameter();
                    SQLiteParameter frameTimeParam = new SQLiteParameter();
                    SQLiteParameter typeParam = new SQLiteParameter();
                    SQLiteParameter bpiParam = new SQLiteParameter();
                    SQLiteParameter bpiMZParam = new SQLiteParameter();
                    SQLiteParameter ticParam = new SQLiteParameter();
                    SQLiteParameter num_peaksParam = new SQLiteParameter();
                    SQLiteParameter num_deisotopedParam = new SQLiteParameter();
                    SQLiteParameter framePressureUnsmoothed = new SQLiteParameter();
                    SQLiteParameter framePressureSmoothed = new SQLiteParameter();


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


                    for (int n = 0; n < rc.ScanResultList.Count; n++)
                    {
                        UimfScanResult r = (UimfScanResult)rc.ScanResultList[n];

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
