using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using NUnit.Framework;
using DeconTools.Backend;
using System.IO;

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

            Assert.That(File.Exists(expectedIsosOutput));

        }








    }
}
