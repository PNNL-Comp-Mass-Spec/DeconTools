using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Data.Common;
using DeconTools.Backend.Utilities;
using System.Data.SqlClient;
using DeconTools.Backend.DTO;
using DeconTools.Backend.Core;
using System.ComponentModel;

namespace DeconTools.Backend.Data
{
    public class PeakImporterFromSQLite : IPeakImporter
    {
        private string sqliteFilename;


        #region Constructors
        public PeakImporterFromSQLite(string sqliteFilename)
            : this(sqliteFilename, null)
        {

        }

        public PeakImporterFromSQLite(string sqliteFilename, BackgroundWorker bw)
        {
            this.backgroundWorker = bw;

            if (!File.Exists(sqliteFilename))
            {
                throw new System.IO.IOException("Peak import failed. File doesn't exist: " + DiagnosticUtilities.GetFullPathSafe(sqliteFilename));
            }
            this.sqliteFilename = sqliteFilename;
        }

        #endregion

        #region Properties
        #endregion

        #region Public Methods
        public override void ImportPeaks(List<MSPeakResult> peakList)
        {
            var fact = DbProviderFactories.GetFactory("System.Data.SQLite");
            var queryString = "SELECT peak_id, scan_num, mz, intensity, fwhm FROM T_Peaks;";

            using (var cnn = fact.CreateConnection())
            {
                cnn.ConnectionString = "Data Source=" + this.sqliteFilename;
                try
                {
                    cnn.Open();
                }
                catch (Exception ex)
                {
                    throw new Exception("Peak import failed. Couldn't connect to SQLite database. \n\nDetails: " + ex.Message);
                }

                using (var command = cnn.CreateCommand())
                {
                    command.CommandText = "SELECT COUNT(*) FROM T_Peaks;";
                    numRecords = Convert.ToInt32(command.ExecuteScalar());
                }

                using (var command = cnn.CreateCommand())
                {
                    command.CommandText = queryString;
                    var reader = command.ExecuteReader();


                    MSPeakResult peakresult;

                    var progressCounter = 0;
                    while (reader.Read())
                    {
                        peakresult = new MSPeakResult();

                        var test = (long)reader["peak_id"];

                        peakresult.PeakID = (int)(long)reader["peak_id"];
                        peakresult.Scan_num = (int)(long)reader["scan_num"];
                        peakresult.MSPeak = new MSPeak();
                        peakresult.MSPeak.XValue = (double)reader["mz"];
                        peakresult.MSPeak.Height = (float)(double)reader["intensity"];
                        peakresult.MSPeak.Width = (float)(double)reader["fwhm"];
                        peakList.Add(peakresult);

                        if (this.backgroundWorker != null)
                        {
                            if (backgroundWorker.CancellationPending)
                            {
                                return;
                            }
                        }

                        progressCounter++;
                        reportProgress(progressCounter);
                    }
                    reader.Close();
                }
            }
        }

 


        #endregion

        #region Private Methods
    

        #endregion



    }
}
