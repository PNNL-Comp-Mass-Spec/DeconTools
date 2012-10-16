using System;
using DeconTools.Backend.Utilities;
using NUnit.Framework;

namespace DeconTools.UnitTesting2.Utilities
{
    public class IsotopicProfileElutionExtractorTests
    {

        [Test]
        public void Get3DElutionTest1()
        {
            string peaksFile = FileRefs.PeakDataFiles.OrbitrapPeakFile_scans5500_6500;
            string orbiFile = FileRefs.RawDataMSFiles.OrbitrapStdFile1;

            var run = RunUtilities.CreateAndLoadPeaks(orbiFile, peaksFile);

            Assert.IsNotNull(run);
            Assert.IsTrue(run.ResultCollection.MSPeakResultList.Count > 0);

            var extractor = new IsotopicProfileElutionExtractor();

            int minScan = 5900;
            int maxScan = 6300;
            double minMZ = 749;
            double maxMZ = 754;

            int[] scans;
            float[] intensities;
            double[] mzBinVals;


            extractor.Get3DElutionProfileFromPeakLevelData(run, minScan, maxScan, minMZ, maxMZ, out scans, out mzBinVals, out intensities);

            var intensities2D = extractor.GetIntensitiesAs2DArray();
            Console.WriteLine(extractor.OutputElutionProfileAsString());
        }


    }
}
