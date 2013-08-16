using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using DeconTools.Backend;
using DeconTools.Backend.Core;
using DeconTools.Backend.Data;
using DeconTools.Backend.DTO;
using DeconTools.Backend.Parameters;
using DeconTools.Backend.Runs;
using DeconTools.Backend.Workflows;
using NUnit.Framework;

namespace DeconTools.UnitTesting2.Workflow_Tests
{
    [TestFixture]
    public class ScanBasedWorkflowTests
    {
        [Test]
        public void TraditionalWorkflowTestOrbitrapData_usingNewThrash()
        {
            string parameterFile = FileRefs.ParameterFiles.Orbitrap_Scans6000_6050ParamFile;

            Run run = new RunFactory().CreateRun(FileRefs.RawDataMSFiles.OrbitrapStdFile1);
            string expectedIsosFile = run.DataSetPath + Path.DirectorySeparatorChar + run.DatasetName + "_isos.csv";
            string expectedScansFile = run.DataSetPath + Path.DirectorySeparatorChar + run.DatasetName + "_scans.csv";
            string expectedPeaksFile = run.DataSetPath + Path.DirectorySeparatorChar + run.DatasetName + "_peaks.txt";

            if (File.Exists(expectedIsosFile)) File.Delete(expectedIsosFile);
            if (File.Exists(expectedScansFile)) File.Delete(expectedScansFile);
         //   if (File.Exists(expectedPeaksFile)) File.Delete(expectedPeaksFile);

            var parameters = new DeconToolsParameters();
            parameters.LoadFromOldDeconToolsParameterFile(parameterFile);
            parameters.ThrashParameters.UseThrashV1 = false;

            parameters.MSGeneratorParameters.MinLCScan = 1;// run.GetMinPossibleLCScanNum();
            parameters.MSGeneratorParameters.MaxLCScan = 6005;// run.GetMaxPossibleLCScanNum();
            

            var workflow = ScanBasedWorkflow.CreateWorkflow(run, parameters);
            workflow.Execute();

            Assert.IsTrue(File.Exists(expectedIsosFile), "Isos file was not created.");
            Assert.IsTrue(File.Exists(expectedScansFile), "Scans file was not created.");
            Assert.IsTrue(File.Exists(expectedPeaksFile), "Peaks file was not created.");

            IsosImporter isosImporter = new IsosImporter(expectedIsosFile, run.MSFileType);
            var isos = isosImporter.Import();

            //Assert.AreEqual(186, isos.Count);
            //TODO: still report
            PeakImporterFromText peakImporter = new PeakImporterFromText(expectedPeaksFile);

            List<MSPeakResult> peaklist = new List<MSPeakResult>();
            peakImporter.ImportPeaks(peaklist);

           // Assert.AreEqual(809, peaklist.Count);

            var sumIntensities = isos.Select(p => p.IntensityAggregate).Sum();
           // Assert.AreEqual(266185816d, Math.Round(sumIntensities));

            var sumPeakIntensities = peaklist.Select(p => p.Height).Sum();
           // Assert.AreEqual(605170496.0f, sumPeakIntensities);

        }



        [Category("MustPass")]
        [Test]
        public void TraditionalWorkflowTestOrbitrapData1()
        {
            string parameterFile = FileRefs.ParameterFiles.Orbitrap_Scans6000_6050ParamFile;

            Run run = new RunFactory().CreateRun(FileRefs.RawDataMSFiles.OrbitrapStdFile1);
            string expectedIsosFile = run.DataSetPath + Path.DirectorySeparatorChar + run.DatasetName + "_isos.csv";
            string expectedScansFile = run.DataSetPath + Path.DirectorySeparatorChar + run.DatasetName + "_scans.csv";
            string expectedPeaksFile = run.DataSetPath + Path.DirectorySeparatorChar + run.DatasetName + "_peaks.txt";

            if (File.Exists(expectedIsosFile)) File.Delete(expectedIsosFile);
            if (File.Exists(expectedScansFile)) File.Delete(expectedScansFile);
            if (File.Exists(expectedPeaksFile)) File.Delete(expectedPeaksFile);

            var parameters=new DeconToolsParameters();
            parameters.LoadFromOldDeconToolsParameterFile(parameterFile);
            parameters.ScanBasedWorkflowParameters.DeconvolutionType = Globals.DeconvolutionType.ThrashV1;


            parameters.MSGeneratorParameters.MinLCScan = 6005;
            parameters.MSGeneratorParameters.MaxLCScan = 6005;

           
            var workflow = ScanBasedWorkflow.CreateWorkflow(run, parameters);
            workflow.Execute();

            Assert.IsTrue(File.Exists(expectedIsosFile), "Isos file was not created.");
            Assert.IsTrue(File.Exists(expectedScansFile), "Scans file was not created.");
            Assert.IsTrue(File.Exists(expectedPeaksFile), "Peaks file was not created.");
            
            IsosImporter isosImporter = new IsosImporter(expectedIsosFile, run.MSFileType);
            var isos = isosImporter.Import();

            Assert.AreEqual(186, isos.Count);

            PeakImporterFromText peakImporter = new PeakImporterFromText(expectedPeaksFile);

            List<MSPeakResult> peaklist = new List<MSPeakResult>();
            peakImporter.ImportPeaks(peaklist);

            Assert.AreEqual(809, peaklist.Count);

            var sumIntensities = isos.Select(p => p.IntensityAggregate).Sum();
            Assert.AreEqual(266185816d, Math.Round(sumIntensities));

            var sumPeakIntensities = peaklist.Select(p => p.Height).Sum();
            Assert.AreEqual(605170496.0f, sumPeakIntensities);

        }


        [Category("MustPass")]
        [Test]
        public void TraditionalWorkflowTestOrbitrapData_InformedThrash()
        {
            string parameterFile = FileRefs.ParameterFiles.Orbitrap_Scans6000_6050ParamFile;

            Run run = new RunFactory().CreateRun(FileRefs.RawDataMSFiles.OrbitrapStdFile1);
            string expectedIsosFile = run.DataSetPath + Path.DirectorySeparatorChar + run.DatasetName + "_isos.csv";
            string expectedScansFile = run.DataSetPath + Path.DirectorySeparatorChar + run.DatasetName + "_scans.csv";
            string expectedPeaksFile = run.DataSetPath + Path.DirectorySeparatorChar + run.DatasetName + "_peaks.txt";

            if (File.Exists(expectedIsosFile)) File.Delete(expectedIsosFile);
            if (File.Exists(expectedScansFile)) File.Delete(expectedScansFile);
            //if (File.Exists(expectedPeaksFile)) File.Delete(expectedPeaksFile);

            var parameters = new DeconToolsParameters();
            parameters.LoadFromOldDeconToolsParameterFile(parameterFile);
            parameters.ScanBasedWorkflowParameters.DeconvolutionType = Globals.DeconvolutionType.ThrashV2;
            
            parameters.MSGeneratorParameters.MinLCScan = 6005;
            parameters.MSGeneratorParameters.MaxLCScan = 6050;


            var workflow = ScanBasedWorkflow.CreateWorkflow(run, parameters);
            workflow.Execute();

            Assert.IsTrue(File.Exists(expectedIsosFile), "Isos file was not created.");
            Assert.IsTrue(File.Exists(expectedScansFile), "Scans file was not created.");
            Assert.IsTrue(File.Exists(expectedPeaksFile), "Peaks file was not created.");

            IsosImporter isosImporter = new IsosImporter(expectedIsosFile, run.MSFileType);
            var isos = isosImporter.Import();

            Assert.AreEqual(1287, isos.Count);

            var sumIntensities = isos.Select(p => p.IntensityAggregate).Sum();
            Assert.AreEqual(1973657234m,(decimal)Math.Round(sumIntensities));
        }


        [Test]
        public void TraditionalWorkflowTestOrbitrapData_useThrashV1()
        {
            string parameterFile = FileRefs.ParameterFiles.Orbitrap_Scans6000_6050ParamFile;

            Run run = new RunFactory().CreateRun(FileRefs.RawDataMSFiles.OrbitrapStdFile1);
            string expectedIsosFile = run.DataSetPath + Path.DirectorySeparatorChar + run.DatasetName + "_isos.csv";
            string expectedScansFile = run.DataSetPath + Path.DirectorySeparatorChar + run.DatasetName + "_scans.csv";
            string expectedPeaksFile = run.DataSetPath + Path.DirectorySeparatorChar + run.DatasetName + "_peaks.txt";

            if (File.Exists(expectedIsosFile)) File.Delete(expectedIsosFile);
            if (File.Exists(expectedScansFile)) File.Delete(expectedScansFile);
            if (File.Exists(expectedPeaksFile)) File.Delete(expectedPeaksFile);

            var parameters = new DeconToolsParameters();
            parameters.LoadFromOldDeconToolsParameterFile(parameterFile);
            parameters.ScanBasedWorkflowParameters.DeconvolutionType = Globals.DeconvolutionType.ThrashV1;

            parameters.MSGeneratorParameters.MinLCScan = 6005;
            parameters.MSGeneratorParameters.MaxLCScan = 6005;


            var workflow = ScanBasedWorkflow.CreateWorkflow(run, parameters);
            workflow.Execute();

            Assert.IsTrue(File.Exists(expectedIsosFile), "Isos file was not created.");
            Assert.IsTrue(File.Exists(expectedScansFile), "Scans file was not created.");
            Assert.IsTrue(File.Exists(expectedPeaksFile), "Peaks file was not created.");

            IsosImporter isosImporter = new IsosImporter(expectedIsosFile, run.MSFileType);
            var isos = isosImporter.Import();

            Assert.AreEqual(186, isos.Count);

            PeakImporterFromText peakImporter = new PeakImporterFromText(expectedPeaksFile);

            List<MSPeakResult> peaklist = new List<MSPeakResult>();
            peakImporter.ImportPeaks(peaklist);

            Assert.AreEqual(809, peaklist.Count);

            var sumIntensities = isos.Select(p => p.IntensityAggregate).Sum();
            //Assert.AreEqual(263499300d, Math.Round(sumIntensities));

            var sumPeakIntensities = peaklist.Select(p => p.Height).Sum();
            //Assert.AreEqual(605170496.0f, sumPeakIntensities);

        }


        [Test]
        public void NegativeIonModeDeisotoping_useThrashV1()
        {
            string parameterFile =
                @"\\protoapps\UserData\Slysz\DeconTools_TestFiles\ParameterFiles\LTQ_Orb\LTQ_Orb_SN2_PeakBR2_PeptideBR1_NegIon_Thrash_Sum3.xml";

            string testFile =
                @"\\protoapps\UserData\Slysz\DeconTools_TestFiles\Orbitrap\NegativeIonMode\AC2_Neg_highpH_14Apr13_Sauron_13-04-03.raw";


            Run run = new RunFactory().CreateRun(testFile);
            string expectedIsosFile = run.DataSetPath + Path.DirectorySeparatorChar + run.DatasetName + "_isos.csv";
            string expectedScansFile = run.DataSetPath + Path.DirectorySeparatorChar + run.DatasetName + "_scans.csv";
            string expectedPeaksFile = run.DataSetPath + Path.DirectorySeparatorChar + run.DatasetName + "_peaks.txt";

            if (File.Exists(expectedIsosFile)) File.Delete(expectedIsosFile);
            if (File.Exists(expectedScansFile)) File.Delete(expectedScansFile);
            if (File.Exists(expectedPeaksFile)) File.Delete(expectedPeaksFile);

            var parameters = new DeconToolsParameters();
            parameters.LoadFromOldDeconToolsParameterFile(parameterFile);
            parameters.ScanBasedWorkflowParameters.DeconvolutionType =Globals.DeconvolutionType.ThrashV1;

            parameters.MSGeneratorParameters.MinLCScan = 4100;
            parameters.MSGeneratorParameters.MaxLCScan = 4100;


            var workflow = ScanBasedWorkflow.CreateWorkflow(run, parameters);
            workflow.Execute();

            Assert.IsTrue(File.Exists(expectedIsosFile), "Isos file was not created.");
            Assert.IsTrue(File.Exists(expectedScansFile), "Scans file was not created.");
            Assert.IsTrue(File.Exists(expectedPeaksFile), "Peaks file was not created.");

            IsosImporter isosImporter = new IsosImporter(expectedIsosFile, run.MSFileType);
            var isos = isosImporter.Import();

            var testIso =
                (from n in isos where n.IsotopicProfile.MonoPeakMZ > 744 && n.IsotopicProfile.MonoPeakMZ < 749 select n).FirstOrDefault();

            Assert.IsNotNull(testIso, "Test iso not found.");

            Console.WriteLine("monomass= " + testIso.IsotopicProfile.MonoIsotopicMass);
            Assert.AreEqual(1491.32852m, (decimal) Math.Round(testIso.IsotopicProfile.MonoIsotopicMass, 5));


        }




        [Test]
        public void TraditionalWorkflowTestOrbitrapData_useThrashV1_test2()
        {
            string parameterFile =
                @"\\protoapps\UserData\Slysz\DeconTools_TestFiles\ParameterFiles\LTQ_Orb\LTQ_Orb_SN2_PeakBR1pt3_PeptideBR1_Thrash_scan6000_9000 - oldThrash.xml";

            Run run = new RunFactory().CreateRun(FileRefs.RawDataMSFiles.OrbitrapStdFile1);
            string expectedIsosFile = run.DataSetPath + Path.DirectorySeparatorChar + run.DatasetName + "_isos.csv";
            string expectedScansFile = run.DataSetPath + Path.DirectorySeparatorChar + run.DatasetName + "_scans.csv";
            string expectedPeaksFile = run.DataSetPath + Path.DirectorySeparatorChar + run.DatasetName + "_peaks.txt";

            if (File.Exists(expectedIsosFile)) File.Delete(expectedIsosFile);
            if (File.Exists(expectedScansFile)) File.Delete(expectedScansFile);
            if (File.Exists(expectedPeaksFile)) File.Delete(expectedPeaksFile);

            var parameters = new DeconToolsParameters();
            parameters.LoadFromOldDeconToolsParameterFile(parameterFile);

            //parameters.MSGeneratorParameters.MinLCScan = 6005;
            //parameters.MSGeneratorParameters.MaxLCScan = 6005;


            var workflow = ScanBasedWorkflow.CreateWorkflow(run, parameters);
            workflow.Execute();

            Assert.IsTrue(File.Exists(expectedIsosFile), "Isos file was not created.");
            Assert.IsTrue(File.Exists(expectedScansFile), "Scans file was not created.");
            Assert.IsTrue(File.Exists(expectedPeaksFile), "Peaks file was not created.");

            IsosImporter isosImporter = new IsosImporter(expectedIsosFile, run.MSFileType);
            var isos = isosImporter.Import();

            Assert.AreEqual(186, isos.Count);

            PeakImporterFromText peakImporter = new PeakImporterFromText(expectedPeaksFile);

            List<MSPeakResult> peaklist = new List<MSPeakResult>();
            peakImporter.ImportPeaks(peaklist);

            Assert.AreEqual(809, peaklist.Count);

            var sumIntensities = isos.Select(p => p.IntensityAggregate).Sum();
            //Assert.AreEqual(263499300d, Math.Round(sumIntensities));

            var sumPeakIntensities = peaklist.Select(p => p.Height).Sum();
            //Assert.AreEqual(605170496.0f, sumPeakIntensities);

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

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            workflow.Execute();
            stopwatch.Stop();

            double typicalTimeInSeconds = 11.0;
            double currentTimeInSeconds = Math.Round(stopwatch.ElapsedMilliseconds/(double) 1000, 1);

            Console.WriteLine("Typical processing time (sec)= "+ typicalTimeInSeconds);
            Console.WriteLine("Current Processing time (sec) = " + currentTimeInSeconds);

            double percentDiff = (currentTimeInSeconds - typicalTimeInSeconds)/typicalTimeInSeconds*100;

            Assert.IsTrue(percentDiff < 20, "Processing failed time test. Too slow.");
            Assert.That(File.Exists(expectedIsosOutput));
            Assert.That(File.Exists(expectedPeaksFileOutput));

            Console.WriteLine(percentDiff);
            

           

            IsosImporter importer = new IsosImporter(expectedIsosOutput, Globals.MSFileType.Finnigan);

            List<IsosResult> results = new List<IsosResult>();
            results = importer.Import();

            //TestUtilities.DisplayMSFeatures(results);

            //Assert.AreEqual(1340, results.Count);
            //Assert.AreEqual(2006580356, results.Sum(p => p.IntensityAggregate));

           


        }


        [Test]
        public void processOrbitrapData_outputMS2_and_peaks_test1()
        {
            string testFile = FileRefs.RawDataMSFiles.OrbitrapStdFile1;
            string parameterFile = @"\\protoapps\UserData\Slysz\DeconTools_TestFiles\ParameterFiles\LTQ_Orb\LTQ_Orb_SN2_PeakBR1pt3_PeptideBR1_Thrash_scans6000_6050_MS2.xml";

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

//            TestUtilities.DisplayMSFeatures(results);

            //Assert.AreEqual(1340, results.Count);
            //Assert.AreEqual(2006580356, results.Sum(p => p.IntensityAggregate));
        }

        
        [Test]
        public void processUIMF_demultiplexedUIMF()
        {
            string testFile = FileRefs.RawDataMSFiles.UIMFStdFile3;
            //string parameterFile = FileRefs.RawDataBasePath + "\\ParameterFiles\\IMS_UIMF_PeakBR4_PeptideBR4_SN3_SumScans3_NoLCSum_Frame_500-501.xml";
            var parameterFile =
                @"\\protoapps\UserData\Slysz\DeconTools_TestFiles\ParameterFiles\IMS\IMS_UIMF_PeakBR4_PeptideBR4_SN3_SumScans3_NoLCSum_Frame_500-501.xml";

            string expectedIsosOutput = Path.GetDirectoryName(testFile) + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(testFile) + "_isos.csv";
            string expectedScansOutput = Path.GetDirectoryName(testFile) + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(testFile) + "_scans.csv";

            string expectedPeaksOutput = Path.GetDirectoryName(testFile) + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(testFile) + "_peaks.txt";




            if (File.Exists(expectedIsosOutput))
            {
                File.Delete(expectedIsosOutput);
            }

            if (File.Exists(expectedScansOutput))
            {
                File.Delete(expectedScansOutput);
            }

            if (File.Exists(expectedPeaksOutput))
            {
                File.Delete(expectedPeaksOutput);
            }

            var workflow = ScanBasedWorkflow.CreateWorkflow(testFile, parameterFile);
            workflow.NewDeconToolsParameters.ScanBasedWorkflowParameters.ExportPeakData=true;
            workflow.Execute();


            List<IsosResult> results = new List<IsosResult>();

            Assert.That(File.Exists(expectedIsosOutput));
            Assert.That(File.Exists(expectedScansOutput));
            Assert.That(File.Exists(expectedPeaksOutput));


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
           // Assert.AreEqual(1573, results.Count);
            Assert.IsTrue(results.Sum(p => p.IntensityAggregate) > 1e6);
            //Assert.AreEqual(109217766, results.Sum(p => p.IntensityAggregate));

            var testResult1 = results[0] as UIMFIsosResult;

            //Assert.AreEqual(9.476, (decimal)testResult1.DriftTime);


        }


        [Test]
        public void processUIMF_Frames800_802()
        {
            string testFile = FileRefs.RawDataMSFiles.UIMFStdFile3;
            
            string expectedIsosOutput = Path.GetDirectoryName(testFile) + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(testFile) + "_isos.csv";

            if (File.Exists(expectedIsosOutput))
            {
                File.Delete(expectedIsosOutput);
            }

            DeconToolsParameters parameters = new DeconToolsParameters();
            parameters.MSGeneratorParameters.MinLCScan = 800;
            parameters.MSGeneratorParameters.MaxLCScan = 802;
            parameters.MSGeneratorParameters.UseLCScanRange = true;

            parameters.MiscMSProcessingParameters.UseZeroFilling = true;

            Run run = new RunFactory().CreateRun(testFile);

            var workflow = ScanBasedWorkflow.CreateWorkflow(run,parameters);
            workflow.Execute();

            List<IsosResult> results = new List<IsosResult>();

            Assert.That(File.Exists(expectedIsosOutput));
            IsosImporter importer = new IsosImporter(expectedIsosOutput, Globals.MSFileType.PNNL_UIMF);
            results = importer.Import();

            TestUtilities.DisplayMSFeatures(results);
            //Assert.AreEqual(189, results.Count);
            //Assert.AreEqual(62294623, (int)results.Sum(p => p.IntensityAggregate));

        }




        [Test]
        public void processUIMF_Frames800_802_SumAllIMSScansPerFrame()
        {
            string testFile = FileRefs.RawDataMSFiles.UIMFStdFile3;
            string parameterFile = FileRefs.RawDataBasePath + "\\ParameterFiles\\UIMF_frames_peakBR7_800-802_OneSpectrumPerFrame.xml";

            Run run = new RunFactory().CreateRun(testFile);


            string expectedIsosOutput = Path.GetDirectoryName(testFile) + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(testFile) + "_isos.csv";

            if (File.Exists(expectedIsosOutput))
            {
                File.Delete(expectedIsosOutput);
            }

            DeconToolsParameters parameters = new DeconToolsParameters();
            parameters.MSGeneratorParameters.MinLCScan = 800;
            parameters.MSGeneratorParameters.MaxLCScan = 802;
            parameters.MSGeneratorParameters.UseLCScanRange = true;
            parameters.MSGeneratorParameters.SumAllSpectra = true;
            parameters.MiscMSProcessingParameters.UseZeroFilling = true;
            



            var workflow = ScanBasedWorkflow.CreateWorkflow(run, parameters);
            workflow.Execute();

            List<IsosResult> results = new List<IsosResult>();

            Assert.That(File.Exists(expectedIsosOutput));
            IsosImporter importer = new IsosImporter(expectedIsosOutput, Globals.MSFileType.PNNL_UIMF);
            results = importer.Import();

            //TestUtilities.DisplayMSFeatures(results);
            //Assert.AreEqual(189, results.Count);
            //Assert.AreEqual(62294623, (int)results.Sum(p => p.IntensityAggregate));

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

            var parameters = new DeconToolsParameters();
            parameters.MSGeneratorParameters.NumLCScansToSum = 3;
            parameters.MSGeneratorParameters.NumImsScansToSum = 9;
            parameters.PeakDetectorParameters.PeakToBackgroundRatio = 4;
            parameters.MiscMSProcessingParameters.UseZeroFilling = true;
            parameters.ScanBasedWorkflowParameters.ProcessMS2 = true;
            parameters.MSGeneratorParameters.UseLCScanRange = true;
            parameters.MSGeneratorParameters.SumSpectraAcrossLC = true;
            parameters.MSGeneratorParameters.MinLCScan = 1;    //min frame
            parameters.MSGeneratorParameters.MaxLCScan = 15;    //max frame

           
            var workflow = ScanBasedWorkflow.CreateWorkflow(run, parameters);
            workflow.Execute();

            List<IsosResult> results = new List<IsosResult>();
            Assert.That(File.Exists(expectedIsosOutput));
            IsosImporter importer = new IsosImporter(expectedIsosOutput, Globals.MSFileType.PNNL_UIMF);
            results = importer.Import();

            //TestUtilities.DisplayMSFeatures(results);
            //Assert.AreEqual(36078, results.Count);
            //Assert.AreEqual(1224247916, (int)results.Sum(p => p.IntensityAggregate));
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
        public void ProcessBruker15TSolarixFile1_sumAllScans()
        {
            string testFile = FileRefs.RawDataMSFiles.Bruker15TFile1;

            DirectoryInfo dirInfo = new DirectoryInfo(testFile);
            string datasetName = dirInfo.Name;

            string expectedIsosOutput = FileRefs.RawDataMSFiles.Bruker15TFile1 + Path.DirectorySeparatorChar + datasetName + "_isos.csv";

            if (File.Exists(expectedIsosOutput))
            {
                File.Delete(expectedIsosOutput);
            }

            Run run = new RunFactory().CreateRun(testFile);

            var parameters = new DeconToolsParameters();
            parameters.MSGeneratorParameters.SumAllSpectra = true;
            parameters.PeakDetectorParameters.PeakToBackgroundRatio = 15;

            var workflow = new TraditionalScanBasedWorkflow(parameters, run);
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

        [Test]
        public void ProcessMZ5File()
        {


            string testFile =
                @"\\protoapps\UserData\Slysz\DeconTools_TestFiles\mzXML\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18.mz5";
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
            Assert.AreEqual(2006580356, results.Sum(p => p.IntensityAggregate));
        }

        [Ignore("Not currently used")]
        [Test]
        public void ProcessAgilentCentroidedFile()
        {
            string testFile =
             @"\\protoapps\UserData\Nikola\DDD_Milk\D6.1.forExpRepAnal_3.14.2012.d";
            string parameterFile = @"\\protoapps\UserData\Nikola\DDD_Milk\agilTOF_Normal_SavGolSmooth_2007-08-16_DEFAULT.xml";


            var workflow = ScanBasedWorkflow.CreateWorkflow(testFile, parameterFile);
            workflow.Execute();
            //return;

            string expectedIsosOutput=null;
            IsosImporter importer = new IsosImporter(expectedIsosOutput, Globals.MSFileType.Finnigan);

            List<IsosResult> results = new List<IsosResult>();
            results = importer.Import();

            TestUtilities.DisplayMSFeatures(results);

            Assert.AreEqual(1340, results.Count);
            Assert.AreEqual(2006580356, results.Sum(p => p.IsotopicProfile.IntensityMostAbundant));
        }

    }
}
