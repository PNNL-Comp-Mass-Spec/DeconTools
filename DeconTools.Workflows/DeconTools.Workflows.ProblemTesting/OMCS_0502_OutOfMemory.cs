using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
            executorParameters.FolderPathForCopiedRawDataset = copyToFolder;
            executorParameters.LoggingFolder = outputFolder;
            executorParameters.ResultsFolder = outputFolder;
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
            executorParameters.FolderPathForCopiedRawDataset = copyToFolder;
            executorParameters.LoggingFolder = outputFolder;
            executorParameters.ResultsFolder = outputFolder;
            executorParameters.TargetsFilePath =
                @"\\proto-7\VOrbiETD01\2012_1\CPTAC_Peptidome_Test1_P1_Poroshell_03Feb12_Frodo_Poroshell300SB\MSA201202231748_Auto796395\CPTAC_Peptidome_Test1_P1_Poroshell_03Feb12_Frodo_Poroshell300SB_MSAlign_ResultTable.txt";

            const string testDatasetPath =
                @"\\proto-7\VOrbiETD01\2012_1\CPTAC_Peptidome_Test1_P1_Poroshell_03Feb12_Frodo_Poroshell300SB\CPTAC_Peptidome_Test1_P1_Poroshell_03Feb12_Frodo_Poroshell300SB.raw";



            var executor = new TopDownTargetedWorkflowExecutor(executorParameters, testDatasetPath);
            executor.Execute();

        }


    }
}
