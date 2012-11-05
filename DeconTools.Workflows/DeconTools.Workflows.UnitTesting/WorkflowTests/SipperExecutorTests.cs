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

            parameters.FolderPathForCopiedRawDataset = @"D:\data\temp";


            string testDataset =
                @"\\protoapps\DataPkgs\Public\2012\601_Sipper_paper_data_processing_and_analysis\RawData\Yellow_C13_070_23Mar10_Griffin_10-01-28.raw";


            string outputParameterFile = Path.GetDirectoryName(paramFile) + Path.DirectorySeparatorChar +
                                         Path.GetFileNameWithoutExtension(paramFile) + " - copy.xml";
            parameters.SaveParametersToXML(outputParameterFile);


            SipperWorkflowExecutor executor = new SipperWorkflowExecutor(parameters, testDataset);


            int[] targetsOfInterest = new int[]{5555, 11985};

            executor.Targets.TargetList =
                (executor.Targets.TargetList.Where(n => targetsOfInterest.Contains(n.ID))).ToList();

            executor.Execute();

            SipperTargetedWorkflow workflow = executor.TargetedWorkflow as SipperTargetedWorkflow;



            SipperLcmsTargetedResult result = workflow.Result as SipperLcmsTargetedResult;

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.ChromCorrelationData);
            Assert.IsNotNull(result.ChromCorrelationData.CorrelationDataItems);

            Assert.IsTrue(result.ChromCorrelationData.CorrelationDataItems.Count > 0);


            foreach (var dataItem in result.ChromCorrelationData.CorrelationDataItems)
            {
                Console.WriteLine(dataItem.CorrelationRSquaredVal);
            }
            

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
