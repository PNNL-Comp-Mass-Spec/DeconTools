using System;
using System.Data.Common;
using System.Data.SQLite;
using System.IO;
using DeconTools.Backend.Core;
using DeconTools.Backend.Utilities;
using DeconTools.Utilities.SqliteUtils;

namespace DeconTools.Backend.ProcessingTasks.ResultExporters.ScanResultExporters
{
    public class BasicScanResult_SqliteExporter:ScanResult_SqliteExporter
    {

        #region Constructors
        public BasicScanResult_SqliteExporter(string fileName)
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
                throw ex;
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
                    SQLiteParameter scanNumParam = new SQLiteParameter();
                    SQLiteParameter scanTimeParam = new SQLiteParameter();
                    SQLiteParameter typeParam = new SQLiteParameter();
                    SQLiteParameter bpiParam = new SQLiteParameter();
                    SQLiteParameter bpiMZParam = new SQLiteParameter();
                    SQLiteParameter ticParam = new SQLiteParameter();
                    SQLiteParameter num_peaksParam = new SQLiteParameter();
                    SQLiteParameter num_deisotopedParam = new SQLiteParameter();

                    mycommand.CommandText = "INSERT INTO T_MS_ScanSummary ([scan_num],[scan_time],[type],[bpi],[bpi_mz],[tic],[num_peaks],[num_deisotoped]) VALUES(?,?,?,?,?,?,?,?)";
                    mycommand.Parameters.Add(scanNumParam);
                    mycommand.Parameters.Add(scanTimeParam);
                    mycommand.Parameters.Add(typeParam);
                    mycommand.Parameters.Add(bpiParam);
                    mycommand.Parameters.Add(bpiMZParam);
                    mycommand.Parameters.Add(ticParam);
                    mycommand.Parameters.Add(num_peaksParam);
                    mycommand.Parameters.Add(num_deisotopedParam);

                    
                    for (int n = 0; n < rc.ScanResultList.Count; n++)
                    {
                        ScanResult item = rc.ScanResultList[n];

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
