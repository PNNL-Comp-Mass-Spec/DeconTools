using System;
using System.Data.Common;

namespace DeconTools.Workflows.Backend.Utilities
{
    public class DatasetUtilities
    {

        public string DbUsername { get; set; }
        public string DbServerName { get; set; }
        public string DbUserPassWord { get; set; }
        public string DbName { get; set; }



        #region Constructors
        public DatasetUtilities()
        {
            this.DbUsername = "DMSReader";
            this.DbUserPassWord = "dms4fun";
            this.DbServerName = "Gigasax";
            this.DbName = "DMS5";

        }

        #endregion

        #region Properties


       

        public string GetDatasetPath(string datasetName)
        {
            DbProviderFactory fact = DbProviderFactories.GetFactory("System.Data.SqlClient");

            string datasetPath = "";

            using (DbConnection cnn = fact.CreateConnection())
            {
                string query =

                cnn.ConnectionString = buildConnectionString();
                cnn.Open();

                using (DbCommand command = cnn.CreateCommand())
                {
                    string queryString = "SELECT [Experiment],[Dataset],[Comment],[Rating],[Folder Name],[Dataset Folder Path],[Archive Folder Path] FROM [V_Dataset_Detail_Report_Ex] where (Dataset = '" + datasetName + "')";

                    command.CommandText = queryString;
                    command.CommandTimeout = 60;
                    DbDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {

                        if (!reader["Dataset Folder Path"].Equals(DBNull.Value))
                        {
                            datasetPath = Convert.ToString(reader["Dataset Folder Path"]);
                            break;
                        }


                    }
                }
            }

            return datasetPath;


        }

        public string GetDatasetID(string datasetName)
        {
            DbProviderFactory fact = DbProviderFactories.GetFactory("System.Data.SqlClient");

            string datasetPath = "";

            using (DbConnection cnn = fact.CreateConnection())
            {
                string query =

                cnn.ConnectionString = buildConnectionString();
                cnn.Open();

                using (DbCommand command = cnn.CreateCommand())
                {
                    string queryString = "SELECT [Experiment],[Dataset],[Comment],[Rating],[Folder Name],[Dataset Folder Path],[Archive Folder Path] FROM [V_Dataset_Detail_Report_Ex] where (Dataset = '" + datasetName + "')";

                    command.CommandText = queryString;
                    command.CommandTimeout = 60;
                    DbDataReader reader = command.ExecuteReader();

                   
                    while (reader.Read())
                    {

                        if (!reader["Dataset Folder Path"].Equals(DBNull.Value))
                        {
                            datasetPath = Convert.ToString(reader["Dataset Folder Path"]);
                            break;
                        }


                    }
                }
            }

            return datasetPath;


        }


        public string GetDatasetPathArchived(string datasetName)
        {
            DbProviderFactory fact = DbProviderFactories.GetFactory("System.Data.SqlClient");

            string datasetPath = "";

            using (DbConnection cnn = fact.CreateConnection())
            {
                string query = 

                cnn.ConnectionString = buildConnectionString();
                cnn.Open();

                using (DbCommand command = cnn.CreateCommand())
                {
                    string queryString = "SELECT [Experiment],[Dataset],[Comment],[Rating],[Folder Name],[Dataset Folder Path],[Archive Folder Path] FROM [V_Dataset_Detail_Report_Ex] where (Dataset = '" + datasetName + "')";

                    command.CommandText = queryString;
                    command.CommandTimeout = 60;
                    DbDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {

                        if (!reader["Archive Folder Path"].Equals(DBNull.Value))
                        {
                            datasetPath = Convert.ToString(reader["Archive Folder Path"]);
                            break;
                        }
                      

                    }
                }
            }

            return datasetPath;


        }



        #endregion

     
        #region Private Methods
        private string buildConnectionString()
        {
            System.Data.SqlClient.SqlConnectionStringBuilder builder = new System.Data.SqlClient.SqlConnectionStringBuilder();
            builder.UserID = DbUsername;
            builder.DataSource = DbServerName;
            builder.Password = DbUserPassWord;
            builder.InitialCatalog = DbName;
            builder.ConnectTimeout = 5;

            return builder.ConnectionString;
        }
        #endregion
    }
}
