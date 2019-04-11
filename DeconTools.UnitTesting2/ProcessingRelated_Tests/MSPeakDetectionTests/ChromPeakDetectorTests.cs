using System;
using System.Linq;
using DeconTools.Backend.Core;
using DeconTools.Backend.ProcessingTasks.PeakDetectors;
using DeconTools.Backend.Runs;
using NUnit.Framework;

namespace DeconTools.UnitTesting2.ProcessingRelated_Tests.MSPeakDetectionTests
{
    public class ChromPeakDetectorTests
    {

        [Test]
        public void DetectPeaks1()
        {
            //see https://jira.pnnl.gov/jira/browse/OMCS-634
            var testChromatogramDataFile = @"\\protoapps\UserData\Slysz\DeconTools_TestFiles\TargetedWorkflowStandards\massTag635428_chromatogramData.txt";

            var xyData = TestUtilities.LoadXYDataFromFile(testChromatogramDataFile);
            Assert.IsNotNull(xyData);


            var peakDetector = new ChromPeakDetector();
            peakDetector.PeakToBackgroundRatio = 0.5;
            peakDetector.SignalToNoiseThreshold = 0.5;
            peakDetector.IsDataThresholded = true;


            var oldChromPeakDetector = new ChromPeakDetectorOld();
            oldChromPeakDetector.PeakBackgroundRatio = 0.5;
            oldChromPeakDetector.SigNoise = 0.5;

            var peaks = peakDetector.FindPeaks(xyData.Xvalues, xyData.Yvalues);
            var peaksFromOld = oldChromPeakDetector.FindPeaks(xyData, 0, 0);

            Assert.AreEqual(8, peaks.Count);

            TestUtilities.DisplayPeaks(peaks);

            Console.WriteLine("------------ old peak detector----------------");
            TestUtilities.DisplayPeaks(peaksFromOld);
            // run.XYData.Display();

        }

        [Test]
        public void ExecutePeakDetectorTest1()
        {
            var testChromatogramDataFile = @"\\protoapps\UserData\Slysz\DeconTools_TestFiles\TargetedWorkflowStandards\massTag635428_chromatogramData.txt";

            var xyData = TestUtilities.LoadXYDataFromFile(testChromatogramDataFile);
            Assert.IsNotNull(xyData);

            Run run = new ConcreteXYDataRun(xyData.Xvalues, xyData.Yvalues);

            var peakDetector = new ChromPeakDetector();
            peakDetector.PeakToBackgroundRatio = 0.5;
            peakDetector.SignalToNoiseThreshold = 0.5;
            peakDetector.IsDataThresholded = true;

            run.XYData = xyData;
            peakDetector.Execute(run.ResultCollection);

            Assert.IsTrue(run.PeakList.Count > 0);
            Assert.AreEqual(8, run.PeakList.Count);

            var mostAbundantPeak = run.PeakList.OrderByDescending(p => p.Height).First() ;

            Assert.IsTrue(mostAbundantPeak is ChromPeak);
            Assert.AreEqual(2498682m, (decimal)mostAbundantPeak.Height);
            Assert.AreEqual(10065.52418m, (decimal)Math.Round(mostAbundantPeak.XValue,5));
            Assert.AreEqual(92.27364m, (decimal)Math.Round( mostAbundantPeak.Width,5));


        }

    }
}
