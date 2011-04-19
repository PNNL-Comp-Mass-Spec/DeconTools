using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using DeconTools.Backend.Utilities;
using DeconTools.Backend.Core;
using DeconTools.Backend.Runs;

namespace DeconTools.UnitTesting2.ProcessingRelated_Tests.ScanSet_FrameSet_relatedTests
{
    [TestFixture]
    public class ScanSetCollectionCreatorTests
    {
        [Test]
        public void createScanSets_processMSMS_test1()
        {
            RunFactory rf=new RunFactory();
            Run run = rf.CreateRun(FileRefs.RawDataMSFiles.OrbitrapStdFile1);

            bool processMSMS = true;
            ScanSetCollectionCreator sscc = new ScanSetCollectionCreator(run, run.MinScan, run.MaxScan, 1, 1, processMSMS);
            sscc.Create();
        }

    }
}
