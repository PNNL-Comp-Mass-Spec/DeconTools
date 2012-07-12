using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend.Runs;
using NUnit.Framework;

namespace DeconTools.UnitTesting2.ProcessingRelated_Tests.MSPeakDetectionTests
{
    [TestFixture]
    public class DeconToolsPeakDetectorV2Tests
    {
        public void OldPeakDetectorTest1()
        {

            Run run = new XCaliburRun(FileRefs.RawDataMSFiles.OrbitrapStdFile1);

            ScanSet scan = new ScanSet(6005);


            MSGenerator msgen = MSGeneratorFactory.CreateMSGenerator(run.MSFileType);
            DeconToolsPeakDetector peakDetector = new DeconToolsPeakDetector(1.3, 2, DeconTools.Backend.Globals.PeakFitType.QUADRATIC, true);
            peakDetector.IsDataThresholded = true;

            run.CurrentScanSet = scan;

            msgen.Execute(run.ResultCollection);

            var peaks= peakDetector.FindPeaks(run.XYData, 481.1, 481.4);

            TestUtilities.DisplayPeaks(peaks);


        }
    }
}
