using System;
using System.Collections.Generic;
using System.Data.Common;
using DeconTools.Workflows.Backend.Results;

namespace DeconTools.Workflows.Backend.Utilities
{
    public class PeakMatchedResultUtilities
    {

        #region Constructors

        public PeakMatchedResultUtilities(string dbServer, string dbSource, string resultTableName)
        {
            DbServer = dbServer;
            DbSource = dbSource;
            ResultTableName = resultTableName;
        }


        #endregion

        #region Properties

        public string DbSource { get; set; }

        public string DbServer { get; set; }

        public string ResultTableName { get; set; }




        #endregion

        #region Public Methods

        public List<TargetedResultDTO> LoadResultsForDataset(string datasetName)
        {
            var importedResults = new List<TargetedResultDTO>();

            var fact = DbProviderFactories.GetFactory("System.Data.SqlClient");
            using (var cnn = fact.CreateConnection())
            {

                cnn.ConnectionString = buildConnectionString();
                cnn.Open();

                using (var command = cnn.CreateCommand())
                {
                    string queryString;

                    queryString =
                        @"SELECT * FROM " + ResultTableName + " where Dataset = '" +
                        datasetName + "'";

                    command.CommandText = queryString;
                    command.CommandTimeout = 60;
                    var reader = command.ExecuteReader();

                    ReadResultsFromDB(importedResults, reader);


                }


            }

            return importedResults;


        }

        private void ReadResultsFromDB(List<TargetedResultDTO> results, DbDataReader reader)
        {
            while (reader.Read())
            {
                var result = new TargetedResultDTO();

                result.DatasetName = readString(reader, "Dataset");
                result.TargetID = readInt(reader, "UMC_Ind");
                result.MatchedMassTagID = readInt(reader, "Mass_Tag_ID");


                result.FailureType = "";
                //result.FitScore = readFloat(reader,"


                result.IndexOfMostAbundantPeak = -1;

                result.ChargeState = readInt(reader, "Class_Stats_Charge_Basis");
                result.Intensity = readFloat(reader, "Class_Abundance");
                result.IntensityI0 = result.Intensity;
                result.IntensityMostAbundantPeak = result.Intensity;
                result.IScore = 0;
                result.MassErrorBeforeCalibration = readFloat(reader, "MassErrorPPM");

                result.MonoMass = readDouble(reader, "Class_Mass");
                result.MonoMassCalibrated = result.MonoMass;
                result.MonoMZ = result.MonoMass / result.ChargeState + 1.00727649;
                result.NET = readFloat(reader, "ElutionTime");
                result.NETError = readFloat(reader, "NET_Error");
                result.NumChromPeaksWithinTol = 1;
                result.NumQualityChromPeaksWithinTol = 1;
                result.ScanLC = readInt(reader, "Scan_Max_Abundance");
                result.ScanLCStart = readInt(reader, "Scan_First");
                result.ScanLCEnd = readInt(reader, "Scan_Last");

                results.Add(result);
            }
        }



        #endregion

        #region Private Methods

        private string buildConnectionString()
        {
            var builder = new System.Data.SqlClient.SqlConnectionStringBuilder();
            builder.UserID = "mtuser";
            builder.DataSource = DbServer;
            builder.Password = "mt4fun";
            builder.InitialCatalog = DbSource;
            builder.ConnectTimeout = 5;

            return builder.ConnectionString;
        }

        private double readDouble(DbDataReader reader, string columnName, double defaultVal = 0)
        {
            if (!reader[columnName].Equals(DBNull.Value))
            {
                return Convert.ToDouble(reader[columnName]);
            }
            else
            {
                return defaultVal;
            }
        }

        private float readFloat(DbDataReader reader, string columnName, float defaultVal = 0f)
        {
            if (!reader[columnName].Equals(DBNull.Value))
            {
                return Convert.ToSingle(reader[columnName]);
            }
            else
            {
                return defaultVal;
            }
        }

        private string readString(DbDataReader reader, string columnName, string defaultVal = "")
        {

            if (!reader[columnName].Equals(DBNull.Value))
            {
                return Convert.ToString(reader[columnName]);
            }
            else
            {
                return defaultVal;
            }
        }

        private int readInt(DbDataReader reader, string columnName, int defaultVal = 0)
        {

            if (!reader[columnName].Equals(DBNull.Value))
            {
                return Convert.ToInt32(reader[columnName]);
            }
            else
            {
                return defaultVal;
            }
        }


        #endregion

    }
}
