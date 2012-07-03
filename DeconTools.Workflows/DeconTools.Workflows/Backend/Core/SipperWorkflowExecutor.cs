using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.IO;
using System.Linq;
using DeconTools.Backend.Core;
using DeconTools.Backend.FileIO;
using DeconTools.Backend.Utilities;
using DeconTools.Backend.Utilities.IsotopeDistributionCalculation;
using DeconTools.Utilities;

namespace DeconTools.Workflows.Backend.Core
{
    public sealed class SipperWorkflowExecutor : TargetedWorkflowExecutor
    {

        

        #region Properties

        

        public bool TargetsAreFromPeakMatchingDataBase { get; set; }

        #endregion

        #region Public Methods

        #endregion

        #region Private Methods

        #endregion

        public SipperWorkflowExecutor(WorkflowExecutorBaseParameters parameters, string datasetPath, BackgroundWorker backgroundWorker = null)
            : base(parameters, datasetPath, backgroundWorker)
        {


        }


        public override void InitializeWorkflow()
        {
            //_loggingFileName = getLogFileName(ExecutorParameters.LoggingFolder);

            Check.Require(WorkflowParameters is SipperWorkflowExecutorParameters, "Parameters are not of the right type.");


            string db = ((SipperWorkflowExecutorParameters)WorkflowParameters).DbName;
            string server = ((SipperWorkflowExecutorParameters)WorkflowParameters).DbServer;
            string table = ((SipperWorkflowExecutorParameters)WorkflowParameters).DbTableName;


            List<int> massTagIDsForFiltering =
                GetMassTagsToFilterOn(((SipperWorkflowExecutorParameters)WorkflowParameters).TargetsToFilterOn).Distinct().ToList();

            _loggingFileName = ExecutorParameters.LoggingFolder + "\\" + RunUtilities.GetDatasetName(DatasetPath) + "_log.txt";


            TargetsAreFromPeakMatchingDataBase = (!String.IsNullOrEmpty(db) && !String.IsNullOrEmpty(server));

            bool targetsFilePathIsEmpty = (String.IsNullOrEmpty(ExecutorParameters.TargetsFilePath));


            if (TargetsAreFromPeakMatchingDataBase)
            {
                Targets = LoadTargetsFromPeakMatchingResultsForGivenDataset(RunUtilities.GetDatasetName(DatasetPath));
            }
            else
            {
                if (targetsFilePathIsEmpty)
                {
                    string currentTargetsFilePath = TryFindTargetsForCurrentDataset();     //check for a _targets file specifically associated with dataset

                    if (String.IsNullOrEmpty(currentTargetsFilePath))
                    {
                        Targets = null;
                    }
                    else
                    {
                        Targets = GetLcmsFeatureTargets(currentTargetsFilePath);
                    }

                }
                else
                {
                    Targets = GetLcmsFeatureTargets(ExecutorParameters.TargetsFilePath);
                }


            }

            Check.Ensure(Targets != null && Targets.TargetList.Count > 0, "Failed to initialize - Target list is empty. Please check parameter file.");


            if (massTagIDsForFiltering.Count > 0)
            {

                Targets.TargetList =
                    (from n in Targets.TargetList
                     where massTagIDsForFiltering.Contains(((LcmsFeatureTarget)n).FeatureToMassTagID)
                     select n).ToList();

            }



            var massTagIDList = Targets.TargetList.Select(p => (long)((LcmsFeatureTarget)p).FeatureToMassTagID).ToList();

            if (TargetsAreFromPeakMatchingDataBase)
            {
                MassTagFromSqlDBImporter mtImporter = new MassTagFromSqlDBImporter(db, server, massTagIDList);
                MassTagsForReference = mtImporter.Import();
            }
            else
            {


                MassTagsForReference = GetMassTagTargets(((SipperWorkflowExecutorParameters)WorkflowParameters).ReferenceDataForTargets, massTagIDList.Select(p => (int)p).ToList());
            }

            MassTagsForReference.TargetList = (from n in MassTagsForReference.TargetList
                                               group n by new
                                               {
                                                   n.ID,
                                                   n.ChargeState
                                               }
                                                   into grp
                                                   select grp.First()).ToList();





            UpdateTargetMissingInfo(massTagIDList);


            _resultsFolder = getResultsFolder(ExecutorParameters.ResultsFolder);


            _workflowParameters = WorkflowParameters.CreateParameters(ExecutorParameters.WorkflowParameterFile);
            _workflowParameters.LoadParameters(ExecutorParameters.WorkflowParameterFile);

            TargetedWorkflow = TargetedWorkflow.CreateWorkflow(_workflowParameters);
        }

        private void UpdateTargetMissingMonoisotopicMass()
        {
            throw new NotImplementedException();
        }

        private void UpdateTargetMissingInfo(List<long> massTagIDList)
        {
            bool canUseReferenceMassTags = MassTagsForReference != null && MassTagsForReference.TargetList.Count > 0;

            foreach (LcmsFeatureTarget target in Targets.TargetList)
            {
                bool isMissingMonoMass = target.MonoIsotopicMass <= 0 ;

                if (String.IsNullOrEmpty(target.EmpiricalFormula))
                {
                    if (massTagIDList.Contains(target.FeatureToMassTagID) && canUseReferenceMassTags)
                    {
                        var mt = MassTagsForReference.TargetList.First(p => p.ID == target.FeatureToMassTagID);

                        //in DMS, Sequest will put an 'X' when it can't differentiate 'I' and 'L'
                        //  see:   \\gigasax\DMS_Parameter_Files\Sequest\sequest_ETD_N14_NE.params
                        //To create the theoretical isotopic profile, we will change the 'X' to 'L'
                        if (mt.Code.Contains("X"))
                        {
                            mt.Code = mt.Code.Replace('X', 'L');
                            mt.EmpiricalFormula = mt.GetEmpiricalFormulaFromTargetCode();
                        }

                        target.Code = mt.Code;
                        target.EmpiricalFormula = mt.EmpiricalFormula;
                    }
                    else
                    {
                        if (isMissingMonoMass)
                        {
                            throw new ApplicationException(
                                "Trying to prepare target list, but Target is missing both the 'Code' and the Monoisotopic Mass. One or the other is needed.");
                        }
                        target.Code = "AVERAGINE";
                        target.EmpiricalFormula =
                            IsotopicDistributionCalculator.GetAveragineFormulaAsString(target.MonoIsotopicMass);
                    }
                }

                
                if (isMissingMonoMass)
                {
                    target.MonoIsotopicMass =
                        EmpiricalFormulaUtilities.GetMonoisotopicMassFromEmpiricalFormula(target.EmpiricalFormula);

                    target.MZ = target.MonoIsotopicMass/target.ChargeState + DeconTools.Backend.Globals.PROTON_MASS;
                }



            }
        }


        private List<int> GetMassTagsToFilterOn(string fileRefForMassTagsToFilterOn)
        {
            List<int> masstagList = new List<int>();

            bool fileRefIsOk = !String.IsNullOrEmpty(fileRefForMassTagsToFilterOn) &&
                               File.Exists(fileRefForMassTagsToFilterOn);
            if (fileRefIsOk)
            {
                using (StreamReader reader = new StreamReader(fileRefForMassTagsToFilterOn))
                {
                    while (reader.Peek() != -1)
                    {
                        string line = reader.ReadLine();
                        int mtid = -1;
                        bool parsedOK = Int32.TryParse(line, out mtid);

                        if (parsedOK)
                        {
                            masstagList.Add(mtid);
                        }
                        else
                        {
                            masstagList.Add(-1);
                        }
                    }
                }
            }

            return masstagList;
        }

        private TargetCollection LoadTargetsFromPeakMatchingResultsForGivenDataset(string datasetName)
        {
            string table = ((SipperWorkflowExecutorParameters)WorkflowParameters).DbTableName;


            TargetCollection targetCollection = new TargetCollection();




            DbProviderFactory fact = DbProviderFactories.GetFactory("System.Data.SqlClient");
            using (DbConnection cnn = fact.CreateConnection())
            {

                cnn.ConnectionString = buildConnectionString();
                cnn.Open();

                using (DbCommand command = cnn.CreateCommand())
                {
                    string queryString;

                    queryString =
                        @"SELECT * FROM " + table + " where Dataset = '" +
                        datasetName + "' ORDER BY UMC_Ind";

                    command.CommandText = queryString;
                    command.CommandTimeout = 60;
                    DbDataReader reader = command.ExecuteReader();

                    ReadResultsFromDB(targetCollection, reader);


                }


            }

            return targetCollection;


        }




        private string buildConnectionString()
        {
            string db = ((SipperWorkflowExecutorParameters)WorkflowParameters).DbName;
            string server = ((SipperWorkflowExecutorParameters)WorkflowParameters).DbServer;


            System.Data.SqlClient.SqlConnectionStringBuilder builder = new System.Data.SqlClient.SqlConnectionStringBuilder();
            builder.UserID = "mtuser";
            builder.DataSource = server;
            builder.Password = "mt4fun";
            builder.InitialCatalog = db;
            builder.ConnectTimeout = 5;

            return builder.ConnectionString;
        }

        private double readDouble(DbDataReader reader, string columnName, double defaultVal = 0)
        {
            if (!reader[columnName].Equals(DBNull.Value))
            {
                return Convert.ToDouble(reader[columnName]);
            }
            else
            {
                return defaultVal;
            }
        }

        private float readFloat(DbDataReader reader, string columnName, float defaultVal = 0f)
        {
            if (!reader[columnName].Equals(DBNull.Value))
            {
                return Convert.ToSingle(reader[columnName]);
            }
            else
            {
                return defaultVal;
            }
        }

        private string readString(DbDataReader reader, string columnName, string defaultVal = "")
        {

            if (!reader[columnName].Equals(DBNull.Value))
            {
                return Convert.ToString(reader[columnName]);
            }
            else
            {
                return defaultVal;
            }
        }

        private int readInt(DbDataReader reader, string columnName, int defaultVal = 0)
        {

            if (!reader[columnName].Equals(DBNull.Value))
            {
                return Convert.ToInt32(reader[columnName]);
            }
            else
            {
                return defaultVal;
            }
        }

        private void ReadResultsFromDB(TargetCollection targetCollection, DbDataReader reader)
        {
            int progressCounter = 0;
            while (reader.Read())
            {
                LcmsFeatureTarget result = new LcmsFeatureTarget();


                result.ID = readInt(reader, "UMC_Ind");
                result.FeatureToMassTagID = readInt(reader, "Mass_Tag_ID");

                result.ChargeState = (short)readInt(reader, "Class_Stats_Charge_Basis");

                result.MonoIsotopicMass = readDouble(reader, "Class_Mass");

                result.MZ = result.MonoIsotopicMass / result.ChargeState + 1.00727649;
                result.NormalizedElutionTime = readFloat(reader, "ElutionTime");

                result.ScanLCTarget = readInt(reader, "Scan_Max_Abundance");
                result.ElutionTimeUnit = DeconTools.Backend.Globals.ElutionTimeUnit.ScanNum;

                targetCollection.TargetList.Add(result);
            }
        }






    }
}
