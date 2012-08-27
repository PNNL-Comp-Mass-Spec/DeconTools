using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using DeconTools.Backend.Utilities.IsotopeDistributionCalculation.MercuryIsotopicDistribution;
using DeconTools.Workflows.Backend;
using DeconTools.Workflows.Backend.Core;

namespace TestConsole1
{
    class Program
    {
        public static string sarcUIMFFile1 = @"D:\Data\UIMF\SmartSumming\Sarc_MS_90_21Aug10_Cheetah_10-08-02_0000.uimf";
        //public static string bsaUIMFFile1 = @"\\protoapps\UserData\Shah\TestFiles\BSA_0pt01_2_20Sep10_Cheetah_10-08-05_0000 - Copy.uimf";


        static void Main(string[] args)
        {

            /*
            WholisticChromBasedLCMSFeatureFinderWorkflow workflow = new WholisticChromBasedLCMSFeatureFinderWorkflow();
            int minScan = 200;
            int maxScan = 18500;

            //int minScan = 5500;
            //int maxScan = 6500;


            Run run = new XCaliburRun(FileRefs.RawDataMSFiles.OrbitrapStdFile1);
            PeakImporterFromText peakImporter = new DeconTools.Backend.Data.PeakImporterFromText(FileRefs.PeakDataFiles.OrbitrapPeakFile1);
            peakImporter.ImportPeaks(run.ResultCollection.MSPeakResultList);

            run.ResultCollection.MSPeakResultList = run.ResultCollection.MSPeakResultList.Where(p => p.Scan_num > minScan && p.Scan_num < maxScan).ToList();
            //run.ResultCollection.MSPeakResultList = run.ResultCollection.MSPeakResultList.Where(p => p.Scan_num > minScan && p.Scan_num < maxScan && p.MSPeak.XValue > 771 && p.MSPeak.XValue < 775).ToList();


            workflow.ExecuteWorkflow2(run);
            // * */

            //string masterPeaksFilepath = @"\\protoapps\UserData\Shah\TestFiles\Sarc_MS_90_21Aug10_Cheetah_10-08-02_0000_peaksDesc.txt";
            //Run run = new UIMFRun(@"D:\Data\UIMF\Sarc\the_10_testDatasets\Sarc_MS_90_21Aug10_Cheetah_10-08-02_0000.uimf");

            //IMS_SmartFeatureFinderWorkflow workflow = new IMS_SmartFeatureFinderWorkflow(run, masterPeaksFilepath);
            //workflow.Execute();

            //testMercury();

            testTopDownTargetedWorkflow();

            //TestErnestosData1();
        }


        public static void TestErnestosData1()
        {

            string baseFolder = @"\\protoapps\DataPkgs\Public\2012\573_O16O18_Data_analysis_of_ubiquintated_peptides";

            string outputFolder = @"D:\Temp\TargetedTesting";

            BasicTargetedWorkflowExecutorParameters executorParameters = new BasicTargetedWorkflowExecutorParameters();
            executorParameters.TargetsBaseFolder = baseFolder + Path.DirectorySeparatorChar + "Targets";
            executorParameters.WorkflowParameterFile = baseFolder + Path.DirectorySeparatorChar + "Parameters" + Path.DirectorySeparatorChar + "O16O18WorkflowParameters_2011_08_23_sum5.xml";

            executorParameters.TargetType = Globals.TargetType.LcmsFeature;
            executorParameters.ResultsFolder = outputFolder + Path.DirectorySeparatorChar + "Results\\Testing";
            executorParameters.LoggingFolder = outputFolder + Path.DirectorySeparatorChar + "Logs";

            string testDatasetPath = @"\\protoapps\UserData\Slysz\Data\O16O18\Ernesto\PSI_LRW_18O_02A_18Jun12_Falcon_12-03-34.RAW";
            testDatasetPath = @"D:\Data\O16O18\Ernesto\PSI_LRW_1to1_03A_6Jul12_Falcon_12-06-04.raw";

            string outputtedParameterFile =
                @"\\protoapps\DataPkgs\Public\2012\573_O16O18_Data_analysis_of_ubiquintated_peptides\Parameters\ExecutorParameters1.xml";

            //executorParameters.SaveParametersToXML(outputtedParameterFile);


            TargetedWorkflowExecutor executor = new BasicTargetedWorkflowExecutor(executorParameters, testDatasetPath);
            executor.Execute();


        }



        public static void testTopDownTargetedWorkflow()
        {
            string copyToFolder = @"D:\Data\TopDown";

            string outputFolder = @"\\protoapps\UserData\Slysz\DeconTools_TestFiles\TargetedWorkflowTesting\Output";

            var executorParameters = new TopDownTargetedWorkflowExecutorParameters();
            executorParameters.CopyRawFileLocal = true;
            executorParameters.FolderPathForCopiedRawDataset = copyToFolder;
            executorParameters.LoggingFolder = outputFolder;
            executorParameters.ResultsFolder = outputFolder;
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



        private static void testMercury()
        {
            var mercury = new MercuryIsoDistCreator2();
            mercury.Resolution = 100000;
            var iso = mercury.GetIsotopePattern("C66H114N20O21S2", 2);    //Peptide 'SAMPLERSAMPLER'

            var timeVals = new List<long>();

            int numIterations = 200;
            Stopwatch sw = new Stopwatch();
            sw.Start();

            for (int i = 0; i < numIterations; i++)
            {
                iso = mercury.GetIsotopePattern("C66H114N20O21S2", 2);
            }
            sw.Stop();
            Console.WriteLine(sw.ElapsedMilliseconds / (double)numIterations);
        }
    }
}
