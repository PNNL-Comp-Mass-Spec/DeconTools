using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.IO;
using DeconTools.Backend.Core;
using DeconTools.Backend.DTO;
using DeconTools.Backend.Utilities;

namespace DeconTools.Backend.Data
{
    public class PeakImporterFromSQLite : IPeakImporter
    {
        private readonly string sqliteFilename;

        #region Constructors
        public PeakImporterFromSQLite(string sqliteFilename)
            : this(sqliteFilename, null)
        {

        }

        public PeakImporterFromSQLite(string sqliteFilename, BackgroundWorker bw)
        {
            backgroundWorker = bw;

            if (!File.Exists(sqliteFilename))
            {
                throw new IOException("Peak import failed. File doesn't exist: " + DiagnosticUtilities.GetFullPathSafe(sqliteFilename));
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
                if (cnn == null)
                    throw new Exception("Factory.CreateConnection returned a null DbConnection instance in ImportPeaks");

                cnn.ConnectionString = "Data Source=" + sqliteFilename;

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

                    var progressCounter = 0;
                    var lastReportProgress = DateTime.UtcNow;
                    var lastReportProgressConsole = DateTime.UtcNow;

                    while (reader.Read())
                    {
                        var peakresult = new MSPeakResult
                        {
                            PeakID = (int)(long)reader["peak_id"],
                            Scan_num = (int)(long)reader["scan_num"]
                        };

                        var mz = (double)reader["mz"];
                        var intensity = (float)(double)reader["intensity"];
                        var fwhm = (float)(double)reader["fwhm"];

                        peakresult.MSPeak = new MSPeak(mz, intensity, fwhm);
                        peakList.Add(peakresult);

                        if (backgroundWorker != null)
                        {
                            if (backgroundWorker.CancellationPending)
                            {
                                return;
                            }
                        }

                        progressCounter++;
                        reportProgress(progressCounter, ref lastReportProgress, ref lastReportProgressConsole);
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
