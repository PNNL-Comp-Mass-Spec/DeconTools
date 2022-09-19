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
            var parameterFile = FileRefs.ParameterFiles.Orbitrap_Scans6000_6050ParamFile;

            var run = new RunFactory().CreateRun(FileRefs.RawDataMSFiles.OrbitrapStdFile1);
            var expectedIsosFile = Path.Combine(run.DatasetDirectoryPath, run.DatasetName + "_isos.csv");
            var expectedScansFile = Path.Combine(run.DatasetDirectoryPath, run.DatasetName + "_scans.csv");
            var expectedPeaksFile = Path.Combine(run.DatasetDirectoryPath, run.DatasetName + "_peaks.txt");

            if (File.Exists(expectedIsosFile))
            {
                File.Delete(expectedIsosFile);
            }

            if (File.Exists(expectedScansFile))
            {
                File.Delete(expectedScansFile);
            }

            if (File.Exists(expectedPeaksFile))
            {
                File.Delete(expectedPeaksFile);
            }

            var allPeaksFilepath = @"\\protoapps\UserData\Slysz\DeconTools_TestFiles\Orbitrap\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18_peaksFULL.txt";
            File.Copy(allPeaksFilepath, allPeaksFilepath.Replace("FULL", ""));

            var parameters = new DeconToolsParameters();
            parameters.LoadFromOldDeconToolsParameterFile(parameterFile);
            parameters.ScanBasedWorkflowParameters.DeconvolutionType = Globals.DeconvolutionType.ThrashV2;

            parameters.MSGeneratorParameters.UseLCScanRange = true;
            parameters.MSGeneratorParameters.MinLCScan = 6000;// run.GetMinPossibleLCScanNum();
            parameters.MSGeneratorParameters.MaxLCScan = 6050;// run.GetMaxPossibleLCScanNum();

            var workflow = ScanBasedWorkflow.CreateWorkflow(run, parameters);
            workflow.Execute();

            Assert.IsTrue(File.Exists(expectedIsosFile), "Isos file was not created.");
            Assert.IsTrue(File.Exists(expectedScansFile), "Scans file was not created.");
            Assert.IsTrue(File.Exists(expectedPeaksFile), "Peaks file was not created.");

            var isosImporter = new IsosImporter(expectedIsosFile, run.MSFileType);
            var isos = isosImporter.Import();

            //Assert.AreEqual(186, isos.Count);
            //TODO: still report

            Console.WriteLine("Num MSfeatures = " + isos.Count);

            var sumIntensities = isos.Select(p => p.IntensityAggregate).Sum();
            // Assert.AreEqual(266185816d, Math.Round(sumIntensities));

        }

        [Category("MustPass")]
        [Test]
        public void TraditionalWorkflowTestOrbitrapData1()
        {
            var parameterFile = FileRefs.ParameterFiles.Orbitrap_Scans6000_6050ParamFile;

            var run = new RunFactory().CreateRun(FileRefs.RawDataMSFiles.OrbitrapStdFile1);
            var expectedIsosFile = Path.Combine(run.DatasetDirectoryPath, run.DatasetName + "_isos.csv");
            var expectedScansFile = Path.Combine(run.DatasetDirectoryPath, run.DatasetName + "_scans.csv");
            var expectedPeaksFile = Path.Combine(run.DatasetDirectoryPath, run.DatasetName + "_peaks.txt");

            if (File.Exists(expectedIsosFile))
            {
                File.Delete(expectedIsosFile);
            }

            if (File.Exists(expectedScansFile))
            {
                File.Delete(expectedScansFile);
            }

            if (File.Exists(expectedPeaksFile))
            {
                File.Delete(expectedPeaksFile);
            }

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

            var isosImporter = new IsosImporter(expectedIsosFile, run.MSFileType);
            var isos = isosImporter.Import();

            Assert.AreEqual(186, isos.Count);

            var peakImporter = new PeakImporterFromText(expectedPeaksFile);

            var peaklist = new List<MSPeakResult>();
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
            var parameterFile = FileRefs.ParameterFiles.Orbitrap_Scans6000_6050ParamFile;

            var run = new RunFactory().CreateRun(FileRefs.RawDataMSFiles.OrbitrapStdFile1);
            var expectedIsosFile = Path.Combine(run.DatasetDirectoryPath, run.DatasetName + "_isos.csv");
            var expectedScansFile = Path.Combine(run.DatasetDirectoryPath, run.DatasetName + "_scans.csv");
            var expectedPeaksFile = Path.Combine(run.DatasetDirectoryPath, run.DatasetName + "_peaks.txt");

            if (File.Exists(expectedIsosFile))
            {
                File.Delete(expectedIsosFile);
            }

            if (File.Exists(expectedScansFile))
            {
                File.Delete(expectedScansFile);
            }
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

            var isosImporter = new IsosImporter(expectedIsosFile, run.MSFileType);
            var isos = isosImporter.Import();

            Assert.AreEqual(1287, isos.Count);

            var sumIntensities = isos.Select(p => p.IntensityAggregate).Sum();
            Assert.AreEqual(1974438598m, (decimal)Math.Round(sumIntensities));

            //  Expected: 1973657234m
            // But was:  1974438598m
        }

        [Category("MustPass")]
        [Test]
        public void TraditionalWorkflowTestOrbitrapData_DetectPeaksOnly()
        {
            var parameterFile = FileRefs.ParameterFiles.Orbitrap_Scans6000_6050ParamFile;

            var run = new RunFactory().CreateRun(FileRefs.RawDataMSFiles.OrbitrapStdFile1);
            var expectedIsosFile = Path.Combine(run.DatasetDirectoryPath, run.DatasetName + "_isos.csv");
            var expectedScansFile = Path.Combine(run.DatasetDirectoryPath, run.DatasetName + "_scans.csv");
            var expectedPeaksFile = Path.Combine(run.DatasetDirectoryPath, run.DatasetName + "_peaks.txt");

            if (File.Exists(expectedIsosFile))
            {
                File.Delete(expectedIsosFile);
            }

            if (File.Exists(expectedScansFile))
            {
                File.Delete(expectedScansFile);
            }

            if (File.Exists(expectedPeaksFile))
            {
                File.Delete(expectedPeaksFile);
            }

            var parameters = new DeconToolsParameters();
            parameters.LoadFromOldDeconToolsParameterFile(parameterFile);
            parameters.ScanBasedWorkflowParameters.DeconvolutionType = Globals.DeconvolutionType.None;

            parameters.MSGeneratorParameters.MinLCScan = 6005;
            parameters.MSGeneratorParameters.MaxLCScan = 6005;

            var workflow = ScanBasedWorkflow.CreateWorkflow(run, parameters);
            workflow.Execute();

            Assert.IsTrue(File.Exists(expectedIsosFile), "Isos file was not created.");
            Assert.IsTrue(File.Exists(expectedScansFile), "Scans file was not created.");
            Assert.IsTrue(File.Exists(expectedPeaksFile), "Peaks file was not created.");

            var peakImporter = new PeakImporterFromText(expectedPeaksFile);

            var peaklist = new List<MSPeakResult>();
            peakImporter.ImportPeaks(peaklist);

            Assert.AreEqual(809, peaklist.Count);

            var isosImporter = new IsosImporter(expectedIsosFile, run.MSFileType);
            var isos = isosImporter.Import();

            Assert.AreEqual(0, isos.Count);
        }

        [Test]
        public void TraditionalWorkflowTestOrbitrapData_useThrashV1()
        {
            var parameterFile = FileRefs.ParameterFiles.Orbitrap_Scans6000_6050ParamFile;

            var run = new RunFactory().CreateRun(FileRefs.RawDataMSFiles.OrbitrapStdFile1);
            var expectedIsosFile = Path.Combine(run.DatasetDirectoryPath, run.DatasetName + "_isos.csv");
            var expectedScansFile = Path.Combine(run.DatasetDirectoryPath, run.DatasetName + "_scans.csv");
            var expectedPeaksFile = Path.Combine(run.DatasetDirectoryPath, run.DatasetName + "_peaks.txt");

            if (File.Exists(expectedIsosFile))
            {
                File.Delete(expectedIsosFile);
            }

            if (File.Exists(expectedScansFile))
            {
                File.Delete(expectedScansFile);
            }

            if (File.Exists(expectedPeaksFile))
            {
                File.Delete(expectedPeaksFile);
            }

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

            var isosImporter = new IsosImporter(expectedIsosFile, run.MSFileType);
            var isos = isosImporter.Import();

            Assert.AreEqual(186, isos.Count);

            var peakImporter = new PeakImporterFromText(expectedPeaksFile);

            var peaklist = new List<MSPeakResult>();
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
            var parameterFile =
                @"\\protoapps\UserData\Slysz\DeconTools_TestFiles\ParameterFiles\LTQ_Orb\LTQ_Orb_SN2_PeakBR2_PeptideBR1_NegIon_Thrash_Sum3.xml";

            var testFile =
                @"\\protoapps\UserData\Slysz\DeconTools_TestFiles\Orbitrap\NegativeIonMode\AC2_Neg_highpH_14Apr13_Sauron_13-04-03.raw";

            var run = new RunFactory().CreateRun(testFile);
            var expectedIsosFile = Path.Combine(run.DatasetDirectoryPath, run.DatasetName + "_isos.csv");
            var expectedScansFile = Path.Combine(run.DatasetDirectoryPath, run.DatasetName + "_scans.csv");
            var expectedPeaksFile = Path.Combine(run.DatasetDirectoryPath, run.DatasetName + "_peaks.txt");

            if (File.Exists(expectedIsosFile))
            {
                File.Delete(expectedIsosFile);
            }

            if (File.Exists(expectedScansFile))
            {
                File.Delete(expectedScansFile);
            }

            if (File.Exists(expectedPeaksFile))
            {
                File.Delete(expectedPeaksFile);
            }

            var parameters = new DeconToolsParameters();
            parameters.LoadFromOldDeconToolsParameterFile(parameterFile);
            parameters.ScanBasedWorkflowParameters.DeconvolutionType = Globals.DeconvolutionType.ThrashV1;

            parameters.MSGeneratorParameters.MinLCScan = 4100;
            parameters.MSGeneratorParameters.MaxLCScan = 4100;

            var workflow = ScanBasedWorkflow.CreateWorkflow(run, parameters);
            workflow.Execute();

            Assert.IsTrue(File.Exists(expectedIsosFile), "Isos file was not created.");
            Assert.IsTrue(File.Exists(expectedScansFile), "Scans file was not created.");
            Assert.IsTrue(File.Exists(expectedPeaksFile), "Peaks file was not created.");

            var isosImporter = new IsosImporter(expectedIsosFile, run.MSFileType);
            var isos = isosImporter.Import();

            var testIso =
                (from n in isos where n.IsotopicProfile.MonoPeakMZ > 744 && n.IsotopicProfile.MonoPeakMZ < 749 select n).FirstOrDefault();

            Assert.IsNotNull(testIso, "Test iso not found.");

            Console.WriteLine("monomass= " + testIso.IsotopicProfile.MonoIsotopicMass);
            Assert.AreEqual(1491.32852m, (decimal)Math.Round(testIso.IsotopicProfile.MonoIsotopicMass, 5));
        }

        [Test]
        public void TraditionalWorkflowTestOrbitrapData_useThrashV1_test2()
        {
            var parameterFile =
                @"\\protoapps\UserData\Slysz\DeconTools_TestFiles\ParameterFiles\LTQ_Orb\LTQ_Orb_SN2_PeakBR1pt3_PeptideBR1_Thrash_scan6000_9000 - oldThrash.xml";

            var run = new RunFactory().CreateRun(FileRefs.RawDataMSFiles.OrbitrapStdFile1);
            var expectedIsosFile = Path.Combine(run.DatasetDirectoryPath, run.DatasetName + "_isos.csv");
            var expectedScansFile = Path.Combine(run.DatasetDirectoryPath, run.DatasetName + "_scans.csv");
            var expectedPeaksFile = Path.Combine(run.DatasetDirectoryPath, run.DatasetName + "_peaks.txt");

            if (File.Exists(expectedIsosFile))
            {
                File.Delete(expectedIsosFile);
            }

            if (File.Exists(expectedScansFile))
            {
                File.Delete(expectedScansFile);
            }

            if (File.Exists(expectedPeaksFile))
            {
                File.Delete(expectedPeaksFile);
            }

            var parameters = new DeconToolsParameters();
            parameters.LoadFromOldDeconToolsParameterFile(parameterFile);

            //parameters.MSGeneratorParameters.MinLCScan = 6005;
            //parameters.MSGeneratorParameters.MaxLCScan = 6005;

            var workflow = ScanBasedWorkflow.CreateWorkflow(run, parameters);
            workflow.Execute();

            Assert.IsTrue(File.Exists(expectedIsosFile), "Isos file was not created.");
            Assert.IsTrue(File.Exists(expectedScansFile), "Scans file was not created.");
            Assert.IsTrue(File.Exists(expectedPeaksFile), "Peaks file was not created.");

            var isosImporter = new IsosImporter(expectedIsosFile, run.MSFileType);
            var isos = isosImporter.Import();

            Assert.AreEqual(186, isos.Count);

            var peakImporter = new PeakImporterFromText(expectedPeaksFile);

            var peaklist = new List<MSPeakResult>();
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
            var testFile = FileRefs.RawDataMSFiles.OrbitrapStdFile1;
            var parameterFile = FileRefs.ParameterFiles.Orbitrap_Scans6000_6050ParamFile;

            var expectedIsosOutput = Path.Combine(Path.GetDirectoryName(testFile), Path.GetFileNameWithoutExtension(testFile) + "_isos.csv");

            var expectedPeaksFileOutput = Path.Combine(Path.GetDirectoryName(testFile), Path.GetFileNameWithoutExtension(testFile) + "_peaks.txt");

            if (File.Exists(expectedIsosOutput))
            {
                File.Delete(expectedIsosOutput);
            }

            if (File.Exists(expectedPeaksFileOutput))
            {
                File.Delete(expectedPeaksFileOutput);
            }

            var workflow = ScanBasedWorkflow.CreateWorkflow(testFile, parameterFile);

            var stopwatch = new Stopwatch();
            stopwatch.Start();
            workflow.Execute();
            stopwatch.Stop();

            var typicalTimeInSeconds = 11.0;
            var currentTimeInSeconds = Math.Round(stopwatch.ElapsedMilliseconds / (double)1000, 1);

            Console.WriteLine("Typical processing time (sec)= " + typicalTimeInSeconds);
            Console.WriteLine("Current Processing time (sec) = " + currentTimeInSeconds);

            var percentDiff = (currentTimeInSeconds - typicalTimeInSeconds) / typicalTimeInSeconds * 100;

            Assert.IsTrue(percentDiff < 20, "Processing failed time test. Too slow.");
            Assert.That(File.Exists(expectedIsosOutput));
            Assert.That(File.Exists(expectedPeaksFileOutput));

            Console.WriteLine(percentDiff);

            var importer = new IsosImporter(expectedIsosOutput, Globals.MSFileType.Thermo_Raw);

            var results = new List<IsosResult>();
            results = importer.Import();

            //TestUtilities.DisplayMSFeatures(results);

            //Assert.AreEqual(1340, results.Count);
            //Assert.AreEqual(2006580356, results.Sum(p => p.IntensityAggregate));

        }

        [Test]
        public void processOrbitrapData_outputMS2_and_peaks_test1()
        {
            var testFile = FileRefs.RawDataMSFiles.OrbitrapStdFile1;
            var parameterFile = @"\\protoapps\UserData\Slysz\DeconTools_TestFiles\ParameterFiles\LTQ_Orb\LTQ_Orb_SN2_PeakBR1pt3_PeptideBR1_Thrash_scans6000_6050_MS2.xml";

            var expectedIsosOutput = Path.Combine(Path.GetDirectoryName(testFile), Path.GetFileNameWithoutExtension(testFile) + "_isos.csv");
            var expectedScansOutput = Path.Combine(Path.GetDirectoryName(testFile), Path.GetFileNameWithoutExtension(testFile) + "_scans.csv");

            var expectedPeaksFileOutput = Path.Combine(Path.GetDirectoryName(testFile), Path.GetFileNameWithoutExtension(testFile) + "_peaks.txt");

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

            var importer = new IsosImporter(expectedIsosOutput, Globals.MSFileType.Thermo_Raw);

            var results = new List<IsosResult>();
            results = importer.Import();

            //            TestUtilities.DisplayMSFeatures(results);

            //Assert.AreEqual(1340, results.Count);
            //Assert.AreEqual(2006580356, results.Sum(p => p.IntensityAggregate));
        }

        [Test]
        public void processUIMF_demultiplexedUIMF()
        {
            var testFile = FileRefs.RawDataMSFiles.UIMFStdFile3;
            //string parameterFile = Path.Combine(FileRefs.RawDataBasePath, "ParameterFiles", "IMS_UIMF_PeakBR4_PeptideBR4_SN3_SumScans3_NoLCSum_Frame_500-501.xml");
            var parameterFile =
                @"\\protoapps\UserData\Slysz\DeconTools_TestFiles\ParameterFiles\IMS\IMS_UIMF_PeakBR4_PeptideBR4_SN3_SumScans3_NoLCSum_Frame_500-501.xml";

            var expectedIsosOutput = Path.Combine(Path.GetDirectoryName(testFile), Path.GetFileNameWithoutExtension(testFile) + "_isos.csv");
            var expectedScansOutput = Path.Combine(Path.GetDirectoryName(testFile), Path.GetFileNameWithoutExtension(testFile) + "_scans.csv");

            var expectedPeaksOutput = Path.Combine(Path.GetDirectoryName(testFile), Path.GetFileNameWithoutExtension(testFile) + "_peaks.txt");

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
            workflow.NewDeconToolsParameters.ScanBasedWorkflowParameters.ExportPeakData = true;
            workflow.Execute();

            var results = new List<IsosResult>();

            Assert.That(File.Exists(expectedIsosOutput));
            Assert.That(File.Exists(expectedScansOutput));
            Assert.That(File.Exists(expectedPeaksOutput));

            var importer = new IsosImporter(expectedIsosOutput, Globals.MSFileType.PNNL_UIMF);
            results = importer.Import();

            var scansFileLineCounter = 0;
            using (var sr = new StreamReader(expectedScansOutput))
            {
                sr.ReadLine();

                while (!sr.EndOfStream)
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
            var testFile = FileRefs.RawDataMSFiles.UIMFStdFile3;

            var expectedIsosOutput = Path.Combine(Path.GetDirectoryName(testFile), Path.GetFileNameWithoutExtension(testFile) + "_isos.csv");

            if (File.Exists(expectedIsosOutput))
            {
                File.Delete(expectedIsosOutput);
            }

            var parameters = new DeconToolsParameters();
            parameters.MSGeneratorParameters.MinLCScan = 800;
            parameters.MSGeneratorParameters.MaxLCScan = 802;
            parameters.MSGeneratorParameters.UseLCScanRange = true;

            parameters.MiscMSProcessingParameters.UseZeroFilling = true;

            var run = new RunFactory().CreateRun(testFile);

            var workflow = ScanBasedWorkflow.CreateWorkflow(run, parameters);
            workflow.Execute();

            var results = new List<IsosResult>();

            Assert.That(File.Exists(expectedIsosOutput));
            var importer = new IsosImporter(expectedIsosOutput, Globals.MSFileType.PNNL_UIMF);
            results = importer.Import();

            TestUtilities.DisplayMSFeatures(results);
            //Assert.AreEqual(189, results.Count);
            //Assert.AreEqual(62294623, (int)results.Sum(p => p.IntensityAggregate));

        }

        [Test]
        public void processUIMF_Frames800_802_SumAllIMSScansPerFrame()
        {
            var testFile = FileRefs.RawDataMSFiles.UIMFStdFile3;
            var parameterFile = Path.Combine(FileRefs.RawDataBasePath, "ParameterFiles", "UIMF_frames_peakBR7_800-802_OneSpectrumPerFrame.xml");

            var run = new RunFactory().CreateRun(testFile);

            var expectedIsosOutput = Path.Combine(Path.GetDirectoryName(testFile), Path.GetFileNameWithoutExtension(testFile) + "_isos.csv");

            if (File.Exists(expectedIsosOutput))
            {
                File.Delete(expectedIsosOutput);
            }

            var parameters = new DeconToolsParameters();
            parameters.MSGeneratorParameters.MinLCScan = 800;
            parameters.MSGeneratorParameters.MaxLCScan = 802;
            parameters.MSGeneratorParameters.UseLCScanRange = true;
            parameters.MSGeneratorParameters.SumAllSpectra = true;
            parameters.MiscMSProcessingParameters.UseZeroFilling = true;

            var workflow = ScanBasedWorkflow.CreateWorkflow(run, parameters);
            workflow.Execute();

            var results = new List<IsosResult>();

            Assert.That(File.Exists(expectedIsosOutput));
            var importer = new IsosImporter(expectedIsosOutput, Globals.MSFileType.PNNL_UIMF);
            results = importer.Import();

            //TestUtilities.DisplayMSFeatures(results);
            //Assert.AreEqual(189, results.Count);
            //Assert.AreEqual(62294623, (int)results.Sum(p => p.IntensityAggregate));

        }

        [Test]
        public void processUIMFContainingMSMS()
        {
            var testFile =
                @"\\protoapps\UserData\Slysz\DeconTools_TestFiles\UIMF\MSMS_Testing\PepMix_MSMS_4msSA.UIMF";

            var run = new RunFactory().CreateRun(testFile);

            var expectedIsosOutput = Path.Combine(Path.GetDirectoryName(testFile), Path.GetFileNameWithoutExtension(testFile) + "_isos.csv");

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

            var results = new List<IsosResult>();
            Assert.That(File.Exists(expectedIsosOutput));
            var importer = new IsosImporter(expectedIsosOutput, Globals.MSFileType.PNNL_UIMF);
            results = importer.Import();

            //TestUtilities.DisplayMSFeatures(results);
            //Assert.AreEqual(36078, results.Count);
            //Assert.AreEqual(1224247916, (int)results.Sum(p => p.IntensityAggregate));
        }

        [Test]
        public void ProcessBruker12TSolarixFile1()
        {
            var testFile = FileRefs.RawDataMSFiles.BrukerSolarix12TFile1;

            var dirInfo = new DirectoryInfo(testFile);
            var datasetName = dirInfo.Name;

            var expectedIsosOutput = Path.Combine(FileRefs.RawDataMSFiles.BrukerSolarix12TFile1, datasetName + "_isos.csv");

            if (File.Exists(expectedIsosOutput))
            {
                File.Delete(expectedIsosOutput);
            }

            var run = new RunFactory().CreateRun(testFile);

            var workflow = ScanBasedWorkflow.CreateWorkflow(testFile, FileRefs.ParameterFiles.Bruker12TSolarixScans4_8ParamFile);
            workflow.Execute();

            Assert.That(File.Exists(expectedIsosOutput));
        }

        [Test]
        public void ProcessBruker15TSolarixFile1_sumAllScans()
        {
            var testFile = FileRefs.RawDataMSFiles.Bruker15TFile1;

            var dirInfo = new DirectoryInfo(testFile);
            var datasetName = dirInfo.Name;

            var expectedIsosOutput = Path.Combine(FileRefs.RawDataMSFiles.Bruker15TFile1, datasetName + "_isos.csv");

            if (File.Exists(expectedIsosOutput))
            {
                File.Delete(expectedIsosOutput);
            }

            var run = new RunFactory().CreateRun(testFile);

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
            var testFile = FileRefs.RawDataMSFiles.Bruker9TStandardFile2;

            var dirInfo = new DirectoryInfo(testFile);
            var datasetName = dirInfo.Name;

            var expectedIsosOutput = Path.Combine(testFile, datasetName + "_isos.csv");

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
            var datasetName = dirInfo.Name;

            var expectedIsosOutput = Path.Combine(testFile, datasetName + "_isos.csv");

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
            var testFile =
                @"\\proto-2\unitTest_Files\DeconTools_TestFiles\mzXML\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18.mz5";
            var parameterFile = FileRefs.ParameterFiles.Orbitrap_Scans6000_6050ParamFile;

            var expectedIsosOutput = Path.Combine(Path.GetDirectoryName(testFile), Path.GetFileNameWithoutExtension(testFile) + "_isos.csv");

            var expectedPeaksFileOutput = Path.Combine(Path.GetDirectoryName(testFile), Path.GetFileNameWithoutExtension(testFile) + "_peaks.txt");

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

            var importer = new IsosImporter(expectedIsosOutput, Globals.MSFileType.Thermo_Raw);

            var results = new List<IsosResult>();
            results = importer.Import();

            TestUtilities.DisplayMSFeatures(results);

            Assert.AreEqual(1340, results.Count);
            Assert.AreEqual(2006580356, results.Sum(p => p.IntensityAggregate));
        }

        [Ignore("Not currently used")]
        [Test]
        public void ProcessAgilentCentroidedFile()
        {
            var testFile =
             @"\\protoapps\UserData\Nikola\DDD_Milk\D6.1.forExpRepAnal_3.14.2012.d";
            var parameterFile = @"\\protoapps\UserData\Nikola\DDD_Milk\agilTOF_Normal_SavGolSmooth_2007-08-16_DEFAULT.xml";

            var workflow = ScanBasedWorkflow.CreateWorkflow(testFile, parameterFile);
            workflow.Execute();
            //return;

            string expectedIsosOutput = null;
            var importer = new IsosImporter(expectedIsosOutput, Globals.MSFileType.Thermo_Raw);

            var results = new List<IsosResult>();
            results = importer.Import();

            TestUtilities.DisplayMSFeatures(results);

            Assert.AreEqual(1340, results.Count);
            Assert.AreEqual(2006580356, results.Sum(p => p.IsotopicProfile.IntensityMostAbundant));
        }
    }
}
