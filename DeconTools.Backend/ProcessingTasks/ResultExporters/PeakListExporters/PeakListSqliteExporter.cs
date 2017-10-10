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
using System.IO;

namespace DeconTools.Backend.ProcessingTasks.PeakListExporters
{
    public class PeakListSQLiteExporter : IPeakListExporter, IDisposable
    {

        private readonly DbConnection cnn;
        private readonly bool createIndexOnMZ;

        #region Constructors
        public PeakListSQLiteExporter(string sqliteFilename)
            : this(10000, sqliteFilename)
        {
        }

        public PeakListSQLiteExporter(int triggerValue, string sqliteFilename)
        {
            if (File.Exists(sqliteFilename)) File.Delete(sqliteFilename);

            var fact = DbProviderFactories.GetFactory("System.Data.SQLite");

            cnn = fact.CreateConnection();

            if (cnn == null)
                throw new Exception("Factory.CreateConnection returned a null DbConnection instance in PeakListSQLiteExporter constructor");

            cnn.ConnectionString = "Data Source=" + sqliteFilename;

            createIndexOnMZ = false;
            try
            {

                cnn.Open();

            }
            catch (Exception ex)
            {
                Logger.Instance.AddEntry("SqlitePeakListExporter failed. Details: " + ex.Message, Logger.Instance.OutputFilename);
                throw;
            }

            createMSPeakTable();

            this.TriggerToWriteValue = triggerValue;
        }

        private void createMSPeakTable()
        {
            Table mspeakTable = new MSPeakTable("T_Peaks");
            var command = cnn.CreateCommand();

            command.CommandText = mspeakTable.BuildCreateTableString();
            if (createIndexOnMZ)
            {
                command.CommandText += " CREATE INDEX mzIndex on T_Peaks (mz)";
            }


            command.ExecuteNonQuery();

        }
        #endregion

        #region Properties

        public override int[] MSLevelsToExport { get; set; }

        public override int TriggerToWriteValue { get; set; }

        #endregion

        #region Public Methods
        #endregion

        #region Private Methods
        #endregion

        /// <summary>
        /// Append the peaks to the database
        /// </summary>
        /// <param name="peakList">Peak list to write</param>
        public override void WriteOutPeaks(List<MSPeakResult> peakList)
        {
            WriteOutPeaks(peakList);
        }

        /// <summary>
        /// Append the peaks to the database
        /// </summary>
        /// <param name="sw">Ignored by this class</param>
        /// <param name="peakList">Peak list to write</param>
        public override void WriteOutPeaks(StreamWriter sw, List<MSPeakResult> peakList)
        {
            var myconnection = (SQLiteConnection)cnn;

            using (var mytransaction = myconnection.BeginTransaction())
            {
                using (var mycommand = new SQLiteCommand(myconnection))
                {
                    var peakIDParam = new SQLiteParameter();
                    var scanIDParam = new SQLiteParameter();
                    var mzParam = new SQLiteParameter();
                    var intensParam = new SQLiteParameter();
                    var fwhmParam = new SQLiteParameter();
                    var msfeatureParam = new SQLiteParameter();

                                       mycommand.CommandText = "INSERT INTO T_Peaks ([peak_id],[scan_num],[mz],[intensity],[fwhm],[msfeatureID]) VALUES(?,?,?,?,?,?)";
                    mycommand.Parameters.Add(peakIDParam);
                    mycommand.Parameters.Add(scanIDParam);
                    mycommand.Parameters.Add(mzParam);
                    mycommand.Parameters.Add(intensParam);
                    mycommand.Parameters.Add(fwhmParam);
                    mycommand.Parameters.Add(msfeatureParam);


                    foreach (var peak in peakList)
                    {
                        peakIDParam.Value = peak.PeakID;
                        scanIDParam.Value = peak.Scan_num;
                        mzParam.Value = peak.MSPeak.XValue.ToString("#.#####");
                        intensParam.Value = peak.MSPeak.Height;
                        fwhmParam.Value = peak.MSPeak.Width;
                        msfeatureParam.Value = peak.MSPeak.MSFeatureID;
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
   

            base.Cleanup();
        }



        #region IDisposable Members

        void IDisposable.Dispose()
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
        }

        #endregion
    }
}
