using System;
using System.Data.Common;
using System.Data.SQLite;
using System.IO;
using DeconTools.Backend.Utilities;
using DeconTools.Utilities.SqliteUtils;

namespace DeconTools.Backend.ProcessingTasks.ResultExporters.ScanResultExporters
{
    public sealed class BasicScanResult_SqliteExporter : ScanResult_SqliteExporter
    {

        #region Constructors
        public BasicScanResult_SqliteExporter(string fileName)
        {
            if (File.Exists(fileName)) File.Delete(fileName);

            var fact = DbProviderFactories.GetFactory("System.Data.SQLite");
            cnn = fact.CreateConnection();

            if (cnn == null)
                throw new Exception("Factory.CreateConnection returned a null DbConnection instance in BasicScanResult_SqliteExporter");

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
            Table scanResultTable = new BasicScanResult_SqliteTable("T_MS_ScanSummary");
            var command = cnn.CreateCommand();

            command.CommandText = scanResultTable.BuildCreateTableString();
            command.ExecuteNonQuery();
        }

        protected override void addScanResults(Core.ResultCollection rc)
        {
            var myconnection = (SQLiteConnection)cnn;

            using (var mytransaction = myconnection.BeginTransaction())
            {
                using (var mycommand = new SQLiteCommand(myconnection))
                {
                    var scanNumParam = new SQLiteParameter();
                    var scanTimeParam = new SQLiteParameter();
                    var typeParam = new SQLiteParameter();
                    var bpiParam = new SQLiteParameter();
                    var bpiMZParam = new SQLiteParameter();
                    var ticParam = new SQLiteParameter();
                    var num_peaksParam = new SQLiteParameter();
                    var num_deisotopedParam = new SQLiteParameter();

                    mycommand.CommandText = "INSERT INTO T_MS_ScanSummary ([scan_num],[scan_time],[type],[bpi],[bpi_mz],[tic],[num_peaks],[num_deisotoped]) VALUES(?,?,?,?,?,?,?,?)";
                    mycommand.Parameters.Add(scanNumParam);
                    mycommand.Parameters.Add(scanTimeParam);
                    mycommand.Parameters.Add(typeParam);
                    mycommand.Parameters.Add(bpiParam);
                    mycommand.Parameters.Add(bpiMZParam);
                    mycommand.Parameters.Add(ticParam);
                    mycommand.Parameters.Add(num_peaksParam);
                    mycommand.Parameters.Add(num_deisotopedParam);


                    foreach (var item in rc.ScanResultList)
                    {
                        scanNumParam.Value = item.ScanSet.PrimaryScanNumber;
                        scanTimeParam.Value = item.ScanTime;
                        typeParam.Value = item.SpectrumType;
                        bpiParam.Value = item.BasePeak.Height;
                        bpiMZParam.Value = item.BasePeak.XValue;
                        ticParam.Value = item.TICValue;
                        num_peaksParam.Value = item.NumPeaks;
                        num_deisotopedParam.Value = item.NumIsotopicProfiles;

                        mycommand.ExecuteNonQuery();
                    }
                }
                mytransaction.Commit();

            }

        }




    }
}
