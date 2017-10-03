using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text;
using DeconTools.Backend.Utilities;
using DeconTools.Backend.Utilities.IqLogger;
using DeconTools.Utilities;
using DeconTools.Workflows.Backend.Core;

namespace DeconTools.Workflows.Backend.FileIO
{
    public class IqTargetFromDmsImporter
    {
        private List<Tuple<int, string, int, string>> _massTagModData;

        private List<int> _targetsContainingMods;

       


        #region Constructors

        public IqTargetFromDmsImporter(string serverName, string databaseName)
        {
            DbServerName = serverName;
            DbName = databaseName;

            DbUsername = "mtuser";
            DbUserPassWord = "mt4fun";
            ChunkSize = 500;

            MinPmtQualityScore = 0;
            _targetsContainingMods = new List<int>();

            IsEmpiricalFormulaExtracted = true;
            IsMonoMassCalculatedFromEmpiricalFormula = true;
        }


        #endregion

        #region Properties


        public string DbUsername { get; set; }
        public string DbServerName { get; set; }
        public string DbUserPassWord { get; set; }
        public string DbName { get; set; }
        public int ChunkSize { get; set; }
        public int MinPmtQualityScore { get; set; }

        /// <summary>
        /// If true, empirical formula is extracted from DMS. This can be a bit slow
        /// </summary>
        public bool IsEmpiricalFormulaExtracted { get; set; }

        // if true, the DMS monomass is superceded by the one calculated from the empirical formula
        public bool IsMonoMassCalculatedFromEmpiricalFormula { get; set; }


        #endregion

        #region Public Methods




        public List<IqTarget> Import()
        {
            var targets= GetMassTagDataFromDb();

            if (IsEmpiricalFormulaExtracted)
            {
                GetModDataFromDb(_targetsContainingMods);

                var peptideUtils = new PeptideUtils();

                foreach (IqTargetDms iqTarget in targets)
                {
                    var baseEmpiricalFormula = peptideUtils.GetEmpiricalFormulaForPeptideSequence(iqTarget.Code);
                    if (!string.IsNullOrEmpty(iqTarget.ModDescription))
                    {
                        var target = iqTarget;
                        var mods = (from n in _massTagModData where n.Item1 == target.ID select n);

                        foreach (var tuple in mods)
                        {
                            var modString = tuple.Item4;

                            try
                            {
                                baseEmpiricalFormula = EmpiricalFormulaUtilities.AddFormula(baseEmpiricalFormula, modString);
                            }
                            catch (Exception ex)
                            {
                                IqLogger.Log.Debug("Failed to calculate empirical formula for the Target " + target.ID + " (" + ex.Message + ")" +
                                                  "; Having trouble with the mod: " + modString + "; This Target was NOT imported!!");

                                
                            }
                        }
                    }

                    iqTarget.EmpiricalFormula = baseEmpiricalFormula;
                    
                    if (IsMonoMassCalculatedFromEmpiricalFormula)
                    {
                        iqTarget.MonoMassTheor = EmpiricalFormulaUtilities.GetMonoisotopicMassFromEmpiricalFormula(iqTarget.EmpiricalFormula);    
                    }
                    
                }
            }

            return targets;





        }



        public void SaveIqTargetsToFile(string fileName, List<IqTarget>targets  )
        {
            using (var writer=new StreamWriter(fileName))
            {
                var header = GetHeaderLine();
                writer.WriteLine(header);

                foreach (IqTargetDms iqTarget in targets)
                {
                    var targetString = GetTargetStringForExport(iqTarget);
                    writer.WriteLine(targetString);
                }

            }

            IqLogger.Log.Info("IqTargetFromDmsImporter saved " + targets.Count + " to the following file: " + fileName);
        }

        private string GetTargetStringForExport(IqTargetDms iqTarget)
        {
            var data = new List<string>
            {
                iqTarget.ID.ToString(),
                iqTarget.Code,
                iqTarget.EmpiricalFormula,
                iqTarget.MonoMassTheor.ToString("0.00000"),
                iqTarget.ElutionTimeTheor.ToString("0.000"),
                iqTarget.QualityScore.ToString("0.000"),
                iqTarget.PmtQualityScore.ToString("0.000")
            };

            return string.Join("\t", data);

        }


        private string GetHeaderLine()
        {
            var data = new List<string>
            {
                "TargetID",
                "Code",
                "EmpiricalFormula",
                "MonomassTheor",
                "ElutionTimeTheor",
                "QualityScore",
                "PmtQualityScore"
            };

            return string.Join("\t", data);

        }


        public List<IqTarget> Import(List<int>massTagIDsToImport )
        {
            return GetMassTagDataFromDb(massTagIDsToImport);
        }




        #endregion

        #region Private Methods


        private List<IqTarget> GetMassTagDataFromDb()
        {

            var iqTargetList = new List<IqTarget>();

            var fact = DbProviderFactories.GetFactory("System.Data.SqlClient");

            using (var cnn = fact.CreateConnection())
            {
                cnn.ConnectionString = BuildConnectionString();
                cnn.Open();

                var progressCounter = 0;

                var queryString = CreateQueryString(MinPmtQualityScore);
                //Console.WriteLine(queryString);


                using (var command = cnn.CreateCommand())
                {
                    command.CommandText = queryString;
                    command.CommandTimeout = 120;
                    var reader = command.ExecuteReader();


                    while (reader.Read())
                    {
                        var target = new IqTargetDms();

                        progressCounter++;

                        if (!reader[0].Equals(DBNull.Value))
                            target.ID = Convert.ToInt32(reader[0]);
                        if (!reader[1].Equals(DBNull.Value))
                            target.MonoMassTheor = Convert.ToDouble(reader[1]);
                        if (!reader[2].Equals(DBNull.Value))
                            target.ElutionTimeTheor = Convert.ToDouble(reader[2]);

                        if (!reader[3].Equals(DBNull.Value))
                            target.ElutionTimeTheorVariation = Convert.ToDouble(reader[3]);

                        //if (!reader[4].Equals(DBNull.Value))
                        //    target.ElutionTimeTheor = Convert.ToDouble(reader[4]);

                        if (!reader[5].Equals(DBNull.Value))
                            target.QualityScore = Convert.ToDouble(reader[5]);
                        if (!reader[6].Equals(DBNull.Value))
                            target.ModDescription = Convert.ToString(reader[6]);
                        if (!reader[7].Equals(DBNull.Value))
                            target.PmtQualityScore = Convert.ToInt32(reader[7]);
                        if (!reader[8].Equals(DBNull.Value))
                            target.Code = Convert.ToString(reader[8]);


                        if (!string.IsNullOrEmpty(target.ModDescription))
                        {
                            _targetsContainingMods.Add(target.ID);
                        }


                        iqTargetList.Add(target);


                        if (progressCounter % 100 == 0)
                            IqLogger.Log.Debug(progressCounter + " records loaded; " + reader[0]);
                    }
                    reader.Close();

                    IqLogger.Log.Debug("Total targets= " + iqTargetList.Count);
                    IqLogger.Log.Debug("Targets with mods = " + _targetsContainingMods.Count);
                }
            }

            return iqTargetList;





        }


        private void GetModDataFromDb(List<int> massTagsToBeRetrivedList)
        {
            var fact = DbProviderFactories.GetFactory("System.Data.SqlClient");
            _massTagModData = new List<Tuple<int, string, int, string>>();

            using (var cnn = fact.CreateConnection())
            {
                cnn.ConnectionString = BuildConnectionString();
                cnn.Open();

                using (var command = cnn.CreateCommand())
                {
                    command.CommandText = getModDataQueryString(massTagsToBeRetrivedList);

                    command.CommandTimeout = 60;
                    var reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        var mtid = 0;
                        var modName = "";
                        var modPosition = 0;
                        var empiricalFormula = "";

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
                            IqLogger.Log.Debug("ignoring this mod: " + rowData.Item1 + "; " + rowData.Item2 + "; " + rowData.Item3 + "; " + rowData.Item4 + "; " + empiricalFormula);
                            //ignore O18 modifications. In O18 workflows we look for the unmodified peptide and the labeled 
                        }
                        else if (rowData.Item2.Contains("N15"))
                        {
                            //ignore N15 modifications for now
                            IqLogger.Log.Debug("ignoring this mod: " + rowData.Item1 + "; " + rowData.Item2 + "; " + rowData.Item3 + "; " + rowData.Item4 + "; " + empiricalFormula);

                        }
                        else
                        {
                            _massTagModData.Add(rowData);
                        }


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


        private string CreateQueryString(int minPmtQualityScore)
        {
            var sb = new StringBuilder();
            sb.Append(@"SELECT dbo.t_mass_tags.mass_tag_id as MassTagID, 
                           dbo.t_mass_tags.monoisotopic_mass as MonoisotopicMass, 
                           dbo.t_mass_tags_net.avg_ganet					AS NET, 
                           dbo.T_Mass_Tags_NET.StD_GANET					as NETStDev,
                           dbo.t_mass_tags.peptide_obs_count_passing_filter AS Obs, 
                           dbo.T_Mass_Tags.Min_MSGF_SpecProb				as minMSGF, 
                           dbo.t_mass_tags.mod_description, 
                           dbo.t_mass_tags.pmt_quality_score, 
                           dbo.t_mass_tags.peptide 
                        FROM   dbo.t_mass_tags 
                           INNER JOIN dbo.t_mass_tags_net 
                             ON dbo.t_mass_tags.mass_tag_id = dbo.t_mass_tags_net.mass_tag_id 
                        WHERE dbo.t_mass_tags.pmt_quality_score >= " + minPmtQualityScore + 
                        " Order by MassTagID");

            return sb.ToString();
        }


        private List<IqTarget> GetMassTagDataFromDb(List<int> massTagsToBeRetrivedList)
        {

            var iqTargetList = new List<IqTarget>();

            var fact = DbProviderFactories.GetFactory("System.Data.SqlClient");
            var currentListPos = 0;

            using (var cnn = fact.CreateConnection())
            {
                cnn.ConnectionString = BuildConnectionString();
                cnn.Open();

                var progressCounter = 0;
                while (currentListPos < massTagsToBeRetrivedList.Count)
                {
                    var nextGroupOfMassTagIDs = massTagsToBeRetrivedList.Skip(currentListPos).Take(ChunkSize).ToList();// GetRange(currentIndex, 5000);
                    currentListPos += (ChunkSize - 1);

                    var queryString = CreateQueryString(nextGroupOfMassTagIDs);
                    //Console.WriteLine(queryString);


                    using (var command = cnn.CreateCommand())
                    {
                        command.CommandText = queryString;
                        command.CommandTimeout = 120;
                        var reader = command.ExecuteReader();


                        while (reader.Read())
                        {
                            var target = new IqTargetDms();

                            progressCounter++;

                            if (!reader[0].Equals(DBNull.Value))
                                target.ID = Convert.ToInt32(reader[0]);
                            if (!reader[1].Equals(DBNull.Value))
                                target.MonoMassTheor = Convert.ToDouble(reader[1]);
                            if (!reader[2].Equals(DBNull.Value))
                                target.ElutionTimeTheor = Convert.ToDouble(reader[2]);

                            if (!reader[3].Equals(DBNull.Value))
                                target.ElutionTimeTheorVariation = Convert.ToDouble(reader[3]);

                            //if (!reader[4].Equals(DBNull.Value))
                            //    target.ElutionTimeTheor = Convert.ToDouble(reader[4]);

                            if (!reader[5].Equals(DBNull.Value))
                                target.QualityScore = Convert.ToDouble(reader[5]);
                            if (!reader[6].Equals(DBNull.Value))
                                target.ModDescription = Convert.ToString(reader[6]);
                            if (!reader[7].Equals(DBNull.Value))
                                target.PmtQualityScore = Convert.ToInt32(reader[7]);
                            if (!reader[8].Equals(DBNull.Value))
                                target.Code = Convert.ToString(reader[8]);


                            iqTargetList.Add(target);


                            if (progressCounter % 100 == 0)
                                Console.WriteLine(progressCounter + " records loaded; " + reader[0]);
                        }
                        reader.Close();
                    }

                }
            }


            return iqTargetList;




        }

        private string BuildConnectionString()
        {
            var builder = new System.Data.SqlClient.SqlConnectionStringBuilder
                              {
                                  UserID = DbUsername,
                                  DataSource = DbServerName,
                                  Password = DbUserPassWord,
                                  InitialCatalog = DbName,
                                  ConnectTimeout = 5
                              };

            return builder.ConnectionString;
        }

        private string CreateQueryString(List<int> massTagsToBeRetrieved)
        {
            var sb = new StringBuilder();
            sb.Append(@"SELECT dbo.t_mass_tags.mass_tag_id as MassTagID, 
                           dbo.t_mass_tags.monoisotopic_mass as MonoisotopicMass, 
                           dbo.t_mass_tags_net.avg_ganet					AS NET, 
                           dbo.T_Mass_Tags_NET.StD_GANET					as NETStDev,
                           dbo.t_mass_tags.peptide_obs_count_passing_filter AS Obs, 
                           dbo.T_Mass_Tags.Min_MSGF_SpecProb				as minMSGF, 
                           dbo.t_mass_tags.mod_description, 
                           dbo.t_mass_tags.pmt_quality_score, 
                           dbo.t_mass_tags.peptide 
                        FROM   dbo.t_mass_tags 
                           INNER JOIN dbo.t_mass_tags_net 
                             ON dbo.t_mass_tags.mass_tag_id = dbo.t_mass_tags_net.mass_tag_id 
                        Order by MassTagID ");


            Check.Require(massTagsToBeRetrieved != null && massTagsToBeRetrieved.Count > 0, "Importer is trying to import mass tag data, but list of MassTags has not been set.");
            sb.Append("Mass_Tag_ID in (");

            for (var i = 0; i < massTagsToBeRetrieved.Count; i++)
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

            return sb.ToString();
        }




        #endregion


    }
}
