using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core;
using DeconTools.Backend.DTO;
using DeconTools.Backend.Runs;
using System.Data.Common;
using System.Data.SQLite;
using DeconTools.Utilities.SqliteUtils;
using DeconTools.Backend.Utilities;

namespace DeconTools.Backend.ProcessingTasks.PeakListExporters
{
    public class SqlitePeakListExporter : IPeakListExporter
    {


        private DbConnection cnn;
        private bool createIndexOnMZ;


        #region Constructors
        public SqlitePeakListExporter(string sqliteFilename)
            : this(sqliteFilename, 10000)
        {
        }

        public SqlitePeakListExporter(string sqliteFilename, int triggerValue)
        {

            DbProviderFactory fact = DbProviderFactories.GetFactory("System.Data.SQLite");

            cnn = fact.CreateConnection();
            cnn.ConnectionString = "Data Source=" + sqliteFilename;

            createIndexOnMZ = false;


            try
            {

                cnn.Open();

            }
            catch (Exception ex)
            {
                Logger.Instance.AddEntry("SqlitePeakListExporter failed. Details: " + ex.Message, Logger.Instance.OutputFilename);
                throw ex;
            }

            createMSPeakTable();

            this.TriggerToWriteValue = triggerValue;
        }

        private void createMSPeakTable()
        {
            Table mspeakTable = new MSPeakTable("T_Peaks");
            DbCommand command = cnn.CreateCommand();

            command.CommandText = mspeakTable.BuildCreateTableString();
            if (createIndexOnMZ)
            {
                command.CommandText += " CREATE INDEX mzIndex on T_Peaks (mz)";
            }


            command.ExecuteNonQuery();

        }
        #endregion

        #region Properties
        private int[] mSLevelsToExport;
        public override int[] MSLevelsToExport
        {
            get { return mSLevelsToExport; }
            set { mSLevelsToExport = value; }
        }

        private int triggerValue;
        public override int TriggerToWriteValue
        {
            get { return triggerValue; }
            set { triggerValue = value; }
        }
        #endregion

        #region Public Methods
        #endregion

        #region Private Methods
        #endregion


        public override void WriteOutPeaks(DeconTools.Backend.Core.ResultCollection resultList)
        {
            SQLiteConnection myconnection = (SQLiteConnection)cnn;

            using (SQLiteTransaction mytransaction = myconnection.BeginTransaction())
            {
                using (SQLiteCommand mycommand = new SQLiteCommand(myconnection))
                {
                    SQLiteParameter peakIDParam = new SQLiteParameter();
                    SQLiteParameter scanIDParam = new SQLiteParameter();
                    SQLiteParameter mzParam = new SQLiteParameter();
                    SQLiteParameter intensParam = new SQLiteParameter();
                    SQLiteParameter fwhmParam = new SQLiteParameter();

                    int n;

                    mycommand.CommandText = "INSERT INTO T_Peaks ([peak_id],[scan_num],[mz],[intensity],[fwhm]) VALUES(?,?,?,?,?)";
                    mycommand.Parameters.Add(peakIDParam);
                    mycommand.Parameters.Add(scanIDParam);
                    mycommand.Parameters.Add(mzParam);
                    mycommand.Parameters.Add(intensParam);
                    mycommand.Parameters.Add(fwhmParam);

                    for (n = 0; n < resultList.MSPeakResultList.Count; n++)
                    {
                        peakIDParam.Value = resultList.MSPeakResultList[n].PeakID;
                        scanIDParam.Value = resultList.MSPeakResultList[n].Scan_num;
                        mzParam.Value = resultList.MSPeakResultList[n].MSPeak.MZ;
                        intensParam.Value = resultList.MSPeakResultList[n].MSPeak.Intensity;
                        fwhmParam.Value = resultList.MSPeakResultList[n].MSPeak.FWHM;

                        mycommand.ExecuteNonQuery();
                    }
                }
                mytransaction.Commit();


            }

            //results = resultList;

            //List<MSPeakResult> resultBuffer = new List<MSPeakResult>();
            //for (int i = 0; i < resultList.MSPeakResultList.Count; i++)
            //{
            //    resultBuffer.Add(resultList.MSPeakResultList[i]);

            //    if (resultBuffer.Count > sqliteBufferThreshold)
            //    {
            //        InsertDataIntoTable(resultBuffer);
            //        resultBuffer.Clear();
            //    }
            //}
            //if (resultBuffer.Count > 0) InsertDataIntoTable(resultBuffer);   //clear out remaining

        }

        //private string buildInsertionString(List<MSPeakResult> resultList)
        //{
        //    StringBuilder sb = new StringBuilder();
        //    string delim = ",";

        //    for (int i = 0; i < resultList.Count; i++)
        //    {
        //        MSPeakResult pr = resultList[i];

        //        sb.Append(" SELECT ");
        //        sb.Append(pr.PeakID);
        //        sb.Append(delim);
        //        if (results.Run is UIMFRun)
        //        {
        //            sb.Append(resultList[i].Frame_num);
        //            sb.Append(delim);
        //        }
        //        sb.Append(pr.Scan_num);
        //        sb.Append(delim);
        //        sb.Append(pr.MSPeak.MZ);
        //        sb.Append(delim);
        //        sb.Append(pr.MSPeak.Intensity);
        //        sb.Append(delim);
        //        sb.Append(pr.MSPeak.FWHM);

        //        if (i < resultList.Count - 1)   //if not the last element
        //        {
        //            sb.Append(" UNION ALL");
        //        }
        //        else        //if the last one...
        //        {
        //            sb.Append(";");
        //        }
        //    }
        //    return sb.ToString();


        //}

        public override void Cleanup()
        {
            if (cnn != null)
            {

                try
                {
                    if (cnn.State != System.Data.ConnectionState.Closed)
                    {
                        cnn.Close();
                    }
                    cnn.Dispose();

                }
                catch (Exception)
                {
                    Console.WriteLine("************** there was a problem closing the Sqlite database *****************");

                }
            }


            base.Cleanup();
        }


    }
}
