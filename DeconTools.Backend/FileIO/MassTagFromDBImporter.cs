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
    public class MassTagFromSqlDbImporter : IMassTagImporter
    {
        private List<Tuple<int, string, int, string>> _massTagModData;
        public List<long> MassTagsToBeRetrieved { get; }

        #region Constructors
        public MassTagFromSqlDbImporter()
        {
            DbUsername = "mtuser";
            DbUserPassWord = "mt4fun";
            ImporterMode = Globals.MassTagDBImporterMode.List_of_MT_IDs_Mode;
            ChunkSize = 500;
            ChargeStateFilterThreshold = 0.1;

            MinMZForChargeStateRange = 400;

            MaxMZForChargeStateRange = 1500;

        }



        public MassTagFromSqlDbImporter(string dbName, string serverName, List<long> massTagsToBeRetrieved)
            : this()
        {
            DbName = dbName;
            DbServerName = serverName;
            MassTagsToBeRetrieved = massTagsToBeRetrieved;
        }

        #endregion

        #region Properties
        public string DbUsername { get; set; }
        public string DbServerName { get; set; }
        public string DbUserPassWord { get; set; }
        public string DbName { get; set; }


        public int ChunkSize { get; set; }

        public double MinMZForChargeStateRange { get; set; }

        public double MaxMZForChargeStateRange { get; set; }

        public bool ChargeStateRangeBasedOnDatabase { get; set; }


        public Globals.MassTagDBImporterMode ImporterMode { get; set; }

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
        #endregion
        public override TargetCollection Import()
        {

            var targetCollection = new TargetCollection();
            targetCollection.TargetList.Clear();

            GetMassTagDataFromDB(targetCollection, MassTagsToBeRetrieved);

            GetModDataFromDB(targetCollection, MassTagsToBeRetrieved);

            var peptideUtils = new PeptideUtils();

            var troubleSomePeptides = new List<TargetBase>();


            foreach (var targetBase in targetCollection.TargetList)
            {
                var peptide = (PeptideTarget)targetBase;
                var baseEmpiricalFormula = peptideUtils.GetEmpiricalFormulaForPeptideSequence(peptide.Code);

                if (peptide.ContainsMods)
                {
                    var peptide1 = peptide;
                    var mods = (from n in _massTagModData where n.Item1 == peptide1.ID select n);

                    foreach (var tuple in mods)
                    {
                        var modString = tuple.Item4;

                        try
                        {
                            baseEmpiricalFormula = EmpiricalFormulaUtilities.AddFormula(baseEmpiricalFormula, modString);
                        }
                        catch
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
                catch
                {
                    Console.WriteLine("Failed to calculate empirical formula for the Target " + peptide.ID +
                                             "; Cannot parse this formula: " + peptide.EmpiricalFormula + "; This Target was NOT imported!!");

                    troubleSomePeptides.Add(peptide);
                }


            }

            var cleanTargetList = new List<TargetBase>();

            //filter out the bad peptides (the once with errors during empirical formula parsing)
            foreach (var peptide in targetCollection.TargetList)
            {
                if (!troubleSomePeptides.Contains(peptide))
                {
                    cleanTargetList.Add(peptide);
                }

            }

            targetCollection.TargetList = cleanTargetList;


            if (ChargeStateRangeBasedOnDatabase)
            {
                targetCollection.ApplyChargeStateFilter(ChargeStateFilterThreshold);
            }
            else
            {
                var chargeStateTargets = new List<TargetBase>();

                foreach (var targetBase in targetCollection.TargetList)
                {
                    var minCharge = 1;
                    var maxCharge = 100;

                    for (var charge = minCharge; charge <= maxCharge; charge++)
                    {
                        var mz = targetBase.MonoIsotopicMass / charge + Globals.PROTON_MASS;

                        if (mz < MaxMZForChargeStateRange)
                        {

                            if (mz < MinMZForChargeStateRange)
                            {
                                break;
                            }

                            TargetBase chargeStateTarget = new PeptideTarget((PeptideTarget) targetBase);
                            chargeStateTarget.ChargeState = (short) charge;
                            chargeStateTarget.MZ = chargeStateTarget.MonoIsotopicMass/charge + Globals.PROTON_MASS;
                            chargeStateTarget.ObsCount = -1;
                            chargeStateTargets.Add(chargeStateTarget);
                        }
                    }

                }



                targetCollection.TargetList = chargeStateTargets.OrderBy(p=>p.ID).ThenBy(p=>p.ChargeState).ToList();


            }


            return targetCollection;



        }

        private void CalculateEmpiricalFormulas(TargetCollection data)
        {
            var peptideUtils = new PeptideUtils();
            foreach (var targetBase in data.TargetList)
            {
                var peptide = (PeptideTarget)targetBase;
                var baseEmpiricalFormula = peptideUtils.GetEmpiricalFormulaForPeptideSequence(peptide.Code);

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

        private void GetModDataFromDB(TargetCollection data, IReadOnlyList<long> massTagsToBeRetrivedList)
        {
            var fact = DbProviderFactories.GetFactory("System.Data.SqlClient");
            var queryString = createQueryString(ImporterMode, massTagsToBeRetrivedList);
            //Console.WriteLine(queryString);

            var modContainingPeptides = (from n in data.TargetList where n.ModCount > 0 select n).ToList();
            if (modContainingPeptides.Count == 0) return;


            _massTagModData = new List<Tuple<int, string, int, string>>();

            using (var cnn = fact.CreateConnection())
            {
                if (cnn == null)
                    throw new Exception("Factory.CreateConnection returned a null DbConnection instance in GetModDataFromDB");

                cnn.ConnectionString = buildConnectionString();
                cnn.Open();

                using (var command = cnn.CreateCommand())
                {
                    command.CommandText = getModDataQueryString(modContainingPeptides.Select(p => p.ID).Distinct());

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

        private void GetMassTagDataFromDB(TargetCollection data, IReadOnlyCollection<long> massTagsToBeRetrivedList)
        {
            var fact = DbProviderFactories.GetFactory("System.Data.SqlClient");


            var currentListPos = 0;


            using (var cnn = fact.CreateConnection())
            {
                if (cnn == null)
                    throw new Exception("Factory.CreateConnection returned a null DbConnection object in GetMassTagDataFromDB");

                cnn.ConnectionString = buildConnectionString();
                cnn.Open();

                var progressCounter = 0;
                while (currentListPos < massTagsToBeRetrivedList.Count )
                {
                    var nextGroupOfMassTagIDs = massTagsToBeRetrivedList.Skip(currentListPos).Take(ChunkSize).ToList();// GetRange(currentIndex, 5000);
                    currentListPos += (ChunkSize-1);

                    var queryString = createQueryString(ImporterMode,nextGroupOfMassTagIDs);
                    //Console.WriteLine(queryString);


                    using (var command = cnn.CreateCommand())
                    {
                        command.CommandText = queryString;
                        command.CommandTimeout = 120;
                        var reader = command.ExecuteReader();


                        while (reader.Read())
                        {
                            var massTag = new PeptideTarget();

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

        private string createQueryString(Globals.MassTagDBImporterMode massTagDBImporterMode, IReadOnlyList<long> massTagsToBeRetrieved)
        {
            var sb = new StringBuilder();
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

                    if (massTagsToBeRetrieved == null)
                        break;

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

                    break;
            }
            return sb.ToString();
        }
    }
}
