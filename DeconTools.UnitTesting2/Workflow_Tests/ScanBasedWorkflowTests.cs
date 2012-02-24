﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using DeconTools.Backend;
using DeconTools.Backend.Core;
using DeconTools.Backend.Data;
using DeconTools.Backend.Runs;
using DeconTools.Backend.Workflows;
using NUnit.Framework;

namespace DeconTools.UnitTesting2.Workflow_Tests
{
    [TestFixture]
    public class ScanBasedWorkflowTests
    {
        [Test]
        public void TraditionalWorkflowTest1()
        {
            Run run = new RunFactory().CreateRun(FileRefs.RawDataMSFiles.OrbitrapStdFile1);

            var parameters=new OldDecon2LSParameters();
            parameters.HornTransformParameters.UseScanRange = true;
            parameters.HornTransformParameters.MinScan = 6005;
            parameters.HornTransformParameters.MaxScan = 6005;
            parameters.PeakProcessorParameters.WritePeaksToTextFile = true;


            var workflow = ScanBasedWorkflow.CreateWorkflow(run, parameters);
            workflow.Execute();

            
        }


        [Test]
        public void ProcessOrbitrapData1()
        {
            string testFile = FileRefs.RawDataMSFiles.OrbitrapStdFile1;
            string parameterFile = FileRefs.ParameterFiles.Orbitrap_Scans6000_6050ParamFile;

            string expectedIsosOutput = Path.GetDirectoryName(testFile) + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(testFile) + "_isos.csv";

            string expectedPeaksFileOutput = Path.GetDirectoryName(testFile) + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(testFile) + "_peaks.txt";



            if (File.Exists(expectedIsosOutput))
            {
                File.Delete(expectedIsosOutput);
            }

            if (File.Exists(expectedPeaksFileOutput))
            {
                File.Delete(expectedPeaksFileOutput);
            }

            var workflow = ScanBasedWorkflow.CreateWorkflow(testFile, parameterFile);
            workflow.Execute();

            Assert.That(File.Exists(expectedIsosOutput));
            Assert.That(File.Exists(expectedPeaksFileOutput));


            IsosImporter importer = new IsosImporter(expectedIsosOutput, Globals.MSFileType.Finnigan);

            List<IsosResult> results = new List<IsosResult>();
            results = importer.Import();

            TestUtilities.DisplayMSFeatures(results);

            Assert.AreEqual(1340, results.Count);
            Assert.AreEqual(2006580356, results.Sum(p => p.IsotopicProfile.IntensityAggregate));

        }


        [Test]
        public void processOrbitrapData_outputMS2_and_peaks_test1()
        {
            string testFile = FileRefs.RawDataMSFiles.OrbitrapStdFile1;
            string parameterFile = @"\\protoapps\UserData\Slysz\DeconTools_TestFiles\ParameterFiles\LTQ_Orb_SN2_PeakBR1pt3_PeptideBR1_Thrash_scans6000_6050_MS2.xml";

            string expectedIsosOutput = Path.GetDirectoryName(testFile) + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(testFile) + "_isos.csv";
            string expectedScansOutput = Path.GetDirectoryName(testFile) + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(testFile) + "_scans.csv";

            string expectedPeaksFileOutput = Path.GetDirectoryName(testFile) + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(testFile) + "_peaks.txt";



            if (File.Exists(expectedIsosOutput))
            {
                File.Delete(expectedIsosOutput);
            }

            if (File.Exists(expectedScansOutput))
            {
                File.Delete(expectedScansOutput);
            }


            if (File.Exists(expectedPeaksFileOutput))
            {
                File.Delete(expectedPeaksFileOutput);
            }


            var workflow = ScanBasedWorkflow.CreateWorkflow(testFile, parameterFile);
            workflow.Execute();

            Assert.That(File.Exists(expectedIsosOutput));
            Assert.That(File.Exists(expectedScansOutput));
            Assert.That(File.Exists(expectedPeaksFileOutput));


            IsosImporter importer = new IsosImporter(expectedIsosOutput, Globals.MSFileType.Finnigan);

            List<IsosResult> results = new List<IsosResult>();
            results = importer.Import();

            TestUtilities.DisplayMSFeatures(results);

            Assert.AreEqual(1340, results.Count);
            Assert.AreEqual(2006580356, results.Sum(p => p.IsotopicProfile.IntensityAggregate));
        }

        
        [Test]
        public void processUIMF_demultiplexedUIMF()
        {
            string testFile = FileRefs.RawDataMSFiles.UIMFStdFile3;
            string parameterFile = FileRefs.RawDataBasePath + "\\ParameterFiles\\IMS_UIMF_PeakBR4_PeptideBR4_SN3_SumScans3_NoLCSum_Frame_500-501.xml";

            string expectedIsosOutput = Path.GetDirectoryName(testFile) + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(testFile) + "_isos.csv";
            string expectedScansOutput = Path.GetDirectoryName(testFile) + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(testFile) + "_scans.csv";


            if (File.Exists(expectedIsosOutput))
            {
                File.Delete(expectedIsosOutput);
            }

            if (File.Exists(expectedScansOutput))
            {
                File.Delete(expectedScansOutput);
            }


            var workflow = ScanBasedWorkflow.CreateWorkflow(testFile, parameterFile);
            workflow.Execute();


            List<IsosResult> results = new List<IsosResult>();

            Assert.That(File.Exists(expectedIsosOutput));
            Assert.That(File.Exists(expectedScansOutput));

            IsosImporter importer = new IsosImporter(expectedIsosOutput, Globals.MSFileType.PNNL_UIMF);
            results = importer.Import();

            int scansFileLineCounter = 0;
            using (StreamReader sr = new StreamReader(expectedScansOutput))
            {
                sr.ReadLine();


                while (sr.Peek() != -1)
                {
                    sr.ReadLine();
                    scansFileLineCounter++;
                }

            }

            Assert.AreEqual(2, scansFileLineCounter);
            Assert.AreEqual(1573, results.Count);
            Assert.AreEqual(109217766, results.Sum(p => p.IsotopicProfile.IntensityAggregate));

        }


        [Test]
        public void processUIMF_Frames800_802_SumAllIMSScansPerFrame()
        {
            string testFile = FileRefs.RawDataMSFiles.UIMFStdFile3;
            string parameterFile = FileRefs.RawDataBasePath + "\\ParameterFiles\\UIMF_frames_peakBR7_800-802_OneSpectrumPerFrame.xml";

            string expectedIsosOutput = Path.GetDirectoryName(testFile) + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(testFile) + "_isos.csv";

            if (File.Exists(expectedIsosOutput))
            {
                File.Delete(expectedIsosOutput);
            }

            var workflow = ScanBasedWorkflow.CreateWorkflow(testFile, parameterFile);
            workflow.Execute();

            List<IsosResult> results = new List<IsosResult>();

            Assert.That(File.Exists(expectedIsosOutput));
            IsosImporter importer = new IsosImporter(expectedIsosOutput, Globals.MSFileType.PNNL_UIMF);
            results = importer.Import();

            //TestUtilities.DisplayMSFeatures(results);
            Assert.AreEqual(189, results.Count);
            Assert.AreEqual(62294623, (int)results.Sum(p => p.IsotopicProfile.IntensityAggregate));

        }

        [Test]
        public void processUIMFContainingMSMS()
        {
            string testFile =
                @"\\protoapps\UserData\Slysz\DeconTools_TestFiles\UIMF\MSMS_Testing\PepMix_MSMS_4msSA.UIMF";

            Run run = new RunFactory().CreateRun(testFile);

            string expectedIsosOutput = Path.GetDirectoryName(testFile) + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(testFile) + "_isos.csv";

            if (File.Exists(expectedIsosOutput))
            {
                File.Delete(expectedIsosOutput);
            }

            var parameters = new OldDecon2LSParameters();
            parameters.HornTransformParameters.NumFramesToSumOver = 1;
            parameters.PeakProcessorParameters.PeakBackgroundRatio = 4;
            parameters.HornTransformParameters.ZeroFill = true;
            parameters.HornTransformParameters.ProcessMSMS = true;
            parameters.HornTransformParameters.UseScanRange = true;
            parameters.HornTransformParameters.MinScan = 1;    //min frame
            parameters.HornTransformParameters.MaxScan = 2;    //max frame

           
            var workflow = ScanBasedWorkflow.CreateWorkflow(run, parameters);
            workflow.Execute();

            List<IsosResult> results = new List<IsosResult>();
            Assert.That(File.Exists(expectedIsosOutput));
            IsosImporter importer = new IsosImporter(expectedIsosOutput, Globals.MSFileType.PNNL_UIMF);
            results = importer.Import();

            //TestUtilities.DisplayMSFeatures(results);
            Assert.AreEqual(554, results.Count);
            Assert.AreEqual(3990436, (int)results.Sum(p => p.IsotopicProfile.IntensityAggregate));


        }


        [Test]
        public void ProcessBruker12TSolarixFile1()
        {
            string testFile = FileRefs.RawDataMSFiles.BrukerSolarix12TFile1;

            DirectoryInfo dirInfo = new DirectoryInfo(testFile);
            string datasetName = dirInfo.Name;

            string expectedIsosOutput = FileRefs.RawDataMSFiles.BrukerSolarix12TFile1 + Path.DirectorySeparatorChar + datasetName + "_isos.csv";

            if (File.Exists(expectedIsosOutput))
            {
                File.Delete(expectedIsosOutput);
            }

            Run run = new RunFactory().CreateRun(testFile);

            var workflow = ScanBasedWorkflow.CreateWorkflow(testFile, FileRefs.ParameterFiles.Bruker12TSolarixScans4_8ParamFile);
            workflow.Execute();

            Assert.That(File.Exists(expectedIsosOutput));
        }


        [Test]
        public void ProcessBruker9T()
        {
            string testFile = FileRefs.RawDataMSFiles.Bruker9TStandardFile2;

            DirectoryInfo dirInfo = new DirectoryInfo(testFile);
            string datasetName = dirInfo.Name;

            string expectedIsosOutput = testFile + Path.DirectorySeparatorChar + datasetName + "_isos.csv";

            if (File.Exists(expectedIsosOutput))
            {
                File.Delete(expectedIsosOutput);
            }

            var workflow = ScanBasedWorkflow.CreateWorkflow(testFile, FileRefs.ParameterFiles.Bruker9T_Scans1000_1010ParamFile);
            workflow.Execute();


            Assert.That(File.Exists(expectedIsosOutput));
        }

        [Test]
        public void processYAFMSFile1()
        {
            var testFile = FileRefs.RawDataMSFiles.YAFMSStandardFile2;
            var parameterFile = FileRefs.ParameterFiles.YAFMSParameterFileScans4000_4050;

            var dirInfo = new DirectoryInfo(testFile);
            string datasetName = dirInfo.Name;

            string expectedIsosOutput = testFile + Path.DirectorySeparatorChar + datasetName + "_isos.csv";
            if (File.Exists(expectedIsosOutput))
            {
                File.Delete(expectedIsosOutput);
            }

            var workflow = ScanBasedWorkflow.CreateWorkflow(testFile, parameterFile);
            workflow.Execute();

            //Assert.That(File.Exists(expectedIsosOutput));
        }



    }
}