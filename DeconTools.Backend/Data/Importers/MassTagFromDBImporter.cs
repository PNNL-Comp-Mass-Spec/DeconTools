using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Common;
using DeconTools.Utilities;
using DeconTools.Backend.Core;
using System.Collections.ObjectModel;

namespace DeconTools.Backend.Data.Importers
{
    public class MassTagFromSqlDBImporter : IMassTagImporter
    {
        public List<long> MassTagsToBeRetrieved {get; private set;} 

        #region Constructors
        public MassTagFromSqlDBImporter()
        {
            this.DbUsername = "mtuser";
            this.DbUserPassWord = "mt4fun";
            this.ImporterMode = Globals.MassTagDBImporterMode.List_of_MT_IDs_Mode;
            this.chargeStateFilterThreshold = 0.1;
        }

        

        public MassTagFromSqlDBImporter(string dbName, string serverName, List<long> massTagsToBeRetrieved)
            : this()
        {
            this.DbName = dbName;
            this.DbServerName = serverName;
            this.MassTagsToBeRetrieved = massTagsToBeRetrieved;
        }

        #endregion

        #region Properties
        public string DbUsername { get; set; }
        public string DbServerName { get; set; }
        public string DbUserPassWord { get; set; }
        public string DbName { get; set; }
        public DeconTools.Backend.Globals.MassTagDBImporterMode ImporterMode { get; set; }

        /// <summary>
        /// A value between 0 and 1.  A value of 0 means 
        /// </summary>
        public double chargeStateFilterThreshold { get; set; }

        #endregion

        #region Public Methods
  
  
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
        public override DeconTools.Backend.Core.MassTagCollection Import()
        {
            DbProviderFactory fact = DbProviderFactories.GetFactory("System.Data.SqlClient");
            
            DeconTools.Backend.Core.MassTagCollection data=new MassTagCollection();
            data.MassTagList = new List<MassTag>();

            

            string queryString = createQueryString(this.ImporterMode);
            //Console.WriteLine(queryString);


            using (DbConnection cnn = fact.CreateConnection())
            {
                cnn.ConnectionString = buildConnectionString();
                cnn.Open();

                using (DbCommand command = cnn.CreateCommand())
                {
                    command.CommandText = queryString;
                    command.CommandTimeout = 60;
                    DbDataReader reader = command.ExecuteReader();

                    int progressCounter = 0;
                    while (reader.Read())
                    {
                        MassTag massTag = new MassTag();

                        progressCounter++;
   
                        if (!reader["Mass_Tag_ID"].Equals(DBNull.Value)) massTag.ID = Convert.ToInt32(reader["Mass_Tag_ID"]);
                        if (!reader["Monoisotopic_Mass"].Equals(DBNull.Value)) massTag.MonoIsotopicMass = Convert.ToDouble(reader["Monoisotopic_Mass"]);
                        if (!reader["Peptide"].Equals(DBNull.Value)) massTag.PeptideSequence = Convert.ToString(reader["Peptide"]);
                        if (!reader["Charge_State"].Equals(DBNull.Value)) massTag.ChargeState = Convert.ToInt16(reader["Charge_State"]);
                        if (!reader["ObsCount"].Equals(DBNull.Value)) massTag.ObsCount = Convert.ToInt32(reader["ObsCount"]);
                        if (massTag.ChargeState != 0)
                        {
                            massTag.MZ = massTag.MonoIsotopicMass / massTag.ChargeState + Globals.PROTON_MASS;
                        }

                        if (!reader["Avg_GANET"].Equals(DBNull.Value)) massTag.NETVal = Convert.ToSingle(reader["Avg_GANET"]);
                        if (!reader["Ref_ID"].Equals(DBNull.Value)) massTag.RefID = Convert.ToInt32(reader["Ref_ID"]);
                        if (!reader["Description"].Equals(DBNull.Value)) massTag.ProteinDescription = Convert.ToString(reader["Description"]);
                      
                        massTag.CreatePeptideObject();

                        data.MassTagList.Add(massTag);
                        
                        if (progressCounter % 100 == 0) Console.WriteLine(progressCounter + " records loaded; " + reader[0]);
                    }
                }
            }

            data.ApplyChargeStateFilter(this.chargeStateFilterThreshold);
            return data;
         


        }

        private string testQueryString()
        {
            string qry = @"SELECT *
FROM ( SELECT Mass_Tag_ID,
              Monoisotopic_Mass,
              Peptide,
              Charge_State,
              ObsCount,
              Monoisotopic_Mass/Charge_State+1.00727649 as mz,
              Avg_GANET,                            
              Ref_ID,
              Description,
              Row_Number() OVER ( PARTITION BY mass_tag_id ORDER BY ObsCount DESC ) AS ObsRank
              
       FROM ( SELECT T_Mass_Tags.Mass_Tag_ID,
                     T_Mass_Tags.Monoisotopic_Mass,
                     T_Mass_Tags.Peptide,
                     T_Peptides.Charge_State,
                     T_Mass_Tags_NET.Avg_GANET,
                     T_Mass_Tag_to_Protein_Map.Ref_ID,
                     T_Proteins.Description,
                     COUNT(*) AS ObsCount
              FROM T_Mass_Tags
                   INNER JOIN T_Peptides
                     ON T_Mass_Tags.Mass_Tag_ID = T_Peptides.Mass_Tag_ID
                     INNER JOIN T_Mass_Tags_NET
                     ON T_Mass_Tags.Mass_Tag_ID=T_Mass_Tags_NET.Mass_Tag_ID
                     INNER JOIN T_Mass_Tag_to_Protein_Map
                     ON T_Mass_Tags.Mass_Tag_ID=T_Mass_Tag_to_Protein_Map.Mass_Tag_ID
                     INNER JOIN T_Proteins
                     ON T_Mass_Tag_to_Protein_Map.Ref_ID=T_Proteins.Ref_ID
              GROUP BY T_Mass_Tags.Mass_Tag_ID,T_Mass_Tags.Monoisotopic_Mass, T_Mass_Tags.Peptide, T_Peptides.Charge_State,T_Mass_Tags_NET.Avg_GANET, T_Mass_Tag_to_Protein_Map.Ref_ID, T_Proteins.Description
             ) LookupQ 
      ) OuterQ WHERE (ObsRank in (1,2,3) and Mass_Tag_ID in (339661, 1880720, 127913, 1100499, 1239111, 994489, 417866, 106915, 1149424, 2763428, 2763428, 2763428, 239704, 44696, 213135, 971852, 24917, 101068, 243782, 24826, 194781, 194781, 1709835, 614192, 614192, 25982, 313378, 232945, 2193778, 323142, 1844543, 3176757, 3176757, 56475, 311742, 1116349, 987418, 27168, 306160, 1220666))
      ORDER BY Mass_Tag_ID";
            return qry;
        }




        private string createQueryString(Globals.MassTagDBImporterMode massTagDBImporterMode)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(@"SELECT *
FROM ( SELECT Mass_Tag_ID,
              Monoisotopic_Mass,
              Peptide,
              Charge_State,
              ObsCount,
              Monoisotopic_Mass/Charge_State+1.00727649 as mz,
              Avg_GANET,                            
              Ref_ID,
              Description,
              Row_Number() OVER ( PARTITION BY mass_tag_id ORDER BY ObsCount DESC ) AS ObsRank
              
       FROM ( SELECT T_Mass_Tags.Mass_Tag_ID,
                     T_Mass_Tags.Monoisotopic_Mass,
                     T_Mass_Tags.Peptide,
                     T_Peptides.Charge_State,
                     T_Mass_Tags_NET.Avg_GANET,
                     T_Mass_Tag_to_Protein_Map.Ref_ID,
                     T_Proteins.Description,
                     COUNT(*) AS ObsCount
              FROM T_Mass_Tags
                   INNER JOIN T_Peptides
                     ON T_Mass_Tags.Mass_Tag_ID = T_Peptides.Mass_Tag_ID
                     INNER JOIN T_Mass_Tags_NET
                     ON T_Mass_Tags.Mass_Tag_ID=T_Mass_Tags_NET.Mass_Tag_ID
                     INNER JOIN T_Mass_Tag_to_Protein_Map
                     ON T_Mass_Tags.Mass_Tag_ID=T_Mass_Tag_to_Protein_Map.Mass_Tag_ID
                     INNER JOIN T_Proteins
                     ON T_Mass_Tag_to_Protein_Map.Ref_ID=T_Proteins.Ref_ID
              GROUP BY T_Mass_Tags.Mass_Tag_ID,T_Mass_Tags.Monoisotopic_Mass, T_Mass_Tags.Peptide, T_Peptides.Charge_State,T_Mass_Tags_NET.Avg_GANET, T_Mass_Tag_to_Protein_Map.Ref_ID, T_Proteins.Description
             ) LookupQ 
      ) OuterQ ");
            

            switch (massTagDBImporterMode)
            {
                case Globals.MassTagDBImporterMode.Std_four_parameter_mode:
                    throw new NotImplementedException();
                    break;
                case Globals.MassTagDBImporterMode.List_of_MT_IDs_Mode:
                    Check.Require(this.MassTagsToBeRetrieved != null && this.MassTagsToBeRetrieved.Count > 0, "Importer is trying to import mass tag data, but list of MassTags has not been set.");
                    sb.Append("WHERE (ObsRank in (1,2,3) and Mass_Tag_ID in (");

                    for (int i = 0; i < this.MassTagsToBeRetrieved.Count; i++)
                    {
                        sb.Append(this.MassTagsToBeRetrieved[i]);    //Appends the mass_tag_id

                        //if last one in list, then close parentheses. If not, just append a comma separator.
                        if (i == this.MassTagsToBeRetrieved.Count - 1)
                        {
                            //sb.Append(")) ORDER BY Mass_Tag_ID");
                            sb.Append("))");

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
