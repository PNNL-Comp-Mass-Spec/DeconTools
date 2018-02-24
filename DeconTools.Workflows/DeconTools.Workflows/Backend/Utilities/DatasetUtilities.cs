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
            var fact = DbProviderFactories.GetFactory("System.Data.SqlClient");

            var datasetPath = "";

            using (var cnn = fact.CreateConnection())
            {
                cnn.ConnectionString = buildConnectionString();
                cnn.Open();

                using (var command = cnn.CreateCommand())
                {
                    var queryString = "SELECT [Experiment],[Dataset],[Comment],[Rating],[Folder Name],[Dataset Folder Path],[Archive Folder Path] FROM [V_Dataset_Detail_Report_Ex] where (Dataset = '" + datasetName + "')";

                    command.CommandText = queryString;
                    command.CommandTimeout = 60;
                    var reader = command.ExecuteReader();

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
            var fact = DbProviderFactories.GetFactory("System.Data.SqlClient");

            var datasetPath = "";

            using (var cnn = fact.CreateConnection())
            {
                var query =

                cnn.ConnectionString = buildConnectionString();
                cnn.Open();

                using (var command = cnn.CreateCommand())
                {
                    var queryString = "SELECT [Experiment],[Dataset],[Comment],[Rating],[Folder Name],[Dataset Folder Path],[Archive Folder Path] FROM [V_Dataset_Detail_Report_Ex] where (Dataset = '" + datasetName + "')";

                    command.CommandText = queryString;
                    command.CommandTimeout = 60;
                    var reader = command.ExecuteReader();


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
            var fact = DbProviderFactories.GetFactory("System.Data.SqlClient");

            var datasetPath = "";

            using (var cnn = fact.CreateConnection())
            {
                var query =

                cnn.ConnectionString = buildConnectionString();
                cnn.Open();

                using (var command = cnn.CreateCommand())
                {
                    var queryString = "SELECT [Experiment],[Dataset],[Comment],[Rating],[Folder Name],[Dataset Folder Path],[Archive Folder Path] FROM [V_Dataset_Detail_Report_Ex] where (Dataset = '" + datasetName + "')";

                    command.CommandText = queryString;
                    command.CommandTimeout = 60;
                    var reader = command.ExecuteReader();

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
            var builder = new System.Data.SqlClient.SqlConnectionStringBuilder();
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
