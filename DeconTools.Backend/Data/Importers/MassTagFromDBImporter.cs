using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Common;
using DeconTools.Utilities;
using DeconTools.Backend.Core;

namespace DeconTools.Backend.Data.Importers
{
    public class MassTagFromSqlDBImporter : IMassTagImporter
    {
        private List<long> massTagsToBeRetrieved;

        #region Constructors
        public MassTagFromSqlDBImporter()
        {
            this.DbUsername = "mtuser";
            this.DbUserPassWord = "mt4fun";
            this.ImporterMode = Globals.MassTagDBImporterMode.List_of_MT_IDs_Mode;
        }
        public MassTagFromSqlDBImporter(string dbName, string serverName)
            : this()
        {
            this.DbName = dbName;
            this.DbServerName = serverName;
        }

        #endregion

        #region Properties
        public string DbUsername { get; set; }
        public string DbServerName { get; set; }
        public string DbUserPassWord { get; set; }
        public string DbName { get; set; }
        public DeconTools.Backend.Globals.MassTagDBImporterMode ImporterMode { get; set; }
        


        #endregion

        #region Public Methods
        /// <summary>
        /// Use this to set the List of Mass Tags that will be retrieved from the database;  
        /// make sure the ImporterMode is set to 'List_of_MT_IDs_Mode'
        /// </summary>
        /// <param name="MassTagIDList"></param>
        public void SetMassTagsToRetrieve(List<long> MassTagIDList)
        {
            this.massTagsToBeRetrieved = MassTagIDList;
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
        public override void Import(DeconTools.Backend.Core.MassTagCollection data)
        {
            DbProviderFactory fact = DbProviderFactories.GetFactory("System.Data.SqlClient");
            data.MassTagList = new List<MassTag>();

            string queryString = createQueryString(this.ImporterMode);
            
            //string queryString = "SELECT mt.Mass_Tag_ID, mt.Peptide, mt.Monoisotopic_Mass, mt.Peptide_Obs_Count_Passing_Filter, ";
            //queryString += "mt.Mod_Count ";
            //queryString += "FROM [T_Mass_Tags] AS mt WHERE Peptide_Obs_Count_Passing_Filter > 50";

            using (DbConnection cnn = fact.CreateConnection())
            {
                cnn.ConnectionString = buildConnectionString();
                cnn.Open();

                using (DbCommand command = cnn.CreateCommand())
                {
                    command.CommandText = queryString;
                    DbDataReader reader = command.ExecuteReader();

                    int progressCounter = 0;
                    while (reader.Read())
                    {
                        MassTag massTag = new MassTag();

                        progressCounter++;
   
                        if (!reader["Mass_Tag_ID"].Equals(DBNull.Value)) massTag.ID = Convert.ToInt32(reader["Mass_Tag_ID"]);
                        if (!reader["Monoisotopic_Mass"].Equals(DBNull.Value)) massTag.MonoIsotopicMass = Convert.ToDouble(reader["Monoisotopic_Mass"]);
                        if (!reader["Peptide"].Equals(DBNull.Value)) massTag.PeptideSequence = Convert.ToString(reader["Peptide"]);
                        
                        massTag.CreatePeptideObject();

                        data.MassTagList.Add(massTag);
                        
                        if (progressCounter % 100 == 0) Console.WriteLine(progressCounter + " records loaded; " + reader[0]);
                    }
                }
            }

         


        }

        private string createQueryString(Globals.MassTagDBImporterMode massTagDBImporterMode)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("SELECT mt.Mass_Tag_ID, mt.Peptide, mt.Monoisotopic_Mass, mt.Peptide_Obs_Count_Passing_Filter, ");
            sb.Append("mt.Mod_Count FROM [T_Mass_Tags] as mt ");
            

            switch (massTagDBImporterMode)
            {
                case Globals.MassTagDBImporterMode.Std_four_parameter_mode:
                    throw new NotImplementedException();
                    break;
                case Globals.MassTagDBImporterMode.List_of_MT_IDs_Mode:
                    Check.Require(this.massTagsToBeRetrieved != null && this.massTagsToBeRetrieved.Count > 0, "Importer is trying to import mass tag data, but list of MassTags has not been set.");
                    sb.Append("WHERE mt.Mass_Tag_ID in (");

                    for (int i = 0; i < this.massTagsToBeRetrieved.Count; i++)
                    {
                        sb.Append(this.massTagsToBeRetrieved[i]);    //Appends the mass_tag_id

                        //if last one in list, then close parentheses. If not, just append a comma separator.
                        if (i==this.massTagsToBeRetrieved.Count-1)
                        {
                            sb.Append(")");
                        }
                        else
                        {
                            sb.Append(", ");
                        }
                        
                    }
                  
                    break;
                default:
                    break;


            }
            return sb.ToString();
        }
    }
}
