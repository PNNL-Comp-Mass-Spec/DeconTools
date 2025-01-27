﻿using System;
using DeconTools.Backend.Core;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend.ProcessingTasks.PeakDetectors;
using DeconTools.Backend.Runs;
using NUnit.Framework;

namespace DeconTools.UnitTesting2.ProcessingRelated_Tests.Post_MSFeatureDetectionTests
{
    [TestFixture]
    public class PeakToMSFeatureAssociatorTests
    {
        [Test]
        public void Test1()
        {
            var peakAssociator = new PeakToMSFeatureAssociator();

            Run run = new XCaliburRun2(FileRefs.RawDataMSFiles.OrbitrapStdFile1);

            var scanSet = new ScanSet(6005);
            run.CurrentScanSet = scanSet;

            var generator = MSGeneratorFactory.CreateMSGenerator(run.MSFileType);

            var peakDetector = new DeconToolsPeakDetectorV2(1.3, 2, DeconTools.Backend.Globals.PeakFitType.QUADRATIC, true);

            var decon = new HornDeconvolutor();

            generator.Execute(run.ResultCollection);
            peakDetector.Execute(run.ResultCollection);
            decon.Execute(run.ResultCollection);

            peakAssociator.Execute(run.ResultCollection);

            var numPeaksAssociatedWithFeatures = 0;
            foreach (var msfeature in run.ResultCollection.ResultList)
            {
                foreach (var peak in msfeature.IsotopicProfile.Peaklist)
                {
                    numPeaksAssociatedWithFeatures++;
                }
            }

            var numPeaksInPeakListWithAssociations = 0;
            foreach (MSPeak peak in run.PeakList)
            {
                Console.WriteLine(peak.MSFeatureID + "\t" + peak.XValue + "\t" + peak.Height + "\t");

                if (peak.MSFeatureID != -1)
                {
                    numPeaksInPeakListWithAssociations++;
                }
            }

            //I don't think I can assume this...  msfeature1 might overlap and share a peak with msfeature2
            //Assert.AreEqual(numPeaksAssociatedWithFeatures, numPeaksInPeakListWithAssociations);

            Console.WriteLine("total peaks associated with MSFeatures = " + numPeaksAssociatedWithFeatures);
            Console.WriteLine("total peaks in original peaklist that were associated = " + numPeaksInPeakListWithAssociations);
            Console.WriteLine("fraction peaks assigned = " + (double)(numPeaksInPeakListWithAssociations / (double)run.PeakList.Count));
        }
    }
}
