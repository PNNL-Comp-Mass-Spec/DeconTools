using System;
using DeconTools.Backend.Core;
using DeconTools.Backend.Runs;
using NUnit.Framework;

namespace DeconTools.UnitTesting2.Run_relatedTests
{
    [TestFixture]
    public class BrukerMaxisRun_Tests
    {
        private int _testScan = 479;

        [Test]
        public void constructorTest1()
        {
            string path =
                @"\\protoapps\UserData\Slysz\DeconTools_TestFiles\Bruker\Bruker_Maxis\2012_05_15_MN9_A_000001.d";

            BrukerTOF run = new BrukerTOF(path);

            Assert.IsNotNull(run);
            Assert.AreEqual("2012_05_15_MN9_A_000001", run.DatasetName);
            Assert.AreEqual(path, run.DataSetPath);

        }

        [Test]
        public void GetNumMSTest1()
        {
            string path =
                @"\\protoapps\UserData\Slysz\DeconTools_TestFiles\Bruker\Bruker_Maxis\2012_05_15_MN9_A_000001.d";

            BrukerTOF run = new BrukerTOF(path);

            Assert.AreEqual(1131, run.GetNumMSScans());
        }

        [Test]
        public void GetFirstAndLastMassSpectrumTest1()
        {
            string path =
                @"\\protoapps\UserData\Slysz\DeconTools_TestFiles\Bruker\Bruker_Maxis\2012_05_15_MN9_A_000001.d";

            BrukerTOF run = new BrukerTOF(path);

            run.GetMassSpectrum(new ScanSet(1));

            run.GetMassSpectrum(new ScanSet(run.GetNumMSScans()));

            Assert.AreEqual(1, run.GetMinPossibleScanNum());
            Assert.AreEqual(1131, run.GetMaxPossibleScanNum());

        }


        [Test]
        public void GetMassSpectrumTest1()
        {
            string path =
             @"\\protoapps\UserData\Slysz\DeconTools_TestFiles\Bruker\Bruker_Maxis\2012_05_15_MN9_A_000001.d";

            BrukerTOF run = new BrukerTOF(path);

            ScanSet scan = new ScanSet(_testScan);


            run.GetMassSpectrum(scan);

            TestUtilities.DisplayXYValues(run.XYData);

            run.Close();

            run = null;
            GC.Collect();

        }


        [Test]
        public void GetRetentionTimeTest1()
        {
            string path =
           @"\\protoapps\UserData\Slysz\DeconTools_TestFiles\Bruker\Bruker_Maxis\2012_05_15_MN9_A_000001.d";


            BrukerTOF run = new BrukerTOF(path);

            //for (int i = 478; i < 600; i++)
            //{
            //    Console.WriteLine(i + "\t" + run.GetTime(i));

            //}

            Assert.AreEqual(32.328, (decimal)run.GetTime(478));


        }


        [Test]
        public void GetMSLevelTest1()
        {
            string path =
             @"\\protoapps\UserData\Slysz\DeconTools_TestFiles\Bruker\Bruker_Maxis\2012_05_15_MN9_A_000001.d";


            BrukerTOF run = new BrukerTOF(path);

            for (int i = 478; i < 600; i++)
            {
                Console.WriteLine(i + "\t" + run.GetMSLevel(i));

            }
            Assert.AreEqual(1, run.GetMSLevel(_testScan));
        }
    }
}
