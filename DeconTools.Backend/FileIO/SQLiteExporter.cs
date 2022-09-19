using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Text;
using DeconTools.Utilities;
using DeconTools.Utilities.SqliteUtils;
using System.IO;
using System.Data.Common;
using DeconTools.Backend.Utilities;

namespace DeconTools.Backend.FileIO
{
    public abstract class SQLiteExporter<T> : ExporterBase<T>
    {
        protected DbConnection m_dbConnection;

        #region Constructors
        #endregion

        #region Properties
        /// <summary>
        /// Name of the Exporter - e.g. 'ScanResultExporter'; to be used in error reporting, etc.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Full file path to which data is written
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Name of the SQLite Table into which records will be written;  eg 'MSFeatures'
        /// </summary>
        public abstract string TableName { get; }

        /// <summary>
        /// List of SQLite Fields that comprise the Table
        /// </summary>
        public abstract List<Field> FieldList { get; }

        #endregion

        #region Public Methods
        public override void ExportResults(IEnumerable<T> resultList)
        {
            Check.Assert(!string.IsNullOrEmpty(FileName), Name + " failed. Illegal filename.");
            Check.Assert(m_dbConnection != null, string.Format("{0} failed. No connection was made to a database", Name));
            if (m_dbConnection == null)
            {
                return;
            }

            Check.Assert(m_dbConnection.State == System.Data.ConnectionState.Open, string.Format("{0} failed. Connection to database is not open", Name));

            using (var myTransaction = m_dbConnection.BeginTransaction())
            {
                using (var myCommand = m_dbConnection.CreateCommand())
                {
                    CreateParameterList(myCommand);

                    myCommand.CommandText = CreateInsertionCommandString();
                    Console.WriteLine(myCommand.CommandText);
                    foreach (var result in resultList)
                    {
                        AddResults(myCommand.Parameters, result);
                        myCommand.ExecuteNonQuery();
                    }
                }

                myTransaction.Commit();
            }
        }
        #endregion

        #region Private Methods
        protected virtual void InitializeAndBuildTable()
        {
            if (File.Exists(FileName))
            {
                File.Delete(FileName);
            }

            DbProviderFactory fact = new SQLiteFactory();
            m_dbConnection = fact.CreateConnection();

            if (m_dbConnection == null)
            {
                return;
            }

            m_dbConnection.ConnectionString = "Data Source=" + FileName;

            try
            {
                m_dbConnection.Open();
            }
            catch (Exception ex)
            {
                Logger.Instance.AddEntry("SQLitePeakListExporter failed. Details: " + ex.Message, true);
                throw;
            }

            BuildTable();
        }

        protected virtual void BuildTable()
        {
            var command = m_dbConnection.CreateCommand();
            command.CommandText = BuildCreateTableSQLiteCommandString();
            command.ExecuteNonQuery();
        }

        protected abstract List<Field> CreateFieldList();

        private string CreateInsertionCommandString()
        {
            //for example..."INSERT INTO T_MSFeatures ([feature_id],[scan_num],[charge],[abundance],[mz],[fit],[average_mw],[monoisotopic_mw],[mostabundant_mw],[fwhm],[signal_noise],[mono_abundance],[mono_plus2_abundance],[flag]) VALUES(?,?,?,?,?,?,?,?,?,?,?,?,?,?)";

            var sb = new StringBuilder();
            sb.Append("INSERT INTO ");
            sb.Append(TableName);
            sb.Append(" (");

            foreach (var fieldItem in FieldList)
            {
                sb.Append(fieldItem.Name);
                if (fieldItem == FieldList[FieldList.Count - 1])  //if last one...
                {
                    sb.Append(")");
                }
                else   //not last
                {
                    sb.Append(", ");
                }
            }

            sb.Append("VALUES(");
            foreach (var fieldItem in FieldList)
            {
                sb.Append("?");
                if (fieldItem == FieldList[FieldList.Count - 1])  //if last one...
                {
                    sb.Append(")");
                }
                else   //not last
                {
                    sb.Append(", ");
                }
            }

            return sb.ToString();
        }

        protected abstract void AddResults(DbParameterCollection dbParameters, T result);

        protected virtual string BuildCreateTableSQLiteCommandString()
        {
            Check.Assert(!string.IsNullOrEmpty(TableName), string.Format("SQLite TableName has not been declared within {0}.", Name));
            Check.Assert(FieldList?.Count > 0, string.Format("SQLite Table fields have not been declared within {0}.", Name));

            if (FieldList == null)
            {
                return "";
            }

            var sb = new StringBuilder();
            sb.Append("CREATE TABLE ");
            sb.Append(TableName);
            sb.Append(" (");

            foreach (var fieldItem in FieldList)
            {
                sb.Append(fieldItem);
                if (fieldItem == FieldList[FieldList.Count - 1])  //if last one...
                {
                    sb.Append(");");
                }
                else   //not last
                {
                    sb.Append(", ");
                }
            }

            return sb.ToString();
        }

        private void CreateParameterList(DbCommand cmd)
        {
            foreach (var unused in FieldList)
            {
                var param = cmd.CreateParameter();
                cmd.Parameters.Add(param);
            }
        }

        #endregion
    }
}
