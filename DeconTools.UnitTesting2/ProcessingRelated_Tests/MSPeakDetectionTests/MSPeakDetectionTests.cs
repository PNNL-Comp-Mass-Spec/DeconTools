using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using NUnit.Framework;
using DeconTools.Backend.Core;
using DeconTools.Backend.Runs;
using DeconTools.Backend.Utilities;
using DeconTools.Backend.ProcessingTasks;

namespace DeconTools.UnitTesting2.ProcessingRelated_Tests.MSPeakDetectionTests
{
    [TestFixture]
    public class MSPeakDetectionTests
    {
        [Test]
        public void DetectPeaksInOrbitrapData()
        {
            double peakBR = 1.3;
            double sigNoise = 2;
            bool isThresholded = true;
            DeconTools.Backend.Globals.PeakFitType peakfitType = DeconTools.Backend.Globals.PeakFitType.QUADRATIC;

            string testFile = FileRefs.RawDataMSFiles.OrbitrapStdFile1;

            Run run = new XCaliburRun(testFile);
            ScanSetCollectionCreator sscc = new ScanSetCollectionCreator(run, 6000, 6015, 1, 1);
            sscc.Create();

            MSGeneratorFactory msFactory = new MSGeneratorFactory();



            Task msgen = msFactory.CreateMSGenerator(run.MSFileType);
            Task peakDet = new DeconToolsPeakDetector(peakBR, sigNoise, peakfitType, isThresholded);

            



            foreach (var scan in run.ScanSetCollection.ScanSetList)
            {
                run.CurrentScanSet = scan;

                
                
            }







        }


    }
}
