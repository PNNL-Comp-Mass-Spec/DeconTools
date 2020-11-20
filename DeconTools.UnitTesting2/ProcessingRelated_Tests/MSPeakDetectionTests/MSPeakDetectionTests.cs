using System;
using System.Text;
using DeconTools.Backend.Core;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend.ProcessingTasks.PeakDetectors;
using DeconTools.Backend.Runs;
using NUnit.Framework;

namespace DeconTools.UnitTesting2.ProcessingRelated_Tests.MSPeakDetectionTests
{
    [TestFixture]
    public class MSPeakDetectionTests
    {
        [Test]
        public void PeakDetectorDemo1()
        {
            //In this list on Scan 6005 we can see what the peak of the parent is. Slide 4 1059.45898 is the Monoisotopic peak of Scan 6009
            var run = new RunFactory().CreateRun(FileRefs.RawDataMSFiles.OrbitrapStdFile1);

            var testScan = new ScanSet(6009);

            run.CurrentScanSet = testScan;

            var generator = MSGeneratorFactory.CreateMSGenerator(run.MSFileType);

            var peakDetector = new DeconToolsPeakDetectorV2();

            generator.Execute(run.ResultCollection);
            peakDetector.Execute(run.ResultCollection);

            var sb = new StringBuilder();
            foreach (var peak in run.PeakList)
            {
                sb.Append(peak.XValue + "\t" + peak.Height + "\t" + peak.Width + "\n");
            }

            Console.WriteLine("----------------------PeakList---------------");
            Console.WriteLine("m/z\tintensity\twidth");
            Console.WriteLine(sb.ToString());
        }

        [Test]
        public void DetectPeaksTest1()
        {
            var peakBR = 1.3;
            double sigNoise = 2;
            var isThresholded = true;
            var peakFitType = Backend.Globals.PeakFitType.QUADRATIC;

            var testFile = FileRefs.RawDataMSFiles.OrbitrapStdFile1;

            Run run = new XCaliburRun2(testFile);

            var generator = MSGeneratorFactory.CreateMSGenerator(run.MSFileType);

            run.CurrentScanSet = new ScanSet(6005);

            generator.Execute(run.ResultCollection);

            var peakDetector = new DeconToolsPeakDetectorV2(peakBR, sigNoise, peakFitType, isThresholded)
            {
                PeaksAreStored = true
            };

            var peakList = peakDetector.FindPeaks(run.XYData, 0, 50000);

            TestUtilities.DisplayPeaks(peakList);
        }

        [Test]
        public void DetectPeaksInOrbitrapData()
        {
            var peakBR = 1.3;
            double sigNoise = 2;
            var isThresholded = true;
            var peakFitType = Backend.Globals.PeakFitType.QUADRATIC;

            var testFile = FileRefs.RawDataMSFiles.OrbitrapStdFile1;

            Run run = new XCaliburRun2(testFile);

            //create list of target ScanSets
            run.ScanSetCollection.Create(run, 6000, 6015, 1, 1);

            //in the 'run' object there is now a list of scans : run.ScanSetCollection
            var generator = MSGeneratorFactory.CreateMSGenerator(run.MSFileType);

            var peakDetector = new DeconToolsPeakDetectorV2(peakBR, sigNoise, peakFitType, isThresholded)
            {
                PeaksAreStored = true
            };

            foreach (var scan in run.ScanSetCollection.ScanSetList)
            {
                //set the target scan:
                run.CurrentScanSet = scan;

                generator.Execute(run.ResultCollection);
                peakDetector.Execute(run.ResultCollection);
            }
        }
    }
}
