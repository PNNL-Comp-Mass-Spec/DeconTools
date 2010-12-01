using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Utilities;
using DeconTools.Utilities.SqliteUtils;
using System.IO;
using System.Data.Common;

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
            Check.Assert(this.FileName != null && this.FileName.Length > 0, this.Name + " failed. Illegal filename.");
            Check.Assert(m_dbConnection != null, String.Format("{0} failed. No connection was made to a database", this.Name));
            Check.Assert(m_dbConnection.State == System.Data.ConnectionState.Open, String.Format("{0} failed. Connection to database is not open", this.Name));


            using (DbTransaction myTransaction = m_dbConnection.BeginTransaction())
            {
                using (DbCommand myCommand = m_dbConnection.CreateCommand())
                {
                    createParameterList(myCommand);

                    myCommand.CommandText = createInsertionCommandString();
                    Console.WriteLine(myCommand.CommandText);
                    foreach (T result in resultList)
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

            if (File.Exists(FileName)) File.Delete(FileName);

            DbProviderFactory fact = DbProviderFactories.GetFactory("System.Data.SQLite");
            this.m_dbConnection = fact.CreateConnection();
            m_dbConnection.ConnectionString = "Data Source=" + FileName;

            try
            {
                m_dbConnection.Open();
            }
            catch (Exception ex)
            {
                //Logger.Instance.AddEntry("SqlitePeakListExporter failed. Details: " + ex.Message, Logger.Instance.OutputFilename);
                throw ex;
            }

            buildTable();

        }

        protected virtual void buildTable()
        {
            DbCommand command = m_dbConnection.CreateCommand();
            command.CommandText = buildCreateTableSQLiteCommandString();
            command.ExecuteNonQuery();
        }

        protected abstract List<Field> CreateFieldList();


        private string createInsertionCommandString()
        {
            //for example..."INSERT INTO T_MSFeatures ([feature_id],[scan_num],[charge],[abundance],[mz],[fit],[average_mw],[monoisotopic_mw],[mostabundant_mw],[fwhm],[signal_noise],[mono_abundance],[mono_plus2_abundance],[flag]) VALUES(?,?,?,?,?,?,?,?,?,?,?,?,?,?)";

            StringBuilder sb = new StringBuilder();
            sb.Append("INSERT INTO ");
            sb.Append(this.TableName);
            sb.Append(" (");

            foreach (Field fieldItem in FieldList)
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
            foreach (Field fieldItem in FieldList)
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

        protected virtual string buildCreateTableSQLiteCommandString()
        {
            Check.Assert(this.TableName != null && this.TableName.Length > 0, String.Format("SQLite TableName has not been declared within {0}.", this.Name));
            Check.Assert(this.FieldList != null && this.FieldList.Count > 0, String.Format("SQLite Table fields have not been declared within {0}.", this.Name));

            StringBuilder sb = new StringBuilder();
            sb.Append("CREATE TABLE ");
            sb.Append(this.TableName);
            sb.Append(" (");

            foreach (Field fieldItem in FieldList)
            {
                sb.Append(fieldItem.ToString());
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

        private void createParameterList(DbCommand cmd)
        {
            foreach (var field in FieldList)
            {
                DbParameter param = cmd.CreateParameter();
                cmd.Parameters.Add(param);
            }

        }

        #endregion
    }
}
