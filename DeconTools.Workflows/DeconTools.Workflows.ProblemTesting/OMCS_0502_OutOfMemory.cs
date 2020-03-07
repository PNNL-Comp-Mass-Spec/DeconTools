using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DeconTools.Workflows.Backend;
using DeconTools.Workflows.Backend.Core;
using NUnit.Framework;

namespace DeconTools.Workflows.ProblemTesting
{
    [TestFixture]
    public class OMCS_0502_OutOfMemory
    {
        //see:   https://jira.pnnl.gov/jira/browse/OMCS-502

        [Test]
        public void Test1()
        {
            string copyToFolder = @"D:\Data\TopDown";

            string outputFolder = @"\\protoapps\UserData\Slysz\DeconTools_TestFiles\TargetedWorkflowTesting\Output";

            var executorParameters = new TopDownTargetedWorkflowExecutorParameters();
            executorParameters.CopyRawFileLocal = true;
            executorParameters.LocalDirectoryPathForCopiedRawDataset = copyToFolder;

            executorParameters.TargetsFilePath =
                @"\\proto-7\VOrbiETD04\2012_1\CPTAC_Peptidome_Test2_P6-5_13Jan12_Polaroid_11-10-14\MSA201202231748_Auto796393\CPTAC_Peptidome_Test2_P6-5_13Jan12_Polaroid_11-10-14_MSAlign_ResultTable.txt";

            const string testDatasetPath =
                @"\\proto-7\VOrbiETD04\2012_1\CPTAC_Peptidome_Test2_P6-5_13Jan12_Polaroid_11-10-14\CPTAC_Peptidome_Test2_P6-5_13Jan12_Polaroid_11-10-14.raw";



            var executor = new TopDownTargetedWorkflowExecutor(executorParameters, testDatasetPath);
            executor.Execute();

        }


        [Test]
        public void Test2()
        {
            string copyToFolder = @"D:\Data\TopDown";

            string outputFolder = @"\\protoapps\UserData\Slysz\DeconTools_TestFiles\TargetedWorkflowTesting\Output";

            var executorParameters = new TopDownTargetedWorkflowExecutorParameters();
            executorParameters.CopyRawFileLocal = true;
            executorParameters.LocalDirectoryPathForCopiedRawDataset = copyToFolder;
            executorParameters.OutputDirectoryBase = @"\\protoapps\UserData\Slysz\DeconTools_TestFiles\TargetedWorkflowTesting\Output";

            executorParameters.TargetType = Globals.TargetType.LcmsFeature;
            executorParameters.WorkflowParameterFile =
                @"\\proto-7\VOrbiETD01\2012_1\CPTAC_Peptidome_Test1_P1_Poroshell_03Feb12_Frodo_Poroshell300SB\MSA201202231748_Auto796395\MSAlign_Quant_Workflow_2012-07-25.xml";


            executorParameters.TargetsFilePath =
                @"\\proto-7\VOrbiETD01\2012_1\CPTAC_Peptidome_Test1_P1_Poroshell_03Feb12_Frodo_Poroshell300SB\MSA201202231748_Auto796395\CPTAC_Peptidome_Test1_P1_Poroshell_03Feb12_Frodo_Poroshell300SB_MSAlign_ResultTable.txt";

            const string testDatasetPath =
                @"\\proto-7\VOrbiETD01\2012_1\CPTAC_Peptidome_Test1_P1_Poroshell_03Feb12_Frodo_Poroshell300SB\CPTAC_Peptidome_Test1_P1_Poroshell_03Feb12_Frodo_Poroshell300SB.raw";



            var executor = new TopDownTargetedWorkflowExecutor(executorParameters, testDatasetPath);
            executor.Execute();

        }


        [Test]
        public void TestErnestosData1()
        {

            string baseFolder = @"\\protoapps\DataPkgs\Public\2012\573_O16O18_Data_analysis_of_ubiquintated_peptides";

            string outputFolder = @"D:\Temp\TargetedTesting";

            BasicTargetedWorkflowExecutorParameters executorParameters = new BasicTargetedWorkflowExecutorParameters();
            executorParameters.TargetsBaseFolder = Path.Combine(baseFolder, "Targets");
            executorParameters.WorkflowParameterFile = Path.Combine(baseFolder, "Parameters", "O16O18WorkflowParameters_2011_08_23_sum5.xml");

            executorParameters.OutputDirectoryBase = @"\\protoapps\UserData\Slysz\DeconTools_TestFiles\TargetedWorkflowTesting\Output";

            executorParameters.TargetType = Globals.TargetType.LcmsFeature;


            string testDatasetPath = @"\\protoapps\UserData\Slysz\Data\O16O18\Ernesto\PSI_LRW_18O_02A_18Jun12_Falcon_12-03-34.RAW";
            testDatasetPath = @"D:\Data\O16O18\Ernesto\PSI_LRW_1to1_03A_6Jul12_Falcon_12-06-04.raw";

            string outputtedParameterFile =
                @"\\protoapps\DataPkgs\Public\2012\573_O16O18_Data_analysis_of_ubiquintated_peptides\Parameters\ExecutorParameters1.xml";

            //executorParameters.SaveParametersToXML(outputtedParameterFile);


            TargetedWorkflowExecutor executor = new BasicTargetedWorkflowExecutor(executorParameters, testDatasetPath);
            executor.Execute();


        }


    }
}
