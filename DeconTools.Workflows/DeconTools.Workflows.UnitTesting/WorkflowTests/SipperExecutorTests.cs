using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core;
using DeconTools.Backend.Core.Results;
using DeconTools.Backend.FileIO;
using DeconTools.Workflows.Backend;
using DeconTools.Workflows.Backend.Core;
using DeconTools.Workflows.Backend.Utilities;
using NUnit.Framework;

namespace DeconTools.Workflows.UnitTesting.WorkflowTests
{
    [TestFixture]
    public class SipperExecutorTests
    {

        [Test]
        public void ExecuteSipper()
        {
            string paramFile =
                @"\\protoapps\DataPkgs\Public\2012\601_Sipper_paper_data_processing_and_analysis\Parameters\SipperExecutorParams1.xml";

            SipperWorkflowExecutorParameters parameters = new SipperWorkflowExecutorParameters();
            parameters.LoadParameters(paramFile);
            parameters.CopyRawFileLocal = false;
            //parameters.FolderPathForCopiedRawDataset = @"D:\data\temp";


            string testDataset =
                @"\\protoapps\DataPkgs\Public\2012\601_Sipper_paper_data_processing_and_analysis\RawData\Yellow_C13_070_23Mar10_Griffin_10-01-28.raw";


            string outputParameterFile = Path.GetDirectoryName(paramFile) + Path.DirectorySeparatorChar +
                                         Path.GetFileNameWithoutExtension(paramFile) + " - copy.xml";
            parameters.SaveParametersToXML(outputParameterFile);


            SipperWorkflowExecutor executor = new SipperWorkflowExecutor(parameters, testDataset);


            int[] targetsOfInterest = new int[]{5555};

            executor.Targets.TargetList =
                (executor.Targets.TargetList.Where(n => targetsOfInterest.Contains(n.ID))).ToList();

            executor.Execute();

            SipperTargetedWorkflow workflow = executor.TargetedWorkflow as SipperTargetedWorkflow;



            SipperLcmsTargetedResult result = workflow.Result as SipperLcmsTargetedResult;

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.ChromCorrelationData);
            Assert.IsNotNull(result.ChromCorrelationData.CorrelationDataItems);

            Assert.IsTrue(result.ChromCorrelationData.CorrelationDataItems.Count > 0);


            //foreach (var dataItem in result.ChromCorrelationData.CorrelationDataItems)
            //{
            //    Console.WriteLine(dataItem.CorrelationRSquaredVal);
            //}

            foreach (var fitScoreDataItem in workflow.FitScoreData)
            {
                Console.WriteLine(fitScoreDataItem.Key + "\t" + fitScoreDataItem.Value);
            }
            

        }


        [Test]
        public void ExecuteSipperUsingStandardExecutorClass1()
        {
            string paramFile =
                @"\\protoapps\DataPkgs\Public\2012\601_Sipper_paper_data_processing_and_analysis\Parameters\ExecutorParameters1.xml";

            var parameters = new BasicTargetedWorkflowExecutorParameters();
            parameters.LoadParameters(paramFile);
            parameters.CopyRawFileLocal = false;
            //parameters.FolderPathForCopiedRawDataset = @"D:\data\temp";


            string testDataset =
                @"\\protoapps\DataPkgs\Public\2012\601_Sipper_paper_data_processing_and_analysis\RawData\Yellow_C13_070_23Mar10_Griffin_10-01-28.raw";


            string outputParameterFile = Path.GetDirectoryName(paramFile) + Path.DirectorySeparatorChar +
                                         Path.GetFileNameWithoutExtension(paramFile) + " - copy.xml";
            parameters.SaveParametersToXML(outputParameterFile);


            parameters.TargetType = Globals.TargetType.LcmsFeature;
            var executor = new BasicTargetedWorkflowExecutor(parameters, testDataset);
            

            int[] targetsOfInterest = new int[] { 5555 };

            targetsOfInterest = new int[] { 5905 };   //throwing error in Chromcorr

            //targetsOfInterest = new int[]{6110};

            targetsOfInterest = new int[]
                                    {
                                        5555, 5677, 5746, 5905, 6110, 6496, 7039, 7116, 7220, 7229, 7370, 7585, 8338, 8491, 8517, 8616, 8618,
                                        8715, 8947, 8958, 8968, 9024, 9159, 9240, 9242, 9261, 9328, 9441, 9474, 9506, 9519, 9583, 9792, 9944,
                                        9965, 10223, 10251, 10329, 10649, 10673, 11249, 11367, 11523, 11677, 11912, 12178, 12304, 12383, 12395,
                                        12492, 12517, 12692, 12700, 12828, 13443, 13590, 13740, 14090, 14256
                                    };

            executor.Targets.TargetList =
                (executor.Targets.TargetList.Where(n => targetsOfInterest.Contains(n.ID))).ToList();

            executor.Execute();

            SipperTargetedWorkflow workflow = executor.TargetedWorkflow as SipperTargetedWorkflow;
            workflow.WorkflowParameters.SaveParametersToXML(
                @"\\protoapps\DataPkgs\Public\2012\601_Sipper_paper_data_processing_and_analysis\Parameters\SipperTargetedWorkflowParameters_Sum5 - copy.xml");


            SipperLcmsTargetedResult result = workflow.Result as SipperLcmsTargetedResult;

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.ChromCorrelationData);
            Assert.IsNotNull(result.ChromCorrelationData.CorrelationDataItems);

            Assert.IsTrue(result.ChromCorrelationData.CorrelationDataItems.Count > 0);


            //foreach (var dataItem in result.ChromCorrelationData.CorrelationDataItems)
            //{
            //    Console.WriteLine(dataItem.CorrelationRSquaredVal);
            //}

            //foreach (var fitScoreDataItem in workflow.FitScoreData)
            //{
            //    Console.WriteLine(fitScoreDataItem.Key + "\t" + fitScoreDataItem.Value);
            //}


        }

        [Test]
        public void ExecuteSipperOnMSGFOutputTest1()
        {
            string paramFile =
                @"\\protoapps\DataPkgs\Public\2013\788_Sipper_C13_Analysis_Hot_Lake_SNC_Ana_preliminary\Parameters\ExecutorParameters1.xml";


            var parameters = new BasicTargetedWorkflowExecutorParameters();
            parameters.LoadParameters(paramFile);
            parameters.CopyRawFileLocal = false;
            //parameters.FolderPathForCopiedRawDataset = @"D:\data\temp";


            string testDataset =
                @"D:\Data\Sipper\HLP_Ana\HLP_Ana_SIP_02_19APR13_Frodo_12-12-04.raw";


            string outputParameterFile = Path.GetDirectoryName(paramFile) + Path.DirectorySeparatorChar +
                                         Path.GetFileNameWithoutExtension(paramFile) + " - copy.xml";
            parameters.SaveParametersToXML(outputParameterFile);


            
            var executor = new BasicTargetedWorkflowExecutor(parameters, testDataset);


            int[] targetsOfInterest = new int[] { 5555 };

            targetsOfInterest = new int[] { 5905 };   //throwing error in Chromcorr

            //targetsOfInterest = new int[]{6110};

            //targetsOfInterest = new int[]
            //                        {
            //                            5555, 5677, 5746, 5905, 6110, 6496, 7039, 7116, 7220, 7229, 7370, 7585, 8338, 8491, 8517, 8616, 8618,
            //                            8715, 8947, 8958, 8968, 9024, 9159, 9240, 9242, 9261, 9328, 9441, 9474, 9506, 9519, 9583, 9792, 9944,
            //                            9965, 10223, 10251, 10329, 10649, 10673, 11249, 11367, 11523, 11677, 11912, 12178, 12304, 12383, 12395,
            //                            12492, 12517, 12692, 12700, 12828, 13443, 13590, 13740, 14090, 14256
            //                        };

            //executor.Targets.TargetList =
            //    (executor.Targets.TargetList.Where(n => targetsOfInterest.Contains(n.ID))).ToList();

            executor.Targets.TargetList = executor.Targets.TargetList.Take(10).ToList();


            executor.Execute();

            SipperTargetedWorkflow workflow = executor.TargetedWorkflow as SipperTargetedWorkflow;
            

            SipperLcmsTargetedResult result = workflow.Result as SipperLcmsTargetedResult;

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.ChromCorrelationData);
            Assert.IsNotNull(result.ChromCorrelationData.CorrelationDataItems);

            Assert.IsTrue(result.ChromCorrelationData.CorrelationDataItems.Count > 0);


            //foreach (var dataItem in result.ChromCorrelationData.CorrelationDataItems)
            //{
            //    Console.WriteLine(dataItem.CorrelationRSquaredVal);
            //}

            //foreach (var fitScoreDataItem in workflow.FitScoreData)
            //{
            //    Console.WriteLine(fitScoreDataItem.Key + "\t" + fitScoreDataItem.Value);
            //}


        }


        [Test]
        public void loadLCMSFeaturesNotIdentifiedAndProcess()
        {
            string paramFile =
                @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\SIPPER_standard_testing\SipperExecutorParams_importedFeaturesWithEmpFormula.xml";

            SipperWorkflowExecutorParameters parameters = new SipperWorkflowExecutorParameters();
            parameters.LoadParameters(paramFile);

            parameters.TargetsToFilterOn = String.Empty;

            parameters.TargetsFilePath =
                @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\SIPPER_standard_testing\Yellow_C13_070_23Mar10_Griffin_10-01-28_select_unidentified_LCMSFeatures.txt";

            parameters.ReferenceDataForTargets = String.Empty;

            string testDataset =
                @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\SIPPER_standard_testing\Yellow_C13_070_23Mar10_Griffin_10-01-28.raw";

            SipperWorkflowExecutor executor = new SipperWorkflowExecutor(parameters, testDataset);

            Assert.IsTrue(executor.ExecutorParameters.WorkflowType ==
                          Globals.TargetedWorkflowTypes.SipperWorkflowExecutor1);

            executor.Execute();
        }


        [Test]
        public void loadLCMSFeaturesNotIdentifiedAndProcess2()
        {
            string paramFile =
                @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\SIPPER_standard_testing\SipperExecutorParams_importedFeaturesWithEmpFormula.xml";

            SipperWorkflowExecutorParameters parameters = new SipperWorkflowExecutorParameters();
            parameters.LoadParameters(paramFile);

            parameters.TargetsToFilterOn = String.Empty;

            parameters.TargetsFilePath =
                @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\SIPPER_standard_testing\Yellow_C13_070_23Mar10_Griffin_10-01-28_select_unidentified_LCMSFeatures.txt";

            parameters.ReferenceDataForTargets = String.Empty;

            string testDataset =
                @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\SIPPER_standard_testing\Yellow_C13_070_23Mar10_Griffin_10-01-28.raw";

            SipperWorkflowExecutor executor = new SipperWorkflowExecutor(parameters, testDataset);

            Assert.IsTrue(executor.ExecutorParameters.WorkflowType ==
                          Globals.TargetedWorkflowTypes.SipperWorkflowExecutor1);

            executor.Execute();
        }







        [Test]
        public void loadPreviousResultsAndReProcess()
        {
            string paramFile =
                @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\SIPPER_standard_testing\SipperExecutorParams_importedFeaturesWithEmpFormula.xml";

            SipperWorkflowExecutorParameters parameters = new SipperWorkflowExecutorParameters();
            parameters.LoadParameters(paramFile);

            parameters.TargetsFilePath =
                @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\SIPPER_standard_testing\Results\Yellow_C13_070_23Mar10_Griffin_10-01-28_copy_results.txt";

            //string exportedParameterFile =
            //    @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\SIPPER_standard_testing\SipperExecutorParams1.xml";
            //parameters.SaveParametersToXML(exportedParameterFile);

            string testDataset =
                @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\SIPPER_standard_testing\Yellow_C13_070_23Mar10_Griffin_10-01-28.raw";

            SipperWorkflowExecutor executor = new SipperWorkflowExecutor(parameters, testDataset);
            

            Assert.IsTrue(executor.ExecutorParameters.WorkflowType ==
                          Globals.TargetedWorkflowTypes.SipperWorkflowExecutor1);

            executor.Execute();
        }




        [Test]
        public void tempUpdateLCMSFeaturesWithEmpiricalFormulaTest1()
        {
            string testLCMSFile1 =
                @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\SIPPER_standard_testing\Yellow_C13_070_23Mar10_Griffin_10-01-28_allWithIDs_LCMSFeatures.txt";

            string massTagFileName =
                @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\SIPPER_standard_testing\Yellow_C13_070_23Mar10_Griffin_10-01-28_MassTags.txt";


            LcmsTargetFromFeaturesFileImporter importer =
              new LcmsTargetFromFeaturesFileImporter(testLCMSFile1);

            var features = importer.Import();


            MassTagFromTextFileImporter mtImporter = new MassTagFromTextFileImporter(massTagFileName);
            var massTags = mtImporter.Import();

            massTags.TargetList = (from n in massTags.TargetList
                                   group n by new
                                   {
                                       n.ID,
                                       n.ChargeState
                                   }
                                       into grp
                                       select grp.First()).ToList();


            var massTagIDList = features.TargetList.Select(p => (long)((LcmsFeatureTarget)p).FeatureToMassTagID).ToList();




            string outputLCMSFeaturesFile =
                @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\SIPPER_standard_testing\Yellow_C13_070_23Mar10_Griffin_10-01-28_withEmpFormulas_LCMSFeatures.txt";

            using (StreamWriter writer = new StreamWriter(outputLCMSFeaturesFile))
            {
                string headerLine = "TargetID\tScan\tMonoIsotopicMass\tChargeState\tMatchedMassTagID\tEmpiricalFormula";
                writer.WriteLine(headerLine);

                foreach (var targetBase in features.TargetList)
                {
                    UpdateTargetsUsingMassTagInfo(massTagIDList, features, massTags);
                    StringBuilder stringBuilder = new StringBuilder();
                    stringBuilder.Append(targetBase.ID);
                    stringBuilder.Append("\t");
                    stringBuilder.Append(targetBase.ScanLCTarget);
                    stringBuilder.Append("\t");
                    stringBuilder.Append(targetBase.MonoIsotopicMass);
                    stringBuilder.Append("\t");
                    stringBuilder.Append(targetBase.ChargeState);
                    stringBuilder.Append("\t");
                    stringBuilder.Append(((LcmsFeatureTarget) targetBase).FeatureToMassTagID);
                    stringBuilder.Append("\t");
                    stringBuilder.Append(targetBase.EmpiricalFormula);
                    writer.WriteLine(stringBuilder.ToString());
                }

            }



        }


        private void UpdateTargetsUsingMassTagInfo(List<long> massTagIDList, TargetCollection features, TargetCollection massTags)
        {
            foreach (LcmsFeatureTarget target in features.TargetList)
            {
                if (String.IsNullOrEmpty(target.EmpiricalFormula))
                {
                    if (massTagIDList.Contains(target.FeatureToMassTagID))
                    {
                        var mt = massTags.TargetList.First(p => p.ID == target.FeatureToMassTagID);

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
                    }
                }
            }
        }





        [Test]
        public void test2()
        {
            string paramFile =
                @"\\protoapps\UserData\Slysz\Data\Yellowstone\SIPPER\SipperExecutorParams1.xml";

            SipperWorkflowExecutorParameters parameters = new SipperWorkflowExecutorParameters();
            parameters.LoadParameters(paramFile);
           
            string testDataset =
               @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\SIPPER_standard_testing\Yellow_C13_070_23Mar10_Griffin_10-01-28.raw";

            SipperWorkflowExecutor executor = new SipperWorkflowExecutor(parameters, testDataset);


            Assert.IsTrue(executor.ExecutorParameters.WorkflowType ==
                          Globals.TargetedWorkflowTypes.SipperWorkflowExecutor1);

            executor.Execute();
        }





        [Test]
        public void dbTest1()
        {
            string[] datasetNames = new string[]
                                        {
                                            "Yellow_C13_068_30Mar10_Griffin_10-01-28",
                                            "Yellow_C13_064_30Mar10_Griffin_10-03-01",
                                            "Yellow_C13_063_30Mar10_Griffin_10-01-13"
                                        };

            DatasetUtilities utilities = new DatasetUtilities();
            var path=   utilities.GetDatasetPath(datasetNames[0]);

            Console.WriteLine(path);

        }


        [Test]
        public void ExecuteSipper_FindAssociatedTargetsFile()
        {
            string paramFile =
                @"\\protoapps\DataPkgs\Public\2012\601_Sipper_paper_data_processing_and_analysis\Parameters\SipperExecutorParams1.xml";

            SipperWorkflowExecutorParameters parameters = new SipperWorkflowExecutorParameters();
            parameters.LoadParameters(paramFile);
            parameters.DbName = String.Empty;
            parameters.DbServer = String.Empty;
            parameters.DbTableName = String.Empty;

            parameters.CopyRawFileLocal = false;

            //parameters.ReferenceDataForTargets =
            //    @"\\protoapps\DataPkgs\Public\2012\601_Sipper_paper_data_processing_and_analysis\Targets\massTag_reference_file.txt";

            parameters.TargetsBaseFolder =
                @"\\protoapps\DataPkgs\Public\2012\601_Sipper_paper_data_processing_and_analysis\Targets";


            string testDataset =
                @"\\protoapps\DataPkgs\Public\2012\601_Sipper_paper_data_processing_and_analysis\RawData\Yellow_C13_070_23Mar10_Griffin_10-01-28.raw";


            string outputParameterFile = Path.GetDirectoryName(paramFile) + Path.DirectorySeparatorChar +
                                         Path.GetFileNameWithoutExtension(paramFile) + " - copy.xml";
            parameters.SaveParametersToXML(outputParameterFile);


            SipperWorkflowExecutor executor = new SipperWorkflowExecutor(parameters, testDataset);
            executor.Execute();


        }


        [Test]
        public void ExecuteSipper_UsingSpecifiedTargets()
        {
            string paramFile =
                @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\SIPPER_standard_testing\SipperExecutorParams1.xml";

            SipperWorkflowExecutorParameters parameters = new SipperWorkflowExecutorParameters();
            parameters.LoadParameters(paramFile);
            parameters.DbName = String.Empty;
            parameters.DbServer = String.Empty;
            parameters.DbTableName = String.Empty;

            parameters.CopyRawFileLocal = false;

            parameters.ReferenceDataForTargets = @"\\protoapps\DataPkgs\Public\2012\548_YSIP_C12_C13_data_analysis\massTag_reference_file.txt";
          
            parameters.TargetsFilePath = @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\SIPPER_standard_testing\Targets\Yellow_C13_070_23Mar10_Griffin_10-01-28_10practice_targets.txt";

            string testDataset = @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\SIPPER_standard_testing\Yellow_C13_070_23Mar10_Griffin_10-01-28.raw";

            SipperWorkflowExecutor executor = new SipperWorkflowExecutor(parameters, testDataset);
            executor.Execute();


        }


        [Test]
        public void ExecuteSipper_UsingTargetsWithEmpiricalFormulaOnly()
        {
            string paramFile =
                @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\SIPPER_standard_testing\SipperExecutorParams1.xml";

            SipperWorkflowExecutorParameters parameters = new SipperWorkflowExecutorParameters();
            parameters.LoadParameters(paramFile);
            parameters.DbName = String.Empty;
            parameters.DbServer = String.Empty;
            parameters.DbTableName = String.Empty;

            parameters.CopyRawFileLocal = false;

            parameters.TargetsFilePath =
                @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\SIPPER_standard_testing\Targets\Yellow_C13_070_23Mar10_Griffin_10-01-28_10practice_targets_empFormula_noMonoMass.txt";

            
            string testDataset =
               @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\SIPPER_standard_testing\Yellow_C13_070_23Mar10_Griffin_10-01-28.raw";

            SipperWorkflowExecutor executor = new SipperWorkflowExecutor(parameters, testDataset);
            executor.Execute();


        }

        [Test]
        public void temp_ExecuteSipper()
        {
            string paramFile =
                @"\\protoapps\DataPkgs\Public\2012\601_Sipper_paper_data_processing_and_analysis\Parameters\SipperExecutorParams1.xml";

            SipperWorkflowExecutorParameters parameters = new SipperWorkflowExecutorParameters();
            parameters.LoadParameters(paramFile);

            parameters.FolderPathForCopiedRawDataset = @"D:\data\temp";


            string testDataset =
                @"\\protoapps\DataPkgs\Public\2012\601_Sipper_paper_data_processing_and_analysis\RawData\Yellow_C13_070_23Mar10_Griffin_10-01-28.raw";


            string outputParameterFile = Path.GetDirectoryName(paramFile) + Path.DirectorySeparatorChar +
                                         Path.GetFileNameWithoutExtension(paramFile) + " - copy.xml";
            parameters.SaveParametersToXML(outputParameterFile);


            SipperWorkflowExecutor executor = new SipperWorkflowExecutor(parameters, testDataset);
            executor.Execute();


        }



    }
}
