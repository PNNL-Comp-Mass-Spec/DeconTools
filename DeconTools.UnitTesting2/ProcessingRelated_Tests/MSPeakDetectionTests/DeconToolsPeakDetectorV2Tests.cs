using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend;
using DeconTools.Backend.Core;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend.ProcessingTasks.PeakDetectors;
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



        public void NewPeakDetectorTest1()
        {

            Run run = new XCaliburRun(FileRefs.RawDataMSFiles.OrbitrapStdFile1);

            ScanSet scan = new ScanSet(6005);




            MSGenerator msgen = MSGeneratorFactory.CreateMSGenerator(run.MSFileType);
            DeconToolsPeakDetectorV2 peakDetector = new DeconToolsPeakDetectorV2();
            peakDetector.PeakToBackgroundRatio = 1.3;
            peakDetector.SignalToNoiseThreshold = 2;
            peakDetector.PeakFitType = Globals.PeakFitType.QUADRATIC;
            peakDetector.IsDataThresholded = true;


            DeconToolsPeakDetector oldPeakDetector = new DeconToolsPeakDetector(1.3, 2, DeconTools.Backend.Globals.PeakFitType.QUADRATIC, true);
            peakDetector.IsDataThresholded = true;



            run.CurrentScanSet = scan;

            msgen.Execute(run.ResultCollection);

            var peaks = peakDetector.FindPeaks(run.XYData.Xvalues,run.XYData.Yvalues, 481.1, 481.4);

            var oldPeaks = oldPeakDetector.FindPeaks(run.XYData, 481.1, 481.4);

            TestUtilities.DisplayPeaks(peaks);

            TestUtilities.DisplayPeaks(oldPeaks);

            //TestUtilities.DisplayXYValues(run.XYData);
        }

        public void NewPeakDetectorTest2()
        {

            Run run = new XCaliburRun(FileRefs.RawDataMSFiles.OrbitrapStdFile1);

            ScanSet scan = new ScanSet(6005);




            MSGenerator msgen = MSGeneratorFactory.CreateMSGenerator(run.MSFileType);
            DeconToolsPeakDetectorV2 peakDetector = new DeconToolsPeakDetectorV2();
            peakDetector.PeakToBackgroundRatio = 1.3;
            peakDetector.SignalToNoiseThreshold = 2;
            peakDetector.PeakFitType = Globals.PeakFitType.QUADRATIC;
            peakDetector.IsDataThresholded = true;


            DeconToolsPeakDetector oldPeakDetector = new DeconToolsPeakDetector(1.3, 2, DeconTools.Backend.Globals.PeakFitType.QUADRATIC, true);
            peakDetector.IsDataThresholded = true;



            run.CurrentScanSet = scan;

            msgen.Execute(run.ResultCollection);



            var peaks = peakDetector.FindPeaks(run.XYData.Xvalues, run.XYData.Yvalues, 400, 1000);

            var oldPeaks = oldPeakDetector.FindPeaks(run.XYData, 400, 1000);

            TestUtilities.DisplayPeaks(peaks);

            TestUtilities.DisplayPeaks(oldPeaks);

            //TestUtilities.DisplayXYValues(run.XYData);
        }

    }
}
