using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using NUnit.Framework;
using DeconTools.Backend;
using System.IO;
using DeconTools.Backend.Data;
using DeconTools.Backend.Core;

namespace DeconTools.UnitTesting2.ProjectControllerTests
{
    [TestFixture]
    public class OldSchoolsProcRunnerTests
    {
        [Test]
        public void processYAFMSFile1()
        {
            OldSchoolProcRunner oldSchool = new OldSchoolProcRunner(FileRefs.RawDataMSFiles.YAFMSStandardFile2, Globals.MSFileType.YAFMS, FileRefs.ParameterFiles.YAFMSParameterFileScans4000_4050);
            oldSchool.Execute();
        }


        [Test]
        public void processOrbitrapData1()
        {
            string testFile = FileRefs.RawDataMSFiles.OrbitrapStdFile1;
            string expectedIsosOutput = Path.GetDirectoryName(testFile) + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(testFile) + "_isos.csv";

            if (File.Exists(expectedIsosOutput))
            {
                File.Delete(expectedIsosOutput);
            }

            OldSchoolProcRunner oldSchool = new OldSchoolProcRunner(FileRefs.RawDataMSFiles.OrbitrapStdFile1, Globals.MSFileType.Finnigan, FileRefs.ParameterFiles.Orbitrap_Scans6000_6050ParamFile);
            oldSchool.Execute();

            Assert.That(File.Exists(expectedIsosOutput));
            IsosImporter importer = new IsosImporter(expectedIsosOutput, Globals.MSFileType.Finnigan);

            List<IsosResult> results = new List<IsosResult>();
            results = importer.Import();

            TestUtilities.DisplayMSFeatures(results);

            Assert.AreEqual(1340, results.Count);
            Assert.AreEqual(2006580356, results.Sum(p => p.IsotopicProfile.IntensityAggregate));

        }


        [Test]
        public void processBruker12TSolarixFile1()
        {
            string testFile = FileRefs.RawDataMSFiles.BrukerSolarix12TFile1;

            DirectoryInfo dirInfo = new DirectoryInfo(testFile);
            string datasetName = dirInfo.Name;

            string expectedIsosOutput = FileRefs.RawDataMSFiles.BrukerSolarix12TFile1 + Path.DirectorySeparatorChar + datasetName + "_isos.csv";

            if (File.Exists(expectedIsosOutput))
            {
                File.Delete(expectedIsosOutput);
            }

            OldSchoolProcRunner oldSchool = new OldSchoolProcRunner(testFile, Globals.MSFileType.Bruker, FileRefs.ParameterFiles.Bruker12TSolarixScans4_8ParamFile);
            oldSchool.Execute();

            Assert.That(File.Exists(expectedIsosOutput));
        }


        [Test]
        public void processBruker9T()
        {
            string testFile = FileRefs.RawDataMSFiles.Bruker9TStandardFile2;

            DirectoryInfo dirInfo = new DirectoryInfo(testFile);
            string datasetName = dirInfo.Name;

            string expectedIsosOutput = testFile + Path.DirectorySeparatorChar + datasetName + "_isos.csv";

            if (File.Exists(expectedIsosOutput))
            {
                File.Delete(expectedIsosOutput);
            }

            OldSchoolProcRunner oldSchool = new OldSchoolProcRunner(testFile, Globals.MSFileType.Bruker, FileRefs.ParameterFiles.Bruker9T_Scans1000_1010ParamFile);
            oldSchool.Execute();

            Assert.That(File.Exists(expectedIsosOutput));
        }

        [Test]
        public void processUIMF_Frames800_802()
        {
            string testFile = FileRefs.RawDataMSFiles.UIMFStdFile1;
            string expectedIsosOutput = Path.GetDirectoryName(testFile) + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(testFile) + "_isos.csv";


            if (File.Exists(expectedIsosOutput))
            {
                File.Delete(expectedIsosOutput);
            }

            OldSchoolProcRunner oldSchool = new OldSchoolProcRunner(testFile, Globals.MSFileType.PNNL_UIMF, FileRefs.ParameterFiles.UIMFFrames800_802);
            oldSchool.Execute();

            List<IsosResult> results = new List<IsosResult>();

            Assert.That(File.Exists(expectedIsosOutput));
            IsosImporter importer = new IsosImporter(expectedIsosOutput, Globals.MSFileType.PNNL_UIMF);
            results =importer.Import();

            Assert.AreEqual(4709, results.Count);
            Assert.AreEqual(21756127, results.Sum(p => p.IsotopicProfile.IntensityAggregate));
        }


      
        [Test]
        public void processUIMF_Frames800_802_SumAllIMSScansPerFrame()
        {
            string testFile = FileRefs.RawDataMSFiles.UIMFStdFile1;
            string parameterFile = FileRefs.RawDataBasePath + "\\ParameterFiles\\UIMF_frames_peakBR7_800-802_OneSpectrumPerFrame.xml";

            string expectedIsosOutput = Path.GetDirectoryName(testFile) + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(testFile) + "_isos.csv";

            if (File.Exists(expectedIsosOutput))
            {
                File.Delete(expectedIsosOutput);
            }

            OldSchoolProcRunner oldSchool = new OldSchoolProcRunner(testFile, Globals.MSFileType.PNNL_UIMF, parameterFile);
            oldSchool.Execute();

            
            List<IsosResult>results=new List<IsosResult>();

            Assert.That(File.Exists(expectedIsosOutput));
            IsosImporter importer = new IsosImporter(expectedIsosOutput, Globals.MSFileType.PNNL_UIMF);
            results =importer.Import();

            //TestUtilities.DisplayMSFeatures(results);
            Assert.AreEqual(180, results.Count);
            Assert.AreEqual(2330765, results.Sum(p => p.IsotopicProfile.IntensityAggregate));

        }









    }
}
