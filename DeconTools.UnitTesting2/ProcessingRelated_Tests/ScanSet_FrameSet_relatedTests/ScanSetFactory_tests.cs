using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using DeconTools.Backend.Core;
using DeconTools.Backend.Runs;

namespace DeconTools.UnitTesting2.ProcessingRelated_Tests.ScanSet_FrameSet_relatedTests
{
    [TestFixture]
    public class ScanSetFactory_tests
    {
        [Test]
        public void Test1()
        {
             RunFactory rf = new RunFactory();
            Run run = rf.CreateRun(FileRefs.RawDataMSFiles.OrbitrapStdFile1);


            ScanSetFactory ssf = new ScanSetFactory();
            ScanSet scan=  ssf.CreateScanSet(run, 6005, 11);

            Console.WriteLine(scan);

        }

    }
}
