﻿using System;
using System.Data.Common;
using System.Globalization;

namespace DeconTools.Backend.Utilities
{
    public class UIMFCalibrationConstantUpdater
    {
        #region Public Methods

        public static bool UpdateUIMFFileWithTOFCorrectionTime(string filePath, float tofCorrectionTime)
        {
            var fact = DbProviderFactories.GetFactory("System.Data.SQLite");
            var cnn = fact.CreateConnection();

            if (cnn == null)
            {
                throw new Exception("Factory.CreateConnection returned a null DbConnection object in UpdateUIMFFileWithTOFCorrectionTime");
            }

            cnn.ConnectionString = "Data Source=" + filePath;

            try
            {
                cnn.Open();
            }
            catch (Exception ex)
            {
                Logger.Instance.AddEntry("SQLitePeakListExporter failed. Details: " + ex.Message, true);
                throw;
            }

            modifyTOFCorrectionTime(cnn, tofCorrectionTime);

            return true;
        }

        private static void modifyTOFCorrectionTime(DbConnection cnn, float tofCorrectionTime)
        {
            addTOFCorrectionTimeIfAbsent(cnn);

            using (var myTransaction = cnn.BeginTransaction())
            {
                using (var cmd = cnn.CreateCommand())
                {
                    cmd.CommandText = "UPDATE Global_Parameters SET TOFCorrectionTime = " + tofCorrectionTime.ToString(CultureInfo.InvariantCulture) + ";";
                    cmd.ExecuteNonQuery();
                }

                myTransaction.Commit();
            }
        }

        private static void addTOFCorrectionTimeIfAbsent(DbConnection cnn)
        {
            var columnNames = SqliteNETUtils.SqliteNETUtils.GetColumnNames(cnn, "Global_Parameters");
            var targetColumn = "TOFCorrectionTime";

            if (!columnNames.Contains(targetColumn))
            {
                addNewColumn(cnn, "Global_Parameters", targetColumn, "double");
            }
        }

        public static bool UpdateUIMFFileWithCalibrationConstants(string filePath, double calSlope, double calIntercept, double a2, double b2, double c2, double d2, double e2, double f2)
        {
            var fact = DbProviderFactories.GetFactory("System.Data.SQLite");
            var cnn = fact.CreateConnection();

            if (cnn == null)
            {
                throw new Exception("Factory.CreateConnection returned a null DbConnection object in UpdateUIMFFileWithCalibrationConstants");
            }

            cnn.ConnectionString = "Data Source=" + filePath;

            try
            {
                cnn.Open();
            }
            catch (Exception ex)
            {
                Logger.Instance.AddEntry("SQLitePeakListExporter failed. Details: " + ex.Message, true);
                throw;
            }

            add_a2b2c2d2e2f2ColumnsIfAbsent(cnn);

            modifySlopeAndIntercept(cnn, calSlope, calIntercept);

            modifyPolynomialConstants(cnn, a2, b2, c2, d2, e2, f2);

            return true;
        }

        private static void modifyPolynomialConstants(DbConnection cnn, double a2, double b2, double c2, double d2, double e2, double f2)
        {
            using (var myTransaction = cnn.BeginTransaction())
            {
                using (var cmd = cnn.CreateCommand())
                {
                    cmd.CommandText = "UPDATE Frame_Parameters SET a2 = " + a2.ToString(CultureInfo.InvariantCulture) + ";";
                    cmd.ExecuteNonQuery();

                    cmd.CommandText = "UPDATE Frame_Parameters SET b2 = " + b2.ToString(CultureInfo.InvariantCulture) + ";";
                    cmd.ExecuteNonQuery();

                    cmd.CommandText = "UPDATE Frame_Parameters SET c2 = " + c2.ToString(CultureInfo.InvariantCulture) + ";";
                    cmd.ExecuteNonQuery();

                    cmd.CommandText = "UPDATE Frame_Parameters SET d2 = " + d2.ToString(CultureInfo.InvariantCulture) + ";";
                    cmd.ExecuteNonQuery();

                    cmd.CommandText = "UPDATE Frame_Parameters SET e2 = " + e2.ToString(CultureInfo.InvariantCulture) + ";";
                    cmd.ExecuteNonQuery();

                    cmd.CommandText = "UPDATE Frame_Parameters SET f2 = " + f2.ToString(CultureInfo.InvariantCulture) + ";";
                    cmd.ExecuteNonQuery();
                }

                myTransaction.Commit();
            }
        }

        private static void modifySlopeAndIntercept(DbConnection cnn, double calSlope, double calIntercept)
        {
            using (var myTransaction = cnn.BeginTransaction())
            {
                using (var cmd = cnn.CreateCommand())
                {
                    cmd.CommandText = "UPDATE Frame_Parameters SET CalibrationSlope = " + calSlope.ToString(CultureInfo.InvariantCulture) + ";";
                    cmd.ExecuteNonQuery();

                    cmd.CommandText = "UPDATE Frame_Parameters SET CalibrationIntercept = " + calIntercept.ToString(CultureInfo.InvariantCulture) + ";";
                    cmd.ExecuteNonQuery();
                }

                myTransaction.Commit();
            }
        }

        private static void add_a2b2c2d2e2f2ColumnsIfAbsent(DbConnection cnn)
        {
            var columnNames = SqliteNETUtils.SqliteNETUtils.GetColumnNames(cnn, "Frame_Parameters");
            string[] targetColumns = { "a2", "b2", "c2", "d2", "e2", "f2" };

            foreach (var col in targetColumns)
            {
                if (!columnNames.Contains(col))
                {
                    addNewColumn(cnn, "Frame_Parameters", col, "double");
                }
            }
        }

        private static void addNewColumn(DbConnection cnn, string tableName, string col, string variableType)
        {
            using (var myTransaction = cnn.BeginTransaction())
            {
                using (var cmd = cnn.CreateCommand())
                {
                    cmd.CommandText = "ALTER TABLE " + tableName + " ADD COLUMN " + col + " " + variableType + ";";
                    cmd.ExecuteNonQuery();
                }

                myTransaction.Commit();
            }
        }

        #endregion

        #region Private Methods
        #endregion
    }
}
