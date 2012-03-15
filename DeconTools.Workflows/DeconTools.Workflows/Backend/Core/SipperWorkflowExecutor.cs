using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using DeconTools.Backend.Core;
using DeconTools.Backend.FileIO;
using DeconTools.Backend.Utilities;
using DeconTools.Utilities;

namespace DeconTools.Workflows.Backend.Core
{
    public sealed class SipperWorkflowExecutor : TargetedWorkflowExecutor
    {

        #region Constructors
        #endregion

        #region Properties

        public TargetCollection MassTagsForReference { get; set; }

        #endregion

        #region Public Methods

        #endregion

        #region Private Methods

        #endregion

        public SipperWorkflowExecutor(WorkflowExecutorBaseParameters parameters, string datasetPath)
            : base(parameters, datasetPath)
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
                GetMassTagsToFilterOn(((SipperWorkflowExecutorParameters) WorkflowParameters).MassTagsToFilterOn).Distinct().ToList();

            Targets = LoadResultsForDataset(RunUtilities.GetDatasetName(DatasetPath));

           
            if (massTagIDsForFiltering.Count > 0)
            {

                Targets.TargetList =
                    (from n in Targets.TargetList
                     where massTagIDsForFiltering.Contains(((LcmsFeatureTarget) n).FeatureToMassTagID)
                     select n).ToList();
                
            }

            var massTagIDList = Targets.TargetList.Select(p => (long)((LcmsFeatureTarget)p).FeatureToMassTagID).ToList();
            

            MassTagFromSqlDBImporter mtImporter = new MassTagFromSqlDBImporter(db, server, massTagIDList);
            MassTagsForReference = mtImporter.Import();

            MassTagsForReference.TargetList = (from n in MassTagsForReference.TargetList
                                               group n by new
                                               {
                                                   n.ID,
                                                   n.ChargeState
                                               }
                                                   into grp
                                                   select grp.First()).ToList();

           

            //Targets.TargetList = Targets.TargetList.Take(10).ToList();


            //Update Targets using MassTag info  (targets are LCMSFeatures. We need to grab empirical formulas, etc. from the MassTag info)
            foreach (LcmsFeatureTarget target in Targets.TargetList)
            {

                if (massTagIDList.Contains(target.FeatureToMassTagID))
                {
                    var mt = MassTagsForReference.TargetList.First(p => p.ID == target.FeatureToMassTagID);
                    
                    //in DMS, Sequest will put an 'X' when it can't differentiate 'I' and 'L'
                    //  see:   \\gigasax\DMS_Parameter_Files\Sequest\sequest_ETD_N14_NE.params
                    //To create the theoretical isotopic profile, we will change the 'X' to 'L'
                    if (mt.Code.Contains("X"))
                    {

                       mt.Code= mt.Code.Replace('X', 'L');
                       mt.EmpiricalFormula=  mt.GetEmpiricalFormulaFromTargetCode();
                    }
                    
                    target.Code = mt.Code;
                    target.EmpiricalFormula = mt.EmpiricalFormula;
                }
                else
                {

                }

            }


            _resultsFolder = getResultsFolder(ExecutorParameters.ResultsFolder);

            Check.Ensure(Targets != null && Targets.TargetList.Count > 0,
                         "Target massTags is empty. Check the path to the massTag data file.");


            _workflowParameters = WorkflowParameters.CreateParameters(ExecutorParameters.WorkflowParameterFile);
            _workflowParameters.LoadParameters(ExecutorParameters.WorkflowParameterFile);

            targetedWorkflow = TargetedWorkflow.CreateWorkflow(_workflowParameters);
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
                    while (reader.Peek()!=-1)
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

        private TargetCollection LoadResultsForDataset(string datasetName)
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
                        datasetName + "'";

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
