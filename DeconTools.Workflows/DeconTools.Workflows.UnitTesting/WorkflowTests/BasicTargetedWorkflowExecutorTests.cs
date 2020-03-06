﻿using System;
using System.IO;
using System.Linq;
using DeconTools.Workflows.Backend;
using DeconTools.Workflows.Backend.Core;
using DeconTools.Workflows.Backend.FileIO;
using NUnit.Framework;

namespace DeconTools.Workflows.UnitTesting.WorkflowTests
{
    [TestFixture]
    [Category("Functional")]
    public class BasicTargetedWorkflowExecutorTests
    {
        private readonly string baseFolder = @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\Unlabelled";

        [Test]
        public void ParameterTest1()
        {
            var outputFileName = @"\\protoapps\UserData\Slysz\IQDemo\Parameters" + @"\IQExecutorParameters.xml";
            var executorParameters = new BasicTargetedWorkflowExecutorParameters
            {
                CopyRawFileLocal = false,
                DeleteLocalDatasetAfterProcessing = true,
                LocalDirectoryPathForCopiedRawDataset = @"\\protoapps\UserData\Slysz\IQ_Demo\RawData",
                TargetType = Globals.TargetType.DatabaseTarget,
                TargetsBaseFolder = "",
#pragma warning disable 618
                TargetedAlignmentIsPerformed = false,
#pragma warning restore 618
                TargetedAlignmentWorkflowParameterFile = @"\\protoapps\UserData\Slysz\IQ_Demo\Parameters\TargetedAlignmentWorkflowParameters1.xml",
                TargetsFilePath =
                    @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\Unlabelled\Targets\QCShew_Formic_MassTags_Bin10_first10.txt",
                WorkflowParameterFile = @"\\protoapps\UserData\Slysz\IQ_Demo\Parameters\IQParameters1.xml"
            };

            executorParameters.SaveParametersToXML(outputFileName);
        }


        [Test]
        public void DemoParameterTest1()
        {
            var outputFileName = baseFolder + @"\BasicTargetedWorkflowExecutorParameters_autoGenerated.xml";
            var executorParameters = new BasicTargetedWorkflowExecutorParameters();
            executorParameters.SaveParametersToXML(outputFileName);
        }


        [Test]
        public void LoadExecutorParametersTest1()
        {
            var inputFilename = baseFolder + @"\Parameters\BasicTargetedWorkflowExecutorParameters_forImport.xml";
            var executorParameters = new BasicTargetedWorkflowExecutorParameters();
            executorParameters.LoadParameters(inputFilename);

            Assert.AreEqual(1500,(int)executorParameters.MaxMzForDefiningChargeStateTargets);
            Assert.AreEqual(100, executorParameters.MaxNumberOfChargeStateTargetsToCreate);

        }






        [Category("MustPass")]
        [Test]
        public void targetedWorkflow_alignUsingDataFromFiles()
        {
            // https://jira.pnnl.gov/jira/browse/OMCS-714

            var executorParameters =
                new BasicTargetedWorkflowExecutorParameters
                {
                    TargetsFilePath = baseFolder + @"\Targets\QCShew_Formic_MassTags_Bin10_MT24702_Z3.txt",
#pragma warning disable 618
                    TargetedAlignmentIsPerformed = true,
#pragma warning restore 618
                    TargetedAlignmentWorkflowParameterFile = baseFolder + @"\Parameters\TargetedAlignmentWorkflowParameters1.xml"
                };

            var workflowParameters = new BasicTargetedWorkflowParameters
            {
                ChromSmootherNumPointsInSmooth = 9,
                ChromPeakDetectorPeakBR = 1,
                ChromPeakDetectorSigNoise = 3,
                ChromGenTolerance = 20,
                ChromNETTolerance = 0.025,
                MSToleranceInPPM = 20
            };

            var workflow = new BasicTargetedWorkflow(workflowParameters);

            var testDatasetPath =
                baseFolder + @"\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18.RAW";

            TargetedWorkflowExecutor executor = new BasicTargetedWorkflowExecutor(executorParameters, workflow, testDatasetPath);
            executor.Execute();

            // var expectedResultsFilename = @"QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18_results.txt";

            var result = executor.TargetedWorkflow.Result;
            Assert.IsTrue(workflow.Success);
            Assert.IsFalse(result.FailedResult);
            Assert.IsNotNull(result.ScanSet);
            Assert.IsNotNull(result.ChromPeakSelected);

            Assert.IsTrue(result.Score < 0.1);

            Assert.AreEqual(3, result.NumChromPeaksWithinTolerance);
            Assert.AreEqual(8627, (decimal)Math.Round(result.ChromPeakSelected.XValue, 0));

            //non-calibrated mass directly from mass spectrum
            Assert.AreEqual(2920.49120m, (decimal) Math.Round(result.IsotopicProfile.MonoIsotopicMass,5));

            //calibrated mass
            Assert.AreEqual(2920.50017m, (decimal) Math.Round(result.MonoIsotopicMassCalibrated,5));

            Console.WriteLine("theor monomass= \t" + result.Target.MonoIsotopicMass);
            Console.WriteLine("monomass= \t" + result.IsotopicProfile.MonoIsotopicMass);
            Console.WriteLine("calibrated monomass= \t" + result.MonoIsotopicMassCalibrated);

            Console.WriteLine("ppmError before= \t" + result.MassErrorBeforeAlignment);
            Console.WriteLine("ppmError after= \t" + result.MassErrorAfterAlignment);


            Console.WriteLine("Database NET= " + result.Target.NormalizedElutionTime);
            Console.WriteLine("Result NET= " + result.GetNET());
            Console.WriteLine("Result NET Error= " + result.GetNETAlignmentError());
            Console.WriteLine("NumChromPeaksWithinTol= " + result.NumChromPeaksWithinTolerance);

        //Dataset	TargetID	Code	EmpiricalFormula	ChargeState	Scan	ScanStart	ScanEnd	NumMSSummed	NET	NETError	NumChromPeaksWithinTol	NumQualityChromPeaksWithinTol	MonoisotopicMass	MonoisotopicMassCalibrated	MassErrorInPPM	MonoMZ	IntensityRep	FitScore	IScore	FailureType	ErrorDescription
        //QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18	24702	LLKEEGYIADYAVADEAKPELEITLK	C133H213N29O44	3	8624	8596	8659	0	0.42916	-0.009395	3	1	2920.49120	2920.50018	13.96	974.50434	7529645	0.0193	0.0000
        }




        [Category("MustPass")]
        [Test]
        public void TargetedWorkflowAlignUsingViperDataTest1()
        {
            // https://jira.pnnl.gov/jira/browse/OMCS-854

            var executorParameters =
                new BasicTargetedWorkflowExecutorParameters
                {
                    TargetsFilePath = baseFolder + @"\Targets\QCShew_Formic_MassTags_Bin10_MT24702_Z3.txt",
#pragma warning disable 618
                    TargetedAlignmentIsPerformed = true,
#pragma warning restore 618
                    TargetedAlignmentWorkflowParameterFile = baseFolder + @"\Parameters\TargetedAlignmentWorkflowParameters1.xml"
                };

            var workflowParameters = new BasicTargetedWorkflowParameters
            {
                ChromSmootherNumPointsInSmooth = 9,
                ChromPeakDetectorPeakBR = 1,
                ChromPeakDetectorSigNoise = 3,
                ChromGenTolerance = 20,
                ChromNETTolerance = 0.025,
                MSToleranceInPPM = 20
            };

            var workflow = new BasicTargetedWorkflow(workflowParameters);

            var testDatasetPath =
                baseFolder + @"\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18.RAW";

            TargetedWorkflowExecutor executor = new BasicTargetedWorkflowExecutor(executorParameters, workflow, testDatasetPath);
            executor.Execute();

            // var expectedResultsFilename = @"QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18_results.txt";

            var result = executor.TargetedWorkflow.Result;
            Assert.IsTrue(workflow.Success);
            Assert.IsFalse(result.FailedResult);
            Assert.IsNotNull(result.ScanSet);
            Assert.IsNotNull(result.ChromPeakSelected);

            Assert.IsTrue(result.Score < 0.1);

            Console.WriteLine("TargetID = \t" + result.Target.ID);
            Console.WriteLine("ChargeState = \t" + result.Target.ChargeState);
            Console.WriteLine("theor monomass= \t" + result.Target.MonoIsotopicMass);
            Console.WriteLine("theor m/z= \t" + result.Target.MZ);
            Console.WriteLine("obs monomass= \t" + result.IsotopicProfile.MonoIsotopicMass);
            Console.WriteLine("obs m/z= \t" + result.IsotopicProfile.MonoPeakMZ);
            Console.WriteLine("ppmError before= \t" + result.MassErrorBeforeAlignment);
            Console.WriteLine("ppmError after= \t" + result.MassErrorAfterAlignment);

            Assert.AreEqual(3, result.NumChromPeaksWithinTolerance);
            Assert.AreEqual(8627, (decimal)Math.Round(result.ChromPeakSelected.XValue, 0));

            //non-calibrated mass directly from mass spectrum
            Assert.AreEqual(2920.49120m, (decimal)Math.Round(result.IsotopicProfile.MonoIsotopicMass, 5));

            //calibrated mass
            //Assert.AreEqual(2920.50018m, (decimal)Math.Round(result.GetCalibratedMonoisotopicMass(), 5));

            Console.WriteLine("Database NET= " + result.Target.NormalizedElutionTime);
            Console.WriteLine("Result NET= " + result.GetNET());
            Console.WriteLine("Result NET Error= " + result.GetNETAlignmentError());
            Console.WriteLine("NumChromPeaksWithinTol= " + result.NumChromPeaksWithinTolerance);

            //Dataset	TargetID	Code	EmpiricalFormula	ChargeState	Scan	ScanStart	ScanEnd	NumMSSummed	NET	NETError	NumChromPeaksWithinTol	NumQualityChromPeaksWithinTol	MonoisotopicMass	MonoisotopicMassCalibrated	MassErrorInPPM	MonoMZ	IntensityRep	FitScore	IScore	FailureType	ErrorDescription
            //QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18	24702	LLKEEGYIADYAVADEAKPELEITLK	C133H213N29O44	3	8624	8596	8659	0	0.42916	-0.009395	3	1	2920.49120	2920.50018	13.96	974.50434	7529645	0.0193	0.0000
        }


        public void TargetedWorkflowUsingMsgfInputsTest1()
        {
            var executorParameters = new BasicTargetedWorkflowExecutorParameters
            {
                TargetsFilePath =
                    @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\Unlabelled\Targets\Yellow_C13_070_23Mar10_Griffin_10-01-28_msgfplus.tsv",
#pragma warning disable 618
                TargetedAlignmentIsPerformed = false
#pragma warning restore 618
            };

            executorParameters.SaveParametersToXML(
                @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\IQConsoleDemo\IqExecutorParameters.xml");

            var workflowParameters = new BasicTargetedWorkflowParameters
            {
                ChromSmootherNumPointsInSmooth = 9,
                ChromPeakDetectorPeakBR = 1,
                ChromPeakDetectorSigNoise = 3,
                ChromGenTolerance = 20,
                ChromNETTolerance = 0.025,
                MSToleranceInPPM = 20
            };

            workflowParameters.SaveParametersToXML(
                @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\IQConsoleDemo\IqWorkflowParameters.xml");


            var workflow = new BasicTargetedWorkflow(workflowParameters);




            var testDatasetPath =
                @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\SIPPER_standard_testing\Yellow_C13_070_23Mar10_Griffin_10-01-28.raw";

            TargetedWorkflowExecutor executor = new BasicTargetedWorkflowExecutor(executorParameters, workflow, testDatasetPath);

            executor.Targets.TargetList = executor.Targets.TargetList.Take(10).ToList();

            executor.Execute();
        }


        [Test]
        public void ExecutorTest2()
        {
            var executorParamFile = baseFolder + @"\Parameters" + "\\" + @"QCShew_OrbiStandard_workflowExecutorParameters.xml";

            var testDatasetPath = baseFolder +"\\"+ "QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18.RAW";

            var executorParameters = new BasicTargetedWorkflowExecutorParameters();
            executorParameters.LoadParameters(executorParamFile);

            TargetedWorkflowExecutor executor = new BasicTargetedWorkflowExecutor(executorParameters, testDatasetPath);
            executor.Execute();


        }

        [Test]
        public void ExecutorTest3Temp()
        {
            var executorParamFile =
                @"\\protoapps\DataPkgs\Public\2012\601_Sipper_paper_data_processing_and_analysis\Parameters\ExecutorParameters1 - Copy.xml";

            var testDatasetPath =
                @"\\protoapps\DataPkgs\Public\2012\601_Sipper_paper_data_processing_and_analysis\RawData\Yellow_C13_070_23Mar10_Griffin_10-01-28.raw";


            var executorParameters = new BasicTargetedWorkflowExecutorParameters();
            executorParameters.LoadParameters(executorParamFile);
            executorParameters.TargetType = Globals.TargetType.LcmsFeature;

            TargetedWorkflowExecutor executor = new BasicTargetedWorkflowExecutor(executorParameters, testDatasetPath);
            executor.Execute();


        }




        [Ignore("Local only")]
        [Test]
        public void targetedWorkflow_alignUsingDataFromFiles_localVersion()
        {
            //TODO: figure out result is correct
            //TODO: get MS and Chrom in Jira
            var executorParameters = new BasicTargetedWorkflowExecutorParameters
            {
                TargetsFilePath = @"C:\Users\d3x720\Documents\Data\QCShew\IQ\QCShew_Formic_MassTags_Bin10_MT24702_Z3.txt",
#pragma warning disable 618
                TargetedAlignmentIsPerformed = true
#pragma warning restore 618
            };

            var workflowParameters = new BasicTargetedWorkflowParameters
            {
                ChromSmootherNumPointsInSmooth = 9,
                ChromPeakDetectorPeakBR = 1,
                ChromPeakDetectorSigNoise = 1,
                ChromGenTolerance = 20,
                ChromNETTolerance = 0.025,
                MSToleranceInPPM = 20
            };

            var workflow = new BasicTargetedWorkflow(workflowParameters);

            var testDatasetPath =
                @"C:\Users\d3x720\Documents\Data\QCShew\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18.RAW";

            TargetedWorkflowExecutor executor = new BasicTargetedWorkflowExecutor(executorParameters, workflow, testDatasetPath);
            executor.Execute();

            // var expectedResultsFilename = @"QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18_results.txt";


            var result = executor.TargetedWorkflow.Result;

            Console.WriteLine("theor monomass= \t" + result.Target.MonoIsotopicMass);
            Console.WriteLine("monomass= \t" + result.IsotopicProfile.MonoIsotopicMass);
            Console.WriteLine("ppmError before= \t" + result.MassErrorBeforeAlignment);
            Console.WriteLine("ppmError after= \t" + result.MassErrorAfterAlignment);

            Console.WriteLine("Database NET= " + result.Target.NormalizedElutionTime);
            Console.WriteLine("Result NET= " + result.GetNET());
            Console.WriteLine("Result NET Error= " + result.GetNETAlignmentError());
            Console.WriteLine("NumChromPeaksWithinTol= " + result.NumChromPeaksWithinTolerance);

            //Dataset	MassTagID	ChargeState	Scan	ScanStart	ScanEnd	NET	NumChromPeaksWithinTol	NumQualityChromPeaksWithinTol	MonoisotopicMass	MonoMZ	IntensityRep	FitScore	IScore	FailureType

            //QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18	24702	3	8119	8112	8124	0.4172	2	1	2920.53082	974.51755	1379489	0.1136	0.0000

        }

        [Test]
        public void targetedWorkflow_noAlignment()
        {
            //TODO: fix test - it is using previous data for alignment
            //TODO: make JIRA issue so we can see chromatogram, etc.

            var executorParameterFile = baseFolder + @"\Parameters\QCShew_OrbiStandard_workflowExecutorParameters.xml";
            var executorParameters = new BasicTargetedWorkflowExecutorParameters();
            executorParameters.LoadParameters(executorParameterFile);




            var resultsFolderLocation = executorParameters.OutputDirectoryBase+ "\\Results";
            var testDatasetPath = baseFolder + @"\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18.RAW";
            var testDatasetName = "QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18";

            var expectedResultsFilename = Path.Combine(resultsFolderLocation, testDatasetName + "_results.txt");
            if (File.Exists(expectedResultsFilename))
            {
                File.Delete(expectedResultsFilename);
            }


            var rawFileInfo = new FileInfo(testDatasetPath);

            //delete alignment files
            if (rawFileInfo.DirectoryName != null)
            {
                var mzalignmentFile = Path.Combine(rawFileInfo.DirectoryName, testDatasetName + "_mzAlignment.txt");
                var netAlignmentFile = Path.Combine(rawFileInfo.DirectoryName, testDatasetName + "_netAlignment.txt");
                if (File.Exists(mzalignmentFile)) File.Delete(mzalignmentFile);
                if (File.Exists(netAlignmentFile)) File.Delete(netAlignmentFile);
            }


#pragma warning disable 618
            executorParameters.TargetedAlignmentIsPerformed = false;    //no targeted alignment
#pragma warning restore 618

            TargetedWorkflowExecutor executor = new BasicTargetedWorkflowExecutor(executorParameters, testDatasetPath);
            executor.Execute();

            Assert.IsTrue(File.Exists(expectedResultsFilename));

            var importer = new UnlabeledTargetedResultFromTextImporter(expectedResultsFilename);
            var repository = importer.Import();

            Assert.AreEqual(10, repository.Results.Count);

            var result1 = repository.Results[0];
            Assert.AreEqual(24702, result1.TargetID);
            Assert.AreEqual(3, result1.ChargeState);
            Assert.AreEqual(8637, result1.ScanLC);

            //TODO: confirm/fix this NET value
            //Assert.AreEqual(0.41724m, (decimal)Math.Round(result1.NET, 5));
            // Assert.AreEqual(0.002534m, (decimal)Math.Round(result1.NETError, 6));
            //Assert.AreEqual(974.52068m, (decimal)Math.Round(result1.MonoMZ, 5));
            //Assert.AreEqual(2920.53082m, (decimal)Math.Round(result1.MonoMass, 5));
            //Assert.AreEqual(2920.53733m, (decimal)Math.Round(result1.MonoMassCalibrated, 5));
            //Assert.AreEqual(-1.83m, (decimal)Math.Round(result1.MassErrorInPPM, 2));


        }


        [Test]
        public void AlternateConstructor_targetedWorkflowNoAlignment()
        {
            var executorParameterFile = baseFolder + @"\Parameters\QCShew_OrbiStandard_workflowExecutorParameters.xml";
            var executorParameters = new BasicTargetedWorkflowExecutorParameters();
            executorParameters.LoadParameters(executorParameterFile);
            var resultsFolderLocation = Path.Combine(executorParameters.OutputDirectoryBase, "Results");
            var testDatasetPath = baseFolder + @"\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18.RAW";
            var testDatasetName = "QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18";

            var expectedResultsFilename = Path.Combine(resultsFolderLocation, testDatasetName + "_results.txt");
            if (File.Exists(expectedResultsFilename))
            {
                File.Delete(expectedResultsFilename);
            }

            var basicTargetedWorkflowParameters = new BasicTargetedWorkflowParameters();
            var workflow = new BasicTargetedWorkflow(basicTargetedWorkflowParameters);

            TargetedWorkflowExecutor executor = new BasicTargetedWorkflowExecutor(executorParameters,
                workflow, testDatasetPath);
            executor.Execute();

            Assert.IsTrue(File.Exists(expectedResultsFilename));
        }

        [Test]
        [Category("LongRunning")]
        public void targetedWorkflow_withTargetedAlignment_test()
        {

            var executorParameterFile = baseFolder + @"\Parameters\QCShew_OrbiStandard_workflowExecutorParameters.xml";
            var executorParameters = new BasicTargetedWorkflowExecutorParameters();
            executorParameters.LoadParameters(executorParameterFile);

            var resultsFolderLocation = Path.Combine(executorParameters.OutputDirectoryBase, "Results");
            var testDatasetPath = baseFolder + @"\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18.RAW";
            var testDatasetName = "QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18";

            var expectedResultsFilename = Path.Combine(resultsFolderLocation, testDatasetName + "_results.txt");
            if (File.Exists(expectedResultsFilename))
            {
                File.Delete(expectedResultsFilename);
            }


            var rawFileInfo = new FileInfo(testDatasetPath);

            if (rawFileInfo.DirectoryName != null)
            {
                var mzalignmentFile = Path.Combine(rawFileInfo.DirectoryName, testDatasetName + "_mzAlignment.txt");
                var netAlignmentFile = Path.Combine(rawFileInfo.DirectoryName, testDatasetName + "_netAlignment.txt");
                if (File.Exists(mzalignmentFile)) File.Delete(mzalignmentFile);
                if (File.Exists(netAlignmentFile)) File.Delete(netAlignmentFile);
            }


            TargetedWorkflowExecutor executor = new BasicTargetedWorkflowExecutor(executorParameters, testDatasetPath);
            executor.Execute();

            Assert.IsTrue(File.Exists(expectedResultsFilename));

            var importer = new UnlabeledTargetedResultFromTextImporter(expectedResultsFilename);
            var repository = importer.Import();

            Assert.AreEqual(10, repository.Results.Count);

            var result1 = repository.Results[0];


            Assert.AreEqual(24702, result1.TargetID);
            Assert.AreEqual(3, result1.ChargeState);
            Assert.AreEqual(8627, result1.ScanLC);
            //Assert.AreEqual(0.41724m, (decimal)Math.Round(result1.NET, 5));
            //Assert.AreEqual(0.002534m, (decimal)Math.Round(result1.NETError, 6));
            //Assert.AreEqual(2920.53082m, (decimal)Math.Round(result1.MonoMass, 5));
            //Assert.AreEqual(2920.53879m, (decimal)Math.Round(result1.MonoMassCalibrated, 5));
            //Assert.AreEqual(-2.33m, (decimal)Math.Round(result1.MassErrorInPPM, 2));

            //Dataset	MassTagID	ChargeState	Scan	ScanStart	ScanEnd	NET	NumChromPeaksWithinTol	NumQualityChromPeaksWithinTol	MonoisotopicMass	MonoMZ	IntensityRep	FitScore	IScore	FailureType

            //QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18	24702	3	8119	8112	8124	0.4172	2	1	2920.53082	974.51755	1379489	0.1136	0.0000

        }



        [Test]
        [Category("LongRunning")]
        public void copyToLocalTest1()
        {

            var executorParameterFile =
                @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\basicTargetedWorkflowExecutorParameters_CopyToLocalTestCase2.xml";
            var datasetPath = @"\\protoapps\UserData\Slysz\DeconTools_TestFiles\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18.RAW";
            var testDatasetName = "QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18";



            var executorParameters = new BasicTargetedWorkflowExecutorParameters();
            executorParameters.LoadParameters(executorParameterFile);



            var expectedResultsFilename = Path.Combine(executorParameters.OutputDirectoryBase, "Results", testDatasetName + "_results.txt");
            if (File.Exists(expectedResultsFilename))
            {
                File.Delete(expectedResultsFilename);
            }

            TargetedWorkflowExecutor executor = new BasicTargetedWorkflowExecutor(executorParameters, datasetPath);
            executor.Execute();

            Assert.IsTrue(File.Exists(expectedResultsFilename));

            var importer = new UnlabeledTargetedResultFromTextImporter(expectedResultsFilename);
            var repository = importer.Import();

            Assert.AreEqual(10, repository.Results.Count);

            var result1 = repository.Results[2];


            Assert.AreEqual(24702, result1.TargetID);
            Assert.AreEqual(3, result1.ChargeState);
            Assert.AreEqual(8112, result1.ScanLC);




            //Dataset	MassTagID	ChargeState	Scan	ScanStart	ScanEnd	NET	NumChromPeaksWithinTol	NumQualityChromPeaksWithinTol	MonoisotopicMass	MonoMZ	IntensityRep	FitScore	IScore	FailureType

            //QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18	24702	3	8119	8112	8124	0.4172	2	1	2920.53082	974.51755	1379489	0.1136	0.0000

        }


    }
}
