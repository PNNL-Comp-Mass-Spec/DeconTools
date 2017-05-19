using System.IO;
using DeconTools.Backend.ProcessingTasks.PeakDetectors;
using NUnit.Framework;

namespace DeconTools.UnitTesting2.ProcessingRelated_Tests.ChromatogramRelatedTests
{
    [TestFixture]
    public class ChromatogramPeakDetectorTests
    {
        [Test]
        public void MedianBasedChromPeakDetectorTest1()
        {
            var chromPeakDetector = new ChromPeakDetectorMedianBased();
            chromPeakDetector.PeakToBackgroundRatio = 3;
            chromPeakDetector.SignalToNoiseThreshold = 1;



            var testDataFile =
                @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\ChromatogramAlgorithmTesting\SampleChromatogram_Highpeak_lowpeak.txt";

            Assert.IsTrue(File.Exists(testDataFile), "Test file doesn't exist. File path= " + testDataFile);


            var xydata=  TestUtilities.LoadXYDataFromFile(testDataFile);

            var peakList= chromPeakDetector.FindPeaks(xydata);

            TestUtilities.DisplayPeaks(peakList);


            
        }


        [Test]
        public void MedianBasedChromPeakDetectorTest2()
        {
            var chromPeakDetector = new ChromPeakDetectorMedianBased();
            chromPeakDetector.PeakToBackgroundRatio = 3;
            chromPeakDetector.SignalToNoiseThreshold = 1;



            var testDataFile =
                @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\ChromatogramAlgorithmTesting\SampleChromatogram02_Lots of candidate peaks.txt";

            Assert.IsTrue(File.Exists(testDataFile), "Test file doesn't exist. File path= " + testDataFile);


            var xydata = TestUtilities.LoadXYDataFromFile(testDataFile);

            var peakList = chromPeakDetector.FindPeaks(xydata);

            TestUtilities.DisplayPeaks(peakList);


            
        }


    }
}
