using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend.Runs;
using DeconTools.UnitTesting2;
using NUnit.Framework;

namespace DeconTools.Testing.ProblemCases
{
    [TestFixture]
    public class OMCS_0437_AgilentTOFFileReadingProblem
    {
        [Test]
        public void Test1()
        {
            string testfile = @"\\protoapps\UserData\Nikola\DDD_Milk\D6.1.forExpRepAnal_3.14.2012.d";

            Run run = new RunFactory().CreateRun(testfile);

            TestUtilities.DisplayRunInformation(run);



        }


        [Test]
        public void FileFromZaiaTest1()
        {
            string testfile = @"D:\Data\From_Joe_Zaia\RNaseB20_2_nocal.d\RNaseB20_2_nocal.d";

            Run run = new RunFactory().CreateRun(testfile);

            TestUtilities.DisplayRunInformation(run);

            var msgen = MSGeneratorFactory.CreateMSGenerator(run.MSFileType);

            ScanSet scanSet = new ScanSet(1000);
            run.CurrentScanSet = scanSet;

            msgen.Execute(run.ResultCollection);

            TestUtilities.DisplayXYValues(run.XYData);
        }

        [Test]
        public void FileFromZaiaTest2()
        {
            string testfile = @"D:\Data\From_Joe_Zaia\RNaseB20_2.d\RNaseB20_2.d";

            Run run = new RunFactory().CreateRun(testfile);

            TestUtilities.DisplayRunInformation(run);



        }


    }
}
