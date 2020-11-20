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
            var path =
                @"\\protoapps\UserData\Slysz\DeconTools_TestFiles\Bruker\Bruker_Maxis\2012_05_15_MN9_A_000001.d";

            var run = new BrukerTOF(path);

            Assert.IsNotNull(run);
            Assert.AreEqual("2012_05_15_MN9_A_000001", run.DatasetName);
            Assert.AreEqual(path, run.DatasetDirectoryPath);
        }

        [Test]
        public void GetNumMSTest1()
        {
            var path =
                @"\\protoapps\UserData\Slysz\DeconTools_TestFiles\Bruker\Bruker_Maxis\2012_05_15_MN9_A_000001.d";

            var run = new BrukerTOF(path);

            Assert.AreEqual(1131, run.GetNumMSScans());
        }

        [Test]
        public void GetFirstAndLastMassSpectrumTest1()
        {
            var path =
                @"\\protoapps\UserData\Slysz\DeconTools_TestFiles\Bruker\Bruker_Maxis\2012_05_15_MN9_A_000001.d";

            var run = new BrukerTOF(path);

            run.GetMassSpectrum(new ScanSet(1));

            run.GetMassSpectrum(new ScanSet(run.GetNumMSScans()));

            Assert.AreEqual(1, run.GetMinPossibleLCScanNum());
            Assert.AreEqual(1131, run.GetMaxPossibleLCScanNum());
        }

        [Test]
        public void GetMassSpectrumTest1()
        {
            var path =
             @"\\protoapps\UserData\Slysz\DeconTools_TestFiles\Bruker\Bruker_Maxis\2012_05_15_MN9_A_000001.d";

            var run = new BrukerTOF(path);

            var scan = new ScanSet(_testScan);

            var xyData = run.GetMassSpectrum(scan);

            Assert.IsTrue(xyData.Xvalues.Length > 100);

            TestUtilities.DisplayXYValues(xyData);

            run.Close();

            run = null;
            GC.Collect();
        }

        [Test]
        public void GetRetentionTimeTest1()
        {
            var path =
           @"\\protoapps\UserData\Slysz\DeconTools_TestFiles\Bruker\Bruker_Maxis\2012_05_15_MN9_A_000001.d";

            var run = new BrukerTOF(path);

            //for (int i = 478; i < 600; i++)
            //{
            //    Console.WriteLine(i + "\t" + run.GetTime(i));

            //}

            Assert.AreEqual(0.5388m, (decimal)run.GetTime(478));
        }

        [Test]
        public void GetMSLevelTest1()
        {
            var path =
             @"\\protoapps\UserData\Slysz\DeconTools_TestFiles\Bruker\Bruker_Maxis\2012_05_15_MN9_A_000001.d";

            var run = new BrukerTOF(path);

            for (var i = 478; i < 600; i++)
            {
                Console.WriteLine(i + "\t" + run.GetMSLevel(i));
            }
            Assert.AreEqual(1, run.GetMSLevel(_testScan));
        }
    }
}
