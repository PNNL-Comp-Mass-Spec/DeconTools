using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core;
using DeconTools.Backend.Utilities;
using DeconTools.Utilities;

namespace DeconTools.Backend.FileIO
{
    public class MassTagFromSqlDBImporter : IMassTagImporter
    {
        private List<Tuple<int, string, int, string>> _massTagModData;
        public List<long> MassTagsToBeRetrieved { get; private set; }

        #region Constructors
        public MassTagFromSqlDBImporter()
        {
            DbUsername = "mtuser";
            DbUserPassWord = "mt4fun";
            ImporterMode = Globals.MassTagDBImporterMode.List_of_MT_IDs_Mode;
            ChunkSize = 500;
            ChargeStateFilterThreshold = 0.1;
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


        public int ChunkSize { get; set; }


        public DeconTools.Backend.Globals.MassTagDBImporterMode ImporterMode { get; set; }

        /// <summary>
        /// A value between 0 and 1.  A value of 0 means 
        /// </summary>
        public double ChargeStateFilterThreshold { get; set; }

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
        public override DeconTools.Backend.Core.TargetCollection Import()
        {

            DeconTools.Backend.Core.TargetCollection data = new TargetCollection();
            data.TargetList.Clear();

            GetMassTagDataFromDB(data, MassTagsToBeRetrieved);

            GetModDataFromDB(data, MassTagsToBeRetrieved);

            PeptideUtils peptideUtils = new PeptideUtils();

            List<TargetBase> troubleSomePeptides = new List<TargetBase>();


            foreach (PeptideTarget peptide in data.TargetList)
            {
                string baseEmpiricalFormula = peptideUtils.GetEmpiricalFormulaForPeptideSequence(peptide.Code);

                if (peptide.ContainsMods)
                {
                    PeptideTarget peptide1 = peptide;
                    var mods = (from n in _massTagModData where n.Item1 == peptide1.ID select n);

                    foreach (var tuple in mods)
                    {
                        string modString = tuple.Item4;

                        try
                        {
                            baseEmpiricalFormula = EmpiricalFormulaUtilities.AddFormula(baseEmpiricalFormula, modString);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Failed to calculate empirical formula for the Target " + peptide.ID +
                                              "; Having trouble with the mod: " + modString + "; This Target was NOT imported!!");

                            troubleSomePeptides.Add(peptide);
                        }
                        
                    }
                }

                try
                {
                    peptide.EmpiricalFormula = baseEmpiricalFormula;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Failed to calculate empirical formula for the Target " + peptide.ID +
                                             "; Cannot parse this formula: " + peptide.EmpiricalFormula + "; This Target was NOT imported!!");

                    troubleSomePeptides.Add(peptide);
                }

                
            }

            var cleanTargetList = new List<TargetBase>();

            //filter out the bad peptides (the once with errors during empirical formula parsing)
            foreach (var peptide in data.TargetList)
            {
                if (!troubleSomePeptides.Contains(peptide))
                {
                    cleanTargetList.Add(peptide);
                }

            }

            data.TargetList = cleanTargetList;

            data.ApplyChargeStateFilter(this.ChargeStateFilterThreshold);

            return data;



        }

        private void CalculateEmpiricalFormulas(TargetCollection data)
        {
            PeptideUtils peptideUtils = new PeptideUtils();
            foreach (PeptideTarget peptide in data.TargetList)
            {
                string baseEmpiricalFormula = peptideUtils.GetEmpiricalFormulaForPeptideSequence(peptide.Code);

                if (peptide.ContainsMods)
                {
                    TargetBase peptide1 = peptide;
                    var mods = (from n in _massTagModData where n.Item1 == peptide1.ID select n);

                    foreach (var tuple in mods)
                    {
                        baseEmpiricalFormula = EmpiricalFormulaUtilities.AddFormula(baseEmpiricalFormula, tuple.Item4);
                    }
                }


                peptide.EmpiricalFormula = baseEmpiricalFormula;
            }
        }

        private void GetModDataFromDB(TargetCollection data, List<long> massTagsToBeRetrivedList)
        {
            var fact = DbProviderFactories.GetFactory("System.Data.SqlClient");
            string queryString = createQueryString(this.ImporterMode, massTagsToBeRetrivedList);
            //Console.WriteLine(queryString);

            var modContainingPeptides = (from n in data.TargetList where n.ModCount > 0 select n).ToList();
            if (modContainingPeptides.Count == 0) return;


            _massTagModData = new List<Tuple<int, string, int, string>>();

            using (var cnn = fact.CreateConnection())
            {
                cnn.ConnectionString = buildConnectionString();
                cnn.Open();

                using (DbCommand command = cnn.CreateCommand())
                {
                    command.CommandText = getModDataQueryString(modContainingPeptides.Select(p => p.ID).Distinct());

                    command.CommandTimeout = 60;
                    DbDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        int mtid = 0;
                        string modName = "";
                        int modPosition = 0;
                        string empiricalFormula = "";

                        if (!reader["Mass_Tag_ID"].Equals(DBNull.Value))
                            mtid = Convert.ToInt32(reader["Mass_Tag_ID"]);
                        if (!reader["Mod_Name"].Equals(DBNull.Value))
                            modName = Convert.ToString(reader["Mod_Name"]);
                        if (!reader["Mod_Position"].Equals(DBNull.Value)) modPosition = Convert.ToInt32(reader["Mod_Position"]);
                        if (!reader["Empirical_Formula"].Equals(DBNull.Value))
                            empiricalFormula = Convert.ToString(reader["Empirical_Formula"]);

                        var rowData = Tuple.Create(mtid, modName, modPosition, empiricalFormula);

                        

                        if (rowData.Item2.Contains("O18"))
                        {
                            Console.WriteLine("ignoring this mod: " + rowData.Item1 + "; " + rowData.Item2 + "; " + rowData.Item3 + "; " + rowData.Item4 + "; " + empiricalFormula);
                            //ignore O18 modifications. In O18 workflows we look for the unmodified peptide and the labeled 
                        }
                        else if (rowData.Item2.Contains("N15"))
                        {
                            //ignore N15 modifications for now
                            Console.WriteLine("ignoring this mod: " + rowData.Item1 + "; " + rowData.Item2 + "; " + rowData.Item3 + "; " + rowData.Item4 + "; " + empiricalFormula);
                            
                        }
                        else
                        {
                            _massTagModData.Add(rowData); 
                        }

                        
                    }
                }
            }





        }



        private void GetMassTagDataFromDB(TargetCollection data, List<long> massTagsToBeRetrivedList)
        {
            var fact = DbProviderFactories.GetFactory("System.Data.SqlClient");


            int currentListPos = 0;
            

            using (var cnn = fact.CreateConnection())
            {
                cnn.ConnectionString = buildConnectionString();
                cnn.Open();

                int progressCounter = 0;
                while (currentListPos < massTagsToBeRetrivedList.Count )
                {
                    List<long> nextGroupOfMassTagIDs = massTagsToBeRetrivedList.Skip(currentListPos).Take(ChunkSize).ToList();// GetRange(currentIndex, 5000);
                    currentListPos += (ChunkSize-1);

                    string queryString = createQueryString(this.ImporterMode,nextGroupOfMassTagIDs);
                    //Console.WriteLine(queryString);

                    
                    using (DbCommand command = cnn.CreateCommand())
                    {
                        command.CommandText = queryString;
                        command.CommandTimeout = 120;
                        DbDataReader reader = command.ExecuteReader();

                        
                        while (reader.Read())
                        {
                            PeptideTarget massTag = new PeptideTarget();

                            progressCounter++;

                            if (!reader["Mass_Tag_ID"].Equals(DBNull.Value))
                                massTag.ID = Convert.ToInt32(reader["Mass_Tag_ID"]);
                            if (!reader["Monoisotopic_Mass"].Equals(DBNull.Value))
                                massTag.MonoIsotopicMass = Convert.ToDouble(reader["Monoisotopic_Mass"]);
                            if (!reader["Peptide"].Equals(DBNull.Value))
                                massTag.Code = Convert.ToString(reader["Peptide"]);
                            if (!reader["Charge_State"].Equals(DBNull.Value))
                                massTag.ChargeState = Convert.ToInt16(reader["Charge_State"]);
                            if (!reader["Mod_Count"].Equals(DBNull.Value))
                                massTag.ModCount = Convert.ToInt16(reader["Mod_Count"]);
                            if (!reader["Mod_Description"].Equals(DBNull.Value))
                                massTag.ModDescription = Convert.ToString(reader["Mod_Description"]);
                            if (!reader["ObsCount"].Equals(DBNull.Value))
                                massTag.ObsCount = Convert.ToInt32(reader["ObsCount"]);
                            if (massTag.ChargeState != 0)
                            {
                                massTag.MZ = massTag.MonoIsotopicMass/massTag.ChargeState + Globals.PROTON_MASS;
                            }

                            if (!reader["Avg_GANET"].Equals(DBNull.Value))
                                massTag.NormalizedElutionTime = Convert.ToSingle(reader["Avg_GANET"]);
                            if (!reader["Ref_ID"].Equals(DBNull.Value))
                                massTag.RefID = Convert.ToInt32(reader["Ref_ID"]);
                            if (!reader["Reference"].Equals(DBNull.Value))
                                massTag.GeneReference = Convert.ToString(reader["Reference"]);
                            if (!reader["Description"].Equals(DBNull.Value))
                                massTag.ProteinDescription = Convert.ToString(reader["Description"]);


                            data.TargetList.Add(massTag);

                            if (progressCounter%100 == 0)
                                Console.WriteLine(progressCounter + " records loaded; " + reader[0]);
                        }
                        reader.Close();
                    }

                }
            }






            
        }


        private string getModDataQueryString(IEnumerable<int> massTagIDs)
        {
            var sb = new StringBuilder();

            sb.Append(
                @"SELECT MTMI.Mass_Tag_ID,MTMI.Mod_Name,MTMI.Mod_Position,MCF.Empirical_Formula
                    FROM T_Mass_Tag_Mod_Info MTMI INNER JOIN MT_Main.dbo.V_DMS_Mass_Correction_Factors MCF
                    ON MTMI.Mod_Name = MCF.Mass_Correction_Tag INNER JOIN T_Mass_Tags MT
                    ON MTMI.Mass_Tag_ID = MT.Mass_Tag_ID 
                    WHERE MT.Mass_Tag_ID in (");

            foreach (var massTagID in massTagIDs)
            {
                sb.Append(massTagID);    //Appends the mass_tag_id
                sb.Append(",");
            }

            return sb.ToString().TrimEnd(',') + ")";

        }

        private string createQueryString(Globals.MassTagDBImporterMode massTagDBImporterMode, List<long> massTagsToBeRetrieved)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(@"SELECT * FROM ( SELECT Mass_Tag_ID,
              Monoisotopic_Mass,
              Peptide,
              PeptideEx,
              Mod_Count,
              Mod_Description,
              Charge_State,
              ObsCount,
              Monoisotopic_Mass/Charge_State+1.00727649 as mz,
              Avg_GANET,                            
              Ref_ID,
              Reference,
              Description,
              Row_Number() OVER ( PARTITION BY mass_tag_id ORDER BY ObsCount DESC ) AS ObsRank
              
       FROM ( SELECT T_Mass_Tags.Mass_Tag_ID,
                     T_Mass_Tags.Monoisotopic_Mass,
                     T_Mass_Tags.Peptide,
                     T_Mass_Tags.PeptideEx,
                     T_Mass_Tags.Mod_Count,
                     T_Mass_Tags.Mod_Description,
                     T_Peptides.Charge_State,
                     T_Mass_Tags_NET.Avg_GANET,
                     T_Mass_Tag_to_Protein_Map.Ref_ID,
                     T_Proteins.Reference,   
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
              WHERE pmt_quality_score >= 2
              GROUP BY T_Mass_Tags.Mass_Tag_ID,T_Mass_Tags.Monoisotopic_Mass, T_Mass_Tags.Peptide, T_Mass_Tags.PeptideEx,
              T_Mass_Tags.Mod_Count,T_Mass_Tags.Mod_Description,T_Peptides.Charge_State,T_Mass_Tags_NET.Avg_GANET, T_Mass_Tag_to_Protein_Map.Ref_ID,
              T_Proteins.Reference, T_Proteins.Description
             ) LookupQ 
      ) OuterQ ");


            switch (massTagDBImporterMode)
            {
                case Globals.MassTagDBImporterMode.Std_four_parameter_mode:
                    throw new NotImplementedException();
                case Globals.MassTagDBImporterMode.List_of_MT_IDs_Mode:
                    Check.Require(massTagsToBeRetrieved != null && massTagsToBeRetrieved.Count > 0, "Importer is trying to import mass tag data, but list of MassTags has not been set.");
                    sb.Append("WHERE (ObsRank in (1,2,3) and Mass_Tag_ID in (");

                    for (int i = 0; i < massTagsToBeRetrieved.Count; i++)
                    {
                        sb.Append(massTagsToBeRetrieved[i]);    //Appends the mass_tag_id

                        //if last one in list, then close parentheses. If not, just append a comma separator.
                        if (i == massTagsToBeRetrieved.Count - 1)
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
